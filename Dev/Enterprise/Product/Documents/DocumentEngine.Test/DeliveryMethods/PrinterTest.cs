using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class PrinterTest : QueuedForBatchProcessorTest
	{
		public void TestSetAdditionalProperties()
		{
			var queueFactory = new BusinessObjectFactory();
			var queue = queueFactory.NewWithValidTestData<StmPrintQueue>();
			queue.SQ_DisplayName = "Test Print Queue";
			queue.SQ_ServerName = "Test";

			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.PrintQueue = queue;
			info.Copies = 5;
			var printDetails = new DocDeliveryPrintDetails(queueFactory);
			printDetails.NumberOfCopies = 2;
			var printer = new PrinterForTesting(printDetails);
			var job = Factory.New<StmPrintJob>();

			queueFactory.Save();

			printer.SetAdditionalPropertiesForTesting(job, info);

			AssertEquals(queue.PK, job.SP_SQ);
			AssertEquals((ZShort)10, job.SP_Copies);
		}

		public void TestEscapeSequenceSet()
		{
			BusinessObjectFactory queueFactory = new BusinessObjectFactory();
			StmPrintQueue queue = queueFactory.New(typeof(StmPrintQueue)) as StmPrintQueue;

			DeliveryInfo info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.TrailingSpace = 24;
			DocDeliveryPrintDetails printDetails = new DocDeliveryPrintDetails(queueFactory);
			printDetails.PrintQueuePK = queue.PK;
			PrinterForTesting printer = new PrinterForTesting(printDetails);
			StmPrintJob job = Factory.New(typeof(StmPrintJob)) as StmPrintJob;

			queue.SQ_PrintLanguage = "ESP";
			queueFactory.Save();
			printer.SetAdditionalPropertiesForTesting(job, info);
			AssertEquals("Epson sequence", new PrinterLanguages.EpsonEscP().PaperFeedSequence(24), job.SP_EscapeSequence);

			queue.SQ_PrintLanguage = "OML";
			queueFactory.Save();
			printer.SetAdditionalPropertiesForTesting(job, info);
			AssertEquals("OKI sequence", new PrinterLanguages.OkiMicroline().PaperFeedSequence(24), job.SP_EscapeSequence);

			queue.SQ_PrintLanguage = "N/A";
			queueFactory.Save();
			printer.SetAdditionalPropertiesForTesting(job, info);
			AssertEquals("None sequence", new PrinterLanguages.None().PaperFeedSequence(24), job.SP_EscapeSequence);
		}

		public void TestPrinterEmptyPrintQueue()
		{
			var factory = new BusinessObjectFactory();

			var printDetails = new DocDeliveryPrintDetails(factory);

			var instructions = new DeliveryInstructions();
			instructions.PrinterDelivery = printDetails;
			instructions.Destination = DeliveryInstructionDestination.Auto;

			var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			deliveryInfo.Name = "Some doc";
			deliveryInfo.TrailingSpace = 24;
			deliveryInfo.Instructions = instructions;

			var printer = new PrinterForTesting(printDetails);
			var printJob = Factory.New<StmPrintJob>();

			var message = "No Printer for [Some doc]. A printer must be specified when using a delivery method of 'PRN'. Destination [Auto]. Printer has not been set. Please close the form and reopen it again";
			AssertExceptionThrown<InvalidPrinterException>("", message,
				() => printer.SetAdditionalPropertiesForTesting(printJob, deliveryInfo));
		}

		public void TestPrinterInvalidPrintQueue()
		{
			var factory = new BusinessObjectFactory();

			var invalidPrinterPK = ZGuid.NewZGuid();

			var printDetails = new DocDeliveryPrintDetails(factory);
			printDetails.PrintQueuePK = invalidPrinterPK;

			var instructions = new DeliveryInstructions();
			instructions.PrinterDelivery = printDetails;
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;

			var recipient1 = instructions.Recipients.AddNew();
			recipient1.Name = "Fake Alex";
			recipient1.CompanyName = "Das Huhn Gmbh";
			recipient1.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

			var recipient2 = instructions.Recipients.AddNew();
			recipient2.Name = "Karczoch";
			recipient2.CompanyName = "Zumba Trading";
			recipient2.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;

			var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			deliveryInfo.Name = "Some doc";
			deliveryInfo.TrailingSpace = 24;
			deliveryInfo.Instructions = instructions;

			var printer = new PrinterForTesting(printDetails);
			var printJob = Factory.New<StmPrintJob>();

			var message = "No Printer for [Some doc]. A printer must be specified when using a delivery method of 'PRN'. Destination [TakenFromContact]. " +
				"Recipient: name [Fake Alex] company [Das Huhn Gmbh] delivery method [PRN]. Recipient: name [Karczoch] company [Zumba Trading] delivery method [FAX]. " +
				"Printer was requested but could not be loaded. Please close the form and reopen it again";

			AssertExceptionThrown<InvalidPrinterException>("", message,
				() => printer.SetAdditionalPropertiesForTesting(printJob, deliveryInfo));
		}

		public void TestNumberOfCopies()
		{
			BusinessObjectFactory queueFactory = new BusinessObjectFactory();
			StmPrintQueue queue = queueFactory.New(typeof(StmPrintQueue)) as StmPrintQueue;

			DeliveryInfo info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.Copies = 4;

			DocDeliveryPrintDetails printDetails = new DocDeliveryPrintDetails(queueFactory);
			printDetails.PrintQueuePK = queue.PK;
			printDetails.NumberOfCopies = 3;
			PrinterForTesting printer = new PrinterForTesting(printDetails);
			StmPrintJob job = Factory.New(typeof(StmPrintJob)) as StmPrintJob;

			printer.SetAdditionalPropertiesForTesting(job, info);
			AssertEquals("PrintJob.SP_Copies", (short)(4 * 3), job.SP_Copies);
			AssertEquals("PrintJob.SP_SQ", queue.PK, job.SP_SQ);
		}
	}
}
