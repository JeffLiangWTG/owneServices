using System.ComponentModel;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B2JobComInvoiceLineViewCollection))]
	sealed class B2JobComInvoiceLineViewCollectionTest : InvoiceLineCollectionBOTest<B2JobComInvoiceLineViewCollection>
	{
		new JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.Invoice; }
		}

		public void TestDefaultForNewChild()
		{
			var invoiceLine = (JobComInvoiceLine)GetNewElementToAddToTheCollection();
			Assert(invoiceLine.CA_IsAccountForLine);
		}

		public void TestIsThisPartOfTheCollection()
		{
			Invoice.AsAccountForFilteredInvoiceLines.RemoveAndDeleteAll();
			Invoice.AsAccountForFilteredInvoiceLines.AddNew();
			var invoiceLine1 = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.CA_IsAccountForLine = true;
			AssertEquals(2, Invoice.AsAccountForFilteredInvoiceLines.Count);
			var invoiceLine2 = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_IsAccountForLine = false;
			AssertEquals(2, Invoice.AsAccountForFilteredInvoiceLines.Count);
		}

		public void TestCompare()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice = declaration.B2AsAccountedForInvoices.AddNew();
			var invoiceLines = invoice.AsAccountForFilteredInvoiceLines;

			CreateInvoiceLine(invoiceLines, "1");
			CreateInvoiceLine(invoiceLines, "1/SL");
			CreateInvoiceLine(invoiceLines, "11");
			CreateInvoiceLine(invoiceLines, "11/SL");
			CreateInvoiceLine(invoiceLines, "2");
			CreateInvoiceLine(invoiceLines, "2/SL");

			const string expectedText =
				"1\n" +
				"1/SL\n" +
				"2\n" +
				"2/SL\n" +
				"11\n" +
				"11/SL\n" +
				"";

			const string expectedTextInverted =
				"11/SL\n" +
				"11\n" +
				"2/SL\n" +
				"2\n" +
				"1/SL\n" +
				"1\n" +
				"";

			invoiceLines.Sort(JobComInvoiceLine.Schema.CA_OriginalLineNo, ListSortDirection.Ascending);
			AssertMultilineASCIIEquals("Normal", expectedText, OrderAsString(invoiceLines));

			invoiceLines.Sort(JobComInvoiceLine.Schema.CA_OriginalLineNo, ListSortDirection.Descending);
			AssertMultilineASCIIEquals("Inverted", expectedTextInverted, OrderAsString(invoiceLines));
		}

		public void TestRemoveAndDelete()
		{
			AssertRemoveAndDelete(JobMessageTypeList.Codes.B2Adjustments);
			AssertRemoveAndDelete(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertRemoveAndDelete(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var asAccountedInvoice = declaration.B2AsAccountedForInvoices.AddNew();
			asAccountedInvoice.JZ_InvoiceNumber = "INV1";
			var asAccountedInvoiceLine = asAccountedInvoice.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedInvoiceLine.CA_OriginalLineNo = "1";
			var asClaimedInvoice = asAccountedInvoice.CorrespondingAsClaimedForInvoice;
			var asClaimedInvoiceLine = asAccountedInvoiceLine.CorrespondingAsClaimedForInvoiceLine;
			var asClaimedInvoiceLine1 = asClaimedInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			asClaimedInvoiceLine1.JI_ParentID = asAccountedInvoiceLine.PK;
			asClaimedInvoiceLine1.JI_ParentTableCode = asAccountedInvoice.TablePrefix;
			var asClaimedInvoiceLine2 = asClaimedInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			asClaimedInvoiceLine2.JI_ParentID = asAccountedInvoiceLine.PK;
			asClaimedInvoiceLine2.JI_ParentTableCode = asAccountedInvoice.TablePrefix;

			var collection = asClaimedInvoice.AsClaimForFilteredInvoiceLines;
			collection.RemoveAndDelete(asClaimedInvoiceLine2);
			AssertEquals(2, collection.Count);

			collection.RemoveAndDelete(asClaimedInvoiceLine);
			AssertEquals(0, collection.Count);
		}

		public void TestAllowNew()
		{
			AssertAllowNew(JobMessageTypeList.Codes.B2Adjustments);
			AssertAllowNew(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertAllowNew(ZString messageType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var invoice = declaration.B2AsAccountedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			var claimedInvoice = invoice.CorrespondingAsClaimedForInvoice;
			invoice.CA_IsSeeded = true;
			Assert(!invoice.AsAccountForFilteredInvoiceLines.AllowNew);
			Assert(claimedInvoice.AsClaimForFilteredInvoiceLines.AllowNew);
			invoice.CA_IsSeeded = false;
			Assert(invoice.AsAccountForFilteredInvoiceLines.AllowNew);
			Assert(claimedInvoice.AsClaimForFilteredInvoiceLines.AllowNew);
		}

		void CreateInvoiceLine(B2JobComInvoiceLineViewCollection invoiceLines, string originalNo)
		{
			var line = invoiceLines.AddNew();
			line.CA_OriginalLineNo = originalNo;
		}

		string OrderAsString(B2JobComInvoiceLineViewCollection invoiceLines)
		{
			var builder = new StringBuilder();

			foreach (JobComInvoiceLine line in invoiceLines)
			{
				builder.AppendFormat("{0}\n", line.CA_OriginalLineNo);
			}

			return builder.ToString();
		}

		protected override B2JobComInvoiceLineViewCollection GetCollectionToTest()
		{
			return Invoice.AsAccountForFilteredInvoiceLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Invoice.AsAccountForFilteredInvoiceLines.AddNew();
		}
	}
}
