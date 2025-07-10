using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderControlBag))]
	sealed class JobComInvoiceHeaderControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(JobComInvoiceHeaderControlBag.GrossWeightCalcDropEdit);
				yield return nameof(JobComInvoiceHeaderControlBag.NetWeightCalcDropEdit);
				yield return nameof(JobComInvoiceHeaderControlBag.InvoiceAmountConvertToLocalCurrencyControl);
				yield return nameof(JobComInvoiceHeaderControlBag.IncoTermsUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => JobComInvoiceHeaderControlBag.Instance;
	}
}
