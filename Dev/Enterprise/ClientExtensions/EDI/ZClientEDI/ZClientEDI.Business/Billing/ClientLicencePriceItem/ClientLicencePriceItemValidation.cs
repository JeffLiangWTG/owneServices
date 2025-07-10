using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicencePriceItemValidation : AutoClientLicencePriceItemValidation
	{
		public ClientLicencePriceItemValidation(AutoClientLicencePriceItem parent)
			: base(parent)
		{
		}

		#region Parent

		public new ClientLicencePriceItem Parent
		{
			get { return (ClientLicencePriceItem)base.Parent; }
		}

		#endregion

		protected override void CheckL7_Category()
		{
			base.CheckL7_Category();
			if (!Parent.L7_Code.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.L7_CategoryInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.L7_CategoryInfo);
		}

		protected override void CheckL7_Code()
		{
			base.CheckL7_Code();
			CheckSameCodeSameSubCodesSameUnitBreak(Parent.L7_CodeInfo);

			if (!Parent.L7_Code.IsEmpty && Parent.Parent != null &&
				(Parent.L7_CodeInfo.HasChanges || !Parent.IsInDatabase || Parent.L7_CategoryInfo.HasChanges))
			{
				CheckNoDuplicatePriceCodes();
				CheckMinimumFeeUsageCode();
				CheckHostingUsageCode();
			}
		}

		void CheckNoDuplicatePriceCodes()
		{
			var priceHeader = Parent.Parent;

			if (EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes.Value.ContainsCode(Parent.L7_Code))
			{
				Parent.L7_CodeInfo.AddError("Code " + Parent.L7_Category + '-' + Parent.L7_Code + $" has already been used by Registry. {EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes.GetLocationInEnglish()}");
			}

			if (priceHeader.IsSecondaryPriceListTypeWithLocallyUniqueCodes)
			{
				return;
			}

			// Codes with fallback to main prices
			if (Parent.L7_Code == BillingConstants.BillingSystem.ClientMapping
				&&
				(
					priceHeader.L6_SystemCode == BillingConstants.PriceHeaderType.EHub
					|| priceHeader.L6_SystemCode == BillingConstants.PriceHeaderType.STL
					|| priceHeader.L6_SystemCode == BillingConstants.PriceHeaderType.ODM)
				)
			{
				return;
			}

			var priceItemQuery = new ZQuery(ClientLicencePriceItemSchema.L7_Code, Parent.L7_Code);
			priceItemQuery.AddToFilter(ClientLicencePriceItemSchema.L7_Category, Parent.L7_Category);
			priceItemQuery.AddToFilter(ClientLicencePriceItemSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			priceItemQuery.AddToFilter(ClientLicencePriceItemSchema.L7_L6, SQLComparisonOperator.NotEqual, Parent.L7_L6);
			var itemsWithSameCodes = Parent.Factory.Load<ClientLicencePriceItem>(priceItemQuery);
			if (itemsWithSameCodes.Length == 0)
			{
				return;
			}

			// preload parents
			Parent.Factory.Load<ClientLicencePriceHeader>(
				new ZQuery(ClientLicencePriceHeaderSchema.PK, itemsWithSameCodes.Select(x => x.L7_L6).Distinct()));

			var globalItemsWithSameCodes = itemsWithSameCodes.Where(x => !x.Parent.IsSecondaryPriceListTypeWithLocallyUniqueCodes).ToList();
			if (globalItemsWithSameCodes.Count == 0)
			{
				return;
			}

			if (Parent.Parent.IsMainPriceListType)
			{
				if (globalItemsWithSameCodes.Any(x => !x.Parent.IsMainPriceListType))
				{
					if (Parent.Parent.L6_SystemCode == PriceHeaderType.STL)
					{
						Parent.L7_CodeInfo.AddWarning("Code " + Parent.L7_Category + '-' + Parent.L7_Code + " has already been used on one or more global price lists. This item will be used instead of the global price item.");
					}
					else
					{
						Parent.L7_CodeInfo.AddWarning("Code " + Parent.L7_Category + '-' + Parent.L7_Code + " has already been used on one or more non-main (not ODM / STL / PUR) price lists.");
					}
				}
			}
			else
			{
				var itemsUsedByOtherSystems = globalItemsWithSameCodes.Where(x => x.Parent.L6_SystemCode != Parent.Parent.L6_SystemCode);
				if (itemsUsedByOtherSystems.Any())
				{
					if (itemsUsedByOtherSystems.All(x => x.Parent.L6_SystemCode == PriceHeaderType.STL))
					{
						Parent.L7_CodeInfo.AddWarning("Code " + Parent.L7_Category + '-' + Parent.L7_Code + " is also on some STL price lists. For those STL price lists, the STL price will be used instead of this item.");
					}
					else
					{
						Parent.L7_CodeInfo.AddError("Code " + Parent.L7_Category + '-' + Parent.L7_Code + " has already been used on one or more other price lists.");
					}
				}
			}
		}

		void CheckMinimumFeeUsageCode()
		{
			var priceHeader = Parent.Parent;
			var standardPricesCompany = LicenceCompany.StandardPricesCompany;
			if (standardPricesCompany != null && standardPricesCompany.PK == priceHeader?.LicCompany?.PK && !Parent.L7_FeeType.IsEmpty && Parent.L7_FeeType == FeeType.MinimumFee)
			{
				var priceListCode = priceHeader.L6_SystemCode;
				var usageMinimumFee = EDIDataRegistry.Instance.UsageMinimumFeeSettings.Value.MinimumFeeList.OfType<UsageMinimumFee>().FirstOrDefault(x => x.PriceListCode == priceListCode);
				if (usageMinimumFee != null && usageMinimumFee.MinimumFeeCode != Parent.L7_Code)
				{
					Parent.L7_CodeInfo.AddWarning("Code doesn't match with usage code in WiseTech Global Client Extensions > Licence Billing > Products (non-CW1) Enabled for Minimum Fee usage.");
				}
			}
		}

		void CheckHostingUsageCode()
		{
			if (Parent.L7_Code.EqualsIgnoringCase(BillingConstants.Hosting.DataAccessCode)
				|| Parent.L7_Code.EqualsIgnoringCase(BillingConstants.Hosting.DataAccessByGBCode))
			{
				if (Parent.Parent.Items.FindByCode(BillingConstants.Hosting.DataAccessCode) != null
					&& Parent.Parent.Items.FindByCode(BillingConstants.Hosting.DataAccessByGBCode) != null)
				{
					Parent.L7_CodeInfo.AddError("Only one Read-Only Access hosting price code can be added to a pricelist.");
				}

				if (Parent.L7_Code.EqualsIgnoringCase(BillingConstants.Hosting.DataAccessCode)
						&& Parent.L7_FeeType.EqualsIgnoringCase(BillingConstants.FeeType.PerGBPerMonthMin1GB))
				{
					Parent.L7_CodeInfo.AddError("Fee Code G1G is not valid for this Price Code.");
				}

				if (Parent.L7_Code.EqualsIgnoringCase(BillingConstants.Hosting.DataAccessByGBCode)
						&& Parent.L7_FeeType.EqualsIgnoringCase(BillingConstants.FeeType.PerMBPerMonthMin1GB))
				{
					Parent.L7_CodeInfo.AddError("Fee Code M1G is not valid for this Price Code.");
				}
			}
		}

		protected override void CheckL7_UnitBreak()
		{
			base.CheckL7_UnitBreak();

			if (Parent.L7_UnitBreak < 0)
			{
				Parent.L7_UnitBreakInfo.AddError("Please enter a valid break value.");
			}
			else if (Parent.L7_UnitBreak > 0)
			{
				CheckSameCodeSameSubCodesSameUnitBreak(Parent.L7_UnitBreakInfo);

				if ((BillingConstants.FeeType.IsPerDatabase(Parent.L7_FeeType) || BillingConstants.FeeType.IsPerLicence(Parent.L7_FeeType))
					&& Parent.L7_FeeType != BillingConstants.FeeType.VolumeDatabaseFee)
				{
					Parent.L7_UnitBreakInfo.AddError("Fee type can only have zero break value: " + Parent.L7_FeeType);
				}
			}
		}

		void CheckSameCodeSameSubCodesSameUnitBreak(ZPropertyInfo propertyInfo)
		{
			if (!Parent.L7_Code.IsEmpty && Parent.Parent != null && Parent.L7_CountryTierCode.IsEmpty)
			{
				if (!IsCargoWiseNextPriceItem && Parent.Parent.Items.Any(x => x.L7_Code == Parent.L7_Code && x.L7_Ref4 == Parent.L7_Ref4 && x.L7_UnitBreak == Parent.L7_UnitBreak && x.PK != Parent.PK && x.L7_Ref4 == Parent.L7_Ref4))
				{
					propertyInfo.AddError("You can't have two price items with the same codes and same sub codes and same unit break.");
				}
			}
		}

		protected override void CheckL7_RX_NKCurrency()
		{
			base.CheckL7_RX_NKCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.L7_RX_NKCurrencyInfo);
		}

		protected override void CheckL7_Ref4()
		{
			base.CheckL7_Ref4();
			if (Parent.L7_Code != BillingConstants.BillingSystem.ClientMapping || Parent.L7_Ref4.Length != 3)
			{
				ListValidation.ErrorIfInvalidCode(Parent.L7_Ref4Info);
			}
		}

		protected override void CheckL7_UnitBreakParentCode()
		{
			base.CheckL7_UnitBreakParentCode();

			var priceItem = Parent;
			if (!priceItem.L7_UnitBreakParentCode.IsEmpty)
			{
				var info = priceItem.L7_UnitBreakParentCodeInfo;

				if (priceItem.L7_FeeType != BillingConstants.FeeType.TransactionalOneVolumeBreak)
				{
					info.AddError("A value here requires a fee type of " + BillingConstants.FeeType.TransactionalOneVolumeBreak + " - " + BillingConstants.FeeTypeDescriptions.TransactionalOneVolumeBreak);
				}

				if (!priceItem.IsInDatabase || info.HasChanges)
				{
					var msg = "Value must match an existing price code in the same category as this: " + priceItem.L7_Category;
					ListValidation.ErrorIfInvalidCode((NoResString)msg, info);
				}
			}
		}

		protected override void CheckL7_FeeType()
		{
			base.CheckL7_FeeType();

			if (!Parent.L7_FeeType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.L7_FeeTypeInfo);
			}
		}

		protected override void CheckL7_Price()
		{
			base.CheckL7_Price();

			if (Parent.L7_Price != 0)
			{
				if (Parent.Lookups.PriceItemDiscountTypes.ContainsCode(Parent.L7_Code))
				{
					if (Parent.L7_Price > 0)
					{
						var message = ResString.GetMultilingualString("69EA843B-51A6-4CF2-A8CF-994AC85C79F0", "Discount should have a negative value.");
						Parent.L7_PriceInfo.AddError(message);
					}
				}
				else
				{
					MandatoryValidation.CheckNotNegative(Parent.L7_PriceInfo);
				}
			}
		}

		protected override void CheckL7_Language()
		{
			base.CheckL7_Language();

			if (Parent.L7_FeeType == FeeType.DatabaseLanguage || Parent.L7_FeeType == FeeType.DatabaseLanguageZ)
			{
				MandatoryValidation.CheckEntered(Parent.L7_LanguageInfo);
				ListValidation.ErrorIfInvalidCode(Parent.L7_LanguageInfo);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.L7_LanguageInfo);
			}
		}

		protected override void CheckL7_ParentCategory()
		{
			var info = Parent.L7_ParentCategoryInfo;
			if (!Parent.L7_ParentCode.IsEmpty)
			{
				if (Parent.L7_ParentCodeInfo.HasChanges || !Parent.IsInDatabase)
				{
					MandatoryValidation.CheckEntered(info);
				}
				else
				{
					MandatoryValidation.WarnIfNotEntered(info);
				}
			}

			if (!Parent.IsInDatabase || info.HasChanges)
			{
				ListValidation.ErrorIfInvalidCode(info);
			}
		}

		protected override void CheckL7_ParentCode()
		{
			base.CheckL7_ParentCode();
			var priceItem = Parent;
			var info = priceItem.L7_ParentCodeInfo;
			if (!priceItem.L7_ParentCode.IsEmpty)
			{
				if (!priceItem.IsInDatabase || info.HasChanges || priceItem.Parent.HasChanges)
				{
					if (priceItem.Parent.IsMainPriceListType)
					{
						ListValidation.ErrorIfInvalidCode(info);
					}
					else
					{
						ListValidation.WarnIfInvalidCode(info, priceItem.Lookups.ParentCategoryCodeList,
							(IMultilingualString)(NoResString)"Category+Code not found on this price list. This is only supported if the codes are for the minimum fee item on the customers STL pricelist.");
					}
				}
			}
			else if (!priceItem.L7_ParentCategory.IsEmpty)
			{
				info.AddError("Parent Code is mandatory if Parent Category is entered.");
			}
		}

		protected override void CheckL7_WebParentCode()
		{
			base.CheckL7_WebParentCode();

			if (Parent.L7_FeeType == FeeType.Module && !Parent.IsInDatabase)
			{
				MandatoryValidation.CheckEntered(Parent.L7_WebParentCodeInfo);
			}
		}

		protected override void CheckL7_ExchangeRateGroupCode()
		{
			base.CheckL7_ExchangeRateGroupCode();
			ListValidation.ErrorIfInvalidCode(Parent.L7_ExchangeRateGroupCodeInfo);

			if (!Parent.L7_ExchangeRateGroupCode.IsEmpty && Parent.Parent != null && !Parent.Parent.L6_HasExchangeRates)
			{
				Parent.L7_ExchangeRateGroupCodeInfo.AddError("Exchange Rate Group Code is only valid where the Price Header has Exchange Rates.");
			}
		}

		protected override void CheckL7_ProductAvailability()
		{
			base.CheckL7_ProductAvailability();
			if (!Parent.L7_ProductAvailability.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.L7_ProductAvailabilityInfo);
			}
		}

		protected override void CheckL7_ProductDisplayCategory()
		{
			base.CheckL7_ProductDisplayCategory();
			if (!Parent.L7_ProductDisplayCategory.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.L7_ProductDisplayCategoryInfo);
			}
		}

		protected override void CheckL7_CountryTierCode()
		{
			base.CheckL7_CountryTierCode();

			if (Parent.L7_CountryTierCode.IsEmpty)
			{
				if (Parent.L7_FeeType == FeeType.CountryTier)
				{
					Parent.L7_CountryTierCodeInfo.AddError(ResString.GetMultilingualString("92b1c256-9411-487d-a523-c856c9e320da", "Country Tier Code must be entered when the Price Item has a Country Tier Fee Type."));
				}

				return;
			}

			if (Parent.L7_FeeType != FeeType.CountryTier)
			{
				Parent.L7_CountryTierCodeInfo.AddError(ResString.GetMultilingualString("8fe11f29-f48b-4ceb-873c-02c0f0c9f739", "Country Tier Code is only valid when the Price Item has a Country Tier Fee Type."));
			}

			ListValidation.ErrorIfInvalidCode(Parent.L7_CountryTierCodeInfo);

			if (Parent.Parent.Items.Any(x => x.L7_Code == Parent.L7_Code && x.PK != Parent.PK && x.L7_CountryTierCode == Parent.L7_CountryTierCode))
			{
				Parent.L7_CountryTierCodeInfo.AddError(ResString.GetMultilingualString("ba34a1f7-f69b-4619-9c29-fddd82e52248", "You can't have two price items with the same codes and country tier codes."));
			}

			var countryTierCodeList = Parent.Lookups.CountryTierCodeList.ToArray().Select(x => x.Code);
			var missingCodes = countryTierCodeList.Where(x => !Parent.Parent.Items.Any(y => y.L7_FeeType == FeeType.CountryTier && y.L7_CountryTierCode == x));

			foreach (var missingCode in missingCodes)
			{
				Parent.L7_CountryTierCodeInfo.AddError(ResString.GetMultilingualString("4ae2042c-7c93-41cd-b369-ce1b224c8520", "You must also add a country tier price item for country tier code: {0}", missingCode));
			}

			if (Parent.Parent.Items.Any(x => x.L7_Code == Parent.L7_Code && x.PK != Parent.PK && x.L7_FeeType == FeeType.CountryTier && x.L7_Price == Parent.L7_Price))
			{
				Parent.L7_CountryTierCodeInfo.AddWarning(ResString.GetMultilingualString("01756543-02d3-4844-a375-d04e7de13cbc", "Potential duplicate (multiple country tiers with the same price)."));
			}

			if (Parent.Parent.Items.Any(x => x.L7_Code == Parent.L7_Code && x.PK != Parent.PK && x.L7_FeeType == FeeType.CountryTier && x.L7_DescriptionLocalized.EqualsIgnoringCase(Parent.L7_DescriptionLocalized)))
			{
				Parent.L7_CountryTierCodeInfo.AddWarning(ResString.GetMultilingualString("159cc133-554e-4b25-a29b-e4cfa66da44e", "Potential duplicate (multiple country tiers with the same description)."));
			}
		}

		protected override void CheckL7_DisbursementDirection()
		{
			base.CheckL7_DisbursementDirection();
			CheckDisbursement(Parent.L7_DisbursementDirectionInfo);
		}

		protected override void CheckL7_RN_NKDisbursementCountry()
		{
			base.CheckL7_RN_NKDisbursementCountry();
			CheckDisbursement(Parent.L7_RN_NKDisbursementCountryInfo);
		}

		void CheckDisbursement(ZPropertyInfo propertyInfo)
		{
			if (IsCargoWiseNextPriceItem)
			{
				ListValidation.ErrorIfInvalidCode(propertyInfo);
				if (Parent.Parent.Items.Any(x => x.PK != Parent.PK && x.L7_Code == Parent.L7_Code && x.L7_Category == BillingConstants.BillingSystem.CargoWiseNext
						 && x.L7_DisbursementDirection == Parent.L7_DisbursementDirection && x.L7_RN_NKDisbursementCountry == Parent.L7_RN_NKDisbursementCountry))
				{
					propertyInfo.AddError($"{Parent.L7_RN_NKDisbursementCountry} - {Parent.L7_DisbursementDirection} has already been used.");
				}
			}
			else
			{
				if (!propertyInfo.Value.IsEmpty)
				{
					propertyInfo.AddError($"{propertyInfo.HumanReadableName} is only valid for {Parent.L7_CategoryInfo.HumanReadableName} CWN.");
				}
			}
		}

		bool IsCargoWiseNextPriceItem => (Parent.Parent?.IsCargoWiseNext ?? false) && Parent.L7_Category == BillingConstants.BillingSystem.CargoWiseNext;
	}
}
