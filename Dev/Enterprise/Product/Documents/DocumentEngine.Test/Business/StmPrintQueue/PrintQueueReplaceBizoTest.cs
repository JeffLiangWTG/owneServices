using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(PrintQueueReplaceBizo))]
	sealed class PrintQueueReplaceBizoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PrintQueueReplaceBizo(Factory.New<StmPrintQueue>());
		}

		public void TestProperties()
		{
			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			var replaceBizo = new PrintQueueReplaceBizo(printQueue);

			AssertEquals("DisplayName", replaceBizo.DisplayName, printQueue.SQ_DisplayName);
			AssertEquals("PrintQueuePK", replaceBizo.PrintQueuePK, printQueue.PK);
			AssertEquals("ReplacePrintQueuePK", replaceBizo.ReplacePrintQueuePK, ZGuid.Empty);
			AssertEquals("Factory", replaceBizo.Factory, printQueue.Factory);
		}

		public void TestReplacePrintQueueList()
		{
			var printQueue1 = Factory.NewWithValidTestData<StmPrintQueue>();
			var printQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue2.SQ_AllowPrinting = true;
			var printQueue3 = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue3.SQ_AllowPrinting = false;

			var replaceBizo = new PrintQueueReplaceBizo(printQueue1);
			replaceBizo.ReplacePrintQueue_List.Load();
			AssertEquals(1, replaceBizo.ReplacePrintQueue_List.Count);
		}
	}
}
