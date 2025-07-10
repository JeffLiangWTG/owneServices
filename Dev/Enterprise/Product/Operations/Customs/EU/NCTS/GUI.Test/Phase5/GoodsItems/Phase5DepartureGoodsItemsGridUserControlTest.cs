using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5DepartureGoodsItemsGridUserControlTest : TestCaseWithFactory
	{
		public void TestCommodityCodeTariffFindBox()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_ValuationDate = new ZDateTime(2022, 12, 15);
				var nctsBill = nctsHeader.Bills.AddNew();
				var goodsItem = Factory.New<NctsDepartureCargoDescForTesting>();
				goodsItem.BY_ParentTableCode = nctsBill.TablePrefix;
				goodsItem.BY_ParentID = nctsBill.PK;
				nctsBill.GoodsItems.Add(goodsItem);
				foreach (var tariffType in new[] { Universal.Constants.TariffTypes.Export, Universal.Constants.TariffTypes.Import })
				{
					goodsItem.TariffTypeForTesting = tariffType;

					using (var form = new Phase5DepartureMovementForm(nctsHeader))
					{
						form.Show();

						var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
						mainTabControl.SelectTab(form.HouseConsignmentsTabPage);

						var houseConsignmentsTabUserControl = form.FindSingle<HouseConsignmentsTabUserControl>();
						var goodsItemsTabPage = houseConsignmentsTabUserControl.GoodsItemsTabPage;
						houseConsignmentsTabUserControl.HouseConsignmentTabControl.SelectTab(goodsItemsTabPage);

						var phase5GoodsItemsTabUserControl = (Phase5GoodsItemsTabUserControl)houseConsignmentsTabUserControl.GoodsItemsTabPage.Controls[0];
						var goodsItemDetailsTabPage = phase5GoodsItemsTabUserControl.GoodsItemDetailsTabPage;
						phase5GoodsItemsTabUserControl.GoodsItemTabControl.SelectTab(goodsItemDetailsTabPage);

						var grid = phase5GoodsItemsTabUserControl.FindSingle<ZGrid>("GoodsItemsGrid");
						var columnStyle = (TariffColumnStyleInfo)grid.GetColumnStyle(nameof(NctsCommonCargoDesc.BY_FormattedHarmonisedTariff));
						AssertEquals($"{tariffType} GetTariffType()", tariffType, columnStyle.GetTariffType());
						AssertEquals($"{tariffType} GetEffectiveDate()", nctsHeader.MovementHeader.BM_ValuationDate, columnStyle.GetEffectiveDate());
						AssertEquals($"{tariffType} GetDataGrouping()", nctsHeader.DefaultDataGroupingCode, columnStyle.GetDataGrouping());
						AssertContainsExactElementsInAnyOrder($"{tariffType} GetTariffNomenclatureSelectionModes()", goodsItem.GetTariffNomenclatureSelectionModes(), columnStyle.GetSelectNomenclatureModes());
					}
				}
			});
		}

		[RequiresSTA]
		public void TestGridWhenFirstRowDeleted()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_ValuationDate = new ZDateTime(2022, 12, 15);
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem2 = Factory.New<NctsDepartureCargoDescForTesting>();
			goodsItem2.BY_ParentTableCode = nctsBill.TablePrefix;
			goodsItem2.BY_ParentID = nctsBill.PK;
			nctsBill.GoodsItems.Add(goodsItem2);

			using (var form = new Phase5DepartureMovementForm(nctsHeader))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectTab(form.HouseConsignmentsTabPage);

				var houseConsignmentsTabUserControl = form.FindSingle<HouseConsignmentsTabUserControl>();
				var goodsItemsTabPage = houseConsignmentsTabUserControl.GoodsItemsTabPage;
				houseConsignmentsTabUserControl.HouseConsignmentTabControl.SelectTab(goodsItemsTabPage);

				var phase5GoodsItemsTabUserControl = (Phase5GoodsItemsTabUserControl)houseConsignmentsTabUserControl.GoodsItemsTabPage.Controls[0];
				var goodsItemDetailsTabPage = phase5GoodsItemsTabUserControl.GoodsItemDetailsTabPage;
				phase5GoodsItemsTabUserControl.GoodsItemTabControl.SelectTab(goodsItemDetailsTabPage);

				var grid = phase5GoodsItemsTabUserControl.FindSingle<ZGrid>("GoodsItemsGrid");

				grid.Select(0);
				grid.OnDeleteKeyPressed();

				var allColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
				var indexOfTariff = Array.IndexOf(allColumns, nameof(NctsCommonCargoDesc.BY_FormattedHarmonisedTariff));

				grid.CurrentCell = new DataGridCell(0, indexOfTariff);
				grid.CurrentCell = new DataGridCell(1, indexOfTariff);

				AssertNoExceptionThrown(() => Application.DoEvents());
			}
		}

		public void TestColumns_InPhase5TransitionPeriod()
		{
			var gridColumnLayoutMock = new Mock<IGridColumnLayout>();
			gridColumnLayoutMock.Setup(m => m.Columns).Returns([new ZTextBoxColumnStyleInfo(NctsDepartureCargoDesc.Schema.BY_DeclarationGoodsItemNumber, 60)]);

			var gridColumnLayoutProviderMock = new Mock<IGridColumnLayoutProvider>();
			gridColumnLayoutProviderMock.Setup(m => m.Layout).Returns(gridColumnLayoutMock.Object);

			var nctsPhase5LayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new NctsPhase5LayoutProviderForTest(gridColumnLayoutProviderMock.Object)) }
			};

			var header = Factory.New<NctsHeader>();

			using (ObjectFactory.Substitute("NctsPhase5LayoutProviders", nctsPhase5LayoutProviders))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			using (var userControl = new Phase5DepartureGoodsItemsGridUserControl())
			{
				userControl.SetDataBinding(header, "");
				var grid = userControl.GoodsItemsGrid;
				var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
				var layoutProvider = gridColumnLayoutProviderMock.Object;
				var expectedColumnNames = layoutProvider.Layout.Columns.Select(x => x.ColumnName);
				AssertContainsExactElementsInExactOrder(expectedColumnNames, columnNames);
			}
		}

		sealed class NctsPhase5LayoutProviderForTest(IGridColumnLayoutProvider gridColumnLayoutProvider) : NctsPhase5LayoutProvider, INctsPhase5TransitionPeriodLayoutProvider
		{
			IGridColumnLayoutProvider INctsPhase5TransitionPeriodLayoutProvider.GetDepartureGoodsItemsGridColumnLayout() => gridColumnLayoutProvider;
		}
	}
}
