using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(MessageSendingInvoiceLine))]
	sealed class MessageSendingInvoiceLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new MessageSendingInvoiceLine(Factory.New<JobComInvoiceLine>());

		[TestDate(2024, 01, 01)]
		public void TestInvoiceLines()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_SequenceNumber = 1;
			invoiceLine1.JI_ScheduledReExportDate = ZDateTime.Today;
			invoiceLine1.JI_Model = "ITEM DESCRIPTION1";
			invoiceLine1.JI_InvoiceQuantity = 10;
			invoiceLine1.JI_InvoiceUQ = "PL";
			invoiceLine1.JI_LinePrice = 100;
			invoiceLine1.CertificateOfOriginNo = "800324356051";
			invoiceLine1.CertificateOfOriginLineNo = 1;
			invoiceLine1.JI_CustomsFifthQuantity = 10m;
			invoiceLine1.CertificateOfOriginUQ = "EA";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "KRW";
			var invoiceLine2_1 = invoice2.InvoiceLines.AddNew();
			invoiceLine2_1.JI_SequenceNumber = 1;
			invoiceLine2_1.JI_ScheduledReExportDate = ZDateTime.Today;
			invoiceLine2_1.JI_Model = "ITEM DESCRIPTION2";
			invoiceLine2_1.JI_InvoiceQuantity = 20;
			invoiceLine2_1.JI_InvoiceUQ = "CT";
			invoiceLine2_1.JI_LinePrice = 200;
			invoiceLine2_1.CertificateOfOriginNo = "800324356052";
			invoiceLine2_1.CertificateOfOriginLineNo = 2;
			invoiceLine2_1.JI_CustomsFifthQuantity = 20m;
			invoiceLine2_1.CertificateOfOriginUQ = "KG";

			var invoiceLine2_2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2_2.JI_SequenceNumber = 2;
			invoiceLine2_2.JI_ScheduledReExportDate = ZDateTime.Today;
			invoiceLine2_2.JI_Model = "ITEM DESCRIPTION3";
			invoiceLine2_2.JI_InvoiceQuantity = 30;
			invoiceLine2_2.JI_InvoiceUQ = "BG";
			invoiceLine2_2.JI_LinePrice = 300;
			invoiceLine2_2.CertificateOfOriginNo = "800324356053";
			invoiceLine2_2.CertificateOfOriginLineNo = 3;
			invoiceLine2_2.JI_CustomsFifthQuantity = 30m;
			invoiceLine2_2.CertificateOfOriginUQ = "KG";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2_1.JI_CL = entryLine2.PK;
			invoiceLine2_2.JI_CL = entryLine2.PK;

			var messageSendingInvoiceLine = new MessageSendingInvoiceLine(invoiceLine1);
			AssertEquals(1u, messageSendingInvoiceLine.EntryLineNo);
			AssertEquals(1u, messageSendingInvoiceLine.InvoiceLineNo);
			AssertEquals("ITEM DESCRIPTION1", messageSendingInvoiceLine.ItemDescription);
			AssertEquals(10m, messageSendingInvoiceLine.Quantity);
			AssertEquals("PL", messageSendingInvoiceLine.UQ);
			AssertEquals(100m, messageSendingInvoiceLine.LinePrice);
			AssertEquals("USD", messageSendingInvoiceLine.AmountCurrency);
			AssertEquals("800324356051", messageSendingInvoiceLine.CertificateOfOriginNo);
			AssertEquals((ZShort)1, messageSendingInvoiceLine.CertificateOfOriginSeq);
			AssertEquals(10m, messageSendingInvoiceLine.CertificateOfOriginUsedQuantity);
			AssertEquals("EA", messageSendingInvoiceLine.CertificateOfOriginUsedUQ);

			messageSendingInvoiceLine = new MessageSendingInvoiceLine(invoiceLine2_1);
			AssertEquals(2u, messageSendingInvoiceLine.EntryLineNo);
			AssertEquals(1u, messageSendingInvoiceLine.InvoiceLineNo);
			AssertEquals("ITEM DESCRIPTION2", messageSendingInvoiceLine.ItemDescription);
			AssertEquals(20m, messageSendingInvoiceLine.Quantity);
			AssertEquals("CT", messageSendingInvoiceLine.UQ);
			AssertEquals(200m, messageSendingInvoiceLine.LinePrice);
			AssertEquals("KRW", messageSendingInvoiceLine.AmountCurrency);
			AssertEquals("800324356052", messageSendingInvoiceLine.CertificateOfOriginNo);
			AssertEquals((ZShort)2, messageSendingInvoiceLine.CertificateOfOriginSeq);
			AssertEquals(20m, messageSendingInvoiceLine.CertificateOfOriginUsedQuantity);
			AssertEquals("KG", messageSendingInvoiceLine.CertificateOfOriginUsedUQ);

			messageSendingInvoiceLine = new MessageSendingInvoiceLine(invoiceLine2_2);
			AssertEquals(2u, messageSendingInvoiceLine.EntryLineNo);
			AssertEquals(2u, messageSendingInvoiceLine.InvoiceLineNo);
			AssertEquals("ITEM DESCRIPTION3", messageSendingInvoiceLine.ItemDescription);
			AssertEquals(30m, messageSendingInvoiceLine.Quantity);
			AssertEquals("BG", messageSendingInvoiceLine.UQ);
			AssertEquals(300m, messageSendingInvoiceLine.LinePrice);
			AssertEquals("KRW", messageSendingInvoiceLine.AmountCurrency);
			AssertEquals("800324356053", messageSendingInvoiceLine.CertificateOfOriginNo);
			AssertEquals((ZShort)3m, messageSendingInvoiceLine.CertificateOfOriginSeq);
			AssertEquals(30m, messageSendingInvoiceLine.CertificateOfOriginUsedQuantity);
			AssertEquals("KG", messageSendingInvoiceLine.CertificateOfOriginUsedUQ);
		}
	}
}
