using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class AvgDaysPaymentAnalysisTest : ScriptTest
	{
		protected override bool ShouldCreateTestPeriodsForToday => false;

		#region TestMatchlinks

		#region DBT

		#region CurrentDue

		[TestDate(2019, 3, 14)]
		public void TestMatchlinks_DBT_PartlyPaid_CurrentDue()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice("INV1", "DBT", 2);
			Factory.Save();
			PayInvoice(invoice, -5, 20);
			PayInvoice(invoice, 1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903", ZDateTime.Today);

			var expectedData = new (string name, object value)[]
			{
				("Balance", 80m),
				("TotalAmountCurrent", 80m),
				("TotalAmountPast", 0m),
				("TotalSTDOutstanding", 0m),
				("TotalSTDCurrent", 0m),
				("TotalSTDPast", 0m),
				("TotalDSBOutstanding", 80m),
				("TotalDSBCurrent", 80m),
				("TotalDSBPast", 0m),
				("TotalSTDPosted", 0m),
				("TotalDSBPosted", 100m),
			};

			AssertEquals("only 1 debtor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], expectedData);
		}

		[TestDate(2019, 3, 14)]
		public void TestMatchlinks_DBT_PaidBefore_CurrentDue()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice("INV1", "DBT", 2);
			Factory.Save();
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903", ZDateTime.Today);

			var expectedData = new (string name, object value)[]
			{
				("Balance", 0m),
				("TotalAmountCurrent", 0m),
				("TotalAmountPast", 0m),
				("TotalSTDOutstanding", 0m),
				("TotalSTDCurrent", 0m),
				("TotalSTDPast", 0m),
				("TotalDSBOutstanding", 0m),
				("TotalDSBCurrent", 0m),
				("TotalDSBPast", 0m),
				("TotalSTDPosted", 0m),
				("TotalDSBPosted", 100m),
			};

			AssertEquals("only 1 debtor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], expectedData);
		}

		#endregion

		#region PastDue

		[TestDate(2019, 3, 14)]
		public void TestMatchlinks_DBT_PartlyPaid_PastDue()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice("INV1", "DBT", -2);
			Factory.Save();
			PayInvoice(invoice, -5, 20);
			PayInvoice(invoice, 1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903", ZDateTime.Today);

			var expectedData = new (string name, object value)[]
			{
				("Balance", 80m),
				("TotalAmountCurrent", 0m),
				("TotalAmountPast", 80m),
				("TotalSTDOutstanding", 0m),
				("TotalSTDCurrent", 0m),
				("TotalSTDPast", 0m),
				("TotalDSBOutstanding", 80m),
				("TotalDSBCurrent", 0m),
				("TotalDSBPast", 80m),
				("TotalSTDPosted", null),
				("TotalDSBPosted", 100m),
			};

			AssertEquals("only 1 debtor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], expectedData);
		}

		[TestDate(2019, 3, 14)]
		public void TestMatchlinks_DBT_PaidBefore_PastDue()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice("INV1", "DBT", -2);
			Factory.Save();
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903", ZDateTime.Today);

			var expectedData = new (string name, object value)[]
			{
				("Balance", 0m),
				("TotalAmountCurrent", 0m),
				("TotalAmountPast", 0m),
				("TotalSTDOutstanding", 0m),
				("TotalSTDCurrent", 0m),
				("TotalSTDPast", 0m),
				("TotalDSBOutstanding", 0m),
				("TotalDSBCurrent", 0m),
				("TotalDSBPast", 0m),
				("TotalSTDPosted", 0m),
				("TotalDSBPosted", 100m),
			};

			AssertEquals("only 1 debtor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], expectedData);
		}

		[TestDate(2019, 3, 14)]
		public void TestMatchlinks_DBT_PaidAfter_PastDue()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice("INV1", "DBT", -2);
			Factory.Save();
			PayInvoice(invoice, 1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903", ZDateTime.Today);

			var expectedData = new (string name, object value)[]
			{
				("Balance", 100m),
				("TotalAmountCurrent", 0m),
				("TotalAmountPast", 100m),
				("TotalSTDOutstanding", 0m),
				("TotalSTDCurrent", 0m),
				("TotalSTDPast", 0m),
				("TotalDSBOutstanding", 100m),
				("TotalDSBCurrent", 0m),
				("TotalDSBPast", 100m),
				("TotalSTDPosted", 0m),
				("TotalDSBPosted", 100m),
			};

			AssertEquals("only 1 debtor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], expectedData);
		}

		#endregion

		#endregion

		#region STD

		#region CurrentDue

		[TestDate(2019, 3, 14)]
		public void TestMatchlinks_STD_PartlyPaid_CurrentDue()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice("INV1", "STD", 2);
			Factory.Save();
			PayInvoice(invoice, -5, 20);
			PayInvoice(invoice, 1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903", ZDateTime.Today);

			var expectedData = new (string name, object value)[]
			{
				("Balance", 80m),
				("TotalAmountCurrent", 80m),
				("TotalAmountPast", 0m),
				("TotalSTDOutstanding", 80m),
				("TotalSTDCurrent", 80m),
				("TotalSTDPast", 0m),
				("TotalDSBOutstanding", 0m),
				("TotalDSBCurrent", 0m),
				("TotalDSBPast", 0m),
				("TotalSTDPosted", 100m),
				("TotalDSBPosted", 0m),
			};

			AssertEquals("only 1 debtor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], expectedData);
		}

		[TestDate(2019, 3, 14)]
		public void TestMatchlinks_STD_PaidBefore_CurrentDue()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice("INV1", "STD", 2);
			Factory.Save();
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903", ZDateTime.Today);

			var expectedData = new (string name, object value)[]
			{
				("Balance", 0m),
				("TotalAmountCurrent", 0m),
				("TotalAmountPast", 0m),
				("TotalSTDOutstanding", 0m),
				("TotalSTDCurrent", 0m),
				("TotalSTDPast", 0m),
				("TotalDSBOutstanding", 0m),
				("TotalDSBCurrent", 0m),
				("TotalDSBPast", 0m),
				("TotalSTDPosted", 100m),
				("TotalDSBPosted", 0m),
			};

			AssertEquals("only 1 debtor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], expectedData);
		}

		#endregion

		#region PastDue

		[TestDate(2019, 3, 14)]
		public void TestMatchlinks_STD_PartlyPaid_PastDue()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice("INV1", "STD", -2);
			Factory.Save();
			PayInvoice(invoice, -5, 20);
			PayInvoice(invoice, 1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903", ZDateTime.Today);

			var expectedData = new (string name, object value)[]
			{
				("Balance", 80m),
				("TotalAmountCurrent", 0m),
				("TotalAmountPast", 80m),
				("TotalSTDOutstanding", 80m),
				("TotalSTDCurrent", 0m),
				("TotalSTDPast", 80m),
				("TotalDSBOutstanding", 0m),
				("TotalDSBCurrent", 0m),
				("TotalDSBPast", 0m),
				("TotalSTDPosted", 100m),
				("TotalDSBPosted", 0m),
			};

			AssertEquals("only 1 debtor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], expectedData);
		}

		[TestDate(2019, 3, 14)]
		public void TestMatchlinks_STD_PaidBefore_PastDue()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice("INV1", "STD", -2);
			Factory.Save();
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903", ZDateTime.Today);

			var expectedData = new (string name, object value)[]
			{
				("Balance", 0m),
				("TotalAmountCurrent", 0m),
				("TotalAmountPast", 0m),
				("TotalSTDOutstanding", 0m),
				("TotalSTDCurrent", 0m),
				("TotalSTDPast", 0m),
				("TotalDSBOutstanding", 0m),
				("TotalDSBCurrent", 0m),
				("TotalDSBPast", 0m),
				("TotalSTDPosted", 100m),
				("TotalDSBPosted", 0m),
			};

			AssertEquals("only 1 debtor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], expectedData);
		}

		[TestDate(2019, 3, 14)]
		public void TestMatchlinks_STD_PaidAfter_PastDue()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice("INV1", "STD", -2);
			Factory.Save();
			PayInvoice(invoice, 1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903", ZDateTime.Today);

			var expectedData = new (string name, object value)[]
			{
				("Balance", 100m),
				("TotalAmountCurrent", 0m),
				("TotalAmountPast", 100m),
				("TotalSTDOutstanding", 100m),
				("TotalSTDCurrent", 0m),
				("TotalSTDPast", 100m),
				("TotalDSBOutstanding", 0m),
				("TotalDSBCurrent", 0m),
				("TotalDSBPast", 0m),
				("TotalSTDPosted", 100m),
				("TotalDSBPosted", 0m),
			};

			AssertEquals("only 1 debtor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], expectedData);
		}

		#endregion 

		#endregion

		InvoicingBase CreateInvoice(string invoiceNum, string invoiceType, int daysToPayment)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceNum, organisation: TestObjectCreator.Debtor);
			invoice.AH_TransactionCategory = invoiceType;
			invoice.AH_PostDate = ZDateTime.Today.AddDays(-10);
			invoice.AH_DueDate = ZDateTime.Today.AddDays(daysToPayment);
			TestObjectCreator.CreateInvoiceLine(invoice, 100, setTaxes: false);

			return invoice;
		}

		void PayInvoice(InvoicingBase invoice, int daysPaid, ZDecimal? amountPaid = null)
		{
			TestObjectCreator.CreateAndMatchARReceiptForARInvoice(invoice, ZDateTime.Today.AddDays(daysPaid), amountPaid, postAndDueDate: ZDateTime.Today.AddDays(1));
			invoice.Factory.Save();
		}

		#endregion

		[TestDate(2020, 7, 24)]
		public void TestDSBPostedWithOtherTaxes()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2020, 01, 01));
			var invoice = CreateInvoice("INV1", 1000m, "DBT", ZDateTime.Today.AddDays(-2), false, 1);
			invoice.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			invoice.AH_LocalTaxAmountOtherTaxes = invoice.AH_OSTaxAmountOtherTaxes = 200m;

			AssertEquals("Precondition: GST Amount", 100m, invoice.AH_GSTAmount);
			Factory.Save();

			var resultTable = RunScript("202007", ZDate.Today);
			AssertDataTableAllRows("", resultTable, new[] { "TotalDSBPosted" }, new object[][] { new object[] { 1300m } });
		}

		[TestDate(2020, 7, 24)]
		public void TestSTDPostedWithOtherTaxes()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2020, 01, 01));
			var invoice = CreateInvoice("INV1", 1000m, "STD", ZDateTime.Today.AddDays(-2), false, 1);
			invoice.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			invoice.AH_LocalTaxAmountOtherTaxes = invoice.AH_OSTaxAmountOtherTaxes = 200m;

			AssertEquals("Precondition: GST Amount", 100m, invoice.AH_GSTAmount);
			Factory.Save();

			var resultTable = RunScript("202007", ZDate.Today);
			AssertDataTableAllRows("", resultTable, new[] { "TotalSTDPosted" }, new object[][] { new object[] { 1300m } });
		}

		[TestDate(2012, 3, 9)]
		public void TestAvgDaysPaymentAnalysis()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2012, 01, 01));

			ZDecimal originalCreditLimit = TestObjectCreator.ABIGAS.CompanyData.OB_ARCreditLimit;
			TestObjectCreator.ABIGAS.CompanyData.OB_ARCreditLimit = 100;

			CreateInvoice("INV1", 2, "DBT", new ZDateTime(2012, 02, 02), true, 4);
			CreateInvoice("INV2", 3, "DBT", new ZDateTime(2012, 02, 02), true, 6);
			CreateInvoice("INV3", 5, "STD", new ZDateTime(2012, 02, 02), true, 7);
			CreateInvoice("INV4", 7, "STD", new ZDateTime(2012, 02, 02), true, 9);

			CreateInvoice("INV5", 11, "DBT", new ZDateTime(2012, 02, 02), false, 14);
			CreateInvoice("INV6", 13, "DBT", new ZDateTime(2012, 02, 02), false, 13);
			CreateInvoice("INV7", 17, "DBT", new ZDateTime(2012, 02, 02), false, 15);

			CreateInvoice("INV5", 19, "STD", new ZDateTime(2012, 02, 02), false, 14);
			CreateInvoice("INV6", 23, "STD", new ZDateTime(2012, 02, 02), false, 13);
			CreateInvoice("INV7", 29, "STD", new ZDateTime(2012, 02, 02), false, 15);

			CreateInvoice("INV5", 31, "DBT", new ZDateTime(2012, 02, 02), false, 27);
			CreateInvoice("INV6", 37, "DBT", new ZDateTime(2012, 02, 02), false, 14);
			CreateInvoice("INV7", 41, "DBT", new ZDateTime(2012, 02, 02), false, 28);

			CreateInvoice("INV5", 43, "STD", new ZDateTime(2012, 02, 02), false, 27);
			CreateInvoice("INV6", 47, "STD", new ZDateTime(2012, 02, 02), false, 8);
			CreateInvoice("INV7", 53, "STD", new ZDateTime(2012, 02, 02), false, 28);

			Factory.Save();

			string[] columnNames =
					new string[] { "AccountCode",
				"AccountName",
				"OrgBranchCode",
				"OrgBranchName",
				"AccountGroup",
				"SalesRep",
				"SalesRepName",
				"CustomerService",
				"CustomerServiceName",
				"CreditController",
				"CreditControllerName",
				"SettlementCode",
				"SettlementName",
				"ARCategory",
				"ConsolidationCategory",
				"CreditLimit",
				"IsOverLimit",
				"InvoiceTerm",
				"Balance",
				"DSBTerm",
				"AverageDSBDaysToFullyPaid",
				"AverageSTDDaysToFullyPaid",
				"AmountExceedingCreditLimit",
				"TotalAmountCurrent",
				"TotalAmountPast",
				"TotalSTDOutstanding",
				"TotalSTDCurrent",
				"TotalSTDPast",
				"TotalDSBOutstanding",
				"TotalDSBCurrent",
				"TotalDSBPast",
				"AverageSTDDaysPastDue",
				"AverageDSBDaysPastDue",
				"OldestSTDDueDate",
				"OldestDSBDueDate",
				"TotalSTDPosted",
				"TotalDSBPosted",
				"OldestSTDPastDue",
				"OldestDSBPastDue" };

			object[] columnValues =
					new object[] { "ABIGAS      ",
				"ABI GAS & TOOLS",
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				"ABIGAS      ",
				null,
				"STD",
				"   ",
				100m,
				"Y",
				"COD",
				364m,
				"COD",
				5,
				8,
				264m,
				168m,
				196m,
				214m,
				96m,
				118m,
				150m,
				72m,
				78m,
				14,
				13,
				"10 Feb 2012",
				"15 Feb 2012",
				226m,
				155m,
				19,
				14 };

			DataTable resultTable = RunScript("201202", ZDateTime.Today);

			AssertEquals("only 1 debitor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], columnNames, columnValues);
		}

		[TestDate(2013, 6, 28)]
		public void TestAvgDaysPaymentAnalysisTerms()
		{
			// Invoice term, no dsb term, no settlement group
			DoTestAvgDaysPaymentAnalysisTerm("COD", 0, null, 0, null, 0, null, 0, "COD", "COD");
			DoTestAvgDaysPaymentAnalysisTerm("INV", 5, null, 0, null, 0, null, 0, "5/INV", "5/INV");
			DoTestAvgDaysPaymentAnalysisTerm("MTH", 5, null, 0, null, 0, null, 0, "5/MTH", "5/MTH");
			DoTestAvgDaysPaymentAnalysisTerm("PER", 5, null, 0, null, 0, null, 0, "5/PER", "5/PER");
			DoTestAvgDaysPaymentAnalysisTerm("SHP", 30, null, 0, null, 0, null, 0, "30/SHP", "30/SHP");
			DoTestAvgDaysPaymentAnalysisTerm("PIA", 0, null, 0, null, 0, null, 0, "PIA", "PIA");
			DoTestAvgDaysPaymentAnalysisTerm("MIC", 1, null, 0, null, 0, null, 0, "1/MIC", "1/MIC");

			// Invoice term, dsb term, no settlement group
			DoTestAvgDaysPaymentAnalysisTerm("MTH", 5, "COD", 0, null, 0, null, 0, "5/MTH", "5/MTH, (DSB)->COD");
			DoTestAvgDaysPaymentAnalysisTerm("MTH", 5, "INV", 5, null, 0, null, 0, "5/MTH", "5/MTH, (DSB)->5/INV");
			DoTestAvgDaysPaymentAnalysisTerm("MTH", 5, "MTH", 15, null, 0, null, 0, "5/MTH", "5/MTH, (DSB)->15/MTH");
			DoTestAvgDaysPaymentAnalysisTerm("MTH", 5, "PER", 5, null, 0, null, 0, "5/MTH", "5/MTH, (DSB)->5/PER");
			DoTestAvgDaysPaymentAnalysisTerm("MTH", 5, "SHP", 30, null, 0, null, 0, "5/MTH", "5/MTH, (DSB)->30/SHP");
			DoTestAvgDaysPaymentAnalysisTerm("MTH", 5, "PIA", 0, null, 0, null, 0, "5/MTH", "5/MTH, (DSB)->PIA");
			DoTestAvgDaysPaymentAnalysisTerm("MTH", 5, "MIC", 1, null, 0, null, 0, "5/MTH", "5/MTH, (DSB)->1/MIC");

			// Invoice term, no dsb term, settlement group
			DoTestAvgDaysPaymentAnalysisTerm("DEF", 0, null, 0, "PER", 5, null, 0, "5/PER", "5/PER");
			DoTestAvgDaysPaymentAnalysisTerm("INV", 5, null, 0, "PER", 5, null, 0, "5/INV", "5/INV");

			// Invoice term, dsb term, settlement group
			DoTestAvgDaysPaymentAnalysisTerm("DEF", 0, "DEF", 0, "PER", 5, "MIC", 1, "5/PER", "5/PER, (DSB)->1/MIC");
			DoTestAvgDaysPaymentAnalysisTerm("INV", 5, "DEF", 0, "PER", 5, "MIC", 1, "5/INV", "(DSB)->1/MIC, 5/INV");
			DoTestAvgDaysPaymentAnalysisTerm("DEF", 0, "INV", 6, "PER", 5, "MIC", 1, "5/PER", "5/PER, (DSB)->6/INV");
			DoTestAvgDaysPaymentAnalysisTerm("INV", 5, "INV", 6, "PER", 5, "MIC", 1, "5/INV", "5/INV, (DSB)->6/INV");
		}

		public void TestAvgDaysPaymentAnalysisTermsCreditLimit()
		{
			// This test doesn't use a TestDate attribute because we are testing date logic within SQL Server for temporary credit limit, and
			// setting that attribute doesn't change the date in SQL Server.

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 01, 01));
			CreateInvoice("INV1", 2, "DBT", ZDateTime.Today, true, 4);
			Factory.Save();

			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.ABIGAS.CompanyData, 99m, 0m);
			Factory.Save();
			var resultTable = RunScript(ZDateTime.Today.ToString("yyyyMM"), ZDateTime.Today);
			AssertEquals("only 1 debitor", 1, resultTable.Rows.Count);
			AssertEquals(99m, resultTable.Rows[0]["CreditLimit"]);
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.ABIGAS.CompanyData, 99m, 3m);
			Factory.Save();
			resultTable = RunScript(ZDateTime.Today.ToString("yyyyMM"), ZDateTime.Today);
			AssertEquals("only 1 debitor", 1, resultTable.Rows.Count);
			AssertEquals(102m, resultTable.Rows[0]["CreditLimit"]);
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(TestObjectCreator.ABIGAS.CompanyData, 0m, 17m);
			Factory.Save();
			resultTable = RunScript(ZDateTime.Today.ToString("yyyyMM"), ZDateTime.Today);
			AssertEquals("only 1 debitor", 1, resultTable.Rows.Count);
			AssertEquals(17m, resultTable.Rows[0]["CreditLimit"]);
		}

		void DoTestAvgDaysPaymentAnalysisTerm(string invoiceTerm, byte invoiceDays, string dsbTerm, byte dsbDays, string settlementGroupInvoiceTerm, byte settlementGroupInvoiceDays, string settlementGroupDsbTerm, byte settlementGroupDsbDays, string expectedInvoiceTermText, string expectedDSBTermText)
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2013, 06, 01));

			var companyData = TestObjectCreator.ABIGAS.CompanyData;
			companyData.OB_ARCreditLimit = 100;
			companyData.ARTerms.DeleteAll();
			companyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = invoiceTerm;
			companyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = invoiceDays;

			if (dsbTerm != null)
			{
				companyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = dsbTerm;
				companyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = dsbDays;
			}

			OrgCompanyData settlementCompanyData = null;
			if (settlementGroupInvoiceTerm != null)
			{
				TestObjectCreator.ABIGAS.ARSettlementGroupPK = TestObjectCreator.AALSHI.PK;
				settlementCompanyData = TestObjectCreator.ABIGAS.ARSettlementGroup.CompanyData;
				settlementCompanyData.ARTerms.DeleteAll();

				settlementCompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = settlementGroupInvoiceTerm;
				settlementCompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = settlementGroupInvoiceDays;

				if (settlementGroupDsbTerm != null)
				{
					settlementCompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = settlementGroupDsbTerm;
					settlementCompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = settlementGroupDsbDays;
				}
			}

			CreateInvoice("INV10", 2, "STD", new ZDateTime(2013, 06, 28), true, 4);

			// Only test valid scenarios
			companyData.RunPreSaveValidation();
			if (settlementCompanyData != null)
			{
				settlementCompanyData.RunPreSaveValidation();
			}

			Factory.Save();

			string[] columnNames = new string[] { "InvoiceTerm", "DSBTerm" };
			object[] columnValues = new string[] { expectedInvoiceTermText, expectedDSBTermText };

			var resultTable = RunScript("201306", ZDateTime.Today);

			AssertEquals("only 1 debitor", 1, resultTable.Rows.Count);
			AssertDataRow(resultTable.Rows[0], columnNames, columnValues);
		}

		#region Implementation

		InvoicingBase CreateInvoice(string invoiceNum, decimal amount, string invoiceType, ZDateTime postDate, bool paid, int daysToPayment)
		{
			InvoicingBase invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), invoiceNum, TestObjectCreator.AUD, 1M, amount, 0M, amount, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.GLHeader1.PK, postDate, postDate.AddDays(daysToPayment), postDate, false);

			if (paid)
			{
				TestObjectCreator.CreateAndMatchARReceiptForARInvoice((ARInvoice)invoice, postDate.AddDays(daysToPayment));
			}

			invoice.AH_TransactionCategory = invoiceType;
			invoice.AH_DueDate = postDate.AddDays(daysToPayment);
			return invoice;
		}

		DataTable RunScript(string period, ZDateTime reportDate)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
EXEC AvgDaysPaymentAnalysis 
'{0}',	--@Period 
'{1}',	--@Company 
'',		--@SettlementGroupList 
'',		--@OrgList
'',		--@OrgGroupList 
'',		--@ConsolidatedCategory 
'',		--@AccountsRelationShip 
'',		--@CreditRating 
'',		--@BranchList 
'',		--@OrgBranch 
'',		--@CurrencyList 
'',		--@SalesRepList 
'',		--@SalesRepRoll 
'{2}'	--@CurrentDateTime
",
			period,
			GlbCompany.CurrentCompany.PK,
			reportDate.ToShortDateString()
			));
		}

		#endregion
	}
}


