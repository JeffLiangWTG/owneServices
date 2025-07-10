using System.Collections;
using System.Text;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAInvoiceLineComparerTest : TestCaseWithFactory
	{
		public void TestCompareCA_OriginalLineNo()
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
			CreateInvoiceLine(invoiceLines, "SL");

			const string expectedText =
				"1\n" +
				"1/SL\n" +
				"2\n" +
				"2/SL\n" +
				"11\n" +
				"11/SL\n" +
				"SL\n" +
				"";

			const string expectedTextInverted =
				"11/SL\n" +
				"11\n" +
				"2/SL\n" +
				"2\n" +
				"1/SL\n" +
				"1\n" +
				"SL\n" +
				"";

			invoiceLines.Sort((IComparer)new CAInvoiceLineComparer(false, JobComInvoiceLine.Schema.CA_OriginalLineNo));
			AssertMultilineASCIIEquals("Normal", expectedText, OrderAsString(invoiceLines));

			invoiceLines.Sort((IComparer)new CAInvoiceLineComparer(true, JobComInvoiceLine.Schema.CA_OriginalLineNo));
			AssertMultilineASCIIEquals("Inverted", expectedTextInverted, OrderAsString(invoiceLines));
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
	}
}
