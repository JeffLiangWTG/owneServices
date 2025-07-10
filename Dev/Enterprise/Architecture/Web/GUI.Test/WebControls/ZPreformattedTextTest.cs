using System.IO;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZPreformattedTextTest : WebControlTest
	{
		protected override Control GetNewControl()
		{
			return new ZPreformattedTextForTest();
		}

		public void TestRender()
		{
			var ctrl = (ZPreformattedTextForTest)this.GetNewControl();

			TestBizO.Z0_Code = "test";
			ctrl.BindTo = "Z0_Code";
			ctrl.Bind(TestBizO);

			var textWriter = new StringWriter();
			var writer = new HtmlTextWriter(textWriter);

			ctrl.RenderForTesting(writer);

			AssertEquals("<pre>test</pre>", textWriter.ToString());
		}

		public void TestRenderEncodesTextToAvoidCrossSiteScripting()
		{
			var ctrl = (ZPreformattedTextForTest)this.GetNewControl();

			TestBizO.Z0_Code = "<tst>";
			ctrl.BindTo = "Z0_Code";
			ctrl.Bind(TestBizO);

			var textWriter = new StringWriter();
			var writer = new HtmlTextWriter(textWriter);

			ctrl.RenderForTesting(writer);
			AssertEquals("<pre>&lt;tst&gt;</pre>", textWriter.ToString());
		}

		public void TestDoNotEncodeHtmlTextWhenDisabled()
		{
			var ctrl = (ZPreformattedTextForTest)this.GetNewControl();

			TestBizO.Z0_Code = "<tst>";
			ctrl.EnableHtmlEncoding = false;
			ctrl.BindTo = "Z0_Code";
			ctrl.Bind(TestBizO);

			var textWriter = new StringWriter();
			var writer = new HtmlTextWriter(textWriter);

			ctrl.RenderForTesting(writer);
			AssertEquals("<pre><tst></pre>", textWriter.ToString());
		}

		class ZPreformattedTextForTest : ZPreformattedText
		{
			#region Test Properties

			public void RenderForTesting(HtmlTextWriter writer) => Render(writer);

			#endregion
		}
	}
}
