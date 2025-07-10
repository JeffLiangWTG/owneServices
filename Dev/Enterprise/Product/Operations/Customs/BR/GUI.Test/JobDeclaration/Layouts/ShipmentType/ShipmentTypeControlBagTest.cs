using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeControlBag))]
	sealed class ShipmentTypeControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ShipmentTypeControlBag.OperationTypeDropEdit);
				yield return nameof(ShipmentTypeControlBag.DeclarantTypeDropEdit);
				yield return nameof(ShipmentTypeControlBag.DispatchModalityDropEdit);
				yield return nameof(ShipmentTypeControlBag.IsMultimodalCheckBox);
				yield return nameof(ShipmentTypeControlBag.SpecialTransportDropEdit);
				yield return nameof(ShipmentTypeControlBag.BRTransportModeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ShipmentTypeControlBag.Instance;
	}
}
