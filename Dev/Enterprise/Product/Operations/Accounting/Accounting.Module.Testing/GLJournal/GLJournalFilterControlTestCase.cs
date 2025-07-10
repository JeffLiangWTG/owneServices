using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;

namespace Enterprise.Accounting.Module.Testing
{
	public class GLJournalFilterControlTestCase : TestCaseWithFactory
	{
		public void TestColumnAuditedByAddedCorrectly()
		{
			using (var filterControl = new GLJournalFilterControl())
			{
				var expectedColumn = "AH_GS_NKAuditedBy";
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var colAuditedBy = columns.FirstOrDefault(x => x.ColumnName == expectedColumn);
				AssertNotNull("New column 'AH_GS_NKAuditedBy' should be added.", colAuditedBy);
				Assert("New column 'AH_GS_NKAuditedBy' should be visible", colAuditedBy.IsVisible);
				Assert("New column 'AH_GS_NKAuditedBy' should not be read only", !colAuditedBy.IsReadOnly);
			}
		}

		public void TestColumnPostDateAddedCorrectly()
		{
			using (var filterControl = new GLJournalFilterControl())
			{
				var expectedColumn = "AH_PostDate";
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var colPostDate = columns.FirstOrDefault(x => x.ColumnName == expectedColumn);
				AssertNotNull("New column 'AH_PostDate' should be added.", colPostDate);
				Assert("New column 'AH_PostDate' should be visible", colPostDate.IsVisible);
				Assert("New column 'AH_PostDate' should not be read only", !colPostDate.IsReadOnly);
			}
		}
	}
}
