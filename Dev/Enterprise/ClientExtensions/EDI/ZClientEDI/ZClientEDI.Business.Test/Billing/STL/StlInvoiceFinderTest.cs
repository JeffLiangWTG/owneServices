using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class StlInvoiceFinderTest : TestCaseWithFactory
	{
		public void TestFindInvoice()
		{
			var finder = new StlInvoiceFinder();
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => finder.BuildInvoiceMap(null));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => finder.BuildInvoiceMap(Array.Empty<ARInvoice>()));
			AssertNull(finder.FindInvoice(default(FeeUsage)));
			AssertNull(finder.FindInvoice(default(UsageLine)));
			AssertNull(finder.InvoiceFactory);

			var invoice1 = Factory.New<ARInvoice>();
			var invoice1Line1 = invoice1.Lines.AddNew() as ARInvoiceLine;
			var invoice1Line2 = invoice1.Lines.AddNew() as ARInvoiceLine;

			var invoice2 = Factory.New<ARInvoice>();
			var invoice2Line1 = invoice2.Lines.AddNew() as ARInvoiceLine;
			var invoice2Line2 = invoice2.Lines.AddNew() as ARInvoiceLine;

			var billLine1 = CreateBillLine();
			var billLine2 = CreateBillLine();
			var billLine3 = CreateBillLine();
			var billLine4 = CreateBillLine();

			var usageLine1 = CreateUsageLine();
			var usageLine2 = CreateUsageLine();
			var usageLine3 = CreateUsageLine();
			var usageLine4 = CreateUsageLine();
			var usageLine5 = CreateUsageLine();
			var usageLine6 = CreateUsageLine();

			var feeUsage1 = CreateFeeUsage();
			var feeUsage2 = CreateFeeUsage();

			finder.AddBillLine(billLine1, new[] { usageLine1, usageLine2, usageLine3 });
			finder.AddBillLine(billLine2, new[] { usageLine4, usageLine5, usageLine6 });
			finder.AddBillLine(billLine3, feeUsage1);
			finder.AddBillLine(billLine4, feeUsage2);
			finder.AddBillLine(billLine1, invoice1Line1);
			finder.AddBillLine(billLine2, invoice2Line1);
			finder.AddBillLine(billLine3, invoice1Line2);
			finder.AddBillLine(billLine4, invoice2Line2);
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => finder.BuildInvoiceMap(null));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => finder.BuildInvoiceMap(Array.Empty<ARInvoice>()));
			finder.BuildInvoiceMap(new[] { invoice1, invoice2 });

			AssertEquals(invoice1.Factory, finder.InvoiceFactory);

			AssertEquals(invoice1, finder.FindInvoice(usageLine1));
			AssertEquals(invoice1, finder.FindInvoice(usageLine2));
			AssertEquals(invoice1, finder.FindInvoice(usageLine3));
			AssertEquals(invoice2, finder.FindInvoice(usageLine4));
			AssertEquals(invoice2, finder.FindInvoice(usageLine5));
			AssertEquals(invoice2, finder.FindInvoice(usageLine6));

			AssertEquals(invoice1, finder.FindInvoice(feeUsage1));
			AssertEquals(invoice2, finder.FindInvoice(feeUsage2));

			AssertEquals(invoice1, finder.FindInvoice(default(FeeUsage)));
			AssertEquals(invoice1, finder.FindInvoice(default(UsageLine)));

			AssertEquals(invoice1, finder.FindInvoice(CreateUsageLine()));
			AssertEquals(invoice1, finder.FindInvoice(CreateFeeUsage()));
		}

		ClientLicenceFee Fee;

		protected override void SetUp()
		{
			base.SetUp();
			Fee = Factory.New<ClientLicenceFee>();
		}

		SystemBill.BillLine CreateBillLine() => new SystemBill.BillLine(1, null, "AUD", "CC", "", 0, "STL");
		UsageLine CreateUsageLine() => new UsageLine(Factory);
		FeeUsage CreateFeeUsage() => new FeeUsage(Fee);
	}
}
