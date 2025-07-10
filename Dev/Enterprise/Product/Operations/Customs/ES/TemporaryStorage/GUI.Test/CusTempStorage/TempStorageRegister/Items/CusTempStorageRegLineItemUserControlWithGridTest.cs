using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.TemporaryStorage.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	public class CusTempStorageRegLineItemUserControlWithGridTest : TestCaseWithFactory
	{
		public void TestItemsGrid()
		{
			using var userControl = new CusTempStorageRegLineItemUserControlWithGrid();
			var itemsGrid = userControl.ItemsGrid;
			CombineAssertions(() =>
			{
				const string linePrefix = "RegLine";
				const string itemPrefix = "RegLineItem";
				void AssertColumnStyleIsVisible(string columnName, string columnPrefix = "")
				{
					var columnStyleName = string.IsNullOrEmpty(columnPrefix) ? columnName : $"{columnPrefix}+{columnName}";
					AssertEquals($"{columnName} is Visible", expected: true, itemsGrid.GetColumnStyle(columnStyleName).IsVisible);
				}

				AssertEquals("Column Count", 6, itemsGrid.ColumnStyles.Count);
				AssertColumnStyleIsVisible(CusTempStorageRegLineItem.Schema.SRI_GoodsItemNumber, itemPrefix);
				AssertColumnStyleIsVisible(CusTempStorageRegLineItem.Schema.FormattedTariff, itemPrefix);
				AssertColumnStyleIsVisible(CusTempStorageRegLineItem.Schema.SRI_CusC4Number, itemPrefix);
				AssertColumnStyleIsVisible(CusTempStorageRegLineItem.Schema.SRI_GoodsDescription, itemPrefix);
				AssertColumnStyleIsVisible(CusTempStorageRegLineItem.Schema.SRI_GoodsItemNumber, itemPrefix);
				AssertColumnStyleIsVisible(CusTempStorageRegLineItemPivot.Schema.SRV_GrossWeight);
				AssertColumnStyleIsVisible(CusTempStorageRegLine.Schema.SRL_GrossWeightUQ, linePrefix);

				AssertEquals("Bind to", "CusTempStorageRegLines.RegLineItemPivots", itemsGrid.BindTo);
			});
		}

		public void TestItemDetailsLayoutControl()
		{
			using var userControl = new CusTempStorageRegLineItemUserControlWithGrid();
			AssertType<CusTempStorageRegLineItemDetailsLayoutControl>(userControl.ItemDetailsLayoutControl);
		}
	}
}
