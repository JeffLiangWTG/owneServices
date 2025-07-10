using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceDetailsControlBag))]
	sealed class InvoiceDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceDetailsControlBag.AgreedPlaceCodeFindBox);
				yield return nameof(InvoiceDetailsControlBag.TransportChargesMethodOfPaymentDropEdit);
				yield return nameof(InvoiceDetailsControlBag.IncoTermsAgreedPlaceLongTextControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceDetailsControlBag.Instance;
	}
}
