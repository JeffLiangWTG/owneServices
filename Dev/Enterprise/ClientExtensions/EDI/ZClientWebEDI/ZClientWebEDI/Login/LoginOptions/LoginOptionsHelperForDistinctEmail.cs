using System;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class LoginOptionsHelperForDistinctEmail : LoginOptionsHelper
	{
		public LoginOptionsHelperForDistinctEmail(BusinessObjectFactory factory, OrgContact contact, EdiCustomerUserAccount userAccount) : base(factory)
		{
			SetContactAndUserAccountValuesForDistinctEmail(contact?.PK ?? ZGuid.Empty, userAccount?.PK ?? ZGuid.Empty);
		}

		public bool RegisterNewEmail(ZString newEmail, Uri originalUrl)
		{
			if (UserAccountForDistinctEmail == null)
			{
				return false;
			}

			if (ContactForDistinctEmail == null)
			{
				var orgPk = UserAccountForDistinctEmail.Database?.LD_OH_WebAccessOrg ?? ZGuid.Empty;
				if (orgPk.IsValid)
				{
					if (ContactImporter.OrgHasContactDuplicate(orgPk, newEmail, null, UserAccountForDistinctEmail.Factory))
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			if (ContactForDistinctEmail != null && ContactImporter.OrgHasContactDuplicate(ContactForDistinctEmail.OC_OH, newEmail, null, ContactForDistinctEmail.Factory))
			{
				return false;
			}

			UserAccountForDistinctEmail.EUA_IsEmailOverridden = true;
			UserAccountForDistinctEmail.Factory.Save();

			return SendDistinctEmailVerification(originalUrl, newEmail);
		}

		bool SendDistinctEmailVerification(Uri originalUrl, ZString newEmail)
		{
			var info = new AccessTokenInfo(FormattableString.Invariant($"{newEmail}::{originalUrl}"), UserAccountForDistinctEmail.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var token = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.DistinctEmailToken, info, TimeSpan.FromDays(1), 1);

			return SendEmailVerification(token, newEmail, ContactForDistinctEmail, UserAccountForDistinctEmail);
		}

		void SetContactAndUserAccountValuesForDistinctEmail(ZGuid contactPK, ZGuid userAccountPK)
		{
			ContactForDistinctEmail = Factory.Load<OrgContact>(contactPK);
			UserAccountForDistinctEmail = Factory.Load<EdiCustomerUserAccount>(userAccountPK);

			if (ContactForDistinctEmail?.Person != null && UserAccountForDistinctEmail != null)
			{
				EmailAddress = ContactForDistinctEmail.Person.PER_EmailAddress;
				MaskedEmailAddress = MaskEmailAddress(EmailAddress);

				HasPassword = !ContactForDistinctEmail.Person.PER_PasswordHash.IsEmpty || !ContactForDistinctEmail.OC_PasswordHash.IsEmpty;
			}
		}

		protected override void PostPasswordVerificationAction() { }

		protected override string GetScopeForEmailVerification(string originalUrlString)
		{
			return FormattableString.Invariant($"{MergeAccountsVerificationKey}::{Contact.PK}::{originalUrlString}");
		}

		public const string MergeAccountsVerificationKey = "MergeAccountsVerification";

		public OrgContact ContactForDistinctEmail { get; private set; }
		public EdiCustomerUserAccount UserAccountForDistinctEmail { get; private set; }

		public override bool IsValid => base.IsValid || ContactForDistinctEmail != null && UserAccountForDistinctEmail != null;
	}
}
