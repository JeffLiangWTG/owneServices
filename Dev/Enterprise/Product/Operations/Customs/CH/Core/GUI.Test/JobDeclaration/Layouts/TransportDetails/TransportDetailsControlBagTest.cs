using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(TransportDetailsControlBag))]
class TransportDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(TransportDetailsControlBag.Instance.VehicleTypeDropEdit);
			yield return nameof(TransportDetailsControlBag.Instance.DispatchCountryUserControl);
			yield return nameof(TransportDetailsControlBag.Instance.SpecificCircumstanceIndicatorDropEdit);
			yield return nameof(TransportDetailsControlBag.Instance.TransportModeDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => TransportDetailsControlBag.Instance;
}
