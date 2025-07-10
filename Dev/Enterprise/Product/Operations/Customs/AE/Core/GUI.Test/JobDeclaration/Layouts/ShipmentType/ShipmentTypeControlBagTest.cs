using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(ShipmentTypeControlBag))]
sealed class ShipmentTypeControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => ShipmentTypeControlBag.Instance;

	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ShipmentTypeControlBag.Instance.ExitPointDropEdit);
			yield return nameof(ShipmentTypeControlBag.Instance.ClearanceLocationDropEdit);
		}
	}
}
