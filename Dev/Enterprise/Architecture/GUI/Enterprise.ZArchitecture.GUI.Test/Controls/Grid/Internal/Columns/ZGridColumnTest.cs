using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZGridColumnTest : NUnit.Framework.TestCase
	{
		public void TestILayoutDetailTreeNode()
		{
			var column = new ZGridColumn();

			using (var columnStyle = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo("Column", 150)))
			{
				columnStyle.HeaderText = "A";
				column.ColumnStyle = columnStyle;

				ILayoutDetailTreeNode ilayoutTreeNode = column;
				AssertEquals("UniqueID", "A", ilayoutTreeNode.UniqueID);
				AssertEquals("", ilayoutTreeNode.ParentUniqueID);

				column.GroupName = new ResourceStringData("", "B");
				AssertEquals("UniqueID", "B", ilayoutTreeNode.UniqueID);
				AssertEquals("", ilayoutTreeNode.ParentUniqueID);
			}
		}
	}
}
