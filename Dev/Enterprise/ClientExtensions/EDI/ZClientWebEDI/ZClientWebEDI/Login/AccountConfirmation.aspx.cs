using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class AccountConfirmation : RoutingEnabledPage
	{
		protected override bool PageRequiresLogin(Uri url) => false;
		protected override bool ShowLoginStatus => false;

		#region Loading

		protected override void OnInitComplete(EventArgs e)
		{
			base.OnInitComplete(e);

			if (!IdentityManager.IsValidID())
			{
				HideLabelsAndShowError(Res.GetString("ec11c0f7-967e-416f-ad7f-3326de9bee79", "The link which directed you to this page was invalid. Try logging in again and contact your system administrator if this issue persists."));
				return;
			}

			var unlinkedUserAccount = GetUnlinkedUserAccount();
			if (unlinkedUserAccount != null)
			{
				Refresh(unlinkedUserAccount);
			}
			else
			{
				RedirectViaLoginRouter(LoginRouter.GetOriginalUrlFromRequest(Request), IdentityManager.Token);
			}
		}

		void HideLabelsAndShowError(string message)
		{
			AccountConfirmationBox.Visible = false;
			ErrorMessage.Text = message;
		}

		#endregion

		#region Unlinked User Account

		EdiCustomerUserAccount GetUnlinkedUserAccount()
		{
			var contact = IdentityManager.Contact;
			return contact != null ? ((EDIOrgContact)contact).GetMostRecentUnlinkedUserAccount() : null;
		}

		protected void ConfirmationYes_Click(object sender, EventArgs e)
		{
			ConfirmCore(true);
		}

		protected void ConfirmationNo_Click(object sender, EventArgs e)
		{
			ConfirmCore(false);
		}

		void ConfirmCore(bool shouldLink)
		{
			var unlinkedUserAccountPK = GetUnlinkedUserAccount()?.PK ?? ZGuid.Empty;
			if (!unlinkedUserAccountPK.IsEmpty)
			{
				var unlinkedUserAccount = new BusinessObjectFactory().Load<EdiCustomerUserAccount>(unlinkedUserAccountPK);
				if (unlinkedUserAccount != null)
				{
					if (shouldLink)
					{
						unlinkedUserAccount.ActivateContactRelationshipAndSave();
					}
					else
					{
						unlinkedUserAccount.EUA_OC_WebAccessContact = Guid.Empty;
						unlinkedUserAccount.Factory.Save();
					}
				}
			}

			RedirectViaLoginRouter(LoginRouter.GetOriginalUrlFromRequest(Request), IdentityManager.Token);
		}

		#endregion

		public void Refresh(EdiCustomerUserAccount userAccount)
		{
			if (userAccount == null || userAccount.EUA_IsContactRelationshipActive)
			{
				Visible = false;
				return;
			}

			ActionMessage1_ACR.Visible = userAccount.EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.AccountReactivated;
			ActionMessage2_ACR.Visible = ActionMessage1_ACR.Visible;

			ActionMessage1_EMC.Visible = userAccount.EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.EmailChanged;
			ActionMessage2_EMC.Visible = ActionMessage1_EMC.Visible;

			ActionMessage1_MUL.Visible = userAccount.EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked;
			ActionMessage2_MUL.Visible = ActionMessage1_MUL.Visible;

			SystemLabel.Text = userAccount.GetSystemText();
			UserLabel.Text = FormattableString.Invariant($"{userAccount.EUA_UserID} , {userAccount.EUA_FullName}");
			EmailLabel.Text = userAccount.EUA_Email;
		}
	}
}
