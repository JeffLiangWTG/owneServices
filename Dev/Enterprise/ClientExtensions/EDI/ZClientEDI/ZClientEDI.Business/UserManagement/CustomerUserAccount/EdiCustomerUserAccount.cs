using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiCustomerUserAccount : AutoEdiCustomerUserAccount
	{
		public EdiCustomerUserAccount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Database")]
		public override ZGuid EUA_LD
		{
			get => base.EUA_LD;
			set => base.EUA_LD = value;
		}

		public LicenceDatabase Database => Factory.Load<LicenceDatabase>(EUA_LD);
		public LicenceEnterprise DatabaseLicenceEnterprise => Database?.LicEnterprise;

		[RelatedBusinessObject("EDIWebAccessContact")]
		public override ZGuid EUA_OC_WebAccessContact
		{
			get => base.EUA_OC_WebAccessContact;
			set => base.EUA_OC_WebAccessContact = value;
		}

		public EDIOrgContact EDIWebAccessContact => Factory.Load<EDIOrgContact>(EUA_OC_WebAccessContact);

		public OrgHeader ContactOrganisation => EDIWebAccessContact?.ParentOrg;

		public GlbPerson ContactPerson => EDIWebAccessContact?.Person;

		public string AccountVerificationStatus
		{
			get
			{
				var result = Res.GetString("4d41de83-30a4-4d33-84f2-04452eb6ad6a", "Verified");
				if (!EUA_IsActive)
				{
					result = Res.GetString("6dd17736-a1dd-48ee-975b-021bc17db912", "User ID Deactivated");
				}
				else if (EUA_IsEmailVerificationRequired)
				{
					result = Res.GetString("1b7b3c48-68c9-457e-bddf-8f1deb0192e3", "Pending Email Verification");
				}
				else if (EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.AccountReactivated)
				{
					result = Res.GetString("d8b6fe5e-028d-4e62-940c-ae9408e6bb98", "Verification Required (User ID Reactivated)");
				}
				else if (EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked)
				{
					result = Res.GetString("d0762b5c-07c8-42d3-9586-c6d1fe36da70", "Verification Required (New System Pairing)");
				}
				else if (EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.EmailChanged)
				{
					result = Res.GetString("addac258-2382-4dec-9ae7-88174543403f", "Verification Required (User ID Reassigned)");
				}
				else if (EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.DistinctEmailRequired)
				{
					result = Res.GetString("063f13d4-b495-4fe0-86c0-9d886f6f0d02", "Verification Required (Distinct Email Required)");
				}
				else if (string.IsNullOrEmpty(EUA_ContactRelationshipStatus) && !EUA_IsContactRelationshipActive)
				{
					result = Res.GetString("50f6b434-88c6-4793-93c3-ce4beab26716", "Account Requires Verification");
				}
				return result;
			}
		}

		public bool IsAwaitingActivation
		{
			get => !EUA_IsContactRelationshipActive
				&& (EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.AccountReactivated
				|| EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.ProductDeactivation
				|| EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked
				|| (EUA_ContactRelationshipStatus.IsEmpty && EUA_IsActive && (WebAccessContact?.OC_IsActive ?? false)));
		}

		public override ZBool EUA_IsActive
		{
			get => base.EUA_IsActive;
			set
			{
				if (value && Database != null && !Database.LD_IsActive)
				{
					return; // Can't reactivate if Licence Database is inactive
				}

				base.EUA_IsActive = value;
			}
		}

		#endregion

		#region Save

		public override void OnSaving()
		{
			DeactivateContactAndRelatedTestUserAccountsIfRequired();
			base.OnSaving();
			ReportAbnormalStatus();
		}

		void DeactivateContactAndRelatedTestUserAccountsIfRequired()
		{
			if (EUA_IsActiveInfo.HasChanges && !EUA_IsActive)
			{
				if (WebAccessContact != null)
				{
					var otherUserAccountsQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, EUA_OC_WebAccessContact);
					otherUserAccountsQuery.AddToFilter(EdiCustomerUserAccountSchema.PK, SQLComparisonOperator.NotEqual, PK);
					var otherUserAccounts = Factory.Load<EdiCustomerUserAccount>(otherUserAccountsQuery);

					if (!otherUserAccounts.Any() || otherUserAccounts.All(x => !x.EUA_IsActive || !x.EUA_IsContactRelationshipActive))
					{
						WebAccessContact.OC_IsActive = false;
						return;
					}

					if (Database.LD_LicenceType != DatabaseTypes.Codes.Production || otherUserAccounts.Any(x => x.EUA_IsActive && x.EUA_IsContactRelationshipActive && x.Database.LD_Product == Database.LD_Product && x.Database.LD_LicenceType == DatabaseTypes.Codes.Production))
					{
						return;
					}

					var shouldDeactivateContact = true;

					foreach (var userAccount in otherUserAccounts)
					{
						if (userAccount.Database.LD_Product == Database.LD_Product)
						{
							userAccount.EUA_IsContactRelationshipActive = false;
							userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.ProductDeactivation;
						}
						else if (userAccount.Database.LD_Product != ProductTypes.Codes.WiseTechAcademy)
						{
							shouldDeactivateContact = false;
						}
					}

					if (shouldDeactivateContact)
					{
						WebAccessContact.OC_IsActive = false;
					}
				}
			}
		}

		void ReportAbnormalStatus()
		{
			if (!EUA_OC_WebAccessContact.IsEmpty && (EUA_IsActiveInfo.HasChanges || EUA_IsContactRelationshipActiveInfo.HasChanges || EUA_ContactRelationshipStatusInfo.HasChanges))
			{
				var contact = WebAccessContact;
				bool hasAbnormalStatus = contact != null && contact.OC_IsActive && EUA_IsActive && !EUA_IsContactRelationshipActive && EUA_ContactRelationshipStatus.IsEmpty;
				if (hasAbnormalStatus)
				{
					var messageBuilder = new ZStringBuilder();
					messageBuilder.AppendLine(FormattableString.Invariant($"EUA_IsActive:{EUA_IsActiveInfo.OriginalValue}=>{EUA_IsActive}"));
					messageBuilder.AppendLine(FormattableString.Invariant($"EUA_IsContactRelationshipActive:{EUA_IsContactRelationshipActiveInfo.OriginalValue}=>{EUA_IsContactRelationshipActive}"));
					messageBuilder.AppendLine(FormattableString.Invariant($"EUA_ContactRelationshipStatus:{EUA_ContactRelationshipStatusInfo.OriginalValue}=>{EUA_ContactRelationshipStatus}"));
					messageBuilder.AppendLine(FormattableString.Invariant($"OC_IsActive:{contact.OC_IsActiveInfo.OriginalValue}=>{contact.OC_IsActive}"));
					ErrorReporter.ReportOnce(messageBuilder.ToString());
				}
			}
		}

		#endregion

		public bool HasAcknowledgedUserAgreement(EdiUserAgreement agreement)
		{
			var agreementAcceptanceLogQuery = new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_ERA, agreement.PK);
			var orQuery = new ZQuery();
			orQuery.AddToFilter(JoinCondition.Or, EdiUserAgreementAcceptanceLogSchema.EUL_EUA, PK);

			if (WebAccessContact != null)
			{
				var orgQuery = new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_OH, WebAccessContact.OC_OH);
				orgQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_EUA, null);
				orQuery.AddToFilter(orgQuery, JoinCondition.Or);
			}

			if (WebAccessContact?.Header is EDIOrgHeader ediOrg && ediOrg.LicEnterprise != null)
			{
				var entQuery = new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_LE, ediOrg.LicEnterprise.PK);
				entQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_EUA, null);
				orQuery.AddToFilter(entQuery, JoinCondition.Or);
			}

			agreementAcceptanceLogQuery.AddToFilter(orQuery);
			return Factory.Exists(typeof(EdiUserAgreementAcceptanceLog), agreementAcceptanceLogQuery);
		}

		public static EdiCustomerUserAccount GetUserAccountForWebAccess(BusinessObjectFactory factory, LicenceDatabase database, string userId)
		{
			var query = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, database.PK);
			query.AddToFilter(EdiCustomerUserAccountSchema.EUA_UserID, userId);
			var userAccount = factory.LoadTop1<EdiCustomerUserAccount>(query);
			return userAccount;
		}

		public void ActivateContactRelationshipAndSave()
		{
			EUA_IsActive = true;
			EUA_IsContactRelationshipActive = true;
			EUA_ContactRelationshipStatus = "";
			EUA_UserVerifiedDateUtc = ZDateTime.UtcNow;
			var importer = new WebRequestContactImporter(Factory, Database);
			importer.ImportFromUserAccount(this);
			importer.SaveIfNeeded();
			Factory.Save();
		}

		public bool ActivateUserAccountAndSave()
		{
			var db = Database;
			if (db == null)
			{
				return false;
			}

			EUA_IsEmailVerificationRequired = false;
			Factory.Save();

			var importer = new WebRequestContactImporter(Factory, db);
			var contact = importer.ImportFromUserAccount(this).Item2;
			if (contact == null)
			{
				return false;
			}

			var contactInDb = contact.IsInDatabase;
			importer.SaveIfNeeded();

			if (!contactInDb)
			{
				VerifyContactRelationship();
			}
			else
			{
				var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contact.PK);
				userAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.PK, SQLComparisonOperator.NotEqual, PK);
				var otherUserAccounts = Factory.Load<EdiCustomerUserAccount>(userAccountQuery);

				if (otherUserAccounts.Length > 0)
				{
					if (otherUserAccounts.All(x => !x.EUA_IsContactRelationshipActive) && !contact.HasPassword)
					{
						VerifyContactRelationship();

						if (!contact.OC_IsActive)
						{
							contact.OC_IsActive = true;
						}
					}
				}
				else
				{
					VerifyContactRelationship();
				}
			}
			Factory.Save();

			return true;

			void VerifyContactRelationship()
			{
				EUA_UserVerifiedDateUtc = ZDateTime.UtcNow;

				if (EUA_ContactRelationshipStatus != ContactRelationshipStatusList.Codes.DistinctEmailRequired)
				{
					EUA_IsContactRelationshipActive = true;
					EUA_ContactRelationshipStatus = string.Empty;
				}
			}
		}

		public string GetSystemText()
		{
			var db = Database;
			var entCode = db.EnterpriseCode;
			var productName = new ProductTypes(true).GetDescriptionFromCode(db.LD_Product);
			var refText = db.IsEnterpriseFamilyDatabase ?
				FormattableString.Invariant($"{entCode} {db.LD_ServerCode}") :
				FormattableString.Invariant($"{entCode} {db.LD_TenantID}");
			var systemText = FormattableString.Invariant($"{productName} ({refText.Trim()})");
			return systemText;
		}

		public static EdiCustomerUserAccount CreateNewUserAccount(BusinessObjectFactory factory, LicenceDatabase licenceDatabase, string userId, string fullName, string email, string country)
		{
			var newUserAccount = factory.New<EdiCustomerUserAccount>();
			newUserAccount.EUA_LD = licenceDatabase.PK;
			newUserAccount.EUA_UserID = userId;
			newUserAccount.EUA_FullName = fullName;
			if (!string.IsNullOrEmpty(country) && country.Length == 2)
			{
				newUserAccount.EUA_RN_NKCountry = country;
			}

			newUserAccount.EUA_IsEmailVerificationRequired = true;

			if (newUserAccount.EUA_RN_NKCountryInfo.HasNotifications())
			{
				newUserAccount.EUA_RN_NKCountry = ZString.Empty;
			}

			if (email != null)
			{
				newUserAccount.EUA_Email = email;
			}

			return newUserAccount;
		}
	}
}
