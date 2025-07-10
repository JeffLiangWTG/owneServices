using System.Web.Security.AntiXss;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTextLabelTest : ZLabelBaseTest
	{
		#region TestConstructorWithText

		public void TestConstructorWithText()
		{
			AssertEquals("Pink Floyd", new ZTextLabel("Pink Floyd").Text);
		}

		#endregion

		#region TestBindToTextWithLineBreak

		public void TestBindToTextWithLineBreak()
		{
			ZLabelTestO.EnableHtmlEncoding = true;
			ZLabelTestO.BindTo = "Z0_Description";
			TestBizO.Z0_Description = "First Line" + System.Environment.NewLine + "Second Line<div />";

			ZLabelTestO.Bind(TestBizO);

			AssertEquals("Should have replaced new line with line break", "First Line<br />Second Line&lt;div /&gt;", ZLabelTestO.Text);
		}

		public void TestBindToTextWithLineBreak_EnableEncoding()
		{
			ZLabelTestO.EnableHtmlEncoding = false;
			ZLabelTestO.BindTo = "Z0_Description";
			TestBizO.Z0_Description = "First Line<br />" + System.Environment.NewLine + "Second Line<div />";

			ZLabelTestO.Bind(TestBizO);

			AssertEquals("Should not add line breaks", TestBizO.Z0_Description, ZLabelTestO.Text);
		}

		#endregion

		#region TestNoTooltipWhenHtmlEncodingIsDisabled

		public void TestNoTooltipWhenHtmlEncodingIsDisabled()
		{
			ZLabelTestO.BindTo = "Z0_Description";
			TestBizO.Z0_Description = "<b>Description</b>";
			ZLabelTestO.Bind(TestBizO);
			Assert("HtmlEncoding should be enabled by default", ZLabelTestO.EnableHtmlEncoding);
			var a = AntiXssEncoder.HtmlEncode("<b>Description</b>", false);
			AssertEquals(AntiXssEncoder.HtmlEncode("<b>Description</b>", false), ZLabelTestO.Text);
			AssertEquals(AntiXssEncoder.HtmlEncode("<b>Description</b>", false), ZLabelTestO.ToolTip);

			ZLabelTestO.EnableHtmlEncoding = false;
			ZLabelTestO.Bind(TestBizO);

			AssertEquals("<b>Description</b>", ZLabelTestO.Text);
			AssertEquals(string.Empty, ZLabelTestO.ToolTip);
		}

		#endregion

		#region Implementation

		protected override ZLabelBase GetNewLabel()
		{
			return new ZTextLabel();
		}

		#endregion
	}
}
