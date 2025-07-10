#if DEBUG

using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.PrintProcessing
{
	public static class TestAssistant
	{
		public const string PrintQueueNameForTesting = "Test Printer";

		public static StmDeliveryGroup CreateNewDeliveryGroup(BusinessObjectFactory factory)
		{
			StmDeliveryGroup deliveryGroup = factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			return deliveryGroup;
		}

		public static StmDeliveryGroup CreateNewDeliveryGroup(BusinessObjectFactory factory, string subjectLine)
		{
			StmDeliveryGroup deliveryGroup = CreateNewDeliveryGroup(factory);
			deliveryGroup.SB_EmailSubjectLine = subjectLine;
			return deliveryGroup;
		}

		public static StmPrintJob CreateNewPrintJobWithDeliveryGroup(BusinessObjectFactory factory)
		{
			StmPrintJob printJob = factory.New<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(factory).PK;
			return printJob;
		}

		public static StmPrintJob CreateNewPrintJobWithDeliveryGroup(BusinessObjectFactory factory, string subjectLine)
		{
			StmPrintJob printJob = factory.New<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(factory, subjectLine).PK;
			return printJob;
		}

		public static StmPrintQueue CreateTestPrintQueue(BusinessObjectFactory factory)
		{
			var printQueue = factory.New<StmPrintQueue>();
			printQueue.SQ_QueueName = PrintQueueNameForTesting;
			printQueue.SQ_DisplayName = PrintQueueNameForTesting;
			printQueue.SQ_ServerName = System.Environment.MachineName;
			factory.Save();
			return printQueue;
		}

		public static void SetupEmailAddressForAllStaff(BusinessObjectFactory factory)
		{
			GlbStaff[] staffmembers = factory.Load<GlbStaff>(new ZQuery());
			staffmembers[0].GS_IsOperational = false;
			staffmembers[0].GS_IsActive = true;
			foreach (GlbStaff staff in staffmembers)
			{
				staff.GS_EmailAddress = "test@test.com";
			}
		}
	}
}
#endif