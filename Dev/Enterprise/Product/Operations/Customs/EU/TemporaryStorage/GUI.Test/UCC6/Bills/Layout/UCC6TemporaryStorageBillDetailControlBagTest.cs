using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageBillDetailControlBag))]
	sealed class UCC6TemporaryStorageBillDetailControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UCC6TemporaryStorageBillDetailControl.typeDropEdit);
				yield return nameof(UCC6TemporaryStorageBillDetailControl.billNumberTextBox);
				yield return nameof(UCC6TemporaryStorageBillDetailControl.uCRNumberTextBox);
				yield return nameof(UCC6TemporaryStorageBillDetailControl.consignorAddressControl);
				yield return nameof(UCC6TemporaryStorageBillDetailControl.consigneeAddressControl);
				yield return nameof(UCC6TemporaryStorageBillDetailControl.notifyPartyAddressControl);
				yield return nameof(UCC6TemporaryStorageBillDetailControl.uCC6TemporaryStorageGrossWeightWithUnitUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UCC6TemporaryStorageBillDetailControlBag.Instance;
	}
}
