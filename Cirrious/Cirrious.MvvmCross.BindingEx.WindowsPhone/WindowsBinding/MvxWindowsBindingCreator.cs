﻿// MvxWindowsBindingCreator.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using System.Collections.Generic;
#if WINDOWS_PHONE
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
#endif
using Cirrious.CrossCore;
using Cirrious.CrossCore.Converters;
#if WINDOWS_PHONE
using Cirrious.CrossCore.WindowsPhone.Converters;
#endif
#if NETFX_CORE
using Cirrious.CrossCore.WindowsStore.Converters;
#endif
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Binders;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
#endif

// ReSharper disable CheckNamespace
namespace Cirrious.MvvmCross.BindingEx.WindowsShared.WindowsBinding
// ReSharper restore CheckNamespace
{
    public class MvxWindowsBindingCreator : MvxBindingCreator
    {
        protected virtual void ApplyBinding(MvxBindingDescription bindingDescription, Type actualType,
                                            FrameworkElement attachedObject)
        {
            DependencyProperty dependencyProperty = actualType.FindDependencyProperty(bindingDescription.TargetName);
            if (dependencyProperty == null)
            {
                Mvx.Warning("Dependency property not found for {0}", bindingDescription.TargetName);
                return;
            }

            var property = actualType.FindActualProperty(bindingDescription.TargetName);
            if (property == null)
            {
                Mvx.Warning("Property not returned {0} - may cause issues", bindingDescription.TargetName);
            }

#if WINDOWS_PHONE
            var newBinding = new System.Windows.Data.Binding
#endif
#if NETFX_CORE
            var newBinding = new Windows.UI.Xaml.Data.Binding
#endif
            {
                    Path = new PropertyPath(bindingDescription.SourcePropertyPath),
                    Mode = ConvertMode(bindingDescription.Mode, property == null ? typeof(object) : property.PropertyType),
                    Converter = GetConverter(bindingDescription.Converter),
                    ConverterParameter = bindingDescription.ConverterParameter,
#if WINDOWS_PHONE
                    FallbackValue = bindingDescription.FallbackValue
#endif
            };

            BindingOperations.SetBinding(attachedObject, dependencyProperty, newBinding);
        }

        protected override void ApplyBindings(FrameworkElement attachedObject,
                                              IEnumerable<MvxBindingDescription> bindingDescriptions)
        {
            var actualType = attachedObject.GetType();
            foreach (var bindingDescription in bindingDescriptions)
            {
                ApplyBinding(bindingDescription, actualType, attachedObject);
            }
        }

        protected static IValueConverter GetConverter(IMvxValueConverter converter)
        {
            if (converter == null)
                return null;

            // TODO - consider caching this wrapper - it is a tiny bit wasteful creating a wrapper for each binding
            return new MvxNativeValueConverter(converter);
        }

        protected static BindingMode ConvertMode(MvxBindingMode mode, Type propertyType)
        {
            switch (mode)
            {
                case MvxBindingMode.Default:
                    // if we return TwoWay for ImageSource then we end up in 
                    // problems with WP7 not doing the auto-conversion
                    // see some of my angst in http://stackoverflow.com/questions/16752242/how-does-xaml-create-the-string-to-bitmapimage-value-conversion-when-binding-to/16753488#16753488
                    // Note: if we discover other issues here, then we should make a more flexible solution
                    if (propertyType == typeof(ImageSource))
                        return BindingMode.OneWay;

                    return BindingMode.TwoWay;

                case MvxBindingMode.TwoWay:
                    return BindingMode.TwoWay;

                case MvxBindingMode.OneWay:
                    return BindingMode.OneWay;

                case MvxBindingMode.OneTime:
                    return BindingMode.OneTime;

                case MvxBindingMode.OneWayToSource:
                    Mvx.Warning("WinPhone doesn't support OneWayToSource");
                    return BindingMode.TwoWay;

                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }
    }
}