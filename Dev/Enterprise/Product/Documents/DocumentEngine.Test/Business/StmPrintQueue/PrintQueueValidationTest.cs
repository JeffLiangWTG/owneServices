using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class PrintQueueValidationTest : TestCaseWithFactory
	{
		public void TestPrintQueueValidation()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();

			StmPrintQueue printer1 = CreateNewTestPrintQueue("Printer1");

			StmPrintQueue printer2 = CreateNewTestPrintQueue("Printer2");
			printer2.SQ_QueueDeleted = ZDateTime.Today.AddDays(-3);

			StmPrintQueue printer3 = CreateNewTestPrintQueue("Printer3");
			Env.Security.GetPrintQueueCheckPoint(printer3.PK.ToGuid(), printer3.SQ_DisplayName).IsAllowed = false;

			StmPrintQueue printer4 = CreateNewTestPrintQueue("Printer4");
			printer4.SQ_AllowPrinting = false;

			Factory.Save();

			DocDeliveryContact contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			AssertEquals("Printer Name is blank by default", ZGuid.Empty, instructions.PrinterDelivery.PrintQueuePK);
			AssertNoErrors("No Error as printer name is not mandatory when mode is not print", instructions.PrinterDelivery.PrintQueuePKInfo);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			instructions.RunPreSaveValidation();
			AssertHasError("Error as printer name is mandatory when mode is print", instructions.PrinterDelivery.PrintQueuePKInfo, "You must select a printer as some of your documents are set to be printed.");

			instructions.PrinterDelivery.PrintQueuePK = printer1.PK;
			AssertNoErrors("Printer selected so no error", instructions.PrinterDelivery.PrintQueuePKInfo);
			AssertEquals("Printer Delivery set", printer1.PK, instructions.PrinterDelivery.PrintQueuePK);

			instructions.PrinterDelivery.PrintQueuePK = printer2.PK;
			AssertHasError("Error as printer is deleted", instructions.PrinterDelivery.PrintQueuePKInfo, "The printer you have selected is not currently installed. Please see your system administrator.");

			instructions.PrinterDelivery.PrintQueuePK = printer3.PK;
			AssertHasError("Error as printer is security denied", instructions.PrinterDelivery.PrintQueuePKInfo, "You do not have the security rights to print to the selected printer.");

			AssertEquals("Printer with error still retained for GUI", printer3.PK, instructions.PrinterDelivery.PrintQueuePK);

			instructions.PrinterDelivery.PrintQueuePK = printer1.PK;
			AssertNoErrors("Printer selected so no error", instructions.PrinterDelivery.PrintQueuePKInfo);

			instructions.PrinterDelivery.PrintQueuePK = printer4.PK;
			AssertHasErrors("Printer inactive, so error", instructions.PrinterDelivery.PrintQueuePKInfo);
		}

		StmPrintQueue CreateNewTestPrintQueue(string printQueueName)
		{
			StmPrintQueue queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_QueueName = printQueueName;
			queue.SQ_DisplayName = printQueueName;
			return queue;
		}
	}
}
