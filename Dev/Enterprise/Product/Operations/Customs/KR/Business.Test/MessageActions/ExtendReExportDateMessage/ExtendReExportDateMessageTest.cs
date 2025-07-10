using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;
namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExtendReExportDateMessageSendingObject))]
	public class ExtendReExportDateMessageTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			return new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Today);
		}

		[TestDate(2024, 01, 01)]
		public void TestD72EntryLines()
		{
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();

			CreateInvoiceLine(CreateEntryLine(entry, "1", 1), 1, ZDateTime.Today);
			CreateInvoiceLine(CreateEntryLine(entry, "2", 2), 2, ZDateTime.Today);
			CreateInvoiceLine(CreateEntryLine(entry, "3", 3), 3, ZDateTime.Today.AddDays(1));
			entry.EntryNumbers.Where(x => x.CE_EntryLineReference == "3").FirstOrDefault().Delete();

			var messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Today);
			messageSendingObject.NewReExportDate = ZDateTime.Today;
			AssertEquals(2, messageSendingObject.D72EntryLines.Count);
		}

		[TestDate(2024, 01, 01)]
		public void TestMessageSendingInvoiceLines()
		{
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			var entryLine1 = CreateEntryLine(entry, "1", 1);
			CreateInvoiceLine(entryLine1, 1, ZDateTime.Today);
			CreateInvoiceLine(entryLine1, 2, ZDateTime.Today);

			var entryLine2 = CreateEntryLine(entry, "2", 2);
			CreateInvoiceLine(entryLine2, 3, ZDateTime.Today.AddDays(1));

			var entryLine3 = CreateEntryLine(entry, "3", 3);
			CreateInvoiceLine(entryLine3, 4, ZDateTime.Today);
			CreateInvoiceLine(entryLine3, 5, ZDateTime.Today);

			AssertEquals(5, entry.InvoiceLines.Count());

			var messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Today.AddDays(-1));
			AssertEquals(0, messageSendingObject.D72EntryLines.Count);
			AssertEquals(0, messageSendingObject.MessageSendingInvoiceLines.Count);

			messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Today);
			AssertEquals(2, messageSendingObject.D72EntryLines.Count);
			AssertEquals(4, messageSendingObject.MessageSendingInvoiceLines.Count);
			AssertEquals(1u, messageSendingObject.MessageSendingInvoiceLines[0].InvoiceLineNo);
			AssertEquals(2u, messageSendingObject.MessageSendingInvoiceLines[1].InvoiceLineNo);
			AssertEquals(4u, messageSendingObject.MessageSendingInvoiceLines[2].InvoiceLineNo);
			AssertEquals(5u, messageSendingObject.MessageSendingInvoiceLines[3].InvoiceLineNo);

			messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Today.AddDays(1));
			AssertEquals(1, messageSendingObject.D72EntryLines.Count);
			AssertEquals(1, messageSendingObject.MessageSendingInvoiceLines.Count);
			AssertEquals(3u, messageSendingObject.MessageSendingInvoiceLines[0].InvoiceLineNo);
		}

		CusEntryLine CreateEntryLine(CusEntryHeader entry, string entryLineReference, short entryLineNumber)
		{
			var entryNum5FN = entry.EntryNumbers.AddNew();
			entryNum5FN.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum5FN.CE_EntryLineReference = entryLineReference;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = entryLineNumber;

			return entryLine;
		}

		void CreateInvoiceLine(CusEntryLine entryLine, short sequenceNumber, ZDateTime scheduledReExportDate)
		{
			var invoiceLine = entryLine.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_SequenceNumber = sequenceNumber;
			invoiceLine.JI_ScheduledReExportDate = scheduledReExportDate;
		}
	}
}
