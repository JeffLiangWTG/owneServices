using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Services.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AIRSValidationRunnerTest : TestCaseWithFactory
	{
		[TestDate(2016, 2, 2)]
		public void TestAIRSValidationAll()
		{
			var service = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
			var runner = new AIRSValidationRunner(declaration, service);
			var queriedLines = runner.AIRSValidationAll(false, new CancellationTokenSource());
			Assert(!queriedLines[0].InvoiceLinePKs.Contains(invoiceLine1.PK));
			Assert(queriedLines[0].InvoiceLinePKs.Contains(invoiceLine2.PK));
			Assert(queriedLines[0].InvoiceLinePKs.Contains(invoiceLine3.PK));
			Assert(queriedLines[0].InvoiceLinePKs.Contains(invoiceLine4.PK));
			Assert(queriedLines[0].InvoiceLinePKs.Contains(invoiceLine5.PK));
			runner.PopulateValidateRequirementResults(declaration, queriedLines, ZDateTime.Now);
			AssertEquals("OKA", declaration.CA_OGDStatus);
			AssertEquals(ZString.Empty, invoiceLine1.CA_OGDStatus);
			AssertEquals("OKA", invoiceLine2.CA_OGDStatus);
			AssertEquals("OKA", invoiceLine3.CA_OGDStatus);
			AssertEquals("OKA", invoiceLine4.CA_OGDStatus);
			AssertEquals("OKA", invoiceLine5.CA_OGDStatus);
			AssertEquals(0, invoiceLine1.Notes.FindByDescription("AIRS Validation Results").Length);
			AssertEquals("2016-02-02T00:00:00|No errors reported by CFIA", invoiceLine2.Notes.FindByDescription("AIRS Validation Results")[0].ST_NoteText);
			AssertEquals("2016-02-02T00:00:00|No errors reported by CFIA", invoiceLine3.Notes.FindByDescription("AIRS Validation Results")[0].ST_NoteText);
			AssertEquals("2016-02-02T00:00:00|No errors reported by CFIA", invoiceLine4.Notes.FindByDescription("AIRS Validation Results")[0].ST_NoteText);
			AssertEquals("2016-02-02T00:00:00|No errors reported by CFIA", invoiceLine5.Notes.FindByDescription("AIRS Validation Results")[0].ST_NoteText);
		}

		public void TestAIRSValidationNOT()
		{
			var service = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
			var runner = new AIRSValidationRunner(declaration, service);
			var queriedLines = runner.AIRSValidationNOT(false, new CancellationTokenSource());
			Assert(!queriedLines[0].InvoiceLinePKs.Contains(invoiceLine1.PK));
			Assert(!queriedLines[0].InvoiceLinePKs.Contains(invoiceLine2.PK));
			Assert(queriedLines[0].InvoiceLinePKs.Contains(invoiceLine3.PK));
			Assert(queriedLines[0].InvoiceLinePKs.Contains(invoiceLine4.PK));
			Assert(queriedLines[0].InvoiceLinePKs.Contains(invoiceLine5.PK));
		}

		[TestDate(2016, 2, 2)]
		public void TestPopulateValidateRequirementResults_Error()
		{
			var service = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
			var runner = new AIRSValidationRunner(declaration, service);
			var queriedLines = runner.AIRSValidationAll(false, new CancellationTokenSource());
			queriedLines[0].ValidationFaultMessage = "AIRS Validation Query Aborted";
			AssertEquals("AIRS Validation Query Aborted", runner.PopulateValidateRequirementResults(declaration, queriedLines, ZDateTime.Now));
			AssertEquals("UNK", declaration.CA_OGDStatus);
			AssertEquals("ERR", invoiceLine2.CA_OGDStatus);
			AssertEquals("2016-02-02T00:00:00|AIRS Validation Query Aborted", invoiceLine2.Notes.FindByDescription("AIRS Validation Results")[0].ST_NoteText);
		}

		[TestDate(2016, 2, 2)]
		public void TestPopulateValidateRequirementResults_Response()
		{
			var service = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
			var runner = new AIRSValidationRunner(declaration, service);

			var queriedLines = runner.AIRSValidationAll(false, new CancellationTokenSource());
			queriedLines[0].ValidationResponse = "COMMODITY CANNOT BE IMPORTED";
			runner.PopulateValidateRequirementResults(declaration, queriedLines, ZDateTime.Now);
			AssertEquals("NOI", declaration.CA_OGDStatus);
			AssertEquals("NOI", invoiceLine2.CA_OGDStatus);
			AssertEquals("2016-02-02T00:00:00|COMMODITY CANNOT BE IMPORTED", invoiceLine2.Notes.FindByDescription("AIRS Validation Results").OrderBy(n => n.ST_NoteText).Last().ST_NoteText);

			queriedLines[0].ValidationResponse = "COMMODITY REJECTED";
			runner.PopulateValidateRequirementResults(declaration, queriedLines, ZDateTime.Now.AddMinutes(1));
			AssertEquals("REJ", declaration.CA_OGDStatus);
			AssertEquals("REJ", invoiceLine2.CA_OGDStatus);
			AssertEquals("2016-02-02T00:01:00|COMMODITY REJECTED", invoiceLine2.Notes.FindByDescription("AIRS Validation Results").OrderBy(n => n.ST_NoteText).Last().ST_NoteText);

			queriedLines[0].ValidationResponse = "COMMODITY NEED REVIEW";
			runner.PopulateValidateRequirementResults(declaration, queriedLines, ZDateTime.Now.AddMinutes(2));
			AssertEquals("RVW", declaration.CA_OGDStatus);
			AssertEquals("RVW", invoiceLine2.CA_OGDStatus);
			AssertEquals("2016-02-02T00:02:00|COMMODITY NEED REVIEW", invoiceLine2.Notes.FindByDescription("AIRS Validation Results").OrderBy(n => n.ST_NoteText).Last().ST_NoteText);

			queriedLines[0].ValidationResponse = "COMMODITY INSPECT";
			runner.PopulateValidateRequirementResults(declaration, queriedLines, ZDateTime.Now.AddMinutes(3));
			AssertEquals("INS", declaration.CA_OGDStatus);
			AssertEquals("INS", invoiceLine2.CA_OGDStatus);
			AssertEquals("2016-02-02T00:03:00|COMMODITY INSPECT", invoiceLine2.Notes.FindByDescription("AIRS Validation Results").OrderBy(n => n.ST_NoteText).Last().ST_NoteText);

			queriedLines[0].ValidationResponse = "COMMODITY DO NOT REQUIRE";
			runner.PopulateValidateRequirementResults(declaration, queriedLines, ZDateTime.Now.AddMinutes(4));
			AssertEquals("OKA", declaration.CA_OGDStatus);
			AssertEquals("OKN", invoiceLine2.CA_OGDStatus);
			AssertEquals("2016-02-02T00:04:00|COMMODITY DO NOT REQUIRE", invoiceLine2.Notes.FindByDescription("AIRS Validation Results").OrderBy(n => n.ST_NoteText).Last().ST_NoteText);

			queriedLines[0].ValidationResponse = "COMMODITY APPROVED";
			runner.PopulateValidateRequirementResults(declaration, queriedLines, ZDateTime.Now.AddMinutes(5));
			AssertEquals("OKA", declaration.CA_OGDStatus);
			AssertEquals("OKA", invoiceLine2.CA_OGDStatus);
			AssertEquals("2016-02-02T00:05:00|COMMODITY APPROVED", invoiceLine2.Notes.FindByDescription("AIRS Validation Results").OrderBy(n => n.ST_NoteText).Last().ST_NoteText);

			queriedLines[0].ValidationResponse = "COMMODITY NOT REGULATED";
			runner.PopulateValidateRequirementResults(declaration, queriedLines, ZDateTime.Now.AddMinutes(6));
			AssertEquals("", declaration.CA_OGDStatus);
			AssertEquals("", invoiceLine2.CA_OGDStatus);
			AssertEquals("2016-02-02T00:06:00|COMMODITY NOT REGULATED", invoiceLine2.Notes.FindByDescription("AIRS Validation Results").OrderBy(n => n.ST_NoteText).Last().ST_NoteText);

			queriedLines[0].ValidationResponse = "COMMODITY";
			runner.PopulateValidateRequirementResults(declaration, queriedLines, ZDateTime.Now.AddMinutes(7));
			AssertEquals("REJ", declaration.CA_OGDStatus);
			AssertEquals("REJ", invoiceLine2.CA_OGDStatus);
			AssertEquals("2016-02-02T00:07:00|COMMODITY", invoiceLine2.Notes.FindByDescription("AIRS Validation Results").OrderBy(n => n.ST_NoteText).Last().ST_NoteText);

			AssertEquals(8, invoiceLine2.Notes.FindByDescription("AIRS Validation Results").Length);
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;
		JobComInvoiceLine invoiceLine3;
		JobComInvoiceLine invoiceLine4;
		JobComInvoiceLine invoiceLine5;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoiceLine2.CA_OGDStatus = AVSStatusList.Codes.Blank;
			invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoiceLine3.CA_OGDStatus = AVSStatusList.Codes.NotValidated;
			invoiceLine4 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoiceLine4.CA_OGDStatus = AVSStatusList.Codes.Error;
			invoiceLine5 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoiceLine5.CA_OGDStatus = AVSStatusList.Codes.Unknown;
			Factory.Save();
		}
	}
}
