using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Popup in a DIV tag that is being used for things like Error providers and similar things
	/// </summary>
	public class ZDivPopup : Panel
	{
		public ZDivPopup(Control ctrl)
		{
			this.Ctrl = ctrl;

			ID = "Popup";
			Style.Add("Z-INDEX", "999");
			Style.Add("DISPLAY", "block");
			Style.Add("POSITION", "absolute");
		}

		protected Control Ctrl;
	}

	#endregion
}
