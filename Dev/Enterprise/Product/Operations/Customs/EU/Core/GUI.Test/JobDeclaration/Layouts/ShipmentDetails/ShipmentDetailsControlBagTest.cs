using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsControlBag))]
	sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsQuantitiesUserControl);
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsCountUserControl);
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsIncoTermsPlaceUserControl);
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsUnlocoIncoTermsPlaceUserControl);
				yield return nameof(ShipmentDetailsControlBag.GoodsLocationDropEdit);
				yield return nameof(ShipmentDetailsControlBag.AgentsReferenceTextBox);
				yield return nameof(ShipmentDetailsControlBag.UCRTextBox);
				yield return nameof(ShipmentDetailsControlBag.ShipmentIncoTermPlaceTextBox);
				yield return nameof(ShipmentDetailsControlBag.AgreedPlaceCodeFindBox);
				yield return nameof(ShipmentDetailsControlBag.RegionOfDestinationDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ShipmentDetailsControlBag.Instance;
	}
}
