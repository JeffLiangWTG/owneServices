using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5DeclarationDetailsLayout))]
	sealed class Phase5DeclarationDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new DeclarationDetailsLayoutBuilder<NctsDepartureMovementHeader>();

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
				yield return (DeclarationDetailsControlBag.Instance.MrnTextBox, ControlWidthClass.Long);
				yield return (DeclarationDetailsControlBag.Instance.DepartureStatusDropEdit, ControlWidthClass.Long);
				yield return (DeclarationDetailsControlBag.Instance.PhaseStatusDropEdit, ControlWidthClass.Long);
				yield return (DeclarationDetailsControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (DeclarationDetailsControlBag.Instance.ReleaseDateEdit, ControlWidthClass.Auto);
				yield return (DeclarationDetailsControlBag.Instance.AcceptanceDateEdit, ControlWidthClass.Auto);
			}
		}
	}
}
