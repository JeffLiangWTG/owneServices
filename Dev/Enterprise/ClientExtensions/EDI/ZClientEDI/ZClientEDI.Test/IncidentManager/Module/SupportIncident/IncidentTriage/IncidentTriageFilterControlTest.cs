using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Core.Forms;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	public class IncidentTriageFilterControlTest : TestCaseWithFactory
	{
		public void TestColumnsCanBeDisplayed()
		{
			using (var filterControl = new IncidentTriageFilterControl(new IncidentTriageCollection(Factory), new IncidentTriageFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				AssertHasColumn(columns, "IMT_TriageNumber");
				AssertHasColumn(columns, "IMT_Type");
				AssertHasColumn(columns, "TypeDescription");
				AssertHasColumn(columns, "IMT_SupportDescription");
				AssertHasColumn(columns, "IMT_Product");
				AssertHasColumn(columns, "ProductDescription");
				AssertHasColumn(columns, "IMT_Module");
				AssertHasColumn(columns, "ModuleDescription");
				AssertHasColumn(columns, "IMT_IsActive");
				AssertHasColumn(columns, "IMT_ProductArea");
				AssertHasColumn(columns, "ProductAreaDescription");
				AssertHasColumn(columns, "IMT_IsInternal");
				AssertHasColumn(columns, "IMT_IsPublishedToAssist");
				AssertHasColumn(columns, "IMT_IsPublished");
				AssertHasColumn(columns, "IMT_Level");
				AssertHasColumn(columns, "LevelDescription");
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
