using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

[TestedType(typeof(BillDetailsControlBag))]
sealed class BillDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(BillDetailsControlBag.SplitBillCheckBox);
			yield return nameof(BillDetailsControlBag.SplitBillNumberCodeFindBox);
			yield return nameof(BillDetailsControlBag.ForwarderMPCITextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => BillDetailsControlBag.Instance;
}
