using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateCSI_ReferenceNumber_InvoiceHeader()
	{
		const string message = "You have not entered a Reference.";

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var doc = invoiceHeader.SupportingDocuments.AddNew();
		doc.CSI_Code = "ABC";

		CombineAssertions(() =>
		{
			doc.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError("Empty", doc.CSI_ReferenceNumberInfo, message);
			doc.CSI_ReferenceNumber = "BECWPA00742";
			AssertNoMessageError("Entered", doc.CSI_ReferenceNumberInfo, message);
		});
	}

	public void TestValidateCSI_ReferenceNumber_InvoiceLine_Mandatory()
	{
		const string message = "You have not entered a Reference.";

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "71";
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Procedure = "7100123";

		var doc = invoiceLine.SupportingDocuments.AddNew();
		doc.CSI_Code = "ABC";

		CombineAssertions(() =>
		{
			doc.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError("Empty", doc.CSI_ReferenceNumberInfo, message);

			doc.CSI_Code = "C517";
			doc.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError("Entered", doc.CSI_ReferenceNumberInfo, message);
		});
	}

	public void TestValidateCSI_ReferenceNumber_InvoiceLine_AuthorizationTypeMatchesDocumentType()
	{
		const string message = "The Authorization Type CW1 of this document doesn't match the selected Document Type, CWP is expected.";

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "71";
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Procedure = "7100123";

		var doc = invoiceLine.SupportingDocuments.AddNew();
		doc.CSI_Code = "C517";
		CombineAssertions(() =>
		{
			doc.CSI_ReferenceNumber = "BECW1A00742";
			AssertHasMessageError("don't match", doc.CSI_ReferenceNumberInfo, message);
			doc.CSI_ReferenceNumber = "BECWPA00742";
			AssertNoMessageError("match", doc.CSI_ReferenceNumberInfo, message);
		});
	}

	public void TestValidateCSI_ReferenceNumber_InvoiceLine_AuthorizationTypeMatchesWarehouseAddress()
	{
		const string message = "No Authorization of type 'CWP' found for the selected warehouse address.";

		new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.CountryCodes.Belgium);

		var procedure = Factory.New<RefCusProcedure>();
		procedure.ZZ6_ProcedureCode = "71";
		procedure.ZZ6_Description = "Description";
		procedure.ZZ6_PreviousProcedureCode = "00";
		procedure.ZZ6_Concession = "123";
		procedure.ZZ6_ShipmentType = "IMP";
		procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Belgium;
		procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
		procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;

		var importer = Factory.New<OrgHeader>();
		importer.OH_Code = "COD";
		importer.OH_IsWarehouseClient = true;
		importer.Addresses.AddNew().Address1 = "2 Street";
		importer.CustomsCodes.AddNew("CCP", "BECWPA00742").OK_OA_PremisesAddress = importer.MainAddress.PK;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = importer.PK;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "71";
		entryInstruction.CEI_OA_Warehouse = importer.MainAddress.PK;
		entryInstruction.CEI_OA_Warehouse2 = importer.MainAddress.PK;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Procedure = "7100123";

		var doc = invoiceLine.SupportingDocuments.AddNew();
		doc.CSI_Code = "C517";

		CombineAssertions(() =>
		{
			doc.CSI_ReferenceNumber = "BECWPA00742";
			AssertHasMessageError("no available auth list", doc.CSI_ReferenceNumberInfo, message);

			var authorisation = Factory.New<CusAuthorisationHeader>();
			authorisation.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);
			authorisation.CPH_EndDate = ZDate.Today.AddDays(1);
			authorisation.CPH_Number = "BECWPA00742";
			authorisation.CPH_OH_PermitHolder = importer.PK;
			authorisation.CPH_Type = "CWP";
			authorisation.CPH_OA_AppliesTo = importer.MainAddress.PK;
			Factory.Save();

			doc.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("available auth list", doc.CSI_ReferenceNumberInfo, message);
		});
	}

	public void TestValidateCSI_ReferenceNumber_InvoiceLine_AuthorizationTypeWithNumberMatchesWarehouseAddres()
	{
		const string message = "No Authorization of type 'CWP' with number BECWPA00742 found for the selected warehouse address. Please checked the entered Authorization Number is correct.";

		new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.CountryCodes.Belgium);

		var procedure = Factory.New<RefCusProcedure>();
		procedure.ZZ6_ProcedureCode = "71";
		procedure.ZZ6_Description = "Description";
		procedure.ZZ6_PreviousProcedureCode = "00";
		procedure.ZZ6_Concession = "123";
		procedure.ZZ6_ShipmentType = "IMP";
		procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Belgium;
		procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
		procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;

		var importer = Factory.New<OrgHeader>();
		importer.OH_Code = "COD";
		importer.OH_IsWarehouseClient = true;
		importer.Addresses.AddNew().Address1 = "2 Street";
		importer.CustomsCodes.AddNew("CCP", "BECWPA00742").OK_OA_PremisesAddress = importer.MainAddress.PK;

		var authorisation = Factory.New<CusAuthorisationHeader>();
		authorisation.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);
		authorisation.CPH_EndDate = ZDate.Today.AddDays(1);
		authorisation.CPH_Number = "BECWXXXXX";
		authorisation.CPH_OH_PermitHolder = importer.PK;
		authorisation.CPH_Type = "CWP";
		authorisation.CPH_OA_AppliesTo = importer.MainAddress.PK;
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = importer.PK;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "71";
		entryInstruction.CEI_OA_Warehouse = importer.MainAddress.PK;
		entryInstruction.CEI_OA_Warehouse2 = importer.MainAddress.PK;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Procedure = "7100123";

		var doc = invoiceLine.SupportingDocuments.AddNew();
		doc.CSI_Code = "C517";

		CombineAssertions(() =>
		{
			doc.CSI_ReferenceNumber = "BECWPA00742";
			AssertHasMessageError("Not match", doc.CSI_ReferenceNumberInfo, message);

			authorisation.CPH_Number = "BECWPA00742";
			Factory.Save();
			doc.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Match", doc.CSI_ReferenceNumberInfo, message);
		});
	}

	public void TestValidateCSI_Quantity()
	{
		var doc = Factory.New<SupportingDocument>();

		CombineAssertions(() =>
		{
			doc.CSI_Quantity = new ZDecimal(123456789012345.123);
			AssertNoMessageErrors(doc.CSI_QuantityInfo);
			AssertNoErrors(doc.CSI_QuantityInfo);

			doc.CSI_Quantity = new ZDecimal(1234567890123456);
			AssertHasErrors(doc.CSI_QuantityInfo);

			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(doc.CSI_QuantityInfo, doc.CSI_UnitOfQuantityInfo, false);
		});
	}

	public void TestValidateValidationOffice()
	{
		const string message = "Validation Office only accepts Western European languages characters.";

		var doc = Factory.New<SupportingDocument>();

		CombineAssertions(() =>
		{
			doc.ValidationOffice = "テスト";
			AssertHasError("Not western characters", doc.ValidationOfficeInfo, message);

			doc.ValidationOffice = "SomeValidationOffice123456";
			AssertNoError("valid", doc.ValidationOfficeInfo, message);
		});
	}

	public void TestValidateArchiveSupport()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(Factory.New<SupportingDocument>().ArchiveSupportInfo, "~", BinaryIntValuesList.Codes.Yes);
	}

	public void TestValidateArchiveLocationIndicator()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(Factory.New<SupportingDocument>().ArchiveLocationIndicatorInfo, "~", BinaryIntValuesList.Codes.Yes);
	}

	public void TestValidateCSI_UnitOfQuantity()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, "Belgium", eun);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Qualifiers", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "KGM Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var doc = Factory.New<SupportingDocument>();

		CombineAssertions(() =>
		{
			doc.CSI_UnitOfQuantity = "KGM";
			AssertNoMessageErrorContaining(doc.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);

			doc.CSI_UnitOfQuantity = "ERR";
			AssertHasMessageErrorContaining(doc.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);

			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(doc.CSI_UnitOfQuantityInfo, doc.CSI_QuantityInfo, false);
		});
	}

	public void TestCSI_Code()
	{
		var doc = Factory.New<SupportingDocument>();
		ValidationTestHelper.AssertErrorIfNotEntered(doc.CSI_CodeInfo);
	}

	public void TestValidateCSI_Value()
	{
		var doc = Factory.New<SupportingDocument>();

		CombineAssertions(() =>
		{
			doc.CSI_Value = new ZDecimal(123456789012345.123);
			AssertNoMessageErrors(doc.CSI_ValueInfo);
			AssertNoErrors(doc.CSI_ValueInfo);

			doc.CSI_Value = new ZDecimal(1234567890123456);
			AssertHasErrors(doc.CSI_ValueInfo);

			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(doc.CSI_ValueInfo, doc.CSI_RX_NKCurrencyInfo, false);
		});
	}

	public void TestValidateCSI_RX_NKCurrency()
	{
		var doc = Factory.New<SupportingDocument>();

		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(doc.CSI_RX_NKCurrencyInfo, doc.CSI_ValueInfo);
	}
}
