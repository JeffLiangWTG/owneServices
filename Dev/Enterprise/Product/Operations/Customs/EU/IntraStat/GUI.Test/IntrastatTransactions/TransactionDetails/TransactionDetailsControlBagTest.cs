
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestedType(typeof(TransactionDetailsControlBag))]
	sealed class TransactionDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransactionDetailsControlBag.SupplierVATTextBox);
				yield return nameof(TransactionDetailsControlBag.SupplierNameTextBox);
				yield return nameof(TransactionDetailsControlBag.ConsigneeVATTextBox);
				yield return nameof(TransactionDetailsControlBag.ConsigneeNameTextBox);
				yield return nameof(TransactionDetailsControlBag.CountryOfReceiptDropEdit);
				yield return nameof(TransactionDetailsControlBag.CountryOfSupplyDropEdit);
				yield return nameof(TransactionDetailsControlBag.IncoTermDropEdit);
				yield return nameof(TransactionDetailsControlBag.NatureOfTransactionDropEdit);
				yield return nameof(TransactionDetailsControlBag.TransactionDateEdit);
				yield return nameof(TransactionDetailsControlBag.TradersReferenceTextBox);
				yield return nameof(TransactionDetailsControlBag.ModeOfTransportDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransactionDetailsControlBag.Instance;
	}
}
