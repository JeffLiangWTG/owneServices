using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.BR.Module.Testing
{
	class GoodsCatalogFilterStripControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var catalog = new BaseCusGoodsCatalogCollection<CusGoodsCatalog>(Factory);
			var filterBO = new GoodsCatalogFilterBusinessObject();
			using (var userControl = new GoodsCatalogFilterStripControl(catalog, filterBO))
			{
				var grid = userControl.FilteredGrid;
				AssertColumnStyle(grid, CusGoodsCatalog.Schema.LocalPartNumbersConcatenated, true, "Local Part Numbers");
			}

			void AssertColumnStyle(ZGrid grid, string columnName, bool isVisible, string caption)
			{
				var column = grid.GetColumnStyle(columnName);

				AssertNotNull($"Column {columnName} should be added", column);
				AssertEquals($"Visibility of Column {columnName}", isVisible, column.IsVisible);
				AssertEquals($"Caption of Column {columnName}", caption, column.CaptionResourceString.Caption);
			}
		}
	}
}
