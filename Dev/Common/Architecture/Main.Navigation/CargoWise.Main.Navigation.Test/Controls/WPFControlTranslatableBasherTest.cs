
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;

namespace CargoWise.Main.Navigation.WPF.Test
{
	internal static class WPFControlTranslatableBasherTest
	{
		internal delegate Control GetFormToBashDelegate();

		static public string BashTranslatable(TestCaseWithFactory basherTest, GetFormToBashDelegate getFormToBashDelegate)
		{
			ResourceStringData markerResource = new ResourceStringData("", "", "", BasherTestMarkerCaption, BasherTestMarkerCaption);

			using (IMockResourceStringCache cache = Res.UseMockData())
			{
				cache.SetResourceGetter((key) => markerResource);

				var testForm = getFormToBashDelegate();
				if (IsExempt(testForm) || (basherTest != null && IsExempt(basherTest)))
				{
					return null;
				}

				List<string> nonTranslatableComponents = new List<string>();

				CheckControlIsTranslatable(testForm, nonTranslatableComponents);

				if (nonTranslatableComponents.Count > 0)
				{
					return
						string.Format(
							"WPF Control {1} has nontranslatable contents:{0}{0}{2}{0}{0}{0}Please ensure that XAML translator is applied to control{0}For detail see Wiki: <A href='{3}'>{3}</A>{0}",
						/*0*/ System.Environment.NewLine,
						/*1*/ testForm.GetType().FullName,
						/*2*/ string.Join(System.Environment.NewLine, nonTranslatableComponents.ToArray()),
						/*3*/ "https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/ResourceStringEditing.aspx"
							).Replace(System.Environment.NewLine, "<br/>");
				}
			}

			return null;
		}

		static void CheckControlIsTranslatable(DependencyObject control, ICollection<string> nonTranslatableComponents)
		{
			// if control is wrapped in MultilingualTextLabel with IsTranslatable = FALSE, then don't translate
			if (!(control is MultilingualTextLabel multilingualTextLabel) || multilingualTextLabel.IsTranslatable)
			{
				foreach (var attribute in XamlMarkupTranslator.Instance.GetTranslatableAttributes())
				{
					CheckControlPropertyIsTranslatable(control, attribute, nonTranslatableComponents);
				}

				for (var index = 0; index < VisualTreeHelper.GetChildrenCount(control); ++index)
				{
					DependencyObject child = VisualTreeHelper.GetChild(control, index);
					if (child != null)
					{
						CheckControlIsTranslatable(child, nonTranslatableComponents);
					}
				}
			}
		}

		static void CheckControlPropertyIsTranslatable(DependencyObject control, string propertyName, ICollection<string> nonTranslatableComponents)
		{
			var property = control.GetType().GetProperty(propertyName);
			if (property != null && property.CanRead)
			{
				string value = property.GetValue(control, Array.Empty<object>()) as string;
				if (!string.IsNullOrEmpty(value) && !value.Contains(BasherTestMarkerCaption))
				{
					nonTranslatableComponents.Add(FormatComponentResolution(control.GetType().FullName, propertyName, value, "null", control));
				}
			}
		}

		public static bool IsExempt(Object controlOrTest)
		{
			return IsExempt(controlOrTest.GetType());
		}

		static readonly string[] exemptFullTypeSubstrings = { "Enterprise.Customs.", "Enterprise.Client.", "ZClient", "Enterprise.Messaging.", "OxyPlot.Wpf" };

		public static bool IsExempt(Type type)
		{
			return exemptFullTypeSubstrings.Any(type.FullName.Contains);
		}

		#region Implementation

		internal const string BasherTestMarkerCaption = "@#$_Basher_Test";

		static string FormatComponentResolution(string componentType, string name, string text, string resolution, DependencyObject locationControl)
		{
			return string.Format(
				"{1}:\t\t{2}{0}Text:\t\t{3}{0}",
				/*0*/ System.Environment.NewLine,
				/*1*/ componentType,
				/*2*/ name,
				/*3*/ text.Replace("<", "&lt;").Replace(">", "&gt;")
			);
		}

		#endregion
	}
}
