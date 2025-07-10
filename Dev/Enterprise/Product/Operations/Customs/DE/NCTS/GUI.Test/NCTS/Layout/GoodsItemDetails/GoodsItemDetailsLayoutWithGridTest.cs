using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	[TestedType(typeof(GoodsItemDetailsLayoutWithGrid))]
	sealed class GoodsItemDetailsLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override int ControlBagCount => 2;

		protected override Type ExpectedGridUserControlType => typeof(EU.NCTS.GUI.Phase5DepartureGoodsItemsGridUserControl);

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (GoodsItemDetailsControlBag.Instance.GoodsItemDetailsItemNoPlusMainPackUserControl, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.DescriptionOfGoodsTextBox, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CommodityCodeTariffFindBox, ControlWidthClass.Long);
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
				yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CusC4NumberCodeFindBox, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CustomsQuantityDropEdit, ControlWidthClass.Auto);
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.GoodsItemDetailsLayoutBuilder<Business.NctsDepartureCargoDesc>();
	}
}
