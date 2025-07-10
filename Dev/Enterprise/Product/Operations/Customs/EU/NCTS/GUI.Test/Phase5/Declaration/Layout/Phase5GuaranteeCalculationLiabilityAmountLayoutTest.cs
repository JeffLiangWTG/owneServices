using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GuaranteeCalculationLiabilityAmountLayout))]
	sealed class Phase5GuaranteeCalculationLiabilityAmountLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new Phase5GuaranteeCalculationLiabilityAmountLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Phase5GuaranteeCalculationLiabilityAmountControlBag.Instance.LiabilityAmountTotalValueCalculationMethodUserControl, ControlWidthClass.Long);
				yield return (Phase5GuaranteeCalculationLiabilityAmountControlBag.Instance.TotalValueCalcDropEdit, ControlWidthClass.Long);
				yield return (Phase5GuaranteeCalculationLiabilityAmountControlBag.Instance.LiabilityPercentageIntEdit, ControlWidthClass.Medium);
				yield return (Phase5GuaranteeCalculationLiabilityAmountControlBag.Instance.LiabilityAmountCalcDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
