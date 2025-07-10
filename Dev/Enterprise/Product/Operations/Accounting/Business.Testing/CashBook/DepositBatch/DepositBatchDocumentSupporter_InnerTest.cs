using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	[TestedType(typeof(DepositBatchDocumentSupporter))]
	public class DepositBatchDocumentSupporter_InnerTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<DepositBatch>();
		}
	}
}
