using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobManagement))]
	public class JobManagementTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesJobManagement()
		{
			JobHeader jobToSave = Factory.NewJobForTesting<JobHeader>();
			jobToSave.JH_JobNum = "TESTJOB1";
			jobToSave.JH_GB = GlbBranch.CurrentBranch.PK;
			jobToSave.JH_GE = GlbDepartment.CurrentDepartment.PK;

			BaseLineSetup(jobToSave);

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobManagement loadedJob = newFactory.Load<JobManagement>(jobToSave.PK);

			var localList = new List<string> {
					nameof(loadedJob.TotalAccrual),
					nameof(loadedJob.TotalCost),
					nameof(loadedJob.TotalLineAmount),
					nameof(loadedJob.TotalRevenue),
					nameof(loadedJob.TotalWIP),
					nameof(loadedJob.TotalMargin),
					nameof(loadedJob.RevRecognized),
					nameof(loadedJob.RevNotRecognized),
					nameof(loadedJob.CstRecognized),
					nameof(loadedJob.CstNotRecognized),
					nameof(loadedJob.WipRecognized),
					nameof(loadedJob.WipNotRecognized),
					nameof(loadedJob.AcrRecognized),
					nameof(loadedJob.AcrNotRecognized),
					nameof(loadedJob.ProfitLossRecognized),
					nameof(loadedJob.ProfitLossNotRecognized)
				};

			var tester = new DecimalPlacesAttributeTester(loadedJob, loadedJob.Company);
			tester.CheckLocalCurrency(localList, nameof(loadedJob.LocalDecimals));
		}

		public void TestParentJobNumberForNonTransportJob()
		{
			var job = Factory.NewWithValidTestData<JobManagement>();

			AssertEquals("ParentJobNumber should be empty.", string.Empty, job.ParentJobNumber);
		}

		public void TestParentJobNumberForTransportJobWithoutParentJob()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var job = new JobHeader.Loader(cartage).TryLoadOrCreateWithoutMutexForTestOnly();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var jobManagement = newFactory.Load<JobManagement>(job.PK);

			AssertEquals("ParentJobNumber should be empty as Job Cartage not have a parent job.", string.Empty, jobManagement.ParentJobNumber);
		}

		public void TestParentJobNumberForTransportJobWithParentJob()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.SetParent(shipment);

			var job = new JobHeader.Loader(cartage).TryLoadOrCreateWithoutMutexForTestOnly();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var jobManagement = newFactory.Load<JobManagement>(job.PK);

			AssertEquals("ParentJobNumber should be job number of transport job's parent job", shipment.JS_UniqueConsignRef, jobManagement.ParentJobNumber);
		}

		public void TestTotalPLValuesWithTwoRelatedJobs()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			ForwardingShipment newMainShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			newMainShipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;

			ForwardingShipment newShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			newMainShipment.CoLoadShipments.Add(newShipment);

			JobHeader jobToSave = Factory.NewJobForTesting<JobHeader>();
			jobToSave.JH_JobNum = "TESTJOB1";
			jobToSave.JH_GB = GlbBranch.CurrentBranch.PK;
			jobToSave.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobToSave.JH_ParentID = newMainShipment.PK;
			jobToSave.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobToSave.JH_OA_LocalChargesAddr = objectCreator.AALSHI.Addresses[0].PK;
			AssertEquals("Pre-codition: Consol Invoicing Style should be Master", Core.Constants.ConsolInvoicingStyles.Master, jobToSave.LocalCharges.CompanyData.EffectiveBuyersConsolInvoicingStyle);

			JobHeader jobToSave2 = Factory.NewJobForTesting<JobHeader>();
			jobToSave2.JH_JobNum = "TESTJOB2";
			jobToSave2.JH_GB = GlbBranch.CurrentBranch.PK;
			jobToSave2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobToSave2.JH_ParentID = newShipment.PK;
			jobToSave2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			BaseLineSetup(jobToSave);
			ChargeSetup(jobToSave);

			BaseLineSetup(jobToSave2);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobManagement loadedJob = newFactory.Load<JobManagement>(jobToSave.PK);
			loadedJob.Parent = newMainShipment;
			AssertEquals(310m, loadedJob.TotalRevenue);
			AssertEquals(-410m, loadedJob.TotalCost);
			AssertEquals(670m, loadedJob.TotalWIP);
			AssertEquals(-480m, loadedJob.TotalAccrual);
			AssertEquals(90m, loadedJob.TotalLineAmount);
			AssertEquals(240m, loadedJob.RevRecognized);
			AssertEquals(70m, loadedJob.RevNotRecognized);
			AssertEquals(-280m, loadedJob.CstRecognized);
			AssertEquals(-130m, loadedJob.CstNotRecognized);
			AssertEquals(370m, loadedJob.WipRecognized);
			AssertEquals(300m, loadedJob.WipNotRecognized);
			AssertEquals(-180m, loadedJob.AcrRecognized);
			AssertEquals(-300m, loadedJob.AcrNotRecognized);
			AssertEquals(150m, loadedJob.ProfitLossRecognized);
			AssertEquals(-60m, loadedJob.ProfitLossNotRecognized);
		}

		public void TestRevenueRecognitionDates()
		{
			JobHeader jobToSave = Factory.NewJobForTesting<JobHeader>();
			jobToSave.JH_JobNum = "TESTJOB1";
			jobToSave.JH_GB = GlbBranch.CurrentBranch.PK;
			jobToSave.JH_GE = GlbDepartment.CurrentDepartment.PK;

			JobChargeRevRecognition revRec1 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			JobChargeRevRecognition revRec2 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			JobChargeRevRecognition revRec3 = Factory.NewWithValidTestData<JobChargeRevRecognition>();

			revRec1.D3_JH = jobToSave.PK;
			revRec2.D3_JH = jobToSave.PK;
			revRec3.D3_JH = jobToSave.PK;

			revRec1.D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			revRec2.D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob;
			revRec3.D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			revRec1.D3_RecognitionDate = ZDateTime.BrettsBirthday;
			revRec2.D3_RecognitionDate = AccountingConstants.RevenueRecognitionDateConstants.CustomsClearanceDate;
			revRec3.D3_RecognitionDate = AccountingConstants.RevenueRecognitionDateConstants.Immediate;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobManagement loadedJob = newFactory.Load<JobManagement>(jobToSave.PK);
			AssertEquals("CUS 18-Sep-71, IMM, JOB CUS", loadedJob.RevenueRecognitionDates);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Shouldn't be fired on this business object", true);
		}

		void BaseLineSetup(JobHeader jobToSave)
		{
			AccChargeRevRecOverride revRecOverride = TestObjectCreator.CC2.RevenueRecOverrides.AddNew();
			revRecOverride.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			revRecOverride.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			revRecOverride.Offset = 0;

			ARAP.Invoicing.ARInvoice aRInvoice = TestObjectCreator.CreateARInvoice<ARAP.Invoicing.ARInvoice>("123654" + jobToSave.JH_JobNum, GlbCompany.CurrentCompany.LocalCurrency, 1, TestObjectCreator.AALSHI);
			TransactionLine rev1 = aRInvoice.Lines.AddNew();
			SetLine(jobToSave, rev1, TransactionLineTypes.Revenue, 120m, TestObjectCreator.CC1);
			JobCharge rev1JobCharge = TestObjectCreator.CreateJobCharge(rev1, jobToSave, rev1.ChargeCode, rev1.TransactionCurrency);
			rev1JobCharge.JR_OSCostAmt = 0;

			TransactionLine rev2 = aRInvoice.Lines.AddNew();
			SetLine(jobToSave, rev2, TransactionLineTypes.Revenue, 35m, TestObjectCreator.CC2); //Disbursement Type
			JobCharge rev2JobCharge = TestObjectCreator.CreateJobCharge(rev2, jobToSave, rev2.ChargeCode, rev2.TransactionCurrency);
			rev2JobCharge.JR_OSCostAmt = 0;

			ARAP.Invoicing.APInvoice aPInvoice = TestObjectCreator.CreateAPInvoice<ARAP.Invoicing.APInvoice>("789456" + jobToSave.JH_JobNum, GlbCompany.CurrentCompany.LocalCurrency, 1, 0, 0, 0, 0, 0, 0);
			TransactionLine cost1 = aPInvoice.Lines.AddNew();
			SetLine(jobToSave, cost1, TransactionLineTypes.Cost, -140m, TestObjectCreator.CC1);
			JobCharge cost1JobCharge = TestObjectCreator.CreateJobCharge(cost1, jobToSave, cost1.ChargeCode, cost1.TransactionCurrency);
			cost1JobCharge.JR_OSSellAmt = 0;

			TransactionLine cost2 = aPInvoice.Lines.AddNew();
			SetLine(jobToSave, cost2, TransactionLineTypes.Cost, -65m, TestObjectCreator.CC2); //Disbursement Type
			JobCharge cost2JobCharge = TestObjectCreator.CreateJobCharge(cost2, jobToSave, cost2.ChargeCode, cost2.TransactionCurrency);
			cost2JobCharge.JR_OSSellAmt = 0;

			BaseCharge charge1 = Factory.NewWithValidTestData<BaseCharge>();
			charge1.JR_JH = jobToSave.PK;
			WIP wIP1 = Factory.New<WIP>();
			wIP1.AL_OSExTaxAmount = 135m;
			wIP1.AL_JH = jobToSave.PK;
			charge1.JR_AL_ARLine = wIP1.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			BaseCharge charge2 = Factory.NewWithValidTestData<BaseCharge>();
			charge2.JR_JH = jobToSave.PK;
			WIP wIP2 = Factory.New<WIP>();
			wIP2.AL_OSExTaxAmount = 50m;
			wIP2.AL_JH = jobToSave.PK;
			charge2.JR_AL_ARLine = wIP2.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP2);

			Accrual accrual1 = Factory.New<Accrual>();
			accrual1.AL_OSExTaxAmount = 35m;
			accrual1.AL_JH = jobToSave.PK;
			charge1.JR_AL_APLine = accrual1.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual1);

			Accrual accrual2 = Factory.New<Accrual>();
			accrual2.AL_OSExTaxAmount = 55m;
			accrual2.AL_JH = jobToSave.PK;
			charge2.JR_AL_APLine = accrual2.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual2);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		void ChargeSetup(JobHeader jobToSave)
		{
			JobCharge jobCharge1 = Factory.New<JobCharge>();
			jobCharge1.JR_AC = TestObjectCreator.CC2.PK;
			jobCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge1.JR_JH = jobToSave.PK;
			jobCharge1.JR_LocalCostAmt = 200m;
			jobCharge1.JR_OSCostAmt = 200m;
			jobCharge1.JR_AL_APLine = ZGuid.Empty;

			JobCharge jobCharge2 = Factory.New<JobCharge>();
			jobCharge2.JR_AC = TestObjectCreator.CC2.PK;
			jobCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge2.JR_JH = jobToSave.PK;
			jobCharge2.JR_LocalSellAmt = 100m;
			jobCharge2.JR_OSSellAmt = 100m;
			jobCharge2.JR_AL_ARLine = ZGuid.Empty;
		}

		void SetLine(JobHeader jobToSave, AccTransactionLines line, ZString lineType, ZDecimal amount, AccChargeCode chargeCode)
		{
			line.AL_JH = jobToSave.PK;
			line.AL_LineType = lineType;
			line.AL_LineAmount = amount;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_AC = chargeCode.PK;
			if (line.AL_ExchangeRate == 1m)
			{
				line.AL_OSAmount = line.AL_LineAmount + line.AL_GSTVAT;
			}
		}
	}
}
