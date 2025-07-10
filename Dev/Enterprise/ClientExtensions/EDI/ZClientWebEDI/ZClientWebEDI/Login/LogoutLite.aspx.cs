using System;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class LogoutLite : BasePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			AppInstance.SignOut(false);
			MyAccountLoginLiteHelper.ExpireLiteViewModeCookies(Request, Response);
			Response.Redirect(AppInstance.HostingSiteLoginPage);
		}
	}
}
