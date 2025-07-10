using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	class ISTAndLADTCusTempStorageDecUserControlTest : TestCaseWithFactory
	{
		public void TestDeclarationStatusTextBoxCaption()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			header.CreateRelatedCusTempStorageDec();

			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var userControlForPLugin = form.FindSingle<ISTTemporyStorageUserControlForPlugin>("userControlForPLugin");
				var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
				var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
				mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;

				var cusDecTabPageUserControl = userControlForPLugin.FindSingle<ISTAndLADTCusTempStorageDecUserControl>("CusDecTabPageUserControl");
				var declarationStatusDropEdit = cusDecTabPageUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "DeclarationStatusDropEdit");
				AssertNotNull(declarationStatusDropEdit);
				AssertEquals("Declaration Status", declarationStatusDropEdit.CaptionResourceString.Caption);
			}
		}

		public void TestLinesGridVisible()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			header.CreateRelatedCusTempStorageDec();

			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var userControlForPLugin = form.FindSingle<ISTTemporyStorageUserControlForPlugin>("userControlForPLugin");
				var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
				var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
				mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;

				var cusDecTabPageUserControl = userControlForPLugin.FindSingle<ISTAndLADTCusTempStorageDecUserControl>("CusDecTabPageUserControl");
				var linesGrid = cusDecTabPageUserControl.FindSingleOrDefault<ZGrid>(c => c.Name == "LinesGrid");
				var linesBottomPanel = cusDecTabPageUserControl.FindSingleOrDefault<ZPanel>(c => c.Name == "LinesBottomPanel");

				AssertEquals(true, linesGrid.Visible);
				AssertEquals(DockStyle.Top, linesBottomPanel.Dock);
			}
		}

		public void TestGoodsLocationColumn()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			header.CreateRelatedCusTempStorageDec();

			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var userControlForPLugin = form.FindSingle<ISTTemporyStorageUserControlForPlugin>("userControlForPLugin");
				var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
				var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
				mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;
				var cusDecTabPageUserControl = userControlForPLugin.FindSingle<ISTAndLADTCusTempStorageDecUserControl>("CusDecTabPageUserControl");
				var linesGrid = cusDecTabPageUserControl.FindSingleOrDefault<ZGrid>(c => c.Name == "LinesGrid");
				var goodsLocationColumn = (linesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == CusTempStorageLine.Schema.TSL_LocationOfGoods));
				AssertNotNull(goodsLocationColumn);
				AssertType<ZDropEditColumnStyleInfo>(goodsLocationColumn);
			}
		}

		public void TestGoodsLocationDropEdit()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			header.CreateRelatedCusTempStorageDec();

			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var userControlForPLugin = form.FindSingle<ISTTemporyStorageUserControlForPlugin>("userControlForPLugin");
				var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
				var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
				mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;
				var cusDecTabPageUserControl = userControlForPLugin.FindSingle<ISTAndLADTCusTempStorageDecUserControl>("CusDecTabPageUserControl");
				var goodsLocationTextBox = cusDecTabPageUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "GoodsLocationTextBox");
				AssertNull("GoodsLocationTextBox should be removed in IST dec.", goodsLocationTextBox);
				var goodsLocationDropEdit = cusDecTabPageUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "GoodsLocationDropEdit");
				AssertNotNull("Instead we have a GoodsLocationDropEdit in IST dec.", goodsLocationDropEdit);
			}
		}

		public void TestDDTNumberFieldIsPresentInIST()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			header.CreateRelatedCusTempStorageDec();

			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var userControlForPLugin = form.FindSingle<ISTTemporyStorageUserControlForPlugin>("userControlForPLugin");
				var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
				var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
				mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;

				var cusDecTabPageUserControl = userControlForPLugin.FindSingle<CusTempStorageDecUserControl>("CusDecTabPageUserControl");
				var statusTextBox = cusDecTabPageUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "DDTNumberTextBox");
				AssertNotNull(statusTextBox);
				Assert(statusTextBox.ReadOnly);
				Assert(statusTextBox.Visible);
			}

			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeLAD;
			header.CreateRelatedCusTempStorageDec();

			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var userControlForPLugin = form.FindSingle<CINTemporyStorageUserControlForPlugin>("userControlForPLugin");
				var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
				var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
				mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;

				var cusDecTabPageUserControl = userControlForPLugin.FindSingle<CusTempStorageDecUserControl>("CusDecTabPageUserControl");
				var statusTextBox = cusDecTabPageUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "DDTNumberTextBox");
				AssertNotNull(statusTextBox);
				Assert(!statusTextBox.Visible);
			}
		}

		#region LineItemsGrid
		public void TestLineItemsGridColumns()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			header.CreateRelatedCusTempStorageDec();

			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var userControlForPLugin = form.FindSingle<ISTTemporyStorageUserControlForPlugin>("userControlForPLugin");
				var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
				var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
				mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;
				var cusDecTabPageUserControl = userControlForPLugin.FindSingle<ISTAndLADTCusTempStorageDecUserControl>("CusDecTabPageUserControl");
				var lineItemsGrid = cusDecTabPageUserControl.FindSingleOrDefault<ZGrid>(c => c.Name == "LineItemsGrid");

				var visibleColumnCount = lineItemsGrid.Columns.Where(x => x.IsVisible).Count();
				AssertEquals(7, visibleColumnCount);

				for (int i = 0; i < ExpectedDefaultColumnsForLineItemsGrid.Count; i++)
				{
					var column = lineItemsGrid.Columns[i];
					AssertNotNull(column);
					var expectedColumnName = ExpectedDefaultColumnsForLineItemsGrid[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", true, column.IsVisible);
				}
			}
		}

		List<string> ExpectedDefaultColumnsForLineItemsGrid
		{
			get
			{
				var columns = new List<string>();
				columns.Add(CusTempStorageLineItem.Schema.TSI_CommodityCode);
				columns.Add(CusTempStorageLineItem.Schema.TSI_GoodsOrigin);
				columns.Add(CusTempStorageLineItem.Schema.TSI_NetWeight);
				columns.Add(CusTempStorageLineItem.Schema.TSI_NetWeightUQ);
				columns.Add(CusTempStorageLineItem.Schema.TSI_GoodsValue);
				columns.Add(CusTempStorageLineItem.Schema.TSI_GuaranteedValue);
				columns.Add(CusTempStorageLineItem.Schema.TSI_RX_NKCurrency);

				return columns;
			}
		}

		public void TestTSI_CommodityCodeColumn()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			header.CreateRelatedCusTempStorageDec();

			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var userControlForPLugin = form.FindSingle<ISTTemporyStorageUserControlForPlugin>("userControlForPLugin");
				var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
				var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
				mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;
				var cusDecTabPageUserControl = userControlForPLugin.FindSingle<ISTAndLADTCusTempStorageDecUserControl>("CusDecTabPageUserControl");
				var lineItemsGrid = cusDecTabPageUserControl.FindSingleOrDefault<ZGrid>(c => c.Name == "LineItemsGrid");
				var commodityCodeColumn = (Universal.GUI.TariffColumnStyleInfo)(lineItemsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == CusTempStorageLineItem.Schema.TSI_CommodityCode));
				AssertEquals(Core.Constants.CountryCodes.France, commodityCodeColumn.GetDataGrouping());
				AssertEquals(Universal.Constants.TariffTypes.Import, commodityCodeColumn.TariffType);
			}
		}

		#endregion
	}
}
