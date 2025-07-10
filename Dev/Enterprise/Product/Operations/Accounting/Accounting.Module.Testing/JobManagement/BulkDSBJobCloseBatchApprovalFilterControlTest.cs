using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;

namespace Enterprise.Accounting.Module.Testing
{
	public class BulkDSBJobCloseBatchApprovalFilterControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			using (var filterControl = new BulkDSBJobCloseBatchApprovalFilterControl())
			{
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals(7, columns.Count());
				Assert(columns.Any(x => x.ColumnName == "JBB_BatchNumber"));
				Assert(columns.Any(x => x.ColumnName == "JBB_BatchStatus"));
				Assert(columns.Any(x => x.ColumnName == "JBB_TotalAmount"));
				Assert(columns.Any(x => x.ColumnName == "JBB_LargestAmount"));
				Assert(columns.Any(x => x.ColumnName == "JBB_SmallestAmount"));
				Assert(columns.Any(x => x.ColumnName == "JBB_ApprovalTime"));
				Assert(columns.Any(x => x.ColumnName == "JBB_GS_NKApprovingUser"));
			}
		}
	}
}
