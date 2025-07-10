using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class EDITracBanner : BaseUserControl
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LogoImage.ImageUrl = Page.AppInstance.LogoImage;
			LogoImage.NavigateUrl = Page.AppInstance.HomePage;
			LogoImage.ToolTip = Page.AppInstance.CompanyName;
		}

		#region Web Form Designer generated code

		protected System.Web.UI.HtmlControls.HtmlTable BannerTable;
		protected System.Web.UI.HtmlControls.HtmlTableCell BannerCell;

		#endregion
	}
}
