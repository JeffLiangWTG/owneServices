using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsControlBag))]
sealed class EntryInstructionDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(EntryInstructionDetailsControlBag.TradeTypeDropEdit);
			yield return nameof(EntryInstructionDetailsControlBag.DeclarationPurposeDropEdit);
			yield return nameof(EntryInstructionDetailsControlBag.DeclarationPurposeDetailsTextBox);
			yield return nameof(EntryInstructionDetailsControlBag.ToWarehouseLabel);
			yield return nameof(EntryInstructionDetailsControlBag.ToWarehouseUserControl);
			yield return nameof(EntryInstructionDetailsControlBag.FromWarehouseLabel);
			yield return nameof(EntryInstructionDetailsControlBag.FromWarehouseUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => EntryInstructionDetailsControlBag.Instance;
}
