using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Module.TransactionApproval;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public class GLJournalFilterControlTest : TestCaseWithFactory
	{
		public void TestDefaultHiddenApprovalColumns()
		{
			using (var form = new ZForm())
			{
				var collection = new GLJournalCollection(Factory);
				var filterControl = new GLJournalFilterControl(collection, new GLJournalApprovalFilterBusinessObject());
				form.Controls.Add(filterControl);
				form.Show();

				Assert(filterControl.Grid.Columns.Count > 0);

				filterControl.Grid.CustomiseColumns();
				var customiseColumnsForm = (ZGridCustomise)ZFormModaliser.LastFormShownForTest;

				var button = (ZButton)customiseColumnsForm.Controls.Find("ResetButton", true)[0];
				button.PerformClick();
				button = (ZButton)customiseColumnsForm.Controls.Find("PostButton", true)[0];
				button.PerformClick();

				Assert(!filterControl.Grid.Columns.First(column => column.ColumnName == "OriginalRequest_RequesterFullName").IsVisible);
				Assert(!filterControl.Grid.Columns.First(column => column.ColumnName == "LastRequest_RequesterFullName").IsVisible);
				Assert(!filterControl.Grid.Columns.First(column => column.ColumnName == "OriginalPostedRequest_ApproverFullName").IsVisible);
			}
		}
	}
}
