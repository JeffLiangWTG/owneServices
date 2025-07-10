using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceDatabaseLookups : AutoLicenceDatabaseLookups
	{
		public LicenceDatabaseLookups(AutoLicenceDatabase parent)
			: base(parent)
		{
			MasterDatabase = (LicenceDatabase)parent;
		}

		public LicenceDatabaseLookups(AutoLicenceDatabase parent, BusinessObjectFactory factory) : base(null)
		{
			this.factory = factory;
		}

		protected override BusinessObjectFactory Factory => factory ?? base.Factory;
		readonly BusinessObjectFactory factory;

		readonly LicenceDatabase MasterDatabase;

		#region ProductList

		public CodeDescriptionPairList ProductTypeList
		{
			get
			{
				var productTypes = (CodeDescriptionPairList)new ProductTypes(includeCargoWiseOne: true);
				productTypes.Sort();
				return productTypes;
			}
		}

		#endregion

		#region Database Security Modes

		public CodeDescriptionPairList DatabaseSecurityModesList
		{
			get { return NewDatabaseSecurityModes(); }
		}

		CodeDescriptionPairList NewDatabaseSecurityModes()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(DatabaseSecurityModePairList.Codes.Locked, DatabaseSecurityModePairList.Descriptions.Locked);
			result.AddPair(DatabaseSecurityModePairList.Codes.ExOpen, DatabaseSecurityModePairList.Descriptions.ExOpen);
			result.AddPair(DatabaseSecurityModePairList.Codes.OpenMode, DatabaseSecurityModePairList.Descriptions.OpenMode);
			return result;
		}

		#endregion

		#region Database Types

		public DatabaseTypes DatabaseTypesList
		{
			get { return new DatabaseTypes(); }
		}

		#endregion

		#region CurrentVersions

		public virtual ReleaseBuildCollection CurrentVersions
		{
			get
			{
				if (fCurrentVersions == null)
				{
					fCurrentVersions = new ReleaseBuildCollection(Factory);
				}
				return fCurrentVersions;
			}
		}
		ReleaseBuildCollection fCurrentVersions;

		#endregion

		#region AllLicenceEnterprises

		public LicenceEnterpriseCollection AllLicenceEnterprises
		{
			get
			{
				if (fAllLicenceEnterprises == null)
				{
					fAllLicenceEnterprises = new LicenceEnterpriseCollection(Factory);
				}

				return fAllLicenceEnterprises;
			}
		}
		LicenceEnterpriseCollection fAllLicenceEnterprises;

		#endregion

		#region LicEnterpriseAddresses

		public OrgAddressCollection LicEnterpriseAddresses
		{
			get
			{
				OrgAddressCollection licEnterpriseAddresses = new OrgAddressCollection(new BusinessObjectFactory(), AllAddressesQuery); // Possible dirty table will barf the ZDBOnlyQuery
				licEnterpriseAddresses.Load();
				return licEnterpriseAddresses;
			}
		}

		#endregion

		#region LicEnterpriseContacts

		public OrgContactCollection LicEnterpriseContacts
		{
			get
			{
				OrgContactCollection licEnterpriseContacts;
				licEnterpriseContacts = Factory.GetCachedValue(string.Concat("LicenceDatabaseLookups.LicEnterpriseContacts",
					MasterDatabase.LicEnterprise != null ? string.Concat(".", MasterDatabase.LicEnterprise.LE_EnterpriseCode) : string.Empty), () =>
				{
					OrgContactCollection result;
					if (MasterDatabase.LicEnterprise != null)
					{
						result = new OrgContactCollection(new BusinessObjectFactory(), AllContactsQuery); // Possible dirty table will barf the ZDBOnlyQuery
					}
					else
					{
						result = new OrgContactCollection(Factory);
					}
					return result;
				});

				return licEnterpriseContacts;
			}
		}

		#endregion

		#region Web Access Orgs

		public void RefreshWebAccessOrgs()
		{
			Factory.ClearCachedValue<OrgHeaderCollection>("LicenceDatabaseLookups.WebAccessOrgs" + MasterDatabase.PK);
		}

		public override OrgHeaderCollection WebAccessOrgs
		{
			get
			{
				return Factory.GetCachedValue("LicenceDatabaseLookups.WebAccessOrgs" + MasterDatabase.PK,
					() =>
					{
						var query = new ZQuery(OrgHeaderSchema.OH_IsActive, true);

						var defaultEnterpriseID = EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.Value;

						if (Parent.IsInDatabase &&
							(string.IsNullOrEmpty(defaultEnterpriseID) || MasterDatabase.EnterpriseID != defaultEnterpriseID))
						{
							var validOrgPks = MasterDatabase.LicEnterprise.Companies.Cast<LicenceCompany>().Select(c => c.LC_OH);
							query.AddToFilter(OrgHeaderSchema.PK, validOrgPks);
						}

						return new OrgHeaderCollection(Factory, query);
					});
			}
		}

		#endregion

		#region Upgrade Methods

		public UpgradeMethods UpgradeMethodsList
		{
			get { return new UpgradeMethods(); }
		}

		#endregion

		#region ReleaseRings

		public CodeDescriptionPairList ReleaseRings => ReleaseRingsCache(Factory);

		public static CodeDescriptionPairList BuildReleaseRings()
		{
			ReleaseRingsList ringList = new ReleaseRingsList();
			CodeDescriptionPairList releaseRings = new CodeDescriptionPairList();
			foreach (string ring in Enterprise.Client.EDI.ReleaseBuilds.Business.ReleaseRingsLookup.CheckInRingCodes)
			{
				releaseRings.AddPair(ring, ringList.GetDescriptionFromCode(ring));
			}
			return releaseRings;
		}

		public static CodeDescriptionPairList ReleaseRingsCache(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ReleaseRings", () => BuildReleaseRings());
		}

		#endregion

		#region SQL Server Editions

		public SqlServerEditionList SqlServerEditions
		{
			get { return new SqlServerEditionList(); }
		}

		#endregion

		#region Hosted Locations

		public CodeDescriptionPairList HostedLocations
		{
			get
			{
				CodeDescriptionPairList hostedLocations = new CodeDescriptionPairList();
				hostedLocations.AddRange(EDIDataRegistry.Instance.DatabaseHostedLocations.Value);
				return hostedLocations;
			}
		}

		#endregion

		#region Registration Statuses

		public CodeDescriptionPairList StatusList
		{
			get { return new DatabaseStatusList(); }
		}

		#endregion

		#region Parent Production Database

		public static CodeDescriptionPairList BuildDatabaseCategoryCodeDescriptionList(IEnumerable<LicenceDatabase> databases)
		{
			var result = new CodeDescriptionPairList();

			var databaseTypeList = new DatabaseTypes();
			foreach (var databaseTypeGroup in databases
				.GroupBy(x => x.LD_LicenceType)
				.OrderBy(x => x.First().LD_LicenceType == DatabaseTypes.Codes.Production ? 0 : 1)
				.ThenBy(x => x.Key))
			{
				result.AddPair("");
				result.Add(new CategoryCodeDescriptionPair(databaseTypeList.GetDescriptionFromCode(databaseTypeGroup.Key), ""));
				foreach (var database in databaseTypeGroup.OrderBy(x => x.LD_ServerCode))
				{
					result.AddPair(database.LD_ServerCode, GetDatabaseDescription(database));
				}
			}

			return result;
		}

		public static ZString GetDatabaseDescription(LicenceDatabase database)
		{
			var databaseDescription = database.ReleaseRingDescription;
			if (database.CurrentVersion != null)
			{
				databaseDescription += " - " + database.CurrentVersion.VersionNumber.ToString() + " - " + database.CurrentVersion.HL_ExeVersionDate.ToShortDateString();
			}
			return databaseDescription;
		}

		public CodeDescriptionPairList ProductionDatabaseCodeDescriptionPairList
		{
			get
			{
				var db = (LicenceDatabase)Parent;
				if (db.LD_LicenceType != DatabaseTypes.Codes.Production && db.LicEnterprise != null)
				{
					return Factory.GetCachedValue("LicenceDatabaseLookups.ProductionDatabaseCodeDescriptionPairList." + db.LD_LE,
						() =>
						{
							return BuildDatabaseCategoryCodeDescriptionList(db.LicEnterprise.Databases
								.Cast<LicenceDatabase>()
								.Where(x => x.LD_LicenceType == DatabaseTypes.Codes.Production && x.LD_IsActive));
						}, CacheStalenessPolicy.StaleOnFactorySave);
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		#endregion

		#region Billing Type

		public CodeDescriptionPairList BillingModels
		{
			get
			{
				if (MasterDatabase.IsEnterpriseFamilyDatabase)
				{
					return BillingConstants.BillingModel.BillingModelList;
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		#endregion

		#region BillableFlagList

		public CodeDescriptionPairList BillableFlagList => Factory.GetCachedValue("LicenceDatabaseLookups_BillableFlagList", () => new DatabaseBillableFlagList());

		#endregion

		#region Telematics Devices

		public ClientDeviceHeaderCollection TelematicsDevices => new ClientDeviceHeaderCollection(Factory);

		#endregion

		#region Multi Tenant

		public ReadOnlyCodeDescriptionPairList MultiTenantDatabaseProductTypes => new MultiTenantDatabaseProductTypeList();

		#endregion

		#region TrustedSystems

		public EdiTrustedSystemCollection TrustedSystems => Factory.GetCachedValue("LicenceDatabaseLookups.TrustedSystems", () => new EdiTrustedSystemCollection(Factory));

		#endregion

		#region Feature Sets

		public FeatureControlSetCollection FeatureSets
		{
			get
			{
				if (featureSets == null)
				{
					var collection = new FeatureControlSetCollection(Factory);
					featureSets = collection;
				}
				return featureSets;
			}
		}
		FeatureControlSetCollection featureSets;

		#endregion

		#region Implementation

		ZDBOnlyQuery AllAddressesQuery
		{
			get { return CreateDBOnlyQueryForEnterpriseCode(typeof(OrgAddress), OrgAddressSchema.OA_OH); }
		}

		ZDBOnlyQuery AllContactsQuery
		{
			get { return CreateDBOnlyQueryForEnterpriseCode(typeof(OrgContact), OrgContactSchema.OC_OH); }
		}

		ZDBOnlyQuery CreateDBOnlyQueryForEnterpriseCode(Type typeToQuery, SchemaColumn foreignKeyColumn)
		{
			ZString sQLText = string.Format(CultureInfo.InvariantCulture,
												@"{0} in (
													select OH_PK from dbo.OrgHeader 
													where OH_PK IN (
														select LC_OH from dbo.LicenceCompany
														where LC_LE in (
															select LE_PK from dbo.LicenceEnterprise where LE_EnterpriseCode <> '' and LE_EnterpriseCode = @EnterpriseCode
														)
													)													
												)",
												foreignKeyColumn.Name);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@EnterpriseCode", MasterDatabase.LicEnterprise.LE_EnterpriseCode, LicenceEnterpriseSchema.LE_EnterpriseCode);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeToQuery);
			result.AddFilterAndZSQLParameterCollection(sQLText, @params);
			return result;
		}

		#endregion
	}
}

