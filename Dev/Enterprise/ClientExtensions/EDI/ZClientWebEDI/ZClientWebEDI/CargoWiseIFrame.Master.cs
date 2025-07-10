using System;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class CargoWiseIFrame : System.Web.UI.MasterPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			SetupAccreditationLink();
			AdjustControlsForLiteViewMode();

			if (Page is BasePage page)
			{
				A12.Visible = page.SiteUser.LoggedInOrgContact != null && page.SiteUser.LoggedInOrgContact.Person != null;
			}
		}

		void SetupAccreditationLink()
		{
			BasePage page = Page as BasePage;
			AccreditationAnchor.HRef = (page != null) ? AccreditationLinkCreator.GetUrl(page.SiteUser) : "";
		}

		void AdjustControlsForLiteViewMode()
		{
			BasePage page = Page as BasePage;
			if (page != null && page.IsInLiteViewMode)
			{
				LeftNavBar.Visible = false;
			}
		}

		protected void LogoutButton_Click(object sender, EventArgs e)
		{
			BasePage contentPage = Content.Page as BasePage;
			if (contentPage != null)
			{
				contentPage.Logout();
			}
		}
	}
}
