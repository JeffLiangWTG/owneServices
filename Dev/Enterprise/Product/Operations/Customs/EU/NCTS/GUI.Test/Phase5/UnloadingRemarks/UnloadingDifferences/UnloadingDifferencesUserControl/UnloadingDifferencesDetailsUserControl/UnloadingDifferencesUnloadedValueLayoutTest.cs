using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(UnloadingDifferencesUnloadedValueLayout))]
	sealed class UnloadingDifferencesUnloadedValueLayoutTest : UnloadingDifferencesLayoutAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (UnloadingDifferencesDetailsControlBag.Instance.UnloadedValueLabel, ControlWidthClass.Auto);
				yield return (UnloadingDifferencesDetailsControlBag.Instance.EffectiveGrossWeightUnloadedCalcEdit, ControlWidthClass.Auto);
				yield return (UnloadingDifferencesDetailsControlBag.Instance.TotalPackagesUnloadedValueCalcEdit, ControlWidthClass.Auto);
				yield return (UnloadingDifferencesDetailsControlBag.Instance.SeparatorLabel, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (UnloadingDifferencesDetailsControlBag.Instance.RecalculateTotalsButton, ControlWidthClass.LongNoCaption);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new UnloadingDifferencesDetailsLayoutBuilder<NctsArrivalMovementHeader>();
	}
}
