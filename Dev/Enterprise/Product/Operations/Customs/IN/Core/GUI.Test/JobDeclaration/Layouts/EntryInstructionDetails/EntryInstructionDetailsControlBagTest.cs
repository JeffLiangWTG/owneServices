using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsControlBag))]
sealed class EntryInstructionDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(EntryInstructionDetailsControlBag.TotalGrossWeightAndNetWeightGroupBox);
			yield return nameof(EntryInstructionDetailsControlBag.RBIWaiverNumberTextBox);
			yield return nameof(EntryInstructionDetailsControlBag.RBIWaiverDateEdit);
			yield return nameof(EntryInstructionDetailsControlBag.PackagesQtyCalcDropEdit);
			yield return nameof(EntryInstructionDetailsControlBag.LoosePackagesCalcDropEdit);
			yield return nameof(EntryInstructionDetailsControlBag.TotalContainerZIntEdit);
			yield return nameof(EntryInstructionDetailsControlBag.ShippingBillOverrideUserControl);
			yield return nameof(EntryInstructionDetailsControlBag.MessageAndCustomsStatusGroupBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => EntryInstructionDetailsControlBag.Instance;
}
