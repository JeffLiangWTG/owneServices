using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing;

[TestedType(typeof(JobComInvoiceLineValidation))]
sealed class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckJI_CEI()
	{
		var expectedMessageError = "You have not entered the Entry Instruction.";

		var info = invoiceLine.JI_CEIInfo;
		AssertNoMessageError(info, expectedMessageError);

		invoiceLine.JI_CEI = ZGuid.Empty;
		AssertHasMessageError(info, expectedMessageError);

		declaration.MakeNonPersistent();
		invoiceLine.Validation.ValidateJI_CEI();
		AssertNoMessageError(info, expectedMessageError);

		var standAlongInvoiceLine = Factory.New<JobComInvoiceLine>();
		AssertNoMessageError(standAlongInvoiceLine.JI_CEIInfo, expectedMessageError);
	}

	public void TestCheckJI_DutyReductionExemptionRefundCode()
	{
		Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DutyExemptionCode, "D1", "D2");
		Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DutyExemptionRefundCode, "DE1", "DE2");
		Factory.Save();

		var targetInfo = invoiceLine.JI_DutyReductionExemptionRefundCodeInfo;
		declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "D0", "D1");

		declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "DE0", "DE1");
	}

	public void TestCheckJI_DutyReductionAmount()
	{
		declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
		var targetInfo = invoiceLine.JI_DutyReductionAmountInfo;
		var expectedErrorMessage = "Duty Reduction Amount should only be entered if Duty Reduction or Exemption Code is not empty.";

		invoiceLine.JI_DutyReductionAmount = 1;
		AssertHasMessageError(targetInfo, expectedErrorMessage);

		invoiceLine.JI_DutyReductionExemptionRefundCode = "DE1";
		invoiceLine.Validation.ValidateJI_DutyReductionAmount();
		AssertNoMessageError(targetInfo, expectedErrorMessage);
	}

	public void TestCheckJI_Tariff_ShouldEnter()
	{
		var targetInfo = invoiceLine.JI_TariffInfo;
		var expectedErrorMessage = "You have not entered Tariff.";

		declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
		invoiceLine.Validation.ValidateJI_Tariff();
		AssertHasMessageError(targetInfo, expectedErrorMessage);
		invoiceLine.JI_Tariff = "123";
		AssertNoMessageError(targetInfo, expectedErrorMessage);

		instruction.CEI_ValueType = ValueTypeList.Codes.L;
		declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;

		AssertNotEnteredTariff(JPExportDeclarationTypeList.Codes.T);
		AssertNotEnteredTariff(JPExportDeclarationTypeList.Codes.E);
		AssertNotEnteredTariff(JPExportDeclarationTypeList.Codes.N);
		AssertNotEnteredTariff(JPExportDeclarationTypeList.Codes.M);
		AssertNotEnteredTariff(JPExportDeclarationTypeList.Codes.R);

		void AssertNotEnteredTariff(string declarationType)
		{
			instruction.CEI_Style = declarationType;
			invoiceLine.JI_Tariff = string.Empty;
			AssertHasMessageError(targetInfo, expectedErrorMessage);
			invoiceLine.JI_Tariff = "123";
			AssertNoMessageError(targetInfo, expectedErrorMessage);
		}
	}

	public void TestCheckJI_Tariff_Common()
	{
		CreateTariffData(Universal.Constants.TariffTypes.Export);
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_TariffInfo, new ZString[] { "4321", "654321", "987654321" }, new ZString[] { "1234", "123456", "123456789" }, "The entered Tariff does not exist. Please select one from the list provided.");
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_TariffInfo, "111111111", "123456789", "Please select a root level tariff code.");
	}

	public void TestCheckJI_Tariff_Import()
	{
		CreateTariffData(Universal.Constants.TariffTypes.Import);
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_CEI = entryInstruction.PK;
		declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
		entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;

		invoiceLine.JI_Tariff = "123456789";
		AssertHasMessageError(invoiceLine.JI_TariffInfo, "Tariff must be 6 characters long.");

		invoiceLine.JI_Tariff = "123456";
		AssertNoMessageError(invoiceLine.JI_TariffInfo, "Tariff must be 6 characters long.");

		entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.H;
		invoiceHeader.JZ_InvoiceAmount = 201000m;
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
		invoiceLine.Validation.ValidateJI_Tariff();
		AssertNoMessageError(invoiceLine.JI_TariffInfo, "Tariff must be 9 characters long.");

		invoiceHeader2.JZ_InvoiceAmount = 1m;
		invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
		invoiceLine.Validation.ValidateJI_Tariff();
		AssertHasMessageError(invoiceLine.JI_TariffInfo, "Tariff must be 9 characters long.");

		invoiceLine.JI_Tariff = "123456789";
		AssertNoMessageError(invoiceLine.JI_TariffInfo, "Tariff must be 9 characters long.");

		entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.B;
		invoiceLine.JI_Tariff = "123456";
		AssertHasMessageError(invoiceLine.JI_TariffInfo, "Tariff must be 9 characters long.");

		invoiceLine.JI_Tariff = "123456789";
		AssertNoMessageError(invoiceLine.JI_TariffInfo, "Tariff must be 9 characters long.");
	}

	public void TestCheckJI_Tariff_Export()
	{
		CreateTariffData(Universal.Constants.TariffTypes.Export);
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		instruction.CEI_ValueType = ValueTypeList.Codes.L;

		invoiceLine.JI_Tariff = "1234";
		AssertHasMessageError(invoiceLine.JI_TariffInfo, "Tariff must be 9 characters long.");

		invoiceLine.JI_Tariff = "123456789";
		AssertNoMessageError(invoiceLine.JI_TariffInfo, "Tariff must be 9 characters long.");
		AssertNoMessageError(invoiceLine.JI_TariffInfo, "Tariff must be 4 or 9 characters long.");

		instruction.CEI_ValueType = ValueTypeList.Codes.S;
		invoiceLine.JI_Tariff = "123456";
		AssertHasMessageError(invoiceLine.JI_TariffInfo, "Tariff must be 4 or 9 characters long.");
	}

	public void TestCheckJI_Description()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var targetInfo = invoiceLine.JI_DescriptionInfo;
		var errorMessage = string.Format("You have not entered {0}.", targetInfo.HumanReadableName);

		instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
		AssertDescriptionShouldNotBeEmpty();

		invoiceLine.JI_Tariff = "123456";
		instruction.CEI_Style = JPImportDeclarationTypeList.Codes.H;
		AssertDescriptionShouldNotBeEmpty();

		instruction.CEI_Style = JPImportDeclarationTypeList.Codes.N;
		AssertDescriptionShouldNotBeEmpty();

		declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
		instruction.CEI_Style = string.Empty;
		invoiceLine.JI_Tariff = "1234567890";
		AssertDescriptionShouldNotBeEmpty();

		invoiceLine.JI_Tariff = "1234";
		AssertDescriptionShouldNotBeEmpty();

		void AssertDescriptionShouldNotBeEmpty()
		{
			invoiceLine.JI_Description = string.Empty;
			AssertHasMessageError(targetInfo, errorMessage);
			invoiceLine.JI_Description = "TestDescription";
			AssertNoMessageError(targetInfo, errorMessage);
		}
	}

	public void TestCertificateOfOriginValidation()
	{
		var expectedErrorMessage = "Required if you want to generate the Customs Declaration Message (IDA).";

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		instruction.CEI_Style = JPImportDeclarationTypeList.Codes.H;
		invoiceLine.JI_Procedure = ZString.Empty;
		AssertNoMessageError("Certificate of Origin ID is not required for Dec Type H, N, or Y", invoiceLine.JI_ProcedureInfo, expectedErrorMessage);

		instruction.CEI_Style = JPImportDeclarationTypeList.Codes.F;
		invoiceLine.JI_Procedure = ZString.Empty;
		AssertHasMessageError("Certificate of Origin ID is mandatory for Non Dec Types H, N, or Y", invoiceLine.JI_ProcedureInfo, expectedErrorMessage);

		invoiceLine.JI_Procedure = "WKAS";
		AssertNoMessageError("Certificate of Origin ID is mandatory for Non Dec Types H, N, or Y", invoiceLine.JI_ProcedureInfo, expectedErrorMessage);
	}

	public void TestCheckJI_Calc_Preference()
	{
		PrepareCodes();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_Calc_PreferenceInfo, "WK", "SG");
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_Calc_PreferenceInfo);
	}

	public void TestCheckJI_Calc_OriginCertifier()
	{
		PrepareCodes();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_Calc_OriginCertifierInfo, "0", "T");
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_Calc_OriginCertifierInfo);
	}

	public void TestCheckJI_Calc_CertificateOfOriginCertifier()
	{
		PrepareCodes();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceLine.JI_Calc_Preference = "SG";

		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_Calc_CertificateOfOriginCertifierInfo, "0", "7");
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_Calc_CertificateOfOriginCertifierInfo);
	}

	[TestDate(2023, 07, 01)]
	public void TestCheckJI_ConcessionOrder()
	{
		CreateTariffDataForConcessionOrder();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceLine.JI_Tariff = "1101";
		invoiceLine.JI_PrimaryPreference = "GEN";

		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_ConcessionOrderInfo, "N", "Y");
	}

	public void TestValidateJI_CountryOfOrigin()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Japan;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOrigin, invoiceLine.JI_CountryOfOriginInfo, "Goods Origin cannot be JP or ZY.");
			invoiceLine.JI_CountryOfOrigin = Common.Constants.CountryCodes.UnknownCountryCode;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOrigin, invoiceLine.JI_CountryOfOriginInfo, "Goods Origin cannot be JP or ZY.");
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoNotifications(invoiceLine.JI_CountryOfOriginInfo);

			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CountryOfOriginInfo, "XX", "US");
		});
	}

	public void TestCheckJI_InvoiceUQ()
	{
		ValidationTestHelper.AssertErrorIfInvalidCode(invoiceLine.JI_InvoiceUQInfo, "XX", "KG");

		var expectedMessage = "You have not entered an Invoice Quantity Unit when Invoice Quantity is entered.";
		invoiceLine.JI_InvoiceQuantity = 100m;
		invoiceLine.JI_InvoiceUQ = ZString.Empty;
		AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, expectedMessage);

		invoiceLine.JI_InvoiceUQ = "KG";
		AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, expectedMessage);

		invoiceLine.JI_InvoiceQuantity = 0m;
		invoiceLine.JI_InvoiceUQ = ZString.Empty;
		AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, expectedMessage);
	}

	public void TestCheckJI_Volume()
	{
		ValidationTestHelper.AssertErrorIfValueIsNegative(invoiceLine.JI_VolumeInfo);
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceLine.JI_VolumeInfo, invoiceLine.JI_VolumeUQInfo);
	}

	public void TestCheckJI_Weight()
	{
		ValidationTestHelper.AssertErrorIfValueIsNegative(invoiceLine.JI_WeightInfo);
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceLine.JI_WeightInfo, invoiceLine.JI_WeightUQInfo);
	}

	public void TestCheckJI_WeightUQ()
	{
		ValidationTestHelper.AssertErrorIfInvalidCode(invoiceLine.JI_WeightUQInfo, "XX", "KG");
	}

	public void TestCheckUnitPrice()
	{
		invoiceLine.UnitPrice = -100m;
		invoiceLine.Validation.ValidateUnitPrice();
		AssertHasErrorContaining(invoiceLine.UnitPriceInfo, "cannot be negative.");

		invoiceLine.UnitPrice = 100m;
		invoiceLine.Validation.ValidateUnitPrice();
		AssertNoErrorContaining(invoiceLine.UnitPriceInfo, "cannot be negative.");
	}

	public void TestCheckJI_InvoiceQuantity()
	{
		ValidationTestHelper.AssertErrorIfValueIsNegative(invoiceLine.JI_InvoiceQuantityInfo);
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceLine.JI_InvoiceQuantityInfo, invoiceLine.JI_InvoiceUQInfo);
	}

	public void TestCheckJI_LinePrice()
	{
		ValidationTestHelper.AssertErrorIfValueIsNegative(invoiceLine.JI_LinePriceInfo);
	}

	public void TestCheckJI_CustomsQuantity()
	{
		var targetInfo = invoiceLine.JI_CustomsQuantityInfo;
		var errorMessage = "the maximum value allowed for Customs Quantity 1 is 999,999,999.99.";
		invoiceLine.JI_CustomsQuantity = 1000000000m;
		AssertHasErrorContaining("value's precision is 10", targetInfo, errorMessage);

		invoiceLine.JI_CustomsQuantity = 999999999m;
		AssertNoErrorContaining("value's precision is 9", targetInfo, errorMessage);

		ValidationTestHelper.AssertErrorIfValueIsNegative(invoiceLine.JI_CustomsQuantityInfo);
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceLine.JI_CustomsQuantityInfo, invoiceLine.JI_CustomsUnitQtyInfo);
	}

	public void TestCheckJI_CustomsUnitQty()
	{
		ValidationTestHelper.AssertErrorIfInvalidCode(invoiceLine.JI_CustomsUnitQtyInfo, "XX", "KG");
	}

	public void TestCheckJI_CustomsSecondQuantity()
	{
		var targetInfo = invoiceLine.JI_CustomsSecondQuantityInfo;
		var errorMessage = "the maximum value allowed for Customs Quantity 2 is 999,999,999.99.";
		invoiceLine.JI_CustomsSecondQuantity = 1000000000m;
		AssertHasErrorContaining("value's precision is 10", targetInfo, errorMessage);

		invoiceLine.JI_CustomsSecondQuantity = 999999999m;
		AssertNoErrorContaining("value's precision is 9", targetInfo, errorMessage);

		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceLine.JI_CustomsSecondQuantityInfo, invoiceLine.JI_CustomsSecondUnitQtyInfo);
	}

	public void TestCheckJI_CustomsSecondUnitQty()
	{
		ValidationTestHelper.AssertErrorIfInvalidCode(invoiceLine.JI_CustomsSecondUnitQtyInfo, "XX", "KG");
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceLine.JI_CustomsSecondUnitQtyInfo, invoiceLine.JI_CustomsSecondQuantityInfo);
	}

	public void TestValidateJI_TradeControlOrderAppendix()
	{
		var startDate = ZDateTime.Today.AddDays(-1);
		var endDate = ZDateTime.Today.AddDays(1);

		var universalReferenceTestHelper = new UniversalReferenceTestDataHelper(Factory);
		universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "Japan Export Trade Control Ordinance Appendix");

		var refCusCodeList1 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "1", startDate, endDate);
		refCusCodeList1.ZZD_Description = "refCusCodeList1";

		var refCusCodeList2 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "2", startDate, endDate);
		refCusCodeList2.ZZD_Description = "refCusCodeList2";

		var refCusCodeList3 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "3", startDate, endDate);
		refCusCodeList3.ZZD_Description = "refCusCodeList3";
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_TradeControlOrderAppendixInfo, new ZString[] { "X", "4" }, new ZString[] { "1", "2", "3" });
	}

	public void TestValidateJI_DomesticConsumptionTaxExemptionCode()
	{
		Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportConsumptionTaxExemptionCode, "B");

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_DomesticConsumptionTaxExemptionCodeInfo, "5", "B");
	}

	public void TestValidateJI_AdvanceRulingOnClassification()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var targetInfo = invoiceLine.JI_AdvanceRulingOnClassificationInfo;
		var expectedErrMsg = $"{targetInfo.HumanReadableName} must be exactly 9 characters long.";

		AssertExceptionThrown<MaxLengthExceededException>("Cannot set to Advanced Ruling on Classification over the length", () => { invoiceLine.JI_AdvanceRulingOnClassification = "1234567890"; });

		ErrorReporter.Clear();

		invoiceLine.JI_AdvanceRulingOnClassification = "123";
		var errMsgUnderLimit = invoiceLine.GetMessageErrors().GetFirst().Message;
		AssertContains(
			expectedErrMsg,
			errMsgUnderLimit
		);
	}

	public void TestValidateJI_AdvanceRulingOnOrigin()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var targetInfo = invoiceLine.JI_AdvanceRulingOnOriginInfo;
		var expectedErrMsg = $"{targetInfo.HumanReadableName} must be exactly 7 characters long.";

		AssertExceptionThrown<MaxLengthExceededException>("Cannot set to Advanced Ruling on Origin over the length", () => { invoiceLine.JI_AdvanceRulingOnOrigin = "12345678"; });

		ErrorReporter.Clear();

		invoiceLine.JI_AdvanceRulingOnOrigin = "123";
		var errMsgUnderLimit = invoiceLine.GetMessageErrors().GetFirst().Message;
		AssertContains(
			expectedErrMsg,
			errMsgUnderLimit
		);
	}

	public void TestValidateJI_NACCSCode()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType("JP", "HSN");
		Factory.Save();
		var tariff = helper.LoadOrCreateNewTariff("JP", tariffType.PK, "110100011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		var targetInfo = invoiceLine.JI_NACCSCodeInfo;

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		instruction.CEI_ValueType = ValueTypeList.Codes.S;
		invoiceLine.JI_NACCSCode = ExportNACCSCodeList.Codes.Y;
		var expectedMessage = "NACCS Code is not required.";
		AssertHasMessageError(targetInfo, expectedMessage);

		invoiceLine.JI_NACCSCode = ZString.Empty;
		AssertNoMessageError(targetInfo, expectedMessage);

		expectedMessage = "You have not entered a NACCS Code.";
		instruction.CEI_ValueType = ValueTypeList.Codes.L;
		invoiceLine.Validation.ValidateJI_NACCSCode();
		AssertHasMessageError(targetInfo, expectedMessage);

		invoiceLine.JI_NACCSCode = ExportNACCSCodeList.Codes.T;
		AssertNoMessageError(targetInfo, expectedMessage);

		expectedMessage = "NACCS Code must be T or empty.";
		instruction.CEI_Style = JPExportDeclarationTypeList.Codes.G;
		invoiceLine.Validation.ValidateJI_NACCSCode();
		AssertNoMessageError(targetInfo, expectedMessage);

		invoiceLine.JI_NACCSCode = ExportNACCSCodeList.Codes.Y;
		AssertHasMessageError(targetInfo, expectedMessage);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		expectedMessage = "You have not entered a NACCS Code.";
		invoiceLine.JI_NACCSCode = ZString.Empty;
		AssertHasMessageError(targetInfo, expectedMessage);

		invoiceLine.JI_NACCSCode = ExportNACCSCodeList.Codes.Y;
		AssertNoMessageError(targetInfo, expectedMessage);

		instruction.CEI_Style = JPImportDeclarationTypeList.Codes.H;
		invoiceLine.JI_Tariff = "110100011";
		invoiceLine.JI_NACCSCode = ZString.Empty;
		AssertHasMessageError(targetInfo, expectedMessage);

		invoiceLine.JI_NACCSCode = ExportNACCSCodeList.Codes.Y;
		AssertNoMessageError(targetInfo, expectedMessage);

		ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "9", "0");

		expectedMessage = "Value is incompatible with Declaration Type.";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		instruction.CEI_Style = JPExportDeclarationTypeList.Codes.R;
		invoiceLine.JI_NACCSCode = ExportNACCSCodeList.Codes.T;
		AssertNoMessageError(targetInfo, expectedMessage);

		instruction.CEI_Style = JPExportDeclarationTypeList.Codes.G;
		invoiceLine.Validation.ValidateJI_NACCSCode();
		AssertNoMessageError(targetInfo, expectedMessage);

		instruction.CEI_Style = JPExportDeclarationTypeList.Codes.T;
		invoiceLine.Validation.ValidateJI_NACCSCode();
		AssertHasMessageError(targetInfo, expectedMessage);

		invoiceLine.JI_NACCSCode = ExportNACCSCodeList.Codes.E;
		AssertNoMessageError(targetInfo, expectedMessage);
	}

	public void TestValidateJI_StorageType()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var jobComInvoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			jobComInvoiceLine.JI_CEI = instruction.PK;

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.S;
			jobComInvoiceLine.Validation.ValidateJI_StorageType();
			AssertNoNotifications("By default storage type can be empty", jobComInvoiceLine.JI_StorageTypeInfo);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.A;
			jobComInvoiceLine.Validation.ValidateJI_StorageType();
			AssertHasMessageError(jobComInvoiceLine.JI_StorageTypeInfo, "Storage Type cannot be empty when Declaration Type is A or G.");

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.G;
			jobComInvoiceLine.Validation.ValidateJI_StorageType();
			AssertHasMessageError(jobComInvoiceLine.JI_StorageTypeInfo, "Storage Type cannot be empty when Declaration Type is A or G.");

			jobComInvoiceLine.JI_StorageType = StorageTypeListWhenDeclarationTypeIsG.Codes._2;
			jobComInvoiceLine.Validation.ValidateJI_StorageType();
			AssertNoNotifications("Value in list is good", jobComInvoiceLine.JI_StorageTypeInfo);

			jobComInvoiceLine.JI_StorageType = "&";
			jobComInvoiceLine.Validation.ValidateJI_StorageType();
			AssertHasMessageError(jobComInvoiceLine.JI_StorageTypeInfo, "The value entered in Storage Type is not a valid list option.");
		});
	}

	public void TestCheckJI_BondedDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
		invoiceLine.JI_CEI = instruction.PK;
		var targetInfo = invoiceLine.JI_BondedDateInfo;

		invoiceLine.JI_BondedDate = ZDateTime.Today.AddDays(30);
		AssertHasMessageError(targetInfo, "Import for Storage (IS) Date must be today or a past date.");

		invoiceLine.JI_BondedDate = ZDateTime.Today;
		AssertNoMessageError(targetInfo, "Import for Storage (IS) Date must be today or a past date.");

		invoiceLine.JI_BondedDate = ZDateTime.Today.AddDays(-30);
		AssertNoMessageError(targetInfo, "Import for Storage (IS) Date must be today or a past date.");
		var importDeclarationTypeNeedISDate = new string[]
		{
			JPImportDeclarationTypeList.Codes.K,
			JPImportDeclarationTypeList.Codes.D,
			JPImportDeclarationTypeList.Codes.U,
			JPImportDeclarationTypeList.Codes.L,
			JPImportDeclarationTypeList.Codes.B,
			JPImportDeclarationTypeList.Codes.E,
			JPImportDeclarationTypeList.Codes.R,
		};
		var importDeclarationTypeNoValidation = new string[]
		{
			JPImportDeclarationTypeList.Codes.S,
			JPImportDeclarationTypeList.Codes.M,
			JPImportDeclarationTypeList.Codes.A,
		};
		var allDeclarationTypeCodes = new JPImportDeclarationTypeList().GetAllCodes();
		var expectedNeedDateErrorMessage = "You have not entered Import for Storage (IS) Date.";
		var expectedNotNeedDateErrorMessage = "Import for Storage (IS) Date is not required.";

		foreach (var code in allDeclarationTypeCodes)
		{
			if (importDeclarationTypeNeedISDate.Contains(code))
			{
				instruction.CEI_Style = code;
				invoiceLine.JI_BondedDate = ZDateTime.Today;
				AssertNoMessageError("DeclarationType:" + code, targetInfo, expectedNeedDateErrorMessage);

				invoiceLine.JI_BondedDate = ZDateTime.Empty;
				AssertHasMessageError("DeclarationType:" + code, targetInfo, expectedNeedDateErrorMessage);
			}
			else if (importDeclarationTypeNoValidation.Contains(code))
			{
				instruction.CEI_Style = code;
				invoiceLine.JI_BondedDate = ZDateTime.Empty;
				AssertEquals("JI_BondedDate empty", false, targetInfo.HasMessageErrors());

				invoiceLine.JI_BondedDate = ZDateTime.Today;
				AssertEquals("JI_BondedDate empty", false, targetInfo.HasMessageErrors());
			}
			else
			{
				instruction.CEI_Style = code;
				invoiceLine.JI_BondedDate = ZDateTime.Empty;
				AssertNoMessageError("DeclarationType:" + code, targetInfo, expectedNotNeedDateErrorMessage);

				invoiceLine.JI_BondedDate = ZDateTime.Today;
				AssertHasMessageError("DeclarationType:" + code, targetInfo, expectedNotNeedDateErrorMessage);
			}
		}
	}

	public void TestCheckJI_TradeControlOrderAppendix()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		var targetInfo = invoiceLine.JI_TradeControlOrderAppendixInfo;
		var errorMessage = "Value cannot be selected with the current declaration type.";
		foreach (var declarationType in new[] { "M", "N", "T" })
		{
			instruction.CEI_Style = declarationType;
			invoiceLine.JI_TradeControlOrderAppendix = "10101";
			AssertHasMessageError(targetInfo, errorMessage);

			invoiceLine.JI_TradeControlOrderAppendix = "10906";
			AssertNoMessageError(targetInfo, errorMessage);
		}
	}

	public void TestCheckJI_FEFTAArticle48()
	{
		var startDate = ZDateTime.Today.AddDays(-1);
		var endDate = ZDateTime.Today.AddDays(1);

		var universalReferenceTestHelper = new UniversalReferenceTestDataHelper(Factory);
		universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "Japan Export Trade Control Ordinance Appendix");

		var refCusCodeList1 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "10418", startDate, endDate);
		refCusCodeList1.ZZD_Description = "別表第１*4－(18)";

		var refCusCodeList2 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "23053", startDate, endDate);
		refCusCodeList2.ZZD_Description = "別表第２の３（第２号（汎用品等））*44";

		var refCusCodeList3 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "50010", startDate, endDate);
		refCusCodeList3.ZZD_Description = "別表第５（輸出令第４条第２項第２号関係）*5 － 1";
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var targetInfo = invoiceLine.JI_FEFTAArticle48Info;
		ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "X", "A");

		var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = cusEntryInstruction.PK;
		invoiceLine.JI_TradeControlOrderAppendix = "10418";
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

		invoiceLine.JI_TradeControlOrderAppendix = "23053";
		ValidationTestHelper.AssertIfIsEnteredMessageError(targetInfo);

		invoiceLine.JI_TradeControlOrderAppendix = "50010";
		ValidationTestHelper.AssertIfIsEnteredMessageError(targetInfo);
	}

	void CreateTariffData(string tariffType)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Japan, "1234", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC 1", "1234");
		helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Japan, "123456", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC 2", "123456");
		helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Japan, "111111111", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC 3", "111111111");
		var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Japan, tariffType);
		Factory.Save();

		cusTariffType.ZZI_ZZ9_NKNomenclatureGroupType = Core.Constants.CountryCodes.Japan;
		helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Japan, cusTariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "test description 1");
	}

	void CreateTariffDataForConcessionOrder()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType("JP", Universal.Constants.TariffTypes.Import);
		var rateType = helper.CreateNewOrGetExistingRateType("JP", "DTY", "Duty");
		var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", rateType.PK);

		var preference = helper.CreatePreferenceForCountry("GEN", "General", "JP");

		var tradeGroup = helper.CreateTradeGroup("JP", "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
		helper.AddCountry(tradeGroup, "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

		var tariff = helper.LoadOrCreateNewTariff("JP", tariffType.PK, "1101", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "15%", "JP");
		helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, "WK", "Y");

		helper.CreateNewOrGetExistingCusCodeType("TT", "Test Type");
		helper.CreateNewOrGetExistingCusCodeList("JP", "TT", "WK", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

		Factory.Save();
	}

	void PrepareCodes()
	{
		var startDate = ZDateTime.Today.AddDays(-1);
		var endDate = ZDateTime.Today.AddDays(1);

		var universalReferenceTestHelper = new UniversalReferenceTestDataHelper(Factory);
		universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType1, "Japan Certificate Of Origin 1");
		universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType2, "Japan Certificate Of Origin 2");
		universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType3, "Japan Certificate Of Origin 3");
		var coot1Code = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType1, "SG", "Description for SG", startDate, endDate);
		universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType2, "T", "Description for T", startDate, endDate);
		var coot3Code = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType3, "7", "Description for 7", startDate, endDate);
		universalReferenceTestHelper.CreateNewOrGetExistingCusCodeListAttribute(coot1Code.PK, "Preference", "EPA");
		universalReferenceTestHelper.CreateNewOrGetExistingCusCodeListAttribute(coot3Code.PK, "Preference", "EPA");

		Factory.Save();
	}

	#region Implementation

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		instruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		Factory.Save();
	}

	JobDeclaration declaration;
	CusEntryInstruction instruction;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;

	#endregion
}
