using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(WarehouseControlBag))]
	sealed class WarehouseControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(WarehouseUserControl.PartGuidFindBox);
				yield return nameof(WarehouseUserControl.BondedWhsQuantityCalcDropEdit);
				yield return nameof(WarehouseUserControl.WarehouseEntryNumberTextBox);
				yield return nameof(WarehouseUserControl.BondedWHSOrderNumberTextBox);
				yield return nameof(WarehouseUserControl.WarehouseEntryLineNoCalcEdit);
				yield return nameof(WarehouseUserControl.BondedWHSOrderLineNumberCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => WarehouseControlBag.Instance;
	}
}
