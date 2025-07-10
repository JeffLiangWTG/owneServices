using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsControlBag))]
	sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceLineDetailsControlBag.InvoiceNumberDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.PreferenceDocNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
	}
}
