using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	[TestedType(typeof(BankTransferDocumentSupporter))]
	public class BankTransferDocumentTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<BankTransferFromRow>();
		}

		public void TestBusinessContext()
		{
			BankTransferFromRow cBTransfer = Factory.New<BankTransferFromRow>();
			AssertEquals(BusinessContext.APTransaction, cBTransfer.DocumentSupporter.BusinessContext);
		}
	}
}
