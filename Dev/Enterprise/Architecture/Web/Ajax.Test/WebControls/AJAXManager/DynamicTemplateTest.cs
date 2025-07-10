using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class DynamicTemplateTest : TestCase
	{
		public void TestRenderChildControls()
		{
			string expectedHTML = "<b>Something</b>";

			DynamicTemplate template = new DynamicTemplate();
			LiteralControl childControl = new LiteralControl(expectedHTML);
			template.Controls.Add(childControl);

			Repeater repeater = new Repeater();
			repeater.HeaderTemplate = template;
			repeater.DataSource = System.Array.Empty<string>();
			repeater.DataBind();

			StringBuilder sb = new StringBuilder();
			repeater.RenderControl(new HtmlTextWriter(new StringWriter(sb)));

			AssertEquals(expectedHTML, sb.ToString());
		}
	}
}
