using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EdecNotificationDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection_NullArgument()
	{
		AssertEquals("Argument == null", 0, EdecNotificationDataProvider.NewCollection(null).Count());
	}

	public void TestNewCollection()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine1);
			invoiceLine1.NotifyCustomsOffices.AddNew().CY_Data = "CH000001";
			invoiceLine1.NotifyCustomsOffices.AddNew().CY_Data = "CH000002";

			var messageBuilders = EdecNotificationDataProvider.NewCollection(entryLine);
			AssertEquals("Count", 2, messageBuilders.Count());
			AssertEquals("CH000001", true, messageBuilders.Any(n => n.NotificationCode == "CH000001"));
			AssertEquals("CH000002", true, messageBuilders.Any(n => n.NotificationCode == "CH000002"));
		});
	}
}
