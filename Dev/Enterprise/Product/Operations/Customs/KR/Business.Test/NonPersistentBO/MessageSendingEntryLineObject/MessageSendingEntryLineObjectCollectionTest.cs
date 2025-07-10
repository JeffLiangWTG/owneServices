using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(MessageSendingEntryLineObjectCollection))]
	public class MessageSendingEntryLineObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingEntryLineObjectCollection>
	{
		protected override MessageSendingEntryLineObjectCollection GetCollectionToTest()
		{
			return new MessageSendingEntryLineObjectCollection(Factory.New<CusEntryHeader>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MessageSendingEntryLineObject(Factory);
		}

		public void TestFilterEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();

			var invoice3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = declaration.JE_MessageType;
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_ValueForVAT = 1000m;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_ValueForVAT = 20000m;
			invoiceLine2.JI_CL = entryLine2.PK;

			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.CL_ValueForVAT = 30000000m;
			invoiceLine3.JI_CL = entryLine3.PK;

			var sendingObjects = new MessageSendingEntryLineObjectCollection(entry);
			sendingObjects.PopulateElementsFromMergedLines(x => true, ElectronicDocumentTypeList.Codes._5BA);
			AssertEquals(3, sendingObjects.Count);
			sendingObjects = new MessageSendingEntryLineObjectCollection(entry);
			sendingObjects.PopulateElementsFromMergedLines(x => x.CL_ValueForVAT >= 20000m, ElectronicDocumentTypeList.Codes._5BA);
			AssertEquals(2, sendingObjects.Count);
		}

		public void Test5FNEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = declaration.JE_MessageType;

			var sendingObjects = new MessageSendingEntryLineObjectCollection(entry);
			sendingObjects.PopulateElementsFrom5FNMessages(null);
			AssertEquals(0, sendingObjects.Count);

			var entryNum1 = entry.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum1.CE_EntryLineReference = "1";
			sendingObjects = new MessageSendingEntryLineObjectCollection(entry);
			sendingObjects.PopulateElementsFrom5FNMessages(null);
			AssertEquals(1, sendingObjects.Count);
			AssertEquals(0, sendingObjects[0].EntryLineNo);

			var entryNum2 = entry.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum2.CE_EntryLineReference = "2";
			var message = entry.Messages.AddNew();
			message.EM_MessageType = ElectronicDocumentTypeList.Codes._5FN;
			message.EM_ApplicationReference = "2";
			var fileReader = new TestFileReader(typeof(MessageSendingEntryLineObjectCollectionTest));
			var messageText1 = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing", "GOVCBR5FN_D1.xml");
			message.EM_MessageText = messageText1;
			sendingObjects = new MessageSendingEntryLineObjectCollection(entry);
			sendingObjects.PopulateElementsFrom5FNMessages(null);
			AssertEquals(2, sendingObjects.Count);
			AssertEquals(2, sendingObjects[1].EntryLineNo);
		}

		public void TestInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			var lineObjects = new MessageSendingEntryLineObjectCollection(entry);
			lineObjects.PopulateElementsFromMergedLines(x => true, ElectronicDocumentTypeList.Codes._5FN);
			AssertEquals(2, lineObjects.Count);

			AssertEquals(invoiceLine1, lineObjects.Cast<MessageSendingEntryLineObject>().FirstOrDefault(x => x.EntryLineNo == 1).InvoiceLine);
			AssertEquals(invoiceLine2, lineObjects.Cast<MessageSendingEntryLineObject>().FirstOrDefault(x => x.EntryLineNo == 2).InvoiceLine);
		}
	}
}
