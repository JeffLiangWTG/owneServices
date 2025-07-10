using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemDetailsLayoutWithGrid))]
	sealed class Phase5GoodsItemDetailsLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(EU.NCTS.GUI.Phase5DepartureGoodsItemsGridUserControl);

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				var euBag = EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance;
				yield return (euBag.ItemNumberTextBox, ControlWidthClass.Auto);
				yield return (euBag.DeclarationGoodsItemNumberTextBox, ControlWidthClass.Auto);
				yield return (euBag.DescriptionOfGoodsTextBox, ControlWidthClass.Long);
				yield return (euBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (euBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (euBag.CommodityCodeTariffFindBox, ControlWidthClass.Long);
				yield return (euBag.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Auto);
				yield return (euBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
				yield return (euBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
				yield return (euBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				var euBag = EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance;
				var esBag = GoodsItemDetailsControlBag.Instance;
				yield return (esBag.IsVehiclesCheckBox, ControlWidthClass.Auto);
				yield return (euBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
				yield return (euBag.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (euBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Long);
				yield return (euBag.UNDangerousGoodsUserControl, ControlWidthClass.Long);
				yield return (euBag.CusC4NumberCodeFindBox, ControlWidthClass.Long);
				yield return (euBag.CustomsQuantityDropEdit, ControlWidthClass.Auto);
				yield return (euBag.CustomsThirdQuantityDropEdit, ControlWidthClass.Auto);
				yield return (euBag.CustomsFourthQuantityDropEdit, ControlWidthClass.Auto);
				yield return (euBag.LinePriceCalcDropEdit, ControlWidthClass.Auto);
				yield return (euBag.CustomsValueCalcDropEdit, ControlWidthClass.Auto);
				yield return (euBag.TaxOrFeeDropEdit, ControlWidthClass.Long);
				yield return (euBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
				yield return (esBag.ExciseCodeDropEdit, ControlWidthClass.Auto);
				yield return (esBag.PVPValueCalcDropEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				var euBag = EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance;
				yield return (euBag.FeesUserControl, ControlWidthClass.Auto);
				yield return (euBag.ConsigneeDocAddressControl, ControlWidthClass.Long);
			}
		}

		[RequiresSTA]
		public void TestPVPCalcFindBox()
		{
			#region Setup

			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");

			var countryCode = Core.Constants.CountryCodes.Spain;
			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "0A0", rateType.PK);
			helper.CreateRate(tariffExcise, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "41.5*[MIL]");

			var tariffExciseWithPvP = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A7", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
			var rateCodePVP = helper.LoadOrCreateNewCusRateCode(Factory, "0A7", rateType.PK);
			helper.CreateRate(tariffExciseWithPvP, rateCodePVP.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.158*PVP");

			helper.CreateTariffRelationship(tariffExcise.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariffExciseWithPvP.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			Factory.Save();

			#endregion

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = nctsBill.GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = tariff.ZZ1_TariffCode;

			nctsHeader.MovementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			var customsOfficeForDeparture = nctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew();
			customsOfficeForDeparture.CY_Code = EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			customsOfficeForDeparture.CY_Data = "ES0035";

			CombineAssertions(() =>
			{
				using (var goodsItemDetailsControl = new GoodsItemDetailsUserControl())
				{
					var pvpControl = GoodsItemDetailsControlBag.Instance.PVPValueCalcDropEdit;
					var layout = new Phase5GoodsItemDetailsLayoutWithGrid().Layout;
					AssertEquals("PVP is not Visible, there is no ExciseCode", false, layout.IsVisible(pvpControl, goodsItem));

					goodsItem.ExciseCode = "0A0";
					AssertEquals("PVP is not Visible, the excise isn't PVP", false, layout.IsVisible(pvpControl, goodsItem));

					goodsItem.ExciseCode = "0A7";
					AssertEquals("PVP is Visible, 0A7 is PVP", true, layout.IsVisible(pvpControl, goodsItem));

					goodsItem.ExciseCode = "0A6";
					AssertEquals("PVP is not Visible, the code is not valid", false, layout.IsVisible(pvpControl, goodsItem));
				}
			});
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.GoodsItemDetailsLayoutBuilder<Business.NctsDepartureCargoDesc>();
	}
}
