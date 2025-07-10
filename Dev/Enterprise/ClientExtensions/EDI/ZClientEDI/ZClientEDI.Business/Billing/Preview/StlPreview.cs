using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.Progress;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlPreview : NonPersistentBusinessObject, IObsoleteValidation
	{
		public StlPreview(BusinessObjectFactory factory) : base(factory)
		{
			factory.Saving += BillFactory_Saving;
		}

		public void SetDefaults()
		{
			var stlPriceHeaders = StlPriceHeaderVersions;
			StlPriceHeaderVersion = stlPriceHeaders.Count > 0 ? stlPriceHeaders[0].Code : "";
			CurrencyCode = "";
		}

		#region Properties

		#region OrganisationPK

		[List("Organisations")]
		public ZGuid OrganisationPK
		{
			get { return organisationPK; }
			set
			{
				SetNonPersistentPropertyValue(OrganisationPKInfo, ref organisationPK, value);

				if (!IsValidationSuspended)
				{
					ValidateOrganisationPK();
				}

				OnOrgChanged();
			}
		}
		ZGuid organisationPK;

		public ZPropertyInfo OrganisationPKInfo
		{
			get { return this.GetZPropertyInfo(nameof(OrganisationPK)); }
		}

		EDIOrgHeader Organisation
		{
			get { return Factory.Load<EDIOrgHeader>(OrganisationPK); }
		}

		public void ValidateOrganisationPK()
		{
			OrganisationPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(OrganisationPKInfo);
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		void OnOrgChanged()
		{
			var org = Organisation;
			if (org == null)
			{
				return;
			}

			var licCompany = org.LicCompany;
			if (licCompany == null)
			{
				return;
			}

			EnterprisePK = licCompany.LC_LE;
		}

		#endregion

		#region Enterprise

		[List("EnterpriseList")]
		public ZGuid EnterprisePK
		{
			get { return enterprisePK; }
			set
			{
				if (enterprisePK != value)
				{
					SetNonPersistentPropertyValue(EnterprisePKInfo, ref enterprisePK, value);
					if (!IsValidationSuspended)
					{
						ValidateEnterprisePK();
					}

					OnEnterprisePkChanged();
				}
			}
		}
		ZGuid enterprisePK;

		public ZPropertyInfo EnterprisePKInfo
		{
			get { return GetZPropertyInfo(nameof(EnterprisePK)); }
		}

		public void ValidateEnterprisePK()
		{
			EnterprisePKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(EnterprisePKInfo);
			ListValidation.ErrorIfInvalidPK(EnterprisePKInfo);
		}

		LicenceEnterprise Enterprise
		{
			get { return Factory.Load<LicenceEnterprise>(EnterprisePK); }
		}

		public LicenceEnterpriseCollectionForEntCodeFilter EnterpriseList
		{
			get
			{
				return Factory.GetCachedValue("StlPreview.EnterpriseList",
					() =>
					{
						return new LicenceEnterpriseCollectionForEntCodeFilter(Factory);
					});
			}
		}

		void OnEnterprisePkChanged()
		{
			var org = Organisation;

			if (EnterprisePK.IsEmpty)
			{
				if (org == null)
				{
					DatabaseCode = "";
				}
			}
			else
			{
				if (org != null && org.LicCompany != null && org.LicCompany.LC_LE != EnterprisePK)
				{
					OrganisationPK = ZGuid.Empty;
				}
			}

			var list = DatabaseCodeDescriptionPairList;
			if (list.Count == 1)
			{
				DatabaseCode = list[0].Code;
			}
			else
			{
				DatabaseCode = "";
			}
		}

		#endregion

		#region Database

		[List("DatabaseCodeDescriptionPairList")]
		public ZString DatabaseCode
		{
			get { return databaseCode; }
			set
			{
				if (databaseCode != value)
				{
					SetNonPersistentPropertyValue(DatabaseCodeInfo, ref databaseCode, value);

					if (!IsValidationSuspended)
					{
						ValidateDatabaseCode();
					}

					OnDatabaseChanged();
				}
			}
		}
		ZString databaseCode;

		public ZPropertyInfo DatabaseCodeInfo
		{
			get { return GetZPropertyInfo(nameof(DatabaseCode)); }
		}

		public CodeDescriptionPairList DatabaseCodeDescriptionPairList
		{
			get
			{
				return BuildDatabaseCodeDescriptionList();
			}
		}

		IEnumerable<LicenceDatabase> GetDatabaseLookupList()
		{
			var org = Organisation;
			if (org != null)
			{
				var licCompany = org.LicCompany;
				if (licCompany != null)
				{
					return licCompany.LicHeadersForAllDatabases.Cast<LicenceHeader>()
						.Where(x => x.Database.LD_LicenceType == DatabaseTypes.Codes.Production
								&& x.Database.IsEnterpriseFamilyDatabase
								&& x.Database.LD_IsActive
								&& x.LA_IsActive)
						.Select(x => x.Database)
						.OrderBy(x => x.LD_ServerCode);
				}
			}

			var enterprise = Enterprise;
			if (enterprise != null)
			{
				var query = new ZQuery(LicenceDatabaseSchema.LD_LE, enterprise.PK);
				query.AddToFilter(LicenceDatabaseSchema.LD_IsActive, true);
				query.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, DatabaseTypes.Codes.Production);
				query.AddToFilter(LicenceDatabaseSchema.LD_Product, new string[] { ProductTypes.Codes.Enterprise, ProductTypes.Codes.CargoWiseOne, ProductTypes.Codes.CargoWiseNext, ProductTypes.Codes.CargoWise, ProductTypes.Codes.ProductivityWise });
				query.OrderBy = LicenceDatabase.Schema.LD_ServerCode;
				return Factory.Load<LicenceDatabase>(query);
			}

			return Enumerable.Empty<LicenceDatabase>();
		}

		CodeDescriptionPairList BuildDatabaseCodeDescriptionList()
		{
			var result = new CodeDescriptionPairList();

			foreach (var database in GetDatabaseLookupList())
			{
				var databaseDescription = database.ReleaseRingDescription;
				if (database.CurrentVersion != null)
				{
					databaseDescription += " - " + database.CurrentVersion.VersionNumber.ToString() + " - " + database.CurrentVersion.HL_ExeVersionDate.ToShortDateString();
				}
				result.AddPair(database.LD_ServerCode, databaseDescription);
			}

			return result;
		}

		public void ValidateDatabaseCode()
		{
			DatabaseCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(DatabaseCodeInfo);
		}

		void OnDatabaseChanged()
		{
			var db = SelectedDatabase;
			if (db != null)
			{
				if (!db.LD_OH_BillingParty.IsEmpty)
				{
					OrganisationPK = db.LD_OH_BillingParty;
				}
				else if (OrganisationPK.IsEmpty)
				{
					var owner = db.UsageOwnerOrFirstLicence;
					OrganisationPK = owner.Company.LC_OH;
				}
			}
		}

		LicenceDatabase SelectedDatabase
		{
			get
			{
				return GetDatabaseLookupList().FirstOrDefault(x => x.LD_ServerCode == DatabaseCode);
			}
		}

		LicenceHeader Licence
		{
			get
			{
				var db = SelectedDatabase;
				var org = Factory.Load<EDIOrgHeader>(OrganisationPK);
				if (db != null)
				{
					if (org != null)
					{
						return db.LicHeadersForAllCompanies.Cast<LicenceHeader>().FirstOrDefault(x => x.Company.LC_OH == OrganisationPK);
					}
					else
					{
						return db.UsageOwnerOrFirstLicence;
					}
				}

				return null;
			}
		}

		#endregion

		#region Currency

		[List("Currencies")]
		[MaxLength(3)]
		public ZString CurrencyCode
		{
			get { return currency; }
			set
			{
				value = value.TrimEndSpaceTab();
				if (currency != value)
				{
					CheckMaximumLength(CurrencyCodeInfo, value);
					SetNonPersistentPropertyValue(CurrencyCodeInfo, ref currency, value);

					if (!IsValidationSuspended)
					{
						ValidateCurrencyCode();
					}

					OnCurrencyChanged();
				}
			}
		}
		ZString currency;

		public ZPropertyInfo CurrencyCodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(CurrencyCode)); }
		}

		public void ValidateCurrencyCode()
		{
			CurrencyCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(CurrencyCodeInfo);
		}

		public RefCurrencyCollection Currencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		void OnCurrencyChanged()
		{
			var currentOdpl = odplPriceHeaderVersion;
			if (!string.IsNullOrEmpty(currentOdpl))
			{
				UpdateOdplPriceHeaderPk(currentOdpl);
				if (OdplPriceHeaderPk.IsEmpty)
				{
					OdplPriceHeaderVersion = "";
				}
			}
		}

		#endregion

		#region STL Price Header

		public ClientLicencePriceHeader StlPriceHeader
		{
			get { return Factory.Load<ClientLicencePriceHeader>(StlPriceHeaderPk); }
		}

		public ZGuid StlPriceHeaderPk { get; set; }

		[List("StlPriceHeaderVersions"), MaxLength(ClientLicencePriceHeader.Schema.L6_PricelistVersionMaxLength)]
		public ZString StlPriceHeaderVersion
		{
			get
			{
				return stlPriceHeaderVersion ?? (stlPriceHeaderVersion = StlPriceHeader != null ? StlPriceHeader.L6_PricelistVersion : null);
			}
			set
			{
				if (stlPriceHeaderVersion != value)
				{
					var available = EdiPriceHeaderLinkLookups.GetPriceHeaderVersionMap(Factory);
					ClientLicencePriceHeader match;
					if (available.TryGetValue(value, out match))
					{
						StlPriceHeaderPk = match.PK;
					}

					CheckMaximumLength(StlPriceHeaderVersionInfo, value);
					stlPriceHeaderVersion = value;
					LoadStlDiscountsFromPricelist();
					StlPriceHeaderVersionInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						ValidateStlPriceHeader();
					}
				}
			}
		}

		string stlPriceHeaderVersion;

		public ZPropertyInfo StlPriceHeaderVersionInfo
		{
			get { return GetZPropertyInfo(nameof(StlPriceHeaderVersion)); }
		}

		public CodeDescriptionPairList StlPriceHeaderVersions
		{
			get
			{
				return Factory.GetCachedValue("StlPreview.StlPriceHeaderVersions", () =>
				{
					var map = EdiPriceHeaderLinkLookups.GetPriceHeaderVersionMap(Factory);
					var result = new CodeDescriptionPairList();
					foreach (var pair in map.OrderByDescending(x => x.Value.L6_SystemCreateTimeUtc).ThenBy(x => x.Key))
					{
						result.AddPair(pair.Key, pair.Value.L6_PricelistVersion);
					}
					return result;
				});
			}
		}

		void ValidateStlPriceHeader()
		{
			StlPriceHeaderVersionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(StlPriceHeaderVersionInfo);
			ListValidation.ErrorIfInvalidCode(StlPriceHeaderVersionInfo);
		}

		#endregion

		#region ODPL Price Header

		public ClientLicencePriceHeader OdplPriceHeader
		{
			get { return Factory.Load<ClientLicencePriceHeader>(OdplPriceHeaderPk); }
		}

		public ZGuid OdplPriceHeaderPk { get; set; }

		[List("OdplPriceHeaderVersions"), MaxLength(ClientLicencePriceHeader.Schema.L6_PricelistVersionMaxLength)]
		public ZString OdplPriceHeaderVersion
		{
			get
			{
				return odplPriceHeaderVersion ?? (odplPriceHeaderVersion = OdplPriceHeader != null ? OdplPriceHeader.L6_PricelistVersion : null);
			}
			set
			{
				if (odplPriceHeaderVersion != value)
				{
					UpdateOdplPriceHeaderPk(value);
					CheckMaximumLength(OdplPriceHeaderVersionInfo, value);
					odplPriceHeaderVersion = value;
					OdplPriceHeaderVersionInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						ValidateOdplPriceHeader();
					}
				}
			}
		}

		void UpdateOdplPriceHeaderPk(string priceHeaderVersion)
		{
			var available = CachedOdplPriceHeaderVersionMap;
			ClientLicencePriceHeader match;
			if (available.TryGetValue(priceHeaderVersion, out match))
			{
				OdplPriceHeaderPk = match.PK;
			}
			else
			{
				OdplPriceHeaderPk = ZGuid.Empty;
			}
		}

		string odplPriceHeaderVersion;

		public ZPropertyInfo OdplPriceHeaderVersionInfo
		{
			get { return GetZPropertyInfo(nameof(OdplPriceHeaderVersion)); }
		}

		public CodeDescriptionPairList OdplPriceHeaderVersions
		{
			get
			{
				var map = CachedOdplPriceHeaderVersionMap;
				var result = new CodeDescriptionPairList();
				foreach (var pair in map.OrderByDescending(x => x.Value.L6_SystemCreateTimeUtc).ThenBy(x => x.Key))
				{
					result.AddPair(pair.Key, pair.Value.L6_PricelistVersion);
				}
				return result;
			}
		}

		public Dictionary<string, ClientLicencePriceHeader> CachedOdplPriceHeaderVersionMap
		{
			get
			{
				if (!CurrencyCodeInfo.HasErrors())
				{
					return GetCachedOdplPriceHeaderVersionMap(Factory, CurrencyCode.IsEmpty ? (ZString)"USD" : CurrencyCode);
				}
				else
				{
					return new Dictionary<string, ClientLicencePriceHeader>(0);
				}
			}
		}

		public static Dictionary<string, ClientLicencePriceHeader> GetCachedOdplPriceHeaderVersionMap(BusinessObjectFactory factory, string currency)
		{
			return factory.GetCachedValue("StlPreview.OdplPriceHeaderVersions:" + currency, () =>
			{
				return GetOdplPriceHeaderVersionMap(factory, currency);
			});
		}

		public static Dictionary<string, ClientLicencePriceHeader> GetOdplPriceHeaderVersionMap(BusinessObjectFactory factory, string currency)
		{
			var stdPrices = LicenceCompany.GetStandardPriceHeaders(factory);

			if (stdPrices != null)
			{
				var allOdpl = stdPrices.Where(x =>
						x.L6_SystemCode == BillingConstants.BillingSystem.ODM &&
						x.L6_RN_NKCountry.IsEmpty &&
						!x.L6_PricelistVersion.IsEmpty &&
						x.L6_RX_NKCurrency == currency);

				return allOdpl.GroupBy(x => x.L6_PricelistVersion)
					.ToDictionary(x => x.Key.ToString(), y => y.OrderByDescending(z => z.L6_SystemCreateTimeUtc).First());
			}
			else
			{
				return new Dictionary<string, ClientLicencePriceHeader>(0);
			}
		}

		void ValidateOdplPriceHeader()
		{
			OdplPriceHeaderVersionInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(OdplPriceHeaderVersionInfo);
		}

		#endregion

		#region STL Discounts

		public StlDiscountPreviewCollection StlDiscounts
		{
			get
			{
				if (stlDiscounts == null)
				{
					stlDiscounts = new StlDiscountPreviewCollection();
					RegisterEditableChildObject(stlDiscounts);
				}

				return stlDiscounts;
			}
		}

		StlDiscountPreviewCollection stlDiscounts;
		string stlDiscountVersion;

		void LoadStlDiscountsFromPricelist()
		{
			var priceHeader = StlPriceHeader;
			if (priceHeader != null && priceHeader.L6_DiscountCode != stlDiscountVersion)
			{
				stlDiscountVersion = priceHeader.L6_DiscountCode;

				StlDiscounts.RemoveAndDeleteAll();

				foreach (var headerDiscount in priceHeader.StlDiscounts)
				{
					var discountPreview = StlDiscounts.AddNew();
					discountPreview.Name = headerDiscount.PHD_Name;
					discountPreview.IsActive = headerDiscount.PHD_IsDefaultEnabled;
					discountPreview.Percent = headerDiscount.PHD_Percent;
					discountPreview.Percent_ReadOnly = !BillingConstants.DiscountCalculator.IsCustomerPercentageSettingApplicable(headerDiscount.PHD_Type);
				}
			}
		}

		#endregion

		#region Uplift Settings

		public EdiPriceHeaderLinkCollectionForPreview PriceHeaderLinks
		{
			get
			{
				if (priceHeaderLinks == null)
				{
					priceHeaderLinks = new EdiPriceHeaderLinkCollectionForPreview(Factory);
					priceHeaderLinks.AddNew();
				}

				return priceHeaderLinks;
			}
		}

		EdiPriceHeaderLinkCollectionForPreview priceHeaderLinks;

		#endregion

		ZDateTime GenerateDate
		{
			get
			{
				if (generateDate.IsEmpty)
				{
					generateDate = ZDateTime.Today;
				}
				return generateDate;
			}
			set { generateDate = value; }
		}
		ZDateTime generateDate;

		protected override void RunPreSaveValidationCore()
		{
			ValidateOrganisationPK();
			ValidateEnterprisePK();
			ValidateDatabaseCode();
			ValidateCurrencyCode();
			ValidateStlPriceHeader();
			ValidateOdplPriceHeader();

			base.RunPreSaveValidationCore();
		}

		#endregion

		#region Generate Report

		const int MaxLicenceHeadersPerFactory = 50;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Scope = "member", Target = "Enterprise.Client.EDI.Billing.Business.StlPreview.#GenerateReport(IEdiProgress)")]
		public void GenerateReport(IEdiProgress progress)
		{
			StlPreviewLines.RemoveAll();
			GenerateDate = ZDateTime.Today;
			var previewContext = new StlPreviewContext(GenerateDate, StlPriceHeaderPk, OdplPriceHeaderPk, CurrencyCode, Enterprise?.LE_EnterpriseCode ?? ZString.Empty, StlDiscounts.ToArray<StlDiscountPreview>(), PriceHeaderLinks.OfType<EdiPriceHeaderLink>().Single());

			var today = ZDateTime.Today;
			var lastMonthPeriodStart = ZDateTime.Today.AddDays(1 - today.Day).AddMonths(-1);

			var databaseOwners = StlPreviewLicenceDatabaseOwner.GenerateDatabaseOwners(lastMonthPeriodStart.AddMonths(-2), Licence, GetDatabaseLookupList());
			var databaseOwnerGroups = databaseOwners.Select((pk, index) => new { Pk = pk, Index = index }).GroupBy(g => g.Index / MaxLicenceHeadersPerFactory, pk => pk.Pk);

			var progressTotal = databaseOwners.Length * 3;
			var progressCounter = 0;

			foreach (var group in databaseOwnerGroups)
			{
				var billFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				billFactory.Saving += BillFactory_Saving;

				foreach (var databaseOwner in group)
				{
					var periodStart = lastMonthPeriodStart;
					for (int i = 0; i < 3; i++)
					{
						progressCounter++;
						progress.SafeSetStatusAndPercentComplete(ZString.Format("Generating... {0}/{1}", progressCounter, progressTotal), progressCounter * 100 / progressTotal);

						StlPreviewLines.Add(new StlPreviewLine(billFactory, databaseOwner, periodStart, previewContext));
						periodStart = periodStart.AddMonths(-1);

						if (progress.SafeIsCancelled())
						{
							return;
						}
					}
				}

				GC.Collect(); // Calling only every 50 records
			}
		}

		#endregion

		#region Misc.

		static void BillFactory_Saving(BusinessObjectFactory factory)
		{
			throw new InvalidOperationException("Attempt to save the temporary billing preview factory.");
		}

		public StlPreviewLineCollection StlPreviewLines
		{
			get { return stlPreviewLines ?? (stlPreviewLines = new StlPreviewLineCollection()); }
		}
		StlPreviewLineCollection stlPreviewLines;

		#endregion 
	}

	public class StlPreviewContext
	{
		public StlPreviewContext(ZDateTime generateDate,
									ZGuid stlPriceHeaderPk,
									ZGuid odplPriceHeaderPk,
									ZString currencyCode,
									ZString enterpriseCode,
									IEnumerable<StlDiscountPreview> stlDiscounts,
									EdiPriceHeaderLink priceHeaderLink = null)
		{
			GenerateDate = generateDate;
			StlPriceHeaderPk = stlPriceHeaderPk;
			OdplPriceHeaderPk = odplPriceHeaderPk;
			CurrencyCode = currencyCode;
			EnterpriseCode = enterpriseCode;
			StlDiscounts = stlDiscounts.Select(x => x.Clone()).ToArray();
			StlPreviewContextPk = ZGuid.NewZGuid();
			PriceHeaderLink = priceHeaderLink;
		}

		public readonly ZDateTime GenerateDate;
		public readonly ZGuid StlPriceHeaderPk;
		public readonly ZGuid OdplPriceHeaderPk;
		public readonly ZString CurrencyCode;
		public readonly ZString EnterpriseCode;
		public readonly IEnumerable<StlDiscountPreview> StlDiscounts;
		public readonly ZGuid StlPreviewContextPk;
		public readonly EdiPriceHeaderLink PriceHeaderLink;
	}

	public class StlPreviewLicenceDatabaseOwner
	{
		public static StlPreviewLicenceDatabaseOwner[] GenerateDatabaseOwners(ZDateTime periodStart, LicenceHeader licenceHeader, IEnumerable<LicenceDatabase> licenceDatabases)
		{
			if (licenceHeader != null)
			{
				return new StlPreviewLicenceDatabaseOwner[] { new StlPreviewLicenceDatabaseOwner(licenceHeader) };
			}
			else
			{
				var sql = string.Empty;

				if (!licenceDatabases.Any())
				{
					sql = ZString.Format(@"
SELECT DbOwner.LD_PK, DbOwner.LA_PK
FROM dbo.EdiViewLicenceDatabaseOwner DbOwner
JOIN dbo.LicenceHeader LA ON LA.LA_PK = DbOwner.LA_PK
JOIN 
(
	SELECT DISTINCT U1_LD LD_PK FROM dbo.ClientChargeableUsage  WHERE U1_PeriodStart >= '{0}'
) Usage ON Usage.LD_PK = DbOwner.LD_PK
JOIN dbo.OrgHeader ON OH_PK = DbOwner.LC_OH
JOIN dbo.LicenceDatabase LD ON LD.LD_PK = DbOwner.LD_PK
WHERE LA_LicenceAdvStdOth = 'ODM' ORDER BY OH_Code, DbOwner.LA_PK, LD.LD_ServerCode;", periodStart.SqlFormat);
				}
				else
				{
					sql = ZString.Format(@"
SELECT LD_PK, LA_PK
FROM dbo.EdiViewLicenceDatabaseOwner
WHERE LD_PK IN ({1});", periodStart.SqlFormat, string.Join(",", licenceDatabases.Select(x => "'" + x.PK + "'")));
				}

				return Utilities.GetDataTableFromQuery(sql).Rows.OfType<DataRow>().Select(x => new StlPreviewLicenceDatabaseOwner(x)).ToArray();
			}
		}

		StlPreviewLicenceDatabaseOwner(DataRow row)
		{
			LD_PK = (Guid)row[LicenceDatabaseSchema.Constants.PK];
			LA_PK = (Guid)row[LicenceHeaderSchema.Constants.PK];
		}

		public StlPreviewLicenceDatabaseOwner(LicenceHeader licenceHeader)
		{
			LD_PK = licenceHeader.LA_LD;
			LA_PK = licenceHeader.PK;
		}

		public readonly ZGuid PK = ZGuid.NewZGuid();
		public readonly ZGuid LD_PK;
		public readonly ZGuid LA_PK;
	}

	public class EdiPriceHeaderLinkCollectionForPreview : BusinessObjectCollection<EdiPriceHeaderLink>
	{
		public EdiPriceHeaderLinkCollectionForPreview(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowRemoveCore => false;
		protected override bool AllowNewCore => false;
	}
}

