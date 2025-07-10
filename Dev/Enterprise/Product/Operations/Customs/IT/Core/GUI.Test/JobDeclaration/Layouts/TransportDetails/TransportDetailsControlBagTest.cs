using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(TransportDetailsControlBag))]
sealed class TransportDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(TransportDetailsControlBag.Instance.TransportInlandAirUserControl);
			yield return nameof(TransportDetailsControlBag.Instance.TransportInlandRoadUserControl);
			yield return nameof(TransportDetailsControlBag.Instance.InlandTransportModeAndMeansUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => TransportDetailsControlBag.Instance;
}
