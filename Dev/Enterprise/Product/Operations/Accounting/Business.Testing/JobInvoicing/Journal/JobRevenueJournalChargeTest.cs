using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobRevenueJournalCharge))]
	public class JobRevenueJournalChargeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesJobRevenueJournalCharge()
		{
			JobRevenueJournalCharge charge = new JobRevenueJournalCharge(Journal);

			var localList = new List<string>
			{
				nameof(charge.BillingTabLocalAmount),
				nameof(charge.SharedLocalAmount)
			};

			var osList = new List<string>
			{
				nameof(charge.BillingTabOsAmount),
				nameof(charge.SharedOsAmount)
			};

			var exList = new List<string>
			{
				nameof(charge.ExchangeRate)
			};

			var sharedList = new List<string>
			{
				nameof(charge.Share)
			};

			var tester = new DecimalPlacesAttributeTester(charge);
			tester.CheckLocalCurrency(localList, nameof(charge.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(charge.CurrencyDecimals), nameof(charge.Currency), charge);
			tester.CheckExchangeRate(exList, nameof(charge.ExchangeRateDecimalPlaces));
			tester.CheckConstant(sharedList, nameof(charge.ShareDecimals), 2);
		}

		public void TestConstructor()
		{
			Journal.BranchPKFrom = ZGuid.NewZGuid();
			Journal.BranchPKTo = ZGuid.NewZGuid();
			Journal.DepartmentPKFrom = ZGuid.NewZGuid();
			Journal.DepartmentPKTo = ZGuid.NewZGuid();
			Journal.DefaultSharing = 11M;
			Journal.CostRevenueTypeFrom = TransactionLineTypes.Cost;
			Journal.CostRevenueTypeTo = TransactionLineTypes.Revenue;
			var journalCharge = new JobRevenueJournalCharge(Journal);
			AssertEquals("Journal.Lines.Count", 2, Journal.Lines.Count);
			AssertEquals("JournalLines[0].DebitCreditSign", "DR", Journal.JournalLines[0].DebitCreditSign);
			AssertEquals("JournalLines[1].DebitCreditSign", "CR", Journal.JournalLines[1].DebitCreditSign);
			AssertEquals("JournalLines[0].CostRevenueType", TransactionLineTypes.Cost, Journal.JournalLines[0].CostRevenueType);
			AssertEquals("JournalLines[1].CostRevenueType", TransactionLineTypes.Revenue, Journal.JournalLines[1].CostRevenueType);
			AssertEquals("BranchPKFrom", Journal.BranchPKFrom, journalCharge.BranchPKFrom);
			AssertEquals("BranchPKTo", Journal.BranchPKTo, journalCharge.BranchPKTo);
			AssertEquals("DepartmentPKFrom", Journal.DepartmentPKFrom, journalCharge.DepartmentPKFrom);
			AssertEquals("DepartmentPKTo", Journal.DepartmentPKTo, journalCharge.DepartmentPKTo);
			AssertEquals("Share", Journal.DefaultSharing, journalCharge.Share);
			AssertNull("Job", journalCharge.Job);

			var job = Factory.NewJobForTesting<Job>();
			journalCharge = new JobRevenueJournalCharge(Journal, job);
			AssertEquals("Journal.Lines.Count", 4, Journal.Lines.Count);
			AssertEquals("JournalLines[0].DebitCreditSign", "DR", Journal.JournalLines[0].DebitCreditSign);
			AssertEquals("JournalLines[1].DebitCreditSign", "CR", Journal.JournalLines[1].DebitCreditSign);
			AssertEquals("JournalLines[0].CostRevenueType", TransactionLineTypes.Cost, Journal.JournalLines[0].CostRevenueType);
			AssertEquals("JournalLines[1].CostRevenueType", TransactionLineTypes.Revenue, Journal.JournalLines[1].CostRevenueType);
			AssertEquals("BranchPKFrom", Journal.BranchPKFrom, journalCharge.BranchPKFrom);
			AssertEquals("BranchPKTo", Journal.BranchPKTo, journalCharge.BranchPKTo);
			AssertEquals("DepartmentPKFrom", Journal.DepartmentPKFrom, journalCharge.DepartmentPKFrom);
			AssertEquals("DepartmentPKTo", Journal.DepartmentPKTo, journalCharge.DepartmentPKTo);
			AssertEquals("Share", Journal.DefaultSharing, journalCharge.Share);
			AssertEquals("Job", job, journalCharge.Job);
		}

		public void TestDelete()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			AssertEquals("Precondition: Journal.Lines.Count", 2, Journal.Lines.Count);
			BusinessObject line1 = Journal.Lines[0];
			BusinessObject line2 = Journal.Lines[0];
			journalCharge.Delete();
			AssertEquals("Journal.Lines.Count", 0, Journal.Lines.Count);
			AssertEquals("line1.IsDeleted", true, line1.IsDeleted);
			AssertEquals("line2.IsDeleted", true, line2.IsDeleted);
		}

		public void TestDetach()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			AssertEquals("Precondition: Journal.Lines.Count", 2, Journal.Lines.Count);
			journalCharge.Detach();
			journalCharge.Delete();
			AssertEquals("Journal.Lines.Count", 2, Journal.Lines.Count);
		}

		public void TestJob()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			Job job = Factory.NewJobForTesting<Job>();
			journalCharge.Job = job;
			AssertEquals("Job", job, journalCharge.Job);
			AssertEquals("JournalLines[0].AL_JH", job.PK, Journal.JournalLines[0].AL_JH);
			AssertEquals("JournalLines[1].AL_JH", job.PK, Journal.JournalLines[1].AL_JH);

			journalCharge.Job = null;
			AssertNull("Job", journalCharge.Job);

			journalCharge.Detach();
			journalCharge.Job = job;
			AssertEquals("Job", job, journalCharge.Job);
		}

		public void TestBranchPKFrom()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			ZGuid expectedValue = ZGuid.NewZGuid();
			journalCharge.BranchPKFrom = expectedValue;
			AssertEquals("journalCharge.BranchPKFrom", expectedValue, journalCharge.BranchPKFrom);
			AssertEquals("JournalLines[0].AL_GB", expectedValue, Journal.JournalLines[0].AL_GB);

			journalCharge.Detach();
			AssertEquals("journalCharge.BranchPKFrom", ZGuid.Empty, journalCharge.BranchPKFrom);
			journalCharge.BranchPKFrom = ZGuid.NewZGuid();
			AssertEquals("journalCharge.BranchPKFrom", ZGuid.Empty, journalCharge.BranchPKFrom);
		}

		public void TestBranchPKTo()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			ZGuid expectedValue = ZGuid.NewZGuid();
			journalCharge.BranchPKTo = expectedValue;
			AssertEquals("journalCharge.BranchPKTo", expectedValue, journalCharge.BranchPKTo);
			AssertEquals("JournalLines[1].AL_GB", expectedValue, Journal.JournalLines[1].AL_GB);

			journalCharge.Detach();
			AssertEquals("journalCharge.BranchPKTo", ZGuid.Empty, journalCharge.BranchPKTo);
			journalCharge.BranchPKTo = ZGuid.NewZGuid();
			AssertEquals("journalCharge.BranchPKTo", ZGuid.Empty, journalCharge.BranchPKTo);
		}

		public void TestDepartmentPKFrom()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			ZGuid expectedValue = ZGuid.NewZGuid();
			journalCharge.DepartmentPKFrom = expectedValue;
			AssertEquals("journalCharge.DepartmentPKFrom", expectedValue, journalCharge.DepartmentPKFrom);
			AssertEquals("JournalLines[0].AL_GE", expectedValue, Journal.JournalLines[0].AL_GE);

			journalCharge.Detach();
			AssertEquals("journalCharge.DepartmentPKFrom", ZGuid.Empty, journalCharge.DepartmentPKFrom);
			journalCharge.DepartmentPKFrom = ZGuid.NewZGuid();
			AssertEquals("journalCharge.DepartmentPKFrom", ZGuid.Empty, journalCharge.DepartmentPKFrom);
		}

		public void TestDepartmentPKTo()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			ZGuid expectedValue = ZGuid.NewZGuid();
			journalCharge.DepartmentPKTo = expectedValue;
			AssertEquals("journalCharge.DepartmentPKTo", expectedValue, journalCharge.DepartmentPKTo);
			AssertEquals("JournalLines[1].AL_GE", expectedValue, Journal.JournalLines[1].AL_GE);

			journalCharge.Detach();
			AssertEquals("journalCharge.DepartmentPKTo", ZGuid.Empty, journalCharge.DepartmentPKTo);
			journalCharge.DepartmentPKTo = ZGuid.NewZGuid();
			AssertEquals("journalCharge.DepartmentPKTo", ZGuid.Empty, journalCharge.DepartmentPKTo);
		}

		public void TestCostRevenueTypeFrom()
		{
			var journalCharge = new JobRevenueJournalCharge(Journal);
			var expectedValue = TransactionLineTypes.Cost;
			journalCharge.CostRevenueTypeFrom = expectedValue;
			AssertEquals(expectedValue, journalCharge.CostRevenueTypeFrom);
			AssertEquals(DebitCreditDataEntry.DR, Journal.JournalLines[0].DebitCreditSign);
			AssertEquals(expectedValue, Journal.JournalLines[0].CostRevenueType);

			journalCharge.Detach();
			AssertEquals(ZString.Empty, journalCharge.CostRevenueTypeFrom);
			journalCharge.CostRevenueTypeFrom = TestObjectCreator.GetRandomString(3);
			AssertEquals(ZString.Empty, journalCharge.CostRevenueTypeFrom);
		}

		public void TestCostRevenueTypeTo()
		{
			var journalCharge = new JobRevenueJournalCharge(Journal);
			var expectedValue = TransactionLineTypes.Cost;
			journalCharge.CostRevenueTypeTo = expectedValue;
			AssertEquals(expectedValue, journalCharge.CostRevenueTypeTo);
			AssertEquals(DebitCreditDataEntry.CR, Journal.JournalLines[1].DebitCreditSign);
			AssertEquals(expectedValue, Journal.JournalLines[1].CostRevenueType);

			journalCharge.Detach();
			AssertEquals(ZString.Empty, journalCharge.CostRevenueTypeTo);
			journalCharge.CostRevenueTypeTo = TestObjectCreator.GetRandomString(3);
			AssertEquals(ZString.Empty, journalCharge.CostRevenueTypeTo);
		}

		public void TestSettingCostRevenueTypeToAndFromInsideConstructorDoesNotShowSecurityError()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var journal = Factory.New<JobRevenueJournal>();
			journal.ActivateSimpleEntry(job);

			var invoicingLevelSecurity = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostRevenueType);
			invoicingLevelSecurity.IsAllowed = false;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var charge = journal.JournalCharges.AddNew();
			AssertNull("Should not show security error when setting values inside constructor", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(TransactionLineTypes.Revenue, charge.CostRevenueTypeTo);
			AssertEquals(TransactionLineTypes.Revenue, charge.CostRevenueTypeFrom);
		}

		public void TestChargeCode()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			ZGuid expectedValue = ZGuid.NewZGuid();
			journalCharge.ChargeCode = expectedValue;
			AssertEquals("journalCharge.ChargeCode", expectedValue, journalCharge.ChargeCode);
			AssertEquals("JournalLines[0].AL_AC", expectedValue, Journal.JournalLines[0].AL_AC);
			AssertEquals("JournalLines[1].AL_AC", expectedValue, Journal.JournalLines[1].AL_AC);

			journalCharge.Detach();
			AssertEquals("journalCharge.ChargeCode", ZGuid.Empty, journalCharge.ChargeCode);
			journalCharge.ChargeCode = ZGuid.NewZGuid();
			AssertEquals("journalCharge.ChargeCode", ZGuid.Empty, journalCharge.ChargeCode);
		}

		public void TestChargeCodeInfo()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			journalCharge.ChargeCode = ZGuid.Empty;
			AssertHasErrors(journalCharge.ChargeCodeInfo);

			journalCharge.Detach();
			AssertNoErrors(journalCharge.ChargeCodeInfo);
		}

		[ExpectNoExceptions]
		public void TestChargeCodeList()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			journalCharge.Detach();
			AccChargeCodeCollection list = journalCharge.ChargeCodeList;
		}

		public void TestShare()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			journalCharge.RunPreSaveValidation();
			AssertEquals("Precondtition: Share is zero.", ZDecimal.Zero, journalCharge.Share);
			AssertNoErrors(journalCharge.ShareInfo);

			journalCharge.Share = -1M;
			AssertHasError(journalCharge.ShareInfo, "value cannot be negative.");

			journalCharge.Share = 1M;
			AssertNoErrors(journalCharge.ShareInfo);

			journalCharge.BillingTabOsAmount = 200M;
			journalCharge.Share = 33M;
			AssertEquals("SharedOsAmount should be recalculated.", 66M, journalCharge.SharedOsAmount);
			AssertEquals("BillingTabOsAmount should stay unchanged.", 200M, journalCharge.BillingTabOsAmount);

			journalCharge.Share = 0M;
			AssertEquals("SharedOsAmount should be recalculated.", 0M, journalCharge.SharedOsAmount);
			AssertEquals("BillingTabOsAmount should stay unchanged.", 200M, journalCharge.BillingTabOsAmount);
			AssertNoErrors(journalCharge.ShareInfo);
		}

		public void TestBillingTabOsAmount()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			journalCharge.RunPreSaveValidation();
			AssertEquals("Precondtition: BillingTabOsAmount is zero.", ZDecimal.Zero, journalCharge.BillingTabOsAmount);
			AssertNoErrors(journalCharge.BillingTabOsAmountInfo);

			journalCharge.BillingTabOsAmount = -1M;
			AssertHasError(journalCharge.BillingTabOsAmountInfo, "value cannot be negative.");

			journalCharge.BillingTabOsAmount = 1M;
			AssertNoErrors(journalCharge.BillingTabOsAmountInfo);

			journalCharge.Share = 33M;
			journalCharge.ExchangeRate = 2M;
			journalCharge.BillingTabOsAmount = 200M;
			AssertEquals("SharedOsAmount should be recalculated.", 66M, journalCharge.SharedOsAmount);
			AssertEquals("BillingTabLocalAmount should be recalculated.", 100M, journalCharge.BillingTabLocalAmount);
			AssertEquals("Share should stay unchanged.", 33M, journalCharge.Share);

			journalCharge.BillingTabOsAmount = 0M;
			AssertEquals("SharedOsAmount should be recalculated.", 0M, journalCharge.SharedOsAmount);
			AssertEquals("BillingTabLocalAmount should be recalculated.", 0M, journalCharge.BillingTabLocalAmount);
			AssertEquals("Share should stay unchanged.", 33M, journalCharge.Share);
			AssertNoErrors(journalCharge.BillingTabOsAmountInfo);
		}

		public void TestBillingTabLocalAmount()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			journalCharge.RunPreSaveValidation();
			AssertEquals("Precondtition: BillingTabLocalAmount is zero.", ZDecimal.Zero, journalCharge.BillingTabLocalAmount);
			AssertNoErrors(journalCharge.BillingTabLocalAmountInfo);

			journalCharge.BillingTabLocalAmount = -1M;
			AssertHasError(journalCharge.BillingTabLocalAmountInfo, "value cannot be negative.");

			journalCharge.BillingTabLocalAmount = 1M;
			AssertNoErrors(journalCharge.BillingTabLocalAmountInfo);

			journalCharge.Share = 33M;
			journalCharge.ExchangeRate = 2M;
			journalCharge.BillingTabLocalAmount = 200M;
			AssertEquals("SharedOsAmount should be recalculated.", 132M, journalCharge.SharedOsAmount);
			AssertEquals("BillingTabOsAmount should be recalculated.", 400M, journalCharge.BillingTabOsAmount);
			AssertEquals("Share should stay unchanged.", 33M, journalCharge.Share);

			journalCharge.BillingTabLocalAmount = 0M;
			AssertEquals("SharedOsAmount should be recalculated.", 0M, journalCharge.SharedOsAmount);
			AssertEquals("BillingTabOsAmount should be recalculated.", 0M, journalCharge.BillingTabOsAmount);
			AssertEquals("Share should stay unchanged.", 33M, journalCharge.Share);
			AssertNoErrors(journalCharge.BillingTabLocalAmountInfo);

			journalCharge.Currency = "";
			journalCharge.BillingTabOsAmount = 33M;
			journalCharge.BillingTabLocalAmount = 55M;
			AssertEquals("SharedOsAmount should stay unchanged", 10.89M, journalCharge.SharedOsAmount);
			AssertEquals("BillingTabOsAmount should stay unchanged.", 33M, journalCharge.BillingTabOsAmount);
			AssertEquals("Share should stay unchanged.", 33M, journalCharge.Share);
		}

		public void TestSharedOsAmount()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			ZDecimal expectedValue = 200M;
			journalCharge.SharedOsAmount = expectedValue;
			AssertEquals("journalCharge.SharedOsAmount", expectedValue, journalCharge.SharedOsAmount);
			AssertEquals("JournalLines[0].OSUnsignedLineAmount", expectedValue, Journal.JournalLines[0].OSUnsignedLineAmount);
			AssertEquals("JournalLines[1].OSUnsignedLineAmount", expectedValue, Journal.JournalLines[1].OSUnsignedLineAmount);

			journalCharge.Share = 33M;
			journalCharge.ExchangeRate = 2M;
			journalCharge.BillingTabOsAmount = 300M;
			journalCharge.SharedOsAmount = 30M;
			AssertEquals("Share should be recalculated.", 10M, journalCharge.Share);
			AssertEquals("SharedLocalAmount should be recalculated.", 15M, journalCharge.SharedLocalAmount);
			AssertEquals("BillingTabOsAmount should stay unchanged.", 300M, journalCharge.BillingTabOsAmount);

			journalCharge.SharedOsAmount = 0M;
			AssertEquals("Share should be recalculated.", 0M, journalCharge.Share);
			AssertEquals("SharedLocalAmount should be recalculated.", 0M, journalCharge.SharedLocalAmount);
			AssertEquals("BillingTabOsAmount should stay unchanged.", 300M, journalCharge.BillingTabOsAmount);

			journalCharge.Detach();
			AssertEquals("journalCharge.SharedOsAmount", 0M, journalCharge.SharedOsAmount);
			AssertEquals("BillingTabOsAmount should stay unchanged.", 300M, journalCharge.BillingTabOsAmount);
			journalCharge.SharedOsAmount = 10M;
			AssertEquals("journalCharge.SharedOsAmount", 0M, journalCharge.SharedOsAmount);
			AssertEquals("BillingTabOsAmount should stay unchanged.", 300M, journalCharge.BillingTabOsAmount);
		}

		public void TestSharedOsAmountInfo()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			journalCharge.SharedOsAmount = 0M;
			AssertHasErrors(journalCharge.SharedOsAmountInfo);

			journalCharge.Detach();
			AssertNoErrors(journalCharge.SharedOsAmountInfo);
		}

		public void TestSharedLocalAmount()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			ZDecimal expectedValue = 200M;
			journalCharge.SharedLocalAmount = expectedValue;
			AssertEquals("journalCharge.SharedLocalAmount", expectedValue, journalCharge.SharedLocalAmount);
			AssertEquals("JournalLines[0].LocalUnsignedLineAmount", expectedValue, Journal.JournalLines[0].LocalUnsignedLineAmount);
			AssertEquals("JournalLines[1].LocalUnsignedLineAmount", expectedValue, Journal.JournalLines[1].LocalUnsignedLineAmount);

			journalCharge.Share = 33M;
			journalCharge.ExchangeRate = 2M;
			journalCharge.BillingTabOsAmount = 300M;
			journalCharge.SharedLocalAmount = 30M;
			AssertEquals("Share should be recalculated.", 20M, journalCharge.Share);
			AssertEquals("SharedOsAmount should be recalculated.", 60M, journalCharge.SharedOsAmount);
			AssertEquals("BillingTabOsAmount should stay unchanged.", 300M, journalCharge.BillingTabOsAmount);

			journalCharge.SharedLocalAmount = 0M;
			AssertEquals("Share should be recalculated.", 0M, journalCharge.Share);
			AssertEquals("SharedOsAmount should be recalculated.", 0M, journalCharge.SharedOsAmount);
			AssertEquals("BillingTabOsAmount should stay unchanged.", 300M, journalCharge.BillingTabOsAmount);

			journalCharge.Detach();
			AssertEquals("journalCharge.SharedLocalAmount", 0M, journalCharge.SharedLocalAmount);
			AssertEquals("BillingTabOsAmount should stay unchanged.", 300M, journalCharge.BillingTabOsAmount);
			journalCharge.SharedLocalAmount = 10M;
			AssertEquals("journalCharge.SharedLocalAmount", 0M, journalCharge.SharedLocalAmount);
			AssertEquals("BillingTabOsAmount should stay unchanged.", 300M, journalCharge.BillingTabOsAmount);
		}

		public void TestSharedLocalAmountInfo()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			journalCharge.SharedLocalAmount = 0M;
			AssertHasErrors(journalCharge.SharedLocalAmountInfo);

			journalCharge.Detach();
			AssertNoErrors(journalCharge.SharedLocalAmountInfo);
		}

		public void TestCurrency()
		{
			Job job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 0.5M);
			job.Factory.Save();
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal, job);
			journalCharge.BillingTabOsAmount = 150M;
			journalCharge.SharedOsAmount = 30M;
			ZString expectedValue = TestObjectCreator.USD.RX_Code;
			journalCharge.Currency = expectedValue;
			AssertEquals("Precondition: journalCharge.ExchangeRate", 0.5M, journalCharge.ExchangeRate);
			AssertEquals("journalCharge.Currency", expectedValue, journalCharge.Currency);
			AssertEquals("JournalLines[0].AL_RX_NKTransactionCurrency", expectedValue, Journal.JournalLines[0].AL_RX_NKTransactionCurrency);
			AssertEquals("JournalLines[1].AL_RX_NKTransactionCurrency", expectedValue, Journal.JournalLines[1].AL_RX_NKTransactionCurrency);
			AssertEquals("journalCharge.BillingTabOsAmount should stay unchanged.", 150M, journalCharge.BillingTabOsAmount);
			AssertEquals("journalCharge.BillingTabLocalAmount should be recalculated.", 300M, journalCharge.BillingTabLocalAmount);
			AssertEquals("journalCharge.SharedOsAmount should stay unchanged.", 30M, journalCharge.SharedOsAmount);
			AssertEquals("journalCharge.SharedLocalAmount should be recalculated.", 60M, journalCharge.SharedLocalAmount);

			journalCharge.Detach();
			AssertEquals("journalCharge.Currency", "", journalCharge.Currency);
			journalCharge.Currency = TestObjectCreator.AUD.RX_Code;
			AssertEquals("journalCharge.Currency", "", journalCharge.Currency);
		}

		public void TestCurrencyInfo()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			journalCharge.ExchangeRate = 1M; //To call TransactionLine ExchangeRate property to add additional validation for AL_RX_NKTransactionCurrency
			journalCharge.Currency = "";
			AssertHasErrors(journalCharge.CurrencyInfo);

			journalCharge.Detach();
			AssertNoErrors(journalCharge.CurrencyInfo);
		}

		public void TestExchangeRate()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			journalCharge.BillingTabOsAmount = 150M;
			journalCharge.SharedOsAmount = 30M;
			journalCharge.Currency = TestObjectCreator.USD.RX_Code;
			ZDecimal expectedValue = 3M;
			journalCharge.ExchangeRate = expectedValue;
			AssertEquals("journalCharge.ExchangeRate", expectedValue, journalCharge.ExchangeRate);
			AssertEquals("JournalLines[0].ExchangeRate.Rate", expectedValue, Journal.JournalLines[0].ExchangeRate.Rate);
			AssertEquals("JournalLines[1].ExchangeRate.Rate", expectedValue, Journal.JournalLines[1].ExchangeRate.Rate);
			AssertEquals("journalCharge.BillingTabOsAmount should stay unchanged.", 150M, journalCharge.BillingTabOsAmount);
			AssertEquals("journalCharge.BillingTabLocalAmount should be recalculated.", 50M, journalCharge.BillingTabLocalAmount);
			AssertEquals("journalCharge.SharedOsAmount should stay unchanged.", 30M, journalCharge.SharedOsAmount);
			AssertEquals("journalCharge.SharedLocalAmount should be recalculated.", 10M, journalCharge.SharedLocalAmount);

			journalCharge.Detach();
			AssertEquals("journalCharge.Currency", 0M, journalCharge.ExchangeRate);
			journalCharge.ExchangeRate = 10M;
			AssertEquals("journalCharge.Currency", 0M, journalCharge.ExchangeRate);
		}

		public void TestFieldsReadOnly()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			Journal.FillWithValidTestData();
			Journal.Lines[0].FillWithValidTestData();
			Journal.Lines[1].FillWithValidTestData();
			Journal.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;
			Journal.Lines[1].AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			AssertEquals("ChargeCodeInfo.ReadOnly", true, journalCharge.ChargeCodeInfo.ReadOnly);
			AssertEquals("ShareInfo.ReadOnly", true, journalCharge.ShareInfo.ReadOnly);
			AssertEquals("BillingTabOsAmountInfo.ReadOnly", true, journalCharge.BillingTabOsAmountInfo.ReadOnly);
			AssertEquals("BillingTabLocalAmountInfo.ReadOnly", true, journalCharge.BillingTabLocalAmountInfo.ReadOnly);
			AssertEquals("SharedOsAmountInfo.ReadOnly", true, journalCharge.SharedOsAmountInfo.ReadOnly);
			AssertEquals("SharedLocalAmountInfo.ReadOnly", true, journalCharge.SharedLocalAmountInfo.ReadOnly);
			AssertEquals("CurrencyInfo.ReadOnly", true, journalCharge.CurrencyInfo.ReadOnly);
			AssertEquals("ExchangeRateInfo.ReadOnly", true, journalCharge.ExchangeRateInfo.ReadOnly);
		}

		public void TestExchangeRateInfo()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			journalCharge.ExchangeRate = 0M;
			AssertHasErrors(journalCharge.ExchangeRateInfo);

			journalCharge.Detach();
			AssertNoErrors(journalCharge.ExchangeRateInfo);
		}

		public void TestLocalDecimals()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			ZInt defaultValue = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			try
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
				AssertEquals("Decimals should be 0", 0, journalCharge.LocalDecimals);
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;
				AssertEquals("Decimals should be 2", 2, journalCharge.LocalDecimals);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = defaultValue;
			}
		}

		public void TestCurrencyDecimals()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			ZInt defaultValue = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			try
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 1000;
				AssertEquals("Precondition: LocalDecimals should be 3", 3, journalCharge.LocalDecimals);

				journalCharge.Currency = string.Empty;
				AssertEquals("Should be 3 when no currency specified", 3, journalCharge.CurrencyDecimals);

				journalCharge.Currency = "USD";
				AssertEquals("Decimals should be 2", 2, journalCharge.CurrencyDecimals);

				journalCharge.Currency = "IDR";
				AssertEquals("Decimals should be 0", 0, journalCharge.CurrencyDecimals);

				journalCharge.Detach();
				AssertEquals("Decimals should be as local decimals", 3, journalCharge.CurrencyDecimals);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = defaultValue;
			}
		}

		public void TestShareDecimals()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			AssertEquals("ShareDecimals", 2, journalCharge.ShareDecimals);
		}

		public void TestExchangeRateDecimals()
		{
			JobRevenueJournalCharge journalCharge = new JobRevenueJournalCharge(Journal);
			var origIsReciprocal = TestObjectCreator.SetCurrentCompanyReciprocal(true);
			try
			{
				AssertEquals("Number of ex rate decimal places = 6", 6, journalCharge.ExchangeRateDecimalPlaces);
				TestObjectCreator.SetCurrentCompanyReciprocal(false);
				AssertEquals("Number of ex rate decimal places = 6", 6, journalCharge.ExchangeRateDecimalPlaces);
				journalCharge.Detach();
				AssertEquals("Number of ex rate decimal places = 6", 6, journalCharge.ExchangeRateDecimalPlaces);
			}
			finally
			{
				TestObjectCreator.SetCurrentCompanyReciprocal(origIsReciprocal);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobRevenueJournalCharge(Journal);
		}

		JobRevenueJournal Journal
		{
			get { return Journal_cached ?? (Journal_cached = Factory.New<JobRevenueJournal>()); }
		}
		JobRevenueJournal Journal_cached;

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;
	}
}
