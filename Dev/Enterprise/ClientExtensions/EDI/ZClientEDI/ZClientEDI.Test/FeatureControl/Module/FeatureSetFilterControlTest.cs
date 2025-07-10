using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Core.Forms;

namespace Enterprise.Client.EDI.FeatureControl.Module.Testing
{
	public class FeatureSetFilterControlTest : TestCaseWithFactory
	{
		public void TestColumnsCanBeDisplayed()
		{
			using (var filterControl = new FeatureSetFilterControl(new FeatureControlSetCollection(Factory), new FeatureSetFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				AssertHasColumn(columns, "FCS_ProductName");
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
