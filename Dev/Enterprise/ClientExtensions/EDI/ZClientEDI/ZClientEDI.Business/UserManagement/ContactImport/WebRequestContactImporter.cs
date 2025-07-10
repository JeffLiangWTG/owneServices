using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class WebRequestContactImporter : ContactImporter
	{
		public WebRequestContactImporter(BusinessObjectFactory factory, LicenceDatabase database)
			: base(database)
		{
			this.factory = factory;
		}

		public WebRequestContactImporter(BusinessObjectFactory factory, LicenceDatabase database, OrgHeader webAccessOrgOverride)
			: base(database, webAccessOrgOverride)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;
		protected override BusinessObjectFactory ImportFactory => factory;

		public ContactImportResult ImportFromContactXsd(Xsd.OrgContact xsdContact, string staffCode, string branchCode)
		{
			if (org == null)
			{
				return new ContactImportResult(null, null, ContactImportResult.NoOrganizationErrorMessage);
			}

			var contactValue = new ContactImportValue(xsdContact, staffCode, branchCode);

			if (string.IsNullOrWhiteSpace(contactValue.Email))
			{
				return new ContactImportResult(null, null, ContactImportResult.NoEmailAddressErrorMessage);
			}

			var (userAccount, contact) = ImportSingleContact(contactValue);
			return new ContactImportResult(userAccount, contact, null);
		}

		public (EdiCustomerUserAccount, OrgContact) ImportFromUserAccount(EdiCustomerUserAccount userAccount, string branchCode = "")
		{
			if (org == null)
			{
				return (null, null);
			}

			var contactValue = new ContactImportValue(userAccount, branchCode);
			return ImportSingleContact(contactValue, shouldUpdateContactRelationship: false);
		}

		public (EdiCustomerUserAccount, OrgContact) ImportFromTrustedUserInfo(ITrustedUserInfo userInfo)
		{
			if (org == null)
			{
				return (null, null);
			}

			var contactValue = new ContactImportValue(userInfo);
			return ImportSingleContact(contactValue, shouldUpdateContactRelationship: false);
		}

		protected override OrgContact FindContact(ContactImportValue contactValue, EdiCustomerUserAccount userAccount)
		{
			return FindSingleBestMatchContact(factory, contactValue, userAccount);
		}

		protected override bool CanLinkAndUpdateOrCreateContact(EdiCustomerUserAccount userAccount, OrgContact contact)
		{
			return userAccount == null
				|| !userAccount.EUA_IsEmailVerificationRequired
				|| contact == null
				|| userAccount.EUA_OC_WebAccessContact == contact.PK
				|| !factory.ExistsInDatabase(EdiCustomerUserAccountSchema.Constants.TableName, new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contact.PK));
		}

		protected override ZString GetUniqueContactName(ZString proposedName, OrgContact contact)
		{
			return OrgContactUniqueNameHelper.GenerateUniqueContactName(contact, proposedName);
		}

		protected override void GrantContactsWebAccess()
		{
			var candidate = WebAccessCandidates.SingleOrDefault();
			WebAccessCandidates.Clear();

			if (candidate != null)
			{
				if (candidate.OC_Email.IsEmpty)
				{
					if (!candidate.OC_IsActive)
					{
						candidate.OC_IsActive = true;
					}
					return;
				}

				OrgContact conflict = null;
				var query = new ZQuery(OrgContactSchema.OC_Email, candidate.OC_Email);
				query.AddToFilter(OrgContactSchema.OC_OH, candidate.OC_OH);
				query.AddToFilter(OrgContactSchema.OC_IsActive, true);
				query.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
				query.AddToFilter(OrgContactSchema.PK, SQLComparisonOperator.NotEqual, candidate.PK);
				conflict = factory.LoadTop1<OrgContact>(query);

				if (conflict == null)
				{
					SetContactWebAccess(candidate, shouldGrantWebAccess: true);
				}
				else
				{
					var userAccountContactMap = database != null
						? factory.Load<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, database.PK).AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, new[] { candidate.PK, conflict.PK })).Select(x => x.EUA_OC_WebAccessContact).Distinct().ToHashSet()
						: new HashSet<ZGuid>();

					var shouldGrantWebAccess = false;
					if (userAccountContactMap == null || !userAccountContactMap.Contains(conflict.PK))
					{
						conflict.OC_WebAccessEnabled = false;
						candidate.OC_IsActive = false;
						shouldGrantWebAccess = true;
					}
					SetContactWebAccess(candidate, shouldGrantWebAccess);
				}
			}
		}

		protected void SetContactWebAccess(OrgContact contact, bool shouldGrantWebAccess)
		{
			using (contact.GetValidationSuspender())
			{
				if (contact.OC_WebAccessEnabled != shouldGrantWebAccess)
				{
					contact.OC_WebAccessEnabled = shouldGrantWebAccess;

					if (!shouldGrantWebAccess)
					{
						CheckDuplicates(new[] { contact }, contact.Factory);
					}
				}
			}
		}

		#region Web Security Rights

		public void MergeContactWebSecurity(OrgContact targetContact)
		{
			LicenceEnterprise enterprise = factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_OH, targetContact.OC_OH));
			if (enterprise == null)
			{
				var org = targetContact.Header as EDIOrgHeader;
				enterprise = org?.LicEnterprise;
			}

			if (enterprise != null)
			{
				OrgContact[] sourceContacts = FindWebContactsInRelatedOrgs(targetContact, enterprise);

				var fullSecurityRights = new List<WebSecurityRight>();
				fullSecurityRights.AddRange(EDIWebSecurityRightsList.New());
				fullSecurityRights.AddRange(EDIWebReportSecurityRightsList.New(factory));

				var targetContactRights = targetContact.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>();

				foreach (WebSecurityRight right in fullSecurityRights)
				{
					var isTargetGranted = IsWebRightGranted(targetContact, right);
					if (!isTargetGranted)
					{
						var isSourceGranted = sourceContacts.Any(x => IsWebRightGranted(x, right));
						if (isSourceGranted)
						{
							OrgSecurityContacts targetSecurity = null;
							if (right.SecurityGuid.IsEmpty)
							{
								targetSecurity = targetContactRights.FirstOrDefault(x => x.Security.OX_SecurityItemName == right.SecurityItemName);
							}
							else
							{
								targetSecurity = targetContactRights.FirstOrDefault(x => x.Security.OX_SU == right.SecurityGuid);
							}

							if (targetSecurity != null)
							{
								targetSecurity.OZ_Granted = true;
							}
						}
					}
				}

				factory.Save();
			}
		}

		OrgContact[] FindWebContactsInRelatedOrgs(OrgContact targetContact, LicenceEnterprise enterprise)
		{
			var contactQuery = new ZDBOnlyQuery(typeof(OrgContact));
			contactQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
			contactQuery.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
			contactQuery.AddToFilter(OrgContactSchema.PK, SQLComparisonOperator.NotEqual, targetContact.PK);

			var contactFilter = new ZQuery();
			contactFilter.AddToFilter(OrgContactSchema.OC_ContactName, targetContact.OC_ContactName);
			contactFilter.AddToFilter(JoinCondition.Or, OrgContactSchema.OC_ContactName, SQLComparisonOperator.StartsWith, targetContact.OC_ContactName + " (");
			contactFilter.AddToFilter(JoinCondition.Or, OrgContactSchema.OC_Email, targetContact.Email);
			contactQuery.AddToFilter(contactFilter);

			var orgQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgContactSchema.OC_OH);
			var clientCompanyQuery = new ZDBOnlySubQuery(typeof(ClientCompany), ClientCompanySchema.LCC_OH);
			var databaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), ClientCompanySchema.LCC_LD);
			databaseQuery.AddToFilter(LicenceDatabaseSchema.LD_LE, enterprise.PK);
			clientCompanyQuery.AddSubQuery(databaseQuery, JoinCondition.And);
			orgQuery.AddSubQuery(clientCompanyQuery, JoinCondition.And);
			contactQuery.AddSubQuery(orgQuery, JoinCondition.And);

			var sourceContacts = factory.Load<OrgContact>(contactQuery);
			return sourceContacts;
		}

		static bool IsWebRightGranted(OrgContact contact, WebSecurityRight right)
		{
			OrgSecurity orgRight = null;
			if (right.SecurityGuid.IsEmpty)
			{
				orgRight = contact.Header.SecurityRights.Cast<OrgSecurity>().FirstOrDefault(x => x.OX_SecurityItemName == right.SecurityItemName);
			}
			else
			{
				orgRight = contact.Header.SecurityRights.Cast<OrgSecurity>().FirstOrDefault(x => x.OX_SU == right.SecurityGuid);
			}

			if (orgRight == null)
			{
				return right.IsGrantedByDefault;
			}

			var contactRight = contact.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>().FirstOrDefault(x => x.OZ_OX == orgRight.PK);
			return contactRight?.OZ_Granted ?? orgRight.OX_Granted;
		}

		#endregion
	}
}
