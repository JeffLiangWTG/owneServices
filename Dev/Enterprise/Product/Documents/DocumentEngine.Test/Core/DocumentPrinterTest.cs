using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Public.Testing
{
	sealed class DocumentPrinterTest : TestCaseWithFactory
	{
		public void TestPrinterSettings()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var documentPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Communication Report")).PK;
			var printer = Factory.NewWithValidTestData<StmPrintQueue>();

			var documentPrinter = new DocumentPrinter();
			documentPrinter.Print(documentPK, printer.PK, parent);
			AssertEquals("Parent Document Menu should be set.", documentPK, documentPrinter.LastPrintSetForTesting.ParentMenuCommand.PK);
			AssertEquals("Destination should be Print.", DeliveryInstructionDestination.Print, documentPrinter.LastDeliveryInstructionsForTesting.Destination);
			AssertEquals("Number of Copies should be one by default.", 1, documentPrinter.LastDeliveryInstructionsForTesting.PrinterDelivery.NumberOfCopies);
			AssertEquals("Printer should be set.", printer.PK, documentPrinter.LastDeliveryInstructionsForTesting.PrinterDelivery.PrintQueuePK);

			documentPrinter.Print(documentPK, printer.PK, parent, 4);
			AssertEquals("Number of Copies should be set to what was passed in.", 4, documentPrinter.LastDeliveryInstructionsForTesting.PrinterDelivery.NumberOfCopies);
		}

		public void TestShowNotification()
		{
			var documentPrinter1 = new DocumentPrinterForTest();
			AssertEquals(true, documentPrinter1.ShowNotificationForTesting);

			var documentPrinter2 = new DocumentPrinterForTest(false);
			AssertEquals(false, documentPrinter2.ShowNotificationForTesting);

			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var documentPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Communication Report")).PK;
			var printer = Factory.NewWithValidTestData<StmPrintQueue>();

			Factory.Save();

			var documentPrinter3 = new DocumentPrinter();
			documentPrinter3.Print(documentPK, printer.PK, parent);
			AssertEquals(PrintTaskUIProviderTypes.None, documentPrinter3.LastPrintSetForTesting.PrintTaskUIProviderType);

			var documentPrinter4 = new DocumentPrinter(false);
			documentPrinter4.Print(documentPK, printer.PK, parent);
			AssertEquals(PrintTaskUIProviderTypes.Unattended, documentPrinter4.LastPrintSetForTesting.PrintTaskUIProviderType);
		}

		public void TestPrint()
		{
			DocumentPrinterForTest documentPrinter = new DocumentPrinterForTest();
			DocumentSupportableForTest parent = new DocumentSupportableForTest(Factory.New<OrgHeader>());
			AssertPrint(documentPrinter, ZGuid.Empty, ZGuid.Empty, parent, false);

			ZGuid documentPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading")).PK;
			AssertPrint(documentPrinter, documentPK, ZGuid.Empty, parent, true);
		}

		void AssertPrint(DocumentPrinterForTest documentPrinter, ZGuid menuPK, ZGuid printer, IDocumentSupportable parent, bool expectedResult)
		{
			documentPrinter.HasRun = false;
			documentPrinter.Print(menuPK, printer, parent);
			Assert("Has run", documentPrinter.HasRun == expectedResult);
		}
	}
}
