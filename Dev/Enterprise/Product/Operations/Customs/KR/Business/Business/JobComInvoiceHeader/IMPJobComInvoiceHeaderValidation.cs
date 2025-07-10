using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class IMPJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public IMPJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected JobDeclaration declaration => Parent.JobDeclaration;
		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateImportSupplierID();

			ValidateValuationQuestion7A_IMP();
			ValidateValuationQuestion7B_IMP();
			ValidateValuationQuestion7C();
			ValidateValuationQuestion7D();
			ValidateValuationQuestion7EA();
			ValidateValuationQuestion7EB();
			ValidateValuationQuestion8A();
			ValidateValuationQuestion8B();
			ValidateValuationQuestion9A();
			ValidateValuationQuestion9B();
		}

		protected override void CheckJZ_CU_RelatedHouseBill()
		{
			base.CheckJZ_CU_RelatedHouseBill();
			switch (declaration.JE_DeclarationPlan)
			{
				case ImportCustomsClearancePlanCodeList.Codes.C:
				case ImportCustomsClearancePlanCodeList.Codes.D:
				case ImportCustomsClearancePlanCodeList.Codes.E:
				case ImportCustomsClearancePlanCodeList.Codes.F:
					if (Parent.JZ_CU_RelatedHouseBill.IsEmpty)
					{
						Parent.JZ_CU_RelatedHouseBillInfo.AddMessageError(Res.GetString("F1991BE3-DBE0-4044-8841-E949DF99373B", "A house bill number is mandatory for the Declaration Plan Code, 'C', 'D', 'E', or 'F'."));
					}
					break;
				case ImportCustomsClearancePlanCodeList.Codes.H:
					if (!Parent.JZ_CU_RelatedHouseBill.IsEmpty)
					{
						Parent.JZ_CU_RelatedHouseBillInfo.AddMessageError(Res.GetString("385B3F75-91F8-4265-8FAD-E9109AB9ED8A", "The Declaration Plan Code is H. Please do not enter a house bill number."));
					}
					break;
				default:
					break;
			}
		}

		protected override void CheckJZ_IncoTerm()
		{
			ListValidation.ErrorIfInvalidCode(Parent.JZ_IncoTermInfo);
			if (!Parent.IsFreeTrade && Parent.JZ_IncoTerm == ZString.Empty)
			{
				Parent.JZ_IncoTermInfo.AddMessageError(Res.GetString("5A2ABAAA-C9C0-4384-ACB8-ACE3DE0671C7", "If Invoice Payment Term is not [GN] and Trade Type is not [71,80~97,100] then Incoterm is mandatory. Please enter a value."));
			}
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JZ_InvoiceAmountInfo);

			if (Parent.IsFreeTrade)
			{
				if (Parent.JZ_InvoiceAmount > ZDecimal.Zero)
				{
					Parent.JZ_InvoiceAmountInfo.AddMessageError(Res.GetString("19A7EE4F-2C46-49FA-B3A3-9BD2ADE2E8DA", "If Invoice Payment Term is [GN] and or Trade Type is [71,80~97,100] then Total Invoice Amount must be zero."));
				}
			}
			else
			{
				if (Parent.JZ_InvoiceAmount == ZDecimal.Zero)
				{
					Parent.JZ_InvoiceAmountInfo.AddMessageError(Res.GetString("A3817749-34EC-4972-B132-99FE000354C4", "If Invoice Payment Term is not [GN] and Trade Type is not [71,80~97,100] then Total Invoice Amount must be greater than zero."));
				}
			}
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JZ_RX_NKInvoice_CurrencyInfo);
			if (Parent.IsFreeTrade && Parent.JZ_RX_NKInvoice_Currency != Core.Constants.CurrencyCodes.UnitedStates)
			{
				Parent.JZ_RX_NKInvoice_CurrencyInfo.AddMessageError(Res.GetString("4BCA6355-B3E7-410D-B4BD-DF8A8CA21FB7", "If Invoice Payment Term is [GN] or Trade Type is [71,80~97,100] then Invoice Amount Currency must be [USD]."));
			}
		}

		protected override void CheckJZ_PaymentTerms()
		{
			base.CheckJZ_PaymentTerms();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JZ_PaymentTermsInfo);
		}

		protected override void CheckJZ_InvoiceCurrExRate()
		{
			base.CheckJZ_InvoiceCurrExRate();
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JZ_InvoiceCurrExRateInfo);
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();
			if (declaration != null)
			{
				if (!ImportDeclarationTypeCodeList.IsSimpleDeclarationType(declaration.JE_MessageSubType) ||
						declaration.JE_TradeType != ImportDealingTypeCodeList.Codes._91)
				{
					var supplier = InvoiceHeader.Supplier;
					if (supplier == null)
					{
						MandatoryValidation.MessageErrorIfNotEntered(InvoiceHeader.JZ_OH_SupplierInfo);
					}
					else
					{
						if (string.IsNullOrEmpty(supplier.OH_FullName))
						{
							InvoiceHeader.JZ_OH_SupplierInfo.AddMessageError(MissingCompanyNameMessage);
						}
						if (string.IsNullOrEmpty(supplier.CountryCode))
						{
							InvoiceHeader.JZ_OH_SupplierInfo.AddMessageError(MissingCountryCodeMessage);
						}
					}
				}
			}
		}
		protected override void CheckJZ_OA_SellerAddress()
		{
			base.CheckJZ_OA_SellerAddress();
			if (declaration != null)
			{
				if (declaration.JE_TradeType == ImportDealingTypeCodeList.Codes._15 &&
						OnlineTradeTypeCodeList.IsIdentifiableType(InvoiceHeader.JZ_OnlineTradeType))
				{
					var seller = InvoiceHeader.SellerAddress;
					if (seller == null)
					{
						MandatoryValidation.MessageErrorIfNotEntered(InvoiceHeader.JZ_OA_SellerAddressInfo);
					}
					else
					{
						if (string.IsNullOrEmpty(seller.GetRegistrationIDNumber(IdentificationType.OnlineTradeSellerID)?.Number))
						{
							if (string.IsNullOrEmpty((seller.Header.OH_FullName)))
							{
								InvoiceHeader.JZ_OA_SellerAddressInfo.AddMessageError(MissingCompanyNameMessage);
							}
						}
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(InvoiceHeader.JZ_OA_SellerAddressInfo);
				}
			}
		}

		protected override void CheckJZ_OH_SellingAgent()
		{
			base.CheckJZ_OH_SellingAgent();
			if (declaration != null && declaration.JE_TradeType != ImportDealingTypeCodeList.Codes._15)
			{
				MandatoryValidation.MessageErrorIfIsEntered(InvoiceHeader.JZ_OH_SellingAgentInfo);
			}
		}

		public void ValidateImportSupplierID()
		{
			ValidateCalculatedProperty(InvoiceHeader.ImportSupplierIDInfo);
		}

		protected void CheckImportSupplierID()
		{
			if (declaration != null &&
					(!ImportDeclarationTypeCodeList.IsSimpleDeclarationType(declaration.JE_MessageSubType) || declaration.JE_TradeType != ImportDealingTypeCodeList.Codes._91))
			{
				if (string.IsNullOrEmpty(InvoiceHeader.ImportSupplierID))
				{
					InvoiceHeader.ImportSupplierIDInfo.AddMessageError(GetMissingRegistrationNumberMessage((NoResString)"Supplier ID", IdentificationType.ForeignCompanyID));
				}
			}
		}

		protected override void CheckJZ_OA_DistributorAddress()
		{
			base.CheckJZ_OA_DistributorAddress();
			if (declaration.JE_TradeType == ImportDealingTypeCodeList.Codes._15 && (ImportOnlineTypeCodeList.IsDistributorRequired(Parent.JZ_OnlineTradeType)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OA_DistributorAddressInfo);
				if (Parent.DistributorAddress != null)
				{
					if ((Parent.DistributorAddress.GetRegistrationNumber(Constants.IdentificationType.ECommerceCompanyID)).IsEmpty)
					{
						if (Parent.DistributorAddress.CompanyName.IsEmpty)
						{
							Parent.JZ_OA_DistributorAddressInfo.AddMessageError(MissingCompanyNameMessage);
						}
					}
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JZ_OA_DistributorAddressInfo);
			}
		}

		protected override void CheckJZ_ImportCargoManagementNumber()
		{
			base.CheckJZ_ImportCargoManagementNumber();
			MandatoryValidation.MessageErrorIfNotEntered(InvoiceHeader.JZ_ImportCargoManagementNumberInfo);
			switch (InvoiceHeader.JZ_ImportCargoManagementNumber.Length)
			{
				case 2:
					if (!InvoiceHeader.JZ_ImportCargoManagementNumber.ToUpper().Equals(ImportCargoManagementNumber.No))
					{
						InvoiceHeader.JZ_ImportCargoManagementNumberInfo.AddMessageError(Res.GetString("9CE2C884-B452-43CF-AF4B-C03E5F068E30", "Only 'NO' is accepted when the length of the value is 2."));
					}
					break;
				case 15:
					if (!ZInt.CanParse(InvoiceHeader.JZ_ImportCargoManagementNumber.Right(4)))
					{
						InvoiceHeader.JZ_ImportCargoManagementNumberInfo.AddMessageError(Res.GetString("0C986FE3-1518-4714-B739-533073484D44", "The last 4 digits should be numeric."));
					}
					break;
				case 19:
					if (!ZInt.CanParse(InvoiceHeader.JZ_ImportCargoManagementNumber.Right(8)))
					{
						InvoiceHeader.JZ_ImportCargoManagementNumberInfo.AddMessageError(Res.GetString("AC9611D3-9459-47C5-A90A-3F02B17595CD", "The last 8 digits should be numeric."));
					}
					break;
				default:
					InvoiceHeader.JZ_ImportCargoManagementNumberInfo.AddMessageError(Res.GetString("F5866CDD-6BBF-43BE-9F21-3347221FA8DA", "The length of the value should be 2, 15 or 19."));
					break;
			}
		}

		protected override void CheckJZ_BlanketValuationDeclarationNumber()
		{
			if (InvoiceHeader.JZ_ValuationDecAttachCode == ValueDeclarationAttachedCodeList.Codes.P && InvoiceHeader.JZ_BlanketValuationDeclarationNumber.IsEmpty)
			{
				InvoiceHeader.JZ_BlanketValuationDeclarationNumberInfo.AddMessageError(Res.GetString("8B02B4CB-A6F8-49F8-9206-5B63D64C2447", "You have indicated that this entry is covered by a periodic valuation declaration. Please enter its declaration number."));
			}
		}

		protected override void CheckJZ_COOStatus()
		{
			base.CheckJZ_COOStatus();

			if (!ImportDeclarationTypeCodeList.IsSimpleDeclarationType(declaration?.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(InvoiceHeader.JZ_COOStatusInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.JZ_COOStatusInfo);
		}

		protected override void CheckJZ_ValuationDecAttachCode()
		{
			base.CheckJZ_ValuationDecAttachCode();

			if (!ImportDeclarationTypeCodeList.IsSimpleDeclarationType(declaration?.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(InvoiceHeader.JZ_ValuationDecAttachCodeInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.JZ_ValuationDecAttachCodeInfo);
		}

		protected override void CheckJZ_OnlineTradeType()
		{
			base.CheckJZ_OnlineTradeType();
			if (declaration.JE_TradeType != ImportDealingTypeCodeList.Codes._15)
			{
				MandatoryValidation.MessageErrorIfIsEntered(InvoiceHeader.JZ_OnlineTradeTypeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.JZ_OnlineTradeTypeInfo);
		}
		protected override void CheckJZ_COOExemptionReason()
		{
			base.CheckJZ_COOExemptionReason();
			if (InvoiceHeader.JZ_COOLabelLocation.Contains(CountryOfOriginLabelLocationCodeList.Codes.E))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(InvoiceHeader.JZ_COOExemptionReasonInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.JZ_COOExemptionReasonInfo);
			}
		}

		protected override void CheckJZ_COOLabelType()
		{
			base.CheckJZ_COOLabelType();
			if (InvoiceHeader.JZ_COOLabelLocation.Contains(CountryOfOriginLabelLocationCodeList.Codes.Y) ||
				InvoiceHeader.JZ_COOLabelLocation.Contains(CountryOfOriginLabelLocationCodeList.Codes.B) ||
				InvoiceHeader.JZ_COOLabelLocation.Contains(CountryOfOriginLabelLocationCodeList.Codes.G))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(InvoiceHeader.JZ_COOLabelTypeInfo);
				if (InvoiceHeader.JZ_COOLabelLocation.Contains(CountryOfOriginLabelLocationCodeList.Codes.B) &&
					(InvoiceHeader.JZ_COOLabelType.Contains(CountryOfOriginLabelTypeCodeList.Codes.A) ||
					InvoiceHeader.JZ_COOLabelType.Contains(CountryOfOriginLabelTypeCodeList.Codes.C) ||
					InvoiceHeader.JZ_COOLabelType.Contains(CountryOfOriginLabelTypeCodeList.Codes.E)))
				{
					InvoiceHeader.JZ_COOLabelTypeInfo.AddMessageError(Res.GetString("1ED8BAA9-8816-4BB1-A549-2EEEDB67FB76", "If Country Of Origin Label Location is 'B', then Country Of Origin Label Type cannot be 'A', 'C' or 'E'"));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.JZ_COOLabelTypeInfo);
			}
		}

		protected override void CheckJZ_DeductionType()
		{
			base.CheckJZ_DeductionType();

			if (IsValidationModeSetFor934 && ValuationCodeList.IsValuationMethodFour(InvoiceHeader.JZ_ValuationCode))
			{
				if (InvoiceHeader.GeneralCost.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfIsEntered(InvoiceHeader.JZ_DeductionTypeInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.JZ_DeductionTypeInfo);
			}
		}

		protected override void CheckJZ_DeductionRate()
		{
			base.CheckJZ_DeductionRate();

			if (IsValidationModeSetFor934 && ValuationCodeList.IsValuationMethodFour(InvoiceHeader.JZ_ValuationCode))
			{
				if (InvoiceHeader.GeneralCost.IsEmpty)
				{
					if (!InvoiceHeader.JZ_DeductionRate.IsEmpty)
					{
						InvoiceHeader.JZ_DeductionRateInfo.AddMessageError(Res.GetString("72C00DBC-E2E6-4365-B831-3E61C6D74CBC", "If General Cost is 0 then Cost Rate must be 0"));
					}
				}
				else
				{
					if (InvoiceHeader.JZ_DeductionRate.IsEmpty && !InvoiceHeader.JZ_DeductionType.IsEmpty)
					{
						InvoiceHeader.JZ_DeductionRateInfo.AddMessageError(Res.GetString("60E48836-95B7-4C1A-90A9-376EC4720852", "You have entered Cost Rate Code without its rate."));
					}
					else if (!InvoiceHeader.JZ_DeductionRate.IsEmpty && InvoiceHeader.JZ_DeductionType.IsEmpty)
					{
						InvoiceHeader.JZ_DeductionRateInfo.AddMessageError(Res.GetString("45664C1D-93C0-4E1D-8682-ADEC387350CE", "You have entered Cost Rate without its rate code."));
					}
				}
			}
		}

		protected override void CheckJZ_ProvAdditionalRateIsValidZDecimal()
		{
			base.CheckJZ_ProvAdditionalRateIsValidZDecimal();
			if (Parent.JZ_ProvAdditionalRate < 0 || Parent.JZ_ProvAdditionalRate > 100)
			{
				Parent.JZ_ProvAdditionalRateInfo.AddMessageError(ResString.GetMultilingualString("B0D93FAD-CCF0-482D-A46F-E56AEC4A8846", "Percentage value should be between 0 and 100."));
			}
		}

		protected override void CheckJZ_SpecificUseProductType()
		{
			base.CheckJZ_SpecificUseProductType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_SpecificUseProductTypeInfo);
		}

		protected override void CheckJZ_ScheduledReExportCustomsOffice()
		{
			base.CheckJZ_ScheduledReExportCustomsOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_ScheduledReExportCustomsOfficeInfo);
		}

		protected override void CheckJZ_JurisdictionalCusOffice()
		{
			base.CheckJZ_JurisdictionalCusOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_JurisdictionalCusOfficeInfo);
		}

		protected override void CheckJZ_RN_NKReExportDestinationCountry()
		{
			base.CheckJZ_RN_NKReExportDestinationCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_RN_NKReExportDestinationCountryInfo);
		}

		public void ValidateValuationQuestion7A_IMP() => ValidateCalculatedProperty(Parent.ValuationQuestion7A_IMPInfo);
		public void ValidateValuationQuestion7B_IMP() => ValidateCalculatedProperty(Parent.ValuationQuestion7B_IMPInfo);
		public void ValidateValuationQuestion7C() => ValidateCalculatedProperty(Parent.ValuationQuestion7CInfo);
		public void ValidateValuationQuestion7D() => ValidateCalculatedProperty(Parent.ValuationQuestion7DInfo);
		public void ValidateValuationQuestion7EA() => ValidateCalculatedProperty(Parent.ValuationQuestion7EAInfo);
		public void ValidateValuationQuestion7EB() => ValidateCalculatedProperty(Parent.ValuationQuestion7EBInfo);
		public void ValidateValuationQuestion8A() => ValidateCalculatedProperty(Parent.ValuationQuestion8AInfo);
		public void ValidateValuationQuestion8B() => ValidateCalculatedProperty(Parent.ValuationQuestion8BInfo);
		public void ValidateValuationQuestion9A() => ValidateCalculatedProperty(Parent.ValuationQuestion9AInfo);
		public void ValidateValuationQuestion9B() => ValidateCalculatedProperty(Parent.ValuationQuestion9BInfo);

		protected void CheckValuationQuestion7A_IMP() => ValidateListIf934ModeEnabled(Parent.ValuationQuestion7A_IMPInfo);
		protected void CheckValuationQuestion7B_IMP()
		{
			if (IsValidationModeSetFor934 && Parent.ValuationQuestion7A_IMP == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion7B_IMPInfo);
			}
		}
		protected void CheckValuationQuestion7C()
		{
			if (IsValidationModeSetFor934 && Parent.ValuationQuestion7A_IMP == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion7CInfo);
			}
		}
		protected void CheckValuationQuestion7D()
		{
			if (IsValidationModeSetFor934 && Parent.ValuationQuestion7A_IMP == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion7DInfo);
			}
		}
		protected void CheckValuationQuestion7EA()
		{
			if (IsValidationModeSetFor934 && Parent.ValuationQuestion7A_IMP == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion7EAInfo);
			}
		}
		protected void CheckValuationQuestion7EB()
		{
			if (IsValidationModeSetFor934 && Parent.ValuationQuestion7A_IMP == YesNoList.Codes.Yes && Parent.ValuationQuestion7EA == PricingCodeList.Codes._99)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ValuationQuestion7EBInfo);
			}
		}
		protected void CheckValuationQuestion8A() => ValidateListIf934ModeEnabled(Parent.ValuationQuestion8AInfo);
		protected void CheckValuationQuestion8B() => ValidateListIf934ModeEnabled(Parent.ValuationQuestion8BInfo);
		protected void CheckValuationQuestion9A() => ValidateListIf934ModeEnabled(Parent.ValuationQuestion9AInfo);
		protected void CheckValuationQuestion9B() => ValidateListIf934ModeEnabled(Parent.ValuationQuestion9BInfo);

		void ValidateListIf934ModeEnabled(ZPropertyInfo propertyInfo)
		{
			if (IsValidationModeSetFor934)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(propertyInfo);
			}
		}
		protected override void CheckJZ_ValuationCode()
		{
			base.CheckJZ_ValuationCode();
			if (IsValidationModeSetFor934 && Parent.JZ_ValuationDecAttachCode == ValueDeclarationAttachedCodeList.Codes.Y)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_ValuationCodeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_ValuationCodeInfo);
		}

		protected override void CheckJZ_ProvPricingYN()
		{
			base.CheckJZ_ProvPricingYN();
			if (IsValidationModeSetFor934)
			{
				var hasValueOfProvisionalPricingReason = Parent.ValuationDeclarationCodes.Where(x => x.CY_Code.StartsWith(PriceDeclarationItemCodeList.StartingDigits.ProvisionalPricingReasonCode)).Any(x => !x.CY_Data.IsEmpty);
				if (Parent.IsProvPricing)
				{
					if (!hasValueOfProvisionalPricingReason)
					{
						Parent.JZ_ProvPricingYNInfo.AddMessageError(SelectProvisionalPricingReasonMessage);
					}
				}
				else
				{
					if (hasValueOfProvisionalPricingReason)
					{
						Parent.JZ_ProvPricingYNInfo.AddMessageError(DoNotSelectProvisionalPricingReasonMessage);
					}
				}
			}
		}
		public static string SelectProvisionalPricingReasonMessage => Res.GetString("50CD28BB-C98E-48D0-A1F0-5C57BF02FAE3", "Please select a Provisional Pricing Reason.");
		public static string DoNotSelectProvisionalPricingReasonMessage => Res.GetString("A251A736-7357-4116-AFCA-51D6ACFA2224", "Do not select a Provisional Pricing Reason.");

		protected override void CheckJZ_EstimatedDateOfFinalPrice()
		{
			base.CheckJZ_EstimatedDateOfFinalPrice();

			if (IsValidationModeSetFor934 && !Parent.IsProvPricing)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JZ_EstimatedDateOfFinalPriceInfo);
			}
		}

		protected override void CheckJZ_ImpContractExpiryDate()
		{
			base.CheckJZ_ImpContractExpiryDate();

			if (IsValidationModeSetFor934 && !Parent.IsProvPricing)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JZ_ImpContractExpiryDateInfo);
			}
		}

		protected override void CheckJZ_ProvAdditionalRate()
		{
			base.CheckJZ_ProvAdditionalRate();

			if (IsValidationModeSetFor934)
			{
				if (!Parent.IsProvPricing)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.JZ_ProvAdditionalRateInfo);
				}
			}
		}

		protected override void CheckJZ_ProvAdditionalAmount()
		{
			base.CheckJZ_ProvAdditionalAmount();

			if (IsValidationModeSetFor934)
			{
				if (!Parent.IsProvPricing)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.JZ_ProvAdditionalAmountInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsNegative(Parent.JZ_ProvAdditionalAmountInfo);
				}
			}
		}

		bool IsValidationModeSetFor934 => InvoiceHeader.JobDeclaration?.IsValidationModeSetFor934 ?? false;
	}
}
