using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;

namespace Enterprise.Accounting.Module.Testing
{
	public class OrgCollectionCallsFilterControlTest : TestCaseWithFactory
	{
		public void TestColumnAdditionalCompanyNameAddedAndVisible()
		{
			using (var filterControl = new OrgCollectionCallsFilterControl())
			{
				var expectedColumn = "AdditionalCompanyName";
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("New column 'AdditionalCompanyName' should be added.'", columns.Any(x => x.ColumnName == expectedColumn));
				Assert("New column 'AdditionalCompanyName' should be visible.", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
			}
		}
	}
}
