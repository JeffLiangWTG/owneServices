using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectReceipt.Testing
{
	[TestedType(typeof(DirectReceipt))]
	public class DirectReceiptDocumentTest : TransactionHeaderDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<DirectReceipt>();
		}
	}
}
