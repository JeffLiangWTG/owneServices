using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRD72SenderTest : TestCaseWithFactory
	{
		IEnumerable<ExtendReExportDateMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBRD72SenderForTest(MessageSendingObjects, Factory) : new GOVCBRD72Sender(MessageSendingObjects, Factory);

		IEnumerable<ExtendReExportDateMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<ExtendReExportDateMessageSendingObject> parents;

		IEnumerable<ExtendReExportDateMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

					var entry1 = declaration.CustomsEntryHeaders.AddNew();
					var invoice1 = declaration.Invoices.AddNew();
					CreateEntryLineAndInvoiceLineData(entry1, "1", invoice1, 1, 1, "Invoice Line 1 in Entry Header 1", "Entry Line 1 in Entry Header 1", new ZDateTime(2024, 05, 30));
					CreateEntryLineAndInvoiceLineData(entry1, "2", invoice1, 2, 2, "Invoice Line 2 in Entry Header 1", "Entry Line 2 in Entry Header 1", new ZDateTime(2024, 06, 30));
					CreateEntryLineAndInvoiceLineData(entry1, "3", invoice1, 3, 3, "Invoice Line 3 in Entry Header 1", "Entry Line 3 in Entry Header 1", new ZDateTime(2024, 05, 30));

					var entry2 = declaration.CustomsEntryHeaders.AddNew();
					var invoice2 = declaration.Invoices.AddNew();
					CreateEntryLineAndInvoiceLineData(entry2, "1", invoice2, 1, 1, "Invoice Line 1 in Entry Header 2", "Entry Line 1 in Entry Header 2", new ZDateTime(2024, 05, 30));

					var sendingObjectParent = new ExtendReExportDateMessageSendingObjectParent(declaration);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<ExtendReExportDateMessageSendingObject>();

					foreach (var sendingObject in messageSendingObjects)
					{
						sendingObject.ShouldSend = true;
					}
				}
				return messageSendingObjects;
			}
		}

		void CreateEntryLineAndInvoiceLineData(CusEntryHeader entry, string entryLineReference, JobComInvoiceHeader invoice, short entryLineNo, short invoiceLineNo, string model, string description, ZDateTime scheduledReExportDate)
		{
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum.CE_EntryLineReference = entryLineReference;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = entryLineNo;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_LineNo = invoiceLineNo;
			invoiceLine.JI_Model = model;
			invoiceLine.JI_Description = description;
			invoiceLine.JI_ScheduledReExportDate = scheduledReExportDate;
		}

		IEnumerable<ExtendReExportDateMessageSendingObject> messageSendingObjects;

		public void TestVersionId()
		{
			var sender = (GOVCBRD72Sender)GetMessageSender();
			sender.Send();
			var sendingObjects = GetMessageParents();
			AssertEquals(3, sendingObjects.Count());
			AssertEquals(true, sendingObjects.ElementAt(0).Header.PK == sendingObjects.ElementAt(1).Header.PK);
			AssertEquals(false, sendingObjects.ElementAt(0).Header.PK == sendingObjects.ElementAt(2).Header.PK);
			AssertEquals(2, sendingObjects.ElementAt(0).Header.Messages.Count);
			AssertEquals(1, sendingObjects.ElementAt(2).Header.Messages.Count);

			var message1 = sendingObjects.ElementAt(0).Header.Messages.Cast<EDIMessage>().First();
			var message2 = sendingObjects.ElementAt(1).Header.Messages.Cast<EDIMessage>().ElementAt(1);
			AssertEquals("1", message1.EM_ApplicationReference);
			AssertContains("<VersionID>1</VersionID>", message1.EM_MessageInterpretation);
			AssertEquals("2", message2.EM_ApplicationReference);
			AssertContains("<VersionID>2</VersionID>", message2.EM_MessageInterpretation);

			var message3 = sendingObjects.ElementAt(2).Header.Messages.Cast<EDIMessage>().First();
			AssertEquals("1", message3.EM_ApplicationReference);
			AssertContains("<VersionID>1</VersionID>", message3.EM_MessageInterpretation);
		}

		public void TestSendingObjectsItems()
		{
			var sender = (GOVCBRD72Sender)GetMessageSender();
			sender.Send();
			var sendingObjects = GetMessageParents();

			AssertEquals(3, sendingObjects.Count());
			AssertEquals(new ZDateTime(2024, 05, 30), sendingObjects.ElementAt(0).CurrentReExportScheduledDate);
			AssertEquals(2, sendingObjects.ElementAt(0).D72EntryLines.Count);
			AssertEquals("Entry Line 1 in Entry Header 1", sendingObjects.ElementAt(0).D72EntryLines[0].ModelName);
			AssertEquals("Entry Line 3 in Entry Header 1", sendingObjects.ElementAt(0).D72EntryLines[1].ModelName);
			AssertEquals(2, sendingObjects.ElementAt(0).MessageSendingInvoiceLines.Count);
			AssertEquals("Invoice Line 1 in Entry Header 1", sendingObjects.ElementAt(0).MessageSendingInvoiceLines[0].ItemDescription);
			AssertEquals("Invoice Line 3 in Entry Header 1", sendingObjects.ElementAt(0).MessageSendingInvoiceLines[1].ItemDescription);

			AssertEquals(new ZDateTime(2024, 06, 30), sendingObjects.ElementAt(1).CurrentReExportScheduledDate);
			AssertEquals(1, sendingObjects.ElementAt(1).D72EntryLines.Count);
			AssertEquals("Entry Line 2 in Entry Header 1", sendingObjects.ElementAt(1).D72EntryLines[0].ModelName);
			AssertEquals(1, sendingObjects.ElementAt(1).MessageSendingInvoiceLines.Count);
			AssertEquals("Invoice Line 2 in Entry Header 1", sendingObjects.ElementAt(1).MessageSendingInvoiceLines[0].ItemDescription);

			AssertEquals(new ZDateTime(2024, 05, 30), sendingObjects.ElementAt(2).CurrentReExportScheduledDate);
			AssertEquals(1, sendingObjects.ElementAt(2).D72EntryLines.Count);
			AssertEquals("Entry Line 1 in Entry Header 2", sendingObjects.ElementAt(2).D72EntryLines[0].ModelName);
			AssertEquals(2, sendingObjects.ElementAt(0).MessageSendingInvoiceLines.Count);
			AssertEquals("Invoice Line 1 in Entry Header 2", sendingObjects.ElementAt(2).MessageSendingInvoiceLines[0].ItemDescription);
		}

		public void TestSendException()
		{
			IsExceptionTest = ZBool.True;
			var messages = Parents.FirstOrDefault().Header.Messages.Cast<EDIMessage>();
			AssertEquals("Two messages exits before sending a message.", 0, messages.Count());
			AssertExceptionThrown<Exception>(() => GetMessageSender().Send());
			AssertEquals("Exception occurred when sending a message, and no new message has been created.", 0, messages.Count());
		}
		public ZBool IsExceptionTest;

		class GOVCBRD72SenderForTest : GOVCBRD72Sender
		{
			public GOVCBRD72SenderForTest(IEnumerable<ExtendReExportDateMessageSendingObject> sendingObjects, BusinessObjectFactory factory) : base(sendingObjects, factory)
			{
			}

			protected override ImportD72Header GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new Exception();
		}
	}
}
