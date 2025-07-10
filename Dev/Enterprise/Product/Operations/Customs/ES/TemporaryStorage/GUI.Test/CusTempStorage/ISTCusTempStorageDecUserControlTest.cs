using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	public class ISTCusTempStorageDecUserControlTest : TestCaseWithFactory
	{
		public void TestLineItemsGridColumns()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = "IST";
			var storageDec = CusTempStorageDec.New(header);
			storageDec.CusTempStorageLines.AddNew();

			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var userControlForPLugin = form.FindSingle<ISTTemporyStorageUserControlForPlugin>("userControlForPLugin");
				var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
				var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
				mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;
				var cusDecTabPageUserControl = userControlForPLugin.FindSingle<ISTCusTempStorageDecUserControl>("CusDecTabPageUserControl");
				var lineItemsGrid = cusDecTabPageUserControl.FindSingleOrDefault<ZGrid>(c => c.Name == "LineItemsGrid");

				var visibleColumnCount = lineItemsGrid.Columns.Where(x => x.IsVisible).Count();
				CombineAssertions(() =>
				{
					AssertEquals("Expected 6 columns", 6, visibleColumnCount);

					for (int i = 0; i < ExpectedDefaultColumnsForLineItemsGrid.Count; i++)
					{
						var column = lineItemsGrid.Columns[i];
						AssertNotNull("lineItemsGrid.Column", column);
						var expectedColumnName = ExpectedDefaultColumnsForLineItemsGrid[i];
						AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
						AssertEquals("IsVisible", true, column.IsVisible);
					}
				});
			}
		}

		List<string> ExpectedDefaultColumnsForLineItemsGrid
		{
			get
			{
				var columns = new List<string>();
				columns.Add(CusTempStorageLineItem.Schema.TSI_CommodityCode);
				columns.Add(CusTempStorageLineItem.Schema.TSI_NetWeight);
				columns.Add(CusTempStorageLineItem.Schema.TSI_NetWeightUQ);
				columns.Add(CusTempStorageLineItem.Schema.TSI_GoodsValue);
				columns.Add(CusTempStorageLineItem.Schema.TSI_GuaranteedValue);
				columns.Add(CusTempStorageLineItem.Schema.TSI_RX_NKCurrency);

				return columns;
			}
		}
	}
}
