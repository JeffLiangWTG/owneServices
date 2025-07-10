using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ShipmentDetailsControlBag))]
sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsOriginUserControl);
			yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsFinalDestinationUserControl);
			yield return nameof(ShipmentDetailsControlBag.LocationQualifierDropEdit);
			yield return nameof(ShipmentDetailsControlBag.GoodsLocationDUserControl);
			yield return nameof(ShipmentDetailsControlBag.GoodsLocationFUserControl);
			yield return nameof(ShipmentDetailsControlBag.GoodsLocationFCUserControl);
			yield return nameof(ShipmentDetailsControlBag.GoodsLocationLBLCUserControl);
			yield return nameof(ShipmentDetailsControlBag.SubLocationTextBox);
			yield return nameof(ShipmentDetailsControlBag.LocationOfGoodsUserControl);
			yield return nameof(ShipmentDetailsControlBag.AdditionalDeliveryTermsTextBox);
		}
	}
	protected override ControlBag GetControlBagForTesting() => ShipmentDetailsControlBag.Instance;
}
