using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

[TestedType(typeof(ManifestControlBag))]
sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ManifestControlBag.Instance.CarrierMPCITextBox);
			yield return nameof(ManifestControlBag.Instance.ShippingAgentMPCITextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ManifestControlBag.Instance;
}
