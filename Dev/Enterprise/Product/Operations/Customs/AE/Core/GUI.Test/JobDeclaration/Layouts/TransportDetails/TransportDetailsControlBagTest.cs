using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(TransportDetailsControlBag))]
sealed class TransportDetailsControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => TransportDetailsControlBag.Instance;

	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(TransportDetailsControlBag.Instance.PlaceOfDischargeDropEdit);
		}
	}
}
