using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	[ToolboxData("<{0}:ZRoundedPanel runat=server></{0}:ZRoundedPanel>")]
	public class ZRoundedPanel : Panel
	{
		protected override void OnPreRender(EventArgs e)
		{
			this.CssClass = "boundingbox";
			base.OnPreRender(e);
		}
		#region Overrides

		protected override void Render(HtmlTextWriter writer)
		{
			base.Render(writer);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "clear");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.WriteLine("&nbsp;");
			writer.RenderEndTag();
		}

		protected override void RenderChildren(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "bl");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "br");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "tl");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "tr");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			base.RenderChildren(writer);

			writer.RenderEndTag();
			writer.RenderEndTag();
			writer.RenderEndTag();
			writer.RenderEndTag();
		}
		#endregion
	}

	#endregion
}
