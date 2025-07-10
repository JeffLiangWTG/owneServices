using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.CommercialInvoice;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing.CommercialInvoice.Layouts
{
	[TestedType(typeof(EUInvoiceHeaderDetailsControlBag))]
	sealed class EUInvoiceHeaderDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EUInvoiceHeaderDetailsControlBag.AgreedPlaceCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EUInvoiceHeaderDetailsControlBag.Instance;
	}
}
