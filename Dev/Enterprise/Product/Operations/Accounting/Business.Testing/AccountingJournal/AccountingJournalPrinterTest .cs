using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Moq;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class AccoutningJournalPrinterTest : TestCaseWithFactory
	{
		public void TestAccoutningJournalPrinterWhenNothingToPrint()
		{
			var loadJournal = new AccountingJournalPrinter.JournalLoader(() => { return Array.Empty<AccountingJournal>(); });
			var dummyPrinter = new DummyAccoutningJournalPrinter(loadJournal);

			var msg = dummyPrinter.CanPrint();
			dummyPrinter.PrintDocuments();
			AssertEquals("Menu name", "Accounting Journal", dummyPrinter.MenuName);
			AssertEquals("Number of documents to print", 0, dummyPrinter.NumberOfDocumentToPrint);
			AssertEquals("Message", "There is no accounting journal within the given selection criteria.", msg);
			AssertEquals("Number of dock packs", 0, dummyPrinter.DockPackCount_ForTestOnly);
		}

		public void TestAccoutningJournalPrinter()
		{
			var readonlyFactory = new ReadOnlyBusinessObjectFactory();
			var dummyAJ1 = new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), readonlyFactory);
			var dummyAJ2 = new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), readonlyFactory);
			var loadJournal = new AccountingJournalPrinter.JournalLoader(() => { return new AccountingJournal[] { dummyAJ1, dummyAJ2 }; });
			var dummyPrinter = new DummyAccoutningJournalPrinter(loadJournal);

			var msg = dummyPrinter.CanPrint();
			dummyPrinter.PrintDocuments();
			AssertEquals("Menu Name", "Accounting Journal", dummyPrinter.MenuName);
			AssertEquals("Number of documents to print", 2, dummyPrinter.NumberOfDocumentToPrint);
			AssertEquals("Message", string.Empty, msg);
			AssertEquals("Number of dock packs", 1, dummyPrinter.DockPackCount_ForTestOnly);
			AssertEquals("Number of transactions in the dock pack", 2, dummyPrinter.TransactionCountInDocPack_ForTestOnly);
		}

		public void TestJournalPrintSavePrinterQueueInDocumentDocumentDestination()
		{
			var printerQueue = Factory.New<StmPrintQueue>();
			printerQueue.SQ_ServerName = System.Environment.MachineName;
			printerQueue.SQ_QueueName = "Printer1";
			printerQueue.SQ_DisplayName = "Printer1";
			Factory.Save();

			var readonlyFactory = new ReadOnlyBusinessObjectFactory();
			var dummyAJ1 = new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), readonlyFactory);
			var dummyAJ2 = new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), readonlyFactory);
			var loadJournal = new AccountingJournalPrinter.JournalLoader(() => { return new AccountingJournal[] { dummyAJ1, dummyAJ2 }; });
			var dummyPrinter = new DummyAccoutningJournalPrinter(loadJournal);

			var instructions = new DeliveryInstructions { PrinterDelivery = { PrintQueuePK = printerQueue.PK } };
			dummyPrinter.DeliveryInstructions_ForTestOnly = instructions;

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();

			mockPrintTaskUIProvider
				.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(),
					It.IsAny<ISecurityCheckpoint>()))
				.Returns(true);

			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			{
				dummyPrinter.PrintDocuments();
			}

			AssertNotEquals("Should be using Document Menu PK", Guid.Empty, dummyPrinter.DocumentMenuPK_ForTestOnly);

			var stmMenuItem = Factory.Load<IStmMenuItem>(dummyPrinter.DocumentMenuPK_ForTestOnly);
			var defaultPrinter = StmDefaultPrinter.LoadDefaultPrinter(Factory, GlbStaff.CurrentUser, stmMenuItem);

			AssertNotNull("Should found saved default printer details", defaultPrinter);
			AssertEquals("Should found saved default printer details", printerQueue.PK, defaultPrinter.SDP_SQ_Printer);

			dummyPrinter = new DummyAccoutningJournalPrinter(loadJournal);

			instructions = new DeliveryInstructions();
			Assert(instructions.PrinterDelivery.PrintQueuePK.IsEmpty);
			dummyPrinter.DeliveryInstructions_ForTestOnly = instructions;

			mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockPrintTaskUIProvider
				.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(),
					It.IsAny<ISecurityCheckpoint>()))
				.Returns(true);

			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			{
				dummyPrinter.PrintDocuments();
			}

			AssertEquals("Should pick up saved Default Printer", printerQueue.PK, instructions.PrinterDelivery.PrintQueuePK);
		}

		public abstract void TestLoadJournal();

		protected TestObjectCreator Creator
		{
			get
			{
				if (creator == null)
				{
					creator = new TestObjectCreator(Factory);
				}
				return creator;
			}
		}
		TestObjectCreator creator;
	}

	public class AccoutningJournalWithHeaderPrinterTest : AccoutningJournalPrinterTest
	{
		public override void TestLoadJournal()
		{
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader2.PK.ToGuid());

			var job = Creator.CreateJob(Creator.LocalClient, 1.0m, Creator.Agent, 1.0m);

			//AP Invoice
			var apInvoice = Creator.CreateAPInvoice<APInvoice>("AP100001", Creator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, Creator.Creditor1);
			apInvoice.AH_AB = Creator.AUDBankAccount.PK;
			apInvoice.Lines.RemoveAndDeleteAll();
			var line = Creator.CreateAPInvoiceLine(apInvoice, job, Creator.CC1, Creator.AUD, 1.0m, "AP Line 001", 250m);
			line.AL_AG = Creator.GLHeader2.PK;
			Creator.CreateJobCharge(line, job, Creator.CC1);

			//AR Invoice
			var arInvoice = Creator.CreateARInvoice<ARInvoice>("AR100001", Creator.AUD, 1.0m, Creator.Debtor);
			arInvoice.AH_AB = Creator.AUDBankAccount2.PK;
			arInvoice.Lines.RemoveAndDeleteAll();
			var line2 = Creator.CreateARInvoiceLine(arInvoice, job, Creator.CC1, Creator.AUD, 1.0m, "AR Line 001", 250m);
			line2.AL_AG = Creator.GLHeader1.PK;
			Creator.CreateJobCharge(line2, job, Creator.CC1);

			//Direct Receipt
			var directReceipt = Creator.CreateDirectReceipt(ZDateTime.Today, 150m, 50m, 250m, 50m);
			directReceipt.AH_AB = Creator.AUDBankAccount2.PK;

			//Direct Payment
			var directPayment = Creator.CreateDirectPayment(ZDateTime.Today, 150m, 50m, 250m, 50m);
			directPayment.AH_AB = Creator.AUDBankAccount.PK;

			Factory.Save();

			var printer = new AccoutningJournalWithHeaderPrinter(new ZGuid[] { apInvoice.PK, directReceipt.PK }, null);
			AssertEquals("NumberOfDocumentToPrint", 2, printer.NumberOfDocumentToPrint);

			printer = new AccoutningJournalWithHeaderPrinter(new ZGuid[] { new ZGuid(Guid.NewGuid()) }, null);
			AssertEquals("NumberOfDocumentToPrint", 0, printer.NumberOfDocumentToPrint);
		}
	}

	public class AccoutningJournalWithoutHeaderPrinterTest : AccoutningJournalPrinterTest
	{
		public override void TestLoadJournal()
		{
			Assert(true);
		}
	}

	public class DummyAccoutningJournalPrinter : AccountingJournalPrinter
	{
		public DummyAccoutningJournalPrinter(JournalLoader loadJournal)
			: base(null)
		{
			this.loadJournal = loadJournal;
		}

		readonly JournalLoader loadJournal;

		public string MenuName
		{
			get { return menuName; }
		}

		protected override JournalLoader LoadJournal
		{
			get { return loadJournal; }
		}
	}
}

