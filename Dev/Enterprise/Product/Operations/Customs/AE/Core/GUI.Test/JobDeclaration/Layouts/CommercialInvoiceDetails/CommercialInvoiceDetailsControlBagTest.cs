using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(CommercialInvoiceDetailsControlBag))]
sealed class CommercialInvoiceDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(CommercialInvoiceDetailsControlBag.Instance.PaymentMethodDropEdit);
			yield return nameof(CommercialInvoiceDetailsControlBag.Instance.ValuationCodeDropEdit);
			yield return nameof(CommercialInvoiceDetailsControlBag.Instance.TotNoOfInvPagesCalcEdit);
			yield return nameof(CommercialInvoiceDetailsControlBag.Instance.InvoiceTypeDropEdit);
			yield return nameof(CommercialInvoiceDetailsControlBag.Instance.AttestationNoTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => CommercialInvoiceDetailsControlBag.Instance;
}
