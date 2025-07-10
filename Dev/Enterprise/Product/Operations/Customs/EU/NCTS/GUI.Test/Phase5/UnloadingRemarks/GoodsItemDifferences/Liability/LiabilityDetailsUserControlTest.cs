using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class LiabilityDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(NctsArrivalCargoDesc), userControl.BindingSource.DataSourceType);
		}

		public void TestCountryOfOrigin()
		{
			var countryOfOrigin = userControl.CountryOfOriginDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(countryOfOrigin);
				AssertEquals("Binding", nameof(NctsArrivalCargoDesc.BY_RN_NKCountryOfOrigin), userControl.CountryOfOriginDropEdit.GetBindingMember());
			});
		}

		[RequiresSTA]
		public void TestCommodityCodeTariffFindBox()
		{
			CombineAssertions(() =>
			{
				AssertType<TariffFindBox>(userControl.CommodityCodeTariffFindBox);

				using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
				{
					var goodsItem = GetArrivalCargoDescForTest();

					foreach (var tariffType in new[] { Universal.Constants.TariffTypes.Export, Universal.Constants.TariffTypes.Import })
					{
						goodsItem.TariffTypeForTesting = tariffType;
						goodsItem.Header.ArrivalMovementHeader.BM_ValuationDate = new ZDateTime(2023, 1, 1);

						using (var form = new Phase5ArrivalMovementForm(goodsItem.Header))
						{
							form.Show();

							var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
							mainTabControl.SelectTab(form.UnloadingRemarksTabPage);

							var unloadingRemarksTabUserControl = form.FindSingle<Phase5UnloadingRemarksTabUserControl>();
							var houseConsignmentDifferencesTabPage = unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabPage;
							unloadingRemarksTabUserControl.UnloadingRemarksTabControl.SelectTab(houseConsignmentDifferencesTabPage);

							var houseConsignmentDifferencesTabUserControl = (HouseConsignmentDifferencesTabUserControl)unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabPage.Controls[0];
							var goodsItemTabPage = houseConsignmentDifferencesTabUserControl.GoodsItemsTabPage;
							houseConsignmentDifferencesTabUserControl.HouseConsignmentDifferencesTabControl.SelectTab(goodsItemTabPage);

							var phase5GoodsItemDifferencesTabUserControl = (Phase5GoodsItemDifferencesTabUserControl)houseConsignmentDifferencesTabUserControl.GoodsItemsTabPage.Controls[0];
							var liabilityCalculationTabPage = phase5GoodsItemDifferencesTabUserControl.LiabilityCalculationTabPage;
							phase5GoodsItemDifferencesTabUserControl.GoodsItemDifferencesTabControl.SelectTab(liabilityCalculationTabPage);

							var tariffFindBox = phase5GoodsItemDifferencesTabUserControl.FindSingle<TariffFindBox>(nameof(LiabilityDetailsUserControl.CommodityCodeTariffFindBox));
							AssertEquals("GetTariffType", tariffType, tariffFindBox.GetTariffType?.Invoke());
							AssertEquals("GetEffectiveDate", new ZDateTime(2023, 1, 1), tariffFindBox.GetEffectiveDate());
							AssertEquals("EffectiveTariffCountry", goodsItem.Header.CountryCode, tariffFindBox.EffectiveTariffCountry);
							AssertEquals("EffectiveDataGrouping", goodsItem.Header.DefaultDataGroupingCode, tariffFindBox.EffectiveDataGrouping);
							AssertEquals("Binding", nameof(NctsArrivalCargoDesc.LiabilityFormattedTariff), userControl.CommodityCodeTariffFindBox.GetBindingMember());
						}
					}
				}
			});
		}

		public void TestSupplementaryUnitsCalcDropEdit()
		{
			var supplementaryUnitsCalcDropEdit = userControl.SupplementaryUnitsCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>(supplementaryUnitsCalcDropEdit);
				AssertEquals("BindToAmount", nameof(NctsArrivalCargoDesc.BY_CustomsSecondQuantity), supplementaryUnitsCalcDropEdit.BindToAmount);
				AssertEquals("BindToUnit", nameof(NctsArrivalCargoDesc.BY_CustomsSecondUnitQty), supplementaryUnitsCalcDropEdit.BindToUnit);
			});
		}

		[RequiresSTA]
		public void TestCustomsThirdQuantityDropEdit()
		{
			var customsThirdQuantityDropEdit = userControl.CustomsThirdQuantityDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>(customsThirdQuantityDropEdit);
				AssertEquals("BindToAmount", nameof(NctsArrivalCargoDesc.BY_CustomsThirdQuantity), customsThirdQuantityDropEdit.BindToAmount);
				AssertEquals("BindToUnit", nameof(NctsArrivalCargoDesc.BY_CustomsThirdUnitQty), customsThirdQuantityDropEdit.BindToUnit);
			});
		}

		public void TestCustomsFourthQuantityDropEdit()
		{
			var customsFourthQuantityDropEdit = userControl.CustomsFourthQuantityDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>(customsFourthQuantityDropEdit);
				AssertEquals("BindToAmount", nameof(NctsArrivalCargoDesc.BY_CustomsFourthQuantity), customsFourthQuantityDropEdit.BindToAmount);
				AssertEquals("BindToUnit", nameof(NctsArrivalCargoDesc.BY_CustomsFourthUnitQty), customsFourthQuantityDropEdit.BindToUnit);
			});
		}

		public void TestCustomsValueCalcDropEdit()
		{
			var customsValueCalcDropEdit = userControl.CustomsValueCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>(customsValueCalcDropEdit);
				AssertEquals("BindToAmount", nameof(NctsArrivalCargoDesc.BY_MonetaryValue), customsValueCalcDropEdit.BindToAmount);
				AssertEquals("BindToUnit", nameof(NctsArrivalCargoDesc.BY_RX_NKCurrency), customsValueCalcDropEdit.BindToUnit);
			});
		}

		public void TestAdditionalSupplementaryCodesUserControl()
		{
			AssertType<AdditionalSupplementaryCodesUserControl>(userControl.AdditionalSupplementaryCodesUserControl);
		}

		public void TestFeesUserControl()
		{
			var feesGridUserControl = userControl.FeesUserControl;
			CombineAssertions(() =>
			{
				AssertType<FeesGridUserControl>(feesGridUserControl);
				AssertEquals("Binding", ".", feesGridUserControl.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new LiabilityDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		LiabilityDetailsUserControl userControl;

		NctsArrivalCargoDescForTest GetArrivalCargoDescForTest()
		{
			var arrivalCargoDesc = Factory.New<NctsArrivalCargoDescForTest>();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, header);
			header.ArrivalMovementHeader.BM_CustomsStatus = "UAP";
			var bill = header.Bills.AddNew();
			arrivalCargoDesc.BY_ParentID = bill.PK;
			arrivalCargoDesc.BY_ParentTableCode = bill.TablePrefix;
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			return arrivalCargoDesc;
		}

		class NctsArrivalCargoDescForTest : NctsArrivalCargoDesc
		{
			public NctsArrivalCargoDescForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString TariffTypeCore => TariffTypeForTesting;
			internal ZString TariffTypeForTesting { get; set; }
		}
	}
}
