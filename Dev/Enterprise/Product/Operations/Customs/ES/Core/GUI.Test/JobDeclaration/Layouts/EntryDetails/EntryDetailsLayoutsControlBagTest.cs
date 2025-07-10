using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(EntryDetailsLayoutsControlBag))]
	sealed class EntryDetailsLayoutsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EntryDetailsLayoutsControlBag.InvoiceAmountCalcDropEdit);
				yield return nameof(EntryDetailsLayoutsControlBag.CircuitTextBox);
				yield return nameof(EntryDetailsLayoutsControlBag.CircuitCanTextBox);
				yield return nameof(EntryDetailsLayoutsControlBag.CSVClearanceTextBox);
				yield return nameof(EntryDetailsLayoutsControlBag.VATDeferredCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EntryDetailsLayoutsControlBag.Instance;
	}
}
