using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(InvoiceDetailsControlBag))]
sealed class InvoiceDetailsControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => InvoiceDetailsControlBag.Instance;

	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(InvoiceDetailsControlBag.AgreedPlaceCodeDropEdit);
		}
	}
}
