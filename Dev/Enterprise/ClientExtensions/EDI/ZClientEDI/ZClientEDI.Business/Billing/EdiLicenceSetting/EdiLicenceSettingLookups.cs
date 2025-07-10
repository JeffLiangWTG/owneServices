//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiLicenceSettingLookups
//
//    This class should be used for overriding collections in AutoEdiLicenceSettingLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiLicenceSettingLookups : AutoEdiLicenceSettingLookups
	{
		public EdiLicenceSettingLookups(AutoEdiLicenceSetting parent) : base(parent)
		{
		}

		#region Types

		public CodeDescriptionPairList Types
		{
			get
			{
				return BillingConstants.LicenceSetting.GetTypeList();
			}
		}

		public CodeDescriptionPairList TypeDescriptions
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (ICodeDescription pair in BillingConstants.LicenceSetting.GetTypeList())
				{
					result.AddPair(pair.Description);
				}

				return result;
			}
		}

		#endregion

		#region Discount Names

		public CodeDescriptionPairList DiscountNames
		{
			get
			{
				return Factory.GetCachedValue("EdiLicenceSetting.DiscountNames",
						() =>
						{
							var result = new CodeDescriptionPairList();

							foreach (var item in EDIDataRegistry.Instance.StlDiscountTypes.Value.Cast<CodeDescriptionBool>()
								.OrderBy(x => x.Code))
							{
								result.AddPair(item.Code, item.Description);
							}

							return result;
						});
			}
		}

		public CodeDescriptionPairList DiscountNamesWithCategories
		{
			get
			{
				return Factory.GetCachedValue("EdiLicenceSetting.DiscountNamesWithCategories",
						() =>
						{
							var discounts = EdiPriceHeaderDiscountCollection.GetCachedValue(Factory);
							var result = new CodeDescriptionPairList();
							bool addedDefaultEnabledCategory = false;
							result.AddPair("");
							result.Add(new CategoryCodeDescriptionPair("Optional", ""));

							foreach (var discountsByName in discounts.Cast<EdiPriceHeaderDiscount>()
								.GroupBy(x => x.PHD_Name)
								.OrderBy(x => x.First().PHD_IsDefaultEnabled ? 1 : 0)
								.ThenBy(x => x.Key))
							{
								var first = discountsByName.First();

								if (first.PHD_IsDefaultEnabled && !addedDefaultEnabledCategory)
								{
									addedDefaultEnabledCategory = true;
									result.AddPair("");
									result.Add(new CategoryCodeDescriptionPair("Active By Default", ""));
								}
								result.AddPair(discountsByName.Key, first.NameDescription);
							}

							return result;
						});
			}
		}

		#endregion

		#region Buying Group Names

		public CodeDescriptionPairList BuyingGroupNames
		{
			get
			{
				return Factory.GetCachedValue("EdiLicenceSetting.BuyingGroupNames",
						() =>
						{
							var query = new ZQuery(EdiLicenceSettingSchema.LS9_Type, BillingConstants.LicenceSetting.BuyingGroup);
							var toQuery = new ZQuery(EdiLicenceSettingSchema.LS9_ValidTo, SQLComparisonOperator.GreaterThan, ZDateTime.UtcToday);
							toQuery.AddToFilter(JoinCondition.Or, EdiLicenceSettingSchema.LS9_ValidTo, ZDateTime.Empty);
							query.AddToFilter(toQuery);
							var settings = Factory.Load<EdiLicenceSetting>(query);
							var result = new CodeDescriptionPairList();
							foreach (var name in settings.Select(x => (string)x.LS9_Name).Distinct().OrderBy(x => x))
							{
								result.AddPair(name, "");
							}

							return result;
						});
			}
		}

		#endregion

		#region Buying Group Names

		public CodeDescriptionPairList CommitmentGroupNames
		{
			get
			{
				return Factory.GetCachedValue("EdiLicenceSetting.CommitmentGroupNames",
						() =>
						{
							var query = new ZQuery(EdiLicenceSettingSchema.LS9_Type, BillingConstants.LicenceSetting.Commitment);
							var toQuery = new ZQuery(EdiLicenceSettingSchema.LS9_ValidTo, SQLComparisonOperator.GreaterThan, ZDateTime.UtcToday);
							toQuery.AddToFilter(JoinCondition.Or, EdiLicenceSettingSchema.LS9_ValidTo, ZDateTime.Empty);
							query.AddToFilter(toQuery);
							var settings = Factory.Load<EdiLicenceSetting>(query);
							var result = new CodeDescriptionPairList();
							foreach (var name in settings.Select(x => (string)x.LS9_Name).Distinct().OrderBy(x => x))
							{
								result.AddPair(name, "");
							}

							return result;
						});
			}
		}

		#endregion

		#region Price Category/Code

		public ReadOnlyCodeDescriptionPairList PriceCategories
			=> EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value;

		public ReadOnlyCodeDescriptionPairList PriceCodesForCategory
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var setting = (EdiLicenceSetting)Parent;
				if (setting.Database != null)
				{
					var link = setting.Database.PriceHeaderLinkForDate(ZDateTime.Now) ?? setting.Database.PriceHeaderLinks.OrderBy(x => x.PHL_ValidFrom).FirstOrDefault();
					if (link?.PriceHeader != null)
					{
						result.AddRange(link.PriceHeader.Lookups.CachedPriceCodesForCategory(setting.PriceCategory));
					}
				}

				var standardPricesCompany = LicenceCompany.StandardPricesCompany;
				if (standardPricesCompany != null)
				{
					var today = ZDateTime.UtcToday;
					var priceHeaders = standardPricesCompany.PriceHeaders;

					foreach (ICodeDescription priceHeaderType in BillingConstants.PriceHeaderType.PriceHeaderTypeList)
					{
						if (BillingConstants.PriceHeaderType.IsGlobal(priceHeaderType.Code))
						{
							var priceHeader = priceHeaders
								.Where(x => x.L6_SystemCode == priceHeaderType.Code)
								.OrderBy(x => x.L6_ValidFrom <= today ? 0 : 1)
								.ThenByDescending(x => x.L6_ValidFrom)
								.FirstOrDefault();
							if (priceHeader != null)
							{
								var priceCodes = priceHeader.Lookups.CachedPriceCodesForCategory(setting.PriceCategory);
								foreach (ICodeDescription priceCode in priceCodes)
								{
									result.AddPairIfNotExist(priceCode.Code, priceCode.Description);
								}
							}
						}
					}
				}

				return result;
			}
		}

		public ReadOnlyCodeDescriptionPairList BorderWisePriceCodes => BillingConstants.BorderWise.GetCachedBorderWiseModuleAndGroupList(Factory);

		public ReadOnlyCodeDescriptionPairList PriceTierCodesForCategory
		{
			get
			{
				var category = ((EdiLicenceSetting)Parent).PriceCategory;
				return Factory.GetCachedValue($"PriceTierLicenceSettingLookups.PriceTierCodesForCategory.{category}",
					() =>
					{
						var codes = new CodeDescriptionPairList(PriceCodesForCategory);
						foreach (var code in codes.GetAllCodes().Except(PriceTierCodes))
						{
							codes.RemoveCode(code);
						}
						return codes;
					});
			}
		}

		public IEnumerable<string> PriceTierCodes =>
			Factory.GetCachedValue($"PriceTierLicenceSettingLookups.PriceTierCodes",
					() =>
					{
						var collection = new DynamicBusinessObjectCollection(Factory);
						collection.Load("SELECT DISTINCT L7_Code FROM dbo.ClientLicencePriceItem WHERE L7_Code <> '' AND L7_UnitBreak <> 0;");
						return collection.Select(d => d["L7_Code"].ToString()).ToHashSet();
					});

		#endregion

		#region DepositChargeCodes

		public CodeDescriptionPairList DepositChargeCodes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (ICodeDescription item in EDIDataRegistry.Instance.DepositChargeCodes.Value)
				{
					result.AddPair(item.Code);
				}
				return result;
			}
		}

		#endregion

		#region DiscountSuspensionPolicyCodes

		public CodeDescriptionPairList DiscountSuspensionPolicyCodes
			=> Factory.GetCachedValue<CodeDescriptionPairList>("EdiLicenceSetting.DiscountSuspensionPolicyCodes",
					() => new DiscountSuspensionPolicyList());

		#endregion
	}
}

