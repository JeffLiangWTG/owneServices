using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceDatabaseValidation : AutoLicenceDatabaseValidation
	{
		public LicenceDatabaseValidation(AutoLicenceDatabase database)
			: base(database)
		{
			this.Database = (LicenceDatabase)database;
		}

		protected readonly LicenceDatabase Database;

		#region Server Code

		protected override void CheckLD_ServerCode()
		{
			base.CheckLD_ServerCode();

			CheckUniqueServerCodeAmongstEnterprises();

			if (Database != null && Database.LD_ServerCode.Length != 3)
			{
				Database.LD_ServerCodeInfo.AddError("The Server Code must be exactly 3 characters in length");
			}

			if (Database != null && Database.LD_Product == ProductTypes.Codes.BorderWise && Database.LD_IsActive && Database.LD_LicenceType == DatabaseTypes.Codes.Production)
			{
				if (Database is IBusinessObjectInternals internals && internals.ParentCollections.Any(x => x.OfType<LicenceDatabase>()
											.Count(y => y.LD_IsActive && y.LD_LicenceType == DatabaseTypes.Codes.Production && y.IsEnterpriseFamilyDatabase) > 1))
				{
					Database.LD_ServerCodeInfo.AddWarning("The client Enterprise has multiple CargoWise licenses. Ensure the BOR License is unique for each company under this enterprise code.");
				}
			}
		}

		void CheckUniqueServerCodeAmongstEnterprises()
		{
			if (Database != null && Database.LicEnterprise != null)
			{
				ZQuery duplicate = new ZQuery(LicenceDatabaseSchema.LD_ServerCode, Database.LD_ServerCode);
				duplicate.AddToFilter(LicenceDatabaseSchema.LD_LE, Database.LD_LE);
				duplicate.AddToFilter(LicenceDatabaseSchema.PK, SQLComparisonOperator.NotEqual, Database.PK);
				var result = Database.Factory.Load<LicenceDatabase>(duplicate);

				if (result.Length > 0)
				{
					Database.LD_ServerCodeInfo.AddError("A database with this code already exists for this client.");
				}
			}
		}

		#endregion

		#region DBServerSecurityMode

		protected override void CheckLD_DBServerSecurityMode()
		{
			base.CheckLD_DBServerSecurityMode();
			MandatoryValidation.CheckEntered(Database.LD_DBServerSecurityModeInfo);
			ListValidation.ErrorIfInvalidCode(Database.LD_DBServerSecurityModeInfo, Database.Lookups.DatabaseSecurityModesList);

			if (Database.LD_DBServerSecurityModeInfo.HasChanges)
			{
				if (Database.LD_DBServerSecurityMode == DatabaseSecurityModePairList.Codes.ExOpen)
				{
					Database.LD_DBServerSecurityModeInfo.AddError(SecurityModeExOpenIsDeprecated);
				}
				else if (Database.LD_DBServerSecurityMode != DatabaseSecurityModePairList.Codes.Locked)
				{
					var checkpoint = EDISecurityCheckpoints.OrgLicenceModifySetDatabaseSecurityModeToOpen;

					if (checkpoint == null || !checkpoint.IsAllowed)
					{
						Database.LD_DBServerSecurityModeInfo.AddError(SetSecurityModeOpenPermissionDenied);
					}
				}
			}

			if (!Database.LD_DBServerSecurityModeInfo.HasErrors() && Database.LD_DBServerSecurityMode == DatabaseSecurityModePairList.Codes.OpenMode)
			{
				Database.LD_DBServerSecurityModeInfo.AddWarning(Open2012ModeMustHaveManagementApproval);
			}
		}

		const string SecurityModeExOpenIsDeprecated = "Cannot set security to deprecated open-before-SQL2012 mode.";
		const string SetSecurityModeOpenPermissionDenied = "You do not have rights to set server security mode to open.";
		const string Open2012ModeMustHaveManagementApproval = "Setting server security mode to open requires management approval as it conflicts with our license agreement.";

		#endregion

		#region LicenceType

		protected override void CheckLD_LicenceType()
		{
			base.CheckLD_LicenceType();
			MandatoryValidation.CheckEntered(Database.LD_LicenceTypeInfo);
			ListValidation.ErrorIfInvalidCode(Database.LD_LicenceTypeInfo, Database.Lookups.DatabaseTypesList);
		}

		#endregion

		#region Public Email Address For Update

		protected override void CheckLD_PublicEmailAddressForUpdate()
		{
			base.CheckLD_PublicEmailAddressForUpdate();

			if (!Database.LD_PublicEmailAddressForUpdate.IsEmpty)
			{
				if (Database.LD_PublicEmailAddressForUpdateInfo.HasChanges)
				{
					CheckUniqueEmailAddressCodeAmongstEnterprises();
					EmailAddressValidation.ValidateEmailAddress(Database.LD_PublicEmailAddressForUpdateInfo);
				}
			}
			else if (!Database.LD_AvailableUpgradeMethod.IsEmpty &&
				!Database.LD_AvailableUpgradeMethod.EqualsIgnoringCase(UpgradeMethods.Codes.Blocked))
			{
				if (Database.VersionCanReceiveAllSystemMessages != Customs.Business.TriState.True)
				{
					Database.LD_PublicEmailAddressForUpdateInfo.AddWarning(PublicEmailAddressShouldNotBeBlank);
				}
			}
		}

		void CheckUniqueEmailAddressCodeAmongstEnterprises()
		{
			if (Database != null && Database.LicEnterprise != null)
			{
				ZQuery dupe = new ZQuery(LicenceDatabaseSchema.LD_PublicEmailAddressForUpdate, Database.LD_PublicEmailAddressForUpdate);
				dupe.AddToFilter(LicenceDatabaseSchema.LD_LE, Database.LD_LE);
				dupe.AddToFilter(LicenceDatabaseSchema.PK, SQLComparisonOperator.NotEqual, Database.PK);
				var result = Database.Factory.Load<LicenceDatabase>(dupe);

				if (result.Length > 0)
				{
					Database.LD_PublicEmailAddressForUpdateInfo.AddError("A database with this email already exists for this client.");
				}
			}
		}

		const string PublicEmailAddressShouldNotBeBlank = "If you do not populate this field, you will not be able to deploy an upgrade to this database through ediProd.";

		#endregion

		#region Upgrade Method

		protected override void CheckLD_AvailableUpgradeMethod()
		{
			base.CheckLD_AvailableUpgradeMethod();
			ListValidation.ErrorIfInvalidCode(Database.LD_AvailableUpgradeMethodInfo, Database.Lookups.UpgradeMethodsList);

			if (!Database.IsEnterpriseFamilyDatabase && Database.LD_AvailableUpgradeMethod != UpgradeMethods.Codes.Blocked)
			{
				Database.LD_AvailableUpgradeMethodInfo.AddError("The upgrade method must be Blocked. This database product cannot have any deployment method.");
			}
			else
			{
				CheckAvailableUpgradeMethod();
			}
		}

		void CheckAvailableUpgradeMethod()
		{
			if (Database != null)
			{
				switch (Database.LD_AvailableUpgradeMethod)
				{
					case UpgradeMethods.Codes.Http:
						{
							if (!(new HttpDownload().IsSupportingVersion(Database.CurrentVersion)))
							{
								Database.LD_AvailableUpgradeMethodInfo.AddWarning(SelectedHttp);
							}
							else if (Database.VersionCanReceiveAllSystemMessages != Customs.Business.TriState.True && Database.LD_PublicEmailAddressForUpdate.IsEmpty)
							{
								Database.LD_AvailableUpgradeMethodInfo.AddError(EmptyOrBlockedErr);
							}
							break;
						}
				}
			}
		}
		const string EmptyOrBlockedErr = "Upgrade Method should be empty or Blocked when Email Address for Upgrades is empty and Current Version is before November 2016.";
		const string SelectedHttp = "Selected Http Download Upgrade Method is not supported by Current Version of the client's system.";

		#endregion

		#region Release Ring

		protected override void CheckLD_ReleaseRing()
		{
			base.CheckLD_ReleaseRing();

			MandatoryValidation.CheckEntered(Database.LD_ReleaseRingInfo);
			ListValidation.ErrorIfInvalidCode(Database.LD_ReleaseRingInfo, Database.Lookups.ReleaseRings);
			if (ReleaseRings.Lookup(Database.LD_ReleaseRing) == null)
			{
				Database.LD_ReleaseRingInfo.AddError("\"" + Database.LD_ReleaseRing + "\" is not a valid release ring.");
			}
			else
			{
				if (Database.LD_ReleaseRingInfo.HasChanges
					&& !EDISecurityCheckpoints.OrgLicenceModifyLicDatabaseModifyToHigherRing.IsAllowed
					&& new ReleaseRingComparer().Compare(Database.LD_ReleaseRingInfo.OriginalValue.ToString(), Database.LD_ReleaseRing) > 0)
				{
					Database.LD_ReleaseRingInfo.AddError("You do not have the permission to modify to a higher Release Ring (i.e. STD->DPR is NOT allowed, STD->GP1 is allowed)");
				}
			}
			if (Database.LD_ReleaseRingInfo.HasChanges || !Database.IsInDatabase)
			{
				if (Database.LicEnterprise == null || !EDIDataRegistry.Instance.NonBilledEnterpriseCodes.Value.ContainsCode((string)Database.EnterpriseCode))
				{
					var originalRing = Database.IsInDatabase ? ReleaseRings.Lookup((ZString)Database.LD_ReleaseRingInfo.OriginalValue) : null;
					var ring = ReleaseRings.Lookup(Database.LD_ReleaseRing);
					if (ring != null && IsRestrictedRing(ring) && (originalRing == null || !IsRestrictedRing(originalRing)) && !EDISecurityCheckpoints.OrgLicenceModifyLicDatabaseModifyToRestrictedRing.IsAllowed)
					{
						Database.LD_ReleaseRingInfo.AddError("You do not have the permission to modify to a restricted Release Ring");
					}
				}
			}
		}

		static bool IsRestrictedRing(ReleaseRing releaseRing)
		{
			return releaseRing.Code != ReleaseRings.Codes.GP1;
		}

		#endregion

		#region Tech / Admin Contact

		protected override void CheckLD_OC_ContractInstallerOrInternalTechContact()
		{
			base.CheckLD_OC_ContractInstallerOrInternalTechContact();
			CheckTechOrAdminContact(Database.LD_OC_ContractInstallerOrInternalTechContactInfo, Database.ContractInstallerOrInternalTechContact);
		}

		protected override void CheckLD_OC_LicenseeAdminContact()
		{
			base.CheckLD_OC_LicenseeAdminContact();
			CheckTechOrAdminContact(Database.LD_OC_LicenseeAdminContactInfo, Database.LicenseeAdminContact);
		}

		void CheckTechOrAdminContact(ZPropertyInfo info, OrgContact contact)
		{
			MandatoryValidation.WarnIfNotEntered(info);
			if (contact != null && contact.OC_Email.IsEmpty)
			{
				info.AddWarning("This contact does not have an email address specified.");
			}
		}

		#endregion

		#region Database Hosted Location

		protected override void CheckLD_HostedLocation()
		{
			base.CheckLD_HostedLocation();
			MandatoryValidation.CheckEntered(Database.LD_HostedLocationInfo);
			ListValidation.ErrorIfInvalidCode(Database.LD_HostedLocationInfo, Database.Lookups.HostedLocations);
		}

		#endregion

		#region BillingParty

		protected override void CheckLD_OH_BillingParty()
		{
			var db = Database;
			var usageOwner = db.BillingParty;
			if (usageOwner != null)
			{
				var licHeader = Database.LicHeadersForAllCompanies.Cast<LicenceHeader>().FirstOrDefault(x => x.Company != null && x.Company.LC_OH == usageOwner.PK);
				if (licHeader == null ||
					((!usageOwner.OH_IsActive || !licHeader.LA_IsActive) && db.LD_IsActive))
				{
					db.LD_OH_BillingPartyInfo.AddError("Usage owner must be an active licence on the database");
				}
			}
		}

		#endregion

		#region CurrentRunningVersion

		protected override void CheckLD_HL_CurrentRunningVersion()
		{
			base.CheckLD_HL_CurrentRunningVersion();
			var current = Database.CurrentVersion;
			if (current != null && !current.IsCompatibleProduct(Database.LD_Product))
			{
				Database.LD_HL_CurrentRunningVersionInfo.AddError("Product must be the same for both Licence and Release Build");
			}
		}

		#endregion

		#region Product

		protected override void CheckLD_Product()
		{
			base.CheckLD_Product();
			var database = Database;

			if (!database.IsInDatabase || database.LD_ProductInfo.HasChanges)
			{
				MandatoryValidation.CheckEntered(database.LD_ProductInfo);
				ListValidation.ErrorIfInvalidCode(database.LD_ProductInfo);

				if (database.LD_ProductInfo.HasChanges && !database.IsEnterpriseFamilyDatabase)
				{
					var query = new ZDBOnlyQuery(typeof(EdiLicenceUsage));
					var clientCompanySubQuery = new ZDBOnlySubQuery(typeof(ClientCompany), EdiLicenceUsageSchema.LX2_LCC);
					clientCompanySubQuery.AddToFilter(ClientCompanySchema.LCC_LD, database.PK);
					query.AddSubQuery(clientCompanySubQuery, JoinCondition.And);

					bool hasUsageData = database.Factory.ExistsInDatabase(EdiLicenceUsageSchema.Constants.TableName, query);
					if (hasUsageData)
					{
						database.LD_ProductInfo.AddError("Licence Usage data exists, product cannot be changed for billing and accounting reasons.");
					}
				}
			}

			if (database.EnterpriseCode.IsEmpty && EDIDataRegistry.Instance.ProductsRequiringEnterpriseCode.Value.Contains(database.LD_Product.ToString()))
			{
				database.LD_ProductInfo.AddError(ZString.Format("Enterprise Code is mandatory if a database with product {0} is attached", database.LD_Product));
			}
		}

		#endregion

		#region LD_Status

		protected override void CheckLD_Status()
		{
			base.CheckLD_Status();

			if (!Database.LD_Status.IsEmpty
				&& (!Database.IsInDatabase || Database.LD_StatusInfo.HasChanges))
			{
				ListValidation.ErrorIfInvalidCode(Database.LD_StatusInfo);

				if (Database.LD_Status == DatabaseStatusList.Codes.REG && Database.IsEnterpriseFamilyDatabase)
				{
					Database.LD_StatusInfo.AddError("A database can only be registered from within the installation itself.");
				}
			}

			if (Database.IsInDatabase && Database.LD_StatusInfo.HasChanges)
			{
				if ((ZString)(Database.LD_StatusInfo.OriginalValue) == DatabaseStatusList.Codes.REG)
				{
					Database.LD_StatusInfo.AddWarning("You are unregistering this database. Service tasks will not be able to run.");
				}
			}
		}

		#endregion

		#region ProductionDatabaseServerCode

		public void ValidateProductionDatabaseServerCode()
		{
			ValidateCalculatedProperty(Database.ProductionDatabaseServerCodeInfo);
		}

		protected void CheckProductionDatabaseServerCode()
		{
			if (!Database.IsInDatabase || Database.ProductionDatabaseServerCodeInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidCode(Database.ProductionDatabaseServerCodeInfo);
			}

			if (Database.LD_LicenceType != DatabaseTypes.Codes.Production)
			{
				if (Database.LD_Billable == DatabaseBillableFlagList.Codes.YesCustomer)
				{
					MandatoryValidation.CheckEntered(Database.ProductionDatabaseServerCodeInfo);
				}

				ListValidation.ErrorIfInvalidCode(Database.ProductionDatabaseServerCodeInfo);
			}
		}

		#endregion

		#region Manual Licence Expiry

		protected override void CheckLD_ManualLicenceExpiryIsValidZDateTimeRange()
		{
		}

		#endregion

		#region LD_Billable

		protected override void CheckLD_Billable()
		{
			base.CheckLD_Billable();

			if (Database != null)
			{
				if (Database.LD_Billable == DatabaseBillableFlagList.Codes.Null)
				{
					if (!Database.IsInDatabase || (Database.IsInDatabase && Database.LD_BillableInfo.HasChanges))
					{
						Database.LD_BillableInfo.AddError("Please enter a Yes/No value.");
					}
					else
					{
						Database.LD_BillableInfo.AddWarning("Please enter a value.");
					}
				}

				MandatoryValidation.CheckEntered(Database.LD_BillableInfo);
				ListValidation.ErrorIfInvalidCode(Database.LD_BillableInfo);
			}
		}

		#endregion

		#region LD_OH_WebAccessOrg

		IEnumerable<ZGuid> ValidOrgPks
		{
			get
			{
				if (validOrgPks == null)
				{
					Database.ActiveLicHeadersForAllCompanies.Load();
					var headers = Database.ActiveLicHeadersForAllCompanies.Cast<LicenceHeader>();
					validOrgPks = headers.Select(x => x.Company.LC_OH)
						.Union
						(
							headers.SelectMany
							(
								x => x.Company.LicEnterprise.Companies.Cast<LicenceCompany>()
									.Where
									(
										c => !c.LicHeadersForAllDatabases.Any()
										|| c.LicHeadersForAllDatabases.Cast<LicenceHeader>().Any(h => h.LA_IsActive)
									).Select(c => c.LC_OH))
						);
				}

				return validOrgPks;
			}
		}
		IEnumerable<ZGuid> validOrgPks;

		protected override void CheckLD_OH_WebAccessOrg()
		{
			base.CheckLD_OH_WebAccessOrg();
			if (Database.IsInDatabase && Database.LD_IsActive)
			{
				CheckMasterOrgHasRelationshipWithDatabase();
				MandatoryValidation.WarnIfNotEntered(Database.LD_OH_WebAccessOrgInfo);
			}
		}

		void CheckMasterOrgHasRelationshipWithDatabase()
		{
			var hasWarning = false;
			var defaultEnterpriseID = EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.Value;

			if (Database.IsInDatabase && (string.IsNullOrEmpty(defaultEnterpriseID) || Database.EnterpriseID != defaultEnterpriseID) && Database.LD_OH_WebAccessOrg != ZGuid.Empty)
			{
				if (!ValidOrgPks.Contains(Database.LD_OH_WebAccessOrg))
				{
					Database.LD_OH_WebAccessOrgInfo.AddWarning("The selected Master Org doesn't have relationship with the database");
					hasWarning = true;
				}
			}

			if (!hasWarning)
			{
				ListValidation.WarnIfInvalidPK(Database.LD_OH_WebAccessOrgInfo);
			}
		}

		#endregion

		#region LD_TenantID

		protected override void CheckLD_TenantID()
		{
			base.CheckLD_TenantID();
			CheckUniqueTenantIDAmongstProducts();
		}

		void CheckUniqueTenantIDAmongstProducts()
		{
			if (Database != null  && !Database.LD_Product.IsEmpty && !Database.LD_TenantID.IsEmpty && !Database.IsEnterpriseFamilyDatabase)
			{
				var query = new ZQuery(LicenceDatabaseSchema.LD_TenantID, Database.LD_TenantID);
				query.AddToFilter(LicenceDatabaseSchema.LD_Product, Database.LD_Product);
				query.AddToFilter(LicenceDatabaseSchema.PK, SQLComparisonOperator.NotEqual, Database.PK);

				if (Database.Factory.Exists(typeof(LicenceDatabase), query))
				{
					Database.LD_TenantIDInfo.AddError("A database with this Tenant ID already exists for this product.");
				}
			}
		}

		#endregion

		#region LD_AllowAutoLogin

		protected override void CheckLD_AllowAutoLogin()
		{
			base.CheckLD_AllowAutoLogin();
			if (Database.LD_AllowAutoLogin && Database.LD_OH_WebAccessOrg.IsEmpty)
			{
				Database.LD_AllowAutoLoginInfo.AddError("Please set master org before enabling auto web login.");
			}
		}

		#endregion

		#region MUG / UPG schdule

		protected override void CheckLD_NextRunTimeUtcMUGIsValidZDateTime()
		{
			// read only property gets value from version report
		}

		protected override void CheckLD_NextRunTimeUtcMUGIsValidZDateTimeRange()
		{
			// read only property gets value from version report
		}

		protected override void CheckLD_ScheduleStateMUGIsWesternEuropean()
		{
			// read only property gets value from version report
		}

		protected override void CheckLD_NextRunTimeUtcUPGIsValidZDateTime()
		{
			// read only property gets value from version report
		}

		protected override void CheckLD_NextRunTimeUtcUPGIsValidZDateTimeRange()
		{
			// read only property gets value from version report
		}

		protected override void CheckLD_ScheduleStateUPGIsWesternEuropean()
		{
			// read only property gets value from version report
		}

		#endregion

		#region LD_EnablePackageDownloadOptimization
		protected override void CheckLD_EnablePackageDownloadOptimization()
		{
			base.CheckLD_EnablePackageDownloadOptimization();
			if (Database.LD_EnablePackageDownloadOptimization && Database.LD_HostedLocation == "NCW")
			{
				Database.LD_EnablePackageDownloadOptimizationInfo.AddError("You can only enable this setting for CargoWise-Cloud-hosted systems");
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateProductionDatabaseServerCode();
		}
	}
}

