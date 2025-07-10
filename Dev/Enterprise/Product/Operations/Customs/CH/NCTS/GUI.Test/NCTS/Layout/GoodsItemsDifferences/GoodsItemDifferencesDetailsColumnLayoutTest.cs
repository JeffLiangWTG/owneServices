using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(GoodsItemDifferencesDetailsColumnLayout))]
class GoodsItemDifferencesDetailsColumnLayoutTest : EU.NCTS.GUI.Testing.Phase5GoodsItemDifferencesDetailsColumnLayoutTest
{
	protected override Type ExpectedGridUserControlType => typeof(GoodsItemDifferencesDetailsGridColumnLayout);

	protected override IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadingRemarkCodeDropEdit, ControlWidthClass.Long);
			yield return (GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadingRemarkTextTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredValueLabel, ControlWidthClass.Long);
			yield return (GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredCommodityCodeCodeFindBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredCusCodeCodeFindBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredDescriptionTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredGrossWeightDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredNetWeightDropEdit, ControlWidthClass.Long);
		}
	}

	protected override IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedValueLabel, ControlWidthClass.Long);
			yield return (GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedCommodityCodeCodeFindBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedCusCodeCodeFindBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedDescriptionTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedGrossWeightDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedNetWeightDropEdit, ControlWidthClass.Long);
		}
	}

	protected override int ControlBagCount => 2;
}
