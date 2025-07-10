using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(Phase5DeclarationDetailsLayout))]
sealed class Phase5DeclarationDetailsLayoutTest : LayoutsAbstractTest
{
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
			var commonBag = EU.NCTS.GUI.DeclarationDetailsControlBag.Instance;
			yield return (commonBag.MrnTextBox, ControlWidthClass.Long);
			yield return (commonBag.DepartureStatusDropEdit, ControlWidthClass.Long);
			yield return (commonBag.PhaseStatusDropEdit, ControlWidthClass.Long);
			yield return (commonBag.MessageStatusDropEdit, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			var commonBag = EU.NCTS.GUI.DeclarationDetailsControlBag.Instance;
			yield return (commonBag.ReleaseDateEdit, ControlWidthClass.Auto);
			yield return (commonBag.AcceptanceDateEdit, ControlWidthClass.Auto);
			yield return (commonBag.ActivationDeadlineDateEdit, ControlWidthClass.Auto);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.DeclarationDetailsLayoutBuilder<Business.NctsDepartureMovementHeader>();

	protected override int ControlBagCount => 1;
}
