using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicencePriceHeaderLookups : AutoClientLicencePriceHeaderLookups
	{
		public ClientLicencePriceHeaderLookups(AutoClientLicencePriceHeader parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList LicenceEditions
		{
			get { return BillingConstants.GetLicenceEditionList(); }
		}

		public CodeDescriptionPairList SystemCodes
		{
			get
			{
				return BillingConstants.PriceHeaderType.GetPriceHeaderTypeList();
			}
		}

		public CodeDescriptionPairList PricelistVersions
		{
			get
			{
				return Factory.GetCachedValue("ClientLicencePriceHeaderLookups.PricelistVersions", delegate
				{
					return CreatePricelistVersions();
				});
			}
		}

		CodeDescriptionPairList CreatePricelistVersions()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			if (LicenceCompany.StandardPricesCompany != null)
			{
				foreach (ZString val in LicenceCompany.StandardPricesCompany.PriceHeaders.Cast<ClientLicencePriceHeader>().Select(s => s.L6_PricelistVersion).Distinct().OrderBy(s => s))
				{
					result.AddPair(val);
				}
			}
			return result;
		}

		public CodeDescriptionPairList DiscountCodes
		{
			get
			{
				return Factory.GetCachedValue("ClientLicencePriceHeaderLookups.DiscountCodes." + ((ClientLicencePriceHeader)Parent).L6_SystemCode, delegate
				{
					return CreateDiscountCodes();
				});
			}
		}

		CodeDescriptionPairList CreateDiscountCodes()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			if (LicenceCompany.StandardPricesCompany != null)
			{
				string systemCode = ((ClientLicencePriceHeader)Parent).L6_SystemCode;

				if (BillingConstants.PriceHeaderType.IsUsedInBillingStl(systemCode))
				{
					GetStlDiscountVersions(result);
				}
				else
				{
					var billing = LicenceCompany.StandardPricesCompany.ReadonlySelfBilling;
					if (billing != null)
					{
						foreach (ZString val in billing.BillingDiscounts.Cast<ClientLicenceBillingDiscount>().Select(s => s.L5_DiscountCode).Distinct().OrderBy(s => s))
						{
							result.AddPair(val);
						}
					}
				}
			}
			return result;
		}

		void GetStlDiscountVersions(CodeDescriptionPairList list)
		{
			string sql =
@"select distinct L6_DiscountCode from dbo.ClientLicencePriceHeader
where L6_SystemCode = 'STL' and L6_DiscountCode != '' and L6_LC = '" + LicenceCompany.StandardPricesCompany.PK + @"'
order by L6_DiscountCode";

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					list.AddPair(reader.GetString(0));
				}
			}
		}

		public CodeDescriptionPairList CachedPriceCodesForCategory(string category)
		{
			var priceHeader = (ClientLicencePriceHeader)Parent;
			return Factory.GetCachedValue("PriceHeader.PriceCodes." + priceHeader.PK + "." + category,
				() =>
				{
					return PriceCodesForCategory(category);
				});
		}

		public CodeDescriptionPairList PriceCodesForCategory(string category)
		{
			var result = new CodeDescriptionPairList();
			var priceHeader = (ClientLicencePriceHeader)Parent;
			var existing = new HashSet<Tuple<string, string>>();
			bool isCategoryEmpty = string.IsNullOrEmpty(category);
			foreach (ClientLicencePriceItem item in priceHeader.LocalOrStandardItems)
			{
				if (!item.L7_Code.IsEmpty && (isCategoryEmpty || category == item.L7_Category))
				{
					if (existing.Add(new Tuple<string, string>(item.L7_Category, item.L7_Code)))
					{
						var desc = item.PriceDescriptionForUniquePriceCodeLookup;
						if (isCategoryEmpty)
						{
							desc = "(" + item.L7_Category + ") " + desc;
						}
						result.AddPair(item.L7_Code, desc);
					}
				}
			}
			return result;
		}

		public CodeDescriptionPairList CachedPriceCategories
		{
			get
			{
				var priceHeader = (ClientLicencePriceHeader)Parent;
				return Factory.GetCachedValue("PriceHeader.PriceCategories" + priceHeader.PK,
					() =>
					{
						return PriceCategories;
					});
			}
		}

		public CodeDescriptionPairList PriceCategories
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var priceHeader = (ClientLicencePriceHeader)Parent;
				var descriptions = EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value;
				foreach (ClientLicencePriceItem item in priceHeader.LocalOrStandardItems)
				{
					ZString category = item.L7_Category;
					if (!category.IsEmpty && !result.ContainsCode(category))
					{
						result.AddPair(category, descriptions.GetDescriptionFromCode(category));
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList CachedPriceCodesForCategory(string category, bool appendOtherCategories = true)
		{
			var priceHeader = (ClientLicencePriceHeader)Parent;
			return Factory.GetCachedValue("PriceHeader.PriceCodes" + priceHeader.PK + category + (appendOtherCategories ? ".Append" : ""),
				() =>
				{
					return PriceCodesForCategory(category, appendOtherCategories = true);
				});
		}

		public CodeDescriptionPairList PriceCodesForServiceCategory
			=> PriceCodesForCategory(BillingConstants.BillingSystem.Service, false);

		public CodeDescriptionPairList PriceCodesForCategory(string category, bool appendOtherCategories = true)
		{
			var result = new CodeDescriptionPairList();
			var priceHeader = (ClientLicencePriceHeader)Parent;
			bool categoryIsBlank = string.IsNullOrWhiteSpace(category);
			foreach (ClientLicencePriceItem item in priceHeader.LocalOrStandardItems)
			{
				if (!item.L7_Code.IsEmpty && (categoryIsBlank || item.L7_Category == category))
				{
					result.AddPairIfNotExist(item.L7_Code, item.L7_DescriptionLocalized);
				}
			}

			if (appendOtherCategories && !categoryIsBlank)
			{
				foreach (ClientLicencePriceItem item in priceHeader.LocalOrStandardItems)
				{
					if (!item.L7_Code.IsEmpty && item.L7_Category != category)
					{
						result.AddPairIfNotExist(item.L7_Code, item.L7_DescriptionLocalized);
					}
				}
			}

			return result;
		}

		public ReadOnlyCodeDescriptionPairList RoundingList => EDIDataRegistry.Instance.BillingPriceRoundingParams.Value;
	}
}

