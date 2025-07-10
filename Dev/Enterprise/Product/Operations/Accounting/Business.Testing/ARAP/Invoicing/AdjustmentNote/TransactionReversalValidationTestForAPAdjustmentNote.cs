using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TransactionReversalValidationTestForAPAdjustmentNote : InvoicingBaseReversalValidationTestCase
	{
		protected override Type HeaderType
		{
			get { return typeof(APAdjustmentNote); }
		}

		protected override InvoicingBase GetOriginalTransaction() => Factory.NewWithValidTestData(HeaderType) as InvoicingBase;

		protected override ZString TransactionLedger => LedgerTypes.AccountsPayable;

		protected override bool ShouldFillTransactionNumber => true;
	}
}
