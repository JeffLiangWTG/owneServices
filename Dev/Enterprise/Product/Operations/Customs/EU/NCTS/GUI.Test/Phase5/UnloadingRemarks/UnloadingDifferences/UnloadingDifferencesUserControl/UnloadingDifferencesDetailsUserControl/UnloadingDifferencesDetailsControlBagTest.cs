using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(UnloadingDifferencesDetailsControlBag))]
	sealed class UnloadingDifferencesDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UnloadingDifferencesDetailsControlBag.DeclaredValueLabel);
				yield return nameof(UnloadingDifferencesDetailsControlBag.TotalGrossMassDeclaredValueCalcEdit);
				yield return nameof(UnloadingDifferencesDetailsControlBag.TotalPackagesDeclaredValueCalcEdit);
				yield return nameof(UnloadingDifferencesDetailsControlBag.InlandTransportModeDropEdit);

				yield return nameof(UnloadingDifferencesDetailsControlBag.UnloadedValueLabel);
				yield return nameof(UnloadingDifferencesDetailsControlBag.RecalculateTotalsButton);
				yield return nameof(UnloadingDifferencesDetailsControlBag.EffectiveGrossWeightUnloadedCalcEdit);
				yield return nameof(UnloadingDifferencesDetailsControlBag.TotalPackagesUnloadedValueCalcEdit);
				yield return nameof(UnloadingDifferencesDetailsControlBag.SeparatorLabel);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UnloadingDifferencesDetailsControlBag.Instance;
	}
}
