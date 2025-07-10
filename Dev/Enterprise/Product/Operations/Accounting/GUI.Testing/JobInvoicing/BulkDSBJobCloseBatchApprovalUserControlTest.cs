using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing
{
	public class BulkDSBJobCloseBatchApprovalUserControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			using (var filterControl = new BulkDSBJobCloseBatchApprovalUserControl())
			{
				var columns = filterControl.JobGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals(30, columns.Count());
				Assert(columns.Any(x => x.ColumnName == "TotalRevenue"));
				Assert(columns.Any(x => x.ColumnName == "TotalWIP"));
				Assert(columns.Any(x => x.ColumnName == "TotalCost"));
				Assert(columns.Any(x => x.ColumnName == "TotalAccrual"));
				Assert(columns.Any(x => x.ColumnName == "TotalLineAmount"));
				Assert(columns.Any(x => x.ColumnName == "TotalMargin"));
				Assert(columns.Any(x => x.ColumnName == "JH_ProfitLossReasonCode"));
				Assert(columns.Any(x => x.ColumnName == "RevRecognized"));
				Assert(columns.Any(x => x.ColumnName == "RevNotRecognized"));
				Assert(columns.Any(x => x.ColumnName == "CstRecognized"));
				Assert(columns.Any(x => x.ColumnName == "CstNotRecognized"));
				Assert(columns.Any(x => x.ColumnName == "WipRecognized"));
				Assert(columns.Any(x => x.ColumnName == "WipNotRecognized"));
				Assert(columns.Any(x => x.ColumnName == "AcrRecognized"));
				Assert(columns.Any(x => x.ColumnName == "AcrNotRecognized"));
				Assert(columns.Any(x => x.ColumnName == "ProfitLossRecognized"));
				Assert(columns.Any(x => x.ColumnName == "ProfitLossNotRecognized"));
				Assert(columns.Any(x => x.ColumnName == "JH_JobNum"));
				Assert(columns.Any(x => x.ColumnName == "JH_Status"));
				Assert(columns.Any(x => x.ColumnName == "JH_GB"));
				Assert(columns.Any(x => x.ColumnName == "JH_GE"));
				Assert(columns.Any(x => x.ColumnName == "JH_A_JOP"));
				Assert(columns.Any(x => x.ColumnName == "JH_A_JCL"));
				Assert(columns.Any(x => x.ColumnName == "JH_GS_NKRepOps"));
				Assert(columns.Any(x => x.ColumnName == "JH_GS_NKRepSales"));
				Assert(columns.Any(x => x.ColumnName == "LocalChargesPK"));
				Assert(columns.Any(x => x.ColumnName == "AgentCollectPK"));
				Assert(columns.Any(x => x.ColumnName == "RevenueRecognitionDates"));
				Assert(columns.Any(x => x.ColumnName == "JH_JobLocalReference"));
				Assert(columns.Any(x => x.ColumnName == "DSBSurplusAndShortfallAmount"));
			}
		}
	}
}
