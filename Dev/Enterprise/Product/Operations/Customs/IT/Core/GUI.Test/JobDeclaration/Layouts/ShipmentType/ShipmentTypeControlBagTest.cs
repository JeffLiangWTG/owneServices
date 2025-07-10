using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ShipmentTypeControlBag))]
sealed class ShipmentTypeControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ShipmentTypeControlBag.AuthorisationNumberDropEdit);
			yield return nameof(ShipmentTypeControlBag.MessageVersionDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ShipmentTypeControlBag.Instance;
}
