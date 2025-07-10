using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5GoodsItemDifferencesGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsArrivalCargoDesc), userControl.BindingSource.DataSourceType);
		}

		public void TestGoodsItemDifferencesGrid()
		{
			var goodsItemDifferencesGrid = userControl.GoodsItemDifferencesGrid;
			CombineAssertions(() =>
			{
				AssertType<ZGrid>("Type", goodsItemDifferencesGrid);
				AssertEquals("BindTo", ".", goodsItemDifferencesGrid.BindTo);
			});
		}

		public void TestColumnFormattedTariff()
		{
			var header = Factory.New<NctsHeaderForTest>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = header.ArrivalMovementHeader;
			movementHeader.BM_CustomsStatus = header.UnloadingAllowedOrCompleteStatusListExposed[0];
			movementHeader.BM_ValuationDate = new ZDateTime(2023, 1, 1);
			var bill = header.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();

			using (var form = new Phase5ArrivalMovementForm(header))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var unloadingRemarksTabPage = form.UnloadingRemarksTabPage;
				mainTabControl.SelectTab(unloadingRemarksTabPage);

				var unloadingRemarksTabUserControl = form.UnloadingRemarksTabUserControl;
				var houseConsignmentDifferencesTabPage = unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabPage;
				unloadingRemarksTabUserControl.UnloadingRemarksTabControl.SelectTab(houseConsignmentDifferencesTabPage);

				var houseConsignmentDifferencesTabUserControl = unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabUserControl;
				var goodsItemsTabPage = houseConsignmentDifferencesTabUserControl.GoodsItemsTabPage;
				houseConsignmentDifferencesTabUserControl.HouseConsignmentDifferencesTabControl.SelectTab(goodsItemsTabPage);

				var phase5GoodsItemDifferencesTabUserControl = houseConsignmentDifferencesTabUserControl.Phase5GoodsItemDifferencesTabUserControl;
				var itemDetailsTabPage = phase5GoodsItemDifferencesTabUserControl.ItemDetailsTabPage;
				phase5GoodsItemDifferencesTabUserControl.GoodsItemDifferencesTabControl.SelectTab(itemDetailsTabPage);

				var grid = phase5GoodsItemDifferencesTabUserControl.GoodsItemDifferencesGridUserControl.GoodsItemDifferencesGrid;
				var tariffColumnStyleInfo = (TariffColumnStyleInfo)grid.GetColumnStyle(nameof(NctsCommonCargoDesc.BY_FormattedHarmonisedTariff));
				CombineAssertions(() =>
				{
					AssertNull("CountryCode", tariffColumnStyleInfo.GetCountryCode?.Invoke());
					AssertEquals("TariffType", null, tariffColumnStyleInfo.TariffType);
					AssertEquals("SelectNomenclatureModes", null, tariffColumnStyleInfo.SelectNomenclatureModes);
					AssertEquals("SelectNomenclatureModes", false, tariffColumnStyleInfo.ShowDescriptionFilterOnNonNomenclatureTariffModule);
					AssertEquals("GetTariffType()", Constants.TariffTypes.Export, tariffColumnStyleInfo.GetTariffType());
					AssertEquals("GetEffectiveDate()", header.ArrivalMovementHeader.BM_ValuationDate, tariffColumnStyleInfo.GetEffectiveDate());
					AssertEquals("GetDataGrouping()", header.DefaultDataGroupingCode, tariffColumnStyleInfo.GetDataGrouping());
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemDifferencesGridUserControl();
		}
		Phase5GoodsItemDifferencesGridUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
