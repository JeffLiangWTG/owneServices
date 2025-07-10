using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZGridCustomiseTest : OGridColumnGroupBaseTest
	{
		public void TestCustomise()
		{
			using (var grid = new ZGrid())
			using (var columns = new ZGridColumns(grid))
			{
				columns.Add(NewInfo("Column1", new ResourceStringData("", "Group1"), true));
				columns.Add(NewInfo("Column2", new ResourceStringData("", ""), true));
				columns.Add(NewInfo("Column3", new ResourceStringData("", "Group1"), false));

				using (var customiseForm = new ZGridCustomiseTester(columns, columns, grid))
				{
					AssertEquals("Available Columns Items", 0, customiseForm.AvailableColumns.Count);
					AssertEquals("Current Columns Items", 2, customiseForm.SelectedColumns.Count);

					var group1 = (ZGridColumnGroup)customiseForm.SelectedColumns[0];
					var column2Group = (ZGridColumnGroup)customiseForm.SelectedColumns[1];

					AssertEquals("Group1 is visible", true, group1.IsVisible);
					AssertEquals("Group1 name", "Group1", group1.GroupName.Caption);
					AssertEquals("Group1 Column 0 is Column1", columns[0], group1.Columns[0]);
					AssertEquals("Group1 Column 1 is Column3", columns[2], group1.Columns[1]);
					AssertEquals("Column2Group Column 0 is Column3", columns[1], column2Group.Columns[0]);

					customiseForm.SaveColumns();
					var result = customiseForm.Result;

					AssertEquals("Result Count", 3, result.Count);
					AssertEquals("Result Column 0 is original 0", columns[0], result[0]);
					AssertEquals("Result Column 1 is original 2", columns[2], result[1]);
					AssertEquals("Result Column 2 is original 1", columns[1], result[2]);
				}
			}
		}
	}
}
