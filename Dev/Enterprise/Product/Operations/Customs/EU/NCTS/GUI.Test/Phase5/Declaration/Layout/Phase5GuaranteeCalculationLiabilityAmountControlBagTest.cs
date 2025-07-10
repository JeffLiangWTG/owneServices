using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GuaranteeCalculationLiabilityAmountControlBag))]
	sealed class Phase5GuaranteeCalculationLiabilityAmountControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(Phase5GuaranteeCalculationLiabilityAmountControlBag.LiabilityAmountTotalValueCalculationMethodUserControl);
				yield return nameof(Phase5GuaranteeCalculationLiabilityAmountControlBag.TotalValueCalcDropEdit);
				yield return nameof(Phase5GuaranteeCalculationLiabilityAmountControlBag.LiabilityPercentageIntEdit);
				yield return nameof(Phase5GuaranteeCalculationLiabilityAmountControlBag.LiabilityAmountCalcDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => Phase5GuaranteeCalculationLiabilityAmountControlBag.Instance;
	}
}
