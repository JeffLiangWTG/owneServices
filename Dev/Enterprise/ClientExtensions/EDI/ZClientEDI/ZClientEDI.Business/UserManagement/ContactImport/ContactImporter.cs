using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Tools.TextStandardizer.JobCategorizer;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Core;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public abstract class ContactImporter
	{
		protected ContactImporter(LicenceDatabase database, OrgHeader webAccessOrgOverride)
		{
			this.importTimeUtc = ZDateTime.UtcNow;
			this.database = database;
			this.org = webAccessOrgOverride ?? database?.WebAccessOrg;

			if (database != null)
			{
				var registrationDbNumber = ObjectFactory.Get<IProductRegistration>().Key.DatabaseNumber;
				IsRegistrationDatabase = (database.LD_DatabaseNumber == registrationDbNumber);
			}
		}

		protected ContactImporter(LicenceDatabase database)
			: this(database, null)
		{
		}

		protected readonly ZDateTime importTimeUtc;
		protected readonly LicenceDatabase database;
		protected readonly OrgHeader org;

		JobCategorizerRunner Runner => runner ?? (runner = new JobCategorizerRunner(true));
		JobCategorizerRunner runner;

		protected bool HasChanges { get; set; }
		protected readonly List<OrgContact> WebAccessCandidates = new List<OrgContact>();

		protected abstract BusinessObjectFactory ImportFactory { get; }

		readonly bool IsRegistrationDatabase;

		#region Import

		protected (EdiCustomerUserAccount, OrgContact) ImportSingleContact(ContactImportValue contactValue, bool shouldUpdateContactRelationship = true)
		{
			var userAccount = FindOrCreateUserAccount(contactValue);

			if (org == null)
			{
				return (userAccount, null);
			}

			var contact = FindContact(contactValue, userAccount);

			if (!CanLinkAndUpdateOrCreateContact(userAccount, contact))
			{
				UpdateUserAccount(contactValue, userAccount, org, contact);
				return (userAccount, null);
			}

			if (contact != null)
			{
				UpdateUserAccountContactRelationship(contactValue, userAccount, contact, shouldUpdateContactRelationship);
			}

			UpdateUserAccount(contactValue, userAccount, org, contact);
			if (contact != null)
			{
				UpdateContactInfo(contact, contactValue, userAccount);

				if (userAccount == null || userAccount.EUA_IsContactRelationshipActive || !contactValue.IsUserActive)
				{
					SetContactActiveStatus(contact, contactValue.IsUserActive, userAccount);
				}
			}
			else if (contactValue.IsUserActive)
			{
				contact = CreateNewContact(userAccount, contactValue);
			}

			if (contact != null)
			{
				if (userAccount != null && userAccount.EUA_OC_WebAccessContact.IsEmpty && userAccount.EUA_IsActive)
				{
					userAccount.EUA_OC_WebAccessContact = contact.PK;

					if (!userAccount.EUA_IsContactRelationshipActive && userAccount.EUA_ContactRelationshipStatus.IsEmpty)
					{
						userAccount.EUA_IsContactRelationshipActive = true;
					}
				}

				if (contact.OC_IsActive)
				{
					if (!contact.OC_Email.IsEmpty)
					{
						WebAccessCandidates.Add(contact);
					}
					else
					{
						contact.OC_WebAccessEnabled = false;
					}
				}

				if (contact.HasChanges || !contact.IsInDatabase)
				{
					contact.OC_DetailsVerified = importTimeUtc;
					HasChanges = true;
				}
			}

			if (userAccount != null && userAccount.HasChanges)
			{
				HasChanges = true;
			}

			return (userAccount, contact);
		}

		#region Match and Find

		protected EdiCustomerUserAccount FindOrCreateUserAccount(ContactImportValue contactValue)
		{
			EdiCustomerUserAccount result = null;
			if (!contactValue.UserId.IsEmpty && database != null)
			{
				var query = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, database.PK);
				query.AddToFilter(EdiCustomerUserAccountSchema.EUA_UserID, contactValue.UserId);
				result = ImportFactory.LoadTop1<EdiCustomerUserAccount>(query);
				if (result == null)
				{
					result = ImportFactory.New<EdiCustomerUserAccount>();
					result.EUA_LD = database.PK;
					result.EUA_UserID = contactValue.UserId;
					result.EUA_IsEmailVerificationRequired = true;
				}
			}
			return result;
		}

		void UpdateUserAccountContactRelationship(ContactImportValue contactValue, EdiCustomerUserAccount userAccount, OrgContact contact, bool shouldUpdateContactRelationship)
		{
			if (userAccount == null)
			{
				return;
			}

			if (userAccount.EUA_OC_WebAccessContact.IsEmpty)
			{
				var userAccountLinkedContactQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contact.PK);
				if (ImportFactory.ExistsInDatabase(EdiCustomerUserAccountSchema.Constants.TableName, userAccountLinkedContactQuery))
				{
					userAccount.EUA_IsContactRelationshipActive = false;
					userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked;
					shouldUpdateContactRelationship = false;
				}
			}

			if (shouldUpdateContactRelationship && EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.Value)
			{
				if (!contactValue.IsUserActive || (contactValue.IsUserActive && !userAccount.EUA_IsActive))
				{
					userAccount.EUA_IsContactRelationshipActive = false;

					if (!userAccount.EUA_Email.EqualsIgnoringCase(contactValue.Email))
					{
						userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;
					}
					else if (contactValue.IsUserActive && !userAccount.EUA_IsActive)
					{
						userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
					}
					else if (!contactValue.IsUserActive)
					{
						userAccount.EUA_ContactRelationshipStatus = "";
					}
				}
			}
		}

		protected abstract bool CanLinkAndUpdateOrCreateContact(EdiCustomerUserAccount userAccount, OrgContact contact);
		protected abstract OrgContact FindContact(ContactImportValue contactValue, EdiCustomerUserAccount userAccount);
		protected abstract ZString GetUniqueContactName(ZString proposedName, OrgContact contact);

		protected OrgContact FindSingleBestMatchContact(BusinessObjectFactory factory, ContactImportValue contactValue, EdiCustomerUserAccount userAccount)
		{
			var result = userAccount?.WebAccessContact;

			if (result == null && !contactValue.Email.IsEmpty)
			{
				var contactQuery = new ZQuery(OrgContactSchema.OC_OH, org.PK);
				contactQuery.AddToFilter(OrgContactSchema.OC_Email, contactValue.Email);
				var foundContacts = factory.Load<OrgContact>(contactQuery);

				OrgContact[] validContacts;
				if (userAccount != null)
				{
					var contactsLinkedToOtherUserAccountQuery = new ZDBOnlyQuery(typeof(OrgContact));
					var userAccountSubQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact);
					userAccountSubQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_LD, database.PK);
					userAccountSubQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, foundContacts.Select(x => x.PK));
					userAccountSubQuery.AddToFilter(EdiCustomerUserAccountSchema.PK, SQLComparisonOperator.NotEqual, userAccount.PK);
					contactsLinkedToOtherUserAccountQuery.AddSubQuery(userAccountSubQuery, JoinCondition.And);
					var contactsLinkedToOtherUserAccount = factory.Load<OrgContact>(contactsLinkedToOtherUserAccountQuery);
					validContacts = foundContacts.Except(contactsLinkedToOtherUserAccount).ToArray();
				}
				else
				{
					validContacts = foundContacts;
				}

				result = GetBestMatchedContact(validContacts, contactValue.Name);
			}
			else if (result != null && !result.OC_IsActive && !result.OC_Email.IsEmpty && result.OC_Email.EqualsIgnoringCase(contactValue.Email))
			{
				var alternativeContactQuery = new ZQuery(OrgContactSchema.OC_OH, org.PK);
				alternativeContactQuery.AddToFilter(OrgContactSchema.PK, SQLComparisonOperator.NotEqual, result.PK);
				alternativeContactQuery.AddToFilter(OrgContactSchema.OC_Email, result.OC_Email);
				alternativeContactQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
				alternativeContactQuery.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
				var alternativeContact = factory.LoadTop1<OrgContact>(alternativeContactQuery);
				if (alternativeContact != null)
				{
					userAccount.EUA_OC_WebAccessContact = alternativeContact.PK;
					userAccount.EUA_IsEmailVerificationRequired = true;
					result = alternativeContact;
				}
			}

			return result;
		}

		protected OrgContact GetBestMatchedContact(IEnumerable<OrgContact> candidateContacts, string nameForMatching)
		{
			return candidateContacts
				.OrderBy(x => !x.OC_Email.IsEmpty ? 0 : 1)
				.ThenBy(x => x.OC_IsActive ? 0 : 1)
				.ThenBy(x => x.OC_WebAccessEnabled ? 0 : 1)
				.ThenBy(x => x.ContactNameWithoutNumberSuffix.EqualsIgnoringCase(nameForMatching) ? 0 : 1)
				.ThenBy(x => x.PK)
				.FirstOrDefault();
		}

		#endregion

		#region Update / Create

		void UpdateUserAccount(ContactImportValue contactValue, EdiCustomerUserAccount userAccount, OrgHeader webAccessOrg, OrgContact contactForEmailCheck)
		{
			if (userAccount == null)
			{
				return;
			}

			userAccount.EUA_UserID = contactValue.UserId;
			userAccount.EUA_FullName = contactValue.Name;
			userAccount.EUA_IsActive = contactValue.IsUserActive;
			userAccount.EUA_SystemVerifiedDateUtc = importTimeUtc;

			if (!contactValue.IsUserActive)
			{
				userAccount.EUA_OC_WebAccessContact = ZGuid.Empty;
			}

			UpdateUserAccountEmail(userAccount, contactValue.Email, webAccessOrg, contactForEmailCheck);
		}

		void UpdateUserAccountEmail(EdiCustomerUserAccount userAccount, ZString email, OrgHeader webAccessOrg, OrgContact contactForEmailCheck)
		{
			if (!userAccount.EUA_Email.EqualsIgnoringCase(email))
			{
				userAccount.EUA_PreviousEmail = userAccount.EUA_Email;
				userAccount.EUA_Email = email;
			}

			if (!userAccount.EUA_Email.EqualsIgnoringCase(email) || userAccount.EUA_OC_WebAccessContact.IsEmpty)
			{
				userAccount.EUA_IsEmailOverridden = false;
			}

			var checkContact = userAccount.WebAccessContact ?? contactForEmailCheck;
			var hasDuplicate = OrgHasContactDuplicate(webAccessOrg.PK, email, checkContact, userAccount.Factory);

			if (hasDuplicate && (checkContact == null || !checkContact.OC_WebAccessEnabled))
			{
				userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.DistinctEmailRequired;
			}
			else if (userAccount.EUA_ContactRelationshipStatus.EqualsIgnoringCase(ContactRelationshipStatusList.Codes.DistinctEmailRequired))
			{
				userAccount.EUA_ContactRelationshipStatus = string.Empty;
				userAccount.EUA_IsContactRelationshipActive = true;
			}
		}

		protected void CheckDuplicates(OrgContact[] contacts, BusinessObjectFactory factory)
		{
			if (contacts.Length == 0)
			{
				return;
			}

			var duplicates = new List<ZGuid>();
			foreach (var contact in contacts)
			{
				if (contact.OC_WebAccessEnabled)
				{
					continue;
				}

				var hasDuplicate = OrgHasContactDuplicate(contact.OC_OH, contact.OC_Email, contact, factory);
				if (hasDuplicate)
				{
					duplicates.Add(contact.PK);
				}
			}

			if (duplicates.Count > 0)
			{
				var accounts = factory.Load<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, duplicates.ToArray()));
				foreach (var account in accounts)
				{
					if (!account.WebAccessContact.OC_WebAccessEnabled)
					{
						account.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.DistinctEmailRequired;
					}
				}

				factory.Save();

				duplicates.Clear();
			}
		}

		void UpdateContactInfo(OrgContact contact, ContactImportValue contactValue, EdiCustomerUserAccount userAccount)
		{
			var updateType =  GetContactInfoUpdateType(contact, userAccount);
			PopulateContactInfo(contact, contactValue, updateType, userAccount != null && userAccount.EUA_IsEmailOverridden);
		}

		public static bool OrgHasContactDuplicate(ZGuid orgPk, ZString email, OrgContact exceptContact, BusinessObjectFactory factory)
		{
			if (email.IsEmpty || (exceptContact != null && exceptContact.OC_WebAccessEnabled))
			{
				return false;
			}

			var query = new ZQuery(OrgContactSchema.OC_OH, orgPk);
			query.AddToFilter(OrgContactSchema.OC_Email, email);
			query.AddToFilter(OrgContactSchema.OC_IsActive, true);

			if (exceptContact != null)
			{
				query.AddToFilter(OrgContactSchema.PK, SQLComparisonOperator.NotEqual, exceptContact.PK);
			}
			return factory.Exists(typeof(OrgContact), query);
		}

		public static OrgContact GetDuplicateContactFromOrg(ZGuid orgPk, ZString email, OrgContact exceptContact, BusinessObjectFactory factory)
		{
			if (email.IsEmpty)
			{
				return null;
			}

			var query = new ZQuery(OrgContactSchema.OC_OH, orgPk);
			query.AddToFilter(OrgContactSchema.OC_Email, email);
			query.AddToFilter(OrgContactSchema.OC_IsActive, true);

			if (exceptContact != null)
			{
				query.AddToFilter(OrgContactSchema.PK, SQLComparisonOperator.NotEqual, exceptContact.PK);
			}

			query.OrderBy = OrgContactSchema.Constants.OC_WebAccessEnabled + OrderByClause.Descending;

			return factory.LoadTop1<OrgContact>(query);
		}

		OrgContact CreateNewContact(EdiCustomerUserAccount userAccount, ContactImportValue contactValue)
		{
			var contact = ImportFactory.New<OrgContact>();
			using (contact.GetValidationSuspender())
			{
				contact.OC_OH = org.PK;
			}

			LinkContactToStaffPersonIfRegistrationDatabase(contact, userAccount);
			PopulateContactInfo(contact, contactValue, ContactInfoUpdateType.Default, userAccount != null && userAccount.EUA_IsEmailOverridden);
			return contact;
		}

		void LinkContactToStaffPersonIfRegistrationDatabase(OrgContact contact, EdiCustomerUserAccount userAccount)
		{
			if (IsRegistrationDatabase)
			{
				var staff = contact.Factory.LoadFromNaturalKey<EDIGlbStaff>(GlbStaffSchema.GS_Code, userAccount.EUA_UserID);
				if (staff != null)
				{
					contact.OC_PER = staff.GS_PER;
				}
			}
		}

		void PopulateContactInfo(OrgContact contact, ContactImportValue contactValue, ContactInfoUpdateType updateType, bool isEmailOverridden)
		{
			if (!isEmailOverridden && !contact.OC_Email.EqualsIgnoringCase(contactValue.Email))
			{
				contact.OC_Email = contactValue.Email;
			}

			if (updateType == ContactInfoUpdateType.EmailOnly)
			{
				return;
			}

			SetProperty(contact.OC_TitleInfo, contactValue.JobTitle);
			SetProperty(contact.OC_LanguageInfo, contactValue.Language);
			SetProperty(contact.OC_PhoneInfo, GetNormalizedPhone(contact, org, contactValue.WorkPhone));
			SetProperty(contact.OC_PhoneExtensionInfo, contactValue.WorkPhoneExtension);

			if (updateType != ContactInfoUpdateType.ExcludeContactNameAndMobile)
			{
				using (contact.GetValidationSuspender())
				{
					contact.OC_ContactName = GetUniqueContactName(contactValue.Name, contact);
				}

				SetProperty(contact.OC_MobileInfo, GetNormalizedPhone(contact, org, contactValue.Mobile));
			}

			SetJobCategory(contact);
			ReplaceInvalidContactNumbers(contact);
			SetOrgAddress(contact, contactValue.BranchCode);
		}

		void SetContactActiveStatus(OrgContact contact, bool isUserActive, EdiCustomerUserAccount userAccount)
		{
			if (contact.OC_IsActive != isUserActive)
			{
				var activeUserAccountLinkedContactQuery = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
				activeUserAccountLinkedContactQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contact.PK);
				activeUserAccountLinkedContactQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsActive, true);
				if (userAccount != null)
				{
					activeUserAccountLinkedContactQuery.AddToFilter(EdiCustomerUserAccountSchema.PK, SQLComparisonOperator.NotEqual, userAccount.PK);
				}
				var licenceDatabaseSubquery = new ZDBOnlySubQuery(typeof(LicenceDatabase), EdiCustomerUserAccountSchema.EUA_LD, notIn: true);
				licenceDatabaseSubquery.AddToFilter(LicenceDatabaseSchema.LD_Product, SQLComparisonOperator.Equal, ProductTypes.Codes.WiseTechAcademy);
				activeUserAccountLinkedContactQuery.AddSubQuery(licenceDatabaseSubquery, JoinCondition.And);

				if (!ImportFactory.ExistsInDatabase(EdiCustomerUserAccountSchema.Constants.TableName, activeUserAccountLinkedContactQuery))
				{
					contact.OC_IsActive = isUserActive;
				}
				else if (!contact.OC_IsActive && isUserActive)
				{
					contact.OC_IsActive = true;
				}
			}
		}

		void SetProperty(ZPropertyInfo propertyInfo, string valueAsString)
		{
			if (!string.IsNullOrWhiteSpace(valueAsString))
			{
				propertyInfo.SetValueFromString(valueAsString);
			}
			if (propertyInfo.HasErrors() && (!propertyInfo.BizObj.IsInDatabase || propertyInfo.HasChanges))
			{
				propertyInfo.SetValueFromString(string.Empty);
			}
		}

		static string GetNormalizedPhone(OrgContact contact, OrgHeader organisation, string phone)
		{
			string normalizedPhone = phone;

			if (!string.IsNullOrEmpty(phone))
			{
				string countryDialingCode = null;

				if (contact.BranchAddress != null && contact.BranchAddress.RelatedCountry != null)
				{
					countryDialingCode = contact.BranchAddress.RelatedCountry.RN_CountryDialingCode;
				}
				else if (organisation.ClosestPort != null && organisation.ClosestPort.Country != null)
				{
					countryDialingCode = organisation.ClosestPort.Country.RN_CountryDialingCode;
				}

				if (!string.IsNullOrEmpty(countryDialingCode))
				{
					if (!normalizedPhone.StartsWith("+", System.StringComparison.OrdinalIgnoreCase) && !normalizedPhone.StartsWith(countryDialingCode, System.StringComparison.OrdinalIgnoreCase))
					{
						normalizedPhone = countryDialingCode + normalizedPhone;
					}
				}

				if (!normalizedPhone.StartsWith("+", System.StringComparison.OrdinalIgnoreCase))
				{
					normalizedPhone = "+" + normalizedPhone;
				}
			}

			return normalizedPhone;
		}

		void SetOrgAddress(OrgContact contact, string branchCode)
		{
			if (contact != null)
			{
				if (database != null && !string.IsNullOrEmpty(branchCode))
				{
					var branchQuery = new ZQuery(ClientBranchSchema.LCB_LD, database.PK);
					branchQuery.AddToFilter(ClientBranchSchema.LCB_Code, branchCode);
					var clientBranch = ImportFactory.LoadTop1<ClientBranch>(branchQuery);
					var address = clientBranch?.Address;

					if (address != null && address.OA_OH == org.PK)
					{
						contact.OC_OA_OrgAddress = clientBranch.LCB_OA;
					}
					else
					{
						contact.OC_OA_OrgAddress = org.MainAddress.PK;
					}
				}
				else if (contact.OC_OA_OrgAddress.IsEmpty)
				{
					contact.OC_OA_OrgAddress = org.MainAddress.PK;
				}
			}
		}

		static void ReplaceInvalidContactNumbers(OrgContact contact)
		{
			if (!contact.IsInDatabase)
			{
				var pattern = "[^0-9() +-]";

				if (Regex.IsMatch(contact.OC_Phone, pattern))
				{
					contact.OC_Phone = string.Empty;
				}
				if (Regex.IsMatch(contact.OC_Mobile, pattern))
				{
					contact.OC_Mobile = string.Empty;
				}
				if (Regex.IsMatch(contact.OC_OtherPhone, pattern))
				{
					contact.OC_OtherPhone = string.Empty;
				}
				if (Regex.IsMatch(contact.OC_Fax, pattern))
				{
					contact.OC_Fax = string.Empty;
				}

				if (contact.OC_Email.IsEmpty)
				{
					contact.OC_NotifyMode = Constants.ContactNotifyModes.Print;
				}
			}
		}

		protected void SetJobCategory(OrgContact contact)
		{
			if (!contact.OC_Title.IsEmpty)
			{
				var bestCategory = Runner.FindBestCategory(contact.OC_Title);
				if (bestCategory != null && JobCategories.TryGetValue((bestCategory.Level, bestCategory.Area), out string category))
				{
					contact.OC_JobCategory = category;
				}
			}
			else
			{
				contact.OC_JobCategory = OrgContactJobCategories.Codes.EMU;
			}
		}

		Dictionary<(Level, Area), string> JobCategories => jobCategories ??
			(jobCategories = new Dictionary<(Level, Area), string>()
				{
					{ (Level.Leadership, Area.Undefined), OrgContactJobCategories.Codes.LEA },
					{ (Level.SeniorManagement, Area.Administration), OrgContactJobCategories.Codes.SMA },
					{ (Level.SeniorManagement, Area.Finance), OrgContactJobCategories.Codes.SMF },
					{ (Level.SeniorManagement, Area.Operations), OrgContactJobCategories.Codes.SMO },
					{ (Level.SeniorManagement, Area.SalesAndMarketing), OrgContactJobCategories.Codes.SMS },
					{ (Level.SeniorManagement, Area.Undefined), OrgContactJobCategories.Codes.SMU },
					{ (Level.Management, Area.Administration), OrgContactJobCategories.Codes.MAA },
					{ (Level.Management, Area.Finance), OrgContactJobCategories.Codes.MAF },
					{ (Level.Management, Area.Operations), OrgContactJobCategories.Codes.MAO },
					{ (Level.Management, Area.SalesAndMarketing), OrgContactJobCategories.Codes.MAS },
					{ (Level.Management, Area.Undefined), OrgContactJobCategories.Codes.MAU },
					{ (Level.Employee, Area.Administration), OrgContactJobCategories.Codes.EMA },
					{ (Level.Employee, Area.Finance), OrgContactJobCategories.Codes.EMF },
					{ (Level.Employee, Area.Operations), OrgContactJobCategories.Codes.EMO },
					{ (Level.Employee, Area.SalesAndMarketing), OrgContactJobCategories.Codes.EMS },
					{ (Level.Employee, Area.Undefined), OrgContactJobCategories.Codes.EMU },
				}
			);
		Dictionary<(Level, Area), string> jobCategories;

		enum ContactInfoUpdateType
		{
			Default = 0,
			EmailOnly = 1,
			ExcludeContactNameAndMobile = 2,
		}

		static ContactInfoUpdateType GetContactInfoUpdateType(OrgContact contact, EdiCustomerUserAccount userAccount)
		{
			var result = ContactInfoUpdateType.Default;
			if (userAccount == null || !contact.IsInDatabase || userAccount.Database.LD_LicenceType.IsEmpty)
			{
				return result;
			}

			if (!userAccount.EUA_IsContactRelationshipActive)
			{
				result = ContactInfoUpdateType.EmailOnly;
			}

			var productionDbSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			productionDbSubQuery.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, DatabaseTypes.Codes.Production);
			productionDbSubQuery.AddToFilter(LicenceDatabaseSchema.LD_Product, userAccount.Database.LD_Product);

			var productionDbQuery = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
			productionDbQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contact.PK);
			productionDbQuery.AddToFilter(EdiCustomerUserAccountSchema.PK, SQLComparisonOperator.NotEqual, userAccount.PK);
			productionDbQuery.AddSubQuery(EdiCustomerUserAccountSchema.EUA_LD, productionDbSubQuery, JoinCondition.And);

			var hasOtherProductionDatabases = contact.Factory.Exists(typeof(EdiCustomerUserAccount), productionDbQuery);

			var licenceType = userAccount.Database.LD_LicenceType;
			if (licenceType.EqualsIgnoringCase(DatabaseTypes.Codes.Production))
			{
				if (!hasOtherProductionDatabases)  // should respect the updates from the PRD database if there is no more PRD database
				{
					result = ContactInfoUpdateType.Default;
				}
				else
				{
					//the sync should be disabled to avoid racing issue if there is other PRD database
					result = ContactInfoUpdateType.EmailOnly;
				}
			}
			else
			{
				if (hasOtherProductionDatabases)
				{
					result = ContactInfoUpdateType.EmailOnly;
				}
				else if (!contact.OC_PER.IsEmpty)
				{
					var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.PK);
					contactSubQuery.AddToFilter(OrgContactSchema.OC_PER, contact.OC_PER);
					contactSubQuery.AddToFilter(OrgContactSchema.PK, SQLComparisonOperator.NotEqual, contact.PK);

					var accountQuery = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
					accountQuery.AddSubQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contactSubQuery, JoinCondition.And);
					accountQuery.AddSubQuery(EdiCustomerUserAccountSchema.EUA_LD, productionDbSubQuery, JoinCondition.And);

					if (contact.Factory.Exists(typeof(EdiCustomerUserAccount), accountQuery))
					{
						result = ContactInfoUpdateType.ExcludeContactNameAndMobile;
					}
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Save

		protected virtual bool NeedSave => true;

		public void SaveIfNeeded()
		{
			if (NeedSave)
			{
				GrantContactsWebAccess();
				ImportFactory.Save();
				HasChanges = false;
			}
		}

		protected virtual void GrantContactsWebAccess()
		{
		}

		#endregion
	}
}
