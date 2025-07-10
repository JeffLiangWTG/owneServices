using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsControlBag))]
	sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ShipmentDetailsControlBag.Instance.UcrAndBillTypeUserControl);
				yield return nameof(ShipmentDetailsControlBag.Instance.CargoArrivalUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ShipmentDetailsControlBag.Instance;
	}
}
