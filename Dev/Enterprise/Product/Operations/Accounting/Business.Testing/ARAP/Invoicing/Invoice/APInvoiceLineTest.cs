using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.CostVarianceApprovalExtension;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceLine))]
	public class APInvoiceLineTest : InvoiceLineTest
	{
		public void TestChargeCodeChangeDoesntChangeOtherFields()
		{
			TestChargeCodeChangeDoesntChangeOtherFields(tpa => TransactionAllocationConverter.ConvertUnallocatedToAP(tpa).Invoice);
		}

		public virtual void TestReopenClosedJobWithAPInvoiceLine()
		{
			bool oldAllowReopenJob = Env.Security.ReopenJob.IsAllowed;
			try
			{
				ForwardingShipment shipment = TestObjectCreator.CreateShipment("00001");
				Job job = TestObjectCreator.CreateJob(shipment);

				var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
				apInvoice.AH_TransactionNum = "CN001";
				apInvoice.AH_JH = job.PK;

				var apLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1M, 100M);
				apLine.AL_JH = job.PK;

				TestObjectCreator.CreateCharge(apLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

				Env.Security.ReopenJob.IsAllowed = false;

				job.JH_Status = JobHeaderStatus.Closed.Code;

				Factory.Save();
				AssertEquals("[ReopenJob Not Allowed] Job Status still set to CLS", JobHeaderStatus.Closed.Code, apLine.Job.JH_Status);

				Env.Security.ReopenJob.IsAllowed = true;

				apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
				apInvoice.AH_TransactionNum = "CN002";
				apInvoice.AH_JH = job.PK;

				apLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1M, 100M);
				apLine.AL_JH = job.PK;

				TestObjectCreator.CreateCharge(apLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

				job.JH_Status = JobHeaderStatus.Closed.Code;

				Factory.Save();
				AssertEquals("[ReopenJob Allowed] Job Status set to WRK", JobHeaderStatus.Working.Code, apLine.Job.JH_Status);
			}
			finally
			{
				Env.Security.ReopenJob.IsAllowed = oldAllowReopenJob;
			}
		}

		public void TestJobReopenNoSequrityRight()
		{
			Env.Security.ReopenJob.IsAllowed = false;
			Factory.SetContext(BusinessContext.AllowReopenJobWhenImporting);

			var objectCreator = new TestObjectCreator(Factory);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = objectCreator.CreateJob(shipment);
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var apLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1M, 100M);
			apLine.AL_JH = job.PK;
			apLine.AL_AC = objectCreator.CC1.PK;
			objectCreator.CreateJobCharge(apLine, job, objectCreator.CC1, objectCreator.AUD);
			Factory.Save();

			AssertEquals("Job should be reopened", JobHeaderStatus.Working.Code, job.JH_Status);
		}

		public void TestAL_ExchangeRate_ReadOnly()
		{
			APInvoice.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			Line.AL_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			APInvoice.AH_PostedToEFT = false;
			AssertEquals(true, Line.AL_ExchangeRateInfo.ReadOnly);
			APInvoice.AH_PostedToEFT = true;
			AssertEquals(false, Line.AL_ExchangeRateInfo.ReadOnly);
			Env.Security.AllowAPInvoiceLineExchangeRateOverride.IsAllowed = false;
			AssertEquals(true, Line.AL_ExchangeRateInfo.ReadOnly);
			Env.Security.AllowAPInvoiceLineExchangeRateOverride.IsAllowed = true;
			AssertEquals(false, Line.AL_ExchangeRateInfo.ReadOnly);
		}

		public void TestValidationForIncompleteTransactionLine()
		{
			var invoiceLine = (InvoicingLineBase)new BusinessObjectFactory().New(GetExpectedBusinessObjectType());

			invoiceLine.Factory.SetContext(BusinessContext.SavingIncompleteTransaction);
			AssertEquals("Validation Type for Incomplete", typeof(IncompleteInvoicingLineBaseValidation), invoiceLine.Validation.GetType());

			invoiceLine.Factory.RemoveContext(BusinessContext.SavingIncompleteTransaction);
			AssertNotEquals("Validation Type for Regular", typeof(IncompleteInvoicingLineBaseValidation), invoiceLine.Validation.GetType());
		}

		public void TestAL_IsFinalAfterPopulatingFromImportedApportionment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			Factory.Save();

			var invoice = Factory.New<APInvoice>();

			try
			{
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_OSCostAmount = 100m;
				cost.E6_ApportionmentMethod = "SHP";
				cost.SetIsUsedForApportionment();

				invoice.ImportAllApportionmentsFromCosting();

				Assert("Should be set by default", invoice.Lines[0].AL_IsFinalCharge);
				Assert("Should NOT be readonly", !invoice.Lines[0].AL_IsFinalChargeInfo.ReadOnly);
				Assert("Should not have error", !invoice.Lines[0].AL_IsFinalChargeInfo.HasErrors());

				Assert("Should be set by default", invoice.Lines[1].AL_IsFinalCharge);
				Assert("Should NOT be readonly", !invoice.Lines[1].AL_IsFinalChargeInfo.ReadOnly);
				Assert("Should not have error", !invoice.Lines[1].AL_IsFinalChargeInfo.HasErrors());

				cost.IsFinal = ZBool.False;
				invoice.ImportAllApportionmentsFromCosting();

				Assert("Should be set from cost line", !invoice.Lines[0].AL_IsFinalCharge);
				Assert("Should NOT be readonly", !invoice.Lines[0].AL_IsFinalChargeInfo.ReadOnly);
				Assert("Should not have error", !invoice.Lines[0].AL_IsFinalChargeInfo.HasErrors());

				Assert("Should be set by default", !invoice.Lines[1].AL_IsFinalCharge);
				Assert("Should NOT be readonly", !invoice.Lines[1].AL_IsFinalChargeInfo.ReadOnly);
				Assert("Should not have error", !invoice.Lines[1].AL_IsFinalChargeInfo.HasErrors());

				cost.ApportionmentCharges[0].IsFinal = ZBool.False;
				cost.ApportionmentCharges[1].IsFinal = ZBool.True;

				invoice.ImportAllApportionmentsFromCosting();

				AssertEquals("Should be set from cost line", invoice.Lines[0].ApportionmentChargeImportedFrom.IsFinal, invoice.Lines[0].AL_IsFinalCharge);
				Assert("Should NOT be readonly", !invoice.Lines[0].AL_IsFinalChargeInfo.ReadOnly);
				Assert("Should not have error", !invoice.Lines[0].AL_IsFinalChargeInfo.HasErrors());

				AssertEquals("Should be set by default", invoice.Lines[1].ApportionmentChargeImportedFrom.IsFinal, invoice.Lines[1].AL_IsFinalCharge);
				Assert("Should NOT be readonly", !invoice.Lines[1].AL_IsFinalChargeInfo.ReadOnly);
				Assert("Should not have error", !invoice.Lines[1].AL_IsFinalChargeInfo.HasErrors());
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestTransactionLineLocalAmountInconsistentWithOSAmount()
		{
			var invoice = new TransactionCreator().CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice) as APInvoice;
			var disposableAction = new DisposableAction(
				() => ((AccountingSuspenders.IRunMethodSuspending)invoice.Lines[0]).RunMethodSuspended = true,
				() => ((AccountingSuspenders.IRunMethodSuspending)invoice.Lines[0]).RunMethodSuspended = false);
			var line = invoice.Lines[0];
			line.AL_RX_NKTransactionCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			line.AL_OSExTaxAmount = 120m;
			line.AL_LocalExTaxAmount = 100m;
			invoice.AH_RX_NKTransactionCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			invoice.AH_OSExTaxAmount = 120m;
			invoice.AH_LocalExTaxAmount = 100m;

			// Manually trigger critical validation info getter, to simulate same critical validation error happens twice. So that CriticalValidationInfoCollectorService can record additional info with default CollectionFrequency.
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineLocalAmountInconsistentWithOSAmountWhenExRateIsOne);

			using (disposableAction)
			{
				line.AL_RX_NKTransactionCurrency = Enterprise.Core.Constants.CurrencyCodes.Australia;

				AssertEquals(invoice.AH_OSExTaxAmount, line.AL_OSExTaxAmount);
				AssertEquals(invoice.AH_LocalExTaxAmount, line.AL_LocalExTaxAmount);
				AssertNotEquals(line.AL_OSExTaxAmount, line.AL_LocalExTaxAmount);
				AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.Code, line.AL_RX_NKTransactionCurrency);
				AssertEquals(1m, line.AL_ExchangeRate);
			}

			var ex = AssertExceptionThrown<OnSavingCriticalCheckException<AccTransactionLines>>(() => Factory.Save());

			AssertContains("Line: PK", ex.DeveloperErrorMessage);
			AssertContains("Header: PK", ex.DeveloperErrorMessage);
			AssertContains(@"TransactionLineLocalAmountInconsistentWithOSAmountWhenExRateIsOne:
Transaction line local amount is inconsistent with OS amount while exchange rate is 1.
Currency changed from USD to AUD.
Exchange rate changed from 1 to 1.
Line local exclude tax amount changed from 100 to 100.
Line OS exclude tax amount changed from 120 to 120.
Global Company Currency is AUD.
Line RunMethodSuspended is True.
Call Stack when line currency changed:
   at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase", ex.DeveloperErrorMessage);

			ErrorReporter.Instance.Clear();
		}

		public void TestNotAutoTickFinalFlagWhenSuspendedAndAutoTickByResumeActionAfterSuspended()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1) as APInvoice;
			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));

			var approvalValue = new CostVarianceApproval();
			approvalValue.AutoTickFinalFlag = true;
			approvalValue.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			approvalValue.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJobChargeCode;
			CostVarianceApprovalAuthorisationRequirement upTo = approvalValue.AuthorisationRequirements.AddNew();
			upTo.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo.Range = RangeCodes.UpTo;
			upTo.Amount = 80M;
			CostVarianceApprovalAuthorisationRequirement above = approvalValue.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			above.Range = RangeCodes.Above;
			above.Amount = upTo.Amount;

			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, approvalValue);
			invoice.Factory.ClearCachedCostVarianceApproval(GlbCompany.CurrentCompany);

			APInvoiceLine line;

			using (invoice.GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender(true).GetSuspender())
			{
				line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 50M) as APInvoiceLine;
				AssertEquals(false, line.AL_IsFinalCharge);
				AssertEquals(1, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);
			}

			AssertEquals(true, line.AL_IsFinalCharge);
			AssertEquals(2, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);

			job.Dispose();
		}

		public void TestAutoTickFinalFlagForLinesIfMonitorTotalInvoiceVariance()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1) as APInvoice;
			Job job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Job job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));

			var approvalValue = new CostVarianceApproval();
			approvalValue.AutoTickFinalFlag = true;
			approvalValue.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			approvalValue.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJobChargeCode;
			CostVarianceApprovalAuthorisationRequirement upTo = approvalValue.AuthorisationRequirements.AddNew();
			upTo.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo.Range = RangeCodes.UpTo;
			upTo.Amount = 80M;
			CostVarianceApprovalAuthorisationRequirement above = approvalValue.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			above.Range = RangeCodes.Above;
			above.Amount = upTo.Amount;

			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, approvalValue);
			invoice.Factory.ClearCachedCostVarianceApproval(GlbCompany.CurrentCompany);

			APInvoiceLine line1;
			APInvoiceLine line2;
			using (invoice.GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender(true).GetSuspender())
			{
				line1 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.CC1, 50M) as APInvoiceLine;
				line2 = TestObjectCreator.CreateInvoiceLine(invoice, job2, TestObjectCreator.CC1, 90M) as APInvoiceLine;
			}
			AssertEquals(true, line1.AL_IsFinalCharge);
			AssertEquals(false, line2.AL_IsFinalCharge);

			upTo.TotalInvoiceVarianceAmount = 130M;
			above.TotalInvoiceVarianceAmount = 130M;
			upTo.MonitorTotalInvoiceVariance = true;
			above.MonitorTotalInvoiceVariance = true;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, approvalValue);
			invoice.Factory.ClearCachedCostVarianceApproval(GlbCompany.CurrentCompany);

			using (invoice.GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender(true).GetSuspender())
			{
				line1 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.CC1, 50M) as APInvoiceLine;
				line2 = TestObjectCreator.CreateInvoiceLine(invoice, job2, TestObjectCreator.CC1, 90M) as APInvoiceLine;
			}
			AssertEquals(false, line1.AL_IsFinalCharge);
			AssertEquals(false, line2.AL_IsFinalCharge);

			job1.Dispose();
			job2.Dispose();
		}

		public override void TestGenericJobCollection()
		{
			AssertNotNull("Job Collection should be instantiated", InvoicingLine.JobList);

			ForwardingConsol testConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			testConsol.JK_IsForwarding = true;
			ForwardingShipment testShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			InvoicingLine.JobList.Load();
			Assert("The shipment should be in the job list", InvoicingLine.JobList.Contains(testShipment.PK));
			Assert("The consol should be in the job list", InvoicingLine.JobList.Contains(testConsol.PK));
		}

		bool ShowChargesEventRaised;

		void APInvoiceLineTest_ShowChargesForImportEvent(object sender, EventArgs e)
		{
			ShowChargesEventRaised = true;
		}

		public override void TestFallbackTaxRate()
		{
			JobCharge importedJobCharge = Factory.NewWithValidTestData<JobCharge>();
			Charge lineCharge = Factory.NewWithValidTestData<Charge>();
			lineCharge.JR_AL_APLine = APLine.PK;

			AccTaxRate taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			importedJobCharge.JR_AT_CostGSTRate = taxRate1.PK;

			ZGuid overrideInvTaxMsg = ZGuid.Empty;

			APLine.OriginalJobCharge = importedJobCharge;
			AssertEquals("Fallback TaxRate should be the TaxRate on the charge - TaxRate1", taxRate1.PK, APLine.GetFallbackTaxRate_ForTestOnly(out overrideInvTaxMsg).PK);
			lineCharge.JR_AT_CostGSTRate = ZGuid.Empty;
			APLine.OriginalJobCharge = null;

			base.TestFallbackTaxRate();
		}

		public void TestChargesNotShownWhenDefaultingGenericJobForNewChild()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var accr = TestObjectCreator.CreateAccrual(job);
			accr.AL_ReverseDate = ZDateTime.Empty;
			Factory.Save();

			APLine.InvoiceBase.SetIsReversing(false);
			APLine.InvoiceBase.SubmittedFromInvoicingForm = true;
			APLine.ShowJobChargesForImportEvent += APInvoiceLineTest_ShowChargesForImportEvent;
			ShowChargesEventRaised = false;
			APLine.SetDefaultAL_JH(job.PK);
			Assert("Show Charges event should not be raised", !ShowChargesEventRaised);
		}

		public override void TestSetDefaultAL_JH()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreator = new TestObjectCreator(newFactory);
			ForwardingShipment shipment = testObjectCreator.CreateShipment("S00001234");
			Job job = testObjectCreator.CreateJob(shipment);
			newFactory.Save();

			InvoicingLine.InvoiceBase.SetIsReversing(true);
			InvoicingLine.InvoiceBase.SubmittedFromInvoicingForm = false;
			InvoicingLine.SetDefaultAL_JH(job.PK);
			Assert("AL_JH on APInvoiceLine should not be set", InvoicingLine.AL_JH.IsEmpty);

			InvoicingLine.InvoiceBase.SetIsReversing(false);
			InvoicingLine.SetDefaultAL_JH(job.PK);
			Assert("AL_JH on APInvoiceLine should not be set", InvoicingLine.AL_JH.IsEmpty);

			InvoicingLine.InvoiceBase.SetIsReversing(true);
			InvoicingLine.InvoiceBase.SubmittedFromInvoicingForm = true;
			InvoicingLine.SetDefaultAL_JH(job.PK);
			Assert("AL_JH on APInvoiceLine should not be set", InvoicingLine.AL_JH.IsEmpty);

			InvoicingLine.InvoiceBase.SetIsReversing(false);
			InvoicingLine.SetDefaultAL_JH(job.PK);
			AssertEquals("AL_JH on APInvoiceLine should be set", job.PK, InvoicingLine.AL_JH);
		}

		public override void TestDefaultPreviousLineJobToNextLine()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ForwardingShipment shipment = newFactory.NewWithValidTestData<ForwardingShipment>();
			Job job1 = new TestObjectCreator(newFactory).CreateJob(shipment);
			newFactory.Save();

			InvoicingBase invBase = (InvoicingBase)Factory.NewWithValidTestData(MasterHeaderType);
			invBase.SubmittedFromInvoicingForm = false;
			invBase.SetIsReversing(true);
			InvoicingLineBase invLine = invBase.Lines.AddNew() as InvoicingLineBase;
			invLine.AL_JH = job1.PK;

			invBase.Lines.AddNew();
			Assert("AL_JH should not be copied to the next line", invBase.Lines[1].AL_JH.IsEmpty);

			invBase.Lines.Remove(invBase.Lines[1]);
			invBase.SubmittedFromInvoicingForm = true;
			invBase.Lines.AddNew();
			Assert("AL_JH should not be copied to the next line", invBase.Lines[1].AL_JH.IsEmpty);

			invBase.Lines.Remove(invBase.Lines[1]);
			invBase.SetIsReversing(false);
			invBase.SubmittedFromInvoicingForm = false;
			invBase.Lines.AddNew();
			Assert("AL_JH should not be copied to the next line", invBase.Lines[1].AL_JH.IsEmpty);

			invBase.Lines.Remove(invBase.Lines[1]);
			invBase.SubmittedFromInvoicingForm = true;
			invBase.Lines.AddNew();
			AssertEquals("AL_JH should be copied to the next line", job1.PK, invBase.Lines[1].AL_JH);
		}

		public void TestAL_ReverseDateWhenRevenueRecognitionSetupChanged()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job testJob = new TestObjectCreator(Factory).CreateJob(shipment);

			Charge charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC2, "Charge 2", TestObjectCreator.AUD, 1m, TestObjectCreator.CreateOrgHeader("Two", true, false), TestObjectCreator.AUD, 1m, TestObjectCreator.CreateOrgHeader("AgentTwo", false, true));
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_ChequeNo = "10001";
			Factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

			valuesForTest = new RevenueRecognitionCollection();
			setting = valuesForTest.AddNew();
			setting.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			postManager = new InvoicingPostManager(testJob);
			InvoicingBase[] invoicelist = postManager.CreateTransactions(JobInvoicingPostingOption.Costs).GetAllAPInvoicesAndCreditNotes();
			Factory.Save();
			AssertNotEquals("AL_ReverseDate can't be 1900/01/01", ZDateTime.MinSmallDateTimeValue, invoicelist[0].Lines[0].AL_ReverseDate);
			AssertEquals("AL_ReverseDate must be equal to AH_PostDate", testJob.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction), invoicelist[0].Lines[0].AL_ReverseDate);
		}

		[TestDate(2005, 2, 2)]
		public void TestAL_ReverseDate()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "J0000456";
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Charge charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge 1", TestObjectCreator.AUD, 1m, TestObjectCreator.CreateOrgHeader("One", true, false), TestObjectCreator.AUD, 1m, TestObjectCreator.CreateOrgHeader("AgentOne", false, true));
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_ChequeNo = "10001";
			charge.JR_GE = TestObjectCreator.FESDepartment.PK;

			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			InvoicingBase[] invoicelist = postManager.CreateTransactions(JobInvoicingPostingOption.Costs).GetAllAPInvoicesAndCreditNotes();
			Factory.Save();

			AssertEquals("AL_ReverseDate must be equal AL_PostDate", ZDateTime.Today, invoicelist[0].Lines[0].AL_ReverseDate.Date);

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			Factory.Save();

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			Job testJob2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			testJob2.JH_GB = Env.CurrentBranch.PK;
			testJob2.JH_GE = Env.CurrentDepartment.PK;
			testJob2.JH_JobNum = "J0000457";

			Charge charge2 = TestObjectCreator.CreateCharge(testJob2, TestObjectCreator.CC2, "Charge 2", TestObjectCreator.AUD, 1m, TestObjectCreator.CreateOrgHeader("Two", true, false), TestObjectCreator.AUD, 1m, TestObjectCreator.CreateOrgHeader("AgentTwo", false, true));
			charge2.JR_APInvoiceNum = "5";
			charge2.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge2.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge2.JR_ChequeNo = "10001";
			charge2.JR_GE = TestObjectCreator.FESDepartment.PK;

			InvoicingPostManager postManager2 = new InvoicingPostManager(testJob2);
			InvoicingBase[] invoicelist2 = postManager2.CreateTransactions(JobInvoicingPostingOption.Costs).GetAllAPInvoicesAndCreditNotes();
			Factory.Save();
			AssertEquals("AL_ReverseDate must be empty", ZDateTime.Empty, invoicelist2[0].Lines[0].AL_ReverseDate);

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(200502, new ZDateTime(2005, 2, 1), new ZDateTime(2005, 2, 28));

			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			Job testJob3 = new Job.Loader(Factory, shipment3).TryCreateWithoutMutexForTestOnly();
			testJob3.JH_GB = Env.CurrentBranch.PK;
			testJob3.JH_GE = Env.CurrentDepartment.PK;
			testJob3.JH_JobNum = "J0000458";
			AssertNull("Accounting period must be setup for RevenueRecognition date", new AccountingPeriodCalculator(Factory).GetPeriodManagementFromDate(Job.GetRevenueRecognitionDate(Job.GetRevenueRecognitionType(TestObjectCreator.CC1))));

			Charge charge3 = TestObjectCreator.CreateCharge(testJob3, TestObjectCreator.CC3, "Charge 3", TestObjectCreator.AUD, 1m, TestObjectCreator.CreateOrgHeader("Three", true, false), TestObjectCreator.AUD, 1m, TestObjectCreator.CreateOrgHeader("AgentThree", false, true));
			charge3.JR_APInvoiceNum = "6";
			charge3.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge3.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge3.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge3.JR_ChequeNo = "10002";
			charge3.JR_GE = TestObjectCreator.FESDepartment.PK;

			InvoicingPostManager postManager3 = new InvoicingPostManager(testJob3);
			InvoicingBase[] invoicelist3 = postManager3.CreateTransactions(JobInvoicingPostingOption.All).GetAllAPInvoicesAndCreditNotes();
			Factory.Save();
			AssertEquals("AL_ReverseDate must be equal to Revenue Recognition Date ", new ZDateTime(2005, 2, 2), invoicelist3[0].Lines[0].AL_ReverseDate);
		}

		public void TestNotUpdateExchangeRateWhenExRateOptionIsEITFromBulkConsolCostImport()
		{
			using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code))
			{
				var shipment = TestObjectCreator.CreateShipment("S001001");
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Creditor1);
				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "test001");
				invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

				var charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
				charge.FillWithValidTestData();
				charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
				charge.JR_OSCostAmt = 100m;

				invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges.Add(charge);
				invoice.AH_ExchangeRate = 1.1m;

				invoice.Lines[0].ImportFromApportionSplitCharge(charge);
				AssertEquals(1.1m, invoice.AH_ExchangeRate);
			}
		}

		#region TEST: Set Values With GST Applicable WHT Applicable

		public void TestSetValuesWithGSTApplicableWHTApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			ZECTRA.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);
			ZECTRA.MiscServ.OM_APWHTApplicable = true;
			Charge charge = CreateCharge(job, MRG100, "Cost Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);

			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			invoiceLine.SetCostValues(job, charge);

			AssertEquals("Line Type", TransactionLineTypes.Cost, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Cost Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);

			AssertEquals("Currency", USD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", .7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertEquals("Post Date", ZDateTime.Empty, invoiceLine.AL_PostDate);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", newAPInvoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, invoiceLine.AL_GE);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, invoiceLine.AL_GB);
			AssertEquals("General Ledger", ZGuid.Empty, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ZECTRA.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("Cost Tax", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("Cost WHT", WHT1.PK, invoiceLine.AL_AW);

			AssertEquals("OS Ex Tax Amount", 250M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 25M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 12.50M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 275M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 357.14M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 35.71M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 17.86M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 392.85M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -357.14M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("GST Amount", -35.71M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", WHT1.PK, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", -17.86M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", -275.00M, invoiceLine.AL_OSAmount);
			AssertEquals("Place of Supply", PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, invoiceLine.AL_PlaceOfSupply);
			AssertEquals("Place of Supply Type", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(invoiceLine.Company, invoiceLine.AL_PlaceOfSupply), invoiceLine.AL_PlaceOfSupplyType);
		}

		#endregion

		#region TEST: Set Values With GST Applicable WHT Not Applicable

		public void TestSetValuesWithGSTApplicableWHTNotApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			ZECTRA.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);
			ZECTRA.MiscServ.OM_APWHTApplicable = false;
			Charge charge = CreateCharge(job, MRG100, "Cost Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);

			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			invoiceLine.SetCostValues(job, charge);

			AssertEquals("Line Type", TransactionLineTypes.Cost, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Cost Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", USD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", .7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertEquals("Post Date", ZDateTime.Empty, invoiceLine.AL_PostDate);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", newAPInvoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, invoiceLine.AL_GE);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, invoiceLine.AL_GB);
			AssertEquals("General Ledger", ZGuid.Empty, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ZECTRA.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("Cost Tax", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("Cost WHT", ZGuid.Empty, invoiceLine.AL_AW);

			AssertEquals("OS Ex Tax Amount", 250M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 25M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 275M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 357.14M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 35.71M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 0M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 392.85M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -357.14M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("GST Amount", -35.71M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", ZGuid.Empty, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 0M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", -275.00M, invoiceLine.AL_OSAmount);
			AssertEquals("Place of Supply", PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, invoiceLine.AL_PlaceOfSupply);
			AssertEquals("Place of Supply Type", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(invoiceLine.Company, invoiceLine.AL_PlaceOfSupply), invoiceLine.AL_PlaceOfSupplyType);
		}

		#endregion

		#region TEST: Test AP InvoiceLineValidation is NULL

		public void TestAPInvLineValidationIsNull()
		{
			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			newAPInvoice.Lines.Add(invoiceLine);
			newAPInvoice.AH_IsCancelled = true;

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;
			Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = false;
			invoiceLine.AL_IsFinalCharge = true;
			Assert("There should be no errors in AL_IsFinal property.", !invoiceLine.AL_IsFinalChargeInfo.HasErrors());

			newAPInvoice.AH_IsCancelled = false;
			invoiceLine.AL_IsFinalCharge = true;
			Assert("There should be errors in AL_IsFinal property.", invoiceLine.AL_IsFinalChargeInfo.HasErrors());
		}

		#endregion

		#region TEST: Set Values With GST Not Applicable WHT Applicable

		public void TestSetValuesWithGSTNotApplicableWHTApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			ZECTRA.CompanyData.SetAPTaxApplicable(false);
			ZECTRA.MiscServ.OM_APWHTApplicable = true;
			Charge charge = CreateCharge(job, MRG100, "Cost Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);

			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			invoiceLine.SetCostValues(job, charge);

			AssertEquals("Line Type", TransactionLineTypes.Cost, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Cost Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", USD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", .7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertEquals("Post Date", ZDateTime.Empty, invoiceLine.AL_PostDate);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", newAPInvoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, invoiceLine.AL_GE);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, invoiceLine.AL_GB);
			AssertEquals("General Ledger", ZGuid.Empty, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ZECTRA.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("Cost Tax", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("Cost WHT", WHT1.PK, invoiceLine.AL_AW);

			AssertEquals("OS Ex Tax Amount", 250M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 12.50M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 250M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 357.14M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 0M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 17.86M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 357.14M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -357.14M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("GST Amount", 0M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", WHT1.PK, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", -17.86M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", -250.00M, invoiceLine.AL_OSAmount);
			AssertEquals("Place of Supply", PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, invoiceLine.AL_PlaceOfSupply);
			AssertEquals("Place of Supply Type", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(invoiceLine.Company, invoiceLine.AL_PlaceOfSupply), invoiceLine.AL_PlaceOfSupplyType);
		}

		#endregion

		#region TEST: Set Values With GST Not Applicable WHT Not Applicable

		public void TestSetValuesWithGSTNotApplicableWHTNotApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			ZECTRA.CompanyData.SetAPTaxApplicable(false);
			ZECTRA.MiscServ.OM_APWHTApplicable = false;
			Charge charge = CreateCharge(job, MRG100, "Cost Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);

			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			invoiceLine.SetCostValues(job, charge);

			AssertEquals("Line Type", TransactionLineTypes.Cost, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Cost Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", USD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", .7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertEquals("Post Date", ZDateTime.Empty, invoiceLine.AL_PostDate);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", newAPInvoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, invoiceLine.AL_GE);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, invoiceLine.AL_GB);
			AssertEquals("General Ledger", ZGuid.Empty, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ZECTRA.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("Cost Tax", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("Cost WHT", ZGuid.Empty, invoiceLine.AL_AW);

			AssertEquals("OS Ex Tax Amount", 250M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 250M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 357.14M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 0M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 0M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 357.14M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -357.14M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("GST Amount", 0M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", ZGuid.Empty, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 0M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", -250.00M, invoiceLine.AL_OSAmount);
			AssertEquals("Place of Supply", PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, invoiceLine.AL_PlaceOfSupply);
			AssertEquals("Place of Supply Type", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(invoiceLine.Company, invoiceLine.AL_PlaceOfSupply), invoiceLine.AL_PlaceOfSupplyType);
		}

		#endregion

		#region TEST: OSTaxAmount and OSWHTAmount Fields Calculated Based On AH_OSExTaxAmount

		public void TestOSTaxAmountAndOSWHTAmountFieldsCalculatedBasedOnAH_OSExTaxAmount()
		{
			APInvoice invoice = Factory.New<APInvoice>();

			AALSHI.CompanyData.SetAPTaxApplicable(ZBool.True);
			AALSHI.MiscServ.OM_APWHTApplicable = ZBool.True;

			invoice.AH_OH = this.AALSHI.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();

			line.AL_RX_NKTransactionCurrency = USD.RX_Code;
			line.AL_ExchangeRate = .6M;
			line.AL_AC = MRG100.PK;

			AssertEquals("OS Ex Tax Amt", 0.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 0.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 0.00M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 0.00M, line.AL_LocalTaxAmount);

			AssertEquals("Line Amount", 0.00M, line.AL_LineAmount);
			AssertEquals("GST Amount", 0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 0.00M, line.AL_OSAmount);

			line.AL_OSExTaxAmount = 100M;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 16.67M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 8.33M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 183.34M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -16.67M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -8.33M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -110.00M, line.AL_OSAmount);

			line.AL_OSExTaxAmount = 200M;
			AssertEquals("OS Ex Tax Amt", 200.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 20.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 10.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 220.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 333.33M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 33.33M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 16.67M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 366.66M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -333.33M, line.AL_LineAmount);
			AssertEquals("GST Amount", -33.33M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -16.67M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -220.00M, line.AL_OSAmount);
		}

		#endregion

		#region TEST: LocalTaxAmount and LocalWHTAmount Fields Calculated Based On Exchange Rate

		public void TestLocalTaxAmountAndLocalWHTAmountFieldsCalculatedBasedOnExchangeRate()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();

			AALSHI.CompanyData.SetAPTaxApplicable(ZBool.True);
			AALSHI.MiscServ.OM_APWHTApplicable = ZBool.True;

			invoice.AH_OH = this.AALSHI.PK;

			line.AL_AC = MRG100.PK;
			line.AL_OSExTaxAmount = 100M;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 100.00M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 10.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 5.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 110.00M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -100.00M, line.AL_LineAmount);
			AssertEquals("GST Amount", -10.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -5.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -110.00M, line.AL_OSAmount);

			line.AL_RX_NKTransactionCurrency = USD.RX_Code;
			line.AL_ExchangeRate = .6M;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 16.67M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 8.33M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 183.34M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -16.67M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -8.33M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -110.00M, line.AL_OSAmount);

			line.AL_ExchangeRate = .5M;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 200.00M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 20.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 10.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 220.00M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -200.00M, line.AL_LineAmount);
			AssertEquals("GST Amount", -20.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -10.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -110.00M, line.AL_OSAmount);
		}

		#endregion

		#region TEST: OSTaxAmount and OSWHTAmount Fields Calculated Based On ChargeCode

		public void TestOSTaxAmountAndOSWHTAmountFieldsCalculatedBasedOnChargeCode()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();

			AALSHI.CompanyData.SetAPTaxApplicable(ZBool.True);
			AALSHI.MiscServ.OM_APWHTApplicable = ZBool.True;

			invoice.AH_OH = this.AALSHI.PK;

			line.AL_RX_NKTransactionCurrency = USD.RX_Code;
			line.AL_ExchangeRate = .6M;
			line.AL_OSExTaxAmount = 100M;

			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -100.00M, line.AL_OSAmount);

			line.AL_AC = MRG100.PK;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 16.67M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 8.33M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 183.34M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -16.67M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -8.33M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -110.00M, line.AL_OSAmount);

			line.AL_AC = MRG100_1.PK;

			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 5.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 10.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 105.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 8.33M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 16.67M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 175.00M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -8.33M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -16.67M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -105.00M, line.AL_OSAmount);
		}

		#endregion

		#region TEST: OSTaxAmount Calculated Based On TaxRate

		public void TestOSTaxAmountCalculatedBasedOnTaxRate()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();

			line.AL_RX_NKTransactionCurrency = USD.RX_Code;
			line.AL_ExchangeRate = .6M;
			line.AL_OSExTaxAmount = 100M;

			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -100.00M, line.AL_OSAmount);

			line.AL_AT = GST1.PK;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 16.67M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 183.34M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -16.67M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -110.00M, line.AL_OSAmount);

			line.AL_AT = ZGuid.Empty;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -100.00M, line.AL_OSAmount);
		}

		#endregion

		#region TEST: OSWHTAmount Calculated Based On Withholding

		public void TestOSWHTAmountCalculatedBasedOnWithholding()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();

			line.AL_RX_NKTransactionCurrency = USD.RX_Code;
			line.AL_ExchangeRate = .6M;
			line.AL_OSExTaxAmount = 100M;

			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -100.00M, line.AL_OSAmount);

			line.AL_AW = WHT1.PK;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 8.33M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", 0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -8.33M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -100.00M, line.AL_OSAmount);

			line.AL_AW = ZGuid.Empty;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -100.00M, line.AL_OSAmount);
		}

		#endregion

		#region Test: Test Branch and Department defaulted from Job When Selected

		public void TestBranchAndDepartmentCalculatedWhenSettingExistingJob()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingShipment shipment = creator.CreateShipment("S00001234");

			using (Job job = creator.CreateJob(shipment))
			{
				job.JH_GC = GlbCompany.CurrentCompany.PK;

				GlbBranch nonCurrentBranch = Factory.New<GlbBranch>();
				nonCurrentBranch.GB_GC = GlbCompany.CurrentCompany.PK;

				try
				{
					nonCurrentBranch.GB_Code = "NCB";
					job.JH_GB = nonCurrentBranch.PK;

					job.JH_GE = creator.NonCurrentDepartment.PK;
					job.JH_ParentID = shipment.PK;

					APInvoice newAPInvoice = Factory.New<APInvoice>();
					APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

					invoiceLine.AL_JH = job.PK;
					AssertNotNull(job.Branch);
					AssertEquals("Line Branch defaults to Job Branch", invoiceLine.AL_GB, job.JH_GB);
					AssertEquals("Line Department defaults to Department Branch", invoiceLine.AL_GE, job.JH_GE);
				}
				finally
				{
					GlbCompany.CurrentCompany.Branches.RemoveFromRelationship(nonCurrentBranch);
				}
			}
		}

		public void TestBranchAndDepartmentCalculatedWhenSettingNewJob()
		{
			Job freightJob = CreateJob("S00001112", AALSHI, true, 0M, ABIGAS, true, 0M);

			CommonShipment freightShipment = Factory.NewWithValidTestData<CommonShipment>();
			freightShipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			freightShipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			freightShipment.JS_TransportMode = "AIR";

			CommonShipment genericFreightShipment = Factory.NewWithValidTestData<CommonShipment>();
			genericFreightShipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			genericFreightShipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			genericFreightShipment.JS_TransportMode = "AIR";
			Factory.Save();

			freightJob.JH_GE = ZGuid.Empty;
			freightJob.PlugInData = freightShipment;

			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			invoiceLine.AL_JH = freightJob.PK;

			AssertEquals("New Job Line Branch defaults to Shipment Job Branch", invoiceLine.AL_GB, freightJob.JH_GB);
			AssertEquals("New Job Line Department defaults to Shipment Job Department", invoiceLine.AL_GE, freightJob.JH_GE);
		}

		#endregion

		#region ValidateAL_OSExTaxAmount
		public void TestValidateAL_OSExTaxAmountCount()
		{
			var newAPInvoice = Factory.NewWithValidTestData<APInvoice>();
			var invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();
			invoiceLine.ValidateAL_OSExTaxAmountCount_ForTestOnly = 0;

			//precondition
			AssertEquals(false, invoiceLine.IsValidationSuspended);
			AssertNotNull(invoiceLine.APInvoiceLineValidation_ForTestOnly);
			AssertEquals(false, invoiceLine.Factory.HasContext(BusinessContext.APInvoiceForm));

			invoiceLine.AL_AC = ZGuid.NewZGuid();
			invoiceLine.AL_GB = ZGuid.NewZGuid();
			invoiceLine.AL_GE = ZGuid.NewZGuid();
			invoiceLine.AL_JH = ZGuid.NewZGuid();
			AssertEquals(0, invoiceLine.ValidateAL_OSExTaxAmountCount_ForTestOnly);

			invoiceLine.ValidateAL_OSExTaxAmountCount_ForTestOnly = 0;
			invoiceLine.Factory.SetContext(BusinessContext.APInvoiceForm);
			AssertEquals(true, invoiceLine.Factory.HasContext(BusinessContext.APInvoiceForm));
			invoiceLine.AL_AC = ZGuid.NewZGuid();
			invoiceLine.AL_GB = ZGuid.NewZGuid();
			invoiceLine.AL_GE = ZGuid.NewZGuid();
			invoiceLine.AL_JH = ZGuid.NewZGuid();
			AssertEquals(4, invoiceLine.ValidateAL_OSExTaxAmountCount_ForTestOnly);

			invoiceLine.ValidateAL_OSExTaxAmountCount_ForTestOnly = 0;
			invoiceLine.AL_AC = invoiceLine.AL_AC;
			invoiceLine.AL_GB = invoiceLine.AL_GB;
			invoiceLine.AL_GE = invoiceLine.AL_GE;
			invoiceLine.AL_JH = invoiceLine.AL_JH;
			AssertEquals(0, invoiceLine.ValidateAL_OSExTaxAmountCount_ForTestOnly);
		}
		#endregion

		#region Implementation

		protected override Type MasterHeaderType
		{
			get { return typeof(APInvoice); }
		}

		TestObjectCreator fObjectCreator;
		protected TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}
				return fObjectCreator;
			}
		}

		Job Job;

		protected override void SetUp()
		{
			base.SetUp();
			GlbDepartment.CurrentDepartment.GE_Misc = false; // To pass Job Charge Validation
			SetupJob();
		}

		AccChargeCode fChargeCode;
		protected AccChargeCode ChargeCode
		{
			get
			{
				if (fChargeCode == null)
				{
					fChargeCode = ObjectCreator.CreateChargeCode("TST", "Description", Constants.ChargeType.Margin, 100m, null, null, "ALL");
				}
				return fChargeCode;
			}
		}

		AccChargeCode fChargeCode2;
		protected AccChargeCode ChargeCode2
		{
			get
			{
				if (fChargeCode2 == null)
				{
					fChargeCode2 = ObjectCreator.CreateChargeCode("TST2", "Test2 Desc", Constants.ChargeType.Margin, 100m, null, null, "ALL");
				}
				return fChargeCode2;
			}
		}

		protected void SetupJob()
		{
			BusinessObjectFactory jobFactory = new BusinessObjectFactory();
			TestObjectCreator jobCreator = new TestObjectCreator(jobFactory);
			Job = jobCreator.CreateJob(null, 0, null, 0);
			jobFactory.Save();
		}

		APInvoice APInvoice
		{
			get { return (APInvoice)APLine.MasterTransactionHeader; }
		}

		APInvoiceLine APLine
		{
			get { return (APInvoiceLine)base.Line; }
		}

		protected Job CreateJobWithShipment()
		{
			Job job = ObjectCreator.CreateJob(null, 0, null, 0);
			job.PlugInData = GetShipment();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return job;
		}

		protected InvoicingParam GetShipment()
		{
			InvoicingParam shipment = new InvoicingParam();
			shipment.TableName = "JobShipment";
			shipment.PK = ZGuid.NewZGuid();
			shipment.JobNumber = TestObjectCreator.GetRandomString(10);
			return shipment;
		}

		#endregion

	}
}
