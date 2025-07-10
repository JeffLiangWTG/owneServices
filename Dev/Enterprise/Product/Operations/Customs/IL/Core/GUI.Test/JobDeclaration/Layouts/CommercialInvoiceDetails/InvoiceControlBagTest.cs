using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(InvoiceControlBag))]
	sealed class InvoiceControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceControlBag.InvoiceDateDateEdit);
				yield return nameof(InvoiceControlBag.SequenceTextBox);
				yield return nameof(InvoiceControlBag.SupplierCodeFindBox);
				yield return nameof(InvoiceControlBag.PreferenceAgreementDropEdit);
				yield return nameof(InvoiceControlBag.PaymentTermsDropEdit);
				yield return nameof(InvoiceControlBag.IncoTermsWithCountryCodeUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceControlBag.Instance;
	}
}
