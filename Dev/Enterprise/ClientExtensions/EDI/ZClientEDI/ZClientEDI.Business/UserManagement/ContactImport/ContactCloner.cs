using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class ContactCloner
	{
		public ContactCloner(BusinessObjectFactory factory, IProcessStatus processStatus = null)
		{
			this.factory = factory;
			contactPersonPkToMergeList = new List<Tuple<ZGuid, ZGuid>>(1);
			contactPersonFailToMergeList = new List<Tuple<GlbPerson, GlbPerson, string>>(1);
			ProcessStatus = processStatus;
		}
		readonly BusinessObjectFactory factory;
		readonly List<Tuple<ZGuid, ZGuid>> contactPersonPkToMergeList;
		readonly List<Tuple<GlbPerson, GlbPerson, string>> contactPersonFailToMergeList;
		readonly IProcessStatus ProcessStatus;

		public void CloneContacts(OrgHeader sourceOrg, OrgHeader targetOrg, LicenceDatabase database, List<ZGuid> licenceDatabaseBatchForContactsClone = null)
		{
			if (sourceOrg == null || targetOrg == null || sourceOrg.PK == targetOrg.PK || !sourceOrg.IsInDatabase || !targetOrg.IsInDatabase)
			{
				return;
			}

			var securityDict = CloneOrgSecurities(sourceOrg, targetOrg);
			var addressDict = CloneContactAddresses(sourceOrg, targetOrg, database);
			CloneContactsCore(sourceOrg, targetOrg, database, licenceDatabaseBatchForContactsClone, addressDict, securityDict);
		}

		#region Clone related entities

		Dictionary<ZGuid, ZGuid> CloneOrgSecurities(OrgHeader sourceOrg, OrgHeader targetOrg)
		{
			var orgSecuritiesDict = new Dictionary<ZGuid, ZGuid>();

			var sourceOrgSecurities = factory.Load<OrgSecurity>(new ZQuery(OrgSecuritySchema.OX_OH, sourceOrg.PK));
			var targetOrgSecurities = factory.Load<OrgSecurity>(new ZQuery(OrgSecuritySchema.OX_OH, targetOrg.PK));
			var targetOrgSecuritiesDictByName = targetOrgSecurities.Where(x => x.OX_SU.IsEmpty).ToDictionary(x => x.OX_SecurityItemName);
			var targetOrgSecuritiesDictByDoc = targetOrgSecurities.Where(x => !x.OX_SU.IsEmpty).ToDictionary(x => x.OX_SU);
			var securitiesProcessedCount = 0;

			foreach (var sourceOrgSecurity in sourceOrgSecurities)
			{
				UpdateStatus("Cloning Org Securities...", ++securitiesProcessedCount, sourceOrgSecurities.Length);
				OrgSecurity targetOrgSecurity = null;

				if (sourceOrgSecurity.OX_SU.IsEmpty)
				{
					if (!targetOrgSecuritiesDictByName.TryGetValue(sourceOrgSecurity.OX_SecurityItemName, out targetOrgSecurity))
					{
						targetOrgSecurity = factory.New<OrgSecurity>();
						targetOrgSecurity.OX_OH = targetOrg.PK;
						targetOrgSecurity.OX_SecurityItemName = sourceOrgSecurity.OX_SecurityItemName;
					}
				}
				else
				{
					if (!targetOrgSecuritiesDictByDoc.TryGetValue(sourceOrgSecurity.OX_SU, out targetOrgSecurity))
					{
						targetOrgSecurity = factory.New<OrgSecurity>();
						targetOrgSecurity.OX_OH = targetOrg.PK;
						targetOrgSecurity.OX_SU = sourceOrgSecurity.OX_SU;
					}
				}

				if (targetOrgSecurity != null)
				{
					targetOrgSecurity.OX_Granted = sourceOrgSecurity.OX_Granted;
					orgSecuritiesDict.Add(sourceOrgSecurity.PK, targetOrgSecurity.PK);
				}
			}

			return orgSecuritiesDict;
		}

		Dictionary<ZGuid, ZGuid> CloneContactAddresses(OrgHeader sourceOrg, OrgHeader targetOrg, LicenceDatabase database)
		{
			var clientBranchQuery = new ZQuery(ClientBranchSchema.LCB_LD, database.PK);
			clientBranchQuery.AddToFilter(ClientBranchSchema.LCB_OA, SQLComparisonOperator.NotEqual, null);
			var clientBranchList = factory.Load<ClientBranch>(clientBranchQuery);
			var clientBranchDict = clientBranchList.ToDictionary(x => x.LCB_OA);
			var addressDict = new Dictionary<ZGuid, ZGuid>();
			var targetOrgAddresses = targetOrg.Addresses.Cast<OrgAddress>();
			var addressCodesMap = targetOrgAddresses.ToDictionary(x => x.OA_Code.ToString(), x => x.PK, StringComparer.OrdinalIgnoreCase);

			var addressCloneArgs = new BusinessObjectCloneArgs(new string[] { OrgAddressSchema.Constants.OA_OH }, performRowCopyWithoutTriggeringValidationAndSetter: true);
			var addressProcessedCount = 0;
			var sourceOrgAddressCount = sourceOrg.Addresses.Count;
			foreach (OrgAddress sourceAddress in sourceOrg.Addresses)
			{
				UpdateStatus("Cloning Contact Addresses...", ++addressProcessedCount, sourceOrgAddressCount);
				if (clientBranchDict.TryGetValue(sourceAddress.PK, out ClientBranch clientBranch))
				{
					var targetAddress = targetOrgAddresses.FirstOrDefault(x => IsSameAddress(sourceAddress, x) && !x.IsMainAddress);

					if (targetAddress == null || clientBranchDict.ContainsKey(targetAddress.PK))
					{
						targetAddress = sourceAddress.Clone(addressCloneArgs) as OrgAddress;
						targetAddress.OA_OH = targetOrg.PK;
						targetOrg.Addresses.Add(targetAddress);
					}
					else
					{
						targetAddress.CopyPersistentValuesFrom(sourceAddress, addressCloneArgs);
						targetAddress.HasChanges = true;
					}

					targetAddress.OA_Code = ClientBranch.GetUniqueAddressCode(addressCodesMap, clientBranch.LCB_Code, clientBranch.LCB_Name, targetOrg, targetAddress);

					if (!targetAddress.IsAddressOfType(OrgAddressType.Office))
					{
						targetAddress.AddAddressType(OrgAddressType.Office);
					}

					using (clientBranch.GetValidationSuspender())
					{
						clientBranch.LCB_OA = targetAddress.PK;
					}
					using (sourceAddress.GetValidationSuspender())
					{
						sourceAddress.OA_IsActive = false;
					}

					addressDict.Add(sourceAddress.PK, targetAddress.PK);
					clientBranchDict.Remove(sourceAddress.PK);
					clientBranchDict.Add(targetAddress.PK, clientBranch);
				}
			}

			return addressDict;
		}

		static bool IsSameAddress(OrgAddress sourceAddress, OrgAddress targetAddress)
		{
			return sourceAddress.OA_Address1.EqualsIgnoringCase(targetAddress.OA_Address1)
				&& sourceAddress.OA_Address2.EqualsIgnoringCase(targetAddress.OA_Address2)
				&& sourceAddress.OA_City.EqualsIgnoringCase(targetAddress.OA_City)
				&& sourceAddress.OA_State.EqualsIgnoringCase(targetAddress.OA_State)
				&& sourceAddress.OA_PostCode.EqualsIgnoringCase(targetAddress.OA_PostCode)
				&& sourceAddress.OA_RN_NKCountryCode.EqualsIgnoringCase(targetAddress.OA_RN_NKCountryCode);
		}

		#endregion

		#region Clone contacts core

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		void CloneContactsCore(OrgHeader sourceOrg, OrgHeader targetOrg, LicenceDatabase database, List<ZGuid> licenceDatabaseBatchForContactsClone, Dictionary<ZGuid, ZGuid> addressDict, Dictionary<ZGuid, ZGuid> securityDict)
		{
			var userAccountDict = BuildUserAccountDict(database);
			var targetContactNameDict = targetOrg.Contacts.Cast<OrgContact>().ToDictionary(x => x.OC_ContactName.ToString(), x => x.PK, StringComparer.OrdinalIgnoreCase);
			var verifiedContacts = GetVerifiedContacts(sourceOrg, targetOrg, userAccountDict).ToArray();

			if (licenceDatabaseBatchForContactsClone == null)
			{
				licenceDatabaseBatchForContactsClone = new List<ZGuid> { database.PK };
			}

			var contactsToRemainActive = BuildContactsToRemainActiveDict(verifiedContacts, licenceDatabaseBatchForContactsClone);
			var myAccountContactLogsDict = BuildMyAccountContactLogsDict(sourceOrg);

			var contactCloneArgs = GetContactCloneArgs();
			var contactHelper = new ContactValueObjectHelper("OrgContact");

			var contactProcessedCount = 0;
			foreach (var sourceContact in verifiedContacts)
			{
				UpdateStatus("Cloning Contacts...", ++contactProcessedCount, verifiedContacts.Length);
				if (userAccountDict.TryGetValue(sourceContact.PK, out var userAccountList))
				{
					var targetContact = FindMatchContact(sourceContact, targetOrg, contactHelper) as EDIOrgContact;

					if (targetContact != null && !userAccountDict.ContainsKey(targetContact.PK))
					{
						targetContact.CopyPersistentValuesFrom(sourceContact, contactCloneArgs);
						targetContact.OC_IsActive = targetContact.OC_IsActive && sourceContact.OC_IsActive;
						targetContact.HasChanges = true;
					}
					else
					{
						targetContact = sourceContact.Clone(contactCloneArgs) as EDIOrgContact;
						using (targetContact.GetValidationSuspender())
						{
							targetContact.OC_OH = targetOrg.PK;
							targetContact.OC_PER = sourceContact.OC_PER;
						}
						targetOrg.Contacts.Add(targetContact);

						using (targetContact.GetValidationSuspender())
						{
							CloneContactName(targetContactNameDict, sourceContact, targetContact);
						}
					}

					using (targetContact.GetValidationSuspender())
					{
						CloneContactAddress(targetOrg, addressDict, sourceContact, targetContact);
					}

					CloneMyAccountLog(database, myAccountContactLogsDict, sourceContact, targetContact);
					CloneContactSecurities(factory, securityDict, sourceContact, targetContact);
					CloneDocumentGroups(sourceContact, targetContact);

					foreach (var userAccount in userAccountList)
					{
						using (userAccount.GetValidationSuspender())
						{
							userAccount.EUA_OC_WebAccessContact = targetContact.PK;
						}
					}

					using (sourceContact.GetValidationSuspender())
					{
						if (!contactsToRemainActive.ContainsKey(sourceContact.PK))
						{
							sourceContact.SupersedeWebAccess();
						}
					}

					if (targetContact.IsInDatabase && sourceContact.OC_PER != targetContact.OC_PER)
					{
						contactPersonPkToMergeList.Add(Tuple.Create(sourceContact.OC_PER, targetContact.OC_PER));
					}
					else
					{
						TransferPersonPrimaryRelationship(sourceContact, targetContact);
					}

					userAccountDict.Remove(sourceContact.PK);
					userAccountDict.Add(targetContact.PK, userAccountList);
				}
			}
		}

		#region MoveContactToMasterOrg

		public static OrgContact MoveContactToMasterOrg(EDIOrgContact sourceContact, LicenceDatabase database)
		{
			var factory = sourceContact.Factory;
			if (sourceContact.OC_OH.Equals(database.LD_OH_WebAccessOrg))
			{
				return sourceContact;
			}

			var masterOrg = database.WebAccessOrg;
			var contactHelper = new ContactValueObjectHelper("OrgContact");
			var targetContact = FindMatchContact(sourceContact, masterOrg, contactHelper) as EDIOrgContact;

			var sourceUserAccountsQuery = GetUserAccountsQuery(sourceContact);
			var sourceUserAccounts = factory.Load<EdiCustomerUserAccount>(sourceUserAccountsQuery);

			if (sourceUserAccounts.Length > 0)
			{
				var contactCloneArgs = GetContactCloneArgs();
				var isSourceLinkedToProduction = sourceUserAccounts.Any(x => x.Database.LD_LicenceType.EqualsIgnoringCase(DatabaseTypes.Codes.Production));

				targetContact = CreateOrUpdateTargetContact(masterOrg, sourceContact, targetContact, contactCloneArgs, isSourceLinkedToProduction);
				UpdateContactNameAndAddress(masterOrg, sourceContact, targetContact, isSourceLinkedToProduction);

				var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), StmALogSchema.SL_Parent);
				contactSubQuery.AddToFilter(OrgContactSchema.PK, sourceContact.PK);
				var logQuery = GetMyAccountLogQuery(contactSubQuery);
				var myAccountLogs = factory.Load<StmALog>(logQuery);

				if (myAccountLogs.Any())
				{
					CloneEarliestMyAccountLog(database, myAccountLogs, targetContact);
				}

				CloneDocumentGroups(sourceContact, targetContact);
				sourceUserAccounts.Where(x => x.EUA_LD.Equals(database.PK)).ForEach(x => TransferUserAccount(targetContact, x));
				DeactivateContactIfNoRemainingAccounts(factory, sourceContact);

				factory.Save();

				if (!MergePersonForMasterOrg(sourceContact, targetContact, isSourceLinkedToProduction))
				{
					TransferPersonPrimaryRelationship(sourceContact, targetContact);
				}

				factory.Save();

				return targetContact;
			}

			return null;
		}

		static ZQuery GetUserAccountsQuery(OrgContact contact)
		{
			var sourceUserAccountsQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contact.PK);
			return sourceUserAccountsQuery;
		}

		static EDIOrgContact CreateOrUpdateTargetContact(OrgHeader masterOrg, EDIOrgContact sourceContact, EDIOrgContact targetContact,
			BusinessObjectCloneArgs contactCloneArgs, bool isSourceLinkedToProduction)
		{
			if (targetContact != null)
			{
				CopyPersistentValues(sourceContact, targetContact, isSourceLinkedToProduction, contactCloneArgs);
			}
			else
			{
				targetContact = sourceContact.Clone(contactCloneArgs) as EDIOrgContact;
				using (targetContact.GetValidationSuspender())
				{
					targetContact.OC_OH = masterOrg.PK;
					targetContact.OC_PER = sourceContact.OC_PER;
				}
				masterOrg.Contacts.Add(targetContact);
			}

			return targetContact;
		}

		static void CopyPersistentValues(OrgContact sourceContact, OrgContact targetContact, bool isSourceLinkedToProduction, BusinessObjectCloneArgs contactCloneArgs)
		{
			if (isSourceLinkedToProduction)
			{
				contactCloneArgs.AddExcludedColumns(new[]
				{
					OrgContactSchema.Constants.OC_PasswordHash,
					OrgContactSchema.Constants.OC_PasswordHashIterations,
					OrgContactSchema.Constants.OC_PasswordSalt
				});

				targetContact.CopyPersistentValuesFrom(sourceContact, contactCloneArgs);
				targetContact.HasChanges = true;
			}

			if (!targetContact.HasPassword)
			{
				CopyPassword(sourceContact, targetContact);
			}

			targetContact.OC_WebAccessEnabled = sourceContact.OC_WebAccessEnabled;
		}

		static void CopyPassword(OrgContact sourceContact, OrgContact targetContact)
		{
			targetContact.OC_PasswordHash = sourceContact.OC_PasswordHash;
			targetContact.OC_PasswordHashIterations = sourceContact.OC_PasswordHashIterations;
			targetContact.OC_PasswordSalt = sourceContact.OC_PasswordSalt;
		}

		static void UpdateContactNameAndAddress(OrgHeader masterOrg, OrgContact sourceContact, OrgContact targetContact, bool isSourceLinkedToProduction)
		{
			using (targetContact.GetValidationSuspender())
			{
				if (isSourceLinkedToProduction || !targetContact.IsInDatabase)
				{
					var targetContactNameDict = masterOrg.Contacts.Cast<OrgContact>().ToDictionary(x => x.OC_ContactName.ToString(), x => x.PK, StringComparer.OrdinalIgnoreCase);
					CloneContactName(targetContactNameDict, sourceContact, targetContact);
				}

				if (isSourceLinkedToProduction && !sourceContact.OC_OA_OrgAddress.IsEmpty)
				{
					var targetAddress = masterOrg.Addresses.Cast<OrgAddress>().FirstOrDefault(x => IsSameAddress(sourceContact.OrgAddress, x) && !x.IsMainAddress);
					if (targetAddress != null)
					{
						targetContact.OC_OA_OrgAddress = targetAddress.PK;
					}
				}
			}
		}

		static void TransferUserAccount(OrgContact targetContact, EdiCustomerUserAccount userAccount)
		{
			using (userAccount.GetValidationSuspender())
			{
				userAccount.EUA_OC_WebAccessContact = targetContact.PK;
				if (targetContact.HasPassword)
				{
					userAccount.EUA_IsContactRelationshipActive = false;
					userAccount.EUA_ContactRelationshipStatus =
						ContactRelationshipStatusList.Codes.DissolvedContactWithPassword;
				}
				else if (!userAccount.EUA_IsContactRelationshipActive)
				{
					userAccount.EUA_IsEmailVerificationRequired = true;
				}
			}
		}

		static void DeactivateContactIfNoRemainingAccounts(BusinessObjectFactory factory, OrgContact sourceContact)
		{
			if (factory.Exists(typeof(EdiCustomerUserAccount), GetUserAccountsQuery(sourceContact)))
			{
				return;
			}

			using (sourceContact.GetValidationSuspender())
			{
				sourceContact.OC_IsActive = false;
			}
		}

		static bool MergePersonForMasterOrg(OrgContact sourceContact, OrgContact targetContact, bool isSourceLinkedToProduction)
		{
			if (sourceContact.OC_PER != targetContact.OC_PER)
			{
				var sourcePerson = sourceContact.Person;
				var targetPerson = targetContact.Person;

				using (var personMerger = isSourceLinkedToProduction ? new PersonMerger(sourcePerson, targetPerson) : new PersonMerger(targetPerson, sourcePerson))
				{
					try
					{
						personMerger.Merge();
						return true;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("Merging person when master org is changed", ex);
					}
				}
			}

			return false;
		}

		#endregion

		static BusinessObjectCloneArgs GetContactCloneArgs()
		{
			var contactCloneExcludeColumns = new[] {
				OrgContactSchema.Constants.OC_OH,
				OrgContactSchema.Constants.OC_OA_OrgAddress,
				OrgContactSchema.Constants.OC_ContactName,
				OrgContactSchema.Constants.OC_PER,
				OrgContactSchema.Constants.OC_Gender,
				OrgContactSchema.Constants.OC_Birthday,
				OrgContactSchema.Constants.OC_RN_NKNationality,
				OrgContactSchema.Constants.OC_PersonalInfo
			};

			return new BusinessObjectCloneArgs(contactCloneExcludeColumns, performRowCopyWithoutTriggeringValidationAndSetter: true);
		}

		static void CloneDocumentGroups(EDIOrgContact sourceContact, EDIOrgContact targetContact)
		{
			foreach (var contactType in OrgCodeLists.ContactType_List.GetAllCodes())
			{
				if (sourceContact.HasDocumentGroup(contactType))
				{
					targetContact.AddDocumentGroup(contactType, true, null);
				}
			}
		}

		static void TransferPersonPrimaryRelationship(EDIOrgContact sourceContact, EDIOrgContact targetContact)
		{
			var primaryRelationship = sourceContact.Person.PrimaryRelationship;
			if (primaryRelationship == null || primaryRelationship.PPR_PrimaryId == sourceContact.PK)
			{
				sourceContact.Person.SetPrimaryRelationship(targetContact);
			}
		}

		Dictionary<ZGuid, List<EdiCustomerUserAccount>> BuildUserAccountDict(LicenceDatabase database)
		{
			var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, database.PK);
			userAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, SQLComparisonOperator.NotEqual, null);
			var userAccountList = factory.Load<EdiCustomerUserAccount>(userAccountQuery);
			var userAccountDict = userAccountList.ToKeyListDictionary(x => x.EUA_OC_WebAccessContact);
			return userAccountDict;
		}

		Dictionary<ZGuid, OrgContact> BuildContactsToRemainActiveDict(OrgContact[] verifiedContacts, List<ZGuid> licenceDatabaseBatchForContactsClone)
		{
			if (verifiedContacts.Length == 0)
			{
				return new Dictionary<ZGuid, OrgContact>();
			}

			var otherUserAccountsSubQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact);
			otherUserAccountsSubQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, verifiedContacts.Select(x => x.PK));
			otherUserAccountsSubQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_LD, SQLComparisonOperator.NotEqual, licenceDatabaseBatchForContactsClone);

			var contactsToRemainActiveQuery = new ZDBOnlyQuery(typeof(OrgContact));
			contactsToRemainActiveQuery.AddSubQuery(OrgContactSchema.PK, otherUserAccountsSubQuery, JoinCondition.And);
			var contactList = factory.Load<OrgContact>(contactsToRemainActiveQuery);
			var contactDict = contactList.ToDictionary(x => x.PK);
			return contactDict;
		}

		IEnumerable<EDIOrgContact> GetVerifiedContacts(OrgHeader sourceOrg, OrgHeader targetOrg, Dictionary<ZGuid, List<EdiCustomerUserAccount>> userAccountDict)
		{
			var verifiedContacts = sourceOrg.Contacts.Cast<EDIOrgContact>().Where(x => userAccountDict.ContainsKey(x.PK));
			var verifiedContactPks = verifiedContacts.Select(x => x.PK).ToArray();
			factory.AddFetchHint(OrgDocumentSchema.Instance, new ZQuery(OrgDocumentSchema.OD_OC, verifiedContactPks));
			factory.AddFetchHint(OrgSecurityContactsSchema.Instance, new ZQuery(OrgSecurityContactsSchema.OZ_OC, verifiedContactPks));
			factory.AddFetchHint(OrgSecurityContactsSchema.Instance, new ZQuery(OrgSecurityContactsSchema.OZ_OC, targetOrg.Contacts.Select(x => x.PK).ToArray()));
			factory.AddFetchHint(GlbPersonSchema.Instance, new ZQuery(GlbPersonSchema.PK, verifiedContacts.Select(x => x.OC_PER).ToArray()));
			factory.AddFetchHint(GlbPersonPrimaryRelationshipSchema.Instance, new ZQuery(GlbPersonPrimaryRelationshipSchema.PPR_PER, verifiedContacts.Select(x => x.OC_PER).ToArray()));
			factory.AddFetchHint(EdiCustomerUserAccountSchema.Instance, new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, verifiedContactPks));

			return verifiedContacts;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		Dictionary<ZGuid, IGrouping<ZGuid, StmALog>> BuildMyAccountContactLogsDict(OrgHeader sourceOrg)
		{
			var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), StmALogSchema.SL_Parent);
			contactSubQuery.AddToFilter(OrgContactSchema.OC_OH, sourceOrg.PK);
			var logQuery = GetMyAccountLogQuery(contactSubQuery);
			var myAccountLogs = factory.Load<StmALog>(logQuery);
			var myAccountContactLogsDict = myAccountLogs.GroupBy(x => x.SL_Parent).ToDictionary(x => x.Key);
			return myAccountContactLogsDict;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is maintained somewhere else")]
		static ZDBOnlyQuery GetMyAccountLogQuery(ZDBOnlySubQuery contactSubQuery)
		{
			var logQuery = new ZDBOnlyQuery(typeof(StmALog));
			logQuery.AddSubQuery(contactSubQuery, JoinCondition.And);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.ClickThroughAgreementExecuted.Code);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, EDIOrgContact.WebContractLogReference);

			return logQuery;
		}

		static void CloneContactName(Dictionary<string, ZGuid> targetContactNameDict, OrgContact sourceContact, OrgContact targetContact)
		{
			targetContact.OC_ContactName = OrgContactUniqueNameHelper.GenerateUniqueContactName(targetContactNameDict, sourceContact.OC_ContactName, targetContact);
			if (!targetContactNameDict.ContainsKey(targetContact.OC_ContactName))
			{
				targetContactNameDict.Add(targetContact.OC_ContactName, targetContact.PK);
			}
		}

		static void CloneContactAddress(OrgHeader targetOrg, Dictionary<ZGuid, ZGuid> addressDict, EDIOrgContact sourceContact, EDIOrgContact targetContact)
		{
			if (addressDict.TryGetValue(sourceContact.OC_OA_OrgAddress, out ZGuid targetAddressPk))
			{
				targetContact.OC_OA_OrgAddress = targetAddressPk;
			}
			else
			{
				targetContact.OC_OA_OrgAddress = targetOrg.MainAddress.PK;
			}
		}

		static void CloneMyAccountLog(LicenceDatabase database, Dictionary<ZGuid, IGrouping<ZGuid, StmALog>> myAccountContactLogsDict, EDIOrgContact sourceContact, EDIOrgContact targetContact)
		{
			if (myAccountContactLogsDict.TryGetValue(sourceContact.PK, out var sourceLogs))
			{
				CloneEarliestMyAccountLog(database, sourceLogs, targetContact);
			}
		}

		static void CloneEarliestMyAccountLog(LicenceDatabase database, IEnumerable<StmALog> sourceLogs, EDIOrgContact targetContact)
		{
			var firstSourceLog = sourceLogs.OrderBy(x => x.SL_PostedTimeUtc).First();
			var targetLog = targetContact.LogMyAccountDisclaimerAcknowledgement(database);
			using (((IUpdateFieldsLock)targetLog).LockForUpdatingKeyFields())
			{
				targetLog.SL_EventTime = firstSourceLog.SL_EventTime;
			}
		}

		static void CloneContactSecurities(BusinessObjectFactory factory, Dictionary<ZGuid, ZGuid> securityDict, OrgContact sourceContact, OrgContact targetContact)
		{
			var sourceContactSecurities = factory.Load<OrgSecurityContacts>(new ZQuery(OrgSecurityContactsSchema.OZ_OC, sourceContact.PK));
			var targetContactSecurities = factory.Load<OrgSecurityContacts>(new ZQuery(OrgSecurityContactsSchema.OZ_OC, targetContact.PK) { FetchOnlyFromLocalCache = !targetContact.IsInDatabase });
			foreach (var sourceContactSecurity in sourceContactSecurities)
			{
				if (securityDict.TryGetValue(sourceContactSecurity.OZ_OX, out ZGuid targetOrgSecurityPk))
				{
					var targetContactSecurity = targetContactSecurities.FirstOrDefault(x => x.OZ_OX == targetOrgSecurityPk);
					if (targetContactSecurity == null)
					{
						targetContactSecurity = factory.New<OrgSecurityContacts>();
						targetContactSecurity.OZ_OX = targetOrgSecurityPk;
						targetContactSecurity.OZ_OC = targetContact.PK;
					}
					targetContactSecurity.OZ_Granted = sourceContactSecurity.OZ_Granted;
				}
			}
		}

		static OrgContact FindMatchContact(OrgContact sourceContact, OrgHeader targetOrg, ContactValueObjectHelper contactHelper)
		{
			var xsdContact = new Xsd.OrgContact()
			{
				Name = sourceContact.OC_ContactName,
				EmailAddress = sourceContact.OC_Email,
				WebAccessEnable = sourceContact.OC_WebAccessEnabled
			};

			var result = contactHelper.FindOne(targetOrg, xsdContact, activeOnly: false);

			if (result == null || !result.OC_Email.EqualsIgnoringCase(sourceContact.OC_Email))
			{
				var query = new ZQuery(OrgContactSchema.OC_PER, sourceContact.OC_PER);
				var contactsWithSamePerson = targetOrg.Contacts.Find(query).Cast<OrgContact>();
				result = contactsWithSamePerson.OrderByDescending(x => x.OC_IsActive).ThenBy(x => x.OC_WebAccessEnabled == sourceContact.OC_WebAccessEnabled ? 0 : 1).FirstOrDefault();
			}

			return result;
		}

		#endregion

		#region Merge Contact Person

		public void MergeContactsPerson()
		{
			var personQuery = new ZQuery(GlbPersonSchema.PK, contactPersonPkToMergeList.Select(x => x.Item1).Union(contactPersonPkToMergeList.Select(x => x.Item2)));
			var personToMergeDict = factory.Load<GlbPerson>(personQuery).ToDictionary(x => x.PK);

			foreach (var tuple in contactPersonPkToMergeList)
			{
				var sourcePerson = personToMergeDict[tuple.Item1];
				var targetPerson = personToMergeDict[tuple.Item2];

				using (var personMerger = new PersonMerger(sourcePerson, targetPerson))
				{
					try
					{
						personMerger.Merge();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						contactPersonFailToMergeList.Add(Tuple.Create(sourcePerson, targetPerson, ex.Message));
						ErrorReporter.ReportOnce("Merging person when master org is changed", ex);
					}
				}
			}
		}

		public bool HasPersonMergeFailure => contactPersonFailToMergeList.Count > 0;

		public ZString GetPersonMergeFailureMessage()
		{
			var builder = new ZStringBuilder();
			foreach (var tuple in contactPersonFailToMergeList)
			{
				builder.AppendLine(FormattableString.Invariant($"{tuple.Item1.PER_FullName}: {tuple.Item3}"));
			}
			return builder.ToString();
		}

		#endregion

		#region Helpers

		void UpdateStatus(string status, int progressValue, int maximumValue)
		{
			ProcessStatus?.UpdateStatus($"{status} {progressValue} / {maximumValue}", progressValue * 100 / maximumValue);
		}

		#endregion
	}
}
