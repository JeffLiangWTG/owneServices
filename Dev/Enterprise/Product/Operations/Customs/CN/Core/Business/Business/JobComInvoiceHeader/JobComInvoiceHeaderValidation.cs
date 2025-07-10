using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class JobComInvoiceHeaderValidation : AutoCNJobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		internal IValidationModeProvider ValidationModeProvider => Parent.JobDeclaration;

		protected override void CheckJZ_SpecialRelationshipConfirm()
		{
			base.CheckJZ_SpecialRelationshipConfirm();
			Parent.JZ_SpecialRelationshipConfirmInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
		}

		protected override void CheckJZ_PriceAffectConfirm()
		{
			base.CheckJZ_PriceAffectConfirm();
			Parent.JZ_PriceAffectConfirmInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
		}

		protected override void CheckJZ_PaymentOfRoyaltyConfirm()
		{
			base.CheckJZ_PaymentOfRoyaltyConfirm();
			Parent.JZ_PaymentOfRoyaltyConfirmInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);

			var invoice = Parent;
			var declaration = Parent.JobDeclaration;
			if (declaration != null && declaration.IsImport && !declaration.WillGenerateBothEntries)
			{
				if (invoice.JZ_PaymentOfRoyaltyConfirm == ConfirmationTypeList.Codes.No)
				{
					if (invoice.FindRoyaltyChargesOnInvoiceOrGroup().Any())
					{
						Parent.JZ_PaymentOfRoyaltyConfirmInfo.AddNotification(RoyaltyPaymentShouldBeYesErrorMsg, ValidationModeProvider);
					}
				}
				else if (invoice.JZ_PaymentOfRoyaltyConfirm == ConfirmationTypeList.Codes.Yes)
				{
					if (invoice.FindRoyaltyChargesOnInvoiceOrGroup().All(c => c.IsEmpty))
					{
						Parent.JZ_PaymentOfRoyaltyConfirmInfo.AddWarning(RoyaltyPaymentShouldBeProvidedWarningMsg);
					}
				}
			}
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();
			var invoice = Parent;
			var targetInfo = invoice.JZ_RX_NKInvoice_CurrencyInfo;
			targetInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			if (!invoice.JZ_RX_NKInvoice_Currency.IsEmpty && CNRefCusCodeListLoader.GetCurrency(invoice.Factory, invoice.JZ_RX_NKInvoice_Currency, invoice.EffectiveValuationDate) == null)
			{
				targetInfo.AddWarning(CurrencyIsNotSupported);
			}
		}

		protected override void CheckJZ_Calc_OFTInInvoiceCurrency()
		{
			base.CheckJZ_Calc_OFTInInvoiceCurrency();

			if ((Parent.JobDeclaration?.TransportDataHelper.IsCrossBorder ?? false) && Parent.JZ_Calc_OFTInInvoiceCurrency.IsEmpty)
			{
				var incoTermCode = ShipmentIncoTerm.MapIncoTermCode(Parent.JZ_IncoTerm);

				if (CustomsFeeCalculator.ShouldPopulateFee(CustomsChargeTypeList.Codes.OverseasFreight, incoTermCode, Parent.JobDeclaration.WillGenerateEnteringEntry, Parent.JobDeclaration.WillGenerateExitingEntry))
				{
					Parent.JZ_Calc_OFTInInvoiceCurrencyInfo.AddNotification(ResString.GetMultilingualString("CAE238B5-7D2B-4D5A-8711-E541C7BAA65D", "You have not enter any Overseas Freight."), ValidationModeProvider);
				}
			}
		}

		protected override void CheckJZ_Calc_ONSInInvoiceCurrency()
		{
			base.CheckJZ_Calc_ONSInInvoiceCurrency();

			if ((Parent.JobDeclaration?.TransportDataHelper.IsCrossBorder ?? false) && Parent.JZ_Calc_ONSInInvoiceCurrency.IsEmpty)
			{
				var incoTermCode = ShipmentIncoTerm.MapIncoTermCode(Parent.JZ_IncoTerm);

				if (CustomsFeeCalculator.ShouldPopulateFee(CustomsChargeTypeList.Codes.OverseasInsurance, incoTermCode, Parent.JobDeclaration.WillGenerateEnteringEntry, Parent.JobDeclaration.WillGenerateExitingEntry))
				{
					Parent.JZ_Calc_ONSInInvoiceCurrencyInfo.AddNotification(ResString.GetMultilingualString("BE558D1B-AB8C-485A-8D1A-6608E4E7760E", "You have not enter any Overseas Insurance."), ValidationModeProvider);
				}
			}
		}

		protected override void CheckJZ_Calc_CIFAmount_ZeroFreightInsurance()
		{
		}

		protected override bool ShouldCheckRelatedHouseBillEntered => false;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateContractNumbersAsString();
			ValidateFormulaPricingConfirm();
			ValidateTemporaryPricingConfirm();
		}

		public void ValidateContractNumbersAsString()
		{
			ValidateCalculatedProperty(Parent.ContractNumbersAsStringInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		void CheckContractNumbersAsString()
		{
			if (Parent.ContractNumbersAsString.Length > 32)
			{
				Parent.ContractNumbersAsStringInfo.AddWarning(ContractNumberMaxLengthExceeded);
			}
		}

		public void ValidateFormulaPricingConfirm()
		{
			ValidateCalculatedProperty(Parent.JZ_Calc_FormulaPricingConfirmInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		void CheckJZ_Calc_FormulaPricingConfirm()
		{
			var targetPropertyInfo = Parent.JZ_Calc_FormulaPricingConfirmInfo;
			var formulaPricingConfirm = Parent.JZ_Calc_FormulaPricingConfirm;

			targetPropertyInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
			if (formulaPricingConfirm == ConfirmationTypeList.Codes.Yes)
			{
				if (Parent.JZ_Calc_TemporaryPricingConfirm == ConfirmationTypeList.Codes.Yes && !Parent.AnyLineHasFormulaPricingRecordNumber)
				{
					targetPropertyInfo.AddNotification(FormulaPricingRecordNumberShouldExist, ValidationModeProvider);
				}
			}
			else
			{
				if (Parent.AnyLineHasFormulaPricingRecordNumber)
				{
					targetPropertyInfo.AddNotification(FormulaPricingConfirmShouldBeYes, ValidationModeProvider);
				}
			}
		}

		public void ValidateTemporaryPricingConfirm()
		{
			ValidateCalculatedProperty(Parent.JZ_Calc_TemporaryPricingConfirmInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		void CheckJZ_Calc_TemporaryPricingConfirm()
		{
			var temporaryPricingConfirmInfo = Parent.JZ_Calc_TemporaryPricingConfirmInfo;
			temporaryPricingConfirmInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);

			var formulaConfirm = Parent.JZ_Calc_FormulaPricingConfirm;
			var temporaryConfirm = Parent.JZ_Calc_TemporaryPricingConfirm;

			if (formulaConfirm == ConfirmationTypeList.Codes.Yes && temporaryConfirm == ConfirmationTypeList.Codes.Uncertain)
			{
				temporaryPricingConfirmInfo.AddNotification(TemporaryPricingConfirmShouldNotBeUncertain, ValidationModeProvider);
			}
			if (formulaConfirm != ConfirmationTypeList.Codes.Yes && temporaryConfirm != ConfirmationTypeList.Codes.Uncertain)
			{
				temporaryPricingConfirmInfo.AddNotification(TemporaryPricingConfirmShouldBeUncertain, ValidationModeProvider);
			}

			if (temporaryConfirm != ConfirmationTypeList.Codes.Yes)
			{
				if (Parent.AnyLineHasFormulaPricingRecordNumber)
				{
					temporaryPricingConfirmInfo.AddNotification(TemporaryPricingConfirmShouldBeYes, ValidationModeProvider);
				}
			}
		}

		#region Error Messages

		internal static MultilingualString ContractNumberMaxLengthExceeded
			=> ResString.GetMultilingualString("663AA7BC-C9EC-411B-812F-7C09FA457E09", "The total length of the Contract Numbers exceeds 32, some of the Contract Numbers might not be able to be send to the Customs.");

		internal static MultilingualString CurrencyIsNotSupported
			=> ResString.GetMultilingualString("D7D5269B-D19A-402E-ABE4-46A0DD5E2EF4", "The selected currency is not a currency supported by China Customs, it will be declared in CNY.");

		internal static MultilingualString TemporaryPricingConfirmShouldNotBeUncertain
			=> ResString.GetMultilingualString("0607BFB1-D7F6-4C91-A5D6-B2A7D97BA636", "Temporary Pricing Confirm should not be '{0}' when Formula Pricing Confirm is '{1}'.", ConfirmationTypeList.Descriptions.Uncertain, ConfirmationTypeList.Descriptions.Yes);

		internal static MultilingualString TemporaryPricingConfirmShouldBeUncertain
			=> ResString.GetMultilingualString("6EB62548-9E07-4937-B4E7-598E7391AE44", "Temporary Pricing Confirm should be '{0}' when Formula Pricing Confirm is not '{1}'.", ConfirmationTypeList.Descriptions.Uncertain, ConfirmationTypeList.Descriptions.Yes);

		internal static MultilingualString FormulaPricingRecordNumberShouldExist
			=> ResString.GetMultilingualString("0E191CBB-54B2-4976-90F1-9FAE7D01F0E6",
				"Please enter Formula Pricing Record Number on formula priced Invoice Lines.");

		internal static MultilingualString FormulaPricingConfirmShouldBeYes
			=> ResString.GetMultilingualString("AE8E4FCA-9B4E-4987-BD2E-69B8338F8E4F", "Some Invoice Lines have Formula Pricing Record Number entered, please consider change Formula Pricing Confirm to '{0}'.", ConfirmationTypeList.Descriptions.Yes);

		internal static MultilingualString TemporaryPricingConfirmShouldBeYes
			=> ResString.GetMultilingualString("FDB0BC1C-A124-4E54-9357-B9F61BD7E253", "Some Invoice Lines have Formula Pricing Record Number entered, please consider change Temporary Pricing Confirm to '{0}'.", ConfirmationTypeList.Descriptions.Yes);

		internal static MultilingualString RoyaltyPaymentShouldBeYesErrorMsg
			=> ResString.GetMultilingualString("6902daa9-9f64-4347-b3ad-57a35db36ad7", "Payment of Royalty Confirm should be ‘{0}’ if there are any royalty charges on this invoice.", ConfirmationTypeList.Descriptions.Yes);

		internal static MultilingualString RoyaltyPaymentShouldBeProvidedWarningMsg
			=> ResString.GetMultilingualString("7354091e-300f-4a18-b3a1-08ee1e710149", "Please provide the royalty as RYT charge on the invoice, unless it is included in invoice amount or has already been declared separately.");

		#endregion
	}
}
