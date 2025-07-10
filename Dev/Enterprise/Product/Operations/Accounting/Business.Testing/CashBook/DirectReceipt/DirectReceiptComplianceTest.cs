using System;
using Enterprise.Accounting.Business.CashBook;

namespace Enterprise.Accounting.Business.Testing
{
	public class DirectReceiptComplianceTest : DirectTransactionHeaderComplianceTestBase
	{
		protected override DirectTransactionHeaderBase GetSampleTransaction()
		{
			var transaction = ObjectCreator.CreateDirectReceipt(DateTime.Today, 250, 145, 350, 145);
			transaction.Lines[0].AL_AT = ObjectCreator.GST1.PK;
			transaction.Lines[1].AL_AT = ObjectCreator.GST1.PK;
			return transaction;
		}
	}
}