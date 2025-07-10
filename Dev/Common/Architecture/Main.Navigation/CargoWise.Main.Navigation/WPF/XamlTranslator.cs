using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.Main.Navigation.WPF;

public static class XamlTranslator
{
	[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	public static RoutedEventHandler GetControlLoadedEvent<T>() where T : ContentControl
	{
		return (sender, e) =>
		{
#if DEBUG
			var isDifferentLanguage = true;
#else
			var isDifferentLanguage = Res.CurrentLanguage != Res.DefaultLanguage;
#endif
			var parent = sender as T;
			if (parent != null && typeof(DependencyObject).IsAssignableFrom(parent.Content.GetType()))
			{
				var contentControl = parent.Content as DependencyObject;

				var xamlControlRetriever = new XamlEmbeddedControlRetriever();
				xamlControlRetriever.OnControlProcessing += XamlControlRetriever_OnControlProcessing<T>;
				xamlControlRetriever.Retrieve(contentControl);

				if (isDifferentLanguage)
				{
					Localize<T>(contentControl);
				}
			}
		};
	}

	static void XamlControlRetriever_OnControlProcessing<T>(object sender, RetrievedControlEventArgs e) where T : ContentControl
	{
		var multilingualControl = sender as MultilingualTextLabel;
		if (multilingualControl != null)
		{
			multilingualControl.RegisterMultilingualTextFromXAMLContent(GetLocalizationAsmid<T>(), GetLocalizationKeyPrefix<T>());
		}
	}

	static void Localize<T>(DependencyObject control) where T : ContentControl
	{
		Argument.NotNull(control, nameof(control));

		// Skip SetValue for NonTranslatable because MultilingualTextLabel.RegisterMultilingualTextFromXAMLContent is for IsTranslatable
		// SetValue disable INotifyPropertyChanged because WPF assume non-changing text/label (value is not modified using binding) by calling PropertyChangedEventManager.RemoveListener
		if (!(control is MultilingualTextLabel multilingualTextLabel) || multilingualTextLabel.IsTranslatable)
		{
			foreach (var attribute in XamlMarkupTranslator.Instance.GetTranslatableAttributes())
			{
				LocalizeProperty<T>(control, attribute);
			}

			for (var index = 0; index < VisualTreeHelper.GetChildrenCount(control); ++index)
			{
				var child = VisualTreeHelper.GetChild(control, index);
				Localize<T>(child);
			}
		}
	}

	static void LocalizeProperty<T>(DependencyObject control, string propertyName) where T : ContentControl
	{
		Argument.NotNull(control, nameof(control));
		Argument.NotNull(propertyName, nameof(propertyName));

		var property = control.GetType().GetProperty(propertyName);
		if (property != null && property.CanRead && property.CanWrite)
		{
			string value = property.GetValue(control, Array.Empty<object>()) as string;
			if (!string.IsNullOrEmpty(value))
			{
				property.SetValue(control, Translate<T>(value), Array.Empty<object>());
			}
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	public static string Translate<T>(string pageFragment) where T : ContentControl
	{
		Argument.NotNull(pageFragment, nameof(pageFragment));

		return Res._GetString(GetLocalizationAsmid<T>(), XamlMarkupTranslator.Instance.CalculateKey(GetLocalizationKeyPrefix<T>(), pageFragment), pageFragment);
	}

	static string GetLocalizationKeyPrefix<T>() where T : ContentControl
	{
		string name = typeof(T).Assembly.GetName().Name;
		name = name ?? "";
		if (name.Length > 195)
		{
			throw new InvalidOperationException("AssemblyName cannot be more than 195 characters");
		}

		return "xc|" + name + "|";
	}

	static UInt16 GetLocalizationAsmid<T>() where T : ContentControl
	{
		return unchecked((UInt16)ZrsFile.CalculateAsmid("xc." + typeof(T).Assembly.GetName().Name));
	}

#if DEBUG
	[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	public static Int16 GetLocalizationAsmidForTest<T>() where T : ContentControl
	{
		return Convert.ToInt16(GetLocalizationAsmid<T>());
	}
#endif
}
