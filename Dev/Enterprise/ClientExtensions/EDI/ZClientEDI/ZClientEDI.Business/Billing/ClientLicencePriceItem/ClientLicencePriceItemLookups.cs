using System.Linq;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicencePriceItemLookups : AutoClientLicencePriceItemLookups
	{
		public ClientLicencePriceItemLookups(AutoClientLicencePriceItem parent) : base(parent)
		{
		}

		public ReadOnlyCodeDescriptionPairList FeeTypes
		{
			get
			{
				var priceHeader = ((ClientLicencePriceItem)Parent).Parent;
				if (priceHeader != null && priceHeader.L6_SystemCode == BillingConstants.PriceHeaderType.STL)
				{
					return BillingConstants.GetCachedStlFeeTypeList(Factory);
				}
				else
				{
					return BillingConstants.GetCachedFeeTypeList(Factory);
				}
			}
		}

		public ReadOnlyCodeDescriptionPairList PriceCategoryList => EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value;

		/// <summary>
		/// Return list of known module codes for OnDemand price lists.
		/// </summary>
		public CodeDescriptionPairList PriceCodeList
		{
			get
			{
				var priceItem = (ClientLicencePriceItem)Parent;
				bool isOnDemand = priceItem.L7_Category == BillingConstants.PriceHeaderType.ODM || priceItem.L7_Category.IsEmpty;
				return Factory.GetCachedValue("ClientLicencePriceItemLookups.PriceCodeList" + (isOnDemand ? ".ODM" : ""), () =>
				{
					var priceCodeList = new CodeDescriptionPairList();
					if (isOnDemand)
					{
						priceCodeList.AddRange(ModuleCodeList);
						priceCodeList.AddRange(PriceItemDiscountTypes);
					}
					return priceCodeList;
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList PriceItemDiscountTypes
		{
			get
			{
				return EDIDataRegistry.Instance.PriceItemDiscountTypes.Value;
			}
		}

		public CodeDescriptionPairList ModuleCodeList
		{
			get
			{
				return LicenceModuleList.Instance.Names;
			}
		}

		/// <summary>
		/// List of other codes on the pricelist in the same category as L7_Category.
		/// </summary>
		public CodeDescriptionPairList CategoryCodeList
		{
			get
			{
				var priceItem = (ClientLicencePriceItem)Parent;
				var result = new CodeDescriptionPairList();
				foreach (ClientLicencePriceItem other in priceItem.Parent.Items)
				{
					if (other.PK != priceItem.PK && !other.L7_Code.IsEmpty
						&& (other.L7_Category == priceItem.L7_Category || priceItem.L7_Category.IsEmpty))
					{
						if (!result.ContainsCode(other.L7_Code))
						{
							result.AddPair(other.L7_Code, other.L7_DescriptionLocalized);
						}
					}
				}
				return result;
			}
		}

		/// <summary>
		/// List of other codes on the pricelist in the L7_ParentCategory category.
		/// Includes all codes if either category is empty.
		/// </summary>
		public CodeDescriptionPairList ParentCategoryCodeList
		{
			get
			{
				var priceItem = (ClientLicencePriceItem)Parent;
				var result = new CodeDescriptionPairList();
				foreach (ClientLicencePriceItem other in priceItem.Parent.Items)
				{
					if (other.PK != priceItem.PK && !other.L7_Code.IsEmpty
						&& (other.L7_Category == priceItem.L7_ParentCategory
							|| priceItem.L7_ParentCategory.IsEmpty
							|| other.L7_Category.IsEmpty))
					{
						if (!result.ContainsCode(other.L7_Code))
						{
							result.AddPair(other.L7_Code, other.L7_DescriptionLocalized);
						}
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList SubCodes
		{
			get
			{
				var item = (ClientLicencePriceItem)Parent;
				if (item.L7_Code == BillingConstants.BillingSystem.ClientMapping)
				{
					return new CodeDescriptionPairList(EDIDataRegistry.Instance.ClientMappingBillingNames.Value);
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		public CodeDescriptionPairList DiscountGroupCodes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var item = (ClientLicencePriceItem)Parent;
				var header = item.Parent;
				if (header != null && !header.L6_DiscountCode.IsEmpty)
				{
					foreach (var groupCode in header.StlItemDiscounts.Select(x => x.PGM_GroupCode).Distinct())
					{
						result.AddPair(groupCode);
					}
				}

				return result;
			}
		}

		public CodeDescriptionPairList Languages
		{
			get
			{
				return Parent.Factory.GetCachedValue("ClientLicencePriceItemLookups.Languages", () => new CodeDescriptionPairList(OLookUpEditType.Language));
			}
		}

		public ReadOnlyCodeDescriptionPairList ExchangeRateGroupCodes => EDIDataRegistry.Instance.StlPriceListExchangeRateGroups.Value;

		public ReadOnlyCodeDescriptionPairList ProductAvailabilityPairList =>
			Parent.Factory.GetCachedValue("ClientLicencePriceItemLookups.ProductAvailabilityPairList", () => new ClientLicencePriceItemProductAvailabilityPairList());

		public ReadOnlyCodeDescriptionPairList ProductDisplayCategories =>
			Factory.GetCachedValue("ClientLicencePriceItemLookups.ProductDisplayCategories", () =>
				EDIDataRegistry.Instance.ProductDisplayCategories.Value.GetActiveCodeDescriptionPairList());

		/// <summary>
		/// Return list of available Country Tier Codes defined in the registry for the given System and Price Code
		/// </summary>
		public CodeDescriptionPairList CountryTierCodeList
		{
			get
			{
				var priceItem = (ClientLicencePriceItem)Parent;

				return Factory.GetCachedValue("ClientLicencePriceItemLookups.CountryTierCodeList." + priceItem.Parent.L6_SystemCode + "." + priceItem.L7_Code, () =>
				{
					if (priceItem.Parent.L6_SystemCode.IsEmpty || priceItem.L7_Code.IsEmpty)
					{
						return new CodeDescriptionPairList();
					}

					var relevantMapping = EDIDataRegistry.Instance.CountryTierPriceCodeMappings.Value.Cast<CountryTierPriceCodeMapping>()
						.FirstOrDefault(x => x.SystemCode.EqualsIgnoringCase(priceItem.Parent.L6_SystemCode) && x.PriceCode.EqualsIgnoringCase(priceItem.L7_Code));

					if (relevantMapping == null)
					{
						return new CodeDescriptionPairList();
					}

					var countryTierCodeList = new CodeDescriptionPairList();

					foreach (var mapping in relevantMapping.MappingLines)
					{
						if (!countryTierCodeList.ContainsCode(mapping.CountryTierCode))
						{
							countryTierCodeList.AddPair(mapping.CountryTierCode);
						}
					}

					countryTierCodeList.Sort();

					return countryTierCodeList;
				});
			}
		}

		public CodeDescriptionPairList DisbursementDirectionList
			=> Factory.GetCachedValue("ClientLicencePriceItemLookups.DisbursementDirectionList", () => new OrgDocumentLookups(null).FilterDirections);
	}
}

