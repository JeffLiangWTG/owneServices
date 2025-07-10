using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(UnloadingDetailsLayout))]
sealed class UnloadingDetailsLayoutTest : LayoutsAbstractTest
{
	protected override ICommonLayoutBuilder CommonLayoutBuilder => new UnloadingDetailsLayoutBuilder<Business.NctsArrivalMovementHeader>();

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
			yield return (UnloadingDetailsControlBag.Instance.UnloadingDateDateEdit, ControlWidthClass.Medium);
			yield return (UnloadingDetailsControlBag.Instance.UnloadingConformCheckBox, ControlWidthClass.Long);
			yield return (UnloadingDetailsControlBag.Instance.UnloadingRemarksTextBox, ControlWidthClass.Long);
		}
	}
}
