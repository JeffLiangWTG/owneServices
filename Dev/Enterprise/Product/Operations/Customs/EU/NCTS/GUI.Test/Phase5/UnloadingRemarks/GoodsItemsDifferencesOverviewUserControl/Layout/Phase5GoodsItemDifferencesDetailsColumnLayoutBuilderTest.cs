using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemDifferencesDetailsColumnLayoutBuilder<NctsArrivalCargoDesc>))]
	sealed class Phase5GoodsItemDifferencesDetailsColumnLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<Phase5GoodsItemDifferencesDetailsColumnLayoutBuilder<NctsArrivalCargoDesc>, NctsArrivalCargoDesc, Phase5GoodsItemDifferencesDetailsColumnControlBag>
	{
		protected override Phase5GoodsItemDifferencesDetailsColumnLayoutBuilder<NctsArrivalCargoDesc> GetColumnLayoutBuilderForTesting() => new Phase5GoodsItemDifferencesDetailsColumnLayoutBuilder<NctsArrivalCargoDesc>();

		protected override int ExpectedMaxColumns => 2;

		public void TestSetDefaultVisibilities() => CombineAssertions(() =>
		{
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredCommodityCodeCodeFindBox, true, true, true, true);
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredValueLabel, true, true, true, true);
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredCusCodeCodeFindBox, true, true, true, true);
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredDescriptionTextBox, true, true, true, true);
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredGrossWeightDropEdit, true, true, true, true);
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredNetWeightDropEdit, true, true, true, true);
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedCusCodeCodeFindBox, false, false, true, false);
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedDescriptionTextBox, false, false, true, false);
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedGrossWeightDropEdit, false, false, true, false);
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedNetWeightDropEdit, false, false, true, false);
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedCommodityCodeCodeFindBox, false, false, true, false);
			AssertVisibility(Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedValueLabel, false, false, true, false);
		});

		void AssertVisibility(ControlReference control, bool decVisible, bool misVisible, bool difVisible, bool newVisible)
		{
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			AssertEquals(ZString.Format("{0} Visible, if Unloaded State = DEC", control.Name), decVisible, Layout.IsVisible(control, goodsItem));

			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals(ZString.Format("{0} Visible, if Unloaded State = MIS", control.Name), misVisible, Layout.IsVisible(control, goodsItem));

			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals(ZString.Format("{0} Visible, if Unloaded State = DIF", control.Name), difVisible, Layout.IsVisible(control, goodsItem));

			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals(ZString.Format("{0} Visible, if Unloaded State = NEW", control.Name), newVisible, Layout.IsVisible(control, goodsItem));
		}

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new Phase5GoodsItemDifferencesDetailsColumnLayout()).Layout);
		PanelLayout layout;

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		}
		NctsArrivalCargoDesc goodsItem;
	}
}
