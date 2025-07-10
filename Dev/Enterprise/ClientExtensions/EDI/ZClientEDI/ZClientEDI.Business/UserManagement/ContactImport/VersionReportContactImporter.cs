using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class VersionReportContactImporter : ContactImporter
	{
		public VersionReportContactImporter(BusinessObjectFactory factory, LicenceDatabase database)
			: base(database)
		{
			lookupFactory = factory;
			importFactoryProvider = new BusinessObjectFactoryProvider();
			InitLookups();
		}

		readonly BusinessObjectFactory lookupFactory;
		protected override bool NeedSave => HasChanges;
		const int ImportBatchSize = 100;
		const int MaxRetryCount = 5;

		readonly BusinessObjectFactoryProvider importFactoryProvider;
		protected override BusinessObjectFactory ImportFactory
		{
			get
			{
				importFactoryProvider.Current.SetContext(EDIConstants.BusinessContext.VersionReportContactImporter);
				return importFactoryProvider.Current;
			}
		}

#if DEBUG
		internal BusinessObjectFactory ImportFactory_Exposed => ImportFactory;
#endif

		#region Lookups

		void InitLookups()
		{
			PopulateContactsLinkedToUserAccountMap();
			PopulateContactNameAndEmailMap();
		}

		void PopulateContactsLinkedToUserAccountMap()
		{
			ContactsLinkedToUserAccountMap = new HashSet<ZGuid>();

			var contactsLinkedToUserAccountQuery = new ZDBOnlyQuery(typeof(OrgContact));
			var userAccountSubQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact);
			userAccountSubQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_LD, database.PK);
			contactsLinkedToUserAccountQuery.AddSubQuery(userAccountSubQuery, JoinCondition.And);
			var contactsLinkedToUserAccount = lookupFactory.Load<OrgContact>(contactsLinkedToUserAccountQuery);
			foreach (var contact in contactsLinkedToUserAccount)
			{
				ContactsLinkedToUserAccountMap.Add(contact.PK);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison")]
		void PopulateContactNameAndEmailMap()
		{
			ContactsByEmail = new Dictionary<ZString, List<OrgContact>>();
			ContactsByNameWithoutNumberSuffix = new Dictionary<ZString, List<OrgContact>>();
			ContactPKsByName = new Dictionary<string, ZGuid>(StringComparer.InvariantCultureIgnoreCase);

			foreach (OrgContact contact in org.Contacts)
			{
				if (!contact.OC_Email.IsEmpty)
				{
					if (ContactsByEmail.TryGetValue(contact.OC_Email, out var contactsSameEmail))
					{
						contactsSameEmail.Add(contact);
					}
					else
					{
						contactsSameEmail = new List<OrgContact>(1)
						{
							contact
						};
						ContactsByEmail.Add(contact.OC_Email, contactsSameEmail);
					}
				}

				if (!ContactPKsByName.ContainsKey(contact.OC_ContactName))
				{
					ContactPKsByName.Add(contact.OC_ContactName, contact.PK);
				}

				if (ContactsByNameWithoutNumberSuffix.TryGetValue(contact.ContactNameWithoutNumberSuffix, out var contactsSameName))
				{
					contactsSameName.Add(contact);
				}
				else
				{
					contactsSameName = new List<OrgContact>(1)
					{
						contact
					};
					ContactsByNameWithoutNumberSuffix.Add(contact.ContactNameWithoutNumberSuffix, contactsSameName);
				}
			}
		}

		void UpdateLookups(OrgContact contact)
		{
			if (!ContactPKsByName.ContainsKey(contact.OC_ContactName))
			{
				ContactPKsByName.Add(contact.OC_ContactName, contact.PK);
			}

			if (!ContactsLinkedToUserAccountMap.Contains(contact.PK))
			{
				ContactsLinkedToUserAccountMap.Add(contact.PK);
			}
		}

		HashSet<ZGuid> ContactsLinkedToUserAccountMap { get; set; }
		Dictionary<ZString, List<OrgContact>> ContactsByEmail { get; set; }
		Dictionary<ZString, List<OrgContact>> ContactsByNameWithoutNumberSuffix { get; set; }
		Dictionary<string, ZGuid> ContactPKsByName { get; set; }

		#endregion

		#region Import

		public void ImportFromStaffReports(List<StaffReport> staffReports)
		{
			int totalCount = staffReports.Count;
			var reportsFailedToSave = new List<StaffReport>(ImportBatchSize);
			var staffReportBatches = new List<List<StaffReport>>(totalCount / ImportBatchSize + 1);
			for (int i = 0; i < totalCount; i += ImportBatchSize)
			{
				staffReportBatches.Add(staffReports.GetRange(i, Math.Min(ImportBatchSize, staffReports.Count - i)));
			}

			foreach (var batch in staffReportBatches)
			{
				ImportFactory.RefreshEnabled = false;
				AddImportFetchHints(batch);
				foreach (var report in batch)
				{
					ImportFromSingleStaffReport(report);
				}
				SaveAndHandleException((ex) => { reportsFailedToSave.AddRange(batch); });
			}

			for (var maxRetryCount = MaxRetryCount; maxRetryCount > 0 && reportsFailedToSave.Any(); maxRetryCount--)
			{
				var failedReports = reportsFailedToSave.ToArray();
				reportsFailedToSave.Clear();

				foreach (var failedReport in failedReports)
				{
					ImportFromSingleStaffReport(failedReport);
					SaveAndHandleException((ex) =>
					{
						reportsFailedToSave.Add(failedReport);
						if (maxRetryCount == 1)
						{
							ErrorReporter.ReportOnce("VersionReportContactImporter.ImportFromStaffReport", ex);
						}
					});
				}
			}
		}

		public OrgContact ImportFromSingleStaffReport(StaffReport report)
		{
			OrgContact result = null;
			var contactValue = new ContactImportValue(report);
			var contact = ImportSingleContact(contactValue).Item2;
			if (contact != null)
			{
				UpdateLookups(contact);
				result = contact;
			}
			return result;
		}

		void AddImportFetchHints(List<StaffReport> batch)
		{
			var userAccountList = GetFetchHintsUserAccounts(batch);
			var contactPks = userAccountList.Where(x => !x.EUA_OC_WebAccessContact.IsEmpty).Select(x => x.EUA_OC_WebAccessContact);
			ImportFactory.AddFetchHint(typeof(OrgContact), new ZQuery(OrgContactSchema.PK, contactPks));
		}

		protected virtual EdiCustomerUserAccount[] GetFetchHintsUserAccounts(List<StaffReport> batch)
		{
			var staffCodes = batch.Select(x => x.Code);
			var query = new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, staffCodes);
			query.AddToFilter(EdiCustomerUserAccountSchema.EUA_LD, database.PK);
			return ImportFactory.Load<EdiCustomerUserAccount>(query);
		}

		void SaveAndHandleException(Action<ZSaveException> exceptionHandler)
		{
			try
			{
				SaveIfNeeded();
			}
			catch (ZSaveException ex)
			{
				exceptionHandler(ex);
			}
			finally
			{
				importFactoryProvider.CreateNewAndReclaimMemoryWithoutSave();
			}
		}

		protected override OrgContact FindContact(ContactImportValue contactValue, EdiCustomerUserAccount userAccount)
		{
			//Find contact rules
			// - use client staff linked contact if one exists
			// - name and email match
			// - name match and blank email
			// - not linked to another staff
			// - pick active and webaccess contact first
			// - then order by name and PK

			var foundContact = ImportFactory.Load<OrgContact>(userAccount.EUA_OC_WebAccessContact);
			if (foundContact == null || foundContact.OC_OH != org.PK)
			{
				var foundContactCandidates = new List<OrgContact>(1);

				if (!contactValue.Email.IsEmpty)
				{
					if (ContactsByEmail.TryGetValue(contactValue.Email, out var contactsSameEmail))
					{
						foundContactCandidates.AddRange(contactsSameEmail);
					}

					if (ContactsByNameWithoutNumberSuffix.TryGetValue(contactValue.Name, out var contactsSameName))
					{
						foundContactCandidates.AddRange(contactsSameName.Where(x => x.OC_Email.IsEmpty));
					}
				}
				else
				{
					if (ContactsByNameWithoutNumberSuffix.TryGetValue(contactValue.Name, out var contactsSameName))
					{
						foundContactCandidates.AddRange(contactsSameName);
					}
				}

				var validFoundContacts = foundContactCandidates.Where(x => !ContactsLinkedToUserAccountMap.Contains(x.PK));
				foundContact = GetBestMatchedContact(validFoundContacts, contactValue.Name);
			}

			if (foundContact != null)
			{
				return ImportFactory.Load<OrgContact>(foundContact.PK);
			}
			else
			{
				return null;
			}
		}

		protected override bool CanLinkAndUpdateOrCreateContact(EdiCustomerUserAccount userAccount, OrgContact contact)
		{
			return userAccount != null && userAccount.Database.LD_LicenceType == DatabaseTypes.Codes.Production;
		}

		protected override ZString GetUniqueContactName(ZString proposedName, OrgContact contact)
		{
			return OrgContactUniqueNameHelper.GenerateUniqueContactName(ContactPKsByName, proposedName, contact);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void GrantContactsWebAccess()
		{
			if (WebAccessCandidates.Count == 0)
			{
				return;
			}

			var distinctCandidates = IEnumerableExtensions.DistinctBy(WebAccessCandidates.Where(x => !x.OC_Email.IsEmpty), x => x.PK);
			var candidatesByEmail = distinctCandidates.GroupBy(x => x.OC_Email.ToLower()).ToArray();
			var candidateEmailMap = candidatesByEmail.Select(x => x.Key).ToHashSet();
			var candidatePkMap = distinctCandidates.Select(x => x.PK).ToHashSet();
			var webAccessConflictContactPks = new List<ZGuid>();

			var conflictFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var confilctContactQuery = new ZQuery(OrgContactSchema.OC_OH, org.PK);
			confilctContactQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
			confilctContactQuery.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
			confilctContactQuery.AddToFilter(OrgContactSchema.OC_Email, candidateEmailMap.ToArray());
			var conflictContacts = conflictFactory.Load<OrgContact>(confilctContactQuery).Where(x => !candidatePkMap.Contains(x.PK));
			var conflictsByEmail = conflictContacts.ToDictionary(x => x.OC_Email.ToLower());

			var disabledWebAccess = new List<OrgContact>();

			foreach (var element in candidatesByEmail)
			{
				var sortedCandidates = element
					.OrderBy(c => ContactsLinkedToUserAccountMap.Contains(c.PK) ? 0 : 1)
					.ThenBy(c => c.OC_WebAccessEnabled ? 0 : 1)
					.ThenBy(c => c.ContactNameWithoutNumberSuffix)
					.ThenBy(c => c.IsInDatabase)
					.ThenBy(c => c.PK)
					.ToList();

				var firstCandidate = sortedCandidates.First();

				var conflict = conflictsByEmail.GetValueSafe(element.Key);
				if (conflict != null)
				{
					if (!ContactsLinkedToUserAccountMap.Contains(conflict.PK))
					{
						webAccessConflictContactPks.Add(conflict.PK);
					}
					else if (firstCandidate.OC_WebAccessEnabled)
					{
						RevokeWebAccess(firstCandidate);
						disabledWebAccess.Add(firstCandidate);
					}
				}

				if (conflict == null || !ContactsLinkedToUserAccountMap.Contains(conflict.PK))
				{
					using (firstCandidate.GetValidationSuspender())
					{
						if (!firstCandidate.OC_WebAccessEnabled)
						{
							firstCandidate.OC_WebAccessEnabled = true;
						}
					}
				}

				foreach (var candidateNoWebAccess in sortedCandidates.Skip(1)) //only the first candidate may have web access
				{
					RevokeWebAccess(candidateNoWebAccess);
					if (candidateNoWebAccess.IsInDatabase)
					{
						webAccessConflictContactPks.Add(candidateNoWebAccess.PK);
						disabledWebAccess.Add(candidateNoWebAccess);
					}
				}
			}

			if (webAccessConflictContactPks.Count > 0)
			{
				var query = new ZDBOnlyQuery(typeof(OrgContact));
				query.AddToFilter(OrgContactSchema.PK, webAccessConflictContactPks);

				var conflictContactsReload = conflictFactory.Load<OrgContact>(query);
				foreach (var conflict in conflictContactsReload)
				{
					conflict.OC_WebAccessEnabled = false;
				}

				conflictFactory.Save();
			}

			CheckDuplicates(disabledWebAccess.ToArray(), WebAccessCandidates[0].Factory);

			WebAccessCandidates.Clear();
			disabledWebAccess.Clear();
		}

		void RevokeWebAccess(OrgContact contactToDisable)
		{
			contactToDisable.OC_WebAccessEnabled = false;
		}

		#endregion
	}
}
