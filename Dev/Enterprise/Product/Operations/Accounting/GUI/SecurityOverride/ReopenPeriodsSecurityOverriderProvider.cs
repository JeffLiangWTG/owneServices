using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public class ReopenPeriodsSecurityOverriderProvider : InteractiveSecurityOverrideProvider
	{
		static string AccessDeniedError => Res.GetString("30e55021-0db0-43e3-8ded-e39ad2edb357", "Access Denied: Reopen Period");

		static string DeniedSecurityRightsError => Res.GetString("6B9BC74C-451C-4B70-9216-51C0A5E12E56", @"You do not have the appropriate security right to run this function.
If you require access to this function, ask your system administration to change your Staff or Group Security Rights to allow access to:
Manage > General Ledger > Period Management > Reopen Period");

		public ReopenPeriodsSecurityOverriderProvider(List<GlbStaff> staffs)
		{
			this.staffs = staffs;
		}

		protected void ShowDeniedSecurityRightsErrorMsg()
		{
			Globals.Message.ShowError(DeniedSecurityRightsError, AccessDeniedError);
		}

		protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
		{
			SecurityCore securityCore = null;
			using (var loginForm = CreateNewLoginForm())
			{
				loginForm.Message = BuildLoginMessage(checkPoint);

				do
				{
					LastLoginFormResult = ZFormModaliser.ShowDialogWithoutDispose(loginForm);
					if (LastLoginFormResult != DialogResult.OK)
					{
						break;
					}

					if (loginForm.Credentials == null || loginForm.Credentials.UserSecurity == null)
					{
						if (loginForm.Credentials != null
							&& loginForm.Credentials.LoginAuthentication.State == LoginAuthenticationInfo.Status.UserInactive)
						{
							Globals.Message.ShowError(loginForm.Credentials.LoginAuthentication.FailureMessage);
						}
						else
						{
							Globals.Message.ShowError(InvalidLoginErrorMsg, AccessDeniedError);
						}

						return securityCore;
					}

					if (!staffs.Any(user => user.GS_LoginName == loginForm.Credentials.Login))
					{
						ShowDeniedSecurityRightsErrorMsg();
					}
					else
					{
						securityCore = loginForm.Credentials?.UserSecurity;
					}
				}
				while (!Globals.IsTest && securityCore == null);
			}

			return securityCore;
		}

		ZString BuildLoginMessage(SecurityCheckpoint checkpoint)
		{
			var authorizedNames = string.Join(", ", staffs.Select(u => u.GS_FullName));
			return Res.GetString("b54ca8d1-0a4d-4897-9dd1-92ea10cb55f0",
				@"You do not have the required security right to reopen period.
To override this security, a user with security right to [{0}] must login. Please enter username and password details below.
The following users have the required security right. CargoWise has authorized the following people to reopen periods:
{1}",
				checkpoint.DisplayText, authorizedNames);
		}

		readonly List<GlbStaff> staffs;
	}
}
