using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCustomization : AutoEdiCommissionAgreementCustomization, IEdiCommissionAgreementCustomization
	{
		public EdiCommissionAgreementCustomization(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region EZN_IsAllDatabases

		[ResourceStringData("EDICommissionAgreementCustomization|EZN_IsAllDatabases", Caption = "All Database Usages")]
		public override ZBool EZN_IsAllDatabases
		{
			get { return base.EZN_IsAllDatabases; }
			set
			{
				if (base.EZN_IsAllDatabases != value)
				{
					base.EZN_IsAllDatabases = value;
					if (value)
					{
						DatabasePivots.DeleteAll();
					}
				}
			}
		}

		#endregion

		#region EZN_IsAllCompanies

		[ResourceStringData("EDICommissionAgreementCustomization|EZN_IsAllCompanies", Caption = "All Company Usages")]
		public override ZBool EZN_IsAllCompanies
		{
			get { return base.EZN_IsAllCompanies; }
			set
			{
				if (base.EZN_IsAllCompanies != value)
				{
					base.EZN_IsAllCompanies = value;
					if (value)
					{
						CompanyPivots.DeleteAll();
						CompanyAutoAddCountries.DeleteAll();
						CompanyAutoAddDatabases.DeleteAll();
					}
				}
			}
		}

		#endregion

		#endregion

		#region CommissionAgreement

		public new EdiCommissionAgreement CommissionAgreement
		{
			get { return (EdiCommissionAgreement)base.CommissionAgreement; }
		}

		#endregion

		#region Draft

		public EdiCommissionAgreementCustomization ParentVersion
		{
			get
			{
				if (!parentVersionInitialized)
				{
					parentVersionInitialized = true;

					var commissionAgreement = CommissionAgreement;
					var commissionAgreementParentVersion = commissionAgreement != null ? commissionAgreement.ParentVersion : null;
					if (commissionAgreementParentVersion != null)
					{
						parentVersion = commissionAgreementParentVersion.Customization;
					}
				}

				return parentVersion;
			}
		}
		EdiCommissionAgreementCustomization parentVersion;
		bool parentVersionInitialized;

		public EdiCommissionAgreementCustomization MainVersion
		{
			get { return ParentVersion ?? this; }
		}

		public ZBool IsMainVersion
		{
			get { return ParentVersion == null; }
		}

		internal EdiCommissionAgreementCustomization CreateDraft()
		{
			var draft = Factory.New<EdiCommissionAgreementCustomization>();
			draft.CopyPersistentValuesFrom(this, GetDraftCloneArgs());

			foreach (var companyPivot in CompanyPivots)
			{
				draft.CompanyPivots.AddNew(companyPivot.EPY_LCC);
			}

			foreach (var companyAutoAddCountry in CompanyAutoAddCountries)
			{
				draft.CompanyAutoAddCountries.AddNew(companyAutoAddCountry.EPC_LD, companyAutoAddCountry.EPC_RN_NKCountry);
			}

			foreach (var companyAutoAddDatabase in CompanyAutoAddDatabases)
			{
				draft.CompanyAutoAddDatabases.AddNew(companyAutoAddDatabase.EPD_LD);
			}

			foreach (var databasePivot in DatabasePivots)
			{
				draft.DatabasePivots.AddNew(databasePivot.EZD_LD);
			}

			((IBusinessObjectState)draft).ClearHasChangesIncludingChildren();

			return draft;
		}

		BusinessObjectCloneArgs GetDraftCloneArgs()
		{
			return new BusinessObjectCloneArgs(new[] { EdiCommissionAgreementCustomization.Schema.EZN_CA0 }, true);
		}

		internal EdiCommissionAgreementCustomization MergeDraft()
		{
			try
			{
				isMerging = true;
				var draft = this;
				var parent = ParentVersion;
				if (parent != null)
				{
					parent.CopyPersistentValuesFrom(this, GetDraftCloneArgs());

					MergeCollection(parent, draft, x => x.CompanyPivots);
					MergeCollection(parent, draft, x => x.CompanyAutoAddCountries);
					MergeCollection(parent, draft, x => x.CompanyAutoAddDatabases);
					MergeCollection(parent, draft, x => x.DatabasePivots);

					parent.HasChanges = true;
					Delete();

					return parent;
				}
				else
				{
					return this;
				}
			}
			finally
			{
				isMerging = false;
			}
		}
		bool isMerging;

		static void MergeCollection<T>(EdiCommissionAgreementCustomization parentVersion, EdiCommissionAgreementCustomization draftVersion, Func<EdiCommissionAgreementCustomization, ActiveBusinessObjectCollection<T>> collectionGetter)
			where T : BusinessObject
		{
			Argument.NotNull(parentVersion, "parentVersion");
			Argument.NotNull(draftVersion, "draftVersion");
			Argument.NotNull(collectionGetter, "collectionGetter");

			var parentVersionCollection = collectionGetter(parentVersion);
			var draftCollection = collectionGetter(draftVersion);

			var draftItemsToAdd = new HashSet<T>(draftCollection);
			foreach (var parentVersionItem in parentVersionCollection.ToArray())
			{
				if (!draftItemsToAdd.Contains(parentVersionItem))
				{
					if (parentVersionCollection.Relationship is ManyToManyRelationship)
					{
						parentVersionCollection.RemoveFromRelationship(parentVersionItem);
					}
					else
					{
						parentVersionCollection.Delete(parentVersionItem);
					}
				}
				else
				{
					draftItemsToAdd.Remove(parentVersionItem);
				}
			}
			foreach (var draftItem in draftItemsToAdd)
			{
				parentVersionCollection.Add(draftItem);
			}
		}

		#endregion

		#region Customer

		public OrgHeader Customer
		{
			get
			{
				var agreement = CommissionAgreement;
				if (agreement == null)
				{
					return null;
				}

				return agreement.Customer;
			}
		}

		public LicenceEnterprise LicenceEnterprise
		{
			get
			{
				var customer = Customer as EDIOrgHeader;
				if (customer != null)
				{
					var licCompany = customer.LicCompany;
					if (licCompany != null)
					{
						return licCompany.LicEnterprise;
					}
					else
					{
						var enterprise = Factory.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_OH, customer.PK) { OrderBy = LicenceEnterpriseSchema.Constants.LE_EnterpriseCode });
						return enterprise.FirstOrDefault();
					}
				}

				return null;
			}
		}

		#endregion

		#region Reset To Default

		public void ResetToDefault()
		{
			EZN_IsAllDatabases = ZBool.True;
			EZN_IsAllCompanies = ZBool.True;
			DeleteAllRelatedBusinessObjects();
		}

		#endregion

		#region Related Business Objects

		public void RemoveAllRelatedBusinessObjectsNotBelongingToLicenceEnterprise()
		{
			var licenceEnterprise = LicenceEnterprise;
			var validDatabasePks = (licenceEnterprise != null) ? new HashSet<ZGuid>(licenceEnterprise.Databases.Select(x => x.PK)) : new HashSet<ZGuid>();

			foreach (var companyPivot in CompanyPivots.ToArray())
			{
				if (companyPivot.ClientCompany == null || !validDatabasePks.Contains(companyPivot.ClientCompany.LCC_LD))
				{
					companyPivot.Delete();
				}
			}

			foreach (var companyAutoAddDatabase in CompanyAutoAddDatabases.ToArray())
			{
				if (!companyAutoAddDatabase.EPD_LD.IsEmpty && !validDatabasePks.Contains(companyAutoAddDatabase.EPD_LD))
				{
					companyAutoAddDatabase.Delete();
				}
			}

			foreach (var companyAutoAddCountry in CompanyAutoAddCountries.ToArray())
			{
				if (!companyAutoAddCountry.EPC_LD.IsEmpty && !validDatabasePks.Contains(companyAutoAddCountry.EPC_LD))
				{
					companyAutoAddCountry.Delete();
				}
			}

			foreach (var databasePivot in DatabasePivots.ToArray())
			{
				if (!databasePivot.EZD_LD.IsEmpty && !validDatabasePks.Contains(databasePivot.EZD_LD))
				{
					databasePivot.Delete();
				}
			}
		}

		#region Databases

		[ChildEditable]
		public EdiCommissionAgreementDatabasePivotCollection DatabasePivots
		{
			get
			{
				if (databasePivots == null)
				{
					databasePivots = new EdiCommissionAgreementDatabasePivotCollection(this);
					EnsureSavedDatabasesInitialized();
					RegisterEditableChildObject(databasePivots);
				}

				return databasePivots;
			}
		}
		EdiCommissionAgreementDatabasePivotCollection databasePivots;

		void EnsureSavedDatabasesInitialized()
		{
			savedDatabases = new HashSet<ZGuid>(GetCurrentDatabases());
		}
		HashSet<ZGuid> savedDatabases;

		internal HashSet<ZGuid> OriginalDatabases
		{
			get
			{
				if (IsInDatabase)
				{
					return savedDatabases;
				}
				else
				{
					MainVersion.EnsureSavedDatabasesInitialized();
					return MainVersion.savedDatabases;
				}
			}
		}

		internal IEnumerable<ZGuid> GetCurrentDatabases()
		{
			return databasePivots != null ? databasePivots.Select(x => x.EZD_LD) : null;
		}

		#endregion

		#region Companies

		[ChildEditable]
		public EdiCommissionAgreementCompanyPivotCollection CompanyPivots
		{
			get
			{
				if (companyPivots == null)
				{
					companyPivots = new EdiCommissionAgreementCompanyPivotCollection(this);
					EnsureSavedCompaniesInitialized();
					RegisterEditableChildObject(companyPivots);
				}

				return companyPivots;
			}
		}
		EdiCommissionAgreementCompanyPivotCollection companyPivots;

		void EnsureSavedCompaniesInitialized()
		{
			savedCompanies = new HashSet<ZGuid>(GetCurrentCompanies());
		}
		HashSet<ZGuid> savedCompanies;

		internal HashSet<ZGuid> OriginalCompanies
		{
			get
			{
				if (IsInDatabase)
				{
					return savedCompanies;
				}
				else
				{
					MainVersion.EnsureSavedCompaniesInitialized();
					return MainVersion.savedCompanies;
				}
			}
		}

		internal IEnumerable<ZGuid> GetCurrentCompanies()
		{
			return companyPivots != null ? companyPivots.Select(x => x.EPY_LCC) : null;
		}

		#endregion

		#region CompanyAutoAddCountries

		[ChildEditable]
		public EdiCommissionAgreementCompanyAutoAddCountryCollection CompanyAutoAddCountries
		{
			get
			{
				if (companyAutoAddCountries == null)
				{
					companyAutoAddCountries = new EdiCommissionAgreementCompanyAutoAddCountryCollection(this);
					EnsureSavedCompanyAutoAddCountriesInitialized();
					RegisterEditableChildObject(companyAutoAddCountries);
				}

				return companyAutoAddCountries;
			}
		}
		EdiCommissionAgreementCompanyAutoAddCountryCollection companyAutoAddCountries;

		void EnsureSavedCompanyAutoAddCountriesInitialized()
		{
			savedCompanyAutoAddCountries = new HashSet<Tuple<ZGuid, ZString>>(GetCurrentCompanyAutoAddCountries());
		}
		HashSet<Tuple<ZGuid, ZString>> savedCompanyAutoAddCountries;

		internal HashSet<Tuple<ZGuid, ZString>> OriginalCompanyAutoAddCountries
		{
			get
			{
				if (IsInDatabase)
				{
					return savedCompanyAutoAddCountries;
				}
				else
				{
					MainVersion.EnsureSavedCompanyAutoAddCountriesInitialized();
					return MainVersion.savedCompanyAutoAddCountries;
				}
			}
		}

		internal IEnumerable<Tuple<ZGuid, ZString>> GetCurrentCompanyAutoAddCountries()
		{
			return companyAutoAddCountries != null ? companyAutoAddCountries.Select(x => Tuple.Create(x.EPC_LD, x.EPC_RN_NKCountry)) : null;
		}

		#endregion

		#region CompanyAutoAddDatabases

		[ChildEditable]
		public EdiCommissionAgreementCompanyAutoAddDatabaseCollection CompanyAutoAddDatabases
		{
			get
			{
				if (companyAutoAddDatabases == null)
				{
					companyAutoAddDatabases = new EdiCommissionAgreementCompanyAutoAddDatabaseCollection(this);
					EnsureSavedCompanyAutoAddDatabasesInitialized();
					RegisterEditableChildObject(companyAutoAddDatabases);
				}

				return companyAutoAddDatabases;
			}
		}
		EdiCommissionAgreementCompanyAutoAddDatabaseCollection companyAutoAddDatabases;

		void EnsureSavedCompanyAutoAddDatabasesInitialized()
		{
			savedCompanyAutoAddDatabases = new HashSet<ZGuid>(GetCurrentCompanyAutoAddDatabases());
		}
		HashSet<ZGuid> savedCompanyAutoAddDatabases;

		internal HashSet<ZGuid> OriginalCompanyAutoAddDatabases
		{
			get
			{
				if (IsInDatabase)
				{
					return savedCompanyAutoAddDatabases;
				}
				else
				{
					MainVersion.EnsureSavedCompanyAutoAddDatabasesInitialized();
					return MainVersion.savedCompanyAutoAddDatabases;
				}
			}
		}

		internal IEnumerable<ZGuid> GetCurrentCompanyAutoAddDatabases()
		{
			return companyAutoAddDatabases != null ? companyAutoAddDatabases.Select(x => x.EPD_LD) : null;
		}

		#endregion

		#endregion

		#region Save

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || CommissionAgreement == null || !CommissionAgreement.IsUncommittedDraft); }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			ModifiedLogs.AddLogsOnFactorySaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			ModifiedLogs.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				if (companyPivots != null)
				{
					savedCompanies = new HashSet<ZGuid>(GetCurrentCompanies());
				}

				if (databasePivots != null)
				{
					savedDatabases = new HashSet<ZGuid>(GetCurrentDatabases());
				}

				if (companyAutoAddCountries != null)
				{
					savedCompanyAutoAddCountries = new HashSet<Tuple<ZGuid, ZString>>(GetCurrentCompanyAutoAddCountries());
				}

				if (companyAutoAddDatabases != null)
				{
					savedCompanyAutoAddDatabases = new HashSet<ZGuid>(GetCurrentCompanyAutoAddDatabases());
				}
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			DeleteAllRelatedBusinessObjects();
			if (!isMerging)
			{
				ModifiedLogs.AddDetachedLogOnDelete();
			}

			base.Delete();
		}

		void DeleteAllRelatedBusinessObjects()
		{
			CompanyPivots.DeleteAll();
			CompanyAutoAddCountries.DeleteAll();
			CompanyAutoAddDatabases.DeleteAll();
			DatabasePivots.DeleteAll();
		}

		#endregion

		#region Logs

		public EdiCommissionAgreementCustomizationModifiedLogsManager ModifiedLogs
		{
			get { return modifiedLogsManager ?? (modifiedLogsManager = new EdiCommissionAgreementCustomizationModifiedLogsManager(this)); }
		}
		EdiCommissionAgreementCustomizationModifiedLogsManager modifiedLogsManager;

		#endregion

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (IsDeleted || CommissionAgreement == null || CommissionAgreement.IsDeleted)
				{
					return "Commission Agreement Customization";
				}

				return string.Format(CultureInfo.CurrentCulture, "Commission Agreement Customization {0}", CommissionAgreement.AgreementId);
			}
		}

		#endregion
	}
}

