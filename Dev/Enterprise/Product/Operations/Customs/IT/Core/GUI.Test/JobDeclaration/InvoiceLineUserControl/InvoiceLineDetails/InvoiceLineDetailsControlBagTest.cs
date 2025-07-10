using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(InvoiceLineDetailsControlBag))]
sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(InvoiceLineDetailsControlBag.PortTaxRateDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.VatTypeAndDescriptionUserControl);
			yield return nameof(InvoiceLineDetailsControlBag.GoodsOriginDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.OriginCountryStateUserControl);
			yield return nameof(InvoiceLineDetailsControlBag.CountryOfDestinationDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.CountryOfExportDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.InvoiceNumberDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
}
