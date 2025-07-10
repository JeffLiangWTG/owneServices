using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5ArrivalSummaryDeclarationControlBag))]
	class Phase5ArrivalSummaryDeclarationControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(Phase5ArrivalSummaryDeclarationControlBag.SummaryTypeDropEdit);
				yield return nameof(Phase5ArrivalSummaryDeclarationControlBag.PreviousSummaryDeclarationTextBox);
				yield return nameof(Phase5ArrivalSummaryDeclarationControlBag.G4PreviousDocumentGroupBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => Phase5ArrivalSummaryDeclarationControlBag.Instance;
	}
}
