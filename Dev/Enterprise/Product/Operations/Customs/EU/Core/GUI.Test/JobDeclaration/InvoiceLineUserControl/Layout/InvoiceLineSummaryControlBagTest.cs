using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLineSummaryControlBag))]
	sealed class InvoiceLineSummaryControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceLineSummaryControlBag.GSTVATDeferredConvertToLocalCurrencyControl);
				yield return nameof(InvoiceLineSummaryControlBag.StatisticalValueConvertToLocalCurrencyControl);
				yield return nameof(InvoiceLineSummaryControlBag.ValueForVatConvertToLocalCurrencyControl);
				yield return nameof(InvoiceLineSummaryControlBag.CustomsValueConvertToLocalCurrencyControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLineSummaryControlBag.Instance;
	}
}
