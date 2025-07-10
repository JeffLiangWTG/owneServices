using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing;

[TestedType(typeof(ArrivalDeclarationDetailsControlBag))]
sealed class ArrivalDeclarationDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ArrivalDeclarationDetailsControlBag.CircuitTextBox);
			yield return nameof(ArrivalDeclarationDetailsControlBag.ArrivalSummaryDeclarationUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ArrivalDeclarationDetailsControlBag.Instance;
}
