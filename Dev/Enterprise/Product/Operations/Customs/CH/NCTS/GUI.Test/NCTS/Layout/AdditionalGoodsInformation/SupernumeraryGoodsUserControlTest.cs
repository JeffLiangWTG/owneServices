using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.GUI.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class SupernumeraryGoodsUserControlTest : TestCaseWithFactory
{
	public void TestGridColumns() => CombineAssertions(() =>
	{
		using (var control = new SupernumeraryGoodsUserControl())
		{
			var gridColumnStyles = control.SupernumeraryGoodsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(gridColumnStyles, SupernumeraryGoods.Schema.CSI_LineNo, 0);
			UserControlTestHelper.AssertColumnStyles<ZMultiLineTextBoxColumnInfo>(gridColumnStyles, SupernumeraryGoods.Schema.CSI_Description, 1);
			UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(gridColumnStyles, SupernumeraryGoods.Schema.CSI_Quantity, 2, groupName: "Gross mass");
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, SupernumeraryGoods.Schema.CSI_UnitOfQuantity, 3, groupName: "Gross mass");
			UserControlTestHelper.AssertColumnStyles<TariffColumnStyleInfo>(gridColumnStyles, SupernumeraryGoods.Schema.CSI_Tariff, 4);
			UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(gridColumnStyles, SupernumeraryGoods.Schema.CSI_PackQty, 5);
			UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(gridColumnStyles, SupernumeraryGoods.Schema.CSI_PackType, 6);
		}
	});

	public void TestTariffColumnStyle() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeader.ArrivalMovementHeader.BM_ValuationDate = new ZDateTime(2023, 2, 10);

		using (var control = new SupernumeraryGoodsUserControl())
		{
			control.SetDataBinding(nctsHeader, "ArrivalMovementHeader.SupernumeraryGoods");

			var tariffColumnStyleInfo = control.SupernumeraryGoodsGrid.ColumnStyles.OfType<TariffColumnStyleInfo>().Single();
			AssertEquals("TariffType", Universal.Constants.TariffTypes.HarmonizedSystem, tariffColumnStyleInfo.TariffType);
			AssertEquals("GetCountryCode", Core.Constants.CountryCodes.Switzerland, tariffColumnStyleInfo.GetCountryCode());
			AssertEquals("GetDataGrouping", Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, tariffColumnStyleInfo.GetDataGrouping());
			AssertEquals("GetEffectiveDate", new ZDateTime(2023, 2, 10), tariffColumnStyleInfo.GetEffectiveDate());
		}
	});
}
