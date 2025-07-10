using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class AmendedItemsUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumn()
		{
			using (var userControl = new AmendedItemsUserControl())
			{
				var grid = userControl.AmendedItemsGrid;

				var index = 0;
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(AmendedItem.AmendTypeDescription));
				AssertEquals(((ZMultiLineTextBoxColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(AmendedItem.ID));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(AmendedItem.DataItemID));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(AmendedItem.DataItemDescription));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(AmendedItem.BeforeValue));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(AmendedItem.AfterValue));
				AssertEquals(false, grid.GetColumnStyle(nameof(AmendedItem.DataItemID)).IsVisible);
			}
		}
	}
}
