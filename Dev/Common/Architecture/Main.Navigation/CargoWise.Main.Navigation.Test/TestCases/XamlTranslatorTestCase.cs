using System;
using System.Windows;
using System.Windows.Controls;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.WPF.Test
{
	class XamlTranslatorTestCase : TestCase
	{
		[RequiresSTA]
		public void TestTranslateWpfControl()
		{
			using (Res.TemporarilySwitchLanguage("RU-RU"))
			using (var mockRs = Res.UseMockData())
			{
				var results = XamlMarkupTranslator.Instance.Parse("xc|CargoWise.Main.Navigation.Test|", @"
<Button Name=""cancelButton"" Content=""Cancel"" HorizontalAlignment=""Left"" Margin=""90,254,0,0"" VerticalAlignment=""Top"" Width=""75"" RenderTransformOrigin=""-0.011,0.293""/>
<TextBox Name=""cancelBox"" HorizontalAlignment=""Left"" Height=""23"" Margin=""10,10,0,0"" TextWrapping=""Wrap"" Text=""OK"" VerticalAlignment=""Top"" Width=""155"" RenderTransformOrigin=""-0.047,0.535""/>
<Button Name=""okButton"" Content=""OK"" HorizontalAlignment=""Left"" Margin=""10,254,0,0"" VerticalAlignment=""Top"" Width=""75"" RenderTransformOrigin=""-0.061,0.626""/>
<TextBox Name=""okBox"" HorizontalAlignment=""Left"" Height=""23"" Margin=""10,38,0,0"" TextWrapping=""Wrap"" Text=""{Binding Content, ElementName=okButton}"" VerticalAlignment=""Top"" Width=""155""/>
");
				AssertEquals(3, results.Count);
				AssertEquals("Cancel", results[0].TranslatableText);
				AssertEquals("OK", results[1].TranslatableText);
				AssertEquals("OK", results[2].TranslatableText);
				mockRs.Put(results[0].Key, new ResourceStringData(results[0].Key, "Отмена"));
				mockRs.Put(results[1].Key, new ResourceStringData(results[1].Key, "Принять"));

				var window = new Window();
				var control = new UserControlForTesting();
				window.Content = control;
				control.okButton.Content = "Cancel";

				window.Show();

				AssertEquals("Отмена", control.cancelButton.Content);
				AssertEquals("Принять", control.cancelBox.Text);
				AssertEquals("Отмена", control.okButton.Content);
				AssertEquals("Отмена", control.okBox.Text);

				window.Close();
			}
		}

		[RequiresSTA]
		public void TestIsTranslatable()
		{
			using (Res.TemporarilySwitchLanguage("RU-RU"))
			using (var mockRs = Res.UseMockData())
			{
				var key = XamlMarkupTranslator.Instance.CalculateKey("xc|CargoWise.Main.Navigation.Test|", "Test XAML string literal");
				mockRs.Put(key, new ResourceStringData(key, "Тестовый строковый литерал XAML"));

				var control = new UserControlForTesting();
				var window = new Window();
				window.Content = control;
				window.Show();

				AssertEquals("Translatable", "Тестовый строковый литерал XAML", ((TextBlock)control.multilingualXamlString.Content).Text);
				AssertEquals("NonTranslatable", "Test XAML string literal", ((TextBlock)control.nonTranslatableMultilingualXamlString.Content).Text);

				window.Close();
			}
		}

		[RequiresSTA]
		public void TestMultilingualTextAttibuteLoadSuccessfully()
		{
			var window = new Window();
			var control = new UserControlForTesting();
			window.Content = control;

			window.Show();

			AssertEquals("Test XAML string literal", control.multilingualXamlString.MultilingualText.ToString(Res.DefaultLanguage));
			AssertEquals("Test binding attribute", control.multilingualBindingAttribute.MultilingualText.ToString(Res.DefaultLanguage));

			window.Close();
		}

		public void TestAsmidIsCalculatedCorrectly()
		{
			var expectAsmid = unchecked(Convert.ToInt16((UInt16)ZrsFile.CalculateAsmid("xc." + typeof(UserControlForTesting).Assembly.GetName().Name)));
			AssertEquals(expectAsmid, XamlTranslator.GetLocalizationAsmidForTest<UserControlForTesting>());
		}
	}

	class XamlBindingObjectForTest
	{
		public MultilingualString MultilingualText
		{ get { return (NoResString)"Test binding attribute"; } }

		public string Name
		{ get { return "TestName"; } }

		public bool IsTranslatable
		{ get { return true; } }
	}
}
