using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5DeclarationDetailsLayout))]
	sealed class Phase5DeclarationDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.DeclarationDetailsLayoutBuilder<Business.NctsDepartureMovementHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				var euBag = EU.NCTS.GUI.DeclarationDetailsControlBag.Instance;
				var esBag = DeclarationDetailsControlBag.Instance;
				yield return (euBag.MrnTextBox, ControlWidthClass.Long);
				yield return (euBag.DepartureStatusDropEdit, ControlWidthClass.Long);
				yield return (euBag.PhaseStatusDropEdit, ControlWidthClass.Long);
				yield return (euBag.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (esBag.CircuitTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				var esBag = DeclarationDetailsControlBag.Instance;
				yield return (esBag.AcceptanceDateDateEdit, ControlWidthClass.Long);
				yield return (esBag.ClearanceNumberTextBox, ControlWidthClass.Long);
				yield return (esBag.ClearanceDateDateEdit, ControlWidthClass.Long);
				yield return (esBag.ArrivalLimitDateEdit, ControlWidthClass.Long);
			}
		}
	}
}
