using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsControlBag))]
	sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceLineDetailsControlBag.EntryInstructionGuidDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.ObservationsTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.VehicleDetailsUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
	}
}
