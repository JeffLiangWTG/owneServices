using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(ShipmentDetailsControlBag))]
sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ShipmentDetailsControlBag.Instance.TypeOfGoodsDropEdit);
			yield return nameof(ShipmentDetailsControlBag.Instance.OperationalStatusDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ShipmentDetailsControlBag.Instance;
}
