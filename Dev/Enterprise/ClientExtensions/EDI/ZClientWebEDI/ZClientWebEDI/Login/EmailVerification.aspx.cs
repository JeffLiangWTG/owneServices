using System;
using CargoWise.Definitions.Authentication;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class EmailVerification : RoutingEnabledPage
	{
		protected override bool PageRequiresLogin(Uri url) => false;
		string OriginalRequestUrl { get; set; }
		EdiCustomerUserAccount UserAccount { get; set; }
		protected override string StyleSheetFileName => this.AppInstance.LoginPageStyleSheetPath;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		protected void Page_Load(object sender, EventArgs e)
		{
			var tokenQueryString = Request.QueryString["token"];
			if (tokenQueryString == null)
			{
				Response.Redirect(EDIDataRegistry.Instance.MyAccountIndexPage.Value);
			}
		}

		protected void VerifyEmail(object sender, EventArgs e)
		{
			var tokenQueryString = Request.QueryString["token"];
			if (VerifyEmailToken(tokenQueryString) && UserAccount?.WebAccessContact != null)
			{
				RedirectViaLoginRouter(OriginalRequestUrl, UserAccount);
			}
			else
			{
				Response.Redirect(EDIDataRegistry.Instance.MyAccountIndexPage.Value);
			}
		}

		bool VerifyEmailToken(string accessToken)
		{
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var result = accessControl.TryConsume(accessToken, AccessTokenTypes.VerifyEmailToken, out var token);

			if (result)
			{
				SetupSessionIfRequired();
				return VerifyEmailTokenCore(token);
			}

			result = accessControl.TryConsume(accessToken, AccessTokenTypes.DistinctEmailToken, out token);

			if (result)
			{
				SetupSessionIfRequired();
				return VerifyDistinctEmailToken(token);
			}

			return false;
		}

		void SetupSessionIfRequired()
		{
			if (Env.CurrentUser == null)
			{
				AppInstance.SetupSession(null, EventArgs.Empty, false);
			}
		}

		protected virtual bool VerifyDistinctEmailToken(AccessTokenInfo token)
		{
			var scope = new ZString(token.Scope);
			var userAccountPK = token.ParentId;
			UserAccount = Factory.Load<EdiCustomerUserAccount>(userAccountPK);
			if (UserAccount == null)
			{
				return false;
			}

			var splits = scope.Split(new[] { "::" });
			if (splits.Length != 2)
			{
				return false;
			}

			var newEmail = splits[0];
			OriginalRequestUrl = splits[1];

			if (UserAccount?.WebAccessContact != null)
			{
				if (ContactImporter.OrgHasContactDuplicate(UserAccount.WebAccessContact.OC_OH, newEmail, UserAccount.WebAccessContact, UserAccount.Factory))
				{
					return false;
				}

				UserAccount.EUA_ContactRelationshipStatus = ZString.Empty;
				UserAccount.WebAccessContact.OC_Email = newEmail;
				UserAccount.WebAccessContact.OC_WebAccessEnabled = true;
				UserAccount.Factory.Save();

				return true;
			}
			else
			{
				var orgPk = UserAccount.Database?.LD_OH_WebAccessOrg ?? ZGuid.Empty;
				if (orgPk.IsValid)
				{
					if (ContactImporter.OrgHasContactDuplicate(orgPk, newEmail, null, UserAccount.Factory))
					{
						return false;
					}

					var importer = new WebRequestContactImporter(UserAccount.Factory, UserAccount.Database);

					UserAccount.EUA_ContactRelationshipStatus = ZString.Empty;
					var contact = importer.ImportFromUserAccount(UserAccount).Item2;
					if (contact != null)
					{
						contact.OC_Email = newEmail;
					}

					importer.SaveIfNeeded();

					return true;
				}
			}

			return false;
		}

		protected virtual bool VerifyEmailTokenCore(AccessTokenInfo token)
		{
			var result = false;
			var scope = new ZString(token.Scope);
			var userAccountPK = token.ParentId;
			UserAccount = Factory.Load<EdiCustomerUserAccount>(userAccountPK);
			if (UserAccount == null)
			{
				return false;
			}

			if (scope.StartsWith(LoginOptionsHelperForDistinctEmail.MergeAccountsVerificationKey))
			{
				var splits = scope.Split("::");
				if (splits.Length != 3)
				{
					return false;
				}

				if (!ZGuid.TryParse(splits[1], out var contactPK) || UserAccount.EUA_OC_WebAccessContact.Equals(contactPK))
				{
					return false;
				}

				var contact = Factory.Load<OrgContact>(contactPK);

				if (contact == null)
				{
					return false;
				}

				OriginalRequestUrl = splits[2];
				CustomerUserAccountRelationshipStatusResolver.MergeToContact(UserAccount, contact);
				result = true;
			}
			else if (scope.StartsWith(UserEmailVerificationRoutingDescriptor.VerifyTestUserAccountTokenKey, StringComparison.Ordinal))
			{
				OriginalRequestUrl = scope.SubstringSafe(scope.IndexOf(':') + 1);
				result = UserAccount.ActivateUserAccountAndSave();
			}
			else
			{
				OriginalRequestUrl = scope;
				if (UserAccount?.WebAccessContact != null)
				{
					UserAccount.ActivateContactRelationshipAndSave();
					result = true;
				}
			}

			return result;
		}
	}
}
