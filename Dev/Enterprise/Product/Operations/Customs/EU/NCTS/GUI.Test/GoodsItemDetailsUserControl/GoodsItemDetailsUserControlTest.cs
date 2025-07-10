using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class GoodsItemDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), control.BindingSource.DataSourceType);
		}

		public void TestItemNumberTextBox()
		{
			AssertType<ZTextBox>(control.ItemNumberTextBox);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			AssertType<ZTextBox>(control.DeclarationGoodsItemNumberTextBox);
		}

		public void TestDescriptionOfGoodsTextBox()
		{
			AssertType<LongTextControl>(control.DescriptionOfGoodsTextBox);
		}

		public void TestGrossWeightCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.GrossWeightCalcDropEdit);
		}

		public void TestNetWeightCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.NetWeightCalcDropEdit);
		}

		public void TestCommodityCodeTariffFindBox()
		{
			CombineAssertions(() =>
			{
				AssertType<Universal.GUI.TariffFindBox>(control.CommodityCodeTariffFindBox);

				var goodsItem = SetupPhase5DepartureHeader();

				foreach (var tariffType in new[] { Universal.Constants.TariffTypes.Export, Universal.Constants.TariffTypes.Import })
				{
					goodsItem.TariffTypeForTesting = tariffType;
					goodsItem.Header.MovementHeader.BM_ValuationDate = new ZDateTime(2023, 1, 1);

					using (var form = new Phase5DepartureMovementForm(goodsItem.Header))
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

						var tariffFindBox = phase5GoodsItemsTabUserControl.FindSingle<Universal.GUI.TariffFindBox>(nameof(GoodsItemDetailsUserControl.CommodityCodeTariffFindBox));
						AssertEquals("GetTariffType", tariffType, tariffFindBox.GetTariffType?.Invoke());
						AssertEquals("GetEffectiveDate", new ZDateTime(2023, 1, 1), tariffFindBox.GetEffectiveDate());
						AssertEquals("EffectiveTariffCountry", goodsItem.Header.CountryCode, tariffFindBox.EffectiveTariffCountry);
						AssertEquals("EffectiveDataGrouping", goodsItem.Header.DefaultDataGroupingCode, tariffFindBox.EffectiveDataGrouping);
						AssertEquals("BindToForDescription", "UniversalTariffDescription", tariffFindBox.BindToForDescription);
					}
				}
			});
		}

		public void TestDeclarationTypeDropEdit()
		{
			AssertType<ZDropEdit>(control.DeclarationTypeDropEdit);
		}

		public void TestCountryOfDispatchDropEdit()
		{
			AssertType<ZDropEdit>(control.CountryOfDispatchDropEdit);
		}

		public void TestCountryOfOriginDropEdit()
		{
			AssertType<ZDropEdit>(control.CountryOfOriginDropEdit);
		}

		[RequiresSTA]
		public void TestCountryOfDestinationDropEdit()
		{
			AssertType<ZDropEdit>(control.CountryOfDestinationDropEdit);
		}

		[RequiresSTA]
		public void TestConsigneeDocAddressControl()
		{
			var consigneeDocAddressControl = control.ConsigneeDocAddressControl;
			CombineAssertions(() =>
			{
				AssertType<ZDocAddressControl>(consigneeDocAddressControl);
				AssertEquals("DisplayMode", ZDocAddressControlDisplayMode.CompactWithContactTab, consigneeDocAddressControl.DisplayMode);
				AssertEquals("BindToOrganisations", nameof(NctsDepartureCargoDesc.Lookups) + "." + nameof(NctsDepartureCargoDescPhase5Lookups.ConsigneeList), consigneeDocAddressControl.BindToOrganisations);
			});
		}

		public void TestConsigneeDocAddressControl_Caption()
		{
			var nctsDepartureCargoDesc = SetupPhase5DepartureHeader();
			control.SetDataBinding(nctsDepartureCargoDesc, string.Empty);
			AssertEquals("Consignee", control.ConsigneeDocAddressControl.CaptionResourceString.Caption);
		}

		public void TestCommercialReferenceNumberTextBox()
		{
			AssertType<ZTextBox>(control.CommercialReferenceNumberTextBox);
		}

		public void TestTransportChargesMethodOfPaymentDropEdit()
		{
			AssertType<ZDropEdit>(control.TransportChargesMethodOfPaymentDropEdit);
		}

		public void TestCusC4NumberCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.CusC4NumberCodeFindBox);
		}

		public void TestUNDangerousGoodsUserControl()
		{
			AssertType<UNDangerousGoodsUserControl>(control.UNDangerousGoodsUserControl);
		}

		public void TestSupplementaryUnitsCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.SupplementaryUnitsCalcDropEdit);
		}

		public void TestCustomsQuantityDropEdit()
		{
			var customsQuantityDropEdit = control.CustomsQuantityDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>(customsQuantityDropEdit);
				AssertEquals("BindToAmount", nameof(NctsDepartureCargoDesc.CustomsFirstQuantityInKilograms), customsQuantityDropEdit.BindToAmount);
				AssertEquals("BindToUnit", nameof(NctsDepartureCargoDesc.CustomsFirstUnitQtyKilograms), customsQuantityDropEdit.BindToUnit);
			});
		}

		public void TestCustomsThirdQuantityDropEdit()
		{
			var customsThirdQuantityDropEdit = control.CustomsThirdQuantityDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>(customsThirdQuantityDropEdit);
				AssertEquals("BindToAmount", nameof(NctsDepartureCargoDesc.BY_CustomsThirdQuantity), customsThirdQuantityDropEdit.BindToAmount);
				AssertEquals("BindToUnit", nameof(NctsDepartureCargoDesc.BY_CustomsThirdUnitQty), customsThirdQuantityDropEdit.BindToUnit);
			});
		}

		public void TestCustomsFourthQuantityDropEdit()
		{
			var customsFourthQuantityDropEdit = control.CustomsFourthQuantityDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>(customsFourthQuantityDropEdit);
				AssertEquals("BindToAmount", nameof(NctsDepartureCargoDesc.BY_CustomsFourthQuantity), customsFourthQuantityDropEdit.BindToAmount);
				AssertEquals("BindToUnit", nameof(NctsDepartureCargoDesc.BY_CustomsFourthUnitQty), customsFourthQuantityDropEdit.BindToUnit);
			});
		}

		public void TestCustomsValueCalcDropEdit()
		{
			var customsValueCalcDropEdit = control.CustomsValueCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("BindToAmount", nameof(NctsDepartureCargoDesc.BY_MonetaryValue), customsValueCalcDropEdit.BindToAmount);
				AssertEquals("BindToUnit", "Header.Company.CustomsCurrency.Code", customsValueCalcDropEdit.BindToUnit);
				AssertEquals("BindToList", "Lookups.Currencies", customsValueCalcDropEdit.BindToList);
			});
		}

		[RequiresSTA]
		public void TestLinePriceCalcDropEdit()
		{
			var priceCalcDropEdit = control.LinePriceCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("BindToAmount", nameof(NctsDepartureCargoDesc.BY_LinePrice), priceCalcDropEdit.BindToAmount);
				AssertEquals("BindToUnit", nameof(NctsDepartureCargoDesc.BY_RX_NKLinePriceCurrency), priceCalcDropEdit.BindToUnit);
			});
		}

		public void TestTaxOrFeeDropEdit()
		{
			var taxOrFeeDropEdit = control.TaxOrFeeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(taxOrFeeDropEdit);
				AssertEquals("BindTo", nameof(NctsDepartureCargoDesc.BY_ZZF_NKTaxType), taxOrFeeDropEdit.BindTo);
			});
		}

		public void TestAdditionalSupplementaryCodesUserControl()
		{
			AssertType<AdditionalSupplementaryCodesUserControl>(control.AdditionalSupplementaryCodesUserControl);
		}

		public void TestFeesControl()
		{
			AssertType<FeesGridUserControl>(control.FeesUserControl);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new GoodsItemDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		GoodsItemDetailsUserControl control;

		NctsDepartureCargoDescForTesting SetupPhase5DepartureHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = Factory.New<NctsDepartureCargoDescForTesting>();
			goodsItem.BY_ParentTableCode = nctsBill.TablePrefix;
			goodsItem.BY_ParentID = nctsBill.PK;
			nctsBill.GoodsItems.Add(goodsItem);
			return goodsItem;
		}
	}

	sealed class NctsDepartureCargoDescForTesting : NctsDepartureCargoDesc
	{
		public NctsDepartureCargoDescForTesting(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
		{
		}
		protected override ZString TariffTypeCore => TariffTypeForTesting;
		internal ZString TariffTypeForTesting { get; set; }
	}
}
