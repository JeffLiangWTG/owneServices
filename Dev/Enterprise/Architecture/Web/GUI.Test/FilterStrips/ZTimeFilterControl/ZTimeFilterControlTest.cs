using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	sealed class ZTimeFilterControlTest : WebControlTest
	{
		[TestDate(2022, 1, 6)]
		public void TestText()
		{
			FilterControl.SelectedValueForTesting = null;
			AssertEquals(":", FilterControl.Text);

			FilterControl.SelectedValueForTesting = ZDateTime.Empty;
			AssertEquals(":", FilterControl.Text);

			FilterControl.SelectedValueForTesting = new ZDateTime(2022, 1, 1, 1, 2, 0);
			AssertEquals("1:02", FilterControl.Text);

			FilterControl.SelectedValueForTesting = new ZDateTime(2022, 1, 1, 10, 0, 0);
			AssertEquals("10:00", FilterControl.Text);

			FilterControl.SelectedValueForTesting = new ZDateTime(2022, 1, 4, 9, 58, 0);
			AssertEquals("81:58", FilterControl.Text);

			FilterControl.SelectedValueForTesting = new ZDateTime(2022, 2, 11, 15, 59, 0);
			AssertEquals("999:59", FilterControl.Text);
		}

		[TestDate(2022, 1, 6)]
		public void TestSelectedValue()
		{
			FilterControl.Text = null;
			AssertEquals(ZDateTime.Empty, FilterControl.SelectedValueForTesting);

			FilterControl.Text = string.Empty;
			AssertEquals(ZDateTime.Empty, FilterControl.SelectedValueForTesting);

			FilterControl.Text = ":";
			AssertEquals(ZDateTime.Empty, FilterControl.SelectedValueForTesting);

			FilterControl.Text = "00:00";
			AssertEquals(ZDateTime.Empty, FilterControl.SelectedValueForTesting);

			FilterControl.Text = "1:02";
			AssertEquals(new ZDateTime(2022, 1, 1, 1, 2, 0), FilterControl.SelectedValueForTesting);

			FilterControl.Text = "10:00";
			AssertEquals(new ZDateTime(2022, 1, 1, 10, 0, 0), FilterControl.SelectedValueForTesting);

			FilterControl.Text = "81:58";
			AssertEquals(new ZDateTime(2022, 1, 4, 9, 58, 0), FilterControl.SelectedValueForTesting);

			FilterControl.Text = "999:59";
			AssertEquals(new ZDateTime(2022, 2, 11, 15, 59, 0), FilterControl.SelectedValueForTesting);
		}

		public void TestStyle()
		{
			FilterControl.OnPreRenderForTesting();
			AssertEquals($"{HorizontalAlign.Center}", FilterControl.Style["text-align"]);
		}

		public void TestValidationMessage()
		{
			AssertEquals("Please enter hours and minutes, max 999:59", FilterControl.ValidationMessageForTesting);
		}

		public void TestValidationPattern()
		{
			AssertEquals("^\\\\d{1,3}:[0-5][0-9]$", FilterControl.ValidationPatternForTesting);
		}

		public void TestGetNewValidationScriptFile()
		{
			var validationFile = FilterControl.GetNewValidationScriptFileForTesting();
			AssertEquals($"/Runtime/Enterprise_ZArchitecture_Web_GUI/{RuntimeVersion}/ZTextBoxBase/ZTimeFilterControl/ZTimeFilterControlValidation.js", validationFile.FileName);
		}

		public void TestValidationScriptKey()
		{
			AssertEquals("ZTimeFilterControl_ValidationScript", FilterControl.ValidationScriptKeyForTesting);
		}

		public void TestValidationFunctionName()
		{
			AssertEquals("ValidateUserTimeInput", FilterControl.ValidationFunctionNameForTesting);
		}

		public void TestControlFunctionsScript()
		{
			var page = (ZPage)FilterControl.Page;
			AssertEquals(false, page.ZClientScript.IsClientScriptBlockRegistered(FilterControl.GetType(), FilterControl.ControlFunctionsScriptKeyForTesting));

			FilterControl.OnPreRenderForTesting();
			AssertEquals(true, page.ZClientScript.IsClientScriptBlockRegistered(FilterControl.GetType(), FilterControl.ControlFunctionsScriptKeyForTesting));

			var expectedScript = @"
				<script>

				function OnFocusUserTimeInput(control) {
					var value = control.value.trim();
					if (value === ':') {
						control.value = '';
					}
				}

				</script>";

			using (var textWriter = new StringWriter())
			using (var htmlWriter = new HtmlTextWriter(textWriter))
			{
				page.RenderControl(htmlWriter);
				var html = textWriter.ToString();
				AssertContains(expectedScript, html, true);
			}
		}

		ZTimeFilterControlForTest FilterControl => (ZTimeFilterControlForTest)Control;

		protected override Control GetNewControl() => new ZTimeFilterControlForTest();

		class ZTimeFilterControlForTest : ZTimeFilterControl
		{
			public IZType SelectedValueForTesting
			{
				get => SelectedValue;
				set => SelectedValue = value;
			}

			public void OnPreRenderForTesting() => OnPreRender(EventArgs.Empty);

			public string ValidationMessageForTesting => ValidationMessage;

			public string ValidationPatternForTesting => ValidationPattern;

			public ZWebResource GetNewValidationScriptFileForTesting() => GetNewValidationScriptFile();

			public string ValidationScriptKeyForTesting => ValidationScriptKey;

			public string ValidationFunctionNameForTesting => ValidationFunctionName;

			public string ControlFunctionsScriptKeyForTesting => ControlFunctionsScriptKey;
		}
	}
}
