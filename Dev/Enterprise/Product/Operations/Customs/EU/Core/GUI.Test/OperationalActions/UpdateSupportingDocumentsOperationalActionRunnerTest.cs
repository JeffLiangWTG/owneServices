using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.OperationalActions;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.EU.GUI.Testing.OperationalActions;

public class UpdateSupportingDocumentsOperationalActionRunnerTest : TestCaseWithFactory
{
	public void TestUpdateSupportingDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarationReference = "B00000001";

		var applicator = new DeclarationUpdateSupportingDocumentsApplicator(Factory);
		var log = new DummyOperationalActionSectionLog();
		var runner = new UpdateSupportingDocumentsOperationalActionRunner();

		applicator.DocumentCode = ZString.Empty;
		runner.UpdateSupportingDocuments(applicator, declaration, log);
		CombineAssertions(() =>
		{
			AssertEquals("Applicator should not have added any supporting document because of document code being empty.", 0, declaration.SupportingDocuments.Count);
			AssertEquals("INFO: Skipped supporting document with empty code.", log.MessagesString());
		});

		log.messages.Clear();
		applicator.DocumentCode = "Doc1";
		applicator.ReferenceNumber = "Reference1";
		applicator.Date = ZDateTime.BrettsBirthday;
		applicator.OverrideExisitingDocument = true;
		runner.UpdateSupportingDocuments(applicator, declaration, log);
		AssertEquals(1, declaration.SupportingDocuments.Count);
		var supportingDocument1 = declaration.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault();
		CombineAssertions("Properties of added Supporting document", () =>
		{
			AssertEquals("Code", "Doc1", supportingDocument1.CSI_Code);
			AssertEquals("Reference Number", "Reference1", supportingDocument1.CSI_ReferenceNumber);
			AssertEquals("Date", ZDateTime.BrettsBirthday, supportingDocument1.CSI_DateOfIssue);
		});

		AssertEquals("Runner output message", "INFO: Added supporting document Doc1 with reference Reference1 on declaration B00000001.", log.MessagesString());

		log.messages.Clear();
		applicator.ReferenceNumber = "Reference2";
		applicator.Date = ZDateTime.BrettsBirthday.AddDays(1);
		runner.UpdateSupportingDocuments(applicator, declaration, log);
		CombineAssertions("Properties of updated Supporting document", () =>
		{
			AssertEquals("Reference Number", "Reference2", supportingDocument1.CSI_ReferenceNumber);
			AssertEquals("Date", ZDateTime.BrettsBirthday.AddDays(1), supportingDocument1.CSI_DateOfIssue);
		});

		AssertEquals("Runner output message", "INFO: Updated supporting document Doc1 with reference Reference2 on declaration B00000001.", log.MessagesString());

		log.messages.Clear();
		applicator.ReferenceNumber = "Reference3";
		applicator.Date = ZDateTime.BrettsBirthday.AddDays(2);
		applicator.OverrideExisitingDocument = false;
		runner.UpdateSupportingDocuments(applicator, declaration, log);
		CombineAssertions("Properties of updated Supporting document", () =>
		{
			AssertEquals("Reference2", supportingDocument1.CSI_ReferenceNumber);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), supportingDocument1.CSI_DateOfIssue);
		});

		AssertEquals("Runner output message", "INFO: Skipped updating supporting document Doc1 on declaration B00000001 as it already exists.", log.MessagesString());
	}
}
