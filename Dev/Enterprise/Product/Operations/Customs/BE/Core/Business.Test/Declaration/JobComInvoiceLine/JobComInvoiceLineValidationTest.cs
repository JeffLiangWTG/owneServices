using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestAdditionalProcedureCodesMaxNumbers()
	{
		// Make sure that maximum 3 concession beginning with a number can be selected and maximum 3 beginning with a letter. Take into account the concession of �[37] CPC�. It must be a message error.
		invoiceLine.JI_FormattedProcedure = "4000a";
		var additionalProcedureCodesCollection = invoiceLine.AdditionalProcedureCodes;

		additionalProcedureCodesCollection.AddNew("400050");
		additionalProcedureCodesCollection.AddNew("400051");
		additionalProcedureCodesCollection.AddNew("400052");
		additionalProcedureCodesCollection.AddNew("4000a0");
		additionalProcedureCodesCollection.AddNew("4000a1");

		invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
		AssertNoMessageError(invoiceLine.AdditionalProcedureCodesAsStringInfo, "At most 3 Additional Procedures (including CPC), with a concession starting with a digit, can be selected. Values: 4000[5]0, 4000[5]1, 4000[5]2, 4000[5]3.");
		AssertNoMessageError(invoiceLine.AdditionalProcedureCodesAsStringInfo, "At most 3 Additional Procedures (including CPC), with a concession starting with a letter, can be selected. Values: 4000[a], 4000[a]0, 4000[a]1, 4000[a]2.");

		var code = additionalProcedureCodesCollection.AddNew("400053");
		invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
		AssertHasMessageError(invoiceLine.AdditionalProcedureCodesAsStringInfo, "At most 3 Additional Procedures (including CPC), with a concession starting with a digit, can be selected. Values: 4000[5]0, 4000[5]1, 4000[5]2, 4000[5]3.");
		AssertNoMessageError(invoiceLine.AdditionalProcedureCodesAsStringInfo, "At most 3 Additional Procedures (including CPC), with a concession starting with a letter, can be selected. Values: 4000[a], 4000[a]0, 4000[a]1, 4000[a]2.");

		additionalProcedureCodesCollection.Remove(code);
		additionalProcedureCodesCollection.AddNew("4000a2");
		invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
		AssertHasMessageError(invoiceLine.AdditionalProcedureCodesAsStringInfo, "At most 3 Additional Procedures (including CPC), with a concession starting with a letter, can be selected. Values: 4000[a], 4000[a]0, 4000[a]1, 4000[a]2.");
		AssertNoMessageError(invoiceLine.AdditionalProcedureCodesAsStringInfo, "At most 3 Additional Procedures (including CPC), with a concession starting with a digit, can be selected. Values: 4000[5]0, 4000[5]1, 4000[5]2, 4000[5]3.");
	}

	public void TestCheckJI_Procedure_Import()
	{
		new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping("BE");
		var procedure = Factory.New<RefCusProcedure>();
		procedure.ZZ6_ProcedureCode = "71";
		procedure.ZZ6_Description = "Description";
		procedure.ZZ6_PreviousProcedureCode = "00";
		procedure.ZZ6_Concession = "123";
		procedure.ZZ6_ShipmentType = "IMP";
		procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Belgium;
		procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
		procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;

		var helper = new WhsDataTestHelper(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "71";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Procedure = "7100123";
		invoiceLine2.JI_Procedure = "7100123";

		var errorMsg = "The From Warehouse, must be entered and match a Warehouse record.";
		invoiceLine1.Validation.ValidateJI_Procedure();
		AssertHasMessageError("The From Warehouse, must be entered and match a Warehouse record.", invoiceLine1.JI_ProcedureInfo, errorMsg);

		entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
		invoiceLine1.Validation.ValidateJI_Procedure();
		AssertNoMessageError("The From Warehouse, must be entered and match a Warehouse record.", invoiceLine1.JI_ProcedureInfo, errorMsg);

		invoiceLine1.SupportingDocuments.RemoveAndDeleteAll();
		invoiceLine2.SupportingDocuments.RemoveAndDeleteAll();

		var sup2 = invoiceLine1.SupportingDocuments.AddNew();
		sup2.CSI_Code = "C518";
		sup2.CSI_ReferenceNumber = "BECW1A00742";
		invoiceLine1.Validation.ValidateJI_Procedure();

		invoiceLine1.Validation.ValidateJI_Procedure();
		CombineAssertions("Assert Supporting Document has been added both Invoice Line", () =>
		{
			AssertSupportingDocumentCollectionContainsOnlyOne(invoiceLine1, "C518");
		});
	}

	public static void AssertSupportingDocumentCollectionContainsOnlyOne(JobComInvoiceLine invoiceLine, ZString documentType)
	{
		var actualSupportingDocuments = invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == documentType);

		AssertEquals($"Supporting documents collection should contain only one {documentType} document", 1, actualSupportingDocuments.Count());

		var expectedSupportingDocument = actualSupportingDocuments.Single();
		AssertEquals($"Supporting documents collection should contain {documentType}", true, expectedSupportingDocument != null);
	}

	public void TestCheckJI_Procedure_Export()
	{
		new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping("BE");
		var procedure = Factory.New<RefCusProcedure>();
		procedure.ZZ6_ProcedureCode = "40";
		procedure.ZZ6_Description = "Description";
		procedure.ZZ6_PreviousProcedureCode = "71";
		procedure.ZZ6_Concession = "123";
		procedure.ZZ6_ShipmentType = "EXP";
		procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Belgium;
		procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
		procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;

		string errorMsg = "The To Warehouse, must be entered and match a Warehouse record.";
		var helper = new WhsDataTestHelper(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "40";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Procedure = "4071123";
		invoiceLine2.JI_Procedure = "4071123";
		invoiceLine1.Validation.ValidateJI_Procedure();
		AssertHasMessageError("The To Warehouse, must be entered and match a Warehouse record.", invoiceLine1.JI_ProcedureInfo, errorMsg);
		AssertHasMessageError("When the invoice line's Procedure has a To Warehouse or From Warehouse, then there must be only one C517/C518/C519 in the supporting document collection.", invoiceLine1.JI_ProcedureInfo, errorMsg);

		entryInstruction.CEI_OA_Warehouse2 = helper.Warehouse.MainAddress.PK;
		invoiceLine1.Validation.ValidateJI_Procedure();
		AssertNoMessageError("The To Warehouse, must be entered and match a Warehouse record.", invoiceLine1.JI_ProcedureInfo, errorMsg);
		invoiceLine1.SupportingDocuments.RemoveAndDeleteAll();
		invoiceLine2.SupportingDocuments.RemoveAndDeleteAll();

		var sup = invoiceLine1.SupportingDocuments.AddNew();
		sup.CSI_Code = "C517";
		sup.CSI_ReferenceNumber = "BECWPA00742";

		var sup2 = invoiceLine2.SupportingDocuments.AddNew();
		sup2.CSI_Code = "C518";
		sup2.CSI_ReferenceNumber = "BECW1A00742";
		invoiceLine2.Validation.ValidateJI_Procedure();
	}

	public void TestCheckJI_StateOrRegionOfOrigin()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_StateOrRegionOfOriginInfo, new ZString[] { "x" }, new BERegionList().GetAllCodesZString());
	}

	public void TestCheckJI_RN_NKCountryOfExport()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_RN_NKCountryOfExportInfo, "xx", "BE");
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
	}
	JobComInvoiceLine invoiceLine;
}
