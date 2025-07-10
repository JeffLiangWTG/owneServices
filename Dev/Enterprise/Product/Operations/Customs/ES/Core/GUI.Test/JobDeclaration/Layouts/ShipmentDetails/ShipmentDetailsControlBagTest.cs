using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(ShipmentDetailsControlBag))]
sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ShipmentDetailsControlBag.RegionOrTerritoryOfDestinationCodeFindBox);
			yield return nameof(ShipmentDetailsControlBag.RegionOrTerritoryOfDestinationDropEdit);
			yield return nameof(ShipmentDetailsControlBag.DestinationStateDropEdit);
			yield return nameof(ShipmentDetailsControlBag.PartialWriteoffCheckBox);
			yield return nameof(ShipmentDetailsControlBag.GoodsLocationCodeFindBox);
			yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsOriginUserControl);
			yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsFinalDestinationUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ShipmentDetailsControlBag.Instance;
}
