using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Core.Forms
{
	public abstract class OGridColumnGroupBaseTest : TestCaseWithDummy
	{
		protected ZGridColumn NewColumn(string columnName, ResourceStringData groupName, bool isVisible, bool isCustomColumn = false)
		{
			var column = new ZGridColumn();
			column.ColumnStyle = new ZTextBoxColumnStyle(NewInfo(columnName, groupName, isVisible, isCustomColumn));

			// these properties are normally set when adding to ZGridColumns
			column.ColumnStyle.HeaderText = columnName;
			column.GroupName = groupName;
			column.IsVisible = isVisible;
			column.IsCustomColumn = isCustomColumn;

			return column;
		}

		protected ZTextBoxColumnStyleInfo NewInfo(string columnName, ResourceStringData groupName, bool isVisible, bool isCustomColumn = false)
		{
			var info = new ZTextBoxColumnStyleInfo(columnName, 80);
			info.GroupName = groupName;
			info.IsVisible = isVisible;
			info.IsCustomColumn = isCustomColumn;

			return info;
		}
	}
}
