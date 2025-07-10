using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueFullyPaidAPInvoicesForPTRS2024))]
	class QueueFullyPaidAPInvoicesForPTRS2024Test : QueueAPInvoiceForPTRS2024TestBase
	{
		protected override string ReportCode => "PTS";

		protected override string StoredProcedureName => "QueueFullyPaidAPInvoicesForPTRS2024";

		public void TestNonReportingOrganisation()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				//Creditor without PTR documenet
				var creditorPKNoPTR = CreateCreditorWithABN(hasPTRDocument: false);
				CreatePTRSInvoiceAndPaymentPair(creditorPKNoPTR, testDateGroup.InvoiceDate, testDateGroup.DueDate, -110m, 110m, "PAY", testDateGroup.PaymentDate, 1, testDateGroup.PaymentDate);

				//Creditor without PTR documenet but excluded from the reporting explicitly
				var creditorPKExcludedFromReporting = CreateCreditorWithABN(hasPTRDocument: true, excludedFromReport: true, ReportingFromDate, ReportingToDate);
				CreatePTRSInvoiceAndPaymentPair(creditorPKExcludedFromReporting, testDateGroup.InvoiceDate, testDateGroup.DueDate, -210m, 210m, "PAY", testDateGroup.PaymentDate, 2, testDateGroup.PaymentDate);

				// Invoices for creditors having PTR document valid outside of Report period
				var creditorPKPTROutsideReportPeriod = CreateCreditorWithABN(hasPTRDocument: true, excludedFromReport: false, PreviousReportingFromDate, PreviousReportingToDate);
				CreatePTRSInvoiceAndPaymentPair(creditorPKPTROutsideReportPeriod, testDateGroup.InvoiceDate, testDateGroup.DueDate, -310m, 310m, "PAY", testDateGroup.PaymentDate, 3, testDateGroup.PaymentDate);
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestSingleCashPaymentBeforeDueDate()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				var adjustedDueDate = testDateGroup.PaymentDate.AddDays(20);
				var invoice = CreatePTRSInvoiceAndPaymentPair(creditorPKSmallBusinessInReportingPeriod,
					testDateGroup.InvoiceDate, adjustedDueDate, -340m, 340m, "PAY", testDateGroup.PaymentDate, 1, testDateGroup.PaymentDate);

				if (testDateGroup.IsInReportingPeriod)
				{
					//The full payment was made on 20 days before the due date therefore the third part of the pipeline delimited value is 20 (Due Date - FullyPaid Date)
					assertionList.Add((testDateGroup.PaymentDate, invoice, "340.00|16|20|35", "ReportSubCode:$340.00; Payment Times: 16; Before Due Date: 20; Payment Term:35"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestSingleCashPaymentOnDueDate()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				var adjustedDueDate = testDateGroup.PaymentDate;
				var invoice = CreatePTRSInvoiceAndPaymentPair(creditorPKSmallBusinessInReportingPeriod,
				testDateGroup.InvoiceDate, adjustedDueDate, -300m, 300m, "PAY", testDateGroup.PaymentDate, 1, testDateGroup.PaymentDate);

				if (testDateGroup.IsInReportingPeriod)
				{
					//The full payment was made on the due date therefore the third part of the pipeline delimited value is 0 (Due Date - FullyPaid Date)
					assertionList.Add((testDateGroup.PaymentDate, invoice, "300.00|16|0|15", "ReportSubCode:$300.00; Payment Times: 16; On Due Date; Payment Term:15"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestSingleCashPaymentAfterDueDate()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				var adjustedDueDate = testDateGroup.PaymentDate.AddDays(-12);
				var invoice = CreatePTRSInvoiceAndPaymentPair(creditorPKSmallBusinessInReportingPeriod,
					testDateGroup.InvoiceDate, adjustedDueDate, -450m, 450m, "PAY", testDateGroup.PaymentDate, 1, testDateGroup.PaymentDate);

				if (testDateGroup.IsInReportingPeriod)
				{
					//The full payment was made 12 days after the due date therefore the third part of the pipeline delimited value is -12 (Due Date - FullyPaid Date)
					assertionList.Add((testDateGroup.PaymentDate, invoice, "450.00|16|-12|3", "ReportSubCode:$450.00; Payment Times: 16; After Due Date: 12; Payment Term:3"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestSingleCashPaymentAgainstNonABNOrg()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				CreatePTRSInvoiceAndPaymentPair(creditdrPKNoABN,
				testDateGroup.InvoiceDate, testDateGroup.DueDate, -340m, 340m, "PAY", testDateGroup.PaymentDate, 1, testDateGroup.PaymentDate);
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestSingleCreditNotesPayment()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				CreatePTRSInvoiceAndPaymentPair(creditorPKSmallBusinessInReportingPeriod,
				testDateGroup.InvoiceDate, testDateGroup.DueDate, -100m, 100m, "CRD", testDateGroup.PaymentDate, 1, testDateGroup.PaymentDate);
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestSingleAdvancedPayment()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				CreateFullCashAdvancePayment(creditorPKSmallBusinessInReportingPeriod, -110m, 110m, testDateGroup.PaymentDate, 1);
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestSingleARInvoicePayAgainstAPInvoice()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				//This type of payment includes CTR payments, which will be excluded from the report
				var invoice = CreateFullyPaidARInvoicePayingAPInvoice(testDateGroup, creditorPKSmallBusinessInReportingPeriod, -100m, 1);
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestARInvoiceAndCashPaymentCombined()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				var inv01 = CreateFullyPaidARInvoiceAndCashPaymentCombined(testDateGroup, creditorPKSmallBusinessInReportingPeriod, -500m, 300m, 1);
				if (testDateGroup.IsInReportingPeriod)
				{
					assertionList.Add((testDateGroup.PaymentDate, inv01, "200.00|16|30|45", "ReportSubCode:$200.00; Payment Times: 16; Before Due Date: 30; Payment Term:45"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestSingleSmallCreditCardPayment()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				//This credit card payment against the invoice is below $100 therefore excluded from the report
				CreatePTRSInvoiceAndPaymentPair(creditorPKSmallBusinessInReportingPeriod,
				testDateGroup.InvoiceDate, testDateGroup.DueDate, -99.99m, 99.99m, "PAY", testDateGroup.PaymentDate, 1, testDateGroup.PaymentDate, "CCD");
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestSingleBigCreditCardPayment()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				//This credit card payment against the invoice is not below $100 therefore included in the report
				var invoice = CreatePTRSInvoiceAndPaymentPair(creditorPKSmallBusinessInReportingPeriod,
				testDateGroup.InvoiceDate, testDateGroup.DueDate, -100m, 100m, "PAY", testDateGroup.PaymentDate, 1, testDateGroup.PaymentDate, "CCD");

				if (testDateGroup.IsInReportingPeriod)
				{
					assertionList.Add((testDateGroup.PaymentDate, invoice, "100.00|16|30|45", "ReportSubCode:$100.00; Payment Times: 16; Before Due Date: 30; Payment Term:45"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestPartialCreditCardPayment()
		{
			var invoiceDate = PreviousReportingFromDate.AddDays(30);
			var dueDate = PreviousReportingFromDate.AddDays(60);
			var firstPaymentDate = PreviousReportingFromDate.AddDays(45);
			var fullyPaidDate = ReportingFromDate.AddDays(30);

			CreatePartialCreditCardPaymentsSpanningOverReportingPeriod(creditorPKSmallBusinessInReportingPeriod, 1,
				invoiceDate, dueDate, fullyPaidDate, firstPaymentDate);

			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestWhenInvoiceAPAmountIsZero()
		{
			foreach (var testDateGroup in DefaultTestDates)
			{
				CreatePTRSInvoiceAndPaymentPair(creditorPKSmallBusinessInReportingPeriod,
				testDateGroup.InvoiceDate, testDateGroup.DueDate, 0m, 0m, "PAY", testDateGroup.PaymentDate, 1, testDateGroup.PaymentDate);
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult();
		}

		public void TestCombinedPayments()
		{
			for(var i = 0; i < DefaultTestDates.Length; i++)
			{
				var testDateGroup = DefaultTestDates[i];
				var matchNumber = i.ToString("D8");
				//This method is to test a combined payment of cash, credit notes and journal to pay off the an AP invoice.
				var inv01 = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), -350m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate);
				var pay01 = helper.InsertTransactionHeader("AP", "PAY", GetTransactionNum(), 100m, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate);
				var crd01 = helper.InsertTransactionHeader("AP", "CRD", GetTransactionNum(), 120m, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate);
				var inv02 = helper.InsertTransactionHeader("AP", "JNL", GetTransactionNum(), 130m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate);
				helper.InsertTransactionMatchLink(inv01, matchNumber, testDateGroup.PaymentDate, -350m);
				helper.InsertTransactionMatchLink(pay01, matchNumber, testDateGroup.PaymentDate, 100m);
				helper.InsertTransactionMatchLink(crd01, matchNumber, testDateGroup.PaymentDate, 120m);
				helper.InsertTransactionMatchLink(inv02, matchNumber, testDateGroup.PaymentDate, 130m);

				if (testDateGroup.IsInReportingPeriod)
				{
					assertionList.Add((testDateGroup.PaymentDate, inv01, "100.00|16|30|45", "ReportSubCode:$100.00; Payment Times: 16; Before Due Date: 30; Payment Term:45"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}

		public void TestMultipleInvoiceProportionPayments()
		{
			for (var i = 0; i < DefaultTestDates.Length; i++)
			{
				var testDateGroup = DefaultTestDates[i];
				var matchNumber = i.ToString("D8");
				var inv01 = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), -200m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate);
				var inv02 = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), -300m, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate);
				var pay01 = helper.InsertTransactionHeader("AP", "PAY", GetTransactionNum(), 400m, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod);
				var crd01 = helper.InsertTransactionHeader("AP", "CRD", GetTransactionNum(), 100m, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditorPKSmallBusinessInReportingPeriod);
				helper.InsertTransactionMatchLink(inv01, matchNumber, testDateGroup.PaymentDate, -200m);
				helper.InsertTransactionMatchLink(inv02, matchNumber, testDateGroup.PaymentDate, -300m);
				helper.InsertTransactionMatchLink(pay01, matchNumber, testDateGroup.PaymentDate, 400m);
				helper.InsertTransactionMatchLink(crd01, matchNumber, testDateGroup.PaymentDate, 100m);

				if (testDateGroup.IsInReportingPeriod)
				{
					assertionList.Add((testDateGroup.PaymentDate, inv01, "160.00|16|30|45", "ReportSubCode - Invoice Amount 160.00 = 400 * (-200) / (-500); Payment Times: 16; Before Due Date: 30; Payment Term:45"));
					assertionList.Add((testDateGroup.PaymentDate, inv02, "240.00|16|30|45", "ReportSubCode - Invoice Amount 240.00 = 400 * (-300) / (-500); Payment Times: 16; Before Due Date: 30; Payment Term:45"));
				}
			}
			// Executing the stored proc and assert the results
			RunReportAndAssertQueueingResult(assertionList);
		}
	}
}
