using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebThemeControl))]
	sealed class WebThemeControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly;
		}

		public void TestNormalUsageIsOkay()
		{
			var url1 = "BLU.webtracker.com/";
			var url2 = "RED.webtracker.com/";
			WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { url1, url2 });

			using (var form = new ZForm())
			using (var control = new WebThemeControl(WebDataRegistry.Instance.WebTrackerTheme))
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				control.Value = Array.Empty<WebTrackerTheme>();
				AssertEquals("Should have 3 URLs.", 3, control.UrlComboBox.Items.Count);
				AssertEquals("All URLs", (string)control.UrlComboBox.Items[0]);
				AssertEquals(url1, (string)control.UrlComboBox.Items[1]);
				AssertEquals(url2, (string)control.UrlComboBox.Items[2]);

				control.UrlComboBox.SelectedIndex = 0;
				control.ThemeDropEdit.Text = ThemeCodeDescriptionPairList.Codes.ALT;
				control.UrlComboBox.SelectedIndex = 1;
				control.ThemeDropEdit.Text = ThemeCodeDescriptionPairList.Codes.CLS;

				var finalValue = control.Value;
				AssertEquals(2, finalValue.Length);
				AssertNullOrEmpty(finalValue[0].Url);
				AssertEquals(url1, finalValue[1].Url);
				AssertEquals(ThemeCodeDescriptionPairList.Codes.ALT, finalValue[0].Code);
				AssertEquals(ThemeCodeDescriptionPairList.Codes.CLS, finalValue[1].Code);
			}
		}

		public void TestFormHasChanges_UrlComboBox()
		{
			const string url = "BLU.webtracker.com/";
			WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { url });

			using (var testForm = new RegistryFormTest.DummyRegistryForm())
			using (var control = new WebThemeControl(WebDataRegistry.Instance.WebTrackerTheme))
			{
				testForm.Show();

				control.Value = Array.Empty<WebTrackerTheme>();
				testForm.Controls.Add(control);
				Assert(!testForm.ImportantMethodWasCalled);

				control.UrlComboBoxIndexChanged();
				Assert(testForm.ImportantMethodWasCalled);
			}
		}

		public void TestFormHasChanges_ThemeDropEdit()
		{
			const string url = "BLU.webtracker.com/";
			WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { url });

			using (var testForm = new RegistryFormTest.DummyRegistryForm())
			using (var control = new WebThemeControl(WebDataRegistry.Instance.WebTrackerTheme))
			{
				testForm.Show();

				control.Value = Array.Empty<WebTrackerTheme>();
				testForm.Controls.Add(control);
				Assert(!testForm.ImportantMethodWasCalled);

				control.ThemeDropEditIndexChanged();
				Assert(testForm.ImportantMethodWasCalled);
			}
		}
	}
}
