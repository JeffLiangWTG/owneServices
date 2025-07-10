using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobComInvoiceHeaderValidation))]
sealed class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
{
	[SnailTest]
	public void TestCheckMaxInvoiceLines()
	{
		var messageError = "Maximum 9999 Invoice Lines are allowed under each invoice.";
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLines = invoice.JobComInvoiceLines;
		for (var i = 0; i < 9999; i++)
		{
			invoiceLines.AddNew();
		}
		CombineAssertions(() =>
		{
			AssertNoRowMessageError(invoice, messageError);
			var line = invoiceLines.AddNew();
			invoice.RunPreSaveValidation();
			AssertHasRowMessageError(invoice, messageError);
			invoiceLines.Remove(line);
			invoice.RunPreSaveValidation();
			AssertNoRowMessageError(invoice, messageError);
		});
	}

	protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);

	public void TestIncoTermIsRequiredMessage()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_IncoTermInfo);
	}

	public void TestCheckJZ_PaymentDays()
	{
		var messageError = "Payment days should be less than 180 days.";
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertNoMessageErrors("When Import", invoice.JZ_PaymentDaysInfo);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertNoMessageError("When Empty", invoice.JZ_PaymentDaysInfo, messageError);
			invoice.JZ_PaymentDays = -1;
			AssertHasErrorContaining(invoice.JZ_PaymentDaysInfo, MandatoryValidation.ValueCannotBeNegative);
			invoice.JZ_PaymentDays = 180;
			AssertHasMessageError("When Payment Days Greater Equal 180", invoice.JZ_PaymentDaysInfo, messageError);
			invoice.JZ_PaymentDays = 179;
			AssertNoMessageError("When Payment Days Less Than 179", invoice.JZ_PaymentDaysInfo, messageError);
			AssertNoErrorContaining(invoice.JZ_PaymentDaysInfo, MandatoryValidation.ValueCannotBeNegative);
		});
	}

	public void TestCheckJZ_InvoiceNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoice.JZ_InvoiceNumberInfo);
	}

	public void TestCheckJZ_InvoiceDate()
	{
		var message = "You have not entered an Invoice Date.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceDate = ZDateTime.Empty;
		AssertNoMessageError("When InvoiceNumber and InvoiceDate both empty", invoice.JZ_InvoiceDateInfo, message);
		invoice.JZ_InvoiceNumber = "123";
		invoice.Validation.ValidateJZ_InvoiceDate();
		AssertHasMessageError("When InvoiceNumber not empty and InvoiceDate empty", invoice.JZ_InvoiceDateInfo, message);
		invoice.JZ_InvoiceDate = ZDateTime.Today;
		AssertNoMessageError("When InvoiceNumber and InvoiceDate both not empty", invoice.JZ_InvoiceDateInfo, message);
	}

	public void TestCheckJZ_RX_NKInvoice_Currency_IsNonStandardCurrency()
	{
		var warning = "You have selected a Non-Standard Currency.";
		var standardCurrencyCode = RefDataSetupTestHelper.SetupStandardCurrencyCodes(Factory);
		var nonStandardCurrencyCode = "MDD";
		RefCurrency.New(Factory).RX_Code = nonStandardCurrencyCode;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();

		CombineAssertions(() =>
		{
			AssertNoWarning("When Empty", invoice.JZ_RX_NKInvoice_CurrencyInfo, warning);
			invoice.JZ_RX_NKInvoice_Currency = standardCurrencyCode;
			AssertNoWarning("When valid value", invoice.JZ_RX_NKInvoice_CurrencyInfo, warning);
			invoice.JZ_RX_NKInvoice_Currency = nonStandardCurrencyCode;
			AssertHasWarning("When invalid value", invoice.JZ_RX_NKInvoice_CurrencyInfo, warning);
		});
	}

	public void TestCheckJZ_RX_NKInvoice_Currency()
	{
		var messageErrorForStandardCurrency = "There is no valid exchange rate for this currency";
		var messageError = "You have not entered a valid exchange rate for selected Non-Standard currency. Please update the exchange rate under Misc. tab.";
		var standardCurrencyCode = RefDataSetupTestHelper.SetupStandardCurrencyCodes(Factory);
		var nonStandardCurrencyCode = "MDD";
		RefCurrency.New(Factory).RX_Code = nonStandardCurrencyCode;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();

		invoice.JZ_RX_NKInvoice_Currency = standardCurrencyCode;
		AssertHasMessageErrorContaining("Standard Currency", invoice.JZ_RX_NKInvoice_CurrencyInfo, messageErrorForStandardCurrency);
		AssertNoMessageError("Standard Currency", invoice.JZ_RX_NKInvoice_CurrencyInfo, messageError);

		invoice.JZ_RX_NKInvoice_Currency = nonStandardCurrencyCode;
		AssertNoMessageErrorContaining("Non-Standard Currency", invoice.JZ_RX_NKInvoice_CurrencyInfo, messageErrorForStandardCurrency);
		AssertHasMessageError("Non-Standard Currency and Exchange Rate is 0", invoice.JZ_RX_NKInvoice_CurrencyInfo, messageError);

		declaration.NonStandardExchangeRates.GetByCurrencyCode(nonStandardCurrencyCode).CSI_Value = 1;
		AssertNoMessageError("When Non-Standard Currency and Exchange Rate is not 0", invoice.JZ_RX_NKInvoice_CurrencyInfo, messageError);
	}

	public void TestCheckJZ_GSTPaymentStatus()
	{
		var invoice = Factory.New<JobComInvoiceHeader>();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoice.JZ_GSTPaymentStatusInfo);
	}

	public void TestAutherizedEconomicOperatorRole()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		const string messageErrorAEORole = "You have not entered an AEO Role";

		CombineAssertions("JZ_AuthorizedEconomicOperatorRole validation check", () =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.AuthorizedEconomicOperatorAddress.OrganisationPK = ZGuid.BrettsGuid;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoice.JZ_AuthorizedEconomicOperatorRoleInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.AuthorizedEconomicOperatorAddress.OrganisationPK = ZGuid.Empty;
			invoice.JZ_AuthorizedEconomicOperatorRole = "ABC";
			AssertNoMessageError("AEO empty, AEO Role not empty", invoice.JZ_AuthorizedEconomicOperatorRoleInfo, messageErrorAEORole);

			invoice.AuthorizedEconomicOperatorAddress.OrganisationPK = ZGuid.BrettsGuid;
			invoice.JZ_AuthorizedEconomicOperatorRole = ZString.Empty;
			AssertHasMessageError("AEO not empty, AEO Role empty", invoice.JZ_AuthorizedEconomicOperatorRoleInfo, messageErrorAEORole);

			invoice.AuthorizedEconomicOperatorAddress.OrganisationPK = ZGuid.Empty;
			invoice.Validation.ValidateJZ_AuthorizedEconomicOperatorRole();
			AssertNoMessageError("AEO empty, AEO Role empty", invoice.JZ_AuthorizedEconomicOperatorRoleInfo, messageErrorAEORole);
		});
	}
}

