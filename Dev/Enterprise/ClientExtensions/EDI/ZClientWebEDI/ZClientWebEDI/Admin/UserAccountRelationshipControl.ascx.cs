using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Security;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class UserAccountRelationshipControl : BaseUserControl
	{
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		void InitializeComponent()
		{
			DataSourceAssemblyName = "ZClientWebEDI";
			DataSourceTypeName = "Enterprise.ZClientWebCargoWiseEDI.UserAccountDeactivationManager";
		}

		protected UserAccountDeactivationManager Manager => Page.DataSource as UserAccountDeactivationManager;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ContactUserAccountGroupingRepeater.Items.Cast<RepeaterItem>().ForEach(item =>
			{
				var organisationCodeLabel = (ZTextLabel)item.FindControl("Organisation");
				if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem || organisationCodeLabel == null)
				{
					return;
				}

				if (!string.IsNullOrEmpty(organisationCodeLabel.Text))
				{
					var checkBoxLabel = (ZCheckBox)item.FindControl("IsContactRelationshipActive");
					checkBoxLabel.Visible = false;
					return;
				}

				var licenceTypeLabel = (ZTextLabel)item.FindControl("LicenceType");
				if (licenceTypeLabel != null && !string.IsNullOrEmpty(licenceTypeLabel.Text))
				{
					var success = int.TryParse(((ZNumericLabel)item.FindControl("ReferenceNumber")).Text, out var referenceNumber);
					if (success)
					{
						var checkBox = (ZCheckBox)item.FindControl("IsContactRelationshipActive");
						checkBox.Checked = true;
						var matchedUserAccountWrapper = Manager.ContactUserAccountWrappers.FirstOrDefault(x =>
						{
							var userAccountWrapper = x as UserAccountDeactivationWrapper;
							return userAccountWrapper != null && referenceNumber == userAccountWrapper.ReferenceNumber;
						}) as UserAccountDeactivationWrapper;

						checkBox.Enabled = checkBox.Checked = matchedUserAccountWrapper?.UserAccount?.EUA_IsContactRelationshipActive ?? false;
					}
				}
			});
		}

		public virtual void SaveChangesButton_Click(object sender, EventArgs e)
		{
			if (SiteUser == null || !SiteUser.IsLoggedIn)
			{
				RedirectToErrorPage();
			}

			var userAccountsToDeactivate = GetUserAccountsToDeactivate();

			if (userAccountsToDeactivate.Any())
			{
				var loginContactGrouping = Manager.DisplayedContacts.FirstOrDefault(x => x.Contact.PK == SiteUser.LoggedInUserPK);

				if (loginContactGrouping != null
					&& loginContactGrouping.UserAccounts.All(x => !x.EUA_IsContactRelationshipActive || userAccountsToDeactivate.ContainsKey(x.PK))
					&& userAccountsToDeactivate.Any(x => x.Value.EUA_OC_WebAccessContact == loginContactGrouping.Contact.PK))
				{
					ConfirmationDiv.CssClass = CiModalOn;
					return;
				}

				DeactivateUserAccountsAndContactIfRequired();
			}
		}

		Dictionary<ZGuid, EdiCustomerUserAccount> GetUserAccountsToDeactivate()
		{
			var userAccountsToDeactivate = new Dictionary<ZGuid, EdiCustomerUserAccount>();

			ContactUserAccountGroupingRepeater.Items.Cast<RepeaterItem>().ForEach(item =>
			{
				var licenceTypeLabel = (ZTextLabel)item.FindControl("LicenceType");
				if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem || licenceTypeLabel == null || string.IsNullOrEmpty(licenceTypeLabel.Text))
				{
					return;
				}

				var success = int.TryParse(((ZNumericLabel)item.FindControl("ReferenceNumber")).Text, out var referenceNumber);
				if (success)
				{
					var checkBox = (ZCheckBox)item.FindControl("IsContactRelationshipActive");

					if (checkBox.Checked)
					{
						return;
					}

					var matchedUserAccountWrapper = Manager.ContactUserAccountWrappers.FirstOrDefault(x =>
					{
						var userAccountWrapper = x as UserAccountDeactivationWrapper;
						return userAccountWrapper != null && referenceNumber == userAccountWrapper.ReferenceNumber;
					}) as UserAccountDeactivationWrapper;

					if (matchedUserAccountWrapper != null && matchedUserAccountWrapper.UserAccount.EUA_IsContactRelationshipActive)
					{
						userAccountsToDeactivate.Add(matchedUserAccountWrapper.UserAccount.PK, matchedUserAccountWrapper.UserAccount);
					}
				}
			});

			return userAccountsToDeactivate;
		}

		void DeactivateUserAccountsAndContactIfRequired()
		{
			var userAccountsToDeactivate = GetUserAccountsToDeactivate();
			userAccountsToDeactivate.ForEach(x =>
			{
				x.Value.EUA_IsContactRelationshipActive = false;
				x.Value.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.SelfDeactivation;
			});

			foreach (var contactGrouping in Manager.DisplayedContacts)
			{
				if (contactGrouping.UserAccounts.All(x => !x.EUA_IsContactRelationshipActive) && userAccountsToDeactivate.Any(x => x.Value.EUA_OC_WebAccessContact == contactGrouping.Contact.PK))
				{
					contactGrouping.Contact.OC_IsActive = false;
				}
			}

			Manager.Factory.Save();
		}

		protected void DeactivateLoginContactConfirmationYes_Click(object sender, EventArgs e)
		{
			if (SiteUser == null || !SiteUser.IsLoggedIn)
			{
				RedirectToErrorPage();
				return;
			}

			DeactivateUserAccountsAndContactIfRequired();
			ConfirmationDiv.CssClass = CiModalOff;

			FormsAuthentication.SignOut();
			SiteUser.Logout();
			if (Page is BasePage basePage && basePage.IsInLiteViewMode)
			{
				MyAccountLoginLiteHelper.ExpireLiteViewModeCookies(basePage.Request, basePage.Response);
			}

			Page.Response.Redirect(Page.AppInstance.LoginPage);
		}

		protected void DeactivateLoginContactConfirmationNo_Click(object sender, EventArgs e)
		{
			ConfirmationDiv.CssClass = CiModalOff;
		}

		const string CiModalOff = "CiModalOff";
		const string CiModalOn = "CiModalOn";

		void RedirectToErrorPage()
		{
			var queryString = new SecureQueryString
			{
				["title"] = "Session Expired",
				["message"] = "Your login session has expired. Please attempt to login again.",
				ExpireTime = TimeSpan.FromMinutes(10)
			};

			Page.Response.Redirect(Page.AppInstance.ErrorPage + "?data=" + WebUtility.UrlEncode(queryString.ToString()));
		}

		public virtual bool HasChanges
		{
			get
			{
				foreach (var item in ContactUserAccountGroupingRepeater.Items.Cast<RepeaterItem>())
				{
					var licenceTypeLabel = (ZTextLabel)item.FindControl("LicenceType");
					if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem || licenceTypeLabel == null || string.IsNullOrEmpty(licenceTypeLabel.Text))
					{
						continue;
					}

					var success = int.TryParse(((ZNumericLabel)item.FindControl("ReferenceNumber")).Text, out var referenceNumber);
					if (success)
					{
						var checkBox = (ZCheckBox)item.FindControl("IsContactRelationshipActive");
						var matchedUserAccountWrapper = Manager.ContactUserAccountWrappers.FirstOrDefault(x =>
						{
							var userAccountWrapper = x as UserAccountDeactivationWrapper;
							return userAccountWrapper != null && referenceNumber == userAccountWrapper.ReferenceNumber;
						}) as UserAccountDeactivationWrapper;

						if (matchedUserAccountWrapper != null && matchedUserAccountWrapper.UserAccount.EUA_IsContactRelationshipActive && !checkBox.Checked)
						{
							return true;
						}
					}
				}

				return false;
			}
		}
	}
}
