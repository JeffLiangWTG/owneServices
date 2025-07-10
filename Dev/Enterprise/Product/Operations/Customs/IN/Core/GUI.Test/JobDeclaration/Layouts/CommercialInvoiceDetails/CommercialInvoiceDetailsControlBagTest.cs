using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(CommercialInvoiceDetailsControlBag))]
sealed class CommercialInvoiceDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(CommercialInvoiceDetailsControlBag.ExporterContractNumberTextBox);
			yield return nameof(CommercialInvoiceDetailsControlBag.PaymentDaysCalcEdit);
			yield return nameof(CommercialInvoiceDetailsControlBag.AuthorizedEconomicOperatorGuidFindBox);
			yield return nameof(CommercialInvoiceDetailsControlBag.AuthorizedEconomicOperatorCountryTextBox);
			yield return nameof(CommercialInvoiceDetailsControlBag.AuthorizedEconomicOperatorCodeTextBox);
			yield return nameof(CommercialInvoiceDetailsControlBag.AuthorizedEconomicOperatorRoleTextBox);
			yield return nameof(CommercialInvoiceDetailsControlBag.IGSTPaymentStatusDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => CommercialInvoiceDetailsControlBag.Instance;
}
