using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;

namespace Enterprise.Accounting.Business.Testing
{
	public class TransactionHeaderMatchingMonitorFactoryTest : TestCaseWithFactory
	{
		public void TestCreateMatchingMonitor()
		{
			var apJournal = Creator.CreateJournal<APJournal>(199m, new CargoWise.Types.ZDateTime(2025, 01, 22), Creator.ABIGAS.PK);
			var arJournal = Creator.CreateJournal<ARJournal>(199m, new CargoWise.Types.ZDateTime(2025, 01, 22), Creator.ABIGAS.PK);
			var apInvoice = Creator.CreateInvoice(typeof(APInvoice), Creator.AUD, 1m);
			var monitorFactory = new TransactionHeaderMatchingMonitorFactory();

			Assert("monitorFactory will create JournalMatchingMonitor if trnasction header is AP Journal", monitorFactory.CreateMatchingMonitor(apJournal) is JournalMatchingMonitor);
			Assert("monitorFactory will create JournalMatchingMonitor if trnasction header is AR Journal", monitorFactory.CreateMatchingMonitor(arJournal) is JournalMatchingMonitor);
			Assert("monitorFactory will create TransactionHeaderMatchingMonitor if trnasction header is not AR/AP Journal", !(monitorFactory.CreateMatchingMonitor(apInvoice) is JournalMatchingMonitor));
		}

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;
	}
}
