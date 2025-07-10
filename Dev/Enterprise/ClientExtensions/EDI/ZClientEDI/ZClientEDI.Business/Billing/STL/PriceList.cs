using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public sealed class PriceList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public PriceList(ClientLicencePriceHeader priceHeader,
			PriceItemSet itemSet,
			EdiPriceUsageMapping[] mappings,
			EdiPriceItemRate[] rates,
			DiscountVersion discounts,
			bool includeCargoWiseOneDiscounts = false)
		{
			this.header = priceHeader;
			this.itemSet = itemSet;
			this.discounts = discounts;
			this.includeCargoWiseOneDiscounts = includeCargoWiseOneDiscounts;

			if (mappings != null)
			{
				usageKeyToPriceMapping = new Dictionary<UsageCodeKey, EdiPriceUsageMapping>(mappings.Length);
				foreach (var mapping in mappings)
				{
					usageKeyToPriceMapping.Add(new UsageCodeKey(mapping.PUM_UsageCategory, mapping.PUM_UsageCode), mapping);
				}
			}

			currencyToItemPkToPrice = BuildCurrencyToItemPkToPrice(priceHeader, itemSet, rates);
			itemPkToSingleCurrency = BuildItemPkToSingleCurrency(currencyToItemPkToPrice);
			priceCodeToCountryTierMapping = BuildPriceCodeToCountryTierMapping(priceHeader);
		}

		readonly PriceItemSet itemSet;
		readonly ClientLicencePriceHeader header;
		readonly Dictionary<UsageCodeKey, EdiPriceUsageMapping> usageKeyToPriceMapping;
		readonly Dictionary<string, Dictionary<Guid, decimal>> currencyToItemPkToPrice;
		readonly Dictionary<Guid, bool> itemPkToSingleCurrency;
		readonly Dictionary<string, Dictionary<string, string>> priceCodeToCountryTierMapping;
		readonly DiscountVersion discounts;
		readonly bool includeCargoWiseOneDiscounts;

		public bool IsEmpty { get { return itemSet == null || !itemSet.AllItems.Any(); } }
		public ClientLicencePriceHeader Header { get { return header; } }
		public PriceItemSet ItemSet { get { return itemSet; } }
		public bool HasSingleCurrency(ClientLicencePriceItem priceItem)
		{
			return itemPkToSingleCurrency[priceItem.PK.ToGuid()];
		}
		public DiscountVersion Discounts { get { return discounts; } }
		public bool IncludeCargoWiseOneDiscounts => includeCargoWiseOneDiscounts;

		public Dictionary<Guid, decimal> GetItemPkToPrice(string currency)
		{
			Dictionary<Guid, decimal> itemPkToPrice;
			currencyToItemPkToPrice.TryGetValue(currency, out itemPkToPrice);
			return itemPkToPrice;
		}

		public PriceNode GetPriceNode(UsageCodeKey priceKey, string countryCode = null)
		{
			PriceNode result = null;

			if (countryCode != null && priceCodeToCountryTierMapping.TryGetValue(priceKey.Item2, out var mapping))
			{
				if (mapping.TryGetValue(countryCode, out var countryTierCode))
				{
					if (ItemSet.CountryTierCodeToPriceNode.TryGetValue(countryTierCode, out var countryTierPriceNode))
					{
						result = countryTierPriceNode;
					}
				}
			}
			else
			{
				itemSet.KeyToHighestBreakPriceNode.TryGetValue(priceKey, out result);
			}

			return result;
		}

		public PriceNode GetPriceNodeFromUsageKey(UsageCodeKey usageKey, string countryCode = null)
		{
			UsageCodeKey priceKey;
			if (usageKeyToPriceMapping != null && usageKeyToPriceMapping.TryGetValue(usageKey, out var mapping))
			{
				priceKey = new UsageCodeKey(mapping.PUM_PriceCategory, mapping.PUM_PriceCode);
			}
			else
			{
				priceKey = usageKey;
			}
			return GetPriceNode(priceKey, countryCode);
		}

		public IEnumerable<UsageCodeKey> GetUsageKeysFromPriceCode(string priceCategory, string priceCode)
		{
			if (usageKeyToPriceMapping != null)
			{
				foreach (var pair in usageKeyToPriceMapping
					.Where(x => x.Value.PUM_PriceCategory == priceCategory
						&& x.Value.PUM_PriceCode == priceCode))
				{
					yield return pair.Key;
				}
			}
		}

		public IEnumerable<string> GetOnDemandUsageCodesForPreview()
		{
			var codes = new List<string>();

			foreach (var priceNode in ItemSet.AllNodesWithCode
				.Where(x => BillingConstants.FeeType.IsPerDatabase(x.Item.L7_FeeType)
					|| x.Item.L7_FeeType == BillingConstants.FeeType.Country
					//please change EdiLoadAllStlChargeableUsage() accordingly
					|| x.Item.L7_FeeType == BillingConstants.FeeType.UsersPerCountryVolumeBreak))
			{
				var item = priceNode.Item;
				string code = item.L7_Code;
				codes.Add(code);
				foreach (var usageKey in GetUsageKeysFromPriceCode(priceNode.Item.L7_Category, priceNode.Item.L7_Code))
				{
					codes.Add(usageKey.Code);
				}
			}

			return codes;
		}

		static Dictionary<string, Dictionary<Guid, decimal>> BuildCurrencyToItemPkToPrice(ClientLicencePriceHeader priceHeader, PriceItemSet itemSet, EdiPriceItemRate[] rates)
		{
			var currencyToItemPkToPrice = new Dictionary<string, Dictionary<Guid, decimal>>();
			var headerCurrency = priceHeader.L6_RX_NKCurrency;
			currencyToItemPkToPrice.Add(headerCurrency, new Dictionary<Guid, decimal>());

			Action<string, Guid, decimal> addToDictionary = (currency, priceItemPk, price) =>
			{
				Dictionary<Guid, decimal> itemPkToPrice = null;

				if (!currencyToItemPkToPrice.TryGetValue(currency, out itemPkToPrice))
				{
					itemPkToPrice = new Dictionary<Guid, decimal>();
					currencyToItemPkToPrice.Add(currency, itemPkToPrice);
				}

				if (!itemPkToPrice.ContainsKey(priceItemPk))
				{
					itemPkToPrice.Add(priceItemPk, price);
				}
			};

			foreach (var rate in rates ?? Enumerable.Empty<EdiPriceItemRate>())
			{
				addToDictionary(rate.PIR_RX_NKCurrency, rate.PIR_L7.ToGuid(), rate.PIR_Price);
			}

			foreach (var priceNode in itemSet?.AllNodesWithCode ?? Enumerable.Empty<PriceNode>())
			{
				var priceItem = priceNode.Item;
				var currency = priceItem.L7_RX_NKCurrency.IsEmpty ? headerCurrency : priceItem.L7_RX_NKCurrency;
				addToDictionary(currency, priceItem.PK.ToGuid(), priceItem.L7_Price);
			}

			return currencyToItemPkToPrice;
		}

		static Dictionary<Guid, bool> BuildItemPkToSingleCurrency(Dictionary<string, Dictionary<Guid, decimal>> currencyToItemPkToPrice)
		{
			// some prices are in a single currency, e.g. Traxon in EUR.
			var itemPkToSingleCurrency = new Dictionary<Guid, bool>();
			foreach (var currencyGroup in currencyToItemPkToPrice)
			{
				foreach (var itemPk in currencyGroup.Value.Keys)
				{
					bool hasOneCurrency;
					if (!itemPkToSingleCurrency.TryGetValue(itemPk, out hasOneCurrency))
					{
						itemPkToSingleCurrency[itemPk] = true;
					}
					else if (hasOneCurrency)
					{
						itemPkToSingleCurrency[itemPk] = false;
					}
				}
			}

			return itemPkToSingleCurrency;
		}

		static Dictionary<string, Dictionary<string, string>> BuildPriceCodeToCountryTierMapping(ClientLicencePriceHeader priceHeader)
		{
			var priceCodeToCountryTierMapping = new Dictionary<string, Dictionary<string, string>>();

			var registryValue = EDIDataRegistry.Instance.CountryTierPriceCodeMappings.Value;
			var registryMappings = registryValue.Cast<CountryTierPriceCodeMapping>().Where(x => x.SystemCode.EqualsIgnoringCase(priceHeader.L6_SystemCode));

			foreach (var registryMapping in registryMappings)
			{
				var countryTierMappings = new Dictionary<string, string>();
				foreach (var countryCodeTierMapping in registryMapping.MappingLines)
				{
					countryTierMappings.Add(countryCodeTierMapping.CountryCode, countryCodeTierMapping.CountryTierCode);
				}

				priceCodeToCountryTierMapping.Add(registryMapping.PriceCode, countryTierMappings);
			}

			return priceCodeToCountryTierMapping;
		}

		public bool HasAnyCountryTierPriceItems()
		{
			return ItemSet.CountryTierCodeToPriceNode.Any();
		}

		public bool HasPriceCodeCountryTierRegistryMapping(string priceCode)
		{
			return priceCodeToCountryTierMapping.ContainsKey(priceCode);
		}

		public string GetCountryTierCode(string priceCode, string countryCode)
		{
			if (priceCodeToCountryTierMapping.TryGetValue(priceCode, out var mapping))
			{
				if (mapping.TryGetValue(countryCode, out var countryTierCode))
				{
					return countryTierCode;
				}
			}

			return string.Empty;
		}

		public IEnumerable<Tuple<string, string>> GetMissingCountryTierPriceItems()
		{
			var priceCodesWithMissingCountryTierPriceItems = new List<Tuple<string, string>>();

			foreach (var priceCodeCountryTierSet in priceCodeToCountryTierMapping)
			{
				var countryTierCodes = priceCodeCountryTierSet.Value.GroupBy(x => x.Value).Select(g => g.Key);
				var countryTierCodesWithMissingPriceItemsPerPriceCode = countryTierCodes.Where(code => !ItemSet.CountryTierCodeToPriceNode.ContainsKey(code));
				priceCodesWithMissingCountryTierPriceItems.AddRange(countryTierCodesWithMissingPriceItemsPerPriceCode.Select(x => new Tuple<string, string>(priceCodeCountryTierSet.Key, x)));
			}

			return priceCodesWithMissingCountryTierPriceItems;
		}
	}

	/// <summary>
	/// All the discounts and structures for a discount version, such as "STL1"
	/// </summary>
	public class DiscountVersion
	{
		public DiscountVersion(string versionCode, IEnumerable<EdiPriceHeaderDiscount> discounts,
			IEnumerable<EdiPriceDiscountGroupMember> groups)
		{
			this.discounts = discounts ?? Enumerable.Empty<EdiPriceHeaderDiscount>();
			this.versionCode = versionCode;

			if (groups != null)
			{
				groupToDiscount = groups.GroupBy(x => (string)x.PGM_GroupCode, StringComparer.OrdinalIgnoreCase)
					.ToDictionary(x => x.Key, y => new HashSet<Guid>(y.Select(z => z.PGM_PHD.ToGuid())));
			}
		}

		readonly string versionCode;
		readonly Dictionary<string, HashSet<Guid>> groupToDiscount;
		readonly IEnumerable<EdiPriceHeaderDiscount> discounts;

		public string VersionCode { get { return versionCode; } }

		public bool IsVersion(string code) { return versionCode.Equals(code, StringComparison.OrdinalIgnoreCase); }

		public IEnumerable<EdiPriceHeaderDiscount> Discounts { get { return discounts; } }

		public bool GroupContains(string discountStructure, EdiPriceHeaderDiscount headerDiscount)
		{
			HashSet<Guid> pks;
			return groupToDiscount != null && groupToDiscount.TryGetValue(discountStructure, out pks)
				&& pks.Contains(headerDiscount.PK.ToGuid());
		}
	}

	public class DiscountVersionSet
	{
		public DiscountVersionSet(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;
		readonly Dictionary<string, DiscountVersion> discountCodeToVersion = new Dictionary<string, DiscountVersion>(2, StringComparer.OrdinalIgnoreCase);

		public void Add(IEnumerable<string> versionCodes)
		{
			Load(versionCodes.Where(x => !discountCodeToVersion.ContainsKey(x)));
		}

		public DiscountVersion Get(string versionCode)
		{
			DiscountVersion result;
			if (!discountCodeToVersion.TryGetValue(versionCode, out result))
			{
				Load(new string[] { versionCode });
				discountCodeToVersion.TryGetValue(versionCode, out result);
			}
			return result;
		}

		void Load(IEnumerable<string> versionCodes)
		{
			if (versionCodes.Any())
			{
				var allHeaderDiscounts = factory.Load<EdiPriceHeaderDiscount>(new ZQuery(EdiPriceHeaderDiscountSchema.PHD_Version, versionCodes));
				var discountVersionToHeaders = allHeaderDiscounts.GroupBy(x => (string)x.PHD_Version).ToDictionary(x => x.Key);
				var headerPkToHeader = allHeaderDiscounts.ToDictionary(x => x.PK.ToGuid());

				var allDiscountGroupMembers = factory.Load<EdiPriceDiscountGroupMember>(new ZQuery(EdiPriceDiscountGroupMemberSchema.PGM_PHD, allHeaderDiscounts.Select(x => x.PK.ToGuid()).Distinct().ToArray()));
				var discountVersionToGroupMembers = allDiscountGroupMembers.GroupBy(x => (string)headerPkToHeader[x.PGM_PHD.ToGuid()].PHD_Version).ToDictionary(x => x.Key);

				foreach (var versionCode in versionCodes)
				{
					IGrouping<string, EdiPriceHeaderDiscount> headerDiscounts;
					discountVersionToHeaders.TryGetValue(versionCode, out headerDiscounts);

					IGrouping<string, EdiPriceDiscountGroupMember> groupMembers;
					discountVersionToGroupMembers.TryGetValue(versionCode, out groupMembers);

					var discountVersion = new DiscountVersion(versionCode, headerDiscounts, groupMembers);
					discountCodeToVersion.Add(versionCode, discountVersion);
				}
			}
		}
	}

	public class DatabasePriceListSet
	{
		/// <summary>
		/// STL pricelists
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public DatabasePriceListSet(BillingRunContext context, IEnumerable<Guid> databasePks, DiscountVersionSet discountVersionSet)
		{
			var priceHeaderLinks = GetPriceHeaderLinks(databasePks, context);
			dbPkToPriceHeaderLink = priceHeaderLinks.ToDictionary(x => x.PHL_LD.ToGuid());
			var priceHeaderPks = priceHeaderLinks.Select(x => x.PHL_L6.ToGuid()).Distinct().ToArray();

			priceListSet = PriceListSet.NewWithRates(context.Factory, priceHeaderPks, discountVersionSet);

			dbPkToPriceList = dbPkToPriceHeaderLink.ToDictionary(x => x.Key, y => priceListSet.GetPriceList(y.Value.PHL_L6.ToGuid()));
		}

		/// <summary>
		/// Construct a legacy (non-STL) pricelist set. Used for loading the HUB pricelists.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public DatabasePriceListSet(BillingRunContext context, IEnumerable<UsageOwnerDelivery> owners, string priceHeaderType, DiscountVersionSet discountVersionSet)
		{
			var midMonth = context.PeriodStart.AddDays(15);

			// find prices on the using company
			var companyPks = new HashSet<Guid>(owners.Where(x => x.Delivery == null || !x.Delivery.L9_UseParentPrices).Select(x => x.OwnerCompany.PK.ToGuid()));
			var priceHeaders = GetCurrentPriceHeaders(context.Factory, companyPks, priceHeaderType, midMonth);

			// if prices not on using company, they can be on the paying company
			var furtherCompanyPks = owners.Where(x => x.Delivery != null
					&& !x.Delivery.L9_OH_InvoiceTo.IsEmpty
					&& x.InvoicedCompany != null
					&& !companyPks.Contains(x.InvoicedCompany.PK.ToGuid()))
					.Select(x => x.InvoicedCompany.PK.ToGuid());
			var furtherPrices = GetCurrentPriceHeaders(context.Factory, furtherCompanyPks, priceHeaderType, midMonth);

			priceListSet = PriceListSet.NewLegacy(context.Factory, priceHeaders.Concat(furtherPrices).ToArray(), discountVersionSet);

			// build dbPkToPriceList
			dbPkToPriceList = new Dictionary<Guid, PriceList>();
			var companyPkToPriceList = priceListSet.GetPriceLists().ToDictionary(x => x.Header.L6_LC.ToGuid());
			foreach (var owner in owners)
			{
				PriceList priceList;
				if (!companyPkToPriceList.TryGetValue(owner.OwnerCompany.PK.ToGuid(), out priceList)
					&& owner.InvoicedCompany != null)
				{
					companyPkToPriceList.TryGetValue(owner.InvoicedCompany.PK.ToGuid(), out priceList);
				}

				if (priceList != null)
				{
					dbPkToPriceList.Add(owner.Database.PK.ToGuid(), priceList);
				}
			}
		}

		static ClientLicencePriceHeader[] GetCurrentPriceHeaders(BusinessObjectFactory factory, IEnumerable<Guid> companyPks, string priceHeaderType, ZDateTime midMonth)
		{
			if (!companyPks.Any())
			{
				return Array.Empty<ClientLicencePriceHeader>();
			}

			var query = new ZQuery(ClientLicencePriceHeaderSchema.L6_LC, companyPks);
			query.AddToFilter(ClientLicencePriceHeaderSchema.L6_SystemCode, priceHeaderType);

			var fromQuery = new ZQuery(ClientLicencePriceHeaderSchema.L6_ValidFrom, SQLComparisonOperator.LessThanOrEqualTo, midMonth);
			fromQuery.AddToFilter(JoinCondition.Or, ClientLicencePriceHeaderSchema.L6_ValidFrom, ZDateTime.Empty);

			query.AddToFilter(fromQuery);

			var headers = factory.Load<ClientLicencePriceHeader>(query);
			return headers.GroupBy(x => x.L6_LC)
				.Select(x => x.OrderByDescending(y => y.L6_ValidFrom).First())
				.Where(x => x.L6_ValidTo.IsEmpty || x.L6_ValidTo > midMonth)
				.ToArray();
		}

		readonly Dictionary<Guid, EdiPriceHeaderLink> dbPkToPriceHeaderLink;
		readonly Dictionary<Guid, PriceList> dbPkToPriceList;
		readonly PriceListSet priceListSet;

		public Dictionary<Guid, ClientLicencePriceHeader> DatabasePkToPriceHeader
		{
			get { return databasePkToPriceHeader ?? (databasePkToPriceHeader = dbPkToPriceList.ToDictionary(x => x.Key, y => y.Value.Header)); }
		}
		Dictionary<Guid, ClientLicencePriceHeader> databasePkToPriceHeader;

		public PriceList GetPriceListByDatabasePk(Guid databasePk)
		{
			PriceList result = null;

			dbPkToPriceList.TryGetValue(databasePk, out result);

			return result;
		}

		public EdiPriceHeaderLink GetPriceLinkByDatabasePk(Guid databasePk)
		{
			EdiPriceHeaderLink result;
			dbPkToPriceHeaderLink.TryGetValue(databasePk, out result);
			return result;
		}

		public PriceList GetPriceList(EdiPriceHeaderLink priceLink)
		{
			return priceListSet.GetPriceList(priceLink);
		}

		#region Factory Loads

		static EdiPriceHeaderLink[] GetPriceHeaderLinks(IEnumerable<Guid> databasePks, BillingRunContext context)
		{
			var sql =
@"
SELECT PHL_PK
FROM
(
	SELECT 
	PHL_PK,
	PHL_ValidTo,
	RN = ROW_NUMBER() OVER(Partition by PHL_LD ORDER BY PHL_ValidFrom DESC)
	FROM dbo.EdiPriceHeaderLink
	WHERE PHL_ValidFrom <= @PeriodStart
	  AND PHL_LD IN (SELECT VALUE FROM @DatabasePks)
) T WHERE RN = 1
	  AND (PHL_ValidTo IS NULL OR PHL_ValidTo >= @DateToInclusive);
";
			var collection = new DynamicBusinessObjectCollection(context.Factory);
			var paramCollection = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@PeriodStart", context.PeriodStart, EdiPriceHeaderLinkSchema.PHL_ValidFrom),
				ZSqlParameter.New("@DateToInclusive", context.DateToInclusive, EdiPriceHeaderLinkSchema.PHL_ValidTo),
				ZSqlParameter.New("@DatabasePks", databasePks, EdiPriceHeaderLinkSchema.PHL_LD, isTableValued: true)
			};
			collection.Load(sql, paramCollection);

			var query = new ZQuery() { AllowTableValuedParameters = true };
			query.AddToFilter(EdiPriceHeaderLinkSchema.PK, collection.Select(x => (ZGuid)x["PHL_PK"]));
			return context.Factory.Load<EdiPriceHeaderLink>(query);
		}

		#endregion
	}

	/// <summary>
	/// The set of all PriceList needed for a billing run.
	/// Responsible for loading all the required records from the database efficiently in bulk.
	/// </summary>
	public class PriceListSet
	{
		public static PriceListSet NewWithRates(BusinessObjectFactory factory, IEnumerable<Guid> priceHeaderPks, DiscountVersionSet discountVersionSet)
		{
			return NewWithRates(factory, GetPriceHeaders(priceHeaderPks, factory), discountVersionSet);
		}

		public static PriceListSet NewWithRates(BusinessObjectFactory factory, ClientLicencePriceHeader[] priceHeaders, DiscountVersionSet discountVersionSet)
		{
			return new PriceListSet(factory, priceHeaders, discountVersionSet, true);
		}

		public static PriceListSet NewLegacy(BusinessObjectFactory factory, ClientLicencePriceHeader[] priceHeaders, DiscountVersionSet discountVersionSet)
		{
			return new PriceListSet(factory, priceHeaders, discountVersionSet, false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public PriceListSet(BusinessObjectFactory factory, ClientLicencePriceHeader[] priceHeaders, DiscountVersionSet discountVersionSet, bool canHaveRates)
		{
			priceHeaderPkToPriceHeader = priceHeaders.ToDictionary(x => x.PK.ToGuid());
			IEnumerable<Guid> priceHeaderPks = priceHeaderPkToPriceHeader.Keys;

			var allPriceItems = factory.Load<ClientLicencePriceItem>(new ZQuery(ClientLicencePriceItemSchema.L7_L6, priceHeaderPks));
			var headerPkToItems = allPriceItems.GroupBy(x => x.L7_L6.ToGuid()).ToDictionary(x => x.Key, y => y.ToArray());
			var itemPkToHeaderPk = allPriceItems.ToDictionary(x => x.PK, y => y.L7_L6.ToGuid());

			Dictionary<Guid, EdiPriceItemRate[]> headerPkToRates = null;
			Dictionary<Guid, EdiPriceUsageMapping[]> headerPkToMappings = null;

			if (canHaveRates)
			{
				var allRates = GetPriceItemRates(factory, priceHeaderPks);
				var allMappings = GetPriceMappings(factory, priceHeaderPks);
				headerPkToMappings = allMappings.GroupBy(x => x.PUM_L6.ToGuid()).ToDictionary(x => x.Key, y => y.ToArray());
				headerPkToRates = allRates.GroupBy(x => itemPkToHeaderPk[x.PIR_L7.ToGuid()]).ToDictionary(x => x.Key, y => y.ToArray());
			}

			priceHeaderPkToPriceList = new Dictionary<Guid, PriceList>(priceHeaders.Length);
			priceHeaderPkToPriceHeader = new Dictionary<Guid, ClientLicencePriceHeader>(priceHeaders.Length);

			discountVersionSet.Add(priceHeaders.Select(x => (string)x.L6_DiscountCode).Distinct(StringComparer.OrdinalIgnoreCase));

			foreach (var priceHeader in priceHeaders)
			{
				var headerPk = priceHeader.PK.ToGuid();

				ClientLicencePriceItem[] items;
				headerPkToItems.TryGetValue(headerPk, out items);
				var itemSet = new PriceItemSet(items ?? Enumerable.Empty<ClientLicencePriceItem>());

				EdiPriceUsageMapping[] mappings = null;
				if (headerPkToMappings != null)
				{
					headerPkToMappings.TryGetValue(headerPk, out mappings);
				}

				EdiPriceItemRate[] rates = null;
				if (headerPkToRates != null)
				{
					headerPkToRates.TryGetValue(headerPk, out rates);
				}

				var priceList = new PriceList(priceHeader, itemSet, mappings, rates, discountVersionSet.Get(priceHeader.L6_DiscountCode));
				priceHeaderPkToPriceList.Add(priceHeader.PK.ToGuid(), priceList);
			}
		}

		readonly Dictionary<Guid, ClientLicencePriceHeader> priceHeaderPkToPriceHeader;
		readonly Dictionary<Guid, PriceList> priceHeaderPkToPriceList;

		public PriceList GetPriceList(EdiPriceHeaderLink priceLink)
		{
			PriceList result = null;
			if (priceLink != null)
			{
				result = GetPriceList(priceLink.PHL_L6.ToGuid());
			}
			return result;
		}

		public PriceList GetPriceList(Guid priceHeaderPk)
		{
			PriceList result = null;
			priceHeaderPkToPriceList.TryGetValue(priceHeaderPk, out result);
			return result;
		}

		public IEnumerable<PriceList> GetPriceLists()
		{
			return priceHeaderPkToPriceList.Values;
		}

		public static PriceList BuildGlobalPriceList(ZString priceHeaderType, BillingRunContext context, DiscountVersionSet discountVersionSet, bool isMultiCurrencies, bool includeCargoWiseOneDiscounts = false)
		{
			PriceList priceList = null;
			var standardPricesCompany = LicenceCompany.StandardPricesCompany;
			if (standardPricesCompany != null)
			{
				var midMonth = new DateTime(context.PeriodStart.Year, context.PeriodStart.Month, 15);
				var priceHeader = standardPricesCompany.PriceHeaderForDate(midMonth, priceHeaderType);
				if (priceHeader != null)
				{
					var priceItems = context.Factory.Load<ClientLicencePriceItem>(new ZQuery(ClientLicencePriceItemSchema.L7_L6, priceHeader.PK));
					var priceItemSet = new PriceItemSet(priceItems);
					var discountVerion = discountVersionSet.Get(priceHeader.L6_DiscountCode);
					var mappings = GetPriceMappings(context.Factory, new[] { priceHeader.PK.ToGuid() });

					EdiPriceItemRate[] priceItemRates = null;
					if (isMultiCurrencies)
					{
						var query = new ZDBOnlyQuery(typeof(EdiPriceItemRate));
						var itemSubQuery = new ZDBOnlySubQuery(typeof(ClientLicencePriceItem), ClientLicencePriceItemSchema.PK);
						itemSubQuery.AddToFilter(ClientLicencePriceItemSchema.L7_L6, priceHeader.PK);
						query.AddSubQuery(EdiPriceItemRateSchema.PIR_L7, itemSubQuery, JoinCondition.And);
						priceItemRates = context.Factory.Load<EdiPriceItemRate>(query);
					}

					priceList = new PriceList(priceHeader, priceItemSet, mappings, priceItemRates, discountVerion, includeCargoWiseOneDiscounts);
				}
			}
			return priceList;
		}

		#region Factory Loads

		static ClientLicencePriceHeader[] GetPriceHeaders(IEnumerable<Guid> priceHeaderPks, BusinessObjectFactory factory)
		{
			var query = new ZQuery(ClientLicencePriceHeaderSchema.PK, priceHeaderPks);
			return factory.Load<ClientLicencePriceHeader>(query);
		}

		static EdiPriceItemRate[] GetPriceItemRates(BusinessObjectFactory factory, IEnumerable<Guid> priceHeaderPks)
		{
			var query = new ZDBOnlyQuery(typeof(EdiPriceItemRate));
			var itemSubQuery = new ZDBOnlySubQuery(typeof(ClientLicencePriceItem), ClientLicencePriceItemSchema.PK);
			itemSubQuery.AddToFilter(ClientLicencePriceItemSchema.L7_L6, priceHeaderPks);
			query.AddSubQuery(EdiPriceItemRateSchema.PIR_L7, itemSubQuery, JoinCondition.And);
			return factory.Load<EdiPriceItemRate>(query);
		}

		static EdiPriceUsageMapping[] GetPriceMappings(BusinessObjectFactory factory, IEnumerable<Guid> priceHeaderPks)
		{
			var query = new ZDBOnlyQuery(typeof(EdiPriceUsageMapping));
			query.AddToFilter(EdiPriceUsageMappingSchema.PUM_L6, priceHeaderPks);
			return factory.Load<EdiPriceUsageMapping>(query);
		}

		#endregion
	}
}
