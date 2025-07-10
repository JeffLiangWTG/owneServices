using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(ShipmentDetailsControlBag))]
sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ShipmentDetailsControlBag.Instance.PaymentMethodDropEdit);
			yield return nameof(ShipmentDetailsControlBag.Instance.PaymentMethodUserControl);
			yield return nameof(ShipmentDetailsControlBag.Instance.VatPaidByDropEdit);
			yield return nameof(ShipmentDetailsControlBag.Instance.VatPaidByUserControl);
			yield return nameof(ShipmentDetailsControlBag.Instance.ClearanceLocationDropEdit);
			yield return nameof(ShipmentDetailsControlBag.Instance.LocationOfGoodsDropEdit);
			yield return nameof(ShipmentDetailsControlBag.Instance.AdditionalDecisionInfoCheckBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ShipmentDetailsControlBag.Instance;
}
