using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

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
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new GoodsItemDetailsLayoutBuilder<Business.NctsDepartureCargoDesc>();

	protected override Type ExpectedGridUserControlType => typeof(EU.NCTS.GUI.Phase5DepartureGoodsItemsGridUserControl);

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.ItemNumberTextBox, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.DescriptionOfGoodsTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (GoodsItemDetailsControlBag.Instance.HarmonisedTariffFindBox, ControlWidthClass.Long);
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
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.UNDangerousGoodsUserControl, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance.CusC4NumberCodeFindBox, ControlWidthClass.Long);
		}
	}
}
