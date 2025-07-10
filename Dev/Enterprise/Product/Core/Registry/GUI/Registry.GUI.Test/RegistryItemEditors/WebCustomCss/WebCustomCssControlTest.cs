using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class WebCustomCssControlTest : TransactionedTestCase
	{
		public void TestExportToFile()
		{
			var urlWithAllTheBadChars = "some url" + string.Join(string.Empty, Path.GetInvalidFileNameChars());
			using (var form = new ZChildForm())
			using (var control = new WebCustomCssControlWithSettableExport() { Dock = DockStyle.Fill })
			{
				control.SetDataBinding(new WebCustomsCssCollectionWrapper(System.Array.Empty<string>(), new[] { new WebTrackerCustomCss(urlWithAllTheBadChars, "blah") }), string.Empty);

				form.Controls.Add(control);
				form.Show();

				control.UrlComboBox.SelectedIndex = 1;

				var testDir = Temp.GetNewTempSubdirectory();
				try
				{
					control.FolderForExport = testDir;
					control.ExportButton.PerformClick();

					var expectedPath = Path.Combine(testDir, "WebCustomCss_some_url.css");
					Assert("The file should be created, and all illegal chars stripped. Available files: \r\n" + string.Join("\r\n", Directory.EnumerateFiles(testDir)), File.Exists(expectedPath));
					AssertEquals("Should contain our custom css", "blah", File.ReadAllText(expectedPath));
				}
				finally
				{
					Directory.Delete(testDir, recursive: true);
				}
			}
		}

		public void TestExportCancelled()
		{
			var urlWithAllTheBadChars = "some url" + string.Join(string.Empty, Path.GetInvalidFileNameChars());
			using (var form = new ZChildForm())
			using (var control = new WebCustomCssControlWithSettableExport() { Dock = DockStyle.Fill })
			{
				control.SetDataBinding(new WebCustomsCssCollectionWrapper(System.Array.Empty<string>(), new[] { new WebTrackerCustomCss(urlWithAllTheBadChars, "blah") }), string.Empty);

				form.Controls.Add(control);
				form.Show();

				control.UrlComboBox.SelectedIndex = 1;

				control.FolderForExport = null;
				AssertNoExceptionThrown(control.ExportButton.PerformClick);
			}
		}

		public void TestHasChangesIsPropogatedFromTextbox()
		{
			using (var form = new ZChildForm())
			using (var control = new WebCustomCssControl() { Dock = DockStyle.Fill })
			{
				control.SetDataBinding(new WebCustomsCssCollectionWrapper(System.Array.Empty<string>(), System.Array.Empty<WebTrackerCustomCss>()), string.Empty);

				form.Controls.Add(control);
				form.Show();

				control.CurrentDataItem.HasChanges = false;

				control.CssTextBox.Text += "Some more text";

				Assert("HasChanges should be set when we change the css text", control.CurrentDataItem.HasChanges);
			}
		}

		public void TestNormalUsageIsOkay()
		{
			var url1 = "BLU.webtracker.com/";
			var url2 = "RED.webtracker.com/";
			var css1 = ".css1 { a: b; }";
			var css2 = ".css2 { x: y; }";

			using (var form = new ZChildForm())
			using (var control = new WebCustomCssControl { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(new WebCustomsCssCollectionWrapper(new string[] { url1, url2 }, System.Array.Empty<WebTrackerCustomCss>()), string.Empty);
				AssertEquals("Should have 3 URLs.", 3, control.UrlComboBox.Items.Count);
				AssertEquals("All URLs", ((WebCustomCssBusinessObject)control.UrlComboBox.Items[0]).DisplayValue);
				AssertEquals(url1, ((WebCustomCssBusinessObject)control.UrlComboBox.Items[1]).DisplayValue);
				AssertEquals(url2, ((WebCustomCssBusinessObject)control.UrlComboBox.Items[2]).DisplayValue);

				control.UrlComboBox.SelectedIndex = 0;
				SetBoundMemberText(control.CssTextBox, css1);

				control.UrlComboBox.SelectedIndex = 1;
				SetBoundMemberText(control.CssTextBox, css2);

				var finalValue = control.CurrentDataItem.ToWebTrackerCustomCssArray();
				AssertEquals(2, finalValue.Length);
				AssertEquals("", finalValue[0].Url);
				AssertEquals(url1, finalValue[1].Url);
				AssertEquals(css1, finalValue[0].Data);
				AssertEquals(css2, finalValue[1].Data);
			}
		}

		void SetBoundMemberText(ZTextBox textbox, string value)
		{
			var boundObject = typeof(ZTextBox).GetProperty("DataSource", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(textbox);
			((WebCustomCssBusinessObject)boundObject).Data = value;
		}

		public void TestDataForDeletedUrlsRemains()
		{
			var url1 = "BLU.webtracker.com/";
			var url2 = "RED.webtracker.com/";
			var css1 = ".css1 { a: b; }";
			var css2 = ".css2 { x: y; }";

			using (var form = new ZChildForm())
			using (var control = new WebCustomCssControl { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(new WebCustomsCssCollectionWrapper(new[] { url1 }, new[] { new WebTrackerCustomCss(url1, css1), new WebTrackerCustomCss(url2, css2) }), string.Empty);
				AssertEquals("Should have 3 URLs.", 3, control.UrlComboBox.Items.Count);
				AssertEquals("We want the combo box to show the DisplayValue so that an empty url becomes 'All URLs'", control.UrlComboBox.DisplayMember, "DisplayValue");
				AssertEquals("All URLs should always be the first value", "All URLs", ((WebCustomCssBusinessObject)control.UrlComboBox.Items[0]).DisplayValue);
				AssertEquals(url1, ((WebCustomCssBusinessObject)control.UrlComboBox.Items[1]).DisplayValue);
				AssertEquals(url2, ((WebCustomCssBusinessObject)control.UrlComboBox.Items[2]).DisplayValue);

				control.UrlComboBox.SelectedIndex = 2;
				AssertEquals("Should have data for the non-existant URL.", css2, control.CssTextBox.Text);
			}
		}

		public void TestImportStatementIsAddedToNewCustomCss()
		{
			using (var form = new ZChildForm())
			using (var control = new WebCustomCssControl { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(new WebCustomsCssCollectionWrapper(System.Array.Empty<string>(), System.Array.Empty<WebTrackerCustomCss>()), string.Empty);

				AssertEquals("Should have automatically added the import statement.", WebCustomCssControl.CssImportStatement, control.CssTextBox.Text);
			}
		}
	}
}
