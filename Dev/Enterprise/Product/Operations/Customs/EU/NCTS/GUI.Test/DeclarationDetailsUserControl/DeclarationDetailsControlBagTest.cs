using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(DeclarationDetailsControlBag))]
	sealed class DeclarationDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DeclarationDetailsControlBag.MrnTextBox);
				yield return nameof(DeclarationDetailsControlBag.DepartureStatusDropEdit);
				yield return nameof(DeclarationDetailsControlBag.PhaseStatusDropEdit);
				yield return nameof(DeclarationDetailsControlBag.MessageStatusDropEdit);
				yield return nameof(DeclarationDetailsControlBag.ReleaseDateEdit);
				yield return nameof(DeclarationDetailsControlBag.AcceptanceDateEdit);
				yield return nameof(DeclarationDetailsControlBag.ActivationDeadlineDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DeclarationDetailsControlBag.Instance;
	}
}
