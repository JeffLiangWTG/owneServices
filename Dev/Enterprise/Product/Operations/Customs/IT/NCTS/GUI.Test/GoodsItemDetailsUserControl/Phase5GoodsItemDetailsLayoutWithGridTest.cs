using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(Phase5GoodsItemDetailsLayoutWithGrid))]
sealed class Phase5GoodsItemDetailsLayoutWithGridTest : LayoutsAbstractTest
{
	public void TestTransportChargesMethodOfPaymentDropEditControlVisibility()
	{
		var header = Factory.New<NctsHeader>();
		var item = header.Bills.AddNew().GoodsItems.AddNew();

		CombineAssertions(() =>
		{
			using (TemporarilySetTransitionPeriod(false))
			{
				AssertEquals("When TP is OFF then TransportChargesMethodOfPaymentDropEdit is hidden.",
					expected: false,
					Layout.IsVisible(EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.TransportChargesMethodOfPaymentDropEdit, item));
			}

			using (TemporarilySetTransitionPeriod(true))
			{
				AssertEquals("When TP is ON then TransportChargesMethodOfPaymentDropEdit is visible.",
					expected: true,
					Layout.IsVisible(EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.TransportChargesMethodOfPaymentDropEdit, item));
			}
		});
	}

	public void TestFormattedWithDescriptionTariffFindBoxBehaviour()
	{
		var tariffFindBoxControlReference = EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CommodityCodeTariffFindBox;
		AssertEquals("Has TariffBoxNomenclatureSelectionModeBehaviour", true, Layout.HasBehaviourByBehaviourType(tariffFindBoxControlReference, typeof(TariffBoxNomenclatureSelectionModeBehaviour)));
	}

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

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new GoodsItemDetailsLayoutBuilder<NctsDepartureCargoDesc>();

	protected override Type ExpectedGridUserControlType => typeof(Phase5DepartureGoodsItemsGridUserControl);

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.ItemNumberTextBox, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.DeclarationGoodsItemNumberTextBox, ControlWidthClass.Auto);
			yield return (GoodsItemDetailsControlBag.Instance.CustomsStatusUserControl, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.DescriptionOfGoodsTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CommodityCodeTariffFindBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.DeclarationTypeDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CountryOfDispatchDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CountryOfDestinationDropEdit, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.UNDangerousGoodsUserControl, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CusC4NumberCodeFindBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CustomsQuantityDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CustomsThirdQuantityDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CustomsFourthQuantityDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.LinePriceCalcDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CustomsValueCalcDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.TaxOrFeeDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.FeesUserControl, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.ConsigneeDocAddressControl, ControlWidthClass.Long);
		}
	}

	PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new Phase5GoodsItemDetailsLayoutWithGrid()).Layout);
	PanelLayout layout;

	IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);
}
