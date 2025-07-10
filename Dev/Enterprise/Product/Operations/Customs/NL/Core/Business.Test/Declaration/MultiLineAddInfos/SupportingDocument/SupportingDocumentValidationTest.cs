using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckDuplicatedDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		supportingDocument = invoiceLine.SupportingDocuments.AddNew();

		const string expectedErrorMessage = "[R9004] Combination of Type and Reference must be unique.";
		CombineAssertions(() =>
		{
			supportingDocument.CSI_Code = "N002";
			supportingDocument.CSI_ReferenceNumber = "DANIEL";
			AssertNoMessageError("Single", supportingDocument.CSI_CodeInfo, expectedErrorMessage);

			var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument2.CSI_ReferenceNumber = "DANIEL";
			supportingDocument2.CSI_Code = "N002";
			AssertHasMessageError("Duplicate", supportingDocument2.CSI_CodeInfo, expectedErrorMessage);

			supportingDocument2.CSI_Code = "U022";
			AssertNoMessageError("Different Code", supportingDocument2.CSI_CodeInfo, expectedErrorMessage);
			supportingDocument2.CSI_ReferenceNumber = "NEW VALUE";
			supportingDocument2.CSI_Code = "N002";
			AssertNoMessageError("Different Reference Number", supportingDocument2.CSI_CodeInfo, expectedErrorMessage);

			supportingDocument.CSI_ReferenceNumber = ZString.Empty;
			supportingDocument2.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageError("Duplicate", supportingDocument2.CSI_ReferenceNumberInfo, expectedErrorMessage);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_SubStyle = "B";
			supportingDocument2.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Exception of R9004 Rule", supportingDocument2.CSI_ReferenceNumberInfo, expectedErrorMessage);
		});
	}

	public void TestCheckCSI_RX_NKCurrency()
	{
		CombineAssertions(() =>
		{
			supportingDocument.CSI_Value = 6;
			supportingDocument.Validation.ValidateCSI_RX_NKCurrency();
			AssertHasMessageError(supportingDocument.CSI_RX_NKCurrencyInfo, "If Value isn't empty, Currency is required.");

			supportingDocument.CSI_RX_NKCurrency = "EUR";
			AssertNoMessageError(supportingDocument.CSI_RX_NKCurrencyInfo, "If Value isn't empty, Currency is required.");

			supportingDocument.CSI_Value = 0;
			supportingDocument.Validation.ValidateCSI_RX_NKCurrency();
			AssertNoMessageError(supportingDocument.CSI_RX_NKCurrencyInfo, "If Value isn't empty, Currency is required.");
		});
	}

	public void TestCheckCSI_UnitOfQuantity()
	{
		supportingDocument.CSI_Quantity = 5;
		supportingDocument.CSI_UnitOfQuantity = "ZZZ";
		AssertHasMessageError(supportingDocument.CSI_UnitOfQuantityInfo, "The code you have selected is not in the list.");

		supportingDocument.CSI_Quantity = 0;
		supportingDocument.CSI_UnitOfQuantity = "ZZZ";
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantityInfo);

		supportingDocument.CSI_Quantity = 5;
		supportingDocument.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantityInfo);

		supportingDocument.CSI_UnitOfQuantity = "AAA";
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantityInfo);

		supportingDocument.CSI_UnitOfQuantity = ZString.Empty;
		AssertHasMessageError(supportingDocument.CSI_UnitOfQuantityInfo, "If Quantity isn't empty, Unit of Quantity is required.");
		supportingDocument.CSI_Quantity = 0;
		supportingDocument.Validation.ValidateCSI_UnitOfQuantity();
		AssertNoMessageError(supportingDocument.CSI_UnitOfQuantityInfo, "If Quantity isn't empty, Unit of Quantity is required.");
	}

	public void TestCheckWhetherQuantityAndValueExistsMeanwhile()
	{
		CombineAssertions(() =>
		{
			supportingDocument.CSI_Quantity = 5;
			supportingDocument.CSI_Value = 6;
			AssertHasMessageError(supportingDocument.CSI_ValueInfo, "If Qty is completed value details should be disabled and vice versa");
			supportingDocument.Validation.ValidateCSI_Quantity();
			AssertHasMessageError(supportingDocument.CSI_QuantityInfo, "If Qty is completed value details should be disabled and vice versa");

			supportingDocument.CSI_Quantity = 0;
			supportingDocument.Validation.ValidateCSI_Value();
			AssertNoMessageError(supportingDocument.CSI_ValueInfo, "If Qty is completed value details should be disabled and vice versa");
		});
	}

	public void TestCheckCSI_UnitOfQuantity_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		supportingDocument = declaration.SupportingDocuments.AddNew();

		supportingDocument.CSI_Quantity = 5;
		supportingDocument.CSI_UnitOfQuantity = "ZZZ";
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantityInfo);

		supportingDocument.CSI_Quantity = 0;
		supportingDocument.CSI_UnitOfQuantity = "ZZZ";
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantityInfo);

		supportingDocument.CSI_Quantity = 5;
		supportingDocument.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantityInfo);

		supportingDocument.CSI_UnitOfQuantity = "AAA";
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantityInfo);
	}

	public void TestCheckCSI_UnitOfQuantity2()
	{
		supportingDocument.CSI_Quantity2 = 5;
		supportingDocument.CSI_UnitOfQuantity2 = "ZZZ";
		AssertHasMessageError(supportingDocument.CSI_UnitOfQuantity2Info, "The code you have selected is not in the list.");

		supportingDocument.CSI_Quantity2 = 0;
		supportingDocument.CSI_UnitOfQuantity2 = "ZZZ";
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantity2Info);

		supportingDocument.CSI_Quantity2 = 5;
		supportingDocument.CSI_UnitOfQuantity2 = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantity2Info);

		supportingDocument.CSI_UnitOfQuantity2 = "AAA";
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantity2Info);
	}

	public void TestCheckCSI_UnitOfQuantity2_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		supportingDocument = declaration.SupportingDocuments.AddNew();

		supportingDocument.CSI_Quantity2 = 5;
		supportingDocument.CSI_UnitOfQuantity2 = "ZZZ";
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantity2Info);

		supportingDocument.CSI_Quantity2 = 0;
		supportingDocument.CSI_UnitOfQuantity2 = "ZZZ";
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantity2Info);

		supportingDocument.CSI_Quantity2 = 5;
		supportingDocument.CSI_UnitOfQuantity2 = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantity2Info);

		supportingDocument.CSI_UnitOfQuantity2 = "AAA";
		AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantity2Info);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, "Netherlands", eun);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Qualifiers", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Qualifiers", Core.Constants.CountryCodes.Netherlands);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, "Kilogram", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volume.Litre, "Liter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "AAA", "AAA Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "BBB", "BBB Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		supportingDocument = declaration.SupportingDocuments.AddNew();
	}
	SupportingDocument supportingDocument;
}
