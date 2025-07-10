using System.Web.UI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class LoginStatusTest : TestCase
	{
		public void TestShowLogOffLinkButton()
		{
			Assert("Default should be Visible", LoginStatus.LogOffLinkButton.Visible);
			LoginStatus.ShowLogOffLinkButton = false;
			Assert("Should be false now", !LoginStatus.LogOffLinkButton.Visible);
		}

		public void TestShowChangePasswordLinkButton()
		{
			Assert("Default should be Visible", LoginStatus.ChangePasswordLinkButton.Visible);
			LoginStatus.ShowChangePasswordLinkButton = false;
			Assert("Should be false now", !LoginStatus.ChangePasswordLinkButton.Visible);
		}

		LoginStatus LoginStatus
		{
			get
			{
				if (fLoginStatus == null)
				{
					fLoginStatus = new LoginStatus();
					fLoginStatus.LogOffLinkButton = new LinkButton();
					fLoginStatus.ChangePasswordLinkButton = new LinkButton();
				}
				return fLoginStatus;
			}
		}

		LoginStatus fLoginStatus;
	}
}
