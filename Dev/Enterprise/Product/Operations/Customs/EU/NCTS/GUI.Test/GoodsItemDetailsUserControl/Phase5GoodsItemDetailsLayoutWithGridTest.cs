using System;
using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemDetailsLayoutWithGrid))]
	sealed class Phase5GoodsItemDetailsLayoutWithGridTest : LayoutsAbstractTest
	{
		public void TestFormattedWithDescriptionTariffFindBoxBehaviour()
		{
			var tariffFindBoxControlReference = GoodsItemDetailsControlBag.Instance.CommodityCodeTariffFindBox;
			AssertEquals("Has TariffBoxNomenclatureSelectionModeBehaviour", true, Layout.HasBehaviourByBehaviourType(tariffFindBoxControlReference, typeof(TariffBoxNomenclatureSelectionModeBehaviour)));
		}

		public void TestDocAddressControlHidesLabelCaption()
		{
			using (var goodsItemDetailsControl = new GoodsItemDetailsUserControl())
			{
				var docAddressControl = goodsItemDetailsControl.ConsigneeDocAddressControl;
				AssertEquals(false, new LabelCaptionRenderProvider().GetLabelCaptionVisible(docAddressControl));
			}
		}

		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new GoodsItemDetailsLayoutBuilder<Business.NctsDepartureCargoDesc>();

		protected override Type ExpectedGridUserControlType => typeof(Phase5DepartureGoodsItemsGridUserControl);

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (GoodsItemDetailsControlBag.Instance.ItemNumberTextBox, ControlWidthClass.Auto);
				yield return (GoodsItemDetailsControlBag.Instance.DeclarationGoodsItemNumberTextBox, ControlWidthClass.Auto);
				yield return (GoodsItemDetailsControlBag.Instance.DescriptionOfGoodsTextBox, ControlWidthClass.Long);
				yield return (GoodsItemDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (GoodsItemDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (GoodsItemDetailsControlBag.Instance.CommodityCodeTariffFindBox, ControlWidthClass.Long);
				yield return (GoodsItemDetailsControlBag.Instance.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Auto);
				yield return (GoodsItemDetailsControlBag.Instance.DeclarationTypeDropEdit, ControlWidthClass.Long);
				yield return (GoodsItemDetailsControlBag.Instance.CountryOfDispatchDropEdit, ControlWidthClass.Long);
				yield return (GoodsItemDetailsControlBag.Instance.CountryOfDestinationDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (GoodsItemDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
				yield return (GoodsItemDetailsControlBag.Instance.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (GoodsItemDetailsControlBag.Instance.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Long);
				yield return (GoodsItemDetailsControlBag.Instance.UNDangerousGoodsUserControl, ControlWidthClass.Long);
				yield return (GoodsItemDetailsControlBag.Instance.CusC4NumberCodeFindBox, ControlWidthClass.Long);
				yield return (GoodsItemDetailsControlBag.Instance.CustomsQuantityDropEdit, ControlWidthClass.Auto);
				yield return (GoodsItemDetailsControlBag.Instance.CustomsThirdQuantityDropEdit, ControlWidthClass.Auto);
				yield return (GoodsItemDetailsControlBag.Instance.CustomsFourthQuantityDropEdit, ControlWidthClass.Auto);
				yield return (GoodsItemDetailsControlBag.Instance.LinePriceCalcDropEdit, ControlWidthClass.Auto);
				yield return (GoodsItemDetailsControlBag.Instance.CustomsValueCalcDropEdit, ControlWidthClass.Auto);
				yield return (GoodsItemDetailsControlBag.Instance.TaxOrFeeDropEdit, ControlWidthClass.Long);
				yield return (GoodsItemDetailsControlBag.Instance.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (GoodsItemDetailsControlBag.Instance.FeesUserControl, ControlWidthClass.Auto);
				yield return (GoodsItemDetailsControlBag.Instance.ConsigneeDocAddressControl, ControlWidthClass.Long);
			}
		}

		PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new Phase5GoodsItemDetailsLayoutWithGrid()).Layout);
		PanelLayout layout;
	}
}
