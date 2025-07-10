using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(ArrivalDeclarationDetailsControlBag))]
	sealed class ArrivalDeclarationDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ArrivalDeclarationDetailsControlBag.StatusDropEdit);
				yield return nameof(ArrivalDeclarationDetailsControlBag.MessageStatusDropEdit);
				yield return nameof(ArrivalDeclarationDetailsControlBag.PhaseDropEdit);
				yield return nameof(ArrivalDeclarationDetailsControlBag.SeparatorLabel);
				yield return nameof(ArrivalDeclarationDetailsControlBag.ReleaseDateEdit);
				yield return nameof(ArrivalDeclarationDetailsControlBag.AcceptanceDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ArrivalDeclarationDetailsControlBag.Instance;
	}
}
