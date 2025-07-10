using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference.Testing
{
	[TestedType(typeof(CashbookExchangeDiff))]
	public class CashbookExchangeDiffDocumentTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CashbookExchangeDiff>();
		}
	}
}
