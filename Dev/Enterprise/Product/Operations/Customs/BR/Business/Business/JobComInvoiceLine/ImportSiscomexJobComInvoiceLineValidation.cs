using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class ImportSiscomexJobComInvoiceLineValidation : BaseImportJobComInvoiceLineValidation
	{
		public ImportSiscomexJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override IEnumerable<IZZRateSelectionCriteria> RateSelectionCriteriaLists => Parent.DutyRateIsOverridden ? new List<IZZRateSelectionCriteria>() { } : base.RateSelectionCriteriaLists;

		protected override void CheckJI_PrimaryPreference()
		{
			base.CheckJI_PrimaryPreference();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_PrimaryPreferenceInfo);

			MultilingualString taxTypeDescription = null;
			bool hasAdditionTariffForType = true;
			if (Parent.JI_PrimaryPreference == Constants.RatePreferenceType.ExTariff || Parent.JI_PrimaryPreference == Constants.RatePreferenceType.FreeTradeAgreement)
			{
				switch (Parent.JI_PrimaryPreference)
				{
					case Constants.RatePreferenceType.ExTariff:
						hasAdditionTariffForType = Parent.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExDutyTariff) != null;
						taxTypeDescription = AdditionalTaxTypeList.Descriptions.ExDutyTariff;
						break;
					case Constants.RatePreferenceType.FreeTradeAgreement:
						hasAdditionTariffForType = Parent.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement) != null;
						taxTypeDescription = AdditionalTaxTypeList.Descriptions.TariffAgreement;
						break;
				}

				if (!hasAdditionTariffForType)
				{
					Parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("AE2A46A8-701F-4642-9585-1C15FA15B9F1", "You have not entered a '{0}' Additional Tariff", taxTypeDescription));
				}
			}
		}

		protected override void CheckJI_OA_ManufacturerAddress()
		{
			base.CheckJI_OA_ManufacturerAddress();
			if (Parent.JI_OA_ManufacturerAddress.IsValid && !Parent.ManufacturerAddress_ReadOnly)
			{
				if (Parent.InvoiceHeader?.JZ_OA_SupplierAddress == Parent.JI_OA_ManufacturerAddress)
				{
					Parent.JI_OA_ManufacturerAddressInfo.AddMessageError(Res.GetString("211CBD10-195A-4E7F-B95C-378328F0D913", "Manufacturer is equals to Supplier. Manufacturer Indicator should not be \"{0} - {1}\"", ManufacturerIndicatorList.Codes._2, ManufacturerIndicatorList.Descriptions._2));
				}

				AddressValidationHelper.CheckAddressStatusAndStreetNumber(Parent.JI_OA_ManufacturerAddressInfo, Parent.ManufacturerAddress);
			}
		}

		protected override void CheckIPIVigentRateValue()
		{
			base.CheckIPIVigentRateValue();
			if (!Parent.IPIVigentRateValueReadOnly)
			{
				ValidationHelper.CheckValidPercentage(Parent.IPIVigentRateValueInfo);
			}
		}

		protected override void CheckDutyVigentRateValue()
		{
			base.CheckDutyVigentRateValue();
			if (!Parent.DutyVigentRateValueReadOnly)
			{
				ValidationHelper.CheckValidPercentage(Parent.DutyVigentRateValueInfo);
			}
		}

		protected override void CheckFTAMarginRateValue()
		{
			base.CheckFTAMarginRateValue();
			if (!Parent.FTAMarginRateValueReadOnly)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.FTAMarginRateValueInfo);
				ValidationHelper.CheckValidPercentage(Parent.FTAMarginRateValueInfo);
			}
		}

		protected override void CheckReductionMarginRateValue()
		{
			base.CheckReductionMarginRateValue();
			if (!Parent.ReductionMarginRateValueReadOnly)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.ReductionMarginRateValueInfo);
				ValidationHelper.CheckValidPercentage(Parent.ReductionMarginRateValueInfo);
			}
		}

		protected override void CheckReducedDutyRateValue()
		{
			base.CheckReducedDutyRateValue();
			if (!Parent.ReducedDutyRateValueReadOnly)
			{
				ValidationHelper.CheckValidPercentage(Parent.ReducedDutyRateValueInfo);
			}
		}

		protected override void CheckPisVigentRateValue()
		{
			base.CheckPisVigentRateValue();
			if (!Parent.PisVigentRateValueReadOnly)
			{
				ValidationHelper.CheckValidPercentage(Parent.PisVigentRateValueInfo);
			}
		}

		protected override void CheckCofinsVigentRateValue()
		{
			base.CheckCofinsVigentRateValue();
			if (!Parent.CofinsVigentRateValueReadOnly)
			{
				ValidationHelper.CheckValidPercentage(Parent.CofinsVigentRateValueInfo);
			}
		}

		protected override void CheckPisRateIsOverridden()
		{
			base.CheckPisRateIsOverridden();
			if (!Parent.PisCofinsIsOverriddenReadOnly && !Parent.PisRateIsOverridden && Parent.CountryOfOrigin != null && !Parent.DefaultPisVigentRateValue.HasValue)
			{
				Parent.PisRateIsOverriddenInfo.AddMessageError(Res.GetString("51307A2C-3331-4ECF-B23E-472D3A6CED1F", "There is no applicable PIS Ad Valorem Rate (%) for the Tariff. Override must be used."));
			}
			if (Parent.PisRateIsOverridden && Parent.SpecialCaseTaxes.Cast<SpecialCaseTax>().Any(x => x.TaxGroup == Constants.RateCodes.PIS))
			{
				Parent.PisRateIsOverriddenInfo.AddError(Res.GetString("808602E2-9298-497B-8CAD-FDECA2602ED5", "Special Cases Ad Valorem has been entered. You cannot override the rate."));
			}
		}

		protected override void CheckCofinsRateIsOverridden()
		{
			base.CheckCofinsRateIsOverridden();
			if (!Parent.PisCofinsIsOverriddenReadOnly && !Parent.CofinsRateIsOverridden && Parent.CountryOfOrigin != null && !Parent.DefaultCofinsVigentRateValue.HasValue)
			{
				Parent.CofinsRateIsOverriddenInfo.AddMessageError(Res.GetString("5A89B026-A5A7-46BA-9440-AC1F86ACD8BD", "There is no applicable COFINS Ad Valorem Rate (%) for the Tariff. Override must be used."));
			}
			if (Parent.CofinsRateIsOverridden && Parent.SpecialCaseTaxes.Cast<SpecialCaseTax>().Any(x => x.TaxGroup == Constants.RateCodes.Cofins))
			{
				Parent.CofinsRateIsOverriddenInfo.AddError(Res.GetString("031997A5-2371-41AC-8288-87720219E873", "Special Cases Ad Valorem has been entered. You cannot override the rate."));
			}
		}

		protected override void CheckIPIRateIsOverridden()
		{
			base.CheckIPIRateIsOverridden();

			if (!Parent.IPIRateIsOverriddenReadOnly)
			{
				if (Parent.IPITaxRegime != IPITaxRegimeList.Codes.NonTaxable && !Parent.IPIRateIsOverridden && Parent.CountryOfOrigin != null && !Parent.DefaultIPIVigentRateValue.HasValue)
				{
					Parent.IPIRateIsOverriddenInfo.AddMessageError(Res.GetString("A2FAB19D-D7A6-4DF0-B4F5-CB2FB4F8D254", "There is no applicable IPI Ad Valorem (%) for the Tariff. Override must be used."));
				}
				if (Parent.IPIRateIsOverridden && Parent.DefaultIPIVigentRateValue.HasValue && Parent.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExIPITariff) == null)
				{
					Parent.IPIRateIsOverriddenInfo.AddMessageError(Res.GetString("A43EE2CB-8A17-457F-98F9-B9C60CC612C8", "Enter Ex IPI Tariff in Additional Tariff Grid"));
				}
				if (Parent.IPIRateIsOverridden && Parent.SpecialCaseTaxes.Cast<SpecialCaseTax>().Any(x => x.TaxGroup == Constants.RateCodes.IPI))
				{
					Parent.IPIRateIsOverriddenInfo.AddError(Res.GetString("F80AC975-AD98-4747-BEFB-48DCAAFE7D0E", "Special Cases Ad Valorem has been entered. You cannot override the rate."));
				}
			}
		}

		protected override void CheckFullGoodsDescription()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.FullGoodsDescriptionInfo);
		}

		protected override bool IsGoodsApplicationMandatory => Parent?.Declaration is JobDeclaration declaration && MessageSubTypeList.RequiresGoodsApplication(declaration.JE_MessageSubType);
	}
}
