using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;

namespace Enterprise.Accounting.Module.Testing
{
	public class JobRevenueJournalFilterControlTestCase : TestCaseWithFactory
	{
		public void TestColumnAuditedByAddedCorrectly()
		{
			using (var filterControl = new JobRevenueJournalFilterControl())
			{
				var expectedColumn = "AH_GS_NKAuditedBy";
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("New column 'Audited By' should be added.", columns.Any(x => x.ColumnName == expectedColumn));
				Assert("New column 'Audited By' should be visible", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
				Assert("New column 'Audited By' should not be read only", !columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsReadOnly);
			}
		}
	}
}
