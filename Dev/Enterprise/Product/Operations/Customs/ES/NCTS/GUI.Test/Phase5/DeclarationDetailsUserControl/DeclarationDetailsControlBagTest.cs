using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(DeclarationDetailsControlBag))]
	sealed class DeclarationDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DeclarationDetailsControlBag.AcceptanceDateDateEdit);
				yield return nameof(DeclarationDetailsControlBag.CircuitTextBox);
				yield return nameof(DeclarationDetailsControlBag.ClearanceNumberTextBox);
				yield return nameof(DeclarationDetailsControlBag.ClearanceDateDateEdit);
				yield return nameof(DeclarationDetailsControlBag.ArrivalLimitDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DeclarationDetailsControlBag.Instance;
	}
}
