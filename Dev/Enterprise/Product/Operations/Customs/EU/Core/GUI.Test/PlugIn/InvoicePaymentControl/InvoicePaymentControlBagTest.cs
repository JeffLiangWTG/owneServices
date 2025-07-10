using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.Plugin;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoicePaymentControlBag))]
	sealed class InvoicePaymentControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoicePaymentControlBag.CommercialPaymentCodeDropEdit);
				yield return nameof(InvoicePaymentControlBag.PaymentAmountCalcEdit);
				yield return nameof(InvoicePaymentControlBag.PaymentNoTextBox);
				yield return nameof(InvoicePaymentControlBag.PaymentDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoicePaymentControlBag.Instance;
	}
}

