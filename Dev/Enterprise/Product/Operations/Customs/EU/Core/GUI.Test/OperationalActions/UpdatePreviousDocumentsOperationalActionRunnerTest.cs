using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.OperationalActions;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.EU.GUI.Testing.OperationalActions;

public class UpdatePreviousDocumentsOperationalActionRunnerTest : TestCaseWithFactory
{
	public void TestUpdatePreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarationReference = "B00000001";

		var applicator = new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		var runner = new UpdatePreviousDocumentsOperationalActionRunner();
		var log = new DummyOperationalActionSectionLog();

		applicator.DocumentCode = ZString.Empty;
		runner.UpdatePreviousDocuments(applicator, declaration, log);
		CombineAssertions("Runner behaviour when document code is empty", () =>
		{
			AssertEquals("Runner should not have added any previous document because of document code being empty.", 0, declaration.PreviousDocuments.Count);
			AssertEquals("INFO: Skipped previous document with empty code.", log.MessagesString());
		});

		log.messages.Clear();
		applicator.DocumentCode = "TST1";
		applicator.ReferenceNumber = "Reference1";
		applicator.Class = PreviousDocumentClassList.Codes.PreviousDocument;
		applicator.LineNo = 1;
		runner.UpdatePreviousDocuments(applicator, declaration, log);
		CombineAssertions("Runner behaviour when document can be added", () =>
		{
			AssertEquals("Runner should have added the previous document because there originally was none attached to the declaration.", 1, declaration.PreviousDocuments.Count);
			AssertEquals("INFO: Added previous document TST1 with reference Reference1 on declaration B00000001.", log.MessagesString());
		});

		var previousDocument1 = declaration.PreviousDocuments[0];
		CombineAssertions("Properties of added Previous document", () =>
		{
			AssertEquals("Code", "TST1", previousDocument1.CSI_Code);
			AssertEquals("Reference Number", "Reference1", previousDocument1.CSI_ReferenceNumber);
			AssertEquals("Class", PreviousDocumentClassList.Codes.PreviousDocument, previousDocument1.CSI_SubType);
			AssertEquals("Line No", 1, previousDocument1.CSI_LineNo);
		});

		log.messages.Clear();
		applicator.DocumentCode = "TST2";
		applicator.ReferenceNumber = "Reference2";
		applicator.Class = PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures;
		applicator.LineNo = 2;
		runner.UpdatePreviousDocuments(applicator, declaration, log);
		CombineAssertions("Runner behaviour when there is already a previous document attached to the declaration.", () =>
		{
			AssertEquals("Runner should not have added the previous document because there is already one attached to the declaration.", 1, declaration.PreviousDocuments.Count);
			AssertEquals("INFO: A previous document already exists on declaration B00000001.", log.MessagesString());
		});

		log.messages.Clear();
		declaration.PreviousDocuments.RemoveAndDeleteAll();
		applicator.DocumentCode = "TST3";
		applicator.ReferenceNumber = "Reference3";
		applicator.Class = PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures;
		applicator.LineNo = 3;
		runner.UpdatePreviousDocuments(applicator, declaration, log);

		CombineAssertions("Runner behaviour when sublmitted document code doesn't suit the declaration", () =>
		{
			AssertEquals("Runner should not have added the previous document because it's code is not in the list.", 0, declaration.PreviousDocuments.Count);
			AssertEquals("ERROR: Previous document code TST3 doesn't suit declaration B00000001.", log.MessagesString());
		});
	}

	public void TestUpdatePreviousDocuments_override()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarationReference = "B00000001";

		var applicator = new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		var runner = new UpdatePreviousDocumentsOperationalActionRunner();
		var log = new DummyOperationalActionSectionLog();

		applicator.OverrideExisitingDocument = true;
		applicator.DocumentCode = "TST1";
		applicator.ReferenceNumber = "Reference1";
		applicator.Class = PreviousDocumentClassList.Codes.PreviousDocument;
		applicator.LineNo = 1;
		runner.UpdatePreviousDocuments(applicator, declaration, log);

		log.messages.Clear();
		applicator.DocumentCode = "TST3";
		applicator.ReferenceNumber = "Reference3";
		applicator.Class = PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures;
		applicator.LineNo = 3;
		runner.UpdatePreviousDocuments(applicator, declaration, log);
		AssertEquals("should not have overrided the previous document", "ERROR: Previous document code TST3 doesn't suit declaration B00000001.", log.MessagesString());

		log.messages.Clear();
		applicator.DocumentCode = "TST2";
		applicator.ReferenceNumber = "Reference2";
		applicator.Class = PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures;
		applicator.LineNo = 2;
		runner.UpdatePreviousDocuments(applicator, declaration, log);

		CombineAssertions("Runner behaviour when there is already a previous document attached to the declaration and OverrideExisitingDocument is true.", () =>
		{
			AssertEquals("INFO: Overrode previous document TST2 with reference Reference2 on declaration B00000001.", log.MessagesString());
			var document = declaration.PreviousDocuments[0];
			AssertEquals("CSI_Code", "TST2", document.CSI_Code);
			AssertEquals("CSI_ReferenceNumber", "Reference2", document.CSI_ReferenceNumber);
			AssertEquals("CSI_SubType", PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures, document.CSI_SubType);
			AssertEquals("CSI_LineNo", 2, document.CSI_LineNo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var factory = new BusinessObjectFactory();
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "DC40E", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "TST1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "TST2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}
}
