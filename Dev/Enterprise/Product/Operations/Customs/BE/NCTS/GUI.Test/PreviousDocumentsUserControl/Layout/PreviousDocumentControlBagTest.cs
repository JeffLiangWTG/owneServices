using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.GUI.Testing;

[TestedType(typeof(PreviousDocumentControlBag))]
sealed class PreviousDocumentControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(PreviousDocumentControlBag.ReferenceNumberN785UserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => PreviousDocumentControlBag.Instance;
}

