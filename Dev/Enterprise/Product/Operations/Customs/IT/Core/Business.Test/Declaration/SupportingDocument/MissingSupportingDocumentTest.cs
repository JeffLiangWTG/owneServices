using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class MissingSupportingDocumentTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when conditionValue parameter is null", () => new MissingSupportingDocument(null, Factory.New<JobComInvoiceLine>()));
		AssertExceptionThrown<ArgumentNullException>("Exception should be thrwon when conditionValue.Condition is null", () => new MissingSupportingDocument(Factory.New<RefCusConditionValue>(), Factory.New<JobComInvoiceLine>()));
		AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when invoiceLine parameter is null", () => new MissingSupportingDocument(Factory.New<RefCusConditionValue>(), null));

		AssertNoExceptionThrown("No exception expected", () => new MissingSupportingDocument(conditionValueForImport, Factory.New<JobComInvoiceLine>()));
	}

	public void TestCode()
	{
		conditionValueForImport.ZX3_Value = "CODE";

		var missingSupportingDocument = new MissingSupportingDocument(conditionValueForImport, invoiceLine);
		AssertEquals("Code", "CODE", missingSupportingDocument.Code);
	}

	public void TestDescriptionWithIMPDeclaration()
	{
		var conditionValue = Factory.New<RefCusCondition>().ConditionValues.AddNew();

		conditionValue.ZX3_Value = "C400";
		var missingSupportingDocument = new MissingSupportingDocument(conditionValue, invoiceLine);
		AssertEquals("When Code: C400, Description", ZString.Empty, missingSupportingDocument.Description);

		conditionValue.ZX3_Value = "Y900";
		missingSupportingDocument = new MissingSupportingDocument(conditionValue, invoiceLine);
		AssertEquals("When Code: Y900, Description", "Drugs Precursor Chemicals Individual Licence", missingSupportingDocument.Description);

		missingSupportingDocument = new MissingSupportingDocument(conditionValue, Factory.New<JobComInvoiceLine>());
		AssertEquals("When invoice line is orphan, Description", "", missingSupportingDocument.Description);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "";
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertEquals("When declaration has no message type, Description", "", missingSupportingDocument.Description);
	}

	public void TestDescriptionWithEXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var conditionValue = Factory.New<RefCusCondition>().ConditionValues.AddNew();

		conditionValue.ZX3_Value = "C400";
		var missingSupportingDocument = new MissingSupportingDocument(conditionValue, invoiceLine);
		AssertEquals("When Code: C400, Description", ZString.Empty, missingSupportingDocument.Description);

		conditionValue.ZX3_Value = "E900";
		missingSupportingDocument = new MissingSupportingDocument(conditionValue, invoiceLine);
		AssertEquals("When Code: E900, Description", "Musical Instruments for Loud Concerts", missingSupportingDocument.Description);

		missingSupportingDocument = new MissingSupportingDocument(conditionValue, Factory.New<JobComInvoiceLine>());
		AssertEquals("When invoice line is orphan, Description", "", missingSupportingDocument.Description);

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "";
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertEquals("When declaration has no message type, Description", "", missingSupportingDocument.Description);
	}

	public void TestCodeAndDescriptionWithIMPDeclaration()
	{
		conditionValueForImport.ZX3_Value = "C400";
		var missingSupportingDocument = new MissingSupportingDocument(conditionValueForImport, invoiceLine);
		AssertEquals("When Code: C400, CodeAndDescription", "C400", missingSupportingDocument.CodeAndDescription);

		conditionValueForImport.ZX3_Value = "Y900";
		missingSupportingDocument = new MissingSupportingDocument(conditionValueForImport, invoiceLine);
		AssertEquals("When Code: Y900, CodeAndDescription", "Y900 - Drugs Precursor Chemicals Individual Licence", missingSupportingDocument.CodeAndDescription);

		conditionValueForImport.ZX3_Value = "";
		missingSupportingDocument = new MissingSupportingDocument(conditionValueForImport, invoiceLine);
		AssertEquals("When Code is Empty, CodeAndDescription", "", missingSupportingDocument.CodeAndDescription);
	}

	public void TestCodeAndDescriptionWithEXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		conditionValueForExport.ZX3_Value = "C400";
		var missingSupportingDocument = new MissingSupportingDocument(conditionValueForExport, invoiceLine);
		AssertEquals("When Code: C400, CodeAndDescription", "C400", missingSupportingDocument.CodeAndDescription);

		conditionValueForExport.ZX3_Value = "E900";
		missingSupportingDocument = new MissingSupportingDocument(conditionValueForExport, invoiceLine);
		AssertEquals("When Code: Y900, CodeAndDescription", "E900 - Musical Instruments for Loud Concerts", missingSupportingDocument.CodeAndDescription);

		conditionValueForExport.ZX3_Value = "";
		missingSupportingDocument = new MissingSupportingDocument(conditionValueForExport, invoiceLine);
		AssertEquals("When Code is Empty, CodeAndDescription", ZString.Empty, missingSupportingDocument.CodeAndDescription);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Document Type (EU Box 44 Exports)");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Document Type (EU Box 44 Exports)");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Y900", "Drugs Precursor Chemicals Individual Licence", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "E900", "Musical Instruments for Loud Concerts", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();

		var conditionTypeForImport = Factory.New<RefCusConditionType>();
		var conditionForImport = Factory.New<RefCusCondition>();
		conditionForImport.ZX1_ZX2_ConditionType = conditionTypeForImport.PK;
		conditionValueForImport = conditionForImport.ConditionValues.AddNew();

		var conditionTypeForExport = Factory.New<RefCusConditionType>();
		var conditionForExport = Factory.New<RefCusCondition>();
		conditionForExport.ZX1_ZX2_ConditionType = conditionTypeForExport.PK;
		conditionValueForExport = conditionForExport.ConditionValues.AddNew();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
	}
	RefCusConditionValue conditionValueForImport;
	RefCusConditionValue conditionValueForExport;
	JobComInvoiceLine invoiceLine;
}
