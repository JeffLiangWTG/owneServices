using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicencePriceHeaderValidation : AutoClientLicencePriceHeaderValidation
	{
		public ClientLicencePriceHeaderValidation(AutoClientLicencePriceHeader parent)
			: base(parent)
		{
		}

		public new ClientLicencePriceHeader Parent
		{
			get { return (ClientLicencePriceHeader)base.Parent; }
		}

		protected override void CheckL6_LicenceEdition()
		{
			base.CheckL6_LicenceEdition();
			if (!Parent.L6_LicenceEdition_ReadOnly)
			{
				var info = Parent.L6_LicenceEditionInfo;
				MandatoryValidation.CheckEntered(info);
				ListValidation.ErrorIfInvalidCode(info);
			}
		}

		protected override void CheckL6_RN_NKCountry()
		{
			base.CheckL6_RN_NKCountry();
			if (!Parent.L6_RN_NKCountry.IsEmpty)
			{
				var info = Parent.L6_RN_NKCountryInfo;
				if (info.HasChanges || !Parent.IsInDatabase)
				{
					ListValidation.ErrorIfInvalidCode(info);
				}
			}
		}

		protected override void CheckL6_RX_NKCurrency()
		{
			base.CheckL6_RX_NKCurrency();
			var info = Parent.L6_RX_NKCurrencyInfo;
			MandatoryValidation.CheckEntered(info);
			if (info.HasChanges || !Parent.IsInDatabase)
			{
				ListValidation.ErrorIfInvalidCode(info);
			}
		}

		protected override void CheckL6_ValidFrom()
		{
			base.CheckL6_ValidFrom();
			MandatoryValidation.CheckEntered(Parent.L6_ValidFromInfo);

			if (Parent.IsCargoWiseNext)
			{
				if (Parent.L6_ValidFrom.Day != 1)
				{
					Parent.L6_ValidFromInfo.AddError("Valid From date for CargoWise Next price lists must be on the first of the month.");
				}

				if ((!Parent.IsInDatabase || Parent.L6_ValidFromInfo.HasChanges || Parent.L6_SystemCodeInfo.HasChanges) && Parent.IsCargoWiseNextAndPastValidFrom)
				{
					Parent.L6_ValidFromInfo.AddError("Valid From date for CargoWise Next price lists must be in the future.");
				}
			}
		}

		protected override void CheckL6_ValidFromIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.L6_ValidFromInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = 10,
				FutureYearsBeforeWarning = 5,
				PastYearsBeforeError = 50,
				PastYearsBeforeWarning = 15
			});
		}

		protected override void CheckL6_ValidToIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.L6_ValidToInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = 50,
				FutureYearsBeforeWarning = 15,
				PastYearsBeforeError = 50,
				PastYearsBeforeWarning = 15
			});
		}

		protected override void CheckL6_SystemCode()
		{
			base.CheckL6_SystemCode();
			var info = Parent.L6_SystemCodeInfo;
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info);
			if (BillingConstants.PriceHeaderType.IsAlwaysStandard(Parent.L6_SystemCode)
				&& (info.HasChanges || !Parent.IsInDatabase))
			{
				var std = LicenceCompany.StandardPricesCompany;
				if (std == null || Parent.L6_LC != std.PK)
				{
					if (Parent.L6_SystemCode == BillingConstants.BillingSystem.STL)
					{
						Parent.L6_SystemCodeInfo.AddError("STL price lists are configured on the STL tab.");
					}
					else
					{
						var priceListDescription = BillingConstants.PriceHeaderType.GetPriceHeaderTypeList().GetDescriptionFromCode(Parent.L6_SystemCode);
						var errorMessage = priceListDescription + " price list is configured on standard prices company";
						if (std != null && std.Header != null)
						{
							errorMessage += " " + std.Header.OH_Code;
						}
						errorMessage += ".";
						Parent.L6_SystemCodeInfo.AddError(errorMessage);
					}
				}
			}
		}

		protected override void CheckL6_PricelistVersion()
		{
			base.CheckL6_PricelistVersion();
			if (Parent.L6_IsStandard)
			{
				MandatoryValidation.CheckEntered(Parent.L6_PricelistVersionInfo);
			}
		}

		protected override void CheckL6_DiscountCode()
		{
			base.CheckL6_DiscountCode();
			var info = Parent.L6_DiscountCodeInfo;
			if (!BillingConstants.PriceHeaderType.IsUsedInBillingStl(Parent.L6_SystemCode))
			{
				if (info.HasChanges || !Parent.IsInDatabase)
				{
					ListValidation.ErrorIfInvalidCode(info);
				}
			}
			else
			{
				MandatoryValidation.CheckEntered(info);
			}
		}

		protected override void CheckL6_TestDbPriceCode()
		{
			base.CheckL6_TestDbPriceCode();

			var parent = Parent;
			if (!parent.L6_IsStandard
				&& (parent.L6_TestDbPriceCodeInfo.HasChanges || !parent.IsInDatabase || parent.AreItemsLoaded))
			{
				ListValidation.ErrorIfInvalidCode(parent.L6_TestDbPriceCodeInfo);
			}
		}

		protected override void CheckL6_HasExchangeRates()
		{
			base.CheckL6_HasExchangeRates();

			var parent = Parent;
			if (parent.L6_HasExchangeRates)
			{
				if (parent.L6_SystemCode == BillingConstants.BillingSystem.ODM || parent.L6_SystemCode == BillingConstants.BillingSystem.Maintenance)
				{
					parent.L6_HasExchangeRatesInfo.AddError("Exchange Rates are not valid for ODM or Maintenance.");
				}
			}
		}

		protected override void CheckL6_Rounding()
		{
			base.CheckL6_Rounding();
			var header = Parent;
			if (!header.L6_Rounding_ReadOnly && header.L6_HasExchangeRates)
			{
				MandatoryValidation.CheckEntered(header.L6_RoundingInfo);
				ListValidation.ErrorIfInvalidCode(header.L6_RoundingInfo);
			}
		}

		protected override void CheckL6_IsDisbursementBundle()
		{
			base.CheckL6_IsDisbursementBundle();

			var parent = Parent;
			if (parent.L6_IsDisbursementBundle && !PriceHeaderType.IsGlobal(parent.L6_SystemCode))
			{
				parent.L6_IsDisbursementBundleInfo.AddError("You can only bundle Global Pricelists into the Disbursement Pricing Model.");
			}
		}
	}
}

