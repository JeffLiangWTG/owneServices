using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class QuarantineSequenceNumberGeneratorTest : TestCaseWithFactory
	{
		public void TestRecalculateAll_WithoutMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var invoiceLine4 = declaration.InvoiceLines.AddNew();

			using (invoice.GetLineNumberRenumberingSuspender())
			{
				invoiceLine2.Delete();
			}
			AssertEquals("Suspender should have worked and stop renumbering... Suspender is used for data transfer", (short)3, invoiceLine3.JI_LineNo);
			AssertEquals("Suspender should have worked and stop renumbering... Suspender is used for data transfer", (short)4, invoiceLine4.JI_LineNo);

			invoice.InvoiceLineLineNumberGenerator.ReCalculateAll();
			CombineAssertions("RecalculateAll Function should act as normal when messaging has not occured", () =>
			{
				AssertEquals((short)1, invoiceLine1.JI_LineNo);
				AssertEquals((short)2, invoiceLine3.JI_LineNo);
				AssertEquals((short)3, invoiceLine4.JI_LineNo);
			});
		}

		public void TestRecalculateAll_WithMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var invoiceLine4 = declaration.InvoiceLines.AddNew();

			DoMessaging(declaration);

			using (invoice.GetLineNumberRenumberingSuspender())
			{
				invoiceLine2.Delete();
			}

			invoice.InvoiceLineLineNumberGenerator.ReCalculateAll();
			CombineAssertions("RecalculateAll Function should do nothing once messaging has occured,", () =>
			{
				AssertEquals((short)1, invoiceLine1.JI_LineNo);
				AssertEquals((short)3, invoiceLine3.JI_LineNo);
				AssertEquals((short)4, invoiceLine4.JI_LineNo);
			});
		}

		public void TestRecalculateWhenAboutToBeDetachedOrDeleted_WithoutMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var invoiceLine4 = declaration.InvoiceLines.AddNew();

			invoiceLine2.Delete(); //We do not need to use a suspender, as it should recalculate during the deletion process

			CombineAssertions("RecalculateWhenAboutToBeDetachedOrDeleted Function should act as normal when messaging has not occured", () =>
			{
				AssertEquals((short)1, invoiceLine1.JI_LineNo);
				AssertEquals((short)2, invoiceLine3.JI_LineNo);
				AssertEquals((short)3, invoiceLine4.JI_LineNo);
			});
		}

		public void TestRecalculateWhenAboutToBeDetachedOrDeleted_WithMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var invoiceLine4 = declaration.InvoiceLines.AddNew();

			DoMessaging(declaration);

			invoiceLine2.Delete(); //We do not need to use a suspender, as it should recalculate during the deletion process

			CombineAssertions("RecalculateWhenAboutToBeDetachedOrDeleted Function should do nothing once messaging has occured,", () =>
			{
				AssertEquals((short)1, invoiceLine1.JI_LineNo);
				AssertEquals((short)3, invoiceLine3.JI_LineNo);
				AssertEquals((short)4, invoiceLine4.JI_LineNo);
			});
		}

		public void TestRecalculateWhenRenumbered()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			using (invoice.GetLineNumberRenumberingSuspender())
			{
				invoiceLine3.JI_LineNo = 10;
			}
			invoice.InvoiceLineLineNumberGenerator.RecalculateWhenRenumbered(invoiceLine3, 3);

			CombineAssertions("RecalculateWhenRenumbered Function should do nothing,", () =>
			{
				AssertEquals((short)1, invoiceLine1.JI_LineNo);
				AssertEquals((short)2, invoiceLine2.JI_LineNo);
				AssertEquals((short)10, invoiceLine3.JI_LineNo);
			});
		}

		public void TestRecalculateWhenAdded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			var invoiceLine5 = declaration.InvoiceLines.AddNew();

			DoMessaging(declaration);

			using (invoice.GetLineNumberRenumberingSuspender())
			{
				invoiceLine3.Delete();
				invoiceLine5.Delete();
			}

			var invoiceLine6 = declaration.InvoiceLines.AddNew();

			CombineAssertions("Should remember that we have previously submitted an invoice line of 5, and start counting from the highest value", () =>
			{
				AssertEquals((short)1, invoiceLine1.JI_LineNo);
				AssertEquals((short)2, invoiceLine2.JI_LineNo);
				AssertEquals((short)4, invoiceLine4.JI_LineNo);
				AssertEquals((short)6, invoiceLine6.JI_LineNo);
			});
		}

		public void TestComplexScenario()
		{
			//4 invoice lines are created, and sent to customs,
			//line 2 and 4 are deleted,
			//we add 3 more invoice lines,
			//original line 1 and 3 should keep their numbers,
			//lines 5,6,7 should have their respective line numbers (5,6,7)
			//delete line 6,
			//line 7 should now have a line number of 6,
			// --- Essentially testing the sequential counter on invoice lines that havent been sent, and enforcing them line no's on sent lines to be untouched ---

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var invoiceLine4 = declaration.InvoiceLines.AddNew();

			AssertEquals((short)1, invoiceLine1.JI_LineNo);
			AssertEquals((short)2, invoiceLine2.JI_LineNo);
			AssertEquals((short)3, invoiceLine3.JI_LineNo);
			AssertEquals((short)4, invoiceLine4.JI_LineNo);

			DoMessaging(declaration);

			invoiceLine2.Delete();
			invoiceLine4.Delete();

			AssertEquals((short)1, invoiceLine1.JI_LineNo);
			AssertEquals((short)3, invoiceLine3.JI_LineNo);

			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			var invoiceLine6 = declaration.InvoiceLines.AddNew();
			var invoiceLine7 = declaration.InvoiceLines.AddNew();

			AssertEquals((short)5, invoiceLine5.JI_LineNo);
			AssertEquals((short)6, invoiceLine6.JI_LineNo);
			AssertEquals((short)7, invoiceLine7.JI_LineNo);

			invoiceLine6.Delete();

			AssertEquals((short)5, invoiceLine5.JI_LineNo);
			AssertEquals((short)6, invoiceLine7.JI_LineNo);
		}

		void DoMessaging(JobDeclaration dec)
		{
			//Pre-condition for messaging
			dec.Invoices.ToList().ForEach(x => (x as JobComInvoiceHeader).QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs);

			//Sending message
			var sender = dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var rfpmmm = new RFPMultiMessageManager(dec, EXDOCMessageTypeCodes.Codes.ORD);
			rfpmmm.SendMessages(sender);

			//Mocking a successful response from customs.
			dec.JE_MessageStatus = RFPMessage.Status.Received;
			foreach (JobComInvoiceHeader header in dec.Invoices)
			{
				header.QuarantineExDocHeader.QH_RequestForPermitNumber = "10";
				var msg = header.QuarantineExDocHeader.Messages.AddNew();
				msg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				msg.EM_Status = EDIMessage.Status.Received;
				msg.EM_MessageType = EDIMessage.Status.Acknowledged;
			}
		}
	}
}
