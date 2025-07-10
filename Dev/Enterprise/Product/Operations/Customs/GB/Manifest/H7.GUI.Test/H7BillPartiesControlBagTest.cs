using System.Collections.Generic;
using Enterprise.Customs.GB.H7.GUI.Bill;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	[TestedType(typeof(H7BillPartiesControlBag))]
	sealed class H7BillPartiesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(H7BillPartiesControlBag.VatNumberTextBox);
				yield return nameof(H7BillPartiesControlBag.PostponedVatAccountingCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => H7BillPartiesControlBag.Instance;
	}
}
