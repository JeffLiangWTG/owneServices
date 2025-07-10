using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocCashBookTransfer))]
	sealed class DocCashBookTransferTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocCashBookTransfer.New(Transfer, Factory) };
		}

		BankTransferFromRow Transfer;
		protected override void SetUp()
		{
			Transfer = Factory.New<BankTransferFromRow>();
			base.SetUp();
		}
	}
}
