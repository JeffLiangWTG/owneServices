using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(Phase5MiscellaneousOptionsLayout))]
sealed class Phase5MiscellaneousOptionsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscellaneousOptionsLayoutBuilder<Business.NctsHeader>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.MiscellaneousOptionsControlBag.Instance.BranchCodeFindBox, ControlWidthClass.Long);
			yield return (MiscellaneousOptionsControlBag.Instance.CustomsProfileDropEdit, ControlWidthClass.Long);
		}
	}
}
