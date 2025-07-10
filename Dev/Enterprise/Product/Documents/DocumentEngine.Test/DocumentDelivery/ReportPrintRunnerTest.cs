using CargoWise.EntityFramework.Testing;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ReportPrintRunnerTest : TestCaseWithFactory
	{
		public void TestCreateReport_ShouldReturnReport()
		{
			var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
			failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error).Build());
			var dataWrapper = new NonPersistentValidationFailureDocumentWrapper(failure.FailedRuleResults);

			var documentPack = new DocumentPack();
			string reportName = "Workflow Validation Failed Report";
			var report = ReportPrintRunner.CreateReport(documentPack, reportName, dataWrapper);

			AssertNotNull(report);
			AssertEquals("Workflow Validation Failed Report", report.Name);
			AssertType<ExcelTemplateReadFromStmTemplateTable>(report.Template);
		}
	}
}
