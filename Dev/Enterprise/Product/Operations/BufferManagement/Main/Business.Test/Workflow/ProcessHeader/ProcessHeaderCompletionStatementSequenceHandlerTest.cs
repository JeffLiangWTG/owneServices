using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ProcessHeaderCompletionStatementSequenceHandlerTest : TestCaseWithFactory
	{
		ZString GetUniqueCompletionStatementForReappliedWorkflowTemplate(string newCompletionStatement, params string[] currentCompletionStatements)
		{
			var workitem = Factory.New<IWorkItem>() as IWorkflowProvider;
			var jobHeader = ProcessJobHeader.GetForParent(workitem, Factory);

			foreach (var statement in currentCompletionStatements)
			{
				var header = jobHeader.ProcessHeaders.AddNew();
				header.FH_CompletionStatement = statement;
			}

			return ProcessHeaderCompletionStatementSequenceHandler.GetUniqueCompletionStatementForReappliedWorkflowTemplate(jobHeader.ProcessHeaders, newCompletionStatement);
		}

		public void TestUniqueCompletionStatementForReappliedWorkflowTemplate()
		{
			var completionStatements = new[] { "My Workflow", "My Workflow (1)", "My Workflow (27)", "Not My Workflow", "My Workflow (Seven)", "My Workflow ()", "My Workflow )", "MY WORKFLOW (30)" };
			var newCompletionStatement = "My Workflow";
			var expectedCompletionStatement = "My Workflow (28)";

			var actualCompletionStatement = GetUniqueCompletionStatementForReappliedWorkflowTemplate(newCompletionStatement, completionStatements);

			AssertEquals(expectedCompletionStatement, actualCompletionStatement);
		}

		public void TestUniqueCompletionStatementForReappliedWorkflowTemplate_FormatMustMatchIncudingWhitespace()
		{
			var completionStatements = new[] { "My Workflow", " My Workflow (1)", "My  Workflow (2)", "My Workflow(3)", "My Workflow ( 4)", "My Workflow (5 )" };
			//no case with trailing whitespace as it is removed when setting FH_CompletionStatement
			var newCompletionStatement = "My Workflow";
			var expectedCompletionStatement = "My Workflow (1)";

			var actualCompletionStatement = GetUniqueCompletionStatementForReappliedWorkflowTemplate(newCompletionStatement, completionStatements);

			AssertEquals(expectedCompletionStatement, actualCompletionStatement);
		}

		public void TestUniqueCompletionStatementForReappliedWorkflowTemplate_DoNotOverflow()
		{
			var completionStatements = new[] { "My Workflow", "My Workflow (100)", "My Workflow (2545454545454545454)" };
			var newCompletionStatement = "My Workflow";
			var expectedCompletionStatement = "My Workflow (101)";

			var actualCompletionStatement = GetUniqueCompletionStatementForReappliedWorkflowTemplate(newCompletionStatement, completionStatements);

			AssertEquals(expectedCompletionStatement, actualCompletionStatement);
		}

		public void TestUnqiueCompletionStatement_HandledDuringTemplateApplication()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var templateJobHeader = template.GetJobHeader();
			var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow1.FH_CompletionStatement = "Workflow";
			BMSTestHelper.CreateTask(template, templateWorkflow1);
			var templateWorkflow2 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow2.FH_CompletionStatement = "Other Workflow";
			BMSTestHelper.CreateTask(template, templateWorkflow2);
			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Do the thing.";
			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, Factory);
			AssertNotNull("Pre-Conition: Template is applied", jobHeader);
			AssertEquals("Pre-Conition: Template is applied", 2, jobHeader.ProcessHeaders.Count);

			var templateApplicationParameters = new TemplateApplicationParametersForTest();
			templateApplicationParameters.ReapplyProcessHeaders = false;
			jobHeader.ApplyTemplate(template, null);
			jobHeader.ApplyTemplate(template, templateApplicationParameters);
			AssertEquals("Do no reapply workflows if parameters is null or ReapplyProcessHeaders = false", 2, jobHeader.ProcessHeaders.Count);

			templateApplicationParameters.ReapplyProcessHeaders = true;
			jobHeader.ApplyTemplate(template, templateApplicationParameters);
			AssertEquals("Workflows from template are reapplied when parameters.ReapplyProcessHeaders = true", 4, jobHeader.ProcessHeaders.Count);
			AssertContainsExactElementsInAnyOrder(new [] { "Workflow" , "Other Workflow" , "Workflow (1)", "Other Workflow (1)" }, jobHeader.ProcessHeaders.Select(x => x.FH_CompletionStatement));
		}

		class TemplateApplicationParametersForTest : IWorkflowTemplateApplicationParameters
		{
			public bool ReapplyProcessHeaders { get; set; }
		}
	}
}
