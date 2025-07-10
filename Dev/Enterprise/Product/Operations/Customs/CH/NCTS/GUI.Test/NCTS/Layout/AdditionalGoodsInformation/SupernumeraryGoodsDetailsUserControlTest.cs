using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class SupernumeraryGoodsDetailsUserControlTest : TestCaseWithFactory
{
	public void TestControlTypes() => CombineAssertions(() =>
	{
		using (var control = new SupernumeraryGoodsDetailsUserControl())
		{
			AssertType<ZCalcEdit>("LineNoCalcEdit", control.LineNoCalcEdit);
			AssertType<ZTextBox>("DescriptionTextBox", control.DescriptionTextBox);
			AssertType<ZCalcDropEdit>("GrossMassDropEdit", control.GrossMassDropEdit);
			AssertType<TariffFindBox>("TariffFindBox", control.TariffFindBox);
			AssertType<ZCalcDropEdit>("PackagesCalcDropEdit", control.PackagesCalcDropEdit);
		}
	});

	public void TestTariffFindBox()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeader.ArrivalMovementHeader.BM_ValuationDate = new ZDateTime(2023, 2, 10);

		using (var control = new SupernumeraryGoodsDetailsUserControl())
		{
			control.SetDataBinding(nctsHeader, "ArrivalMovementHeader.SupernumeraryGoods");

			var tariffFindBox = control.TariffFindBox;
			AssertEquals("TariffType", Universal.Constants.TariffTypes.HarmonizedSystem, tariffFindBox.TariffType);
			AssertEquals("GetCountryCode", Core.Constants.CountryCodes.Switzerland, tariffFindBox.GetCountryCode());
			AssertEquals("GetDataGrouping", Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, tariffFindBox.GetDataGrouping());
			AssertEquals("GetEffectiveDate", new ZDateTime(2023, 2, 10), tariffFindBox.GetEffectiveDate());
		}
	}

	public static void TestPackagesCalcDropEdit()
	{
		using (var control = new SupernumeraryGoodsDetailsUserControl())
		{
			AssertEquals("ShowDescriptionBox", true, control.PackagesCalcDropEdit.ShowDescriptionBox);
		}
	}
}
