using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.GUI.Testing
{
	[TestedType(typeof(COManifestControlBag))]
	sealed class COManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(COManifestControlBag.CargoDispositionDropEdit);
				yield return nameof(COManifestControlBag.DeliveryModeDropEdit);
				yield return nameof(COManifestControlBag.TravelDocumentTypeDropEdit);
				yield return nameof(COManifestControlBag.MultimodalCheckBox);
				yield return nameof(COManifestControlBag.PrecursorsCheckBox);
				yield return nameof(COManifestControlBag.CarriersLiabilityCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => COManifestControlBag.Instance;
	}
}
