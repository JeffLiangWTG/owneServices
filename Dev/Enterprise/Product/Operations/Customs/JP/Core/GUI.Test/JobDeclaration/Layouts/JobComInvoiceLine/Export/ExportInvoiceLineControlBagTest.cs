using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineControlBag))]
	sealed class ExportInvoiceLineControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames => new[] { nameof(ExportInvoiceLineControlBag.TariffFindBox) };

		protected override ControlBag GetControlBagForTesting() => ExportInvoiceLineControlBag.Instance;
	}
}
