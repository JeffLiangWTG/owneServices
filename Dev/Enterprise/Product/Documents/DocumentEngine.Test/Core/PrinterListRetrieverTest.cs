using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Public.Testing
{
	sealed class PrinterListRetrieverTest : TestCaseWithFactory
	{
		public void TestRetrieve()
		{
			StmPrintQueue queue1 = Factory.New<StmPrintQueue>();
			queue1.SQ_QueueName = "Queue1";
			queue1.SQ_DisplayName = "Queue 1";

			StmPrintQueue queue2 = Factory.New<StmPrintQueue>();
			queue2.SQ_QueueName = "Queue2";
			queue2.SQ_DisplayName = "Queue 2";

			Factory.Save();

			CodeDescriptionPairList printers = PrinterListRetriever.Retrieve();
			AssertNotNull(printers);
			Assert("2 printers", printers.Count == 2);
		}

		PrinterListRetriever PrinterListRetriever
		{
			get { return printerListRetriever ?? (printerListRetriever = new PrinterListRetriever()); }
		}
		PrinterListRetriever printerListRetriever;
	}
}
