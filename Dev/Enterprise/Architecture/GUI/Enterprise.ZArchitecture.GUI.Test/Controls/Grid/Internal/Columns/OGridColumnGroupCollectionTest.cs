using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Core.Forms
{
	sealed class OGridColumnGroupCollectionTest : OGridColumnGroupBaseTest
	{
		public void TestCollection()
		{
			using (var grid = new ZGrid())
			using (var columns = new ZGridColumns(grid))
			{
				columns.Add(NewInfo("Column1", new ResourceStringData("", "Group1"), true));
				columns.Add(NewInfo("Column2", new ResourceStringData("", ""), true));
				columns.Add(NewInfo("Column3", new ResourceStringData("", "Group1"), true));

				var collection = new ZGridColumnGroupCollection(columns);

				var group = collection[new ResourceStringData("", "Group1")];
				AssertEquals("Group1 count", 2, group.Columns.Length);
			}
		}
	}
}
