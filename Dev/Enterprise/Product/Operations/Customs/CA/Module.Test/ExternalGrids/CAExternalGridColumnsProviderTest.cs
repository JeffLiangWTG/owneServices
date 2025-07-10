using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.Testing
{
	abstract class CAExternalGridColumnsProviderTest : TestCaseWithFactory
	{
		protected void AssertColumn(ZGrid grid, string columnName, ResourceStringData groupName, string caption, bool visible)
		{
			var column = grid.Columns[columnName];
			AssertEquals(columnName + " GroupName", groupName.Caption, column.GroupName.Caption);
			AssertEquals(columnName + " Caption", caption, column.ColumnStyle.HeaderText);
			AssertEquals(columnName + " Visible", visible, column.IsVisible);
		}

		protected void AssertColumnValue(ZGrid grid, string columnName, int rowNum, string value)
		{
			var column = grid.Columns[columnName];

			column.IsVisible = true;
			grid.RefreshTableStyles();

			AssertEquals(value, ((ZGridColumnStyle)column.ColumnStyle).GetValueAsString(grid.ListManager, rowNum));
		}
	}
}
