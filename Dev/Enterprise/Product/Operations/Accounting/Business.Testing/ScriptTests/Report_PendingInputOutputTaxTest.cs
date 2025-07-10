

using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_PendingInputOutputTaxTest : ScriptTest
	{
		[TestDate(2014, 03, 11)]
		public void TestReport_PendingInputOutputTax()
		{
			#region Non-CashBasisVAT Invoice

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "APInv1", TestObjectCreator.AUD, 1m, 200m, 20m, 200m, 20m);

			#endregion

			#region CashBasisVAT Invoice with Match Links (Fully matched)

			var invoice2 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 300, 30);
			invoice2.AH_TransactionNum = "APInv2";
			var matchDate = invoice2.AH_PostDate.AddDays(3);
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice2, matchDate);
			Factory.Save();

			#endregion

			#region CashBasisVAT Invoice with Match Links (Not fully matched)

			var invoice3 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 100, 10);
			invoice3.AH_TransactionNum = "APInv3";
			matchDate = invoice3.AH_PostDate.AddDays(3);
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice3, matchDate, -40);
			Factory.Save();
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice3, matchDate, -20);
			Factory.Save();
			var glHeader = Factory.Load<AccGLHeader>(invoice3.Lines[0].AL_AG);
			glHeader.AG_AccountNum = "9999.99.99";
			Factory.Save();

			var invoice4 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 200, 20);
			invoice4.AH_TransactionNum = "APInv4";
			matchDate = invoice4.AH_PostDate.AddDays(200);
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice4, matchDate, -40);
			Factory.Save();
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice4, matchDate, -20);
			Factory.Save();
			glHeader = Factory.Load<AccGLHeader>(invoice4.Lines[0].AL_AG);
			glHeader.AG_AccountNum = "9999.99.99";
			Factory.Save();

			#endregion

			DataTable results = RunScript(201504);
			var expectedResult = @"
AH_Ledger    InOutput     HeaderBranch   AH_TransactionType  AH_TransactionNum   AL_PostDate           AgeInDays   AH_PostDate          AH_DueDate            AL_TaxDate		  AH_FullyPaidDate    AH_ComplianceSubType   AH_TransactionReference   AH_ConsolidatedInvoiceRef  JH_JobNum   JH_JobLocalReference   OH_Code     OH_FullName         LineBranch   AC_Code   AL_Desc         GoodsService   AG_AccountNum   AT_Code   AT_Description   AL_TaxRateNumerator AL_TaxRateDenominator	TaxRate	AL_TaxExtraRateNumerator AL_TaxExtraRateDenominator	ExtraTaxRate A9_Code  A9_Description  PostedLocalTotal   PostedAmount   PostedTax   PostedGST  PostedExtraTax   ReportedAmount   ReportedTax   ReportedGST   ReportedExtraTax   PendingAmount   PendingTax   PendingGST   PendingExtraTax
------------ ------------ -------------- ------------------- ------------------- --------------------- ----------- -------------------- --------------------- ------------------- ------------------- ---------------------- ------------------------- -------------------------- ----------- ---------------------- ----------- ------------------- ------------ --------- --------------- -------------- --------------- --------- ---------------- ------------------- --------------------- ------- ------------------------ -------------------------- ------------ -------- --------------- ------------------ -------------- ----------- ---------- ---------------- ---------------- ------------- ------------- ------------------ --------------- ------------ ------------ -----------------
AP           INPUT        BNE            INV                 APInv3              2014-03-11 00:00:00   121         2014-03-11 00:00:00  2014-03-11 00:00:00	  2014-03-11 00:00:00 NULL                                                                 00001002                   S00001001   00000002               ZOrg        Test Company Name   BNE          ZZCC1     Charge Code 1   SERVICE        9999.99.99      ZZGST1    GST Rate 1		  10				  1 				    10.00 	0 						 1 							0.00		 NULL     NULL            -110.00            -100.00        -10.00      -10.00     0.00             -54.54           -5.46         -5.46         0.00               -45.46          -4.54        -4.54        0.00
AP           INPUT        BNE            INV                 APInv4              2014-03-11 00:00:00   121         2014-03-11 00:00:00  2014-03-11 00:00:00	  2014-03-11 00:00:00 NULL                                                                 00001003                   S00001002   00000003               ZOrg        Test Company Name   BNE          ZZCC1     Charge Code 1   SERVICE        9999.99.99      ZZGST1    GST Rate 1		  10                  1                     10.00   0                        1                          0.00		 NULL     NULL            -220.00            -200.00        -20.00      -20.00     0.00             0.00             0.00          0.00          0.00               -200.00         -20.00       -20.00       0.00                                  
";
			AssertTableAsTextFromSQLServerManagenentStudio("PendingInputOutputTax", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			results = RunScript(new ZDateTime(2014, 3, 31));
			expectedResult = @"
AH_Ledger    InOutput     HeaderBranch   AH_TransactionType  AH_TransactionNum   AL_PostDate           AgeInDays   AH_PostDate          AH_DueDate            AL_TaxDate		  AH_FullyPaidDate    AH_ComplianceSubType   AH_TransactionReference   AH_ConsolidatedInvoiceRef  JH_JobNum   JH_JobLocalReference   OH_Code     OH_FullName         LineBranch   AC_Code   AL_Desc         GoodsService   AG_AccountNum   AT_Code   AT_Description   AL_TaxRateNumerator AL_TaxRateDenominator	TaxRate	AL_TaxExtraRateNumerator AL_TaxExtraRateDenominator	ExtraTaxRate A9_Code  A9_Description  PostedLocalTotal   PostedAmount   PostedTax   PostedGST  PostedExtraTax   ReportedAmount   ReportedTax   ReportedGST   ReportedExtraTax   PendingAmount   PendingTax   PendingGST   PendingExtraTax
------------ ------------ -------------- ------------------- ------------------- --------------------- ----------- -------------------- --------------------- ------------------- ------------------- ---------------------- ------------------------- -------------------------- ----------- ---------------------- ----------- ------------------- ------------ --------- --------------- -------------- --------------- --------- ---------------- ------------------- --------------------- ------- ------------------------ -------------------------- ------------ -------- --------------- ------------------ -------------- ----------- ---------- ---------------- ---------------- ------------- ------------- ------------------ --------------- ------------ ------------ -----------------
AP           INPUT        BNE            INV                 APInv3              2014-03-11 00:00:00   20         2014-03-11 00:00:00  2014-03-11 00:00:00	  2014-03-11 00:00:00 NULL                                                                 00001002                   S00001001   00000002               ZOrg        Test Company Name   BNE          ZZCC1     Charge Code 1   SERVICE        9999.99.99      ZZGST1    GST Rate 1		  10				  1 				    10.00 	0 						 1 							0.00		 NULL     NULL            -110.00            -100.00        -10.00      -10.00     0.00             -54.54           -5.46         -5.46         0.00               -45.46          -4.54        -4.54        0.00
AP           INPUT        BNE            INV                 APInv4              2014-03-11 00:00:00   20         2014-03-11 00:00:00  2014-03-11 00:00:00	  2014-03-11 00:00:00 NULL                                                                 00001003                   S00001002   00000003               ZOrg        Test Company Name   BNE          ZZCC1     Charge Code 1   SERVICE        9999.99.99      ZZGST1    GST Rate 1		  10                  1                     10.00   0                        1                          0.00		 NULL     NULL            -220.00            -200.00        -20.00      -20.00     0.00             0.00             0.00          0.00          0.00               -200.00         -20.00       -20.00       0.00                                  
";
			AssertTableAsTextFromSQLServerManagenentStudio("PendingInputOutputTax", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			results = RunScript(201512);
			expectedResult = @"
AH_Ledger    InOutput     HeaderBranch   AH_TransactionType  AH_TransactionNum   AL_PostDate           AgeInDays   AH_PostDate          AH_DueDate            AL_TaxDate		  AH_FullyPaidDate    AH_ComplianceSubType   AH_TransactionReference   AH_ConsolidatedInvoiceRef  JH_JobNum   JH_JobLocalReference   OH_Code     OH_FullName         LineBranch   AC_Code   AL_Desc         GoodsService   AG_AccountNum   AT_Code   AT_Description   AL_TaxRateNumerator AL_TaxRateDenominator	TaxRate	AL_TaxExtraRateNumerator AL_TaxExtraRateDenominator	ExtraTaxRate A9_Code  A9_Description  PostedLocalTotal   PostedAmount   PostedTax   PostedGST  PostedExtraTax   ReportedAmount   ReportedTax   ReportedGST   ReportedExtraTax   PendingAmount   PendingTax   PendingGST   PendingExtraTax
------------ ------------ -------------- ------------------- ------------------- --------------------- ----------- -------------------- --------------------- ------------------- ------------------- ---------------------- ------------------------- -------------------------- ----------- ---------------------- ----------- ------------------- ------------ --------- --------------- -------------- --------------- --------- ---------------- ------------------- --------------------- ------- ------------------------ -------------------------- ------------ -------- --------------- ------------------ -------------- ----------- ---------- ---------------- ---------------- ------------- ------------- ------------------ --------------- ------------ ------------ -----------------
AP           INPUT        BNE            INV                 APInv3              2014-03-11 00:00:00   364         2014-03-11 00:00:00  2014-03-11 00:00:00	  2014-03-11 00:00:00 NULL                                                                 00001002                   S00001001   00000002               ZOrg        Test Company Name   BNE          ZZCC1     Charge Code 1   SERVICE        9999.99.99      ZZGST1    GST Rate 1		  10				  1 				    10.00 	0 						 1 							0.00		 NULL     NULL            -110.00            -100.00        -10.00      -10.00     0.00             -54.54           -5.46         -5.46         0.00               -45.46          -4.54        -4.54        0.00
AP           INPUT        BNE            INV                 APInv4              2014-03-11 00:00:00   364         2014-03-11 00:00:00  2014-03-11 00:00:00	  2014-03-11 00:00:00 NULL                                                                 00001003                   S00001002   00000003               ZOrg        Test Company Name   BNE          ZZCC1     Charge Code 1   SERVICE        9999.99.99      ZZGST1    GST Rate 1		  10                  1                     10.00   0                        1                          0.00		 NULL     NULL            -220.00            -200.00        -20.00      -20.00     0.00             -54.54           -5.46         -5.46         0.00               -145.46         -14.54       -14.54       0.00                                  
";
			AssertTableAsTextFromSQLServerManagenentStudio("PendingInputOutputTax", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			results = RunScript(new ZDateTime(2015, 03, 11));
			expectedResult = @"
AH_Ledger    InOutput     HeaderBranch   AH_TransactionType  AH_TransactionNum   AL_PostDate           AgeInDays   AH_PostDate          AH_DueDate            AL_TaxDate		  AH_FullyPaidDate    AH_ComplianceSubType   AH_TransactionReference   AH_ConsolidatedInvoiceRef  JH_JobNum   JH_JobLocalReference   OH_Code     OH_FullName         LineBranch   AC_Code   AL_Desc         GoodsService   AG_AccountNum   AT_Code   AT_Description   AL_TaxRateNumerator AL_TaxRateDenominator	TaxRate	AL_TaxExtraRateNumerator AL_TaxExtraRateDenominator	ExtraTaxRate A9_Code  A9_Description  PostedLocalTotal   PostedAmount   PostedTax   PostedGST  PostedExtraTax   ReportedAmount   ReportedTax   ReportedGST   ReportedExtraTax   PendingAmount   PendingTax   PendingGST   PendingExtraTax
------------ ------------ -------------- ------------------- ------------------- --------------------- ----------- -------------------- --------------------- ------------------- ------------------- ---------------------- ------------------------- -------------------------- ----------- ---------------------- ----------- ------------------- ------------ --------- --------------- -------------- --------------- --------- ---------------- ------------------- --------------------- ------- ------------------------ -------------------------- ------------ -------- --------------- ------------------ -------------- ----------- ---------- ---------------- ---------------- ------------- ------------- ------------------ --------------- ------------ ------------ -----------------
AP           INPUT        BNE            INV                 APInv3              2014-03-11 00:00:00   365         2014-03-11 00:00:00  2014-03-11 00:00:00	  2014-03-11 00:00:00 NULL                                                                 00001002                   S00001001   00000002               ZOrg        Test Company Name   BNE          ZZCC1     Charge Code 1   SERVICE        9999.99.99      ZZGST1    GST Rate 1		  10				  1 				    10.00 	0 						 1 							0.00		 NULL     NULL            -110.00            -100.00        -10.00      -10.00     0.00             -54.54           -5.46         -5.46         0.00               -45.46          -4.54        -4.54        0.00
AP           INPUT        BNE            INV                 APInv4              2014-03-11 00:00:00   365         2014-03-11 00:00:00  2014-03-11 00:00:00	  2014-03-11 00:00:00 NULL                                                                 00001003                   S00001002   00000003               ZOrg        Test Company Name   BNE          ZZCC1     Charge Code 1   SERVICE        9999.99.99      ZZGST1    GST Rate 1		  10                  1                     10.00   0                        1                          0.00		 NULL     NULL            -220.00            -200.00        -20.00      -20.00     0.00             -54.54           -5.46         -5.46         0.00               -145.46         -14.54       -14.54       0.00                                  
";
			AssertTableAsTextFromSQLServerManagenentStudio("PendingInputOutputTax", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());
		}

		[TestDate(2014, 03, 11)]
		public void TestREV_WithCashVAT()
		{
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.REV.PK;
			var invoice1 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 150m, 0m);
			invoice1.AH_TransactionNum = "APInv";
			var matchDate = invoice1.AH_PostDate.AddDays(3);
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice1, matchDate, -100m);
			Factory.Save();

			var invoice2 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 300m, 0m);
			invoice2.AH_TransactionNum = "ARInv";
			matchDate = invoice2.AH_PostDate.AddDays(3);
			TestObjectCreator.CreateMatchLinkToPayARInvoice(invoice2, matchDate, 100m);
			Factory.Save();

			var headers = new[] { "AH_Ledger", "InOutput", "PostedTax", "PostedGST", "PostedExtraTax", "ReportedTax", "ReportedGST", "ReportedExtraTax", "PendingTax", "PendingGST", "PendingExtraTax" };
			var lines = new[]
			{
				new object[] { "AR", "OUTPUT", 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M },
				new object[] { "AP", "INPUT", -15M, -15M, 0M, -10M, -10M, 0M, -5M, -5M, 0M },
				new object[] { "AP", "OUTPUT", 15M, 15M, 0M, 10M, 10M, 0M, 5M, 5M, 0M }
			};
			var result = RunScript(201504);
			AssertDataTableAllRowsByKeyColumns("RVS Tax Type", result, headers, lines);
		}

		[TestDate(2014, 03, 11)]
		public void TestExtraTaxRate_WithCashVAT()
		{
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GSTANDQST1.PK;
			var invoice1 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 200m, 29.95m, true, TestObjectCreator.CC1);
			invoice1.AH_TransactionNum = "APInv";
			invoice1.Lines[0].AL_TaxRateNumerator = 5;
			invoice1.Lines[0].AL_TaxRateDenominator = 1;
			invoice1.Lines[0].AL_TaxExtraRateNumerator = 95;
			invoice1.Lines[0].AL_TaxExtraRateDenominator = 10;
			var matchDate = invoice1.AH_PostDate.AddDays(3);
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice1, matchDate, -100m);
			Factory.Save();

			var invoice2 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 200m, 29.95m, true, TestObjectCreator.CC1);
			invoice2.AH_TransactionNum = "ARInv";
			invoice2.Lines[0].AL_TaxRateNumerator = 5;
			invoice2.Lines[0].AL_TaxRateDenominator = 1;
			invoice2.Lines[0].AL_TaxExtraRateNumerator = 95;
			invoice2.Lines[0].AL_TaxExtraRateDenominator = 10;
			matchDate = invoice2.AH_PostDate.AddDays(3);
			TestObjectCreator.CreateMatchLinkToPayARInvoice(invoice2, matchDate, 100m);
			Factory.Save();
			AssertExtraTaxRate_WithCashVAT();
		}

		[TestDate(2014, 03, 11)]
		public void TestExtraTaxRate_WithCashVAT_ServiceTax()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				TestObjectCreator.GSTANDQST1.AT_Type = AccTaxRate.Types.ServiceTax;
				var chargeCode = TestObjectCreator.CC1;
				chargeCode.GSTRate.AT_Type = AccTaxRate.Types.ServiceTax;
				chargeCode.AC_AT_GSTRate = TestObjectCreator.GSTANDQST1.PK;
				Factory.Save();

				var invoice1 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 200m, 29.95m, true, chargeCode);
				invoice1.AH_TransactionNum = "APInv";
				var matchDate = invoice1.AH_PostDate.AddDays(3);
				TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice1, matchDate, -100m);
				Factory.Save();

				var invoice2 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 200m, 29.95m, true, chargeCode);
				invoice2.AH_TransactionNum = "ARInv";
				matchDate = invoice2.AH_PostDate.AddDays(3);
				TestObjectCreator.CreateMatchLinkToPayARInvoice(invoice2, matchDate, 100m);
				Factory.Save();

				AssertExtraTaxRate_WithCashVAT();
			}
		}

		[TestDate(2014, 03, 11)]
		public void TestExtraTaxRate_WithCashVAT_QCTTax()
		{
			var qctTax = TestObjectCreator.CreateTaxRate("QCT", "QCT", AccTaxRate.Types.Rated, 9, AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, 9, 1);
			var chargeCode = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, qctTax, TestObjectCreator.WHTFREE1);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 200m, 29.95m, true, chargeCode);
			invoice1.AH_TransactionNum = "APInv";
			var matchDate = invoice1.AH_PostDate.AddDays(3);
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice1, matchDate, -100m);
			Factory.Save();

			var invoice2 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 200m, 29.95m, true, chargeCode);
			invoice2.AH_TransactionNum = "ARInv";
			matchDate = invoice2.AH_PostDate.AddDays(3);
			TestObjectCreator.CreateMatchLinkToPayARInvoice(invoice2, matchDate, 100m);
			Factory.Save();

			AssertExtraTaxRate_WithCashForQCT();
		}

		[TestDate(2014, 03, 11)]
		public void TestExtraTaxRate_WithCashVAT_StateTax()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var stateTax = TestObjectCreator.CreateTaxRate("CGST", "CGST", AccTaxRate.Types.Rated, 9, AccTaxRate.ExtraTypes.StateGST, 9, 1);
				var chargeCode = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, stateTax, TestObjectCreator.WHTFREE1);
				Factory.Save();

				var invoice1 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 200m, 29.95m, true, chargeCode);
				invoice1.AH_TransactionNum = "APInv";
				var matchDate = invoice1.AH_PostDate.AddDays(3);
				TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice1, matchDate, -100m);
				Factory.Save();

				var invoice2 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 200m, 29.95m, true, chargeCode);
				invoice2.AH_TransactionNum = "ARInv";
				matchDate = invoice2.AH_PostDate.AddDays(3);
				TestObjectCreator.CreateMatchLinkToPayARInvoice(invoice2, matchDate, 100m);
				Factory.Save();

				AssertExtraTaxRate_WithCashForSTA();
			}
		}

		void AssertExtraTaxRate_WithCashVAT()
		{
			var headers = new[] { "AH_Ledger", "InOutput", "PostedTax", "PostedGST", "PostedExtraTax", "ReportedTax", "ReportedGST", "ReportedExtraTax", "PendingTax", "PendingGST", "PendingExtraTax" };
			var lines = new[]
			{
				new object[] { "AP", "INPUT", -29.95M, -10M, -19.95M, -13.02M, -4.35M, -8.67M, -16.93M, -5.65M, -11.28M },
				new object[] { "AR", "OUTPUT", 29.95M, 10M, 19.95M, 13.02M, 4.35M, 8.67M, 16.93M, 5.65M, 11.28M }
			};
			var result = RunScript(201504);
			AssertDataTableAllRowsByKeyColumns("Extra Tax Rate", result, headers, lines);
		}

		void AssertExtraTaxRate_WithCashForQCT()
		{
			var headers = new[] { "AH_Ledger", "InOutput", "PostedTax", "PostedGST", "PostedExtraTax", "ReportedTax", "ReportedGST", "ReportedExtraTax", "PendingTax", "PendingGST", "PendingExtraTax" };
			var lines = new[]
			{
				new object[] { "AP", "INPUT", -29.95M, -14.98M, -14.97M, -13.02M, -6.51M, -6.51M, -16.93M, -8.47M, -8.46M },
				new object[] { "AR", "OUTPUT", 29.95M, 14.98M, 14.97M, 13.02M, 6.51M, 6.51M, 16.93M, 8.47M, 8.46M }
			};
			var result = RunScript(201504);
			AssertDataTableAllRowsByKeyColumns("Extra Tax Rate", result, headers, lines);
		}

		void AssertExtraTaxRate_WithCashForSTA()
		{
			var headers = new[] { "AH_Ledger", "InOutput", "PostedTax", "PostedGST", "PostedExtraTax", "ReportedTax", "ReportedGST", "ReportedExtraTax", "PendingTax", "PendingGST", "PendingExtraTax" };
			var lines = new[]
			{
				new object[] { "AP", "INPUT", -29.95M, -14.97M, -14.98M, -13.02M, 1.96M, -14.98M, -16.93M, -16.93M, 0M },
				new object[] { "AR", "OUTPUT", 29.95M, 14.97M, 14.98M, 13.02M, -1.96M, 14.98M, 16.93M, 16.93M, 0M }
			};
			var result = RunScript(201504);
			AssertDataTableAllRowsByKeyColumns("Extra Tax Rate", result, headers, lines);
		}

		[TestDate(2014, 03, 11)]
		public void TestTaxAmount_WithDecimal()
		{
			var tax = TestObjectCreator.CreateTaxRate("REV", "ReverseRated", AccTaxRate.Types.ReverseRated, 11, 10, string.Empty, 0, 1);
			TestObjectCreator.CC1.AC_AT_GSTRate = tax.PK;
			var invoice1 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 150m, 0m);
			invoice1.AH_TransactionNum = "APInv";
			var matchDate = invoice1.AH_PostDate.AddDays(3);
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice1, matchDate, -100m);
			Factory.Save();

			var invoice2 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 300m, 0m);
			invoice2.AH_TransactionNum = "ARInv";
			matchDate = invoice2.AH_PostDate.AddDays(3);
			TestObjectCreator.CreateMatchLinkToPayARInvoice(invoice2, matchDate, 100m);
			Factory.Save();

			var result = RunScript(201504);

			var inputRow = result.Rows[1];
			AssertEquals("INPUT - PostedTax", -1.65m, inputRow["PostedTax"]);
			AssertEquals("INPUT - PostedGST", -1.65m, inputRow["PostedGST"]);

			var outputRow = result.Rows[2];
			AssertEquals("OUTPUT - PostedTax", 1.65m, outputRow["PostedTax"]);
			AssertEquals("INPUT - PostedGST", 1.65m, outputRow["PostedGST"]);
		}

		[TestDate(2024, 04, 11)]
		public void TestGoodsServiceForDirectPostedChargelines()
		{
			new AccountingPeriodTestHelper().SetupSinglePeriod(202404, new ZDateTime(2024, 04, 01), new ZDateTime(2024, 04, 30));
			var invoice1 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 200, 20);
			invoice1.AH_TransactionNum = "APInv1";
			var matchDate = invoice1.AH_PostDate.AddDays(200);
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice1, matchDate, -40);
			invoice1.Lines[0].AL_AC = Guid.Empty;

			var stateTax = TestObjectCreator.CreateTaxRate("CGST", "CGST", AccTaxRate.Types.Rated, 9, AccTaxRate.ExtraTypes.StateGST, 9, 1);
			var chargeCode = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, stateTax, TestObjectCreator.WHTFREE1);

			var invoice2 = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 200m, 29.95m, true, chargeCode);
			invoice2.AH_TransactionNum = "APInv2";
			matchDate = invoice2.AH_PostDate.AddDays(3);
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice2, matchDate, -100m);
			chargeCode.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;
			Factory.Save();

			var results = RunScript(202404);

			AssertEquals(2, results.Rows.Count);
			Assert("Precondition - For Directly Posted Lines , ChargeCode should be empty", string.IsNullOrEmpty(results.Rows[0]["AC_Code"].ToString()));

			AssertEquals("SERVICE", results.Rows[0]["GoodsService"]);
			AssertEquals("GOOD", results.Rows[1]["GoodsService"]);
		}

		DataTable RunScript(int period)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_PendingInputOutputTax
(
	'{0}'
	,{1}
	,NULL
	,'{2}'
	,NULL
	,NULL
	,NULL
	,NULL
)  
ORDER BY AH_TransactionNum
",
			GlbCompany.CurrentCompany.PK,
			period,
			"BOTH"
			));
		}

		DataTable RunScript(ZDateTime date)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_PendingInputOutputTax
(
	'{0}'
	,NULL
	,'{1}'
	,'{2}'
	,NULL
	,NULL
	,NULL
	,NULL
)  
",
			GlbCompany.CurrentCompany.PK,
			date.ToSmallDateTime().ToISO8601String(),
			"BOTH"
			));
		}
	}
}

