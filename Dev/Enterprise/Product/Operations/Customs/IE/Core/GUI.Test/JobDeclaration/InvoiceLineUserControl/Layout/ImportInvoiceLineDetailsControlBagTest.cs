using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineDetailsControlBag))]
	sealed class ImportInvoiceLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(ImportInvoiceLineDetailsControlBag.CountryOfOriginCodeFindBox));
				yield return (nameof(ImportInvoiceLineDetailsControlBag.CountryOfSupplyCodeFindBox));
			}
		}

		protected override ControlBag GetControlBagForTesting() => ImportInvoiceLineDetailsControlBag.Instance;
	}
}
