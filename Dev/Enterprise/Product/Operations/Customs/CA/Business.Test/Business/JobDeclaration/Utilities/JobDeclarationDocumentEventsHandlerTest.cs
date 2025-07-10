using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobDeclarationDocumentEventsHandlerTest : TestCaseWithFactory
	{
		public void TestCanHandleMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			var documentSupporter = new JobDeclarationDocumentSupporter(declaration);
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			var handler = new JobDeclarationDocumentEventsHandler(declaration);

			menuItem.SU_MenuName = "LVS Summary by Importer";
			AssertEquals("handler.CanHandleMenuItem(menuItem)", false, handler.CanHandleMenuItem(menuItem));

			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.LVSIdentifierDetailsDocument;
			AssertEquals("handler.CanHandleMenuItem(menuItem)", true, handler.CanHandleMenuItem(menuItem));
		}

		[TestDate(2011, 12, 14)]
		public void TestHandleLVSIdentifierDetailsDocumentPrinted()
		{
			var testDate1 = ZDateTime.Today.AddDays(-2);
			var testDate2 = ZDateTime.Today.AddDays(-1);
			var testDateToday = ZDateTime.Today;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.LVSIdentifierDetailsDocument;
			var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
			var docEvents = new DocumentEventsForTesting();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "Inv001";
			invoice1.CA_LVSLastPrintDate = testDate1;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "Inv002";
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.CA_LVSLastPrintDate = testDate2;
			invoice3.JZ_InvoiceNumber = "Inv003";
			var invoice4 = declaration.Invoices.AddNew();
			invoice4.JZ_InvoiceNumber = "Inv004";
			var invoice5 = declaration.Invoices.AddNew();
			invoice5.JZ_InvoiceNumber = "Inv005";
			var invoice6 = declaration.Invoices.AddNew();
			invoice6.JZ_InvoiceNumber = "Inv006";
			var invoicesToPrint = declaration.LinesToPrint;

			AssertEquals("Pre-condition: Invoice 1 should be printed flag", false, invoicesToPrint[0].ShouldBePrinted);
			AssertEquals("Pre-condition: Invoice 1 last printed date", testDate1, invoicesToPrint[0].LastPrintDate);
			AssertEquals("Pre-condition: Invoice 2 should be printed flag", true, invoicesToPrint[1].ShouldBePrinted);
			AssertEquals("Pre-condition: Invoice 3 should be printed flag", false, invoicesToPrint[2].ShouldBePrinted);
			AssertEquals("Pre-condition: Invoice 3 last printed date", testDate2, invoicesToPrint[2].LastPrintDate);
			AssertEquals("Pre-condition: Invoice 4 should be printed flag", true, invoicesToPrint[3].ShouldBePrinted);
			AssertEquals("Pre-condition: Invoice 5 should be printed flag", true, invoicesToPrint[4].ShouldBePrinted);
			AssertEquals("Pre-condition: Invoice 6 should be printed flag", true, invoicesToPrint[5].ShouldBePrinted);

			invoicesToPrint[3].ShouldBePrinted = false;
			invoicesToPrint[5].ShouldBePrinted = false;
			declaration.DocumentSupporter.Initialise(docEvents);
			docEvents.RaiseDocumentPrinted(declaration, eventArgs);

			AssertEquals("Invoice 1 last printed date should not have been updated", testDate1, invoicesToPrint[0].LastPrintDate);
			AssertEquals("Invoice 1", testDate1, invoice1.CA_LVSLastPrintDate);
			AssertEquals("Invoice 2 last printed date should be set to date printed (today)", testDateToday, invoicesToPrint[1].LastPrintDate);
			AssertEquals("Invoice 2", testDateToday, invoice2.CA_LVSLastPrintDate);
			AssertEquals("Invoice 3 last printed date", testDate2, invoicesToPrint[2].LastPrintDate);
			AssertEquals("Invoice 3", testDate2, invoice3.CA_LVSLastPrintDate);
			AssertEquals("Invoice 4 last printed date should not have been set as was not flagged to print", ZDateTime.Empty, invoicesToPrint[3].LastPrintDate);
			AssertEquals("Invoice 4", ZDateTime.Empty, invoice4.CA_LVSLastPrintDate);
			AssertEquals("Invoice 5 last printed date should be set to date printed (today)", testDateToday, invoicesToPrint[4].LastPrintDate);
			AssertEquals("Invoice 5", testDateToday, invoice5.CA_LVSLastPrintDate);
			AssertEquals("Invoice 6 last printed date should not have been set as was not flagged to print", ZDateTime.Empty, invoicesToPrint[5].LastPrintDate);
			AssertEquals("Invoice 6", ZDateTime.Empty, invoice6.CA_LVSLastPrintDate);

			invoicesToPrint[3].ShouldBePrinted = true;
			invoicesToPrint[5].ShouldBePrinted = true;
			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.UserCancelled, menuItem);
			docEvents.RaiseDocumentPrinted(declaration, eventArgs);

			AssertEquals("Print cancelled - Invoice 1 last printed date should not have been updated", testDate1, invoicesToPrint[0].LastPrintDate);
			AssertEquals("Invoice 1", testDate1, invoice1.CA_LVSLastPrintDate);
			AssertEquals("Print cancelled - Invoice 2 last printed date should not have been updated", testDateToday, invoicesToPrint[1].LastPrintDate);
			AssertEquals("Invoice 2", testDateToday, invoice2.CA_LVSLastPrintDate);
			AssertEquals("Print cancelled - Invoice 3 last printed date should not have been updated", testDate2, invoicesToPrint[2].LastPrintDate);
			AssertEquals("Invoice 3", testDate2, invoice3.CA_LVSLastPrintDate);
			AssertEquals("Print cancelled - Invoice 4 last printed date should not have been updated", ZDateTime.Empty, invoicesToPrint[3].LastPrintDate);
			AssertEquals("Invoice 4", ZDateTime.Empty, invoice4.CA_LVSLastPrintDate);
			AssertEquals("Print cancelled - Invoice 5 last printed date should not have been updated", testDateToday, invoicesToPrint[4].LastPrintDate);
			AssertEquals("Invoice 5", testDateToday, invoice5.CA_LVSLastPrintDate);
			AssertEquals("Print cancelled - Invoice 6 last printed date should not have been updated", ZDateTime.Empty, invoicesToPrint[5].LastPrintDate);
			AssertEquals("Invoice 6", ZDateTime.Empty, invoice6.CA_LVSLastPrintDate);

			invoicesToPrint[2].ShouldBePrinted = true;
			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.TakenFromContact, menuItem);
			docEvents.RaiseDocumentPrinted(declaration, eventArgs);
			AssertEquals("Invoice 1 last printed date should not have been updated", testDate1, invoicesToPrint[0].LastPrintDate);
			AssertEquals("Invoice 1", testDate1, invoice1.CA_LVSLastPrintDate);
			AssertEquals("Invoice 2 last printed date should not have been updated", testDateToday, invoicesToPrint[1].LastPrintDate);
			AssertEquals("Invoice 2", testDateToday, invoice2.CA_LVSLastPrintDate);
			AssertEquals("Invoice 3 last printed date should have been updated again with latest print date as selected to re-print", testDateToday, invoicesToPrint[2].LastPrintDate);
			AssertEquals("Invoice 3", testDateToday, invoice3.CA_LVSLastPrintDate);
			AssertEquals("Invoice 4 last printed date should now have been updated", testDateToday, invoicesToPrint[3].LastPrintDate);
			AssertEquals("Invoice 4", testDateToday, invoice4.CA_LVSLastPrintDate);
			AssertEquals("Invoice 5 last printed date should not have been updated", testDateToday, invoicesToPrint[4].LastPrintDate);
			AssertEquals("Invoice 5", testDateToday, invoice5.CA_LVSLastPrintDate);
			AssertEquals("Invoice 6 last printed date should now have been updated", testDateToday, invoicesToPrint[5].LastPrintDate);
			AssertEquals("Invoice 6", testDateToday, invoice6.CA_LVSLastPrintDate);
		}

		#region DocumentEventsForTesting

		class DocumentEventsForTesting : IDocumentEvents
		{
			public void RaiseDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				if (DocumentPrintRequested != null)
				{
					DocumentPrintRequested(sender, e);
				}
			}

			public void RaiseDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed?.Invoke(sender, e);
			}

			public void RaiseDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
			{
				if (DocumentPrePrinted != null)
				{
					DocumentPrePrinted(sender, e);
				}
			}

			public void RaiseDocumentPrinted(object sender, DocumentPrintedEventArgs e)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrinted(sender, e);
				}
			}

			public event DocumentCancelEventHandler DocumentPrintRequested;
			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public event DocumentPrintedEventHandler DocumentPrinted;
		}

		#endregion
	}
}
