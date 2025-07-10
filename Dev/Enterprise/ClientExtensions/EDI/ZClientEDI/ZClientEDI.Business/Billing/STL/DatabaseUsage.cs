using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// Interface to LicenceDatabase fields used in Billing
	/// </summary>
	public interface IBilledDatabase
	{
		ZGuid PK { get; }
		ZString LD_ServerCode { get; }
		ZInt LD_DatabaseNumber { get; }
		ZGuid LD_LE { get; }
		ZBool LD_IsBilledPerCompany { get; }
		ZGuid LD_LD_ParentDatabase { get; }
		ZString LD_HostedLocation { get; }
		ZBool IsHostedOnWiseCloud { get; }
		ZString EnterpriseCode { get; }
		ZString LD_LicenceType { get; }
		ZString LD_Product { get; }
		ZString LD_Billable { get; }
		bool IsEnterpriseFamilyDatabase { get; }
		bool IsBillable { get; }
		ZGuid LD_OH_WebAccessOrg { get; }
	}

	/// <summary>
	/// A production database, plus all its usage being billed for a period, and all the prices and settings for that period.
	/// Usage can include usage from a non-production database which has this database as it's parent.
	/// Usage may end up on multiple invoices if the the billing is per company and there are multiple companies.
	/// Usage may be for a previous period if late usage is being accumulated.
	/// </summary>
	public class DatabaseUsage
	{
		public DatabaseUsage(UsageOwnerDelivery dbOwner,
			Usage[] usages,
			string priceCurrency,
			PriceList stlPriceList,
			EdiLicenceSetting[] settings,
			ZDateTime siteLive,
			bool isLive,
			Dictionary<string, PriceList> additionalPriceLists,
			ClientCompany[] clientCompanies = null,
			EdiPriceHeaderLink priceHeaderLink = null)
		{
			this.dbOwner = dbOwner;
			this.usages = usages;
			this.priceCurrency = priceCurrency ?? "";

			this.stlPriceList = stlPriceList;
			this.additionalPriceLists = additionalPriceLists;

			this.settings = settings;
			this.siteLiveDate = siteLive;
			this.isLive = isLive;

			if (settings != null)
			{
				ConversionCredit = (ConversionCreditLicenceSetting)settings.Where(x => x.LS9_Type == BillingConstants.LicenceSetting.ConversionCredit).FirstOrDefault();
				Commitment = (CommitmentLicenceSetting)settings.Where(x => x.LS9_Type == BillingConstants.LicenceSetting.Commitment).FirstOrDefault();
			}

			countryUsers = new DatabaseCountryUsers(clientCompanies, usages.Where(x => x.ChargeableUsage != null).Select(x => x.ChargeableUsage));
			PriceHeaderLink = priceHeaderLink;
		}

		readonly Usage[] usages;
		readonly UsageOwnerDelivery dbOwner;
		readonly string priceCurrency;
		readonly PriceList stlPriceList;
		readonly Dictionary<string, PriceList> additionalPriceLists;
		readonly EdiLicenceSetting[] settings;
		readonly ZDateTime siteLiveDate;
		readonly bool isLive;
		readonly DatabaseCountryUsers countryUsers;
		public EdiPriceHeaderLink PriceHeaderLink { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal use only, not client visible, ignored.")]
		public void AddValidationNotifications(BusinessObject notificationOwner)
		{
			if (Database != null)
			{
				bool hasAnyStlUsage = usages.Any(x => x.Code == BillingConstants.BillingSystem.STL);
				if (hasAnyStlUsage && StlPriceList == null)
				{
					notificationOwner.AddRowError((NoResString)"Database " + Database.LD_ServerCode + (NoResString)" is missing an STL pricelist");
				}

				if (StlPriceList == null
					&& Database.LD_Product != ProductTypes.Codes.BorderWise
					&& !PriceHeaderType.IsGlobal(Database.LD_Product)
					&& usages.Any(x => x.Code == Database.LD_Product))
				{
					notificationOwner.AddRowError($"Database {Database.LD_ServerCode} is missing the {Database.LD_Product} pricelist");
				}

				if (hasAnyStlUsage && ActiveUserCount == 0 && RoboticUserCount == 0)
				{
					notificationOwner.AddRowError((NoResString)"Database " + Database.LD_ServerCode + (NoResString)" is missing STL Active User and Robotic User data");
				}
				else
				{
					var milestoneUsage = usages.Where(x => x.Code == BillingConstants.BillingSystem.STL
								&& x.SubCode == MilestoneUsageCode
								&& x.PeriodStart >= new ZDateTime(2016, 11, 1)
								&& x.UnitCount > 0)
							.OrderBy(x => x.UnitCount)
							.FirstOrDefault();

					if (milestoneUsage != null && milestoneUsage.UnitCount > 1)
					{
						string msg = (NoResString)"Database " + Database.LD_ServerCode + (NoResString)" is missing some STL data";
						if (milestoneUsage.PeriodStart == new ZDateTime(2016, 11, 1))
						{
							notificationOwner.AddRowWarning(msg);
						}
						else
						{
							notificationOwner.AddRowError(msg);
						}
					}
				}

				if (Database.IsHostedOnWiseCloud)
				{
					if (!usages.Any(x => x.Code == BillingConstants.BillingSystem.HostingStorage))
					{
						notificationOwner.AddRowWarning("Database " + Database.LD_ServerCode + " is missing WiseCloud storage data.");
					}
				}
			}
		}

		public IBilledDatabase Database => dbOwner.Database;
		public IEnumerable<Usage> Usages => usages;
		public UsageOwnerDelivery OwnerDelivery => dbOwner;
		public PriceList StlPriceList => stlPriceList;
		public PriceList GetPriceList(string priceHeaderType) => priceHeaderType == BillingConstants.PriceHeaderType.STL ? stlPriceList : TryGetAdditionalPriceList(priceHeaderType);
		public string PriceCurrency => priceCurrency;
		public ConversionCreditLicenceSetting ConversionCredit { get; private set; }
		public CommitmentLicenceSetting Commitment { get; private set; }
		public DatabaseCountryUsers CountryUsers => countryUsers;

		public bool IsSiteLive { get { return isLive; } }
		public ZDateTime SiteLiveDate { get { return siteLiveDate; } }

		public IEnumerable<PriceList> AdditionalPriceLists => additionalPriceLists.Values.Where(x => x != null);

		PriceList TryGetAdditionalPriceList(string priceHeaderType)
		{
			return additionalPriceLists.TryGetValue(priceHeaderType, out var p) ? p : null;
		}

		public BuyingGroupLicenceSetting GetBuyingGroupSetting()
		{
			return settings != null ? (BuyingGroupLicenceSetting)settings.FirstOrDefault(x => x.LS9_Type == BillingConstants.LicenceSetting.BuyingGroup) : null;
		}

		public static DiscountLicenceSetting GetDiscountSetting(IEnumerable<EdiLicenceSetting> settings, string name)
		{
			return settings != null ? (DiscountLicenceSetting)settings.FirstOrDefault(x => x.LS9_Type == BillingConstants.LicenceSetting.Discount && x.LS9_Name.EqualsIgnoringCase(name)) : null;
		}

		public DiscountLicenceSetting GetDiscountSetting(string name)
		{
			return GetDiscountSetting(settings, name);
		}

		public bool HasManualPrepaymentDiscountSetting
		{
			get
			{
				var prepayHeaderDiscount = stlPriceList?.Discounts?.Discounts.FirstOrDefault(x => x.PHD_Type == BillingConstants.DiscountCalculator.Prepayment);
				if (prepayHeaderDiscount != null)
				{
					var prepaySetting = GetDiscountSetting(prepayHeaderDiscount.PHD_Name);
					return prepaySetting?.LS9_IsManualOverride ?? false;
				}
				return false;
			}
		}

		public PriceLicenceSetting GetCustomPriceSetting(UsageCodeKey priceKey)
		{
			if (settings != null)
			{
				return settings.Where(x => x.LS9_Type == BillingConstants.LicenceSetting.Price || x.LS9_Type == BillingConstants.LicenceSetting.PriceTier)
					.Cast<PriceLicenceSetting>()
					.FirstOrDefault(x => x.PriceKey.EqualsIgnoringCase(priceKey));
			}
			else
			{
				return null;
			}
		}

		public bool HasMinSpendSetting
			=> (hasMinSpendSettingsLazy ?? (hasMinSpendSettingsLazy = settings?.Any(x => x.LS9_Type == BillingConstants.LicenceSetting.MinSpend) ?? false)).Value;
		bool? hasMinSpendSettingsLazy;

		public IEnumerable<MinSpendLicenceSetting> GetMinSpendSettings()
		{
			if (settings != null)
			{
				return settings.Where(x => x.LS9_Type == BillingConstants.LicenceSetting.MinSpend)
					.Cast<MinSpendLicenceSetting>();
			}
			else
			{
				return null;
			}
		}

		public bool HasCustomHighVolumeFeatureSetting(UsageCodeKey priceKey) =>
			settings?.OfType<HighVolumeFeatureSetting>()
					.Any(x => x.LS9_Type == BillingConstants.LicenceSetting.HighVolumeFeature && x.PriceKey.EqualsIgnoringCase(priceKey)) ?? false;

		public IEnumerable<BorderWisePurchasedLicenceSetting> GetBorderWisePurchasedLicenceSettings()
		{
			if (settings != null)
			{
				return settings.Where(x => x.LS9_Type == BillingConstants.LicenceSetting.BorderWisePurchasedLicences)
					.Cast<BorderWisePurchasedLicenceSetting>();
			}
			else
			{
				return null;
			}
		}

		public VersionSurchargeLicenceSetting GetVersionSurchargeLicenceSetting()
			=> (VersionSurchargeLicenceSetting)settings?.FirstOrDefault(x => x.LS9_Type == BillingConstants.LicenceSetting.VersionSurcharge);

		public DiscountSuspensionPolicyLicenceSetting GetDiscountSuspensionPolicyLicenceSetting()
			=> (DiscountSuspensionPolicyLicenceSetting)settings?.FirstOrDefault(x => x.LS9_Type == BillingConstants.LicenceSetting.DiscountSuspensionPolicy);

		public int CompanyCount
		{
			get
			{
				if (!companyCount.HasValue)
				{
					companyCount = Usages.Select(x => x.ClientCompanyPk).Where(x => !x.IsEmpty).Distinct().Count();
				}
				return companyCount.Value;
			}
		}

		int? companyCount;

		public const string ActiveUsersUsageCode = "USR";
		public const string RoboticUsersUsageCode = "RBU";
		public const string MilestoneUsageCode = "STL";

		public int ActiveUserCount
		{
			get
			{
				if (!activeUserCount.HasValue)
				{
					activeUserCount = decimal.ToInt32(Usages.Where(x => x.Code == BillingConstants.BillingSystem.STL && x.SubCode == ActiveUsersUsageCode).Sum(x => x.UnitCount));
				}
				return activeUserCount.Value;
			}
		}

		int? activeUserCount;

		public int RoboticUserCount
		{
			get
			{
				if (!roboticUserCount.HasValue)
				{
					roboticUserCount = decimal.ToInt32(Usages.Where(x => x.Code == BillingConstants.BillingSystem.STL && x.SubCode == RoboticUsersUsageCode).Sum(x => x.UnitCount));
				}
				return roboticUserCount.Value;
			}
		}

		int? roboticUserCount;

		public DiscountVersion GetDiscountVersion(string discountVersion)
		{
			if (stlPriceList?.Discounts?.IsVersion(discountVersion) ?? false)
			{
				return stlPriceList.Discounts;
			}

			foreach (var priceList in additionalPriceLists.Values)
			{
				if (priceList?.Discounts?.IsVersion(discountVersion) ?? false)
				{
					return priceList.Discounts;
				}
			}

			return null;
		}
	}

	/// <summary>
	/// The set of all DatabaseUsage for a billing run.
	/// Responsible for loading all the required records from the database efficiently in bulk.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class DatabaseUsageSet
	{
		public IEnumerable<DatabaseUsage> DatabaseUsages { get { return mainDatabasePkToDatabaseUsage != null ? mainDatabasePkToDatabaseUsage.Values : Enumerable.Empty<DatabaseUsage>(); } }
		public DatabasePriceListSet AllStlPrices { get { return allStlPrices; } }
		public IReadOnlyDictionary<string, IGenericUsageSet> GenericUsageSets => genericUsageSets;
		public IGenericUsageSet GetGenericUsageSet(string productCode) => genericUsageSets.TryGetValue(productCode, out var usageSet) ? usageSet : null;

		class DatabaseUsageDeliveries
		{
			public DatabaseUsageDeliveries(ClientInvoiceDelivery mainTaxDelivery, ClientInvoiceDelivery specialTaxDelivery)
			{
				MainTaxDelivery = mainTaxDelivery;
				SpecialTaxDelivery = specialTaxDelivery;
			}

			public ClientInvoiceDelivery MainTaxDelivery { get; private set; }
			public ClientInvoiceDelivery SpecialTaxDelivery { get; private set; }
		}

		class DatabaseUsageOwnerDeliveries
		{
			public DatabaseUsageOwnerDeliveries(UsageOwnerDelivery mainTaxOwner, UsageOwnerDelivery specialTaxOwner)
			{
				MainTaxOwner = mainTaxOwner;
				SpecialTaxOwner = specialTaxOwner;
			}

			public UsageOwnerDelivery MainTaxOwner { get; private set; }
			public UsageOwnerDelivery SpecialTaxOwner { get; private set; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		public DatabaseUsageSet(
			BillingRunContext context,
			bool includeOnlySiteLive,
			string[] codesFilter,
			int accumulateMonths,
			IEnumerable<Guid> feeDatabasePks = null,
			IGenericUsageSetFactory usageSetFactory = null)
		{
			var factory = context.Factory;

			List<ClientChargeableUsage> chargeableUsages;
			Dictionary<Guid, IBilledDatabase> usageDatabasePkMap;
			Dictionary<Guid, IBilledDatabase> mainDatabasePkMap;
			{
				var databases = new List<BilledDatabase>();
				chargeableUsages = GetUsages(context, includeOnlySiteLive, codesFilter, accumulateMonths, databases);
				mainDatabasePkMap = databases.Where(x => x.IsMainDatabase).ToDictionary(x => x.PK.ToGuid(), x => (IBilledDatabase)x);
				usageDatabasePkMap = databases.ToDictionary(x => x.PK.ToGuid(), x => (IBilledDatabase)x);
				genericUsageSets = usageSetFactory?.GetUsageSets(context, mainDatabasePkMap)
					.ToDictionary(k => k.ProductCode, v => v) ?? new Dictionary<string, IGenericUsageSet>(0);

				foreach (var v in genericUsageSets.Values)
				{
					v.AppendTo(usageDatabasePkMap, mainDatabasePkMap, chargeableUsages);
				}

				var feeAndUsageDatabasePks = usageDatabasePkMap.Keys.ToList();
				if (feeDatabasePks != null)
				{
					feeAndUsageDatabasePks.AddRange(feeDatabasePks.Where(x => !usageDatabasePkMap.ContainsKey(x)));
				}

				if (feeAndUsageDatabasePks.Any())
				{
					allStlPrices = new DatabasePriceListSet(context, feeAndUsageDatabasePks, context.DiscountVersions);
					databasePkToSettings = GetDatabaseSettings(feeAndUsageDatabasePks, context)
						.GroupBy(x => x.LS9_LD.ToGuid())
						.ToDictionary(x => x.Key, y => y.ToArray());
				}

				// pre-fetch all enterprises
				factory.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.PK, mainDatabasePkMap.Values.Select(x => x.LD_LE.ToGuid())));
			}

			var mainDatabasePks = mainDatabasePkMap.Keys.ToArray();
			var genericPriceListsByProduct = genericUsageSets.Values.Where(x => x.HasUsage)
				.Select(x => new { Product = x.ProductCode, PriceList = BuildGlobalPriceList(x.PriceListCode, context, context.DiscountVersions, isMultiCurrencies: true, includeCargoWiseOneDiscounts: true) })
				.Where(x => x.PriceList != null)
				.ToDictionary(k => k.Product, v => v);

			mainDatabasePkToDatabaseUsage = new Dictionary<Guid, DatabaseUsage>(mainDatabasePks.Length);
			if (mainDatabasePks.Length > 0)
			{
				var discountVersionSet = context.DiscountVersions;

				var services = PremiumServiceBillingSystem.GetDue(context.Factory, context.PeriodStart, mainDatabasePks, codesFilter);

				var systemLicenceBilling = new SystemLicenceBilling();
				var productionHostedSystemServices = systemLicenceBilling.CreateHostedProductionServices(factory, mainDatabasePkMap.Values, context.PeriodStart);
				var testSystemServices = systemLicenceBilling.CreateNonProductionServices(factory, mainDatabasePks,
					allStlPrices.DatabasePkToPriceHeader,
					context.PeriodStart);
				var allSystemLicenceServices = new List<ClientPremiumService>(productionHostedSystemServices.Count + testSystemServices.Count);
				allSystemLicenceServices.AddRange(productionHostedSystemServices);
				allSystemLicenceServices.AddRange(testSystemServices);

				if (codesFilter != null && codesFilter.Any())
				{
					allSystemLicenceServices.RemoveAll(x => !codesFilter.Any(y => y == x.CPS_Type));
				}

				// Load all ClientCompany records.
				var clientCompanyPkSet = new HashSet<Guid>(chargeableUsages.Where(x => !x.U1_LCC.IsEmpty).Select(x => x.U1_LCC.ToGuid()));
				clientCompanyPkSet.UnionWith(services.Where(x => x.CPS_LCC.IsValid).Select(x => x.CPS_LCC.ToGuid()));
				clientCompanies = factory.Load<ClientCompany>(new ZQuery(ClientCompanySchema.PK, clientCompanyPkSet));
				var clientCompanyPkMap = clientCompanies.ToDictionary(x => x.PK.ToGuid());

				var usageSet = new UsageSet(chargeableUsages, services, allSystemLicenceServices, context.PeriodStart, usageDatabasePkMap, clientCompanyPkMap);
				var mainDatabasePkToUsages = usageSet.Usages.GroupBy(x => x.MainDatabasePk).ToDictionary(x => x.Key, y => y.ToArray());
				var databasePkToClientCompanies = clientCompanies.GroupBy(x => x.LCC_LD.ToGuid()).ToDictionary(x => x.Key, y => y.ToArray());
				var mainDatabasePksWithUsage = mainDatabasePkToUsages.Select(x => x.Key);

				// Owners of usage
				var mainDatabasePkToOwnerCompanyPk = DatabaseOwnerHelper.GetDatabaseOwner(mainDatabasePks);

				var mainDatabasePksBilledPerCompany = new HashSet<Guid>(mainDatabasePksWithUsage.Where(x => mainDatabasePkMap[x].LD_IsBilledPerCompany));
				var clientCompaniesBilledPerCompany = clientCompanies.Where(x => mainDatabasePksBilledPerCompany.Contains(x.LCC_LD.ToGuid())).ToArray();
				var billedClientCompanyPkToOwnerOrgPK = clientCompaniesBilledPerCompany.Where(x => !x.LCC_OH.IsEmpty).ToDictionary(x => x.PK.ToGuid(), y => y.LCC_OH.ToGuid());
				var companyDeliveries = context.CompanyDeliveries;
				companyDeliveries.AddOwnerCompanyPks(mainDatabasePkToOwnerCompanyPk.Values);
				companyDeliveries.AddOwnerOrgPks(billedClientCompanyPkToOwnerOrgPK.Values);

				var licCompanyPksFromClientCompanyOwners = billedClientCompanyPkToOwnerOrgPK
					.Select(x => companyDeliveries.GetCompanyByOrgPk(x.Value))
					.Where(licCompany => licCompany != null)
					.Select(licCompany => licCompany.PK.ToGuid());

				var licCompanyPksFromDatabaseOwners = mainDatabasePksBilledPerCompany
					.Select(x => { mainDatabasePkToOwnerCompanyPk.TryGetValue(x, out var companyPk); return companyPk; })
					.Where(pk => pk != Guid.Empty);

				var licHeadersBilledPerCompany = GetLicenceHeaders(context.Factory, mainDatabasePksBilledPerCompany, licCompanyPksFromClientCompanyOwners.Concat(licCompanyPksFromDatabaseOwners));
				var dbCompanyPkPairToLicenceHeader = licHeadersBilledPerCompany.ToDictionary(x => new Tuple<Guid, Guid>(x.LA_LD.ToGuid(), x.LA_LC.ToGuid()));

				var mainDatabasePkToDelivery = GetDatabasePkToDelivery(mainDatabasePkMap, mainDatabasePkToOwnerCompanyPk, companyDeliveries);
				var clientCompanyPkToDelivery = GetClientCompanyPkToDelivery(mainDatabasePkMap, clientCompanyPkMap, billedClientCompanyPkToOwnerOrgPK, companyDeliveries);

				var allDeliveries = mainDatabasePkToDelivery.Values.Concat(clientCompanyPkToDelivery.Values);
				companyDeliveries.AddPayingOrgs(allDeliveries.Select(x => x.MainTaxDelivery));
				companyDeliveries.LoadOrgs();

				var mainDatabasePkToDatabaseUsageOwner = BuildMainDatabasePkToDatabaseUsageOwner(mainDatabasePkMap, mainDatabasePkToUsages, mainDatabasePkToOwnerCompanyPk, companyDeliveries, mainDatabasePkToDelivery, dbCompanyPkPairToLicenceHeader);

				DatabasePriceListSet hubPriceListSet = BuildHubPrices(context, chargeableUsages, mainDatabasePkToDatabaseUsageOwner, discountVersionSet);

				var globalPriceLists = BuildGlobalPriceListDictionary(context, discountVersionSet);

				foreach (var usageByMainDatabase in mainDatabasePkToUsages)
				{
					var mainDatabasePk = usageByMainDatabase.Key;
					var mainDb = (BilledDatabase)mainDatabasePkMap[mainDatabasePk];
					var owner = mainDatabasePkToDatabaseUsageOwner[mainDatabasePk];
					var mainTaxOwner = owner.MainTaxOwner;
					var specialTaxOwner = owner.SpecialTaxOwner;
					var priceLink = allStlPrices.GetPriceLinkByDatabasePk(mainDatabasePk);
					var priceList = allStlPrices.GetPriceList(priceLink) ?? genericPriceListsByProduct.GetValueSafe(mainDb.LD_Product)?.PriceList;
					var settings = GetMainAndChildSettings(mainDatabasePk, usageByMainDatabase.Value);
					ZDateTime siteLive = mainDb.AgreedLiveDate;
					bool isLive = mainDb.IsLive;

					PriceList hubPriceList = null;
					if (hubPriceListSet != null)
					{
						hubPriceList = hubPriceListSet.GetPriceListByDatabasePk(mainDatabasePk);
					}

					var additionalPriceLists = new Dictionary<string, PriceList>(globalPriceLists);
					additionalPriceLists[BillingConstants.PriceHeaderType.EHub] = hubPriceList;
					foreach (var genericPriceList in genericPriceListsByProduct.Values.Select(v => v.PriceList))
					{
						additionalPriceLists[genericPriceList.Header.L6_SystemCode] = genericPriceList;
					}

					var priceCurrency = new[]
					{
						priceLink?.PHL_RX_NKCurrency.ToString(),
						settings?.FirstOrDefault(x => x.LS9_Type == BillingConstants.LicenceSetting.BillingSummaryCurrency)?.LS9_RX_NKPriceCurrency.ToString(),
						!mainDb.IsEnterpriseFamilyDatabase ? mainTaxOwner?.Delivery?.L9_RX_NKInvoiceCurrency.ToString() : null,
						""
					}.First(x => x != null);

					databasePkToClientCompanies.TryGetValue(mainDatabasePk, out var clientCompanies);
					var dbUsage = new DatabaseUsage(mainTaxOwner, usageByMainDatabase.Value, priceCurrency, priceList, settings, siteLive, isLive, additionalPriceLists, clientCompanies, priceLink);
					mainDatabasePkToDatabaseUsage.Add(mainDatabasePk, dbUsage);

					if (mainDb.LD_IsBilledPerCompany)
					{
						foreach (var usagesPerCompany in usageByMainDatabase.Value.GroupBy(x => x.ClientCompanyPk))
						{
							UsageOwnerDelivery companyMainTaxOwner = mainTaxOwner;
							UsageOwnerDelivery companySpecialTaxOwner = specialTaxOwner;
							if (!usagesPerCompany.Key.IsEmpty)
							{
								var clientCompanyPk = usagesPerCompany.Key.ToGuid();
								DatabaseUsageDeliveries clientCompanyDelivery;
								clientCompanyPkToDelivery.TryGetValue(clientCompanyPk, out clientCompanyDelivery);
								LicenceCompany clientOwnerCompany = null;
								Guid ownerOrgPk;
								if (billedClientCompanyPkToOwnerOrgPK.TryGetValue(clientCompanyPk, out ownerOrgPk))
								{
									clientOwnerCompany = companyDeliveries.GetCompanyByOrgPk(ownerOrgPk);
								}

								if (clientOwnerCompany != null && clientOwnerCompany.PK != mainTaxOwner.OwnerCompany.PK)
								{
									LicenceHeader licHeader;
									dbCompanyPkPairToLicenceHeader.TryGetValue(new Tuple<Guid, Guid>(mainDb.PK.ToGuid(), clientOwnerCompany.PK.ToGuid()), out licHeader);

									companyMainTaxOwner = new UsageOwnerDelivery(new UsageOwner(mainDb, clientOwnerCompany, licHeader), clientCompanyDelivery?.MainTaxDelivery,
										companyDeliveries.GetInvoicedCompany(clientCompanyDelivery?.MainTaxDelivery));

									companySpecialTaxOwner = new UsageOwnerDelivery(new UsageOwner(mainDb, clientOwnerCompany, licHeader), clientCompanyDelivery?.SpecialTaxDelivery,
										companyDeliveries.GetInvoicedCompany(clientCompanyDelivery?.SpecialTaxDelivery));
								}
							}

							foreach (var usage in usagesPerCompany)
							{
								usage.OwnerDelivery = usage.IsSpecialTaxUsage ? companySpecialTaxOwner : companyMainTaxOwner;
							}
						}
					}
					else
					{
						foreach (var usage in usageByMainDatabase.Value)
						{
							usage.OwnerDelivery = usage.IsSpecialTaxUsage ? specialTaxOwner : mainTaxOwner;
						}
					}
				}
			}
		}

		EdiLicenceSetting[] GetMainAndChildSettings(Guid mainDatabasePk, Usage[] usages)
		{
			EdiLicenceSetting[] settings;
			databasePkToSettings.TryGetValue(mainDatabasePk, out settings);

			var childDatabasePks = usages
				.Where(x => x.ChargeableUsage != null
					&& !x.ChargeableUsage.U1_LD.IsEmpty
					&& x.ChargeableUsage.U1_LD != mainDatabasePk
					&& !x.ChargeableUsage.Database.IsEnterpriseFamilyDatabase)
				.Select(x => x.ChargeableUsage.U1_LD.ToGuid())
				.Distinct();

			foreach (var childDatabasePk in childDatabasePks)
			{
				if (databasePkToSettings.TryGetValue(childDatabasePk, out var childSettings))
				{
					settings = settings != null
						? settings.Concat(childSettings).ToArray()
						: childSettings;
				}
			}

			return settings;
		}

		readonly DatabasePriceListSet allStlPrices;
		readonly ClientCompany[] clientCompanies;
		readonly Dictionary<Guid, DatabaseUsage> mainDatabasePkToDatabaseUsage;
		readonly Dictionary<Guid, EdiLicenceSetting[]> databasePkToSettings;
		readonly Dictionary<string, IGenericUsageSet> genericUsageSets;

		internal DiscountLicenceSetting GetSettingByDatabasePk(Guid databasePk, EdiPriceHeaderDiscount prepayHeaderDiscount)
		{
			EdiLicenceSetting[] result;
			databasePkToSettings.TryGetValue(databasePk, out result);
			return DatabaseUsage.GetDiscountSetting(result, prepayHeaderDiscount.PHD_Name);
		}

		static LicenceHeader[] GetLicenceHeaders(BusinessObjectFactory factory, IEnumerable<Guid> databasePks, IEnumerable<Guid> companyPks)
		{
			var query = new ZQuery(LicenceHeaderSchema.LA_LD, databasePks);
			if (companyPks.Any())
			{
				query.AddToFilter(LicenceHeaderSchema.LA_LC, companyPks);
			}
			return factory.Load<LicenceHeader>(query);
		}

		static DatabasePriceListSet BuildHubPrices(BillingRunContext context,
			IEnumerable<ClientChargeableUsage> chargeableUsages,
			Dictionary<Guid, DatabaseUsageOwnerDeliveries> databasePkToDatabaseUsageOwner,
			DiscountVersionSet discountVersionSet)
		{
			DatabasePriceListSet hubPriceListSet = null;
			var customDatabasePks = new HashSet<Guid>(chargeableUsages.Where(x => x.U1_Code == BillingConstants.BillingSystem.ClientMapping).Select(x => x.U1_LD.ToGuid()));

			if (customDatabasePks.Any())
			{
				var hubUsage = databasePkToDatabaseUsageOwner.Values.Select(x => x.MainTaxOwner).Where(x => customDatabasePks.Contains(x.Database.PK.ToGuid()));
				hubPriceListSet = new DatabasePriceListSet(context, hubUsage, BillingConstants.PriceHeaderType.EHub, discountVersionSet);
			}

			return hubPriceListSet;
		}

		public static Dictionary<string, PriceList> BuildGlobalPriceListDictionary(BillingRunContext context, DiscountVersionSet discountVersionSet)
		{
			var registryList = EDIDataRegistry.Instance.BillingStlGlobalPriceLists.Value;

			var globalPriceLists = new Dictionary<string, PriceList>(5 + registryList.Count);
			globalPriceLists.Add(BillingConstants.PriceHeaderType.GlobalContainerTracking, BuildGlobalPriceList(BillingConstants.PriceHeaderType.GlobalContainerTracking, context, discountVersionSet, isMultiCurrencies: false));
			globalPriceLists.Add(BillingConstants.PriceHeaderType.LDaaS, BuildGlobalPriceList(BillingConstants.PriceHeaderType.LDaaS, context, discountVersionSet, isMultiCurrencies: true));
			globalPriceLists.Add(BillingConstants.PriceHeaderType.ABMCustoms, BuildGlobalPriceList(BillingConstants.PriceHeaderType.ABMCustoms, context, discountVersionSet, isMultiCurrencies: false));
			globalPriceLists.Add(BillingConstants.PriceHeaderType.GoldenTax, BuildGlobalPriceList(BillingConstants.PriceHeaderType.GoldenTax, context, discountVersionSet, isMultiCurrencies: false));
			globalPriceLists.Add(BillingConstants.PriceHeaderType.FlightStats, BuildGlobalPriceList(BillingConstants.PriceHeaderType.FlightStats, context, discountVersionSet, isMultiCurrencies: true));
			globalPriceLists.Add(BillingConstants.PriceHeaderType.CargoWiseNext, BuildGlobalPriceList(BillingConstants.PriceHeaderType.CargoWiseNext, context, discountVersionSet, isMultiCurrencies: true));

			foreach (ICodeDescription priceHeader in EDIDataRegistry.Instance.BillingStlGlobalPriceLists.Value)
			{
				if (!globalPriceLists.ContainsKey(priceHeader.Code))
				{
					globalPriceLists.Add(priceHeader.Code, BuildGlobalPriceList(priceHeader.Code, context, discountVersionSet, isMultiCurrencies: true));
				}
			}

			return globalPriceLists;
		}

		public static PriceList BuildGlobalPriceList(ZString priceHeaderType, BillingRunContext context, DiscountVersionSet discountVersionSet, bool isMultiCurrencies, bool includeCargoWiseOneDiscounts = false)
			=> PriceListSet.BuildGlobalPriceList(priceHeaderType, context, discountVersionSet, isMultiCurrencies, includeCargoWiseOneDiscounts);

		static Dictionary<Guid, DatabaseUsageOwnerDeliveries> BuildMainDatabasePkToDatabaseUsageOwner(Dictionary<Guid, IBilledDatabase> databasePkMap,
			Dictionary<Guid, Usage[]> databasePkToUsages,
			Dictionary<Guid, Guid> databasePkToOwnerCompanyPk,
			LicenceCompanyDeliverySet companyDeliveries,
			Dictionary<Guid, DatabaseUsageDeliveries> databasePkToDelivery,
			Dictionary<Tuple<Guid, Guid>, LicenceHeader> dbCompanyPkPairToLicenceHeader)
		{
			var databasePkToDatabaseUsageOwner = new Dictionary<Guid, DatabaseUsageOwnerDeliveries>(databasePkToUsages.Count);

			foreach (var usageByDatabase in databasePkToUsages)
			{
				var databasePk = usageByDatabase.Key;
				var db = databasePkMap[databasePk];
				LicenceCompany systemOwnerCompany = companyDeliveries.GetCompany(databasePkToOwnerCompanyPk[databasePk]);
				DatabaseUsageDeliveries systemDelivery;
				databasePkToDelivery.TryGetValue(databasePk, out systemDelivery);

				LicenceHeader licHeader;
				dbCompanyPkPairToLicenceHeader.TryGetValue(new Tuple<Guid, Guid>(db.PK.ToGuid(), systemOwnerCompany.PK.ToGuid()), out licHeader);

				var mainTaxOwner = new UsageOwnerDelivery(new UsageOwner(db, systemOwnerCompany, licHeader), systemDelivery?.MainTaxDelivery,
					companyDeliveries.GetInvoicedCompany(systemDelivery?.MainTaxDelivery));
				var specialTaxOwner = new UsageOwnerDelivery(new UsageOwner(db, systemOwnerCompany, licHeader), systemDelivery?.SpecialTaxDelivery,
					companyDeliveries.GetInvoicedCompany(systemDelivery?.SpecialTaxDelivery));
				databasePkToDatabaseUsageOwner.Add(databasePk, new DatabaseUsageOwnerDeliveries(mainTaxOwner, specialTaxOwner));
			}

			return databasePkToDatabaseUsageOwner;
		}

		static Dictionary<Guid, DatabaseUsageDeliveries> GetClientCompanyPkToDelivery(Dictionary<Guid, IBilledDatabase> databasePkMap,
			Dictionary<Guid, ClientCompany> clientCompanyPkMap,
			Dictionary<Guid, Guid> billedClientCompanyPkToOwnerOrgPK,
			LicenceCompanyDeliverySet companyDeliveries)
		{
			var clientCompanyPkToDelivery = new Dictionary<Guid, DatabaseUsageDeliveries>();
			foreach (var clientCompanyAndOwnerOrg in billedClientCompanyPkToOwnerOrgPK)
			{
				LicenceCompany licenceCompanyOwner = companyDeliveries.GetCompanyByOrgPk(clientCompanyAndOwnerOrg.Value);
				if (licenceCompanyOwner != null)
				{
					IEnumerable<ClientInvoiceDelivery> deliveries = companyDeliveries.GetDeliveries(licenceCompanyOwner.PK.ToGuid());
					if (deliveries != null)
					{
						var db = databasePkMap[clientCompanyPkMap[clientCompanyAndOwnerOrg.Key].LCC_LD.ToGuid()];
						var billingSystemCode = EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.ContainsProductCode(db.LD_Product) ? db.LD_Product.ToString() : BillingConstants.BillingSystem.STL;
						var mainTaxDelivery = ClientInvoiceDeliveryCollection.FindByServerAndSystem(deliveries, db.LD_ServerCode, billingSystemCode);
						if (mainTaxDelivery != null)
						{
							var specialTaxDelivery = ClientInvoiceDeliveryCollection.FindByServerAndSystem(deliveries, db.LD_ServerCode, BillingConstants.PriceHeaderType.LDaaS);
							clientCompanyPkToDelivery.Add(clientCompanyAndOwnerOrg.Key, new DatabaseUsageDeliveries(mainTaxDelivery, specialTaxDelivery));
						}
					}
				}
			}
			return clientCompanyPkToDelivery;
		}

		static Dictionary<Guid, DatabaseUsageDeliveries> GetDatabasePkToDelivery(Dictionary<Guid, IBilledDatabase> databasePkMap,
			Dictionary<Guid, Guid> databasePkToOwnerCompanyPk,
			LicenceCompanyDeliverySet companyDeliveries)
		{
			var databasePkToDelivery = new Dictionary<Guid, DatabaseUsageDeliveries>();
			foreach (var dbAndOwner in databasePkToOwnerCompanyPk)
			{
				var db = databasePkMap[dbAndOwner.Key];
				IEnumerable<ClientInvoiceDelivery> deliveries = companyDeliveries.GetDeliveries(dbAndOwner.Value);
				if (deliveries != null)
				{
					var billingSystemCode = EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.ContainsProductCode(db.LD_Product) ? db.LD_Product.ToString() : BillingConstants.BillingSystem.STL;
					var mainTaxDelivery = ClientInvoiceDeliveryCollection.FindByServerAndSystem(deliveries, db.LD_ServerCode, billingSystemCode);
					if (mainTaxDelivery != null)
					{
						var specialTaxDelivery = ClientInvoiceDeliveryCollection.FindByServerAndSystem(deliveries, db.LD_ServerCode, BillingConstants.PriceHeaderType.LDaaS);
						databasePkToDelivery.Add(dbAndOwner.Key, new DatabaseUsageDeliveries(mainTaxDelivery, specialTaxDelivery));
					}
				}
			}
			return databasePkToDelivery;
		}

		#region Database Queries

		public static List<ClientChargeableUsage> GetUsages(BillingRunContext context, bool includeOnlySiteLive, string[] codesFilter, int accumulateMonths
			, List<BilledDatabase> databaseInfoList)
		{
			var dataSet = new DataSet();
			dataSet.Locale = CultureInfo.InvariantCulture;
			using (DbCommand cmd = Db.Connection.Command("EdiLoadAllStlChargeableUsage"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@PeriodStart", SqlDbType.DateTime, context.PeriodStart.ToDateTime());
				cmd.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, !context.OrganisationPK.IsEmpty ? context.OrganisationPK.ToGuid() : DBNull.Value);
				cmd.AddParameter("@EnterpriseCode", SqlDbType.VarChar, (object)context.EnterpriseCode ?? DBNull.Value);
				cmd.AddParameter("@IncludeOnlySiteLive", SqlDbType.Bit, includeOnlySiteLive);
				cmd.AddParameter("@AccumulateMonths", SqlDbType.Int, accumulateMonths);
				cmd.AddParameter("@CodeFilter", SqlDbType.VarChar, codesFilter != null && codesFilter.Length > 0 ? string.Join(",", codesFilter) : DBNull.Value);

				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(dataSet);
				}
			}

			dataSet.Tables[0].TableName = ClientChargeableUsageSchema.Constants.TableName;

			var usageRows = dataSet.Tables[0].Rows;
			int numRows = usageRows.Count;
			var usages = new List<ClientChargeableUsage>(numRows);
			for (int i = 0; i < numRows; ++i)
			{
				usages.Add(new ClientChargeableUsage(context.Factory, usageRows[i]));
			}

			var dbRows = dataSet.Tables[1].Rows;
			numRows = dbRows.Count;
			databaseInfoList.Clear();
			databaseInfoList.Capacity = numRows;
			for (int i = 0; i < numRows; ++i)
			{
				var row = dbRows[i];
				databaseInfoList.Add(new BilledDatabase(AsZGuid(row[LicenceDatabaseSchema.Constants.PK])
					, (string)(row[LicenceDatabaseSchema.Constants.LD_ServerCode])
					, (int)(row[LicenceDatabaseSchema.Constants.LD_DatabaseNumber])
					, AsZGuid(row[LicenceDatabaseSchema.Constants.LD_LE])
					, (bool)row[LicenceDatabaseSchema.Constants.LD_IsBilledPerCompany]
					, (string)(row[LicenceDatabaseSchema.Constants.LD_HostedLocation])
					, (bool)row["IsMainDatabase"]
					, AsZGuid(row["ParentPk"])
					, (string)(row["EnterpriseCode"])
					, AsZGuid(row[EdiPriceHeaderLinkSchema.Constants.PHL_L6])
					, AsZDateTime(row["AgreedLiveDate"])
					, (bool)row["IsLive"]
					, (string)(row[LicenceDatabaseSchema.Constants.LD_LicenceType])
					, ProductTypes.Codes.CargoWiseOne
					, (string)(row[LicenceDatabaseSchema.Constants.LD_Billable])
					, AsZGuid(row[LicenceDatabaseSchema.Constants.LD_OH_WebAccessOrg])
					));
			}

			return usages;
		}

		public static ZGuid AsZGuid(object val)
		{
			return val == DBNull.Value ? ZGuid.Empty : new ZGuid((Guid)val);
		}

		internal static ZDateTime AsZDateTime(object val)
		{
			return val == DBNull.Value ? ZDateTime.Empty : new ZDateTime((DateTime)val);
		}

		public static ClientChargeableUsage[] GetDatabaseUsages(BusinessObjectFactory factory, ZDateTime periodStart, Guid databasePk,
			IEnumerable<string> onDemandUsageCodesWithPerDatabaseFeeType)
		{
			var query = new ZQuery(ClientChargeableUsageSchema.U1_LD, databasePk);
			query.AddToFilter(ClientChargeableUsageSchema.U1_PeriodStart, periodStart);
			query.AddToFilter(ClientChargeableUsageSchema.U1_PeriodStart, periodStart);
			query.AddToFilter(ClientChargeableUsageSchema.U1_ManuallyProcessed, false);
			query.AddToFilter(ClientChargeableUsageSchema.U1_Code, SQLComparisonOperator.NotEqual, "PUR");

			var codeQueryOdm = new ZQuery(ClientChargeableUsageSchema.U1_Code, "ODM");
			codeQueryOdm.AddToFilter(ClientChargeableUsageSchema.U1_SubCode, onDemandUsageCodesWithPerDatabaseFeeType);

			var codeQuery = new ZQuery(ClientChargeableUsageSchema.U1_Code, SQLComparisonOperator.NotEqual, "ODM");
			codeQuery.AddToFilter(codeQueryOdm, JoinCondition.Or);

			query.AddToFilter(codeQuery);

			IEnumerable<ClientChargeableUsage> result = factory.Load<ClientChargeableUsage>(query);
			return result.OrderBy(x => x.U1_Code).ThenBy(x => x.U1_SubCode).ToArray();
		}

		static EdiLicenceSetting[] GetDatabaseSettings(IEnumerable<Guid> databasePks, BillingRunContext context)
		{
			var query = new ZQuery() { AllowTableValuedParameters = true };
			query.AddToFilter(EdiLicenceSettingSchema.LS9_LD, databasePks);

			var midMonth = context.PeriodStart.AddDays(15);

			var fromQuery = new ZQuery(EdiLicenceSettingSchema.LS9_ValidFrom, SQLComparisonOperator.LessThanOrEqualTo, midMonth);
			fromQuery.AddToFilter(JoinCondition.Or, EdiLicenceSettingSchema.LS9_ValidFrom, ZDateTime.Empty);

			var toQuery = new ZQuery(EdiLicenceSettingSchema.LS9_ValidTo, SQLComparisonOperator.GreaterThan, midMonth);
			toQuery.AddToFilter(JoinCondition.Or, EdiLicenceSettingSchema.LS9_ValidTo, ZDateTime.Empty);

			query.AddToFilter(fromQuery);
			query.AddToFilter(toQuery);

			return context.Factory.Load<EdiLicenceSetting>(query);
		}

		#endregion
	}

	public class BilledDatabase : IBilledDatabase
	{
		public BilledDatabase(ZGuid pk, ZString serverCode, ZInt databaseNumber, ZGuid enterprisePk, bool isBilledPerCompany, ZString hostedLocation,
			bool isMainDatabase, ZGuid parentPk, ZString enterpriseCode, ZGuid priceHeaderPk, ZDateTime agreedLiveDate, bool isLive, string licenceType, string product, string billable,
			ZGuid webAccessOrgPk)
		{
			PK = pk;
			LD_ServerCode = serverCode;
			LD_DatabaseNumber = databaseNumber;
			LD_LE = enterprisePk;
			LD_IsBilledPerCompany = isBilledPerCompany;
			LD_HostedLocation = hostedLocation;
			IsMainDatabase = isMainDatabase;
			LD_LD_ParentDatabase = parentPk;
			EnterpriseCode = enterpriseCode;
			PriceHeaderPk = priceHeaderPk;
			AgreedLiveDate = agreedLiveDate;
			IsLive = isLive;
			LD_LicenceType = licenceType;
			LD_Product = product;
			LD_Billable = billable;
			LD_OH_WebAccessOrg = webAccessOrgPk;
		}

		public ZGuid PK { get; private set; }
		public ZString LD_ServerCode { get; private set; }
		public ZInt LD_DatabaseNumber { get; private set; }
		public ZGuid LD_LE { get; private set; }
		public ZBool LD_IsBilledPerCompany { get; private set; }
		public ZGuid LD_LD_ParentDatabase { get; private set; }
		public ZString LD_HostedLocation { get; private set; }
		public bool IsMainDatabase;
		public ZString EnterpriseCode { get; private set; }
		public ZGuid PriceHeaderPk;
		public ZDateTime AgreedLiveDate;
		public bool IsLive;
		public ZString LD_LicenceType { get; private set; }
		public ZString LD_Product { get; private set; }
		public ZString LD_Billable { get; private set; }
		public ZGuid LD_OH_WebAccessOrg { get; private set; }

		public ZBool IsHostedOnWiseCloud
		{
			get { return LicenceDatabase.IsOnWiseCloud(LD_HostedLocation); }
		}

		public bool IsEnterpriseFamilyDatabase => ProductTypes.IsEnterpriseFamily(LD_Product);
		public bool IsBillable => LicenceDatabase.IsBillableCode(LD_Billable);
	}
}
