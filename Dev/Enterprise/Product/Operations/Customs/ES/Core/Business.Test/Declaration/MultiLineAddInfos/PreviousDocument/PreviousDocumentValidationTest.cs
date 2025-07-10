using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

public class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckChildrenPreviousDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		var previousDoc = declaration.PreviousDocuments.AddNew();
		SetDataForPreviousDocument(previousDoc, "AA", 1);

		var previousDoc2 = invoice.PreviousDocuments.AddNew();
		SetDataForPreviousDocument(previousDoc2, "BB", 2);

		var previousDoc3 = invoiceLine.PreviousDocuments.AddNew();
		SetDataForPreviousDocument(previousDoc3, "CC", 3);

		declaration.PreviousDocuments.RunPreSaveValidation();
		AssertHasRowMessageErrorContaining(declaration.PreviousDocuments[0], "This previous document will not be declared as all invoice lines have its own previous document for Box 40");

		previousDoc3.Delete();
		declaration.PreviousDocuments.RunPreSaveValidation();
		AssertHasRowMessageErrorContaining(declaration.PreviousDocuments[0], "This previous document will not be declared as all invoice lines have its own previous document for Box 40");

		previousDoc2.Delete();
		declaration.PreviousDocuments.RunPreSaveValidation();
		AssertNoRowMessageErrorContaining(declaration.PreviousDocuments[0], "This previous document will not be declared as all invoice lines have its own previous document for Box 40");
	}

	public void TestOnePreviousDocumentPerEntryLineForDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();

		var previousDoc = declaration.PreviousDocuments.AddNew();
		SetDataForPreviousDocument(previousDoc, "AA", 1);
		var previousDoc2 = declaration.PreviousDocuments.AddNew();
		SetDataForPreviousDocument(previousDoc2, "BB", 2);

		declaration.PreviousDocuments.RunPreSaveValidation();
		AssertHasRowMessageErrorContaining(declaration.PreviousDocuments[0], "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");
		AssertHasRowMessageErrorContaining(declaration.PreviousDocuments[1], "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");

		previousDoc2.Delete();
		declaration.PreviousDocuments.RunPreSaveValidation();
		AssertNoRowMessageErrorContaining(declaration.PreviousDocuments[0], "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");
	}

	public void TestOnePreviousDocumentPerEntryLineForHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();

		var previousDoc = invoice.PreviousDocuments.AddNew();
		SetDataForPreviousDocument(previousDoc, "AA", 1);
		var previousDoc2 = invoice.PreviousDocuments.AddNew();
		SetDataForPreviousDocument(previousDoc2, "BB", 2);

		invoice.PreviousDocuments.RunPreSaveValidation();
		AssertNoRowMessageErrorContaining(invoice.PreviousDocuments[0], "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");
		AssertNoRowMessageErrorContaining(invoice.PreviousDocuments[1], "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");
	}

	public void TestOnePreviousDocumentPerEntryLineForInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		var previousDoc = invoiceLine.PreviousDocuments.AddNew();
		SetDataForPreviousDocument(previousDoc, "AA", 1);
		var previousDoc2 = invoiceLine.PreviousDocuments.AddNew();
		SetDataForPreviousDocument(previousDoc2, "BB", 2);

		invoiceLine.PreviousDocuments.RunPreSaveValidation();
		AssertNoRowMessageErrorContaining(invoiceLine.PreviousDocuments[0], "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");
		AssertNoRowMessageErrorContaining(invoiceLine.PreviousDocuments[1], "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");
	}

	public void TestSubTypeNotMandatory()
	{
		var validation = new PreviousDocumentValidationForTest(Factory.New<PreviousDocument>());
		AssertEquals("IsSubTypeMandatory is false in ES", false, validation.IsSubTypeMandatory_Exposed);
	}

	public void TestCheckNoEmptyDataWhenMoreThanOne_InvoiceLine()
	{
		SetUpBulkType();

		CombineAssertions(() =>
		{
			var messageErrorExpected = "If there is more than one previous document per entry line, then Quantity, Pack Type and Pack Qty must be filled.";
			var (dec, invLine, entryLine, prevDoc) = CommonSetUpAndAssertT2C_Import_POUS_EntryLine(messageErrorExpected);
			var prevDoc2 = invLine.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "Z";
			prevDoc2.CSI_Quantity = 3;
			prevDoc2.CSI_PackType = "AH3";
			prevDoc2.CSI_PackQty = 2;
			prevDoc.Validation.ValidateAll();
			prevDoc2.Validation.ValidateAll();
			AssertEquals(2, entryLine.PreviousDocuments.Count());
			AssertHasRowMessageErrorContaining(prevDoc, messageErrorExpected);
			AssertNoRowMessageErrorContaining(prevDoc2, messageErrorExpected);

			prevDoc.CSI_PackType = "BK";
			prevDoc.CSI_Quantity = 3;
			prevDoc.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(prevDoc, messageErrorExpected);

			prevDoc.CSI_PackType = ZString.Empty;
			prevDoc.CSI_Quantity = 0;
			prevDoc.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(prevDoc, messageErrorExpected);

			prevDoc.CSI_Quantity = 4;
			prevDoc.CSI_PackType = "AH";
			prevDoc.CSI_PackQty = 3;
			prevDoc.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(prevDoc, messageErrorExpected);
		});
	}

	public void TestCheckNoEmptyDataWhenMoreThanOne_InvoiceHeader()
	{
		SetUpBulkType();

		CombineAssertions(() =>
		{
			var messageErrorExpected = "If there is more than one previous document per entry line, then Quantity, Pack Type and Pack Qty must be filled.";
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var prevDoc = invoice.PreviousDocuments.AddNew();

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var prevDoc2 = invoice.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "Z";
			prevDoc2.CSI_Quantity = 3;
			prevDoc2.CSI_PackType = "AH3";
			prevDoc2.CSI_PackQty = 2;
			prevDoc.Validation.ValidateAll();
			prevDoc2.Validation.ValidateAll();
			AssertEquals(2, entryLine.PreviousDocuments.Count());
			AssertHasRowMessageErrorContaining(prevDoc, messageErrorExpected);
			AssertNoRowMessageErrorContaining(prevDoc2, messageErrorExpected);

			prevDoc.CSI_PackType = "BK";
			prevDoc.CSI_Quantity = 3;
			prevDoc.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(prevDoc, messageErrorExpected);

			prevDoc.CSI_PackType = ZString.Empty;
			prevDoc.CSI_Quantity = 0;
			prevDoc.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(prevDoc, messageErrorExpected);

			prevDoc.CSI_Quantity = 4;
			prevDoc.CSI_PackType = "AH";
			prevDoc.CSI_PackQty = 3;
			prevDoc.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(prevDoc, messageErrorExpected);

			prevDoc.CSI_PackType = ZString.Empty;
			prevDoc.CSI_Quantity = 0;
			entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS2;
			prevDoc.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(prevDoc, messageErrorExpected);

			entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
			prevDoc.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(prevDoc, messageErrorExpected);
		});
	}

	public void TestCheckNoEmptyDataWhenMoreThanOne_Declaration()
	{
		SetUpBulkType();

		CombineAssertions(() =>
		{
			var messageErrorExpected = "If there is more than one previous document per entry line, then Quantity, Pack Type and Pack Qty must be filled.";
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var prevDoc = declaration.PreviousDocuments.AddNew();

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var prevDoc2 = declaration.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "Z";
			prevDoc2.CSI_Quantity = 3;
			prevDoc2.CSI_PackType = "AH3";
			prevDoc2.CSI_PackQty = 2;
			prevDoc.Validation.ValidateAll();
			prevDoc2.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(prevDoc, messageErrorExpected);
			AssertNoRowMessageErrorContaining(prevDoc2, messageErrorExpected);

			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals(2, entryLine.PreviousDocuments.Count());
			prevDoc.Validation.ValidateAll();
			prevDoc2.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(prevDoc, messageErrorExpected);
			AssertNoRowMessageErrorContaining(prevDoc2, messageErrorExpected);

			prevDoc.CSI_PackType = "BK";
			prevDoc.CSI_Quantity = 3;
			prevDoc.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(prevDoc, messageErrorExpected);

			prevDoc.CSI_PackType = ZString.Empty;
			prevDoc.CSI_Quantity = 0;
			prevDoc.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(prevDoc, messageErrorExpected);

			prevDoc.CSI_Quantity = 4;
			prevDoc.CSI_PackType = "AH";
			prevDoc.CSI_PackQty = 3;
			prevDoc.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(prevDoc, messageErrorExpected);
		});
	}

	public void TestCheckNoEmptyDataWhenMultiplePackageTypes()
	{
		SetUpBulkType();

		CombineAssertions(() =>
		{
			var messageErrorExpected = "If there is more than one Package Type per entry line, then Quantity, Pack Type and Pack Qty must be filled.";
			var (dec, invLine, _, prevDoc) = CommonSetUpAndAssertT2C_Import_POUS_EntryLine(messageErrorExpected);
			var billPackingGroup = dec.Bills.AddNew().PackingGroups.AddNew();

			var package1 = dec.Packages.AddNew();
			package1.CW_CR_HouseContainer = billPackingGroup.PK;
			package1.CW_PackQty = 1;
			package1.CW_PackType = "AH3";

			var package2 = dec.Packages.AddNew();
			package2.CW_CR_HouseContainer = billPackingGroup.PK;
			package2.CW_PackQty = 1;
			package2.CW_PackType = "ZZ";

			var pivot1 = invLine.PackagesPivot.AddNew();
			pivot1.CHC_CW = package1.PK;
			pivot1.CHC_NumberOfPacks = 1;
			var pivot2 = invLine.PackagesPivot.AddNew();
			pivot2.CHC_CW = package2.PK;
			pivot2.CHC_NumberOfPacks = 1;

			prevDoc.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(prevDoc, messageErrorExpected);

			prevDoc.CSI_PackType = "BK";
			prevDoc.CSI_Quantity = 3;
			prevDoc.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(prevDoc, messageErrorExpected);

			prevDoc.CSI_PackType = ZString.Empty;
			prevDoc.CSI_Quantity = 0;
			prevDoc.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(prevDoc, messageErrorExpected);

			package2.CW_PackType = "AH3";
			prevDoc.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(prevDoc, messageErrorExpected);

			package2.CW_PackType = "ZZ";
			prevDoc.CSI_Quantity = 4;
			prevDoc.CSI_PackType = "AH";
			prevDoc.CSI_PackQty = 3;
			prevDoc.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(prevDoc, messageErrorExpected);
		});
	}

	void SetUpBulkType()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UnitedNationsPackageTypes");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
								  Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
								  "BK",
								  "Bulk",
								  ZDateTime.MinSmallDateTimeValue,
								  ZDateTime.MaxSmallDateTimeValue,
								  Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk,
								  "");
		Factory.Save();
	}

	(JobDeclaration, JobComInvoiceLine, CusEntryLine, PreviousDocument) CommonSetUpAndAssertT2C_Import_POUS_EntryLine(string messageErrorExpected)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		var previousDoc = invoiceLine.PreviousDocuments.AddNew();
		previousDoc.Validation.ValidateAll();
		AssertNoRowMessageErrorContaining(previousDoc, messageErrorExpected);

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		previousDoc.Validation.ValidateAll();
		AssertNoRowMessageErrorContaining(previousDoc, messageErrorExpected);

		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.AllEntryLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		AssertNotNull("IsImport T2C POUS and has entryLine", invoiceLine.CusEntryLine);
		previousDoc.Validation.ValidateAll();
		AssertNoRowMessageErrorContaining(previousDoc, messageErrorExpected);

		return (declaration, invoiceLine, entryLine, previousDoc);
	}

	void SetDataForPreviousDocument(PreviousDocument doc, string code, ZShort lineNo)
	{
		doc.CSI_Code = code;
		doc.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
		doc.CSI_LineNo = lineNo;
	}

	sealed class PreviousDocumentValidationForTest : PreviousDocumentValidation
	{
		public PreviousDocumentValidationForTest(PreviousDocument parent) : base(parent)
		{
		}

		public ZBool IsSubTypeMandatory_Exposed => IsSubTypeMandatory;
	}
}
