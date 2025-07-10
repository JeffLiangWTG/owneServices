using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Invoicing;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoicingLineCacheUtilityTest : TestCaseWithFactory
	{
		public void TestGenerateInvoicingLineCache()
		{
			var chargeCodePK1 = TestObjectCreator.CC1.PK;
			var chargeCodePK2 = TestObjectCreator.CC2.PK;
			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.Creditor1, 0m, null, 0m);
			var job2 = TestObjectCreator.CreateJob(TestObjectCreator.Creditor1, 0m, null, 0m);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("000001", TestObjectCreator.AUD, 1m, 10m, 0m, 0m, 10m, 0m, 0m, TestObjectCreator.Creditor1);

			var invoiceLineUSD1 = invoice.Lines[0];
			InitializeInvoiceLine(invoiceLineUSD1, Env.CurrentBranchPK, Env.CurrentDepartmentPK, chargeCodePK1, job1.PK, "USD", 0.5m);

			var invoiceLineRON1 = invoice.Lines.AddNew() as InvoicingLineBase;
			InitializeInvoiceLine(invoiceLineRON1, TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.NonCurrentDepartment.PK, chargeCodePK2, job1.PK, "RON", 0.1m);

			var invoicingLineCacheUtility = new InvoicingLineCacheUtility();
			invoicingLineCacheUtility.GenerateInvoicingLineCache(invoice.Lines.OfType<InvoicingLineBase>());
			AssertInvoiceLineCache(invoicingLineCacheUtility, invoiceLineUSD1);
			AssertInvoiceLineCache(invoicingLineCacheUtility, invoiceLineRON1);
		}

		void AssertInvoiceLineCache(IInvoicingLineCacheUtility invoicingLineCacheUtility, InvoicingLineBase originalInvoiceLine)
		{
			var invoiceLineCacheFromMap = invoicingLineCacheUtility.InvoicingLineCache[originalInvoiceLine.PK];
			var invoiceLineCacheFromList = invoicingLineCacheUtility.InvoicingLinePlainDataObjects.Single(x => x.PK == originalInvoiceLine.PK);
			AssertInvoiceLineEqual(invoiceLineCacheFromMap, invoiceLineCacheFromList);
			AssertInvoiceLineEqual(invoiceLineCacheFromMap, InvoicingLinePlainDataObject.Create(originalInvoiceLine));
		}

		void AssertInvoiceLineEqual(InvoicingLinePlainDataObject line1, InvoicingLinePlainDataObject line2)
		{
			AssertEquals(line1.PK, line2.PK);
			AssertEquals(line1.ChargeCodePK, line2.ChargeCodePK);
			AssertEquals(line1.BranchPK, line2.BranchPK);
			AssertEquals(line1.DepartmentPK, line2.DepartmentPK);
			AssertEquals(line1.JobPK, line2.JobPK);
			AssertEquals(line1.RelatedJobPK, line2.RelatedJobPK);
			AssertEquals(line1.Currrency, line2.Currrency);
			AssertEquals(line1.ExchangeRate, line2.ExchangeRate);
		}

		public void TestValidateInvoicingLineCache()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("000001", TestObjectCreator.AUD, 1m, 10m, 0m, 0m, 10m, 0m, 0m, TestObjectCreator.Creditor1);
			var chargeCodePK = TestObjectCreator.CC1.PK;
			var job = TestObjectCreator.CreateJob(TestObjectCreator.Creditor1, 0m, null, 0m);
			var invoiceLineUSD = invoice.Lines[0];
			InitializeInvoiceLine(invoiceLineUSD, Env.CurrentBranchPK, Env.CurrentDepartmentPK, chargeCodePK, job.PK, "USD", 0.5m);
			var invoiceLineRON = invoice.Lines.AddNew() as InvoicingLineBase;
			InitializeInvoiceLine(invoiceLineRON, TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.NonCurrentDepartment.PK, chargeCodePK, job.PK, "RON", 0.1m);

			var invoicingLineCacheUtility = new InvoicingLineCacheUtility();
			invoicingLineCacheUtility.GenerateInvoicingLineCache(invoice.Lines.OfType<InvoicingLineBase>());
			AssertNoExceptionThrown(() => invoicingLineCacheUtility.ValidateInvoicingLineCache(invoice.Lines.OfType<InvoicingLineBase>()));
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			InitializeInvoiceLine(invoiceLineRON, TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.NonCurrentDepartment.PK, chargeCodePK, job.PK, "USD", 0.1m);
			var exception = AssertExceptionThrown<ZCannotSaveException>(() => invoicingLineCacheUtility.ValidateInvoicingLineCache(invoice.Lines.OfType<InvoicingLineBase>()));
			AssertEquals("Invoicing Lines have been changed after the cache was already created.", ErrorReporter.LastMessageReported);
			AssertEquals("PostTransacrtionCacheStale", ErrorReporter.LastKeyReported);
			var expectMessage = @"The transaction line cache is out of date.
Please turn off the 'Accounting -> Enable Post Transaction Calculated Property Cache' registry option, then close and reopen the form to try again.";
			AssertEquals(expectMessage, exception.Message);
			AssertEquals("Cannot Save Invoice", exception.Heading);

			invoicingLineCacheUtility.GenerateInvoicingLineCache(invoice.Lines.OfType<InvoicingLineBase>());
			exception = AssertExceptionThrown<ZCannotSaveException>("Exception should throw again due to exception already thrown before.",() => invoicingLineCacheUtility.ValidateInvoicingLineCache(invoice.Lines.OfType<InvoicingLineBase>()));
			AssertEquals(expectMessage, exception.Message);
			AssertEquals("Cannot Save Invoice", exception.Heading);

			ErrorReporter.Clear();
		}

		void InitializeInvoiceLine(InvoicingLineBase invoiceLine, ZGuid branchPK, ZGuid departmentPK, ZGuid chargeCodePK, ZGuid jobPK, ZString currency, ZDecimal exRate)
		{
			invoiceLine.AL_JH = jobPK;
			invoiceLine.AL_GB = branchPK;
			invoiceLine.AL_GE = departmentPK;
			invoiceLine.AL_AC = chargeCodePK;
			invoiceLine.AL_RX_NKTransactionCurrency = currency;
			invoiceLine.AL_ExchangeRate = exRate;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
