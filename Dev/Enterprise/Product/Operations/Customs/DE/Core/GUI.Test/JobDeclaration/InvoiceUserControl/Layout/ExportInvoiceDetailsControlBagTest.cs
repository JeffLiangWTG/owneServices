using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Declaration.Testing
{
	[TestedType(typeof(ExportInvoiceDetailsControlBag))]
	sealed class ExportInvoiceDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ExportInvoiceDetailsControlBag.Instance.FreeOfChargeCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ExportInvoiceDetailsControlBag.Instance;
	}
}
