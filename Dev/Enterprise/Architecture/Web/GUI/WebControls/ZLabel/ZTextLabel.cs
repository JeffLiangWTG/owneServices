using System.ComponentModel;
using System.Web.UI;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Label on the web form
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZTextLabel runat=server></{0}:ZTextLabel>")]
	public class ZTextLabel : ZLabelBase
	{
		public ZTextLabel()
		{
		}

		public ZTextLabel(string text)
		{
			Text = text;
		}

		#region Get GetText

		protected override string GetToolTip(IZType value)
		{
			// We don't want to show HTML code in a tooltip. 
			if (EnableHtmlEncoding)
			{
				return this.GetHtmlEncodableLabelContent(base.GetToolTip(value));
			}
			else
			{
				return string.Empty;
			}
		}

		#endregion
	}
}
