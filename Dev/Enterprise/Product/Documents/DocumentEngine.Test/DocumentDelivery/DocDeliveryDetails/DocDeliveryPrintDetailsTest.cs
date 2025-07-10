using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(DocDeliveryPrintDetails))]
	sealed class DocDeliveryPrintDetailsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidatePrintQueue_NeedValidatePrintQueue()
		{
			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue.SQ_DisplayName = "Zubs Printer";
			printQueue.SQ_AllowPrinting = true;
			Factory.Save();

			var reportCommand = Factory.New<ReportCommand>();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Report",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT col1, col2 FROM XXX]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""VarCharMax"", HeadingText=""VarCharMax""]
{A}-[#SectionBody]
{B}-[Blah]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			using (var documentPack = new DocumentPack(reportCommand))
			{
				var instructions = new DeliveryInstructions(documentPack);
				var deliverable = (IDeliverable)instructions.DocumentsToBeDelivered.First();
				var contact = instructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

				instructions.RunPreSaveValidation();
				AssertHasError("Error as printer name is mandatory when mode is print", instructions.PrinterDelivery.PrintQueuePKInfo, "You must select a printer as some of your documents are set to be printed.");

				deliverable.PrinterDetails.PrintQueuePK = printQueue.PK;
				instructions.RunPreSaveValidation();
				Assert("should have no error if we set printer to document.", !instructions.PrinterDelivery.HasErrors);
			}
		}

		public void TestDefaultValues()
		{
			DocDeliveryPrintDetails printDetails = new DocDeliveryPrintDetails(Factory);
			AssertEquals("1 copy by default", 1, printDetails.NumberOfCopies);
		}

		public void TestPrintQueue()
		{
			StmPrintQueue printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_DisplayName = "Zubs Printer";
			printQueue.SQ_AllowPrinting = true;
			Factory.Save();

			DocDeliveryPrintDetails printDetails = new DocDeliveryPrintDetails(Factory);
			printDetails.PrintQueuePK = printQueue.PK;

			AssertEquals("Print Queue object loaded", printQueue, printDetails.PrintQueue);
		}

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

		public void TestPrinterNames()
		{
			StmPrintQueueCollection printQueues = new StmPrintQueueCollection(Factory);
			printQueues.Load();
			printQueues.RemoveAndDeleteAll();

			StmPrintQueue printQueue1 = CreateNewTestPrintQueue("Printer 1");
			StmPrintQueue printQueue2 = CreateNewTestPrintQueue("Printer 2");

			Env.Security.GetPrintQueueCheckPoint(printQueue2.PK.ToGuid(), printQueue2.SQ_DisplayName).IsAllowed = false;
			AssertEquals("Precondition: Print queue 2 denied", false, printQueue2.IsPrintAllowed);

			StmPrintQueue printQueue3 = CreateNewTestPrintQueue("Printer 3");

			Factory.Save();

			DocDeliveryPrintDetails printDetails = new DocDeliveryPrintDetails(Factory);
			printDetails.ShowOnlyPrintersUserCanPrintTo = false;

			AssertEquals("3 printers", 3, printDetails.PrinterNames.Count);
			AssertEquals("Correct printer name", "Printer 1", printDetails.PrinterNames[0].Code);
			AssertEquals("Correct printer name", System.Environment.MachineName, printDetails.PrinterNames[0].Description);

			AssertEquals("Correct printer name", "Printer 2 (No Rights)", printDetails.PrinterNames[1].Code);
			AssertEquals("Correct printer name", System.Environment.MachineName, printDetails.PrinterNames[1].Description);

			AssertEquals("Correct printer name", "Printer 3", printDetails.PrinterNames[2].Code);
			AssertEquals("Correct printer name", System.Environment.MachineName, printDetails.PrinterNames[2].Description);
		}

		StmPrintQueue CreateNewTestPrintQueue(string printQueueName)
		{
			StmPrintQueue queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_QueueName = printQueueName;
			queue.SQ_DisplayName = printQueueName;
			return queue;
		}

		public void TestPrinterNamesSortedAlphabetically()
		{
			StmPrintQueueCollection printQueues = new StmPrintQueueCollection(Factory);
			printQueues.Load();
			printQueues.RemoveAndDeleteAll();

			StmPrintQueue printQueue1 = CreateNewTestPrintQueue("Zubin's printer");
			StmPrintQueue printQueue2 = CreateNewTestPrintQueue("A printer in Sydney");
			StmPrintQueue printQueue3 = CreateNewTestPrintQueue("Bob's Printer");

			Factory.Save();

			DocDeliveryPrintDetails printDetails = new DocDeliveryPrintDetails(Factory);

			AssertEquals("Precondition: 3 printers", 3, printDetails.Printers.Count);
			AssertEquals(printQueue2, printDetails.Printers[0]);
			AssertEquals(printQueue3, printDetails.Printers[1]);
			AssertEquals(printQueue1, printDetails.Printers[2]);
		}

		public void TestShowOnlyPrintersUserCanPrintTo()
		{
			DocDeliveryPrintDetails printDetails = new DocDeliveryPrintDetails(Factory);
			AssertEquals(DocumentsDataRegistry.Instance.ShowOnlyPrintersUserCanPrintTo, printDetails.ShowOnlyPrintersUserCanPrintTo);

			StmPrintQueueCollection printQueues = new StmPrintQueueCollection(Factory);
			printQueues.Load();
			printQueues.RemoveAndDeleteAll();

			StmPrintQueue printQueue1 = CreateNewTestPrintQueue("Printer 1");

			StmPrintQueue printQueue2 = CreateNewTestPrintQueue("Printer 2 not Allowed");
			Env.Security.GetPrintQueueCheckPoint(printQueue2.PK.ToGuid(), printQueue2.SQ_DisplayName).IsAllowed = false;
			AssertEquals("Precondition: Print queue 2 denied", false, printQueue2.IsPrintAllowed);

			StmPrintQueue printQueue3 = CreateNewTestPrintQueue("Printer 3");

			Factory.Save();

			printDetails.ShowOnlyPrintersUserCanPrintTo = false;
			AssertEquals("3 printers", 3, printDetails.PrinterNames.Count);

			printDetails.ShowOnlyPrintersUserCanPrintTo = true;
			AssertEquals("2 printers", 2, printDetails.PrinterNames.Count);
		}

		public void TestNumberOfCopies()
		{
			var printDetails = new DocDeliveryPrintDetails(Factory);
			AssertNoErrors("No errors on number of copies by default", printDetails.NumberOfCopiesInfo);

			printDetails.NumberOfCopies = -13;
			AssertHasErrors("Errors on number of copies as it's less than 0", printDetails.NumberOfCopiesInfo);

			printDetails.NumberOfCopies = 0;
			AssertNoErrors("No errors on number of copies if 0", printDetails.NumberOfCopiesInfo);

			printDetails.NumberOfCopies = 10;
			AssertNoErrors("No errors on number of copies as number is greater than 10", printDetails.NumberOfCopiesInfo);

			printDetails.NumberOfCopies = 1000;
			AssertNoErrors("No errors on number of copies if 1000", printDetails.NumberOfCopiesInfo);

			printDetails.NumberOfCopies = 5000;
			AssertNoErrors("No errors on number of copies if 5000", printDetails.NumberOfCopiesInfo);

			printDetails.NumberOfCopies = 15000;
			AssertNoErrors("No errors on number of copies if 15000", printDetails.NumberOfCopiesInfo);

			printDetails.NumberOfCopies = 30000;
			AssertNoErrors("No errors on number of copies if 30000", printDetails.NumberOfCopiesInfo);

			printDetails.NumberOfCopies = 32767;
			AssertNoErrors("No errors on number of copies if 32767", printDetails.NumberOfCopiesInfo);

			printDetails.NumberOfCopies = 32768;
			AssertHasError(printDetails.NumberOfCopiesInfo, "Number of Printed Copies cannot be set to a number bigger than 32,767.");

			printDetails.NumberOfCopies = 50000;
			AssertHasError(printDetails.NumberOfCopiesInfo, "Number of Printed Copies cannot be set to a number bigger than 32,767.");
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocDeliveryPrintDetails(Factory);
		}

		#endregion
	}
}
