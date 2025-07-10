using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueAllPaidAPInvoicesForPTRS2024))]
	class QueueAllPaidAPInvoicesForPTRS2024Test : QueueAPInvoiceForPTRS2024TestBase
	{
		protected override string ReportCode => "TCP";

		protected override string StoredProcedureName => "QueueAllPaidAPInvoicesForPTRS2024";

		public void TestWithMultipleInvoicesMultiplePaymentsInOneMatchGroup()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				var inv01 = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), -200m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate, category: "SBC");
				helper.InsertTransactionMatchLink(inv01, "M00001", testDateGroup.PaymentDate, -200m);

				var inv02 = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), -100m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate, category: "STD");
				helper.InsertTransactionMatchLink(inv02, "M00001", testDateGroup.PaymentDate, -100m);

				var crd01 = helper.InsertTransactionHeader("AP", "CRD", GetTransactionNum(), 120m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate, category: "STD");
				helper.InsertTransactionMatchLink(crd01, "M00001", testDateGroup.PaymentDate, 120m);

				var pay01 = helper.InsertTransactionHeader("AP", "PAY", GetTransactionNum(), 180m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate, category: "STD");
				helper.InsertTransactionMatchLink(pay01, "M00001", testDateGroup.PaymentDate, 180m);

				if (testDateGroup.IsInReportingPeriod)
				{
					assertionList.Add((testDateGroup.PaymentDate, inv01, "120.00|3", "ReportSubCode - Non Small Business Invoice fully paid 120.00"));
					assertionList.Add((testDateGroup.PaymentDate, inv02, "60.00|3", "ReportSubCode - Non Small Business Invoice fully paid 60.00"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestNonReportingOrganisation()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				//Creditor without PTR document
				var creditorPKNoPTR = CreateCreditorWithABN(hasPTRDocument: false);
				var inv11 = CreatePTRSInvoiceAndPaymentPairFullyPaid(creditorPKNoPTR, testDateGroup.InvoiceDate, testDateGroup.DueDate, -110m, "PAY", testDateGroup.PaymentDate, 1);

				//Creditor without PTR documenet but excluded from the reporting explicitly
				var creditorPKExcludedFromReporting = CreateCreditorWithABN(hasPTRDocument: true, excludedFromReport: true);
				CreatePTRSInvoiceAndPaymentPairFullyPaid(creditorPKExcludedFromReporting, testDateGroup.InvoiceDate, testDateGroup.DueDate, -210m, "PAY", testDateGroup.PaymentDate, 2);

				// Invoices for creditors having PTR document valid outside of Report period
				var creditorPKPTROutsideReportPeriod = CreateCreditorWithABN(hasPTRDocument: true, excludedFromReport: false, PreviousReportingFromDate, PreviousReportingToDate);

				var inv31 = CreatePTRSInvoiceAndPaymentPairFullyPaid(creditorPKPTROutsideReportPeriod, testDateGroup.InvoiceDate, testDateGroup.DueDate, -310m, "PAY", testDateGroup.PaymentDate, 2);

				if (testDateGroup.IsInReportingPeriod)
				{
					assertionList.Add((testDateGroup.PaymentDate, inv11, "110.00|1", "ReportSubCode - Non Small Business Invoice fully paid 110.00"));
					assertionList.Add((testDateGroup.PaymentDate, inv31, "310.00|1", "ReportSubCode - Non Small Business Invoice fully paid 310.00"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestCashPaymentsOnly()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				var invoices = CreateSinglePaymentTypeVarieties(testDateGroup, 100m, "PAY");
				if (testDateGroup.IsInReportingPeriod)
				{
					assertionList.Add((testDateGroup.PaymentDate, invoices[0], "100.00|3", "ReportSubCode - 100.00 Small Business Fully Paid"));
					assertionList.Add((testDateGroup.PaymentDate, invoices[1], "100.00|2", "ReportSubCode - 100.00 Small Business Partially Paid"));
					assertionList.Add((testDateGroup.PaymentDate, invoices[2], "100.00|1", "ReportSubCode - 100.00 Non-Small Business Fully Paid"));
					assertionList.Add((testDateGroup.PaymentDate, invoices[3], "100.00|0", "ReportSubCode - 100.00 Non-Small Business Partially Paid"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestCreditNotesPaymentsOnly()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				CreateSinglePaymentTypeVarieties(testDateGroup, 100m, "CRD");
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestAdvancedPaymentOnly()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				CreateFullCashAdvancePayment(creditorPKSmallBusinessInReportingPeriod, -110m, 110m, testDateGroup.PaymentDate, 1);
				//Each advance payment contains two matching groups therefore the matchGroupNumber skips one
				CreateFullCashAdvancePayment(creditorPKNonSmallBusiness, -120m, 120m, testDateGroup.PaymentDate, 3);
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestSingleARInvoicePayAgainstAPInvoice()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				CreateFullyPaidARInvoicePayingAPInvoice(testDateGroup, creditorPKSmallBusinessInReportingPeriod, -100m, 1);
				CreateFullyPaidARInvoicePayingAPInvoice(testDateGroup, creditorPKNonSmallBusiness, -110m, 1);
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestARInvoiceAndCashPaymentCombined()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				var inv01 = CreateFullyPaidARInvoiceAndCashPaymentCombined(testDateGroup, creditorPKSmallBusinessInReportingPeriod, -500m, 300m, 1);
				var inv02 = CreateFullyPaidARInvoiceAndCashPaymentCombined(testDateGroup, creditorPKNonSmallBusiness, -500m, 300m, 1);
				if (testDateGroup.IsInReportingPeriod)
				{
					assertionList.Add((testDateGroup.PaymentDate, inv01, "200.00|3", "ReportSubCode - 200.00 Small Business Fully Paid"));
					assertionList.Add((testDateGroup.PaymentDate, inv02, "200.00|1", "ReportSubCode - 200.00 Non-Small Business Fully Paid"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestSmallCreditCardPaymentsOnly()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				//These credit card payments against the invoice are all below $100 therefore excluded from the report
				CreateSinglePaymentTypeVarieties(testDateGroup, 99.99m, "PAY", "CCD");
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestCreditCardPaymentsOnly()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				//These credit card payment against the invoices are allow $100, which are not below $100 therefore included in the report
				var invoices = CreateSinglePaymentTypeVarieties(testDateGroup, 100m, "PAY", "CCD");
				if (testDateGroup.IsInReportingPeriod)
				{
					assertionList.Add((testDateGroup.PaymentDate, invoices[0], "100.00|3", "ReportSubCode - Small Business Fully Paid"));
					assertionList.Add((testDateGroup.PaymentDate, invoices[1], "100.00|2", "ReportSubCode - Small Business Partially Paid"));
					assertionList.Add((testDateGroup.PaymentDate, invoices[2], "100.00|1", "ReportSubCode - Non Small Business Fully Paid"));
					assertionList.Add((testDateGroup.PaymentDate, invoices[3], "100.00|0", "ReportSubCode - Non Small Business Partially Paid"));
				}
			}
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestPartialCreditCardPaymentSpanningReportingPeriods()
		{
			//Complicated partial payment scenario, make sure the invoice is NOT included in the report
			var invoiceDate = PreviousReportingFromDate.AddDays(30);
			var dueDate = PreviousReportingFromDate.AddDays(60);
			var firstPaymentDate = PreviousReportingFromDate.AddDays(45);
			var fullyPaidDate = ReportingFromDate.AddDays(30);

			CreatePartialCreditCardPaymentsSpanningOverReportingPeriod(creditorPKSmallBusinessInReportingPeriod, 1,
				invoiceDate, dueDate, fullyPaidDate, firstPaymentDate);
			CreatePartialCreditCardPaymentsSpanningOverReportingPeriod(creditorPKNonSmallBusiness, 3,
				invoiceDate, dueDate, fullyPaidDate, firstPaymentDate);

			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestWhenInvoiceAPAmountIsZero()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				CreateSinglePaymentTypeVarieties(testDateGroup, 0m, "PAY");
				CreateSinglePaymentTypeVarieties(testDateGroup, 0m, "REC");
				CreateSinglePaymentTypeVarieties(testDateGroup, 0m, "CTR");
			}
			RunReportAndAssertQueueingResult();
		}

		public void TestCombinedPayments()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				//Fully Paid with multiple payment types, include "PAY" should be included in the report
				var inv01 = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), -500m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate);
				var pay01 = helper.InsertTransactionHeader("AP", "PAY", GetTransactionNum(), 100m, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, dueDate: testDateGroup.DueDate);
				var rec01 = helper.InsertTransactionHeader("AP", "REC", GetTransactionNum(), 110m, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, dueDate: testDateGroup.DueDate);
				var ctr01 = helper.InsertTransactionHeader("AP", "CTR", GetTransactionNum(), 120m, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, dueDate: testDateGroup.DueDate);
				var crd01 = helper.InsertTransactionHeader("AP", "CRD", GetTransactionNum(), 130m, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, dueDate: testDateGroup.DueDate);
				var jnl01 = helper.InsertTransactionHeader("AP", "JNL", GetTransactionNum(), 140m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate);
				helper.InsertTransactionMatchLink(inv01, "M00001", testDateGroup.PaymentDate, -600m);
				helper.InsertTransactionMatchLink(pay01, "M00001", testDateGroup.PaymentDate, 100m);
				helper.InsertTransactionMatchLink(rec01, "M00001", testDateGroup.PaymentDate, 110m);
				helper.InsertTransactionMatchLink(ctr01, "M00001", testDateGroup.PaymentDate, 120m);
				helper.InsertTransactionMatchLink(crd01, "M00001", testDateGroup.PaymentDate, 130m);
				helper.InsertTransactionMatchLink(jnl01, "M00001", testDateGroup.PaymentDate, 140m);

				if (testDateGroup.IsInReportingPeriod)
				{
					assertionList.Add((testDateGroup.PaymentDate, inv01, "100.00|3", "ReportSubCode - Small Business Fully Paid"));
				}
			}
			// Executing the stored proc and assert the results, only PAY, REC, CTR included
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestMultipleInvoiceProportionPayments()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				var inv01 = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), -200m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate);
				var inv02 = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), -300m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate);
				var pay01 = helper.InsertTransactionHeader("AP", "PAY", GetTransactionNum(), 400m, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod);
				var crd01 = helper.InsertTransactionHeader("AP", "CRD", GetTransactionNum(), 100m, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod);
				helper.InsertTransactionMatchLink(inv01, "M00001", testDateGroup.PaymentDate, -200m);
				helper.InsertTransactionMatchLink(inv02, "M00001", testDateGroup.PaymentDate, -300m);
				helper.InsertTransactionMatchLink(pay01, "M00001", testDateGroup.PaymentDate, 400m);
				helper.InsertTransactionMatchLink(crd01, "M00001", testDateGroup.PaymentDate, 100m);

				if (testDateGroup.IsInReportingPeriod)
				{
					assertionList.Add((testDateGroup.PaymentDate, inv01, "160.00|3", "ReportSubCode - Small Business Fully Paid 160.00 = 400 * (-200) / (-500)"));
					assertionList.Add((testDateGroup.PaymentDate, inv02, "240.00|3", "ReportSubCode - Small Business Fully Paid 240.00 = 400 * (-300) / (-500)"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}

		Guid[] CreateSinglePaymentTypeVarieties((DateTime InvoiceDate, DateTime DueDate, DateTime PaymentDate, bool IsInReportingPeriod) testDateGroup, decimal paymentAmount, string paymentType, string receiptType = "CSH")
		{
			//In the partial payment scenario, we make the total invoice amount twice as much as the payment amount
			var invoiceAmountForPartialPayment = (-2) * paymentAmount ;

			var inv01 = CreatePTRSInvoiceAndPaymentPairFullyPaid(creditorPKSmallBusinessInReportingPeriod,
				testDateGroup.InvoiceDate, testDateGroup.DueDate, -paymentAmount, paymentType, testDateGroup.PaymentDate, 1, receiptType);

			var inv02 = CreatePTRSInvoiceAndPaymentPairPartiallyPaid(creditorPKSmallBusinessInReportingPeriod,
				testDateGroup.InvoiceDate, testDateGroup.DueDate, invoiceAmountForPartialPayment, paymentAmount, paymentType, testDateGroup.PaymentDate, 2, receiptType);

			var inv03 = CreatePTRSInvoiceAndPaymentPairFullyPaid(creditorPKNonSmallBusiness,
				testDateGroup.InvoiceDate, testDateGroup.DueDate, -paymentAmount, paymentType, testDateGroup.PaymentDate, 3, receiptType);

			var inv04 = CreatePTRSInvoiceAndPaymentPairPartiallyPaid(creditorPKNonSmallBusiness,
				testDateGroup.InvoiceDate, testDateGroup.DueDate, invoiceAmountForPartialPayment, paymentAmount, paymentType, testDateGroup.PaymentDate, 4, receiptType);

			return new[] { inv01, inv02, inv03, inv04 };
		}
	}
}

