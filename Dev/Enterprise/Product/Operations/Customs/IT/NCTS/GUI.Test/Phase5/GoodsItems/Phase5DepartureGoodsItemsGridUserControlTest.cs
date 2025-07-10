using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class Phase5DepartureGoodsItemsGridUserControlTest : TestCaseWithFactory
{
	public void TestCommodityCodeTariffFindBox()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.BM_ValuationDate = new ZDateTime(2022, 12, 15);

		var nctsBill = nctsHeader.Bills.AddNew();
		var goodsItem = Factory.New<NctsDepartureCargoDescForTesting>();
		goodsItem.BY_ParentTableCode = nctsBill.TablePrefix;
		goodsItem.BY_ParentID = nctsBill.PK;
		nctsBill.GoodsItems.Add(goodsItem);

		CombineAssertions(() =>
		{
			foreach (var tariffType in new[] { Universal.Constants.TariffTypes.Export, Universal.Constants.TariffTypes.Import })
			{
				goodsItem.TariffTypeForTesting = tariffType;

				using (var control = new Phase5DepartureGoodsItemsGridUserControl())
				{
					control.SetDataBinding(goodsItem, "");

					var grid = control.FindSingle<ZGrid>("GoodsItemsGrid");
					var columnStyle = (TariffColumnStyleInfo)grid.GetColumnStyle(nameof(NctsCommonCargoDesc.BY_FormattedHarmonisedTariff));
					AssertEquals($"{tariffType} GetTariffType()", tariffType, columnStyle.GetTariffType());
					AssertEquals($"{tariffType} GetEffectiveDate()", nctsHeader.MovementHeader.BM_ValuationDate, columnStyle.GetEffectiveDate());
					AssertEquals($"{tariffType} GetDataGrouping()", nctsHeader.DefaultDataGroupingCode, columnStyle.GetDataGrouping());
					AssertContainsExactElementsInAnyOrder($"{tariffType} GetTariffNomenclatureSelectionModes()", goodsItem.GetTariffNomenclatureSelectionModes(), columnStyle.GetSelectNomenclatureModes());
				}
			}
		});
	}

	public void TestColumns_InPhase5TransitionPeriod()
	{
		var header = Factory.New<NctsHeader>();
		var item = header.Bills.AddNew().GoodsItems.AddNew();

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
		using (var userControl = new Phase5DepartureGoodsItemsGridUserControl())
		{
			userControl.SetDataBinding(item, "");
			var grid = userControl.GoodsItemsGrid;
			var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
			IGridColumnLayoutProvider layoutProvider = new Phase5TransitionPeriodGoodsItemDetailsGridColumnsLayout();
			var expectedColumnNames = layoutProvider.Layout.Columns.Select(x => x.ColumnName);
			AssertContainsExactElementsInExactOrder(expectedColumnNames, columnNames);
		}
	}
}

sealed class NctsDepartureCargoDescForTesting : Business.NctsDepartureCargoDesc
{
	public NctsDepartureCargoDescForTesting(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
	{
	}
	protected override ZString TariffTypeCore => TariffTypeForTesting;
	public ZString TariffTypeForTesting { get; set; }
}
