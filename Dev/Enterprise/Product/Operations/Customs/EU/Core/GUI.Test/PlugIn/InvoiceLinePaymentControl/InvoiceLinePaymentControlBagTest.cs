using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.Plugin;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLinePaymentControlBag))]
	sealed class InvoiceLinePaymentControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceLinePaymentControlBag.CommercialPaymentCodeDropEdit);
				yield return nameof(InvoiceLinePaymentControlBag.PaymentAmountCalcEdit);
				yield return nameof(InvoiceLinePaymentControlBag.PaymentNoTextBox);
				yield return nameof(InvoiceLinePaymentControlBag.PaymentDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLinePaymentControlBag.Instance;
	}
}

