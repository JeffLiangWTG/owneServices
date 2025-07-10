using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TransactionReversalValidationTestForARAdjustmentNote : InvoicingBaseReversalValidationTestCase
	{
		protected override Type HeaderType
		{
			get { return typeof(ARAdjustmentNote); }
		}

		protected override InvoicingBase GetOriginalTransaction() => Factory.NewWithValidTestData(HeaderType) as InvoicingBase;

		protected override ZString TransactionLedger => LedgerTypes.AccountsReceivable;
	}
}
