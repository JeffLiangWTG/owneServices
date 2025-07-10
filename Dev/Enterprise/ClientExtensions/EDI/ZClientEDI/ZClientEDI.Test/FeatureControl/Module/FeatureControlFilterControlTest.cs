using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Core.Forms;

namespace Enterprise.Client.EDI.FeatureControl.Module.Testing
{
	public class FeatureControlFilterControlTest : TestCaseWithFactory
	{
		public void TestColumnsCanBeDisplayed()
		{
			using (var filterControl = new FeatureControlFilterControl(new FeatureControlHeaderCollection(Factory), new FeatureControlFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				AssertHasColumn(columns, "FCM_FeatureControlCode");
				AssertHasColumn(columns, "FCM_Description");
				AssertHasColumn(columns, "FCM_GG_ReleaseGroup");
				AssertHasColumn(columns, "ReleaseGroup+GG_Desc");
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
