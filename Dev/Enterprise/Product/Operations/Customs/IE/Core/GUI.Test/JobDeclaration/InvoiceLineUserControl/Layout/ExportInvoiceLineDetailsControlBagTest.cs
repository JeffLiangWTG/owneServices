using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineDetailsControlBag))]
	sealed class ExportInvoiceLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(ExportInvoiceLineDetailsControlBag.IsMainPackCheckBox));
			}
		}

		protected override ControlBag GetControlBagForTesting() => ExportInvoiceLineDetailsControlBag.Instance;
	}
}
