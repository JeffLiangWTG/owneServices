using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(DeclarationDetailsControlBag))]
sealed class DeclarationDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(DeclarationDetailsControlBag.ReleaseCodeTextBox);
			yield return nameof(DeclarationDetailsControlBag.ReleaseDateEdit);
			yield return nameof(DeclarationDetailsControlBag.WriteOffDateEdit);
			yield return nameof(DeclarationDetailsControlBag.ControlChannelDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => DeclarationDetailsControlBag.Instance;
}
