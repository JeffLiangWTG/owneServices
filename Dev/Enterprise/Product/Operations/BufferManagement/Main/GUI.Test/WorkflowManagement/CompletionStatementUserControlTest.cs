using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class CompletionStatementUserControlTest : BMSTestCaseWithFactory
	{
		public void TestCompletionStatementCollection_AddNew()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "WorkFlow");

			Factory.Save();

			using (var form = new ZForm())
			using (var control = new CompletionStatementsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				form.SetDataBinding(workflow.CompletionStatementTasksIncludingChildWorkflowTasks, string.Empty);

				var completionStatementGrid = control.FindSingle<ZGrid>("CompletionStatementsGrid").ListManager;
				completionStatementGrid.AddNew();
				Application.DoEvents();

				var newCompletionStatement = (ProcessTask)completionStatementGrid.Current;
				AssertNoErrors("Our new completion statement should have no errors", newCompletionStatement);
				AssertNoWarnings("Our new completion statement should have no warnings", newCompletionStatement);

				newCompletionStatement.P9_Description = "oWo"; // need to manually add this
				newCompletionStatement.Validation.ValidateAll();

				AssertNoErrors("Our new completion statement should still have no errors", newCompletionStatement);
				AssertNoWarnings("Our new completion statement should still have no warnings", newCompletionStatement);
			}
		}

		public void TestP9_StatusShouldBeUpperCase()
		{
			using (var control = new CompletionStatementsUserControl())
			{
				AssertEquals(CharacterCasing.Upper, control.CompletionStatementsGrid.ColumnStyles.OfType<ZDropEditColumnStyleInfo>().Single(c => c.ColumnName == "P9_Status").CharacterCasing);
			}
		}
	}
}
