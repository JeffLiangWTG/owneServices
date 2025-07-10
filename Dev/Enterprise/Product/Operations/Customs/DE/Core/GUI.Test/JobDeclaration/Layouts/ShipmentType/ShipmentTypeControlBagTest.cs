using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeControlBag))]
	sealed class ShipmentTypeControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ShipmentTypeControlBag.BorderTransportMeansDropEdit);
				yield return nameof(ShipmentTypeControlBag.MethodOfPaymentDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ShipmentTypeControlBag.Instance;
	}
}
