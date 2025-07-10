using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public abstract class InvoiceAndDueDateCalculatorTest : TestCaseWithFactory
	{
		public void TestShipmentDateWithoutConsolImport()
		{
			AssertEquals("Prerequisite", 0, TestShipment.Consols.Count);

			ZDateTime expectedDate = new ZDateTime(2004, 9, 13);

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			TestShipment.JS_E_ARV = expectedDate;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, new InvoiceTerm(InvoiceTermsList.FromShipmentDate.Code, "", 0));
			ZDateTime shipmentDate = testDateSetter.GetDateByInvoiceTerm();

			AssertEquals("Shipment Date is incorrect.", expectedDate, shipmentDate);
		}

		public void TestShipmentDateWithoutConsolExport()
		{
			AssertEquals("Prerequisite", 0, TestShipment.Consols.Count);

			ZDateTime expectedDate = new ZDateTime(2004, 9, 13);

			TestShipment.JS_RL_NKOrigin = CNSHA.Code;
			TestShipment.JS_RL_NKDestination = USLAX.Code;

			TestShipment.JS_E_DEP = expectedDate;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, new InvoiceTerm(InvoiceTermsList.FromShipmentDate.Code, "", 0));

			ZDateTime shipmentDate = testDateSetter.GetDateByInvoiceTerm();

			AssertEquals("Shipment Date is incorrect.", expectedDate, shipmentDate);
		}

		[TestDate(2004, 12, 15)]
		public void TestInvoiceDateAndDueDateIfShipmentDateIsInThePast()
		{
			AssertEquals("Prerequisite", 0, TestShipment.Consols.Count);

			var expectedShipmentDate = new ZDateTime(2004, 5, 15);
			var expectedInvoiceDate = ZDateTime.Today;
			var expectedDueDate = expectedInvoiceDate;

			var term = new InvoiceTerm("SHP", "", 21);

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			TestShipment.JS_E_ARV = expectedShipmentDate;

			var testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, term);

			AssertEquals(expectedShipmentDate, testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(expectedInvoiceDate, testDateSetter.InvoiceDate);
			AssertEquals(expectedDueDate, testDateSetter.DueDate);
		}

		[TestDate(2008, 04, 22)]
		public void TestInvoiceDateAndDueDateIfShipmentDateIsInTheFuture()
		{
			AssertEquals("Prerequisite", 0, TestShipment.Consols.Count);

			var term = new InvoiceTerm("SHP", "", 21);

			var expectedShipmentDate = new ZDateTime(2008, 12, 10);
			var expectedInvoiceDate = ZDateTime.Now;
			var expectedDueDate = new ZDateTime(2008, 12, 31);

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			TestShipment.JS_E_ARV = expectedShipmentDate;

			var testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, term);

			AssertEquals(expectedShipmentDate, testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(expectedInvoiceDate, testDateSetter.InvoiceDate);
			AssertEquals(expectedDueDate, testDateSetter.DueDate);
		}

		[TestDate(2004, 12, 15)]
		public void TestInvoiceDateWithInvalidShipmentDate()
		{
			AssertEquals("Prerequisite", 0, TestShipment.Consols.Count);

			InvoiceTerm term = new InvoiceTerm("SHP", "", 21);

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, term);

			AssertEquals(ZDateTime.Today, testDateSetter.InvoiceDate);
		}

		[ExpectNoExceptions]
		[TestDate(2004, 12, 15)]
		public void TestIShipmentIsNullthrowsNoException()
		{
			AssertEquals("Prerequisite", 0, TestShipment.Consols.Count);

			var expectedShipmentDate = new ZDateTime(2004, 5, 15);

			var term = new InvoiceTerm("SHP", "", 21);

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			TestShipment.JS_E_ARV = expectedShipmentDate;

			Factory.Save();

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, term);

			testDateSetter.GetDateByInvoiceTerm();
		}

		[TestDate(2010, 09, 01)]
		public void TestDueDateForMonthInvoiceCycleInvoiceTerm()
		{
			AssertEquals("Prerequisite", 0, TestShipment.Consols.Count);

			OrgHeader organisation = Factory.New<OrgHeader>();

			organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 3;
			organisation.CompanyData.LoadARTermForAllInvoiceTypes().ARTermsCycles.AddNew().P5_PaymentDay = 14;

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			TestShipment.JS_E_ARV = new ZDateTime(2010, 09, 21);

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, organisation, Invoice.JobType, Invoice.Direction, Invoice.TransportMode, Invoice.AH_GB, Invoice.AH_GE, LedgerTypes.AccountsReceivable, InvoiceTypesList.Codes.FinalInvoice);

			AssertEquals("Shipment Date is incorrect.", new ZDateTime(2011, 01, 14), testDateSetter.DueDate);
		}

		[TestDate(2015, 07, 28)]
		public void TestInvoiceDateWithCustomsClearanceDateInThePast()
		{
			var log = TestShipment.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CustomsCleared.Code;
				log.SL_EventTime = new ZDateTime(2011, 02, 02);
				log.SL_IsEstimate = false;
			}

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			Factory.Save();

			var expectedCUSDate = new ZDateTime(2011, 02, 02);
			var expectedInvoiceDate = ZDateTime.Today;
			var expectedDueDate = expectedInvoiceDate;

			var term = new InvoiceTerm("CUS", "", 21);

			var testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, term);

			AssertEquals(expectedCUSDate, testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(expectedInvoiceDate, testDateSetter.InvoiceDate);
			AssertEquals(expectedDueDate, testDateSetter.DueDate);
		}

		[TestDate(2015, 07, 28)]
		public void TestInvoiceDateWithCustomsClearanceDateInTheFuture()
		{
			var log = TestShipment.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CustomsCleared.Code;
				log.SL_EventTime = new ZDateTime(2015, 08, 10);
				log.SL_IsEstimate = true;
			}

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			Factory.Save();

			var expectedCUSDate = new ZDateTime(2015, 08, 10);
			var expectedInvoiceDate = ZDateTime.Now;
			var expectedDueDate = new ZDateTime(2015, 08, 30);

			var term = new InvoiceTerm("CUS", "", 20);

			var testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, term);

			AssertEquals(expectedCUSDate, testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(expectedInvoiceDate, testDateSetter.InvoiceDate);
			AssertEquals(expectedDueDate, testDateSetter.DueDate);
		}

		public void TestInvoiceDateWithCustomsClearanceDateInThePastAndDueDateInFuture()
		{
			var log = TestShipment.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CustomsCleared.Code;
				log.SL_EventTime = Env.Time.CurrentLocalDate.Date.AddDays(-7);
				log.SL_IsEstimate = false;
			}

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			Factory.Save();

			var expectedCUSDate = Env.Time.CurrentLocalDate.Date.AddDays(-7);
			var expectedInvoiceDate = Env.Time.CurrentLocalDate;
			var expectedDueDate = expectedCUSDate.AddDays(21);

			var term = new InvoiceTerm("CUS", "", 21);
			var testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate.Date, term);

			AssertEquals(expectedCUSDate, testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(expectedInvoiceDate, testDateSetter.InvoiceDate);
			AssertEquals(expectedDueDate, testDateSetter.DueDate);
		}

		[TestDate(2015, 07, 28)]
		public void TestInvoiceTermCUSFallbackToSHP()
		{
			var expectedSHPDate = new ZDateTime(2015, 08, 10);
			var expectedCUSDate = new ZDateTime(2015, 08, 15);
			var expectedDueDateSHP = new ZDateTime(2015, 08, 30);
			var expectedDueDateCUS = new ZDateTime(2015, 09, 04);

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;
			TestShipment.JS_E_ARV = expectedSHPDate;

			Factory.Save();

			var term = new InvoiceTerm("CUS", "", 20);

			var testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, term);

			AssertEquals("Should fallback to SHP date", ZDateTime.Now, testDateSetter.InvoiceDate);
			AssertEquals("SHP date + 20 days should be Due Date", expectedDueDateSHP, testDateSetter.DueDate);
			AssertContains("Mesage", "Invoice terms are CUS - 20 Days from Customs Clearance date. As there is no CLR event on this job, Invoice Term bases on Shipment date", testDateSetter.MessageForInvoiceTermOverriding);

			var log = TestShipment.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CustomsCleared.Code;
				log.SL_EventTime = new ZDateTime(2015, 08, 15);
				log.SL_IsEstimate = true;
			}

			Factory.Save();

			testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, term);
			AssertEquals("CUS date should be Invoice Date", ZDateTime.Now, testDateSetter.InvoiceDate);
			AssertEquals("CUS date + 20 days should be Due Date", expectedDueDateCUS, testDateSetter.DueDate);
		}

		public void TestCompanyDataIsNotCreatedIfDoesNotExist()
		{
			var orgHeader = Factory.New<OrgHeader>();
			AssertNull("Precondition", orgHeader.CompanyDataLoadOnly);

			var testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, orgHeader, null, "", "", ZGuid.Empty, ZGuid.Empty, "", "");
			AssertEquals("", testDateSetter.InvoiceTerm);
			AssertNull(orgHeader.CompanyDataLoadOnly);
		}

		[TestDate(2008, 04, 22)]
		public void TestInvoiceDateAndDueDateIfShipmentDateIsInTheFutureWhenInvoiceTermIsLSI()
		{
			AssertEquals("Prerequisite", 0, TestShipment.Consols.Count);

			var term = new InvoiceTerm("LSI", "", 21);

			var expectedShipmentDate = new ZDateTime(2008, 12, 10);
			var expectedInvoiceDate = ZDateTime.Now;
			var expectedDueDate = new ZDateTime(2008, 12, 31);

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			TestShipment.JS_E_ARV = expectedShipmentDate;

			var testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, term);

			AssertEquals(expectedShipmentDate, testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(expectedInvoiceDate, testDateSetter.InvoiceDate);
			AssertEquals(expectedDueDate, testDateSetter.DueDate);
		}

		#region Implementation

		protected RefUNLOCO CNSHA;
		protected RefUNLOCO USLAX;
		protected RefUNLOCO AUSYD;
		protected CommonShipment TestShipment;
		protected Job TestJob;
		protected ARInvoice Invoice;

		protected CommonShipment GetShipment(Job job)
		{
			CommonShipment shipmentToReturn = Factory.New(ShipmentType) as CommonShipment;
			job.JH_ParentID = shipmentToReturn.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			return shipmentToReturn;
		}

		protected override void SetUp()
		{
			base.SetUp();

			USLAX = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USLAX") as RefUNLOCO;
			CNSHA = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "CNSHA") as RefUNLOCO;
			AUSYD = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = CNSHA.Code;

			TestJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			TestShipment = GetShipment(TestJob);
			Invoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
		}

		protected abstract Type ShipmentType { get; }

		#endregion
	}
}
