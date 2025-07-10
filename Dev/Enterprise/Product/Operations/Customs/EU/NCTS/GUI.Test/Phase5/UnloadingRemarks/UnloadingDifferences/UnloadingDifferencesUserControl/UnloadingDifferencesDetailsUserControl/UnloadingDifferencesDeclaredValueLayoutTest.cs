using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(UnloadingDifferencesDeclaredValueLayout))]
	sealed class UnloadingDifferencesDeclaredValueLayoutTest : UnloadingDifferencesLayoutAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (UnloadingDifferencesDetailsControlBag.Instance.DeclaredValueLabel, ControlWidthClass.Auto);
				yield return (UnloadingDifferencesDetailsControlBag.Instance.TotalGrossMassDeclaredValueCalcEdit, ControlWidthClass.Auto);
				yield return (UnloadingDifferencesDetailsControlBag.Instance.TotalPackagesDeclaredValueCalcEdit, ControlWidthClass.Auto);
				yield return (UnloadingDifferencesDetailsControlBag.Instance.InlandTransportModeDropEdit, ControlWidthClass.Auto);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new UnloadingDifferencesDetailsLayoutBuilder<NctsArrivalMovementHeader>();
	}
}
