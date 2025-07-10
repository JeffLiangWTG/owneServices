using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(ShipmentDetailsControlBag))]
sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ShipmentDetailsControlBag.LocationOfGoodsCodeFindBox);
			yield return nameof(ShipmentDetailsControlBag.PresentationStartDateEdit);
			yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsOriginUserControl);
			yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsFinalDestinationUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ShipmentDetailsControlBag.Instance;
}
