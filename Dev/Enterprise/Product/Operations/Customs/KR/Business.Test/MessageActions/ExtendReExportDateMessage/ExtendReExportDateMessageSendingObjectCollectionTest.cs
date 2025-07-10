using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExtendReExportDateMessageSendingObjectCollection))]
	sealed class ExtendReExportDateMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExtendReExportDateMessageSendingObjectCollection>
	{
		protected override ExtendReExportDateMessageSendingObjectCollection GetCollectionToTest()
		{
			return new ExtendReExportDateMessageSendingObjectCollection(Declaration);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._D72;
			return new ExtendReExportDateMessageSendingObject(entry, ZDateTime.Empty);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}

		public void TestGetCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_ScheduledReExportDate = new ZDateTime(2024, 05, 10);
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_ScheduledReExportDate = new ZDateTime(2024, 03, 10);
			invoiceLine2.JI_CL = entryLine1.PK;
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_ScheduledReExportDate = ZDateTime.Empty;
			invoiceLine3.JI_CL = entryLine2.PK;

			var collection = new ExtendReExportDateMessageSendingObjectCollection(Declaration);
			AssertEquals(3, collection.Count);
			AssertEquals(new ZDateTime(2024, 05, 10), collection[0].CurrentReExportScheduledDate);
			AssertEquals(new ZDateTime(2024, 03, 10), collection[1].CurrentReExportScheduledDate);
			AssertEquals(ZDateTime.Empty, collection[2].CurrentReExportScheduledDate);
		}
	}
}
