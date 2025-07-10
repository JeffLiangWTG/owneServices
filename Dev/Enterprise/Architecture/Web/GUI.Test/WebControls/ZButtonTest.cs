using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZButtonTest : WebControlTest
	{
		#region Implementation

		protected override Control GetNewControl()
		{
			return new ZButtonForTest();
		}

		ZButtonForTest TestButton
		{
			get { return Control as ZButtonForTest; }
		}

		class ZButtonForTest : ZButton
		{
			#region Test Properties

			public void OnPreRenderForTesting(EventArgs e) => OnPreRender(e);
			public object SaveControlStateForTesting() => SaveControlState();

			#endregion
		}

		#endregion Implementation

		#region Property Tests

		public void TestShowEvent()
		{
			TestButton.ShowEvent = "onblur";
			AssertEquals("ShowEvent", "onblur", TestButton.ShowEvent);
			TestButton.ShowEvent = "onclick";
			AssertEquals("ShowEvent", "onclick", TestButton.ShowEvent);
		}

		public void TestHideEvent()
		{
			TestButton.HideEvent = "onblur";
			AssertEquals("HideEvent", "onblur", TestButton.HideEvent);
			TestButton.HideEvent = "onclick";
			AssertEquals("HideEvent", "onclick", TestButton.HideEvent);
		}

		public void TestHideEventObject()
		{
			TestButton.HideEventObject = "window";
			AssertEquals("HideEventObject", "window", TestButton.HideEventObject);
			TestButton.HideEventObject = "document";
			AssertEquals("HideEventObject", "document", TestButton.HideEventObject);
		}

		#endregion Property Tests

		#region Controls

		public void TestShowHideScript()
		{
			AssertNotNull("ShowHideScript", TestButton.ShowHideScript);
		}

		public void TestPleaseWaitPanel()
		{
			TestButton.Message = "Please wait";
			AssertNotNull("PleaseWaitPanel", TestButton.PleaseWaitPanel);
			AssertEquals("Panel should contain 1 inner control", 1, TestButton.PleaseWaitPanel.Controls.Count);
			AssertEquals("Panel display", "none", TestButton.PleaseWaitPanel.Style["display"].ToLower());
			AssertEquals("Panel background-color", "lightyellow", TestButton.PleaseWaitPanel.Style["background-color"].ToLower());
			AssertEquals("Panel border-style", "solid", TestButton.PleaseWaitPanel.Style["border-style"].ToLower());
			AssertEquals("Panel border-color", "black", TestButton.PleaseWaitPanel.Style["border-color"].ToLower());
			AssertEquals("Panel position", "relative", TestButton.PleaseWaitPanel.Style["position"].ToLower());
			AssertEquals("Panel left", "0px", TestButton.PleaseWaitPanel.Style["left"].ToLower());
			AssertEquals("Panel top", "0px", TestButton.PleaseWaitPanel.Style["top"].ToLower());
			AssertEquals("Panel z-index", "999", TestButton.PleaseWaitPanel.Style["z-index"]);
			HtmlGenericControl messageControl = TestButton.PleaseWaitPanel.Controls[0] as HtmlGenericControl;
			AssertNotNull("Panel MessageText", messageControl);
			AssertEquals("Panel Message", "Please wait", messageControl.InnerHtml);
		}

		#endregion Controls

		#region AddPleaseWaitPanel

		public void TestAddPleaseWaitPanel()
		{
			TestButton.AddPleaseWaitPanel();
			AssertEquals("Please wait panel should not be added when properties not set", 0, TestButton.Controls.Count);

			TestButton.ShowEvent = "onclick";
			TestButton.HideEvent = "onblur";
			TestButton.HideEventObject = "window";
			TestButton.AddPleaseWaitPanel();
			AssertEquals("Control count with panel", 1, TestButton.Controls.Count);
			AssertSame("PleaseWaitPanel", TestButton.PleaseWaitPanel, TestButton.Controls[0]);
		}

		public void TestRender()
		{
			TestButton.ID = "TestButton";
			StringBuilder renderedControl = new StringBuilder();
			HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(renderedControl));
			TestButton.ShowEvent = "onclick";
			TestButton.HideEvent = "onblur";
			TestButton.HideEventObject = "window";
			TestButton.Message = "Please wait while processing occurs.";
			TestButton.OnPreRenderForTesting(EventArgs.Empty);
			TestButton.SaveControlStateForTesting();
			TestButton.RenderControl(writer);
			writer.Close();

			string expectedOutput = Regex.Escape(@"<span><div id=""TestButton_PleaseWaitPanel"" style=""") + "(.+);" + Regex.Escape(@""">" + System.Environment.NewLine +
				@"	<Div>Please wait while processing occurs.</Div>" + System.Environment.NewLine +
									@"</div><input type=""submit"" name=""TestButton"" value="""" onclick=""ShowHide(&#39;TestButton_PleaseWaitPanel&#39;, true);"" id=""TestButton"" /></span>");
			string html = renderedControl.ToString();
			var match = Regex.Match(html, expectedOutput);
			Assert("Expected Regex:\r\n" + expectedOutput + "\r\n\r\nActual Rendered Control:\r\n" + html, match.Success);
			AssertContainsExactElementsInAnyOrder(new[] { "left:0px", "z-index:999", "position:relative", "background-color:lightyellow", "display:none", "border-width:1px", "border-color:black", "top:0px", "border-style:solid" }, match.Groups[1].Value.Split(';'));
		}

		public void TestPreRender()
		{
			TestButton.ID = "TestButton";
			TestButton.ShowEvent = "onclick";
			TestButton.HideEvent = "onblur";
			TestButton.HideEventObject = "window";
			TestButton.Message = "Please wait while processing occurs.";
			ZPage testPage = Page;
			AssertNotNull(testPage);
			AssertEquals("HidePleaseWaitScript should not be registered", false, testPage.ZClientScript.IsClientScriptBlockRegistered("HidePleaseWaitPanel"));
			AssertEquals("ShowHideScript should not be registered", false, testPage.ZClientScript.IsClientScriptBlockRegistered("ShowHideScript"));
			TestButton.OnPreRenderForTesting(EventArgs.Empty);

			AssertEquals("HidePleaseWaitScript should not be registered", false, testPage.ZClientScript.IsClientScriptBlockRegistered("HidePleaseWaitPanel"));
			AssertEquals("ShowHideScript should not be registered", false, testPage.ZClientScript.IsClientScriptBlockRegistered("ShowHideScript"));
		}

		#endregion AddPleaseWaitPanel
	}
}
