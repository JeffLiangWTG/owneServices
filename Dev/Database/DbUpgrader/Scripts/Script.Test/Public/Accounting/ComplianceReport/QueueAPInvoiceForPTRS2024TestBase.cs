using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script
{
	[TestsSubclassesOf(typeof(DbCreateScript))]
	abstract class QueueAPInvoiceForPTRS2024TestBase : DbCreateScriptTest
	{
		protected TestDbHelper helper;

		protected readonly DateTime PreviousReportingFromDate = new DateTime(2024, 01, 01);
		protected readonly DateTime PreviousReportingToDate = new DateTime(2024, 06, 30);

		protected readonly DateTime ReportingFromDate = new DateTime(2024, 07, 01);
		protected readonly DateTime ReportingToDate = new DateTime(2024, 12, 31);

		protected readonly DateTime NextReportingFromDate = new DateTime(2025, 01, 01);
		protected readonly DateTime NextReportingToDate = new DateTime(2025, 06, 30);

		protected Guid reportPK;
		protected Guid branchPK;
		protected Guid departmentPK;
		protected Guid creditorPKSmallBusinessInReportingPeriod;
		protected Guid creditorPKNonSmallBusiness;
		protected Guid creditdrPKNoABN;
		protected int nextCreditorId;
		protected int nextTransactionNumber;
		protected List<(DateTime QueryDate, Guid InvoicePK, string ReportSubCode, string AssertionComment)> assertionList;
		protected abstract string ReportCode { get; }
		protected abstract string StoredProcedureName { get; }

		protected (DateTime InvoiceDate, DateTime DueDate, DateTime PaymentDate, bool IsInReportingPeriod)[] DefaultTestDates;

		protected override void SetUp()
		{
			base.SetUp();

			nextCreditorId = 0;
			nextTransactionNumber = 0;
			helper = new TestDbHelper(TestConnection);
			reportPK = helper.InsertComplianceReport(ReportCode, ReportingFromDate, ReportingToDate);
			branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			departmentPK = helper.InsertDepartment("ZZD");
			creditorPKSmallBusinessInReportingPeriod = CreateCreditorWithABN(true, false);
			creditorPKNonSmallBusiness = CreateCreditorWithABN(false);
			creditdrPKNoABN = CreateCreditorWithoutABN();

			//two sets of dates, first set with the payment date equals to ReportinFromDate, Payment Terms 30 days
			//second set at with the payment date equals to ReportingToDate, Payment Terms 60 days
			DefaultTestDates = [new (ReportingFromDate.AddDays(-15), ReportingFromDate.AddDays(30), ReportingFromDate, true),
				new (ReportingToDate.AddDays(-15), ReportingToDate.AddDays(30), ReportingToDate, true),
				new (PreviousReportingToDate.AddDays(-15), PreviousReportingToDate.AddDays(30), PreviousReportingToDate, false),
				new (NextReportingFromDate.AddDays(-15), NextReportingFromDate.AddDays(60), NextReportingFromDate, false)
			];
			assertionList = new List<(DateTime QueryDate, Guid InvoicePK, string ReportSubCode, string AssertionComment)>();
		}

		public void TestNonReportingPeriod()
		{
			var nonReportingDateGroups = new (DateTime FromDate, DateTime ToDate)[]
			{
				new (PreviousReportingFromDate, PreviousReportingToDate),
				new (NextReportingFromDate, NextReportingToDate)
			};

			foreach(var dateGroup in nonReportingDateGroups)
			{
				var creditorPK = CreateCreditorWithABN(true, false, dateGroup.FromDate, dateGroup.ToDate);
				var invoiceDate = dateGroup.FromDate.AddDays(30);
				var dueDate = dateGroup.FromDate.AddDays(60);
				var fullyPaidDate = dateGroup.FromDate.AddDays(30);
				CreatePTRSInvoiceAndPaymentPair(creditorPK, invoiceDate, dueDate, -110m, 110m, "PAY", fullyPaidDate, 1, fullyPaidDate);
			}

			RunReportAndAssertQueueingResult();
		}

		protected string GetTransactionNum() => (++nextTransactionNumber).ToString("D8");

		protected Guid CreateCreditorWithoutABN()
		{
			return CreateCreditor(false);
		}

		protected Guid CreateCreditorWithABN(bool hasPTRDocument = false, bool excludedFromReport = false, DateTime? dateReceived = null, DateTime? validToDate = null)
		{
			return CreateCreditor(true, hasPTRDocument, excludedFromReport, dateReceived, validToDate);
		}

		protected Guid CreateCreditor(bool hasABN, bool hasPTRDocument = false, bool excludedFromReport = false, DateTime? dateReceived = null, DateTime? validToDate = null)
		{
			var creditorPK = InsertOrgHeader($"CRD{++nextCreditorId}", $"Creditor{nextCreditorId}", true);
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = TestDbHelper.DefaultCompanyPK, OB_OH = creditorPK, OB_IsCreditor = 1, OB_APExcludeFromPaymentReports = excludedFromReport ? 1 : 0 });
			if (hasPTRDocument)
			{
				if (dateReceived == null)
				{
					dateReceived = ReportingFromDate;
				}
				if (validToDate == null)
				{
					validToDate = ReportingToDate;
				}
				helper.Insert("JobRequiredDocument", new { EQ_PK = Guid.NewGuid(), EQ_IsValid = true, EQ_DocCategory = "CTR", EQ_DocType = "RSB", EQ_ParentTableCode = "OH", EQ_ParentID = creditorPK, EQ_DateReceived = dateReceived, EQ_ValidToDate = validToDate });
			}
			return creditorPK;
		}

		protected Guid CreatePTRSInvoiceAndPaymentPairFullyPaid(Guid creditorPK, DateTime postDate, DateTime dueDate,
			Decimal invoiceAmount, string paymentType, DateTime matchDate, int matchGroupNumber, string receiptType = "CSH")
		{
			return CreatePTRSInvoiceAndPaymentPair(creditorPK, postDate, dueDate, invoiceAmount, invoiceAmount * (-1m), paymentType, matchDate, matchGroupNumber, matchDate, receiptType);
		}

		protected Guid CreatePTRSInvoiceAndPaymentPairPartiallyPaid(Guid creditorPK, DateTime postDate, DateTime dueDate,
			Decimal invoiceAmount, Decimal paymentAmount, string paymentType, DateTime matchDate, int matchGroupNumber, string receiptType = "CSH")
		{
			return CreatePTRSInvoiceAndPaymentPair(creditorPK, postDate, dueDate, invoiceAmount, paymentAmount, paymentType, matchDate, matchGroupNumber, null, receiptType);
		}

		protected Guid CreatePTRSInvoiceAndPaymentPair(Guid creditorPK, DateTime postDate, DateTime dueDate,
			Decimal invoiceAmount, Decimal paymentAmount,
			string paymentType, DateTime matchDate, int matchGroupNumber, DateTime? fullyPaidDate, string receiptType = "CSH")
		{
			var invoice = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), invoiceAmount, postDate, branchPK, departmentPK, org: creditorPK,
				fullyPaidDate: fullyPaidDate, dueDate: dueDate, category: "STD");
			helper.InsertTransactionMatchLink(invoice, matchGroupNumber.ToString(), matchDate, -paymentAmount);

			var payment = helper.InsertTransactionHeader("AP", paymentType, GetTransactionNum(), paymentAmount, matchDate, branchPK, departmentPK, org: creditorPK,
				fullyPaidDate: fullyPaidDate, dueDate: dueDate, category: "STD", receiptType: receiptType);
			helper.InsertTransactionMatchLink(payment, matchGroupNumber.ToString(), matchDate, paymentAmount);

			return invoice;
		}

		protected void CreateFullCashAdvancePayment(Guid creditor, Decimal invoiceAmount, Decimal paymentAmount, DateTime paymentDate, int matchGroupNumber)
		{
			// These four records represents a scenario of a fully paid of an advance payment in CW
			var inv01 = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), invoiceAmount, paymentDate, branchPK, departmentPK, org: creditor, fullyPaidDate: paymentDate, dueDate: paymentDate, category: "SBC");
			helper.InsertTransactionMatchLink(inv01, matchGroupNumber.ToString(), paymentDate, invoiceAmount);

			var jnl01 = helper.InsertTransactionHeader("AP", "JNL", GetTransactionNum(), paymentAmount, paymentDate, branchPK, departmentPK, org: creditor, fullyPaidDate: paymentDate, dueDate: paymentDate, category: "API");
			helper.InsertTransactionMatchLink(jnl01, matchGroupNumber.ToString(), paymentDate, paymentAmount);

			var pay01 = helper.InsertTransactionHeader("AP", "PAY", GetTransactionNum(), paymentAmount, paymentDate, branchPK, departmentPK, org: creditor, fullyPaidDate: paymentDate, category: "O05");
			helper.InsertTransactionMatchLink(pay01, (matchGroupNumber + 1).ToString(), paymentDate, paymentAmount);

			var jnl02 = helper.InsertTransactionHeader("AP", "JNL", GetTransactionNum(), invoiceAmount, paymentDate, branchPK, departmentPK, org: creditor, fullyPaidDate: paymentDate, category: "APP");
			helper.InsertTransactionMatchLink(jnl02, (matchGroupNumber + 1).ToString(), paymentDate, invoiceAmount);
		}

		protected Guid CreateFullyPaidARInvoicePayingAPInvoice((DateTime InvoiceDate, DateTime DueDate, DateTime PaymentDate, bool IsInReportingPeriod) testDateGroup, Guid creditor, Decimal apInvoiceAmount, int matchGroupNumber)
		{
			// These four records represents a scenario of an AR invoice paying against an AP invoice fully in CW

			//AP Invoice,  negative amount
			var apInv = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), apInvoiceAmount, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditor, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate, category: "STD");
			helper.InsertTransactionMatchLink(apInv, matchGroupNumber.ToString(), testDateGroup.PaymentDate, apInvoiceAmount);

			//AR Invoice, positive amount
			var arInv = helper.InsertTransactionHeader("AR", "INV", GetTransactionNum(), -apInvoiceAmount, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditor, fullyPaidDate: testDateGroup.DueDate, category: "FIN");
			helper.InsertTransactionMatchLink(arInv, matchGroupNumber.ToString(), testDateGroup.PaymentDate, -apInvoiceAmount);

			// AP Contra, postiive amount
			var ctr01 = helper.InsertTransactionHeader("AP", "CTR", GetTransactionNum(), -apInvoiceAmount, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditor, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.PaymentDate);
			helper.InsertTransactionMatchLink(ctr01, matchGroupNumber.ToString(), testDateGroup.PaymentDate, -apInvoiceAmount);

			// AR Contra, negative amount
			var ctr02 = helper.InsertTransactionHeader("AP", "CTR", GetTransactionNum(), apInvoiceAmount, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditor, fullyPaidDate: testDateGroup.PaymentDate);
			helper.InsertTransactionMatchLink(ctr02, matchGroupNumber.ToString(), testDateGroup.PaymentDate, apInvoiceAmount);

			return apInv;
		}

		protected Guid CreateFullyPaidARInvoiceAndCashPaymentCombined((DateTime InvoiceDate, DateTime DueDate, DateTime PaymentDate, bool IsInReportingPeriod) testDateGroup, Guid creditor, Decimal apInvoiceAmount, Decimal arInvoiceAmount, int matchGroupNumber)
		{
			// These five records represents a scenario of an AR invoice paying against an AP invoice combined with cashpayment fully in CW

			var cashPaymentAmount = -(apInvoiceAmount + arInvoiceAmount);

			//AP Invoice,  negative amount
			var apInv = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), apInvoiceAmount, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditor, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.DueDate, category: "STD");
			helper.InsertTransactionMatchLink(apInv, matchGroupNumber.ToString(), testDateGroup.PaymentDate, apInvoiceAmount);

			//AR Invoice, positive amount
			var arInv = helper.InsertTransactionHeader("AR", "INV", GetTransactionNum(), arInvoiceAmount, testDateGroup.InvoiceDate, branchPK, departmentPK, org: creditor, fullyPaidDate: testDateGroup.DueDate, category: "FIN");
			helper.InsertTransactionMatchLink(arInv, matchGroupNumber.ToString(), testDateGroup.PaymentDate, arInvoiceAmount);

			// AP Contra, postiive amount
			var ctr01 = helper.InsertTransactionHeader("AP", "CTR", GetTransactionNum(), arInvoiceAmount, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditor, fullyPaidDate: testDateGroup.PaymentDate, dueDate: testDateGroup.PaymentDate);
			helper.InsertTransactionMatchLink(ctr01, matchGroupNumber.ToString(), testDateGroup.PaymentDate, arInvoiceAmount);

			// AR Contra, negative amount
			var ctr02 = helper.InsertTransactionHeader("AP", "CTR", GetTransactionNum(), -arInvoiceAmount, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditor, fullyPaidDate: testDateGroup.PaymentDate);
			helper.InsertTransactionMatchLink(ctr02, matchGroupNumber.ToString(), testDateGroup.PaymentDate, -arInvoiceAmount);

			// CashPayment
			var pay01 = helper.InsertTransactionHeader("AP", "PAY", GetTransactionNum(), cashPaymentAmount, testDateGroup.PaymentDate, branchPK, departmentPK, org: creditor, fullyPaidDate: testDateGroup.PaymentDate);
			helper.InsertTransactionMatchLink(pay01, matchGroupNumber.ToString(), testDateGroup.PaymentDate, cashPaymentAmount);

			return apInv;
		}

		protected Guid CreatePartialCreditCardPaymentsSpanningOverReportingPeriod(Guid creditorPK, int matchGroupNumber,
			DateTime invoiceDate, DateTime dueDate, DateTime fullyPaidDate, DateTime firstPaymentDate)
		{
			//Complicated partial payment scenario
			var invoice = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), -1000m, invoiceDate, branchPK, departmentPK,
				org: creditorPK, fullyPaidDate: fullyPaidDate, dueDate: dueDate, category: "SBC");
			helper.InsertTransactionMatchLink(invoice, matchGroupNumber.ToString(), firstPaymentDate, -900.01m);

			//cash payment $900.01 in the previous reporting period
			var pay011 = helper.InsertTransactionHeader("AP", "PAY", GetTransactionNum(), 900.01m,
				firstPaymentDate, branchPK, departmentPK, org: creditorPK, fullyPaidDate: fullyPaidDate, category: "O05");
			helper.InsertTransactionMatchLink(pay011, matchGroupNumber.ToString(), firstPaymentDate, 900.01m);

			//credit card payment of $99.99 in the current reporting period, paid off the full invoice amount
			var pay012 = helper.InsertTransactionHeader("AP", "PAY", GetTransactionNum(), 99.99m, fullyPaidDate,
				branchPK, departmentPK, org: creditorPK, fullyPaidDate: fullyPaidDate, category: "O05", receiptType: "CCD");
			helper.InsertTransactionMatchLink(invoice, (matchGroupNumber + 1).ToString(), fullyPaidDate, -99.99m);
			helper.InsertTransactionMatchLink(pay012, (matchGroupNumber + 1).ToString(), fullyPaidDate, 99.99m);

			return invoice;
		}

		protected Guid[] CreateAPInvoiceARInvoicePair(Guid creditor, Decimal invoiceAmount, Decimal paymentAmount, DateTime paymentDate, int matchGroupNumber)
		{
			var inv01 = helper.InsertTransactionHeader("AP", "INV", GetTransactionNum(), invoiceAmount, paymentDate, branchPK, departmentPK, org: creditor, fullyPaidDate: paymentDate, dueDate: paymentDate, category: "STD");
			var inv02 = helper.InsertTransactionHeader("AR", "INV", GetTransactionNum(), paymentAmount, paymentDate, branchPK, departmentPK, fullyPaidDate: paymentDate, dueDate: paymentDate, category: "STD");

			helper.InsertTransactionMatchLink(inv01, matchGroupNumber.ToString(), paymentDate, invoiceAmount);
			helper.InsertTransactionMatchLink(inv02, matchGroupNumber.ToString(), paymentDate, paymentAmount);

			return new Guid[] { inv01, inv02 };
		}

		protected void RunReportAndAssertQueueingResult(List<(DateTime QueryDate, Guid InvoicePK, string ReportSubCode, string AssertionComment)> assertionList = null)
		{
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC {StoredProcedureName} @ReportPK = '{reportPK}'");
			//the stored procedure never returns the records to the caller directly. It populates the database table instead
			AssertEquals("Result should not have any rows", 0, result.Rows.Count);

			var expectedRows = (assertionList == null) ? 0 : assertionList.Count;
			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT COUNT(*) FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals($"Result should have {expectedRows} row(s)", expectedRows, (int)result.Rows[0][0]);

			if (assertionList != null)
			{
				foreach (var asserting in assertionList)
				{
					var sql = @$"SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue
						WHERE ACQ_Date = '{asserting.QueryDate:yyyy-MM-dd}' AND ACQ_ReportType = '{ReportCode}'
						AND ACQ_ParentTableCode = 'AH' AND ACQ_ParentID = '{asserting.InvoicePK}'
						AND ACQ_GC_Company = '{TestDbHelper.DefaultCompanyPK}' AND ACQ_GB_Branch = '{branchPK}'";

					var individualResult = DataUtils.GetDataTableFromQuery(TestConnection, sql);
					AssertEquals("Result should have one row", 1, individualResult.Rows.Count);
					AssertEquals(asserting.AssertionComment, asserting.ReportSubCode, (string)individualResult.Rows[0][0]);
				}
			}
		}

		protected Guid InsertOrgHeader(string code, string fullName, bool hasABN)
		{
			var headerPK = Guid.NewGuid();
			helper.Insert(OrgHeaderSchema.Constants.TableName, new
			{
				OH_PK = headerPK,
				OH_Code = code,
				OH_FullName = fullName,
			});

			if (hasABN)
			{
				helper.Insert(OrgCusCodeSchema.Constants.TableName, new
				{
					OK_PK = Guid.NewGuid(),
					OK_CodeType = "ABN",
					OK_CountryDefault = false,
					OK_CustomsRegNo = Guid.NewGuid().ToString(),
					OK_IsValid = true,
					OK_OH = headerPK,
					OK_RN_NKCodeCountry = "AU",
					OK_SystemCreateTimeUtc = DateTime.UtcNow,
					OK_SystemCreateUser = "E",
					OK_SystemLastEditTimeUtc = DateTime.UtcNow,
					OK_SystemLastEditUser = "E"
				});
			}
			return headerPK;
		}
	}
}
