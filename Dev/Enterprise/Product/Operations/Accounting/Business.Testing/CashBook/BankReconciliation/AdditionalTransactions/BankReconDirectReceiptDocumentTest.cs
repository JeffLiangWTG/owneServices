using Enterprise.Accounting.Business.CashBook.DirectReceipt.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(BankReconDirectReceipt))]
	public class BankReconDirectReceiptDocumentTest : DirectReceiptDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<BankReconDirectReceipt>();
		}
	}
}
