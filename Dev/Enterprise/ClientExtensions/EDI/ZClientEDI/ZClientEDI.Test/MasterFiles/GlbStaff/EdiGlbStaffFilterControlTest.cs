using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;

namespace Enterprise.Client.EDI.MasterFiles.Testing
{
	public class EdiGlbStaffFilterControlTest : TestCaseWithFactory
	{
		public void TestColumnsCanBeDisplayed()
		{
			using (var filterControl = new EdiGlbStaffFilterControl())
			{
				var columns = filterControl.Grid.ColumnStyles;
				AssertHasColumn(columns, "DomesticName");
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
