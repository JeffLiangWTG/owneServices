using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(CFXVoucherProvider))]
	public class CFXVoucherProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDebitInFirstRow()
		{
			JCJournalHeader journal = Factory.NewWithValidTestData(typeof(JCJournalHeader)) as JCJournalHeader;
			journal.Lines.AddNew();
			journal.Lines[0].AL_LineType = TransactionLineTypes.Revenue;
			journal.Lines[0].AL_ExchangeRate = 1.0m;
			journal.Lines[0].AL_LineAmount = -120.0m;
			CFXVoucherProvider testProvider = new CFXVoucherProvider(journal);
			AssertEquals(120m, testProvider.VoucherLines[0].DebitAmount);
			journal.Lines[0].AL_LineAmount = 120.0m;
			testProvider = new CFXVoucherProvider(journal);
			AssertEquals(120m, testProvider.VoucherLines[0].DebitAmount);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CFXVoucherProvider(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}
	}
}
