using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	sealed class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		public void TestCheckJZ_SpecialRelationshipConfirm()
		{
			var invoice = Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew();
			AssertConfirmPropertyListValidation(invoice.JZ_SpecialRelationshipConfirmInfo);
		}

		public void TestCheckJZ_PriceAffectConfirm()
		{
			var invoice = Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew();
			AssertConfirmPropertyListValidation(invoice.JZ_PriceAffectConfirmInfo);
		}

		public void TestCheckJZ_PaymentOfRoyaltyConfirm()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			AssertConfirmPropertyListValidation(invoice.JZ_PaymentOfRoyaltyConfirmInfo);
			var propertyInfo = invoice.JZ_PaymentOfRoyaltyConfirmInfo;
			propertyInfo.Value = (ZString)ConfirmationTypeList.Codes.No;
			var charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.Royalty, 100m);
			invoice.Validation.ValidateJZ_PaymentOfRoyaltyConfirm();
			AssertHasMessageError(invoice.JZ_PaymentOfRoyaltyConfirmInfo, JobComInvoiceHeaderValidation.RoyaltyPaymentShouldBeYesErrorMsg.ToString());
			propertyInfo.Value = (ZString)ConfirmationTypeList.Codes.Yes;
			invoice.Validation.ValidateJZ_PaymentOfRoyaltyConfirm();
			AssertNoMessageError(invoice.JZ_PaymentOfRoyaltyConfirmInfo, JobComInvoiceHeaderValidation.RoyaltyPaymentShouldBeYesErrorMsg.ToString());
			AssertNoWarning(invoice.JZ_PaymentOfRoyaltyConfirmInfo, JobComInvoiceHeaderValidation.RoyaltyPaymentShouldBeProvidedWarningMsg.ToString());
			charge.Delete();
			invoice.Validation.ValidateJZ_PaymentOfRoyaltyConfirm();
			AssertHasWarning(invoice.JZ_PaymentOfRoyaltyConfirmInfo, JobComInvoiceHeaderValidation.RoyaltyPaymentShouldBeProvidedWarningMsg.ToString());
		}

		public void TestCheckContractNumbersAsString()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var ref1 = Factory.New<JobComInvoiceHeaderContract>();
			ref1.J2_ReferenceType = "CTR";
			ref1.J2_ReferenceNumber = "ABC";
			ref1.J2_JZ = invoice.PK;
			var ref2 = Factory.New<JobComInvoiceHeaderContract>();
			ref2.J2_ReferenceType = "CTR";
			ref2.J2_ReferenceNumber = "DE12345678901234567980123456789";
			ref2.J2_JZ = invoice.PK;
			AssertEquals(2, invoice.ContractNumbers.Count);
			AssertEquals("ABC,DE12345678901234567980123456789", invoice.ContractNumbersAsString);
			invoice.Validation.ValidateContractNumbersAsString();
			AssertHasWarningContaining(invoice.ContractNumbersAsStringInfo, "The total length of the Contract Numbers exceeds 32, some of the Contract Numbers might not be able to be send to the Customs.");
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJZ_RX_NKInvoice_Currency()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CURR", "Currencies");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CURR", "AAA", "test currency", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			Factory.Save();
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var testInfo = invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo;
			invoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertHasMessageErrorContaining(testInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(testInfo, JobComInvoiceHeaderValidation.CurrencyIsNotSupported.GetUnresolvedString());
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			AssertHasWarningContaining(testInfo, JobComInvoiceHeaderValidation.CurrencyIsNotSupported.GetUnresolvedString());
			invoiceHeader.JZ_RX_NKInvoice_Currency = "AAA";
			AssertNoWarningContaining(testInfo, JobComInvoiceHeaderValidation.CurrencyIsNotSupported.GetUnresolvedString());
		}

		public void TestCheckJZ_Calc_OFTInInvoiceCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKPortOfArrival = "CNBJS";
			declaration.JE_RL_NKPortOfLoading = "CNBJS";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoice.Validation.ValidateJZ_Calc_OFTInInvoiceCurrency();
			AssertNoMessageErrorContaining(invoice.JZ_Calc_OFTInInvoiceCurrencyInfo, "You have not enter any Overseas Freight.");
			declaration.JE_RL_NKPortOfLoading = "GBLON";
			invoice.Validation.ValidateJZ_Calc_OFTInInvoiceCurrency();
			AssertHasMessageErrorContaining(invoice.JZ_Calc_OFTInInvoiceCurrencyInfo, "You have not enter any Overseas Freight.");
			declaration.JE_RL_NKPortOfLoading = "CNBJS";
			invoice.JZ_IncoTerm = "CIF";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoice.Validation.ValidateJZ_Calc_OFTInInvoiceCurrency();
			AssertNoMessageErrorContaining(invoice.JZ_Calc_OFTInInvoiceCurrencyInfo, "You have not enter any Overseas Freight.");
			declaration.JE_RL_NKPortOfLoading = "GBLON";
			invoice.Validation.ValidateJZ_Calc_OFTInInvoiceCurrency();
			AssertHasMessageErrorContaining(invoice.JZ_Calc_OFTInInvoiceCurrencyInfo, "You have not enter any Overseas Freight.");
		}

		public void TestCheckJZ_Calc_ONSInInvoiceCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKPortOfArrival = "CNBJS";
			declaration.JE_RL_NKPortOfLoading = "CNBJS";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoice.Validation.ValidateJZ_Calc_ONSInInvoiceCurrency();
			AssertNoMessageErrorContaining(invoice.JZ_Calc_ONSInInvoiceCurrencyInfo, "You have not enter any Overseas Insurance.");
			declaration.JE_RL_NKPortOfLoading = "GBLON";
			invoice.Validation.ValidateJZ_Calc_ONSInInvoiceCurrency();
			AssertHasMessageErrorContaining(invoice.JZ_Calc_ONSInInvoiceCurrencyInfo, "You have not enter any Overseas Insurance.");
			declaration.JE_RL_NKPortOfLoading = "CNBJS";
			invoice.JZ_IncoTerm = "CIF";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoice.Validation.ValidateJZ_Calc_ONSInInvoiceCurrency();
			AssertNoMessageErrorContaining(invoice.JZ_Calc_ONSInInvoiceCurrencyInfo, "You have not enter any Overseas Insurance.");
			declaration.JE_RL_NKPortOfLoading = "GBLON";
			invoice.Validation.ValidateJZ_Calc_ONSInInvoiceCurrency();
			AssertHasMessageErrorContaining(invoice.JZ_Calc_ONSInInvoiceCurrencyInfo, "You have not enter any Overseas Insurance.");
		}

		public void TestCheckPricingConfirms()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var formulaPrcicingInfo = invoice.JZ_Calc_FormulaPricingConfirmInfo;
			var temporaryPricingInfo = invoice.JZ_Calc_TemporaryPricingConfirmInfo;

			invoice.JZ_Calc_FormulaPricingConfirm = invoice.JZ_Calc_TemporaryPricingConfirm = string.Empty;
			invoice.Validation.ValidateAll();
			AssertHasMessageErrorContaining(formulaPrcicingInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(temporaryPricingInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_Calc_FormulaPricingConfirm = invoice.JZ_Calc_TemporaryPricingConfirm = "X";
			AssertHasMessageErrorContaining(formulaPrcicingInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(temporaryPricingInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.Yes;
			invoice.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.Uncertain;
			AssertHasMessageErrorContaining(temporaryPricingInfo, "Temporary Pricing Confirm should not be '空' when Formula Pricing Confirm is '是'.");
			invoice.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.No;
			AssertNoMessageErrors(temporaryPricingInfo);
			invoice.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.Yes;
			AssertNoMessageErrors(temporaryPricingInfo);

			invoice.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.No;
			AssertHasMessageErrorContaining(temporaryPricingInfo, "Temporary Pricing Confirm should be '空' when Formula Pricing Confirm is not '是'.");
			invoice.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.Uncertain;
			AssertHasMessageErrorContaining(temporaryPricingInfo, "Temporary Pricing Confirm should be '空' when Formula Pricing Confirm is not '是'.");
			invoice.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.Yes;
			AssertNoMessageErrors(temporaryPricingInfo);
		}

		public void TestCheckFormulaPricingConfirmsUsingFormulaPricingRecordNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var formulaPricingInfo = invoice.JZ_Calc_FormulaPricingConfirmInfo;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.FormulaPricingRecordNumber = ZString.Empty;
			invoice.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.Yes;
			invoice.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.Yes;
			AssertHasMessageErrorContaining(formulaPricingInfo, "Please enter Formula Pricing Record Number on formula priced Invoice Lines.");
			invoice.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.No;
			invoice.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.Yes;
			AssertNoMessageErrorContaining(formulaPricingInfo, "Please enter Formula Pricing Record Number on formula priced Invoice Lines.");
			invoice.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.Yes;
			invoice.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.No;
			AssertNoMessageErrorContaining(formulaPricingInfo, "Please enter Formula Pricing Record Number on formula priced Invoice Lines.");
			invoice.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.No;
			invoice.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.No;
			AssertNoMessageErrorContaining(formulaPricingInfo, "Please enter Formula Pricing Record Number on formula priced Invoice Lines.");
			invoiceLine.FormulaPricingRecordNumber = "999";
			invoice.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.Yes;
			invoice.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.Yes;
			AssertNoMessageErrorContaining(formulaPricingInfo, "Please enter Formula Pricing Record Number on formula priced Invoice Lines.");
			invoice.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.Yes;
			AssertNoMessageErrorContaining(formulaPricingInfo, "Some Invoice Lines have Formula Pricing Record Number entered, please consider change Formula Pricing Confirm to '是'.");
			invoice.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.No;
			AssertHasMessageErrorContaining(formulaPricingInfo, "Some Invoice Lines have Formula Pricing Record Number entered, please consider change Formula Pricing Confirm to '是'.");
			invoiceLine.FormulaPricingRecordNumber = ZString.Empty;
			AssertNoMessageErrorContaining(formulaPricingInfo, "Some Invoice Lines have Formula Pricing Record Number entered, please consider change Formula Pricing Confirm to '是'.");
		}

		public void TestCheckTemporaryPricingConfirmsUsingFormulaPricingRecordNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var temporaryPricingInfo = invoice.JZ_Calc_TemporaryPricingConfirmInfo;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.FormulaPricingRecordNumber = "999";
			invoice.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.Yes;
			AssertNoMessageErrorContaining(temporaryPricingInfo, "Some Invoice Lines have Formula Pricing Record Number entered, please consider change Temporary Pricing Confirm to '是'.");
			invoice.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.No;
			AssertHasMessageErrorContaining(temporaryPricingInfo, "Some Invoice Lines have Formula Pricing Record Number entered, please consider change Temporary Pricing Confirm to '是'.");
			invoiceLine.FormulaPricingRecordNumber = ZString.Empty;
			AssertNoMessageErrorContaining(temporaryPricingInfo, "Some Invoice Lines have Formula Pricing Record Number entered, please consider change Temporary Pricing Confirm to '是'.");
		}

		public void TestValidationModeProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			ValidationExtensionsTest.AssertValidationModeProvider(declaration, invoice.Validation.ValidationModeProvider);

			invoice = Factory.New<JobComInvoiceHeader>();
			AssertNull(invoice.Validation.ValidationModeProvider);
		}

		public override void TestValidateAbsenceOfOFTOrONS() => Assert(true);

		protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);

		static void AssertConfirmPropertyListValidation(ZPropertyInfo propertyInfo)
		{
			propertyInfo.Value = (ZString)ConfirmationTypeList.Codes.Uncertain;
			AssertNoNotifications("Initial", propertyInfo);
			propertyInfo.Value = (ZString)"X";
			AssertHasMessageErrorContaining(propertyInfo, ListValidation.InvalidCodeMessageError);
			propertyInfo.Value = (ZString)ConfirmationTypeList.Codes.No;
			AssertNoNotifications("Valid: NoMessageError", propertyInfo);
			propertyInfo.Value = ZString.Empty;
			AssertHasMessageErrorContaining(propertyInfo, "entered");
		}
	}
}
