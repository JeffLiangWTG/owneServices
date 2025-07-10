using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Journal;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Business.JobInvoicing.Journal.Testing
{
	public class AutoJobRevenueJournalCreatorTest : TestCaseWithFactory
	{
		public void TestCreateJRJFromSellDoesNotConsumeApportionedAccrual()
		{
			var sellCharge = CreateSellCharge();
			var accrualCharge = CreateAccrualCharge(false, true, sellCharge);
			accrualCharge.JR_E6 = ZGuid.NewZGuid();
			Assert(accrualCharge.JR_IsApportioned);

			AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(sellCharge);
			var journal = sellCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).First();
			var sellLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == sellCharge.JR_AL_ARLine);
			var costLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK != sellLine.PK);
			var costCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, costLine.PK));
			AssertNotEquals("Expect NOT to use the existing accrual charge", accrualCharge, costCharge);
			AssertEquals(50m, costCharge.JR_OSCostAmt);
			AssertEquals(100m, costCharge.JR_LocalCostAmt);
			Assert(costCharge.IsCostPosted);
			Assert(!accrualCharge.IsCostPosted);
		}

		public void TestSuspendJournalValidationAndJournalLinesListChangedWhenCreatedJournal()
		{
			var autoJRJCreatorCacheService_ForTestOnly = FactoryLevelPropertyStorageForTests.GetOrCreateNewInstance(Factory);

			var sellCharge = CreateSellCharge();
			CreateAccrualCharge(false, true, sellCharge);

			AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(sellCharge);
			var journal = sellCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).First();
			var sellLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == sellCharge.JR_AL_ARLine);

			var lineIsValidationSuspended = autoJRJCreatorCacheService_ForTestOnly.GetValueFromElement(sellLine.PK, "AddJournalLineSetValuesAndLinkToCharge_LineIsValidationSuspended");
			var journalIsValidationSuspended = autoJRJCreatorCacheService_ForTestOnly.GetValueFromElement(journal.PK, "AddJournalLineSetValuesAndLinkToCharge_JournalIsValidationSuspended");
			var journalIsValidationSuspendedInCreateJournal = autoJRJCreatorCacheService_ForTestOnly.GetValueFromElement(journal.PK, "CreateJournal_JournalIsValidationSuspended");
			var journalLinesIsListChangedSuspended = autoJRJCreatorCacheService_ForTestOnly.GetValueFromElement(journal.PK, "AddJournalLineSetValuesAndLinkToCharge_JournalLinesIsListChangedSuspended");

			Assert("Journal should be suspended validation in AddJournalLineSetValuesAndLinkToCharge_JournalIsValidationSuspended", (bool)journalIsValidationSuspended);
			Assert("Journal should be suspended validation in CreateJournal", (bool)journalIsValidationSuspendedInCreateJournal);
			Assert("Journal line should be suspended validation", (bool)lineIsValidationSuspended);
			Assert("Journal lines should be suspended list changed", (bool)journalLinesIsListChangedSuspended);
		}

		public void TestCreateJRJFromSell_WhenFirstJobIsNotGatewayBillingJob_ThenFirstChargeRevenueCalculationDescriptionIsNotCopied()
		{
			var sellCharge = CreateSellCharge();
			sellCharge.RevenueCalculationDescription = ZBlob.FromUTF8("testRevenueCalculationDescription");
			AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(sellCharge);
			var journal = sellCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).First();
			var sellLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == sellCharge.JR_AL_ARLine);
			var costLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK != sellLine.PK);
			var costCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, costLine.PK));
			Assert(!sellCharge.Job.IsGatewayBillingJob());
			AssertNullOrEmptyOrWhitespace(costCharge.CostCalculationDescription.ToUTF8());
			AssertNullOrEmptyOrWhitespace(costCharge.RevenueCalculationDescription.ToUTF8());
		}

		public void TestCreateJRJFromSell_WhenFirstJobIsGatewayBillingJobAndIsUseSellValues_ThenFirstChargeRevenueCalculationDescriptionIsCopied()
		{
			var sellCharge = CreateGatewayBillingCharge();
			Assert(sellCharge.Job.IsGatewayBillingJob());
			sellCharge.RevenueCalculationDescription = ZBlob.FromUTF8("testRevenueCalculationDescription");
			AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(sellCharge);
			var journal = sellCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).First();
			var sellLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == sellCharge.JR_AL_ARLine);
			var costLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK != sellLine.PK);
			var costCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, costLine.PK));
			AssertContains(sellCharge.RevenueCalculationDescription.ToUTF8(), costCharge.CostCalculationDescription.ToUTF8());
			AssertNullOrEmptyOrWhitespace(costCharge.RevenueCalculationDescription.ToUTF8());
		}

		public void TestCreateJRJFromSell_WhenFirstJobIsGatewayBillingJobIsUseCostValues_ThenFirstChargeRevenueCalculationDescriptionIsNotCopied()
		{
			var costCharge = CreateGatewayBillingCharge();

			Assert(costCharge.Job.IsGatewayBillingJob());
			costCharge.CostCalculationDescription = ZBlob.FromUTF8("testCostCalculationDescription");
			AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(costCharge);
			var journal = costCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).First();
			var costLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == costCharge.JR_AL_APLine);
			var sellLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK != costLine.PK);
			var sellCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, sellLine.PK));
			AssertNullOrEmptyOrWhitespace(sellCharge.CostCalculationDescription.ToUTF8());
			AssertNullOrEmptyOrWhitespace(sellCharge.RevenueCalculationDescription.ToUTF8());
		}

		public ChargeWithCost CreateGatewayBillingCharge()
		{
			ChargeWithCost gatewayBillingCharge;
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var setup = testObjectCreator.CreateGatewayConsolsAndShipments();
				Factory.Save();

				using (var gatewayJob = testObjectCreator.CreateJob(setup.gC0001))
				using (var shipmentJob = testObjectCreator.CreateJob(setup.s0001))
				{
					var sourceJob = gatewayJob;
					gatewayBillingCharge = sourceJob.Charges.AddNew();
					gatewayBillingCharge.JR_AC = chargeCode1.PK;
					gatewayBillingCharge.JR_Desc = "charge code description";
					gatewayBillingCharge.JR_JH = gatewayJob.PK;
					gatewayBillingCharge.JR_GB = testObjectCreator.CreateBranch("111", GlbCompany.CurrentCompany).PK;
					gatewayBillingCharge.JR_GE = testObjectCreator.FESDepartment.PK;
					gatewayBillingCharge.JR_JH_InternalJob = shipmentJob.PK;
					gatewayBillingCharge.JR_GB_InternalBranch = testObjectCreator.CreateBranch("222", GlbCompany.CurrentCompany).PK;
					gatewayBillingCharge.JR_GE_InternalDept = testObjectCreator.FISDepartment.PK;
					gatewayBillingCharge.JR_RX_NKCostCurrency = testObjectCreator.USD.RX_Code;
					gatewayBillingCharge.JR_RX_NKSellCurrency = testObjectCreator.USD.RX_Code;

					gatewayBillingCharge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
					gatewayBillingCharge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);

					gatewayBillingCharge.JR_OSSellAmt = 50m;
					gatewayBillingCharge.JR_LocalSellAmt = 100m;
					gatewayBillingCharge.JR_OH_SellAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
					gatewayBillingCharge.JR_OSSellExRate = 0.5;

					gatewayBillingCharge.JR_OSCostAmt = 50m;
					gatewayBillingCharge.JR_LocalCostAmt = 100m;
					gatewayBillingCharge.JR_OH_CostAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
					gatewayBillingCharge.JR_OSCostExRate = 0.5;
				}
			}
			return gatewayBillingCharge;
		}

		public void TestSuspendSecondChargeValidationWhenCanFoundMatchedAcrualOnSaving()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var sellCharge = CreateSellCharge();
				var shipment1 = testObjectCreator.CreateShipment("S00003333");
				var job3 = testObjectCreator.CreateJob(shipment1, false, false);
				sellCharge.JR_GB = GlbBranch.CurrentBranch.PK;
				sellCharge.JR_GE = testObjectCreator.FISDepartment.PK;
				sellCharge.JR_JH = job3.PK;
				sellCharge.JR_GB_InternalBranch = testObjectCreator.NonCurrentBranch.PK;
				sellCharge.JR_GE_InternalDept = testObjectCreator.FESDepartment.PK;
				sellCharge.JR_JH_InternalJob = job2.PK;
				sellCharge.JR_OSSellAmt = 922337203685478m;

				var accrualCharge = Factory.New<Charge>();
				accrualCharge.JR_AC = sellCharge.JR_AC;
				accrualCharge.JR_Desc = "charge code description";
				accrualCharge.JR_JH = job2.PK;
				accrualCharge.JR_GB = sellCharge.JR_GB_InternalBranch;
				accrualCharge.JR_GE = sellCharge.JR_GE_InternalDept;
				accrualCharge.JR_RX_NKCostCurrency = testObjectCreator.USD.RX_Code;
				accrualCharge.JR_OH_CostAccount = ZGuid.Empty;

				try
				{
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					var msg = ex.Message;
					var expectedMsg = "Inner Message = Error converting data type numeric to money";
					AssertContains("Should have this exception message, but we don't care in this UT. ", expectedMsg, msg);
				}

				AssertNoErrors("We suspend validation to improve performance as the creation of Job Revenue Journal on factory saving where we do not care about validation errors.", accrualCharge.JR_OSCostAmtInfo);
				accrualCharge.Validation.ValidateJR_OSCostAmt();
				AssertHasErrors("This proofs that the second charge has validation error if validation is not suspended.", accrualCharge.JR_OSCostAmtInfo);
			}
		}

		public void TestSuspendSecondChargeValidationWhenCannotFoundMatchedAcrualOnSaving()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var sellCharge = CreateSellCharge();
				var branch = testObjectCreator.CreateBranch("333", GlbCompany.CurrentCompany);
				sellCharge.JR_GB_InternalBranch = branch.PK;
				sellCharge.JR_OSSellAmt = 922337203685478.5807m;

				try
				{
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					var msg = ex.Message;
					var expectedMsg = "Inner Message = Error converting data type numeric to money";
					AssertContains("Should have this exception message, but we don't care in this UT. ", expectedMsg, msg);
				}

				var secondChargeJob = sellCharge.Factory.Load<Job>(sellCharge.JR_JH_InternalJob);
				var secondCharge = secondChargeJob.Charges[0];
				AssertNoErrors("We suspend validation to improve performance as the creation of Job Revenue Journal on factory saving where we do not care about validation errors.", secondCharge.JR_OSCostAmtInfo);
				secondCharge.Validation.ValidateJR_OSCostAmt();
				AssertHasErrors("This proofs that the second charge has validation error if validation is not suspended.", secondCharge.JR_OSCostAmtInfo);
			}
		}

		public void TestNotValidateInternalJobPropertiesForSecondChargeOnSaving()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var sellCharge = CreateSellCharge();

				sellCharge.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
				Factory.Save();

				var secondChargeJob = sellCharge.Factory.Load<Job>(sellCharge.JR_JH_InternalJob);
				var secondCharge = secondChargeJob.Charges[0];

				AssertHasWarnings("It should has warning when JR_GB_InternalBranch is empty", secondCharge.JR_GB_InternalBranchInfo);
				AssertHasWarnings("It should has warning when JR_GE_InternalDept is empty", secondCharge.JR_GE_InternalDeptInfo);
				AssertNoWarnings("It should has no warning when JR_GB_InternalBranch is empty", secondCharge.JR_JH_InternalJobInfo);
			}
		}

		public void TestSuspendJournalValidationAndJournalLinesListChangedWhenCreateJournalWithGatewayBillingSellCharge()
		{
			var autoJRJCreatorCacheService_ForTestOnly = FactoryLevelPropertyStorageForTests.GetOrCreateNewInstance(Factory);

			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true;
			AssertCreateAutoJRJWithCostChargeGatewayBatch("Always allow to creat JRJ when has security right", JobHeaderStatus.Codes.Working, false);

			var journal = Factory.Load<JobRevenueJournal>(new ZQuery()).First();
			ShipmentCharge.TryFindGatewaySellAndCostsFromApportionedCost(out var gatewaySellAndCosts);
			var costLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == gatewaySellAndCosts.costCharges[0].JR_AL_APLine);

			var lineIsValidationSuspended = autoJRJCreatorCacheService_ForTestOnly.GetValueFromElement(costLine.PK, "AddJournalLineSetValuesAndLinkToCharge_LineIsValidationSuspended");
			var journalIsValidationSuspended = autoJRJCreatorCacheService_ForTestOnly.GetValueFromElement(journal.PK, "AddJournalLineSetValuesAndLinkToCharge_JournalIsValidationSuspended");
			var journalIsValidationSuspendedInFindOrCreateGatewayBillingChargeLineAndJournal = autoJRJCreatorCacheService_ForTestOnly.GetValueFromElement(journal.PK, "FindOrCreateGatewayBillingChargeLineAndJournal_JournalIsValidationSuspended");
			var journalLinesIsListChangedSuspended = autoJRJCreatorCacheService_ForTestOnly.GetValueFromElement(journal.PK, "AddJournalLineSetValuesAndLinkToCharge_JournalLinesIsListChangedSuspended");

			Assert("Journal should be suspended validation", (bool)journalIsValidationSuspended);
			Assert("Journal should be suspended validation in FindOrCreateGatewayBillingChargeLineAndJournal method", (bool)journalIsValidationSuspendedInFindOrCreateGatewayBillingChargeLineAndJournal);
			Assert("Journal line should be suspended validation", (bool)lineIsValidationSuspended);
			Assert("Journal lines should be suspended list changed", (bool)journalLinesIsListChangedSuspended);
		}

		public void TestCreateJRJFromSellDoesNotConsumeAccrual()
		{
			AssertCreateJRJFromSellConsumeExistingAccrual("S0001", false, false);
		}

		public void TestCreateJRJFromSellConsumeFirstAccrual()
		{
			AssertCreateJRJFromSellConsumeExistingAccrual("S0002", true, false);
		}

		public void TestCreateJRJFromSellConsumeSecondAccrual()
		{
			AssertCreateJRJFromSellConsumeExistingAccrual("S0003", false, true);
		}

		void AssertCreateJRJFromSellConsumeExistingAccrual(string relatedJobNumber, bool isCharge1Posted, bool isCharge2Posted)
		{
			var setup = testObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();
			var branch = testObjectCreator.CreateBranch("222", GlbCompany.CurrentCompany);
			var consolJobHeader1 = testObjectCreator.CreateJob(setup.gC0001);
			var consolJobHeader2 = testObjectCreator.CreateJob(setup.gC0002);

			var accrualCharge1 = consolJobHeader1.Charges.AddNew();
			accrualCharge1.JR_AC = chargeCode1.PK;
			accrualCharge1.JR_Desc = "charge code description";
			accrualCharge1.JR_OH_CostAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			accrualCharge1.JR_GB = branch.PK;
			accrualCharge1.JR_GE = testObjectCreator.FISDepartment.PK;
			accrualCharge1.JR_RX_NKCostCurrency = testObjectCreator.USD.RX_Code;
			accrualCharge1.JR_OSCostAmt = 100m;
			accrualCharge1.JR_LocalCostAmt = 200m;
			accrualCharge1.JR_Calc_RelatedJobNumber = setup.s0002.JS_UniqueConsignRef;

			var accrualCharge2 = consolJobHeader1.Charges.AddNew();
			accrualCharge2.JR_AC = chargeCode1.PK;
			accrualCharge2.JR_Desc = "charge code description";
			accrualCharge2.JR_OH_CostAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			accrualCharge2.JR_GB = branch.PK;
			accrualCharge2.JR_GE = testObjectCreator.FISDepartment.PK;
			accrualCharge2.JR_RX_NKCostCurrency = testObjectCreator.USD.RX_Code;
			accrualCharge2.JR_OSCostAmt = 200m;
			accrualCharge2.JR_LocalCostAmt = 300m;
			accrualCharge2.JR_Calc_RelatedJobNumber = setup.s0003.JS_UniqueConsignRef;
			Factory.Save();

			var sellCharge = consolJobHeader2.Charges.AddNew();
			sellCharge.JR_AC = chargeCode1.PK;
			sellCharge.JR_JH_InternalJob = consolJobHeader1.PK;
			sellCharge.JR_OSCostAmt = 50M;
			sellCharge.JR_LocalCostAmt = 100M;
			sellCharge.JR_OSSellAmt = 50M;
			sellCharge.JR_LocalSellAmt = 100M;
			sellCharge.JR_OH_SellAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			sellCharge.JR_OSSellExRate = 0.5;
			sellCharge.JR_RX_NKSellCurrency = "USD";
			sellCharge.JR_GB_InternalBranch = branch.PK;
			sellCharge.JR_GE_InternalDept = testObjectCreator.FISDepartment.PK;
			sellCharge.JR_Calc_RelatedJobNumber = relatedJobNumber;
			sellCharge.RunPreSaveValidation();

			AssertEquals(branch.PK, sellCharge.JR_GB_InternalBranch);
			AssertEquals(testObjectCreator.FISDepartment.PK, sellCharge.JR_GE_InternalDept);

			AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(sellCharge);

			AssertEquals(isCharge1Posted, accrualCharge1.IsCostPosted);
			AssertEquals(isCharge2Posted, accrualCharge2.IsCostPosted);
		}

		public void TestCreateJRJFromSellConsumeExistingAccrualWhenCreditorIsBlankAndInDB()
		{
			AssertCreateJournalFromSellChargeConsumeExistingAccrual(true, true);
		}

		public void TestCreateJRJFromSellConsumeExistingAccrualWhenCreditorIsBlankAndInMemory()
		{
			AssertCreateJournalFromSellChargeConsumeExistingAccrual(true, false);
		}

		public void TestCreateJRJFromSellConsumeExistingAccrualWhenCreditorMatchesAndInDB()
		{
			AssertCreateJournalFromSellChargeConsumeExistingAccrual(false, true);
		}

		public void TestCreateJRJFromSellConsumeExistingAccrualWhenCreditorMatchesAndInMemory()
		{
			AssertCreateJournalFromSellChargeConsumeExistingAccrual(false, false);
		}

		void AssertCreateJournalFromSellChargeConsumeExistingAccrual(bool isCreditorBlank, bool isAccrualInDB)
		{
			var sellCharge = CreateSellCharge();
			var accrualCharge = CreateAccrualCharge(isCreditorBlank, isAccrualInDB, sellCharge);

			AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(sellCharge);
			var journal = sellCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).First();
			var sellLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == sellCharge.JR_AL_ARLine);
			var costLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK != sellLine.PK);
			var costCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, costLine.PK));
			AssertEquals("Expect to use the existing accrual charge", accrualCharge, costCharge);
			AssertEquals(50m, accrualCharge.JR_OSCostAmt);
			AssertEquals(100m, accrualCharge.JR_LocalCostAmt);
			Assert(accrualCharge.IsCostPosted);

			journal.RunPreSaveValidation();
			AssertNoErrors("journal", journal);
		}

		ChargeWithCost CreateSellCharge()
		{
			var sellCharge = CreateCharge(usePositiveNumbers: true);
			sellCharge.JR_OH_SellAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			sellCharge.JR_OSSellExRate = 0.5;
			sellCharge.JR_RX_NKSellCurrency = "USD";
			sellCharge.RunPreSaveValidation();
			AssertNoErrors(sellCharge);
			return sellCharge;
		}

		Charge CreateAccrualCharge(bool isCreditorBlank, bool isAccrualInDB, ChargeWithCost sellCharge)
		{
			var accrualCharge = Factory.New<Charge>();
			accrualCharge.JR_AC = sellCharge.JR_AC;
			accrualCharge.JR_Desc = "charge code description";
			if (isCreditorBlank)
			{
				accrualCharge.JR_OH_CostAccount = ZGuid.Empty;
			}
			else
			{
				accrualCharge.JR_OH_CostAccount = sellCharge.Branch.GB_OH_OrgProxy;
			}
			accrualCharge.JR_JH = job2.PK;
			accrualCharge.JR_GB = sellCharge.JR_GB_InternalBranch;
			accrualCharge.JR_GE = sellCharge.JR_GE_InternalDept;
			accrualCharge.JR_RX_NKCostCurrency = testObjectCreator.USD.RX_Code;
			accrualCharge.JR_OSCostAmt = 100m;
			accrualCharge.JR_LocalCostAmt = 200m;
			if (isAccrualInDB)
			{
				Factory.Save();
			}
			return accrualCharge;
		}

		public void TestRelatedJobIsCopiedWhenInternalJobIsSameJobAsSource()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var setup = testObjectCreator.CreateGatewayConsolsAndShipments();
				Factory.Save();

				//			C0001		gC0002		C0003		C0004
				//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
				//												 \
				//													C0005
				//														\
				//															USNYC
				//				|-	-	-	-	-	S0001	-	-	-|
				//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
				//	|-	-	-	S0003	-	-|

				using (var gatewayJob = testObjectCreator.CreateJob(setup.gC0001))
				using (var shipment3JobHeader = testObjectCreator.CreateJob(setup.s0003))
				{
					var sourceJob = gatewayJob;
					var targetJob = gatewayJob;
					var relatedJob = shipment3JobHeader;

					SetupSourceChargeAndAssertChargeLinkedToJobRevenueJournal(sourceJob, targetJob, relatedJob);
				}
			}
		}

		public void TestRelatedJobIsCopiedWhenInternalJobIsNextLegJob()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var setup = testObjectCreator.CreateGatewayConsolsAndShipments();
				Factory.Save();

				//			C0001		gC0002		C0003		C0004
				//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
				//												 \
				//													C0005
				//														\
				//															USNYC
				//				|-	-	-	-	-	S0001	-	-	-|
				//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
				//	|-	-	-	S0003	-	-|

				using (var gatewayJobLeg1 = testObjectCreator.CreateJob(setup.gC0001))
				using (var gatewayJobLeg2 = testObjectCreator.CreateJob(setup.gC0002))
				using (var shipment3JobHeader = testObjectCreator.CreateJob(setup.s0003))
				{
					var sourceJob = gatewayJobLeg1;
					var targetJob = gatewayJobLeg2;
					var relatedJob = shipment3JobHeader;

					SetupSourceChargeAndAssertChargeLinkedToJobRevenueJournal(sourceJob, targetJob, relatedJob);
				}
			}
		}

		public void TestRelatedJobIsNotCopiedWhenInternalJobUnrelatedGatewayJob()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var setup = testObjectCreator.CreateGatewayConsolsAndShipments();

				var gC0006 = testObjectCreator.CreateGatewayConsol("SGSIN", "CNBJG", "C0006", sendingGatewayCompany: GlbCompany.CurrentCompany);
				setup.s0001.Consols.Add(gC0006);

				Factory.Save();

				//			C0001		gC0002		C0006			C0003		C0004
				//			C0006
				//	AUBNE	-	AUSYD	-	SGSIN			CNBJG	-	HKHKG	-	USLAX
				//														\
				//														C0005
				//															\
				//																USNYC
				//				|-	-	--				S0001	-	-	-|
				//	|-	-	-	-	-	-			S0002	-	-	-	-	-	-|
				//	|-	-	-	S0003	-	-|

				using (var gatewayJobLeg1 = testObjectCreator.CreateJob(setup.gC0001))
				using (var gatewayJobUnrelated = testObjectCreator.CreateJob(gC0006))
				using (var shipment3JobHeader = testObjectCreator.CreateJob(setup.s0003))
				{
					var sourceJob = gatewayJobLeg1;
					var targetJob = gatewayJobUnrelated;
					var relatedJob = shipment3JobHeader;

					SetupSourceChargeAndAssertChargeLinkedToJobRevenueJournal(sourceJob, targetJob, relatedJob, false, "Related job number should not be copied as target job is unrelated to the Related Job");
				}
			}
		}

		public void TestRelatedJobIsNotCopiedWhenInternalJobIsAShipmentJob()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var setup = testObjectCreator.CreateGatewayConsolsAndShipments();
				Factory.Save();

				//			C0001		gC0002		C0003		C0004
				//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
				//												 \
				//													C0005
				//														\
				//															USNYC
				//				|-	-	-	-	-	S0001	-	-	-|
				//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
				//	|-	-	-	S0003	-	-|

				using (var gatewayJob = testObjectCreator.CreateJob(setup.gC0001))
				using (var shipment1JobHeader = testObjectCreator.CreateJob(setup.s0001))
				using (var shipment3JobHeader = testObjectCreator.CreateJob(setup.s0003))
				{
					var sourceJob = gatewayJob;
					var targetJob = shipment1JobHeader;
					var relatedJob = shipment3JobHeader;

					SetupSourceChargeAndAssertChargeLinkedToJobRevenueJournal(sourceJob, targetJob, relatedJob, false, "Related job number should not be copied as target job is not a gateway consol");
				}
			}
		}

		void SetupSourceChargeAndAssertChargeLinkedToJobRevenueJournal(Job sourceJob, Job internalJob, Job relatedJob, bool relatedJobShouldBeCopied = true, string message = default)
		{
			var creditor = testObjectCreator.AALSHI;
			var debtor = GlbCompany.CurrentCompany.OrgProxy;

			var chargeCode = testObjectCreator.FRT;
			var currency = testObjectCreator.AUD;

			var charge1 = testObjectCreator.CreateCharge(sourceJob, chargeCode, "", currency, 100M, creditor, currency, 100M, debtor);
			charge1.JR_Calc_RelatedJobNumber = relatedJob.JH_JobNum;
			charge1.JR_OH_SellAccount = debtor.PK;

			charge1.JR_JH_InternalJob = internalJob.PK;
			charge1.JR_GB_InternalBranch = testObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			Assert("Original charge should be posted", charge1.IsRevenuePosted);

			var charge2 = internalJob.Charges.Cast<Charge>().SingleOrDefault(x => x.PK != charge1.PK && x.JR_AC == charge1.JR_AC);
			Assert("The job revenue journal should have its cost posted", charge2.IsCostPosted);
			AssertEquals("AP line linked to Job Revenue Journal", TransactionTypes.JobRevenueJournal, charge2.APLine?.TransactionHeader?.AH_TransactionType);
			if (relatedJobShouldBeCopied)
			{
				AssertEquals("Related Job number should be copied", relatedJob.JH_JobNum, charge2.JR_Calc_RelatedJobNumber);
			}
			else
			{
				AssertEquals(message, ZString.Empty, charge2.JR_Calc_RelatedJobNumber);
			}
		}

		public void TestDontCreateJRJIfEmptyJobBranchDepartment()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_GB_InternalBranch = charge.JR_GB;
			charge.JR_GE_InternalDept = charge.JR_GE;
			charge.JR_JH_InternalJob = charge.Job.PK;

			Assert(charge.InternalFieldsPointToSameEntity());
			Assert(!charge.InternalFieldsPointToAnotherEntity());

			var charge2 = Factory.NewWithValidTestData<Charge>();

			charge.JR_JH_InternalJob = charge2.Job.PK;
			Assert(charge.InternalFieldsPointToAnotherEntity());
			Assert(!charge.InternalFieldsPointToSameEntity());

			var chargeJobPK = charge.Job.PK;
			var chargeBranchPK = charge.JR_GB;
			var chargeDeptPK = charge.JR_GE;

			charge.JR_JH = ZGuid.Empty;
			Assert(!charge.InternalFieldsPointToAnotherEntity());
			Assert(!charge.InternalFieldsPointToSameEntity());

			charge.JR_JH = chargeJobPK;
			charge.JR_GB = ZGuid.Empty;
			Assert(!charge.InternalFieldsPointToAnotherEntity());
			Assert(!charge.InternalFieldsPointToSameEntity());

			charge.JR_GB = chargeBranchPK;
			charge.JR_GE = ZGuid.Empty;
			Assert(!charge.InternalFieldsPointToAnotherEntity());
			Assert(!charge.InternalFieldsPointToSameEntity());

			charge.JR_GE = chargeDeptPK;
			Assert(charge.InternalFieldsPointToAnotherEntity());
			Assert(!charge.InternalFieldsPointToSameEntity());
		}

		public void TestDontCreateJRJIfInternalJobIsNull()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_GB_InternalBranch = charge.JR_GB;
			charge.JR_GE_InternalDept = charge.JR_GE;
			charge.JR_JH_InternalJob = ZGuid.NewZGuid();

			Assert("Precondition", charge.JR_JH_InternalJob.IsValid);       //we could not reproduce but apparently the charge ended up with not empty JR_JH_InternalJob which could not be loaded by a Factory in AutoJobRevenueJournalCreator, causing NRE...

			AssertExceptionThrown<CannotSaveAfterCriticalErrorException>("Saving should halt if error creating JRJ", "Job Revenue Journal could not be created", () => AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(charge));
			AssertExceptionThrown<CannotSaveAfterCriticalErrorException>("Saving should halt if error creating JRJ", "Job Revenue Journal could not be created", () => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(charge));

			Assert(!charge.InternalFieldsPointToAnotherEntity());
			Assert(!charge.InternalFieldsPointToSameEntity());

			charge.JR_JH_InternalJob = charge.Job.PK;
			Assert(charge.InternalFieldsPointToSameEntity());
			Assert(!charge.InternalFieldsPointToAnotherEntity());

			var charge2 = Factory.NewWithValidTestData<Charge>();

			charge.JR_JH_InternalJob = charge2.Job.PK;
			Assert(charge.InternalFieldsPointToAnotherEntity());
			Assert(!charge.InternalFieldsPointToSameEntity());

			charge.JR_JH_InternalJob = ZGuid.Empty;
			Assert(!charge.InternalFieldsPointToAnotherEntity());
			Assert(!charge.InternalFieldsPointToSameEntity());
		}

		public void TestDoNotCreateJRJIfHasJFCJob()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var testObjectCreator = new TestObjectCreator(Factory);
			var expectedMsg = "Auto Job Revenue Journal cannot be created as its Invoicing or Internal Job has Ready For Financial Closure status.";

			var shipment1 = testObjectCreator.CreateShipment("S0001");
			var shipment2 = testObjectCreator.CreateShipment("S0002");
			Factory.Save();

			var shipment1Job = testObjectCreator.CreateJob(shipment1, false);
			var shipment2Job = testObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			var charge = testObjectCreator.CreateCharge(shipment1Job, testObjectCreator.FRT, 0m, 0m);
			charge.JR_JH_InternalJob = shipment2Job.PK;
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var job = Factory.Load<Job>(shipment1Job.PK);
			var jobInternal = Factory.Load<Job>(shipment2Job.PK);

			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;

			job.JH_Status = JobHeaderStatus.Codes.Working;
			jobInternal.JH_Status = JobHeaderStatus.Codes.Working;
			newFactory.Save();

			AssertNoExceptionThrown("Allow to create JRJ when there is no JFC job", () => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(charge));
			AssertNoExceptionThrown(() => AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(charge));

			job.JH_Status = JobHeaderStatus.Codes.Working;
			jobInternal.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
			newFactory.Save();

			AssertExceptionThrown<CannotSaveAfterCriticalErrorException>("Do not allow create JRJ when have internal JFC job.", expectedMsg, () => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(charge));
			AssertExceptionThrown<CannotSaveAfterCriticalErrorException>(expectedMsg, () => AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(charge));

			job.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
			jobInternal.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
			newFactory.Save();

			AssertExceptionThrown<CannotSaveAfterCriticalErrorException>("Do not allow create JRJ when have JFC job.", expectedMsg, () => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(charge));
			AssertExceptionThrown<CannotSaveAfterCriticalErrorException>(expectedMsg, () => AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(charge));

			job.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
			jobInternal.JH_Status = JobHeaderStatus.Codes.Working;
			newFactory.Save();

			AssertExceptionThrown<CannotSaveAfterCriticalErrorException>("Do not allow create JRJ when have invocing JFC job.", expectedMsg, () => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(charge));
			AssertExceptionThrown<CannotSaveAfterCriticalErrorException>(expectedMsg, () => AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(charge));

			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true;

			job.JH_Status = JobHeaderStatus.Codes.Working;
			jobInternal.JH_Status = JobHeaderStatus.Codes.Working;
			newFactory.Save();

			AssertNoExceptionThrown("Allow to creat JRJ when has security right", () => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(charge));
			AssertNoExceptionThrown(() => AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(charge));

			job.JH_Status = JobHeaderStatus.Codes.Working;
			jobInternal.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
			newFactory.Save();

			AssertNoExceptionThrown("Allow to creat JRJ when has security right", () => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(charge));
			AssertNoExceptionThrown(() => AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(charge));

			job.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
			jobInternal.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
			newFactory.Save();

			AssertNoExceptionThrown("Allow to creat JRJ when has security right", () => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(charge));
			AssertNoExceptionThrown(() => AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(charge));

			job.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
			jobInternal.JH_Status = JobHeaderStatus.Codes.Working;
			newFactory.Save();

			AssertNoExceptionThrown("Allow to creat JRJ when has security right", () => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(charge));
			AssertNoExceptionThrown(() => AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(charge));
		}

		#region TestInternalJobChargesNotLoaded

		public void TestInternalJobChargesNotLoaded_CreateJournalFromCostCharge()
		{
			AssertInternalJobChargesNotLoaded(
				charge => charge.JR_OH_CostAccount = GlbCompany.CurrentCompany.OrgProxy.PK,
				charge => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(charge)
				);
		}

		public void TestInternalJobChargesNotLoaded_CreateJournalFromSellCharge()
		{
			AssertInternalJobChargesNotLoaded(
				charge => charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK,
				charge => AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(charge)
				);
		}

		public void AssertInternalJobChargesNotLoaded(Action<Charge> setupCharge, Action<Charge> testAction)
		{
			var newFactory = Factory.CreateNewFactory();
			var testObjectCreator = new TestObjectCreator(newFactory);

			var shipment1 = testObjectCreator.CreateShipment("S0001");
			var shipment2 = testObjectCreator.CreateShipment("S0002");

			var job = testObjectCreator.CreateJob(shipment1, testObjectCreator.LocalClient, 0, null, 0);
			var internalJob = testObjectCreator.CreateJob(shipment2, testObjectCreator.LocalClient, 0, null, 0);

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1);
			setupCharge(charge);
			charge.JR_JH_InternalJob = internalJob.PK;

			testObjectCreator.CreateCharge(internalJob, testObjectCreator.CC2);
			testObjectCreator.CreateCharge(internalJob, testObjectCreator.CC2);

			newFactory.Save();

			var chargeInTestFactory = Factory.Load<Charge>(charge.PK);

			var internalJobChargesQuery = new ZQuery(JobChargeSchema.JR_JH, internalJob.PK);
			internalJobChargesQuery.FetchOnlyFromLocalCache = true;
			var internalJobChargesInMemory = Factory.Load<Charge>(internalJobChargesQuery);
			AssertEquals("Precondition: charges in memory", 0, internalJobChargesInMemory.Length);

			testAction(chargeInTestFactory);

			internalJobChargesInMemory = Factory.Load<Charge>(internalJobChargesQuery);
			AssertEquals("Charges in memory", 1, internalJobChargesInMemory.Length);
			AssertEquals("ChargeProcessingForAPTransactionPosting is not on the Factory after processing", false, Factory.HasContext(BusinessContext.ChargeProcessingForAPTransactionPosting));

			internalJobChargesQuery.FetchOnlyFromLocalCache = false;
			var internalJobChargesInMemoryAndDb = Factory.Load<Charge>(internalJobChargesQuery);
			AssertEquals("Post Condition: Charges in memory and db", 3, internalJobChargesInMemoryAndDb.Length);
		}

		#endregion InternalJobChargesNotLoaded

		public void TestCreateJRJWithCostChargeGatewayBatch_NoJFCJob_NoSecurityRight()
		{
			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;
			AssertCreateAutoJRJWithCostChargeGatewayBatch("Allow to create JRJ when there is no JFC job", JobHeaderStatus.Codes.Working, false);
		}

		public void TestCreateJRJWithCostChargeGatewayBatch_HasJFCJob_NoSecurityRight()
		{
			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;
			AssertCreateAutoJRJWithCostChargeGatewayBatch("Do not allow create JRJ when cost charge in Gateway batch has JFC job.", JobHeaderStatus.Codes.JobReadyForFinancialClosure, true);
		}

		public void TestCreateJRJWithCostChargeGatewayBatch_NoJFCJob_HasSecurityRight()
		{
			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true;
			AssertCreateAutoJRJWithCostChargeGatewayBatch("Always allow to creat JRJ when has security right", JobHeaderStatus.Codes.Working, false);
		}

		public void TestCreateJRJWithCostChargeGatewayBatch_HasJFCJob_HasSecurityRight()
		{
			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true;
			AssertCreateAutoJRJWithCostChargeGatewayBatch("Always allow to creat JRJ when has security right", JobHeaderStatus.Codes.JobReadyForFinancialClosure, false);
		}

		void AssertCreateAutoJRJWithCostChargeGatewayBatch(string assertionDisplayMesage, string jobStatus, bool isExpectException)
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var testObjectCreator = new TestObjectCreator(Factory);
			var expectedMsg = "Auto Job Revenue Journal cannot be created as its Invoicing or Internal Job has Ready For Financial Closure status.";

			var proxyOrg = testObjectCreator.CreateOrgHeader("RA_BNE", true, true, "AUBNE");
			var bneBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
			bneBranch.GB_OH_OrgProxy = proxyOrg.PK;
			Factory.Save();

			var gatewayConsol = testObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = testObjectCreator.CreateShipment("S0001", gatewayConsol);
			var shipment2 = testObjectCreator.CreateShipment("S0002", gatewayConsol);
			var gatewayJob = testObjectCreator.CreateJob(gatewayConsol);
			var shipment1Job = testObjectCreator.CreateJob(shipment1);
			var shipment2Job = testObjectCreator.CreateJob(shipment2);
			Factory.Save();

			gatewayConsol.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			var consolCost = testObjectCreator.CreateGatewayConsolCost(gatewayConsol, testObjectCreator.FRT);
			gatewayConsol.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);

			var gatewayCharge = gatewayJob.Charges.AddNew();
			gatewayCharge.JR_E6_GatewaySellHeader = consolCost.PK;
			gatewayCharge.JR_JH_InternalJob = gatewayCharge.JR_JH;
			gatewayCharge.JR_GB_InternalBranch = gatewayCharge.JR_GB;
			gatewayCharge.JR_GE_InternalDept = gatewayCharge.JR_GE;
			gatewayCharge.JR_LocalSellAmt = 380m;

			foreach (ApportionSplitCharge apportionCharge in consolCost.ApportionmentCharges)
			{
				apportionCharge.JR_JH_InternalJob = gatewayCharge.JR_JH_InternalJob;
				apportionCharge.JR_GB_InternalBranch = gatewayCharge.JR_GB_InternalBranch;
				apportionCharge.JR_GE_InternalDept = gatewayCharge.JR_GE_InternalDept;
				apportionCharge.JR_OH_CostAccount = proxyOrg.PK;
				apportionCharge.JR_LocalCostAmt = 190;
				apportionCharge.JR_OSCostAmt = 190;
			}

			ShipmentCharge = Factory.Load<ChargeWithCost>(consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == shipment1Job.PK).PK);
			AssertNotEquals("Pre-condition", JobHeaderStatus.Codes.JobReadyForFinancialClosure, gatewayJob.JH_Status);
			AssertNotEquals(JobHeaderStatus.Codes.JobReadyForFinancialClosure, shipment1Job.JH_Status);

			shipment2Job.JH_Status = jobStatus;

			if (isExpectException)
			{
				AssertExceptionThrown<CannotSaveAfterCriticalErrorException>(assertionDisplayMesage, expectedMsg, () => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(ShipmentCharge));
			}
			else
			{
				AssertNoExceptionThrown(assertionDisplayMesage, () => AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(ShipmentCharge));
			}
		}

		ChargeWithCost ShipmentCharge;

		[ExpectNoExceptions]
		public void TestCreateJournalWithoutErrorWhenApportionSplitChargeHasRoundingError()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var nums = new[] { 5m, 5m, 6m, 15.07m, 26.49m, 26.6m, 30m, 31m, 34.85m, 35.65m, 36.63m, 44.08m, 61.75m, 78m };
				var consol = testObjectCreator.CreateConsol();

				Factory.Save();

				for (var i = 0; i < nums.Length; i++)
				{
					consol.Shipments.AddNew();
				}

				var apportionmentListing = new ApportionmentListing(Factory, consol);
				var consolCost = apportionmentListing.CostsCollection.TryAddNew();
				consolCost.E6_GC = GlbCompany.CurrentCompany.PK;
				var orgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.GB_OH_OrgProxy);
				orgProxy.CompanyData.OB_IsCreditor = true;
				orgProxy.CompanyData.SetAPTaxApplicable(true);
				consolCost.E6_OH_Creditor = orgProxy.PK;
				consolCost.CostExchangeRate.Currency = "CNY";
				consolCost.E6_AC_ChargeCode = testObjectCreator.DSBChargeCode.PK;
				consolCost.E6_OSCostAmount = 436.12m;
				consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				consolCost.E6_ExchangeRate = 1.148550m;

				for (var i = 0; i < consolCost.ApportionmentCharges.Count; i++)
				{
					consolCost.ApportionmentCharges[i].JR_OSCostAmt = nums[i];
				}

				Factory.Save();

				var jobCharge = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.JR_OSCostAmt == 78m);
				AssertNotNull("Job charge should not be null.", jobCharge);
				AssertEquals("Local cost amount should rounding to 67.93.", 67.93m, jobCharge.JR_LocalCostAmt);

				jobCharge.JR_GE_InternalDept = testObjectCreator.NonCurrentDepartment.PK;
				jobCharge.JR_GB_InternalBranch = testObjectCreator.NonCurrentBranch.PK;

				Factory.Save();

				var jobCharges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobCharge.JR_JH));
				var charge = jobCharges.FirstOrDefault(x => x.JR_E6 == ZGuid.Empty);
				AssertNotNull("Charge should not be null.", charge);
				Assert("Charge should be revenue posted.", charge.IsRevenuePosted);
				AssertEquals("Os cost amount should be 78.", 78m, charge.JR_OSCostAmt);
				AssertEquals("Local cost amount should be 67.93", 67.93m, charge.JR_LocalCostAmt);

				var line = Factory.LoadTop1<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.PK, charge.JR_AL_ARLine));
				AssertNotNull("Line should not be null.", line);
				AssertEquals("Os amount should be 78.", 78m, line.AL_OSAmount);
				AssertEquals("Line amount should be 67.93", 67.93m, line.AL_LineAmount);

				var journal = Factory.Load<JobRevenueJournal>(new ZQuery(AccTransactionHeaderSchema.PK, line.AL_AH)).First();
				AssertNotNull("Journal should not be null.", journal);
			}
		}

		[ExpectNoExceptions]
		public void TestEnsureOneToManyJournalCanBeCreated()
		{
			var shipment = testObjectCreator.CreateShipment("S00001000");
			using (Job jobHeader = testObjectCreator.CreateJob(shipment))
			{
				var gatewayBillingSellCharge = testObjectCreator.CreateCharge(jobHeader);
				gatewayBillingSellCharge.JR_JH_InternalJob = jobHeader.PK;

				var costCharge = testObjectCreator.CreateCharge(jobHeader, creditor: GlbCompany.CurrentCompany.OrgProxy);
				costCharge.JR_JH_InternalJob = jobHeader.PK;
				var costCharges = new System.Collections.Generic.List<ChargeWithCost>(new ChargeWithCost[] { costCharge });
				(ChargeWithCost gatewayBillingSellCharge, ChargeWithCost[] costCharges) gatewayBatch = (gatewayBillingSellCharge, costCharges.ToArray());

				costCharge.JR_JH = ZGuid.Empty;
				AssertNull("cost charge job is null", costCharge.Job);
				AssertEquals("ZZCC1 cost charge does not have a Job", AutoJobRevenueJournalCreator.EnsureOneToManyJournalCanBeCreated(gatewayBatch, true));

				costCharge.JR_JH = jobHeader.PK;
				AssertNotNull("cost charge job is not null", costCharge.Job);
				AssertEquals("", AutoJobRevenueJournalCreator.EnsureOneToManyJournalCanBeCreated(gatewayBatch, true));
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestEmptyParameterThrowsArgumentNullException()
		{
			AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(null);
		}

		public void TestSellAndCostRatingOverrideIsCopiedWhenJRJIsCreated()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var setup = testObjectCreator.CreateGatewayConsolsAndShipments();
				Factory.Save();

				using (var gatewayJob = testObjectCreator.CreateJob(setup.gC0001))
				using (var targetJob = testObjectCreator.CreateJob(setup.s0001))
				using (var relatedJob = testObjectCreator.CreateJob(setup.s0003))
				{
					var debtor = GlbCompany.CurrentCompany.OrgProxy;
					var currency = testObjectCreator.AUD;

					var consolRevenueCharge = testObjectCreator.CreateCharge(gatewayJob, testObjectCreator.FRT, "", currency, 100M, testObjectCreator.AALSHI, currency, 100M, debtor);
					consolRevenueCharge.JR_Calc_RelatedJobNumber = relatedJob.JH_JobNum;
					consolRevenueCharge.JR_OH_SellAccount = debtor.PK;

					consolRevenueCharge.JR_JH_InternalJob = targetJob.PK;
					consolRevenueCharge.JR_GB_InternalBranch = testObjectCreator.NonCurrentBranch.PK;
					consolRevenueCharge.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;

					Factory.Save();

					Assert("Original charge should be posted", consolRevenueCharge.IsRevenuePosted);
					AssertEquals(JobChargeLookups.StopFromAutorating, consolRevenueCharge.JR_Calc_SellRatingBehavior);

					var shipmentCostCharge = targetJob.Charges.Cast<Charge>().SingleOrDefault(x => x.PK != consolRevenueCharge.PK && x.JR_AC == consolRevenueCharge.JR_AC);
					Assert("The job revenue journal should have its cost posted", shipmentCostCharge.IsCostPosted);

					AssertEquals("Sell Rating Override Should be copied", consolRevenueCharge.JR_SellRatingOverride, shipmentCostCharge.JR_SellRatingOverride);
					AssertEquals("Cost Rating Override Should be copied", consolRevenueCharge.JR_SellRatingOverride, shipmentCostCharge.JR_CostRatingOverride);

					AssertEquals(JobChargeLookups.StopFromAutorating, shipmentCostCharge.JR_Calc_CostRatingBehavior);
				}
			}
		}

		public void TestNoJRJCreatedFromSell_WhenOSSellAmtIsZero()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var shipment1 = testObjectCreator.CreateShipment("S90006666");
				var job3 = testObjectCreator.CreateJob(shipment1, false, false);

				var sellCharge = CreateCharge(true);
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				GlbCompany.CurrentCompany.Factory.Save();
				sellCharge.JR_JH = job3.PK;
				sellCharge.JR_GB = GlbBranch.CurrentBranch.PK;
				sellCharge.JR_OH_SellAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				sellCharge.JR_RX_NKSellCurrency = "USD";
				job3.ExchangeRates[0].JF_BaseRate = 654321m;
				sellCharge.JR_OSSellAmt = 0m;
				sellCharge.JR_LocalSellAmt = 1m;

				sellCharge.JR_JH_InternalJob = job2.PK;
				Factory.Save();

				var journalSize1 = sellCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).Length;
				AssertEquals(0, journalSize1);

				sellCharge.JR_RX_NKSellCurrency = "AUD";
				sellCharge.JR_OSSellAmt = 1m;
				Factory.Save();

				var journalSize2 = sellCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).Length;
				AssertEquals(1, journalSize2);
			}
		}

		public void TestNoJRJCreatedFromCost_WhenOSCostAmtIsZero()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var shipment1 = testObjectCreator.CreateShipment("S90006667");
				var job3 = testObjectCreator.CreateJob(shipment1, false, false);

				var costCharge = CreateCharge(true);
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				GlbCompany.CurrentCompany.Factory.Save();
				costCharge.JR_JH = job3.PK;
				costCharge.JR_GB = GlbBranch.CurrentBranch.PK;
				costCharge.JR_OH_CostAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				costCharge.JR_RX_NKCostCurrency = "USD";
				job3.ExchangeRates[0].JF_BaseRate = 654321m;
				costCharge.JR_OSCostAmt = 0m;
				costCharge.JR_LocalCostAmt = 1m;

				costCharge.JR_JH_InternalJob = job2.PK;
				Factory.Save();

				var journalSize1 = costCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).Length;
				AssertEquals(0, journalSize1);

				costCharge.JR_RX_NKCostCurrency = "AUD";
				costCharge.JR_OSCostAmt = 1m;
				Factory.Save();

				var journalSize2 = costCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).Length;
				AssertEquals(1, journalSize2);
			}
		}

		#region TestCreateJournalFromCostCharge

		public void TestCreateJournalFromCostChargeWithPositiveAmounts()
		{
			var costCharge = CreateCharge(usePositiveNumbers: true);

			if (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value)
			{
				costCharge.JR_OH_CostAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			}
			if (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value)
			{
				costCharge.JR_OH_SellAccount = testObjectCreator.Debtor.PK;
			}
			costCharge.JR_OSCostExRate = 1m;
			costCharge.RunPreSaveValidation();
			AssertNoErrors(costCharge);

			AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(costCharge);
			var journal = costCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).First();
			var costLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == costCharge.JR_AL_APLine);
			var sellLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK != costLine.PK);
			var sellCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, sellLine.PK));

			journal.RunPreSaveValidation();
			AssertNoErrors("journal", journal);

			AssertGeneratedChargeValuesMatchOriginalCharge(costCharge, sellCharge);
			AssertEquals("description", costCharge.JR_Desc, sellCharge.JR_Desc);
			AssertJRJLineMatchesCostValues(costCharge, costLine, true);
			AssertJRJLineMatchesSellValues(sellCharge, sellLine, true);
			Factory.Save();
			if ((costCharge.ChargeType == Constants.ChargeType.Margin && costCharge.MarginPercentage == 0) || costCharge.ChargeType == Constants.ChargeType.ManualJobAccrual)
			{
				var chargeTypeDetails = (costCharge.ChargeType == Constants.ChargeType.Margin) ? "MRG 0%" : "MJA";
				AssertNull(string.Format("When Internal Cost JRJ is posted against {0} charge type, No ACR should be posted against the second line (internal sell).", chargeTypeDetails), Factory.Load<Accrual>(sellCharge.JR_AL_APLine));
			}

			if (costCharge.IsMarginCharge)
			{
				SetMarginCharges(costCharge, sellCharge);
			}
		}

		[ExpectNoExceptions]
		public void TestCreateJournalFromCostChargeWithPositiveAmountsAndWIPMustHaveDebtorAccrualMustHaveCreditor()
		{
			using (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				TestCreateJournalFromCostChargeWithPositiveAmounts();
			}
		}

		public void TestCreateJournalFromCostChargeWithPositiveAmountsAndMRGChargeCodeAndZeroMarginPercentage()
		{
			chargeCode1 = testObjectCreator.CreateChargeCode("TMRG", "Margin Charge for test", "MRG", 0m, null, null);
			TestCreateJournalFromCostChargeWithPositiveAmounts();

			AssertEquals(50m, costMarginCharge.JR_OSCostAmt);
			AssertEquals(50m, costMarginCharge.JR_LocalCostAmt);
			AssertEquals(50m, costMarginCharge.JR_OSSellAmt);
			AssertEquals(100m, costMarginCharge.JR_LocalSellAmt);

			AssertEquals(0m, sellMarginCharge.JR_OSCostAmt);
			AssertEquals(0m, sellMarginCharge.JR_LocalCostAmt);
			AssertEquals(50m, sellMarginCharge.JR_OSSellAmt);
			AssertEquals(50m, sellMarginCharge.JR_LocalSellAmt);
		}

		public void TestCreateJournalFromCostChargeWithPositiveAmountsAndMRGChargeCodeAndMarginPercentage10()
		{
			chargeCode1 = testObjectCreator.CreateChargeCode("TMRG", "Margin Charge for test", "MRG", 10m, null, null);
			TestCreateJournalFromCostChargeWithPositiveAmounts();

			AssertEquals(50m, costMarginCharge.JR_OSCostAmt);
			AssertEquals(50m, costMarginCharge.JR_LocalCostAmt);
			AssertEquals(50m, costMarginCharge.JR_OSSellAmt);
			AssertEquals(100m, costMarginCharge.JR_LocalSellAmt);

			AssertEquals(5m, sellMarginCharge.JR_OSCostAmt);
			AssertEquals(5m, sellMarginCharge.JR_LocalCostAmt);
			AssertEquals(50m, sellMarginCharge.JR_OSSellAmt);
			AssertEquals(50m, sellMarginCharge.JR_LocalSellAmt);
		}

		public void TestCreateJournalFromCostChargeWithPositiveAmountsAndMRGChargeCodeAndMarginPercentage100()
		{
			chargeCode1 = testObjectCreator.CreateChargeCode("TMRG", "Margin Charge for test", "MRG", 100m, null, null);
			TestCreateJournalFromCostChargeWithPositiveAmounts();

			AssertEquals(50m, costMarginCharge.JR_OSCostAmt);
			AssertEquals(50m, costMarginCharge.JR_LocalCostAmt);
			AssertEquals(50m, costMarginCharge.JR_OSSellAmt);
			AssertEquals(100m, costMarginCharge.JR_LocalSellAmt);

			AssertEquals(50m, sellMarginCharge.JR_OSCostAmt);
			AssertEquals(50m, sellMarginCharge.JR_LocalCostAmt);
			AssertEquals(50m, sellMarginCharge.JR_OSSellAmt);
			AssertEquals(50m, sellMarginCharge.JR_LocalSellAmt);
		}

		public void TestCreateJournalFromCostChargeWithPositiveAmountsAndMJAChargeType()
		{
			chargeCode1 = testObjectCreator.ManualJobAccrualChargeCode;
			TestCreateJournalFromCostChargeWithPositiveAmounts();
		}

		public void TestCreateJournalFromCostChargeWithPositiveAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToBoth()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				TestCreateJournalFromCostChargeWithPositiveAmounts();
			}
		}

		public void TestCreateJournalFromCostChargeWithPositiveAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToCost()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				TestCreateJournalFromCostChargeWithPositiveAmounts();
			}
		}

		public void TestCreateJournalFromCostChargeWithPositiveAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToRevenue()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				TestCreateJournalFromCostChargeWithPositiveAmounts();
			}
		}

		public void TestCreateJournalFromCostChargeWithNegativeAmounts()
		{
			var costCharge = CreateCharge(usePositiveNumbers: false);

			var branchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.GB_OH_OrgProxy);
			branchOrgProxy.CompanyData.OB_APCostsSelfBilled = true; // To allow negative cost Amounts
			costCharge.JR_OH_CostAccount = branchOrgProxy.PK;
			if (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value)
			{
				costCharge.JR_OH_SellAccount = testObjectCreator.Debtor.PK;
			}
			costCharge.CostExchangeRate.SetBuyRate_ForTestOnly(1m);
			costCharge.RunPreSaveValidation();
			AssertNoErrors(costCharge);

			AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(costCharge);
			var journal = costCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).First();
			var costLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == costCharge.JR_AL_APLine);
			var sellLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK != costLine.PK);
			var sellCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, sellLine.PK));

			journal.RunPreSaveValidation();
			AssertNoErrors("journal", journal);

			AssertGeneratedChargeValuesMatchOriginalCharge(costCharge, sellCharge);
			AssertEquals("description", costCharge.JR_Desc, sellCharge.JR_Desc);
			AssertJRJLineMatchesCostValues(costCharge, costLine, true);
			AssertJRJLineMatchesSellValues(sellCharge, sellLine, true);
			Factory.Save();
			if ((costCharge.ChargeType == Constants.ChargeType.Margin && costCharge.MarginPercentage == 0) || costCharge.ChargeType == Constants.ChargeType.ManualJobAccrual)
			{
				var chargeTypeDetails = (costCharge.ChargeType == Constants.ChargeType.Margin) ? "MRG 0%" : "MJA";
				AssertNull(string.Format("When Internal Cost JRJ is posted against {0} charge type, No ACR should be posted against the second line (internal sell).", chargeTypeDetails), Factory.Load<Accrual>(sellCharge.JR_AL_APLine));
			}

			if (costCharge.IsMarginCharge)
			{
				SetMarginCharges(costCharge, sellCharge);
			}
		}

		[ExpectNoExceptions]
		public void TestCreateJournalFromCostChargeWithNegativeAmountsAndWIPMustHaveDebtorAccrualMustHaveCreditor()
		{
			using (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				TestCreateJournalFromCostChargeWithNegativeAmounts();
			}
		}

		public void TestCreateJournalFromCostChargeWithNegativeAmountsAndMRGChargeCodeAndZeroMarginPercentage()
		{
			chargeCode1 = testObjectCreator.CreateChargeCode("TMRG", "Margin Charge for test", "MRG", 0m, null, null);
			TestCreateJournalFromCostChargeWithNegativeAmounts();

			AssertEquals(-50m, costMarginCharge.JR_OSCostAmt);
			AssertEquals(-50m, costMarginCharge.JR_LocalCostAmt);
			AssertEquals(-50m, costMarginCharge.JR_OSSellAmt);
			AssertEquals(-100m, costMarginCharge.JR_LocalSellAmt);

			AssertEquals(0m, sellMarginCharge.JR_OSCostAmt);
			AssertEquals(0m, sellMarginCharge.JR_LocalCostAmt);
			AssertEquals(-50m, sellMarginCharge.JR_OSSellAmt);
			AssertEquals(-50m, sellMarginCharge.JR_LocalSellAmt);
		}

		public void TestCreateJournalFromCostChargeWithNegativeAmountsAndMRGChargeCodeAndMarginPercentage10()
		{
			chargeCode1 = testObjectCreator.CreateChargeCode("TMRG", "Margin Charge for test", "MRG", 10m, null, null);
			TestCreateJournalFromCostChargeWithNegativeAmounts();

			AssertEquals(-50m, costMarginCharge.JR_OSCostAmt);
			AssertEquals(-50m, costMarginCharge.JR_LocalCostAmt);
			AssertEquals(-50m, costMarginCharge.JR_OSSellAmt);
			AssertEquals(-100m, costMarginCharge.JR_LocalSellAmt);

			AssertEquals(-5m, sellMarginCharge.JR_OSCostAmt);
			AssertEquals(-5m, sellMarginCharge.JR_LocalCostAmt);
			AssertEquals(-50m, sellMarginCharge.JR_OSSellAmt);
			AssertEquals(-50m, sellMarginCharge.JR_LocalSellAmt);
		}

		public void TestCreateJournalFromCostChargeWithNegativeAmountsAndMRGChargeCodeAndMarginPercentage100()
		{
			chargeCode1 = testObjectCreator.CreateChargeCode("TMRG", "Margin Charge for test", "MRG", 100m, null, null);
			TestCreateJournalFromCostChargeWithNegativeAmounts();

			AssertEquals(-50m, costMarginCharge.JR_OSCostAmt);
			AssertEquals(-50m, costMarginCharge.JR_LocalCostAmt);
			AssertEquals(-50m, costMarginCharge.JR_OSSellAmt);
			AssertEquals(-100m, costMarginCharge.JR_LocalSellAmt);

			AssertEquals(-50m, sellMarginCharge.JR_OSCostAmt);
			AssertEquals(-50m, sellMarginCharge.JR_LocalCostAmt);
			AssertEquals(-50m, sellMarginCharge.JR_OSSellAmt);
			AssertEquals(-50m, sellMarginCharge.JR_LocalSellAmt);
		}

		public void TestCreateJournalFromCostChargeWithNegativeAmountsAndMJAChargeCode()
		{
			chargeCode1 = testObjectCreator.ManualJobAccrualChargeCode;
			TestCreateJournalFromCostChargeWithNegativeAmounts();
		}

		public void TestCreateJournalFromCostChargeWithNegativeAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToBoth()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				TestCreateJournalFromCostChargeWithNegativeAmounts();
			}
		}

		public void TestCreateJournalFromCostChargeWithNegativeAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToCost()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				TestCreateJournalFromCostChargeWithNegativeAmounts();
			}
		}

		public void TestCreateJournalFromCostChargeWithNegativeAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToRevenue()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				TestCreateJournalFromCostChargeWithNegativeAmounts();
			}
		}

		#endregion

		#region TestCreateJournalFromSellCharge

		public void TestCreateJournalFromSellChargeDoesNotIncludeAutoratingSellText()
		{
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var debtor = testObjectCreator.CreateOrgHeader("DEBORG", false, true, "AUSYD");

				var originalCharge = CreateCharge(usePositiveNumbers: true);
				originalCharge.JR_Desc = "Generic Charge - Rated 15 Kilogram(s) @ 10 AUD/KG";
				originalCharge.ChargeCode.AC_Desc = "Generic Charge";
				originalCharge.ChargeCode.AC_LocalLanguageDescription = "Local Charge";
				originalCharge.JR_OH_SellAccount = debtor.PK;
				var job = originalCharge.InternalInvoicingJob;

				originalCharge.RunPreSaveValidation();
				AssertNoErrors(originalCharge);

				AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(originalCharge);
				var journal = originalCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).FirstOrDefault();

				AssertNotNull(journal);
				AssertEquals(2, journal.Lines.Count);
				AssertEquals(1, job.Charges.Count);

				var createdChargeFromSell = job.Charges[0];
				AssertEquals(originalCharge.JR_AC, createdChargeFromSell.JR_AC);

				Assert("Pre-condition", originalCharge.ShouldDefaultLocalChargeDescription);
				AssertEquals("Description should be copied from local language description", originalCharge.ChargeCode.AC_LocalLanguageDescription, createdChargeFromSell.JR_Desc);
			}
		}

		public void TestCreateJournalFromSellChargeWithPositiveAmounts()
		{
			var sellCharge = CreateCharge(usePositiveNumbers: true);

			if (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value)
			{
				sellCharge.JR_OH_CostAccount = testObjectCreator.Creditor1.PK;
			}
			if (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value)
			{
				sellCharge.JR_OH_SellAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			}
			sellCharge.JR_RX_NKSellCurrency = "USD";
			sellCharge.RunPreSaveValidation();
			AssertNoErrors(sellCharge);

			AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(sellCharge);
			var journal = sellCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).First();
			var sellLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == sellCharge.JR_AL_ARLine);
			var costLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK != sellLine.PK);
			var costCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, costLine.PK));

			costCharge.JR_RX_NKSellCurrency = "USD";
			costCharge.RunPreSaveValidation();
			AssertNoErrors(costCharge);

			journal.RunPreSaveValidation();
			AssertNoErrors("journal", journal);

			AssertGeneratedChargeValuesMatchOriginalCharge(costCharge, sellCharge);
			AssertEquals("description should match charge code, not sell", costCharge.JR_Desc, costCharge.ChargeCode.AC_Desc);
			AssertJRJLineMatchesCostValues(costCharge, costLine, false);
			AssertJRJLineMatchesSellValues(sellCharge, sellLine, false);

			Factory.Save();

			if (sellCharge.ChargeType == Constants.ChargeType.ManualJobAccrual)
			{
				AssertNull("When Internal Sell JRJ is posted against MJA charge type, No WIP should be posted against the second line (internal cost).", Factory.Load<WIP>(costCharge.JR_AL_ARLine));
			}

			if (sellCharge.IsMarginCharge)
			{
				SetMarginCharges(costCharge, sellCharge);
			}
		}

		[ExpectNoExceptions]
		public void TestCreateJournalFromSellChargeWithPositiveAmountsAndWIPMustHaveDebtorAccrualMustHaveCreditor()
		{
			using (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				TestCreateJournalFromSellChargeWithPositiveAmounts();
			}
		}

		[ExpectNoExceptions]
		public void TestCreateJournalFromSellChargeWithPositiveAmountsAndWIPMustHaveDebtorAccrualMustHaveCreditorAndDSBChargeCode()
		{
			using (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				chargeCode1 = testObjectCreator.DSBChargeCode;
				TestCreateJournalFromSellChargeWithPositiveAmounts();
			}
		}

		public void TestCreateJournalFromSellChargeWithPositiveAmountsAndMJAChargeCode()
		{
			chargeCode1 = testObjectCreator.ManualJobAccrualChargeCode;
			TestCreateJournalFromSellChargeWithPositiveAmounts();
		}

		public void TestCreateJournalFromSellChargeWithPositiveAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToBoth()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				TestCreateJournalFromSellChargeWithPositiveAmounts();
			}
		}

		public void TestCreateJournalFromSellChargeWithPositiveAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToCost()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				TestCreateJournalFromSellChargeWithPositiveAmounts();
			}
		}

		public void TestCreateJournalFromSellChargeWithPositiveAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToRevenue()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				TestCreateJournalFromSellChargeWithPositiveAmounts();
			}
		}

		public void TestCreateJournalFromSellChargeWithPositiveAmountsAndMRGChargeCodeAndZeroMarginPercentage()
		{
			chargeCode1 = testObjectCreator.CreateChargeCode("TMRG", "Margin Charge for test", "MRG", 0m, null, null);
			TestCreateJournalFromSellChargeWithPositiveAmounts();

			AssertEquals(50m, sellMarginCharge.JR_OSCostAmt);
			AssertEquals(100m, sellMarginCharge.JR_LocalCostAmt);
			AssertEquals(50m, sellMarginCharge.JR_OSSellAmt);
			AssertEquals(100m, sellMarginCharge.JR_LocalSellAmt);

			AssertEquals(50m, costMarginCharge.JR_OSCostAmt);
			AssertEquals(100m, costMarginCharge.JR_LocalCostAmt);
			AssertEquals(0m, costMarginCharge.JR_OSSellAmt);
			AssertEquals(0m, costMarginCharge.JR_LocalSellAmt);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCreateJournalFromSellChargeWithPositiveAmountsAndMRGChargeCodeAndMarginPercentage10()
		{
			chargeCode1 = testObjectCreator.CreateChargeCode("TMRG", "Margin Charge for test", "MRG", 10m, null, null);
			TestCreateJournalFromSellChargeWithPositiveAmounts();

			AssertEquals(50m, sellMarginCharge.JR_OSCostAmt);
			AssertEquals(100m, sellMarginCharge.JR_LocalCostAmt);
			AssertEquals(50m, sellMarginCharge.JR_OSSellAmt);
			AssertEquals(100m, sellMarginCharge.JR_LocalSellAmt);

			AssertEquals(50m, costMarginCharge.JR_OSCostAmt);
			AssertEquals(100m, costMarginCharge.JR_LocalCostAmt);
			AssertEquals(500m, costMarginCharge.JR_OSSellAmt);
			AssertEquals(1000m, costMarginCharge.JR_LocalSellAmt);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCreateJournalFromSellChargeWithPositiveAmountsAndMRGChargeCodeAndMarginPercentage100()
		{
			chargeCode1 = testObjectCreator.CreateChargeCode("TMRG", "Margin Charge for test", "MRG", 100m, null, null);
			TestCreateJournalFromSellChargeWithPositiveAmounts();

			AssertEquals(50m, sellMarginCharge.JR_OSCostAmt);
			AssertEquals(100m, sellMarginCharge.JR_LocalCostAmt);
			AssertEquals(50m, sellMarginCharge.JR_OSSellAmt);
			AssertEquals(100m, sellMarginCharge.JR_LocalSellAmt);

			AssertEquals(50m, costMarginCharge.JR_OSCostAmt);
			AssertEquals(100m, costMarginCharge.JR_LocalCostAmt);
			AssertEquals(50m, costMarginCharge.JR_OSSellAmt);
			AssertEquals(100m, costMarginCharge.JR_LocalSellAmt);
		}

		public void TestCreateJournalFromSellChargeWithNegativeAmounts()
		{
			var sellCharge = CreateCharge(usePositiveNumbers: false);
			sellCharge.JR_OSSellAmt = -sellCharge.JR_OSSellAmt;
			sellCharge.JR_LocalSellAmt = -sellCharge.JR_LocalSellAmt;

			testObjectCreator.Creditor1.CompanyData.OB_APCostsSelfBilled = true;  // To allow negative cost Amounts
			sellCharge.JR_OH_CostAccount = testObjectCreator.Creditor1.PK;

			if (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value)
			{
				sellCharge.JR_OH_SellAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			}
			sellCharge.JR_RX_NKSellCurrency = "USD";
			sellCharge.RunPreSaveValidation();
			AssertNoErrors(sellCharge);

			AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(sellCharge);
			var journal = sellCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).First();
			var sellLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == sellCharge.JR_AL_ARLine);
			var costLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK != sellLine.PK);
			var costCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, costLine.PK));

			costCharge.JR_RX_NKSellCurrency = "USD";
			journal.RunPreSaveValidation();
			AssertNoErrors("journal", journal);

			AssertGeneratedChargeValuesMatchOriginalCharge(costCharge, sellCharge);
			AssertEquals("description should match charge code, not sell", costCharge.JR_Desc, costCharge.ChargeCode.AC_Desc);
			AssertJRJLineMatchesCostValues(costCharge, costLine, false);
			AssertJRJLineMatchesSellValues(sellCharge, sellLine, false);
			Factory.Save();

			if (sellCharge.ChargeType == Constants.ChargeType.ManualJobAccrual)
			{
				AssertNull("When Internal Sell JRJ is posted against MJA charge type, No WIP should be posted against the second line (internal cost).", Factory.Load<WIP>(costCharge.JR_AL_ARLine));
			}
		}

		[ExpectNoExceptions]
		public void TestCreateJournalFromSellChargeWithNegativeAmountsAndWIPMustHaveDebtorAccrualMustHaveCreditor()
		{
			using (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				TestCreateJournalFromSellChargeWithNegativeAmounts();
			}
		}

		[ExpectNoExceptions]
		public void TestCreateJournalFromSellChargeWithNegativeAmountsAndWIPMustHaveDebtorAccrualMustHaveCreditorAndDSBChargeCode()
		{
			using (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetTemporaryValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				chargeCode1 = testObjectCreator.DSBChargeCode;
				TestCreateJournalFromSellChargeWithNegativeAmounts();
			}
		}

		public void TestCreateJournalFromSellChargeWithNegativeAmountsAndMJAChargeCode()
		{
			chargeCode1 = testObjectCreator.ManualJobAccrualChargeCode;
			TestCreateJournalFromSellChargeWithNegativeAmounts();
		}

		public void TestCreateJournalFromSellChargeWithNegativeAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToBoth()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				TestCreateJournalFromSellChargeWithNegativeAmounts();
			}
		}

		public void TestCreateJournalFromSellChargeWithNegativeAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToCost()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				TestCreateJournalFromSellChargeWithNegativeAmounts();
			}
		}

		public void TestCreateJournalFromSellChargeWithNegativeAmountsAndJobRevenueJournalGLAccountDefaultingRulesRegistrySetToRevenue()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				TestCreateJournalFromSellChargeWithNegativeAmounts();
			}
		}

		public void TestCreateJournalFromCostChargeWhenDebtorHasCFXandForeignDefaultCurrency()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				var costCharge = CreateCharge(usePositiveNumbers: true);

				costCharge.JR_OH_CostAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

				var debtor = testObjectCreator.Debtor;
				debtor.CompanyData.OB_RX_NKARDDefltCurrency = "USD";
				debtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 5m, 0.5m);

				costCharge.JR_OH_SellAccount = testObjectCreator.Debtor.PK;
				costCharge.JR_RX_NKCostCurrency = "USD";
				AssertEquals(1m, costCharge.JR_OSCostExRate);
				AssertEquals("CFX applied", 0.95m, costCharge.JR_OSSellExRate);
				costCharge.RunPreSaveValidation();
				AssertNoErrors(costCharge);

				AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(costCharge);
				var journal = costCharge.Factory.Load<JobRevenueJournal>(new ZQuery()).First();
				var costLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK == costCharge.JR_AL_APLine);
				var sellLine = journal.JournalLines.Cast<JobRevenueJournalLine>().FirstOrDefault(x => x.PK != costLine.PK);
				var sellCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, sellLine.PK));

				journal.RunPreSaveValidation();
				AssertNoErrors("journal", journal);

				AssertGeneratedChargeValuesMatchOriginalCharge(costCharge, sellCharge);
				AssertEquals("description", costCharge.JR_Desc, sellCharge.JR_Desc);
				AssertJRJLineMatchesCostValues(costCharge, costLine, true);
				AssertJRJLineMatchesSellValues(sellCharge, sellLine, true);
				Factory.Save();
				if ((costCharge.ChargeType == Constants.ChargeType.Margin && costCharge.MarginPercentage == 0) || costCharge.ChargeType == Constants.ChargeType.ManualJobAccrual)
				{
					var chargeTypeDetails = (costCharge.ChargeType == Constants.ChargeType.Margin) ? "MRG 0%" : "MJA";
					AssertNull(string.Format("When Internal Cost JRJ is posted against {0} charge type, No ACR should be posted against the second line (internal sell).", chargeTypeDetails), Factory.Load<Accrual>(sellCharge.JR_AL_APLine));
				}

				if (costCharge.IsMarginCharge)
				{
					SetMarginCharges(costCharge, sellCharge);
				}
			}
		}

		public void TestJRJSellCurrencyIsDefaultCurrencyOfDebtorARDefaultCurrencyWhenItIsAProxyOrganization()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var setup = testObjectCreator.CreateGatewayConsolsAndShipments();
				Factory.Save();

				using (var job = testObjectCreator.CreateJob(setup.gC0001))
				{
					var debtor = GlbCompany.CurrentCompany.OrgProxy;
					var debtorSisterOrgProxy = testObjectCreator.DebtorSisterOrgProxy;
					debtorSisterOrgProxy.CompanyData.OB_RX_NKARDDefltCurrency = "USD";
					debtorSisterOrgProxy.Factory.Save();

					var chargeCode = testObjectCreator.FRT;
					var currency = testObjectCreator.AUD;

					var charge1 = testObjectCreator.CreateCharge(job, chargeCode, "", currency, 100M, null, currency, 100M, debtor);
					charge1.JR_OH_SellAccount = debtor.PK;
					charge1.JR_JH_InternalJob = job.PK;
					charge1.JR_GB_InternalBranch = testObjectCreator.NonCurrentBranch.PK;
					charge1.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;

					Factory.Save();

					var charge2 = job.Charges.Cast<Charge>().SingleOrDefault(x => x.PK != charge1.PK && x.JR_AC == charge1.JR_AC);
					var sellAccount = charge2.SellAccount;
					AssertEquals(charge2.SellAccount.CompanyData.OB_RX_NKARDDefltCurrency, charge2.JR_RX_NKSellCurrency);
				}
			}
		}

		public void TestJRJCostCurrencyIsDefaultCurrencyOfCreditorAPDefaultCurrencyWhenItIsAProxyOrganization()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var setup = testObjectCreator.CreateGatewayConsolsAndShipments();
				Factory.Save();

				using (var job = testObjectCreator.CreateJob(setup.gC0001))
				{
					var debtor = testObjectCreator.Debtor;
					debtor.CompanyData.OB_RX_NKARDDefltCurrency = "USD";
					debtor.Factory.Save();

					var creditor = GlbCompany.CurrentCompany.OrgProxy;

					var chargeCode = testObjectCreator.FRT;
					var currency = testObjectCreator.AUD;

					var charge1 = testObjectCreator.CreateCharge(job, chargeCode, "", currency, 100M, creditor, currency, 100M, debtor);
					charge1.JR_OH_CostAccount = creditor.PK;
					charge1.JR_JH_InternalJob = job.PK;
					charge1.JR_GB_InternalBranch = testObjectCreator.NonCurrentBranch.PK;
					charge1.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;

					Factory.Save();

					var charge2 = job.Charges.Cast<Charge>().SingleOrDefault(x => x.PK != charge1.PK && x.JR_AC == charge1.JR_AC);
					AssertEquals(charge1.JR_RX_NKCostCurrency, charge2.JR_RX_NKCostCurrency);
				}
			}
		}

		#endregion

		public void TestCreateAutoJobRevenueJournal_ShouldUseCreateJobRevenueJournalNotInRevenueJournalModuleContext()
		{
			var costCharge = CreateCharge(usePositiveNumbers: true);

			if (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value)
			{
				costCharge.JR_OH_CostAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			}
			if (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value)
			{
				costCharge.JR_OH_SellAccount = testObjectCreator.Debtor.PK;
			}
			costCharge.JR_OSCostExRate = 1m;
			costCharge.RunPreSaveValidation();
			AssertNoErrors(costCharge);

			var jobCostingLevelSecurity = Env.Security.JobRevenueJournalAllowOverrideCostRevenueType;

			using (new DisposableAction(
				() => { jobCostingLevelSecurity.IsAllowed = false; },
				() => { jobCostingLevelSecurity.IsAllowed = true; }
			))
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(costCharge);
				AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(costCharge);
			}

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		static void AssertGeneratedChargeValuesMatchOriginalCharge(ChargeWithCost costCharge, ChargeWithCost sellCharge)
		{
			AssertEquals("charge code", costCharge.JR_AC, sellCharge.JR_AC);
			AssertEquals("currency", costCharge.JR_RX_NKCostCurrency, sellCharge.JR_RX_NKSellCurrency);
			AssertEquals("exchange rate", costCharge.JR_OSCostExRate, sellCharge.JR_OSSellExRate);
			AssertEquals("os amount", costCharge.JR_OSCostAmt, sellCharge.JR_OSSellAmt);
			AssertEquals("local amount", costCharge.JR_LocalCostAmt, sellCharge.JR_LocalSellAmt);

			if (costCharge.JR_GE_InternalDept.IsValid)
			{
				AssertEquals("cost charge internal department = sell charge department", costCharge.JR_GE_InternalDept, sellCharge.JR_GE);
			}

			if (costCharge.JR_GB_InternalBranch.IsValid)
			{
				AssertEquals("cost charge internal branch = sellcharge branch", costCharge.JR_GE_InternalDept, sellCharge.JR_GE);
			}

			if (costCharge.JR_JH_InternalJob.IsValid)
			{
				AssertEquals("cost charge internal job = sellcharge job", costCharge.JR_JH_InternalJob, sellCharge.JR_JH);
			}

			if (sellCharge.JR_GE_InternalDept.IsValid)
			{
				AssertEquals("sell charge internal department = cost charge department", sellCharge.JR_GE_InternalDept, costCharge.JR_GE);
			}

			if (sellCharge.JR_GB_InternalBranch.IsValid)
			{
				AssertEquals("sell charge internal branch = costcharge branch", sellCharge.JR_GE_InternalDept, costCharge.JR_GE);
			}

			if (sellCharge.JR_JH_InternalJob.IsValid)
			{
				AssertEquals("sell charge internal job = costcharge job", sellCharge.JR_JH_InternalJob, costCharge.JR_JH);
			}
		}

		static void AssertJRJLineMatchesCostValues(ChargeWithCost charge, JobRevenueJournalLine line, ZBool isCreatingJournalFromCostCharge)
		{
			var isBoth = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.Value == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code;
			var amountsArePositive = charge.JR_LocalCostAmt > 0;
			var unsignedOsAmount = amountsArePositive ? (decimal)charge.JR_OSCostAmt : -charge.JR_OSCostAmt;
			var unsignedLocalAmount = amountsArePositive ? (decimal)charge.JR_LocalCostAmt : -charge.JR_LocalCostAmt;
			var expectedSign = amountsArePositive || isBoth ? DebitCreditDataEntry.DR : DebitCreditDataEntry.CR;

			AssertEquals(JobChargeSchema.JR_AL_APLine.Name, charge.JR_AL_APLine, line.PK);
			AssertEquals("charge code", charge.JR_AC, line.AL_AC);
			AssertEquals("job", charge.JR_JH, line.AL_JH);
			AssertEquals("description", charge.JR_Desc, line.AL_Desc);
			AssertEquals("branch", charge.JR_GB, line.AL_GB);
			AssertEquals("department", charge.JR_GE, line.AL_GE);
			AssertEquals("currency", charge.JR_RX_NKCostCurrency, line.AL_RX_NKTransactionCurrency);
			AssertEquals("exchange rate", charge.JR_OSCostExRate, line.AL_ExchangeRate);
			AssertEquals("os amount", unsignedOsAmount, line.OSUnsignedLineAmount);
			AssertEquals("local amount", unsignedLocalAmount, line.LocalUnsignedLineAmount);
			AssertEquals("debit credit sign", expectedSign, line.DebitCreditSign);
			AssertEquals(AccTransactionLines.Schema.AL_LineAmount, -charge.JR_LocalCostAmt, line.AL_LineAmount);
			AssertEquals(AccTransactionLines.Schema.AL_OSAmount, -charge.JR_OSCostAmt, line.AL_OSAmount);
			AssertPostToGLAccount(charge, line, isCreatingJournalFromCostCharge);
		}

		static void AssertJRJLineMatchesSellValues(ChargeWithCost charge, JobRevenueJournalLine line, ZBool isCreatingJournalFromCostCharge)
		{
			var isBoth = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.Value == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code;
			var amountsArePositive = charge.JR_LocalSellAmt > 0;
			var unsignedOsAmount = amountsArePositive ? (decimal)charge.JR_OSSellAmt : -charge.JR_OSSellAmt;
			var unsignedLocalAmount = amountsArePositive ? (decimal)charge.JR_LocalSellAmt : -charge.JR_LocalSellAmt;
			var expectedSign = amountsArePositive || isBoth ? DebitCreditDataEntry.CR : DebitCreditDataEntry.DR;

			AssertEquals(JobChargeSchema.JR_AL_ARLine.Name, charge.JR_AL_ARLine, line.PK);
			AssertEquals("charge code", charge.JR_AC, line.AL_AC);
			AssertEquals("job", charge.JR_JH, line.AL_JH);
			AssertEquals("description", charge.JR_Desc, line.AL_Desc);
			AssertEquals("branch", charge.JR_GB, line.AL_GB);
			AssertEquals("department", charge.JR_GE, line.AL_GE);
			AssertEquals("currency", charge.JR_RX_NKSellCurrency, line.AL_RX_NKTransactionCurrency);
			AssertEquals("exchange rate", charge.JR_OSSellExRate, line.AL_ExchangeRate);
			AssertEquals("os amount", unsignedOsAmount, line.OSUnsignedLineAmount);
			AssertEquals("local amount", unsignedLocalAmount, line.LocalUnsignedLineAmount);
			AssertEquals("debit credit sign", expectedSign, line.DebitCreditSign);
			AssertEquals(AccTransactionLines.Schema.AL_LineAmount, charge.JR_LocalSellAmt, line.AL_LineAmount);
			AssertEquals(AccTransactionLines.Schema.AL_OSAmount, charge.JR_OSSellAmt, line.AL_OSAmount);
			AssertPostToGLAccount(charge, line, isCreatingJournalFromCostCharge);
		}

		static void AssertPostToGLAccount(ChargeWithCost charge, JobRevenueJournalLine line, ZBool isCreatingJournalFromCostCharge)
		{
			var jobRevenueJournalGLAccountDefaultingRulesRegistrySetting = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(line.AL_GC.ToGuid(), Guid.Empty, Guid.Empty);
			if (jobRevenueJournalGLAccountDefaultingRulesRegistrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code)
			{
				var expectedGLAccount = isCreatingJournalFromCostCharge ? charge.ChargeCode.AC_AG_CostAccount : charge.ChargeCode.AC_AG_RevenueAccount;
				AssertEquals("post to GL Account", expectedGLAccount, line.AL_AG);
			}
			else if (jobRevenueJournalGLAccountDefaultingRulesRegistrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code)
			{
				AssertEquals("post to GL Account", charge.ChargeCode.AC_AG_CostAccount, line.AL_AG);
			}
			else if (jobRevenueJournalGLAccountDefaultingRulesRegistrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code)
			{
				AssertEquals("post to GL Account", charge.ChargeCode.AC_AG_RevenueAccount, line.AL_AG);
			}
		}

		ChargeWithCost CreateCharge(bool usePositiveNumbers)
		{
			var osAmount = usePositiveNumbers ? 50m : -50m;
			var localAmount = usePositiveNumbers ? 100m : -100m;

			var shipment1 = testObjectCreator.CreateShipment("S00001111");
			var shipment2 = testObjectCreator.CreateShipment("S00002222");
			job1 = testObjectCreator.CreateJob(shipment1, false, false);
			job2 = testObjectCreator.CreateJob(shipment2, false, false);

			var charge = Factory.New<Charge>();
			charge.JR_AC = chargeCode1.PK;
			charge.JR_Desc = "charge code description";
			charge.JR_JH = job1.PK;
			charge.JR_GB = testObjectCreator.CreateBranch("111", GlbCompany.CurrentCompany).PK;
			charge.JR_GE = testObjectCreator.FESDepartment.PK;
			charge.JR_JH_InternalJob = job2.PK;
			charge.JR_GB_InternalBranch = testObjectCreator.CreateBranch("222", GlbCompany.CurrentCompany).PK;
			charge.JR_GE_InternalDept = testObjectCreator.FISDepartment.PK;
			charge.JR_RX_NKCostCurrency = testObjectCreator.USD.RX_Code;
			charge.JR_RX_NKSellCurrency = testObjectCreator.USD.RX_Code;

			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			AssertEquals(2, job1.ExchangeRates.Count);

			charge.JR_OSCostAmt = osAmount;
			charge.JR_LocalCostAmt = localAmount;

			charge.JR_OSSellAmt = osAmount;
			charge.JR_LocalSellAmt = localAmount;

			AssertEquals("Precondition: charge.JR_OSCostAmt", osAmount, charge.JR_OSCostAmt);
			AssertEquals("Precondition: charge.JR_LocalCostAmt", localAmount, charge.JR_LocalCostAmt);
			AssertEquals("Precondition: charge.JR_OSSellAmt", osAmount, charge.JR_OSSellAmt);
			AssertEquals("Precondition: charge.JR_LocalSellAmt", localAmount, charge.JR_LocalSellAmt);

			return charge;
		}

		void SetMarginCharges(ChargeWithCost costCharge, ChargeWithCost sellCharge)
		{
			costMarginCharge = costCharge;
			sellMarginCharge = sellCharge;
		}

		protected override void SetUp()
		{
			base.SetUp();

			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(ZDateTime.Now.Year, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			testObjectCreator = new TestObjectCreator(Factory);
			chargeCode1 = testObjectCreator.CreateChargeCode("1111");
			chargeCode1.AC_Desc = "charge code description";

			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
		}

		TestObjectCreator testObjectCreator;
		AccChargeCode chargeCode1;
		ChargeWithCost costMarginCharge;
		ChargeWithCost sellMarginCharge;
		Job job1;
		Job job2;
	}
}
