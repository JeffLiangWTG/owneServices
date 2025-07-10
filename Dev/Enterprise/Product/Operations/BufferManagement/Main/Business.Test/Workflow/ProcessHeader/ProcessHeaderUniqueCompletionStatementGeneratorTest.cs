using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ProcessHeaderUniqueCompletionStatementGeneratorTest : TestCaseWithFactory
	{
		public void TestIOCConfiguration()
		{
			AssertType<ProcessHeaderUniqueCompletionStatementGenerator>(ObjectFactory.Get<IProcessHeaderUniqueCompletionStatementGenerator>());

			var instance1 = ObjectFactory.Get<IProcessHeaderUniqueCompletionStatementGenerator>();
			var instance2 = ObjectFactory.Get<IProcessHeaderUniqueCompletionStatementGenerator>();
			AssertEquals("Should be a singleton.", instance1, instance2);
		}

		public void TestGetUniqueCompletionStatement()
		{
			var newCompletionStatement = "My Workflow";
			var expectedCompletionStatement = "My Workflow (28)";

			var workitem = Factory.New<IWorkItem>() as IWorkflowProvider;
			var jobHeader = ProcessJobHeader.GetForParent(workitem, Factory);

			foreach (var statement in new[] { "My Workflow", "My Workflow (1)", "My Workflow (27)", "Not My Workflow", "My Workflow (Seven)", "My Workflow ()", "My Workflow )", "MY WORKFLOW (30)" })
			{
				var header = jobHeader.ProcessHeaders.AddNew();
				header.FH_CompletionStatement = statement;
			}

			var actualCompletionStatement = new ProcessHeaderUniqueCompletionStatementGenerator().GetUniqueCompletionStatement(jobHeader.ProcessHeaders, newCompletionStatement);
			AssertEquals(expectedCompletionStatement, actualCompletionStatement);
		}

		public void TestGetUniqueCompletionStatement_AlreadyUnique()
		{
			var newCompletionStatement = "My Workflow";

			var workitem = Factory.New<IWorkItem>() as IWorkflowProvider;
			var jobHeader = ProcessJobHeader.GetForParent(workitem, Factory);

			var header = jobHeader.ProcessHeaders.AddNew();
			header.FH_CompletionStatement = "Other";

			var actualCompletionStatement = new ProcessHeaderUniqueCompletionStatementGenerator().GetUniqueCompletionStatement(jobHeader.ProcessHeaders, newCompletionStatement);
			AssertEquals(newCompletionStatement, actualCompletionStatement);
		}
	}
}
