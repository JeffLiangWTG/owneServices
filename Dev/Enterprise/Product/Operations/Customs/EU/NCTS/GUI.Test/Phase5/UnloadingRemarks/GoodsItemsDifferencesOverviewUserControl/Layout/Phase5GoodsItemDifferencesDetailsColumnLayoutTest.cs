using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemDifferencesDetailsColumnLayout))]
	public class Phase5GoodsItemDifferencesDetailsColumnLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected virtual IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredValueLabel, ControlWidthClass.Long);
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredCommodityCodeCodeFindBox, ControlWidthClass.Long);
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredCusCodeCodeFindBox, ControlWidthClass.Long);
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredDescriptionTextBox, ControlWidthClass.Long);
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredGrossWeightDropEdit, ControlWidthClass.Long);
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.DeclaredNetWeightDropEdit, ControlWidthClass.Long);
			}
		}

		protected virtual IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedValueLabel, ControlWidthClass.Long);
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedCommodityCodeCodeFindBox, ControlWidthClass.Long);
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedCusCodeCodeFindBox, ControlWidthClass.Long);
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedDescriptionTextBox, ControlWidthClass.Long);
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedGrossWeightDropEdit, ControlWidthClass.Long);
				yield return (Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance.UnloadedNetWeightDropEdit, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new Phase5GoodsItemDifferencesDetailsColumnLayoutBuilder<Business.NctsArrivalCargoDesc>();
	}
}
