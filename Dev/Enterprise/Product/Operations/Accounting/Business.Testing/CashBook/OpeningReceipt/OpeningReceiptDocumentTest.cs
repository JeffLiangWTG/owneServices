using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.OpeningReceipt.Testing
{
	[TestedType(typeof(OpeningReceipt))]
	public class OpeningReceiptDocumentTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<OpeningReceipt>();
		}
	}
}
