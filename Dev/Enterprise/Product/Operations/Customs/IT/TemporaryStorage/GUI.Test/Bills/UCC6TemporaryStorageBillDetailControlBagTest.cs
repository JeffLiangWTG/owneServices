using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(UCC6TemporaryStorageBillDetailControlBag))]
sealed class UCC6TemporaryStorageBillDetailControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(UCC6TemporaryStorageBillDetailControl.GoodsDescTextBox);
			yield return nameof(UCC6TemporaryStorageBillDetailControl.LrnTextBox);
			yield return nameof(UCC6TemporaryStorageBillDetailControl.MrnTextBox);
			yield return nameof(UCC6TemporaryStorageBillDetailControl.GrossWeightCalcEdit);
			yield return nameof(UCC6TemporaryStorageBillDetailControl.NetWeightCalcEdit);
			yield return nameof(UCC6TemporaryStorageBillDetailControl.SuppQtyCalcEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => UCC6TemporaryStorageBillDetailControlBag.Instance;
}
