using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(ViewCommissionLine))]
	internal class ViewCommissionLineTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties

		public void TestZDecimalsHaveCorrectDecimalPlacesViewCommissionLine()
		{
			var line = Factory.New<ViewCommissionLine>();

			var localList = new List<string>
			{
				nameof(line.VCL_TotalCommissionableAmountInLocalCurrency),
				nameof(line.VCL_ShareCommissionAmountInLocalCurrency),
				nameof(line.VCL_EntityCommissionAmountInLocalCurrency)
			};

			var transactionList = new List<string> {
				nameof(line.VCL_TotalCommissionableAmount),
				nameof(line.VCL_TransactionAmount),
				nameof(line.VCL_ShareCommissionAmount),
				nameof(line.VCL_EntityCommissionAmount),
			};

			var prefList = new List<string>
			{
				nameof(line.VCL_TotalCommissionableAmountInPreferredCurrency),
				nameof(line.VCL_ShareCommissionAmountInPreferredCurrency),
				nameof(line.VCL_EntityCommissionAmountInPreferredCurrency)
			};

			var exList = new List<string>
			{
				nameof(line.VCL_CommissionToLocalExchangeRate),
				nameof(line.VCL_LocalToPreferredExchangeRate)
			};

			var percentList = new List<string>
			{
				nameof(line.VCL_EntityPercentage),
				nameof(line.VCL_SharePercentage)
			};

			var tester = new DecimalPlacesAttributeTester(line, line.Company);
			tester.CheckNonLocalCurrency(localList, nameof(line.LocalCurrencyDecimalPlaces), nameof(line.VCL_RX_NKLocalCurrency), line);
			tester.CheckNonLocalCurrency(transactionList, nameof(line.TransactionCurrencyDecimalPlaces), nameof(line.VCL_RX_NKTransactionCurrency), line);
			tester.CheckNonLocalCurrency(prefList, nameof(line.PreferredCurrencyDecimalPlaces), nameof(line.VCL_RX_NKPreferredPaymentCurrency), line);
			tester.CheckExchangeRate(exList, nameof(line.ExchangeRateDecimals));
			tester.CheckConstant(percentList, nameof(line.PercentageDecimals), Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
		}

		public void TestEntityCode()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_FullName = "Andrew";
			var party = Factory.New<OrgHeader>();
			party.OH_Code = "WISETECH";
			party.OH_FullName = "WiseTech Global";

			var commissionLine = Factory.New<ViewCommissionLine>();
			AssertEquals("", commissionLine.EntityCode);

			commissionLine.VCL_OH_Party = party.PK;
			AssertEquals("WISETECH", commissionLine.EntityCode);

			commissionLine.VCL_GS_NKStaff = "ADL";
			AssertEquals("ADL", commissionLine.EntityCode);
		}

		public void TestEntityName()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_FullName = "Andrew";
			var party = Factory.New<OrgHeader>();
			party.OH_Code = "WISETECH";
			party.OH_FullName = "WiseTech Global";

			var commissionLine = Factory.New<ViewCommissionLine>();
			AssertEquals("", commissionLine.EntityName);

			commissionLine.VCL_OH_Party = party.PK;
			AssertEquals("WiseTech Global", commissionLine.EntityName);

			commissionLine.VCL_GS_NKStaff = "ADL";
			AssertEquals("Andrew", commissionLine.EntityName);
		}

		public void TestTransactionCurrencyDecimalPlaces()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			commissionLine.VCL_RX_NKTransactionCurrency = "";
			AssertEquals(2, commissionLine.TransactionCurrencyDecimalPlaces);

			commissionLine.VCL_RX_NKTransactionCurrency = "IDR";
			AssertEquals(0, commissionLine.TransactionCurrencyDecimalPlaces);

			commissionLine.VCL_RX_NKTransactionCurrency = "USD";
			AssertEquals(2, commissionLine.TransactionCurrencyDecimalPlaces);
		}

		public void TestLocalCurrencyDecimalPlaces()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			commissionLine.VCL_RX_NKLocalCurrency = "";
			AssertEquals(2, commissionLine.LocalCurrencyDecimalPlaces);

			commissionLine.VCL_RX_NKLocalCurrency = "IDR";
			AssertEquals(0, commissionLine.LocalCurrencyDecimalPlaces);

			commissionLine.VCL_RX_NKLocalCurrency = "USD";
			AssertEquals(2, commissionLine.LocalCurrencyDecimalPlaces);
		}

		public void TestPreferredCurrencyDecimalPlaces()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();

			commissionLine.VCL_RX_NKPreferredPaymentCurrency = "";
			AssertEquals(2, commissionLine.PreferredCurrencyDecimalPlaces);

			commissionLine.VCL_RX_NKPreferredPaymentCurrency = "IDR";
			AssertEquals(0, commissionLine.PreferredCurrencyDecimalPlaces);

			commissionLine.VCL_RX_NKPreferredPaymentCurrency = "USD";
			AssertEquals(2, commissionLine.PreferredCurrencyDecimalPlaces);
		}

		public void TestVCL_TotalCommissionableAmountInLocalCurrency()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			commissionLine.VCL_RX_NKCommissionCurrency = "AUD";
			commissionLine.VCL_CommissionToLocalExchangeRate = 0.5;
			commissionLine.VCL_LocalToPreferredExchangeRate = 10;
			commissionLine.VCL_TotalCommissionableAmount = 100m;

			AssertEquals(50m, commissionLine.VCL_TotalCommissionableAmountInLocalCurrency);
		}

		public void TestVCL_TotalCommissionableAmountInPreferredCurrency()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			commissionLine.VCL_RX_NKCommissionCurrency = "AUD";
			commissionLine.VCL_CommissionToLocalExchangeRate = 0.5;
			commissionLine.VCL_LocalToPreferredExchangeRate = 10;
			commissionLine.VCL_TotalCommissionableAmount = 100m;

			AssertEquals(500m, commissionLine.VCL_TotalCommissionableAmountInPreferredCurrency);
		}

		public void TestVCL_ShareCommissionAmountInLocalCurrency()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			commissionLine.VCL_RX_NKCommissionCurrency = "AUD";
			commissionLine.VCL_CommissionToLocalExchangeRate = 0.5;
			commissionLine.VCL_LocalToPreferredExchangeRate = 10;
			commissionLine.VCL_ShareCommissionAmount = 100m;

			AssertEquals(50m, commissionLine.VCL_ShareCommissionAmountInLocalCurrency);
		}

		public void TestVCL_ShareCommissionAmountInPreferredCurrency()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			commissionLine.VCL_RX_NKCommissionCurrency = "AUD";
			commissionLine.VCL_CommissionToLocalExchangeRate = 0.5;
			commissionLine.VCL_LocalToPreferredExchangeRate = 10;
			commissionLine.VCL_ShareCommissionAmount = 100m;

			AssertEquals(500m, commissionLine.VCL_ShareCommissionAmountInPreferredCurrency);
		}

		public void TestVCL_EntityCommissionAmountInLocalCurrency()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			commissionLine.VCL_RX_NKCommissionCurrency = "AUD";
			commissionLine.VCL_CommissionToLocalExchangeRate = 0.5;
			commissionLine.VCL_LocalToPreferredExchangeRate = 10;
			commissionLine.VCL_EntityCommissionAmount = 100m;

			AssertEquals(50m, commissionLine.VCL_EntityCommissionAmountInLocalCurrency);
		}

		public void TestVCL_EntityCommissionAmountInPreferredCurrency()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			commissionLine.VCL_RX_NKCommissionCurrency = "AUD";
			commissionLine.VCL_CommissionToLocalExchangeRate = 0.5;
			commissionLine.VCL_LocalToPreferredExchangeRate = 10;
			commissionLine.VCL_EntityCommissionAmount = 100m;

			AssertEquals(500m, commissionLine.VCL_EntityCommissionAmountInPreferredCurrency);
		}

		public void TestCommissionStatus()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();

			AssertEquals(AccCommissionLineCommissionStatusList.Codes.Pending, commissionLine.CommissionStatus);
			AssertEquals(AccCommissionLineCommissionStatusList.Codes.Pending, commissionLine.CommissionStatusCode);
			AssertEquals(AccCommissionLineCommissionStatusList.Descriptions.Pending, commissionLine.CommissionStatusDescription);

			commissionLine.VCL_ApprovedDateTimeUtc = new ZDateTime(2000, 1, 1);
			AssertEquals(AccCommissionLineCommissionStatusList.Codes.Approved, commissionLine.CommissionStatus);
			AssertEquals(AccCommissionLineCommissionStatusList.Codes.Approved, commissionLine.CommissionStatusCode);
			AssertEquals(AccCommissionLineCommissionStatusList.Descriptions.Approved, commissionLine.CommissionStatusDescription);

			commissionLine.VCL_PaidDateTimeUtc = new ZDate(2010, 1, 1);
			AssertEquals(AccCommissionLineCommissionStatusList.Codes.Paid, commissionLine.CommissionStatus);
			AssertEquals(AccCommissionLineCommissionStatusList.Codes.Paid, commissionLine.CommissionStatusCode);
			AssertEquals(AccCommissionLineCommissionStatusList.Descriptions.Paid, commissionLine.CommissionStatusDescription);
		}

		public void TestHasFullyPaid()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();

			commissionLine.VCL_TransactionFullyPaidDate = ZDateTime.Today;
			AssertEquals(true, commissionLine.HasFullyPaid);

			commissionLine.VCL_TransactionFullyPaidDate = ZDateTime.Empty;
			AssertEquals(false, commissionLine.HasFullyPaid);

			commissionLine.VCL_TransactionFullyPaidDate = ZDateTime.Today;
			AssertNotEquals(false, commissionLine.HasFullyPaid);
		}

		public void TestIsARInvoice()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();

			commissionLine.VCL_Ledger = "AR";
			commissionLine.VCL_TransactionType = "INV";
			AssertEquals(true, commissionLine.IsARInvoice);

			commissionLine.VCL_Ledger = "AP";
			AssertNotEquals(true, commissionLine.IsARInvoice);

			commissionLine.VCL_Ledger = "AR";
			commissionLine.VCL_TransactionType = "PAY";
			AssertEquals(false, commissionLine.IsARInvoice);
		}

		public void TestRecognitionDate()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_Ledger = LedgerTypes.AccountsReceivable;

			var jobCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			jobCommissionHeader.CH0_AH_Source = header.PK;
			jobCommissionHeader.CH0_GroupingSourceTableCode = job.TablePrefix;
			jobCommissionHeader.CH0_GroupingSourceID = job.PK;
			jobCommissionHeader.CH0_CommissionDate = new ZDate(2005, 1, 1);

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();

			var line = jobCommissionHeader.Lines.AddNew();

			var jobCommissionLineGroup = jobCommissionHeader.LineGroups.AddNew();
			jobCommissionLineGroup.CLG_AC = chargeCode1.PK;
			jobCommissionLineGroup.CLG_CommissionDate = new ZDate(2005, 6, 30);
			var line2 = jobCommissionLineGroup.Lines.AddNew();

			Factory.Save();

			var commissionLine = Factory.Load<ViewCommissionLine>(line.PK);

			AssertEquals("Date for header level", new ZDate(2005, 1, 1), commissionLine.RecognitionDate);

			commissionLine = Factory.Load<ViewCommissionLine>(line2.PK);

			AssertEquals("Date for charge level", new ZDate(2005, 6, 30), commissionLine.RecognitionDate);
		}

		public void TestGroupingSourceUniqueId()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_JobNum = "00001001";
			var invoice1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice1.AH_TransactionType = TransactionTypes.Invoice;
			invoice1.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice1.AH_TransactionNum = "TEST00001001";
			var invoice2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice2.AH_TransactionNum = "TEST00001002";
			invoice2.AH_TransactionType = TransactionTypes.Invoice;
			invoice2.AH_Ledger = LedgerTypes.AccountsReceivable;

			var jobCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			jobCommissionHeader.CH0_AH_Source = invoice1.PK;
			jobCommissionHeader.CH0_GroupingSourceTableCode = job.TablePrefix;
			jobCommissionHeader.CH0_GroupingSourceID = job.PK;
			jobCommissionHeader.CH0_CommissionDate = new ZDate(2005, 1, 1);
			jobCommissionHeader.CH0_JobNumber = job.JH_JobNum;
			jobCommissionHeader.CH0_GC = GlbCompany.CurrentCompany.PK;

			var invoiceCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceCommissionHeader.CH0_AH_Source = invoice2.PK;
			invoiceCommissionHeader.CH0_GroupingSourceTableCode = invoice2.TablePrefix;
			invoiceCommissionHeader.CH0_GroupingSourceID = invoice2.PK;
			invoiceCommissionHeader.CH0_CommissionDate = new ZDate(2005, 1, 1);

			var line = jobCommissionHeader.Lines.AddNew();
			var line2 = invoiceCommissionHeader.Lines.AddNew();
			Factory.Save();

			var commissionLine = Factory.Load<ViewCommissionLine>(line.PK);
			AssertEquals("Unique Id for Job", $"{GlbCompany.CurrentCompany.PK}-{job.JH_JobNum}".ToUpper(), commissionLine.VCL_GroupingSourceUniqueId);

			var commissionLine2 = Factory.Load<ViewCommissionLine>(line2.PK);
			AssertEquals("Unique Id from Invoice", $"{invoice2.PK}".ToUpper(), commissionLine2.VCL_GroupingSourceUniqueId);
		}

		#endregion

		#region Overrides

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Can not save nor delete view", true);
		}

		#endregion
	}
}
