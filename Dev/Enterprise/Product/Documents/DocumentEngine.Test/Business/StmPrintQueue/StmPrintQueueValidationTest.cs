using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	sealed class StmPrintQueueValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPrintLanguageValidation()
		{
			AssertEquals("No errors", false, Queue.HasErrors);
			Queue.Validation.ValidateSQ_PrintLanguage();
			AssertEquals("No errors - default value should be valid", false, Queue.HasErrors);

			Queue.SQ_PrintLanguage = "";
			AssertEquals("Has errors - required field", true, Queue.HasErrors);

			Queue.SQ_PrintLanguage = Queue.SQ_PrintLanguage_List[0].Code;
			AssertEquals("No errors", false, Queue.HasErrors);

			Queue.SQ_PrintLanguage = "JNK"; // Junk value not in the list
			AssertEquals("Has errors", true, Queue.HasErrors);
		}

		StmPrintQueue Queue
		{
			get { return fQueue ?? (fQueue = Factory.New<StmPrintQueue>()); }
		}
		StmPrintQueue fQueue;
	}
}
