using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExtendReExportDateMessageSendingObjectParent))]
	sealed class ExtendReExportDateMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new ExtendReExportDateMessageSendingObjectParent(declaration);
		}

		public void TestSendingObjectsCollection()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var objectParent1 = new ExtendReExportDateMessageSendingObjectParent(declaration1);
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);

			var declaration2 = Factory.New<JobDeclaration>();
			var entry = declaration2.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invoiceLine.JI_ScheduledReExportDate = new ZDateTime("2024-01-01");
			var objectParent2 = new ExtendReExportDateMessageSendingObjectParent(declaration2);
			AssertEquals(1, objectParent2.SendingObjectsCollection.Count);

			var declaration3 = Factory.New<JobDeclaration>();
			entry = declaration3.CustomsEntryHeaders.AddNew();
			entryLine = entry.MergedLines.AddNew();
			invoiceLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_ScheduledReExportDate = new ZDateTime("2024-01-01");
			var invoiceLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invoiceLine3.JI_ScheduledReExportDate = new ZDateTime("2024-01-02");

			var objectParent3 = new ExtendReExportDateMessageSendingObjectParent(declaration3);
			AssertEquals(3, objectParent3.SendingObjectsCollection.Count);
		}

		public void TestObjectsToSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invoiceLine.JI_ScheduledReExportDate = new ZDateTime("2024-01-01");
			var objectParent = new ExtendReExportDateMessageSendingObjectParent(declaration);
			AssertEquals(1, objectParent.ObjectsToSend.Count());
			AssertEquals(entry, objectParent.ObjectsToSend.Single().Header);
		}
	}
}
