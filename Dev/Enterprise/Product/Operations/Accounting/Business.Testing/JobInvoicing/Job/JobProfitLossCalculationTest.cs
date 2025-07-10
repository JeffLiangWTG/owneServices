using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobProfitLossCalculation))]
	public class JobProfitLossCalculationTest : TestCaseWithFactory
	{
		public void TestWarehousingRelatedJobValuesShown()
		{
			JobHeader jobToSave = Factory.NewJobForTesting<JobHeader>();
			jobToSave.JH_JobNum = "TESTJOB1";
			jobToSave.JH_GB = GlbBranch.CurrentBranch.PK;
			jobToSave.JH_GE = GlbDepartment.CurrentDepartment.PK;

			OrgAddress whsAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			DbCommand command = ((IDbConnected)Factory).Connection.Command(@"INSERT INTO dbo.WhsWarehouse (WW_PK, WW_OA_WarehouseAddress, WW_WarehouseCode, WW_WarehouseName, WW_GB_RelatedCompanyBranch, WW_IsVirtualWarehouse, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser) VALUES (NEWID(), @Address, 'W1', 'TEST', @Branch, 1, '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");

			command.AddParameter("@Address", SqlDbType.UniqueIdentifier, whsAddress.PK.ToGuid());
			command.AddParameter("@Branch", SqlDbType.UniqueIdentifier, GlbBranch.CurrentBranch.PK.ToGuid());
			command.ExecuteNonQuery();

			ZGuid warehousePK;
			command = ((IDbConnected)Factory).Connection.Command(@"SELECT TOP 1 WW_PK FROM dbo.WhsWarehouse");
			Assert("Warehouse exists", ZGuid.TryParse(command.ExecuteScalar(), out warehousePK));

			JobStorage storage = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Warehouse.IWhsInvoice)));
			storage.ET_WW = warehousePK;
			storage.ET_StorageFromDate = ZDateTime.Now.AddDays(-2);
			storage.ET_StorageToDate = ZDateTime.Now.AddDays(2);
			jobToSave.JH_ParentTableCode = JobStorageSchema.Constants.Prefix;
			jobToSave.JH_ParentID = storage.PK;

			BaseLineSetup(jobToSave);
			Factory.Save();

			command = ((IDbConnected)Factory).Connection.Command(@"INSERT INTO dbo.WhsDocket (WD_PK, WD_DocketID, WD_DocketType, WD_DocketStatus, WD_DocketSubType, WD_OH_Client, WD_ExternalReference, WD_WW_Whs, WD_BookingDate, WD_FinalisedDate, WD_GS_NKFinalizedBy, WD_SystemCreateTimeUtc, WD_SystemCreateUser, WD_SystemLastEditTimeUtc, WD_SystemLastEditUser) VALUES (NEWID(), 'Docket', 'ORD', 'STA', 'ORD', @Client, '123', @Warehouse, @Date, @Date, 'A', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			command.AddParameter("@Client", SqlDbType.UniqueIdentifier, storage.ET_OH_Client.ToGuid());
			command.AddParameter("@Warehouse", SqlDbType.UniqueIdentifier, storage.ET_WW.ToGuid());
			command.AddParameter("@Date", SqlDbType.DateTime, ZDateTime.Now.ToDateTime());
			command.ExecuteNonQuery();

			var loadedJob = new JobProfitLossCalculation(jobToSave);
			AssertEquals(155m, loadedJob.TotalRevenue);
			AssertEquals(-205m, loadedJob.TotalCost);
			AssertEquals(185m, loadedJob.TotalWIP);
			AssertEquals(-90m, loadedJob.TotalAccrual);
			AssertEquals(45m, loadedJob.TotalLineAmount);
		}

		public void TestTotalPLValues()
		{
			JobHeader jobToSave = Factory.NewJobForTesting<JobHeader>();
			jobToSave.JH_JobNum = "TESTJOB1";
			jobToSave.JH_GB = GlbBranch.CurrentBranch.PK;
			jobToSave.JH_GE = GlbDepartment.CurrentDepartment.PK;

			BaseLineSetup(jobToSave);

			Factory.Save();

			var loadedJob = new JobProfitLossCalculation(jobToSave);
			AssertEquals(155m, loadedJob.TotalRevenue);
			AssertEquals(-205m, loadedJob.TotalCost);
			AssertEquals(185m, loadedJob.TotalWIP);
			AssertEquals(-90m, loadedJob.TotalAccrual);
			AssertEquals(45m, loadedJob.TotalLineAmount);
		}

		public void TestTotalPLValuesWithTwoJobs()
		{
			JobHeader jobToSave = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobToSave.JH_JobNum = "TESTJOB1";

			JobHeader jobToSave2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobToSave2.JH_JobNum = "TESTJOB2";

			BaseLineSetup(jobToSave);

			BaseLineSetup(jobToSave2);

			Factory.Save();

			var loadedJob = new JobProfitLossCalculation(jobToSave);
			AssertEquals(155m, loadedJob.TotalRevenue);
			AssertEquals(-205m, loadedJob.TotalCost);
			AssertEquals(185m, loadedJob.TotalWIP);
			AssertEquals(-90m, loadedJob.TotalAccrual);
			AssertEquals(45m, loadedJob.TotalLineAmount);
		}

		public void TestTotalPLRecognizedValuesWithTwoJobs()
		{
			JobHeader jobToSave = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobToSave.JH_JobNum = "TESTJOB1";

			JobHeader jobToSave2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobToSave2.JH_JobNum = "TESTJOB2";

			BaseLineSetup(jobToSave);
			ChargeSetup(jobToSave);

			BaseLineSetup(jobToSave2);

			Factory.Save();

			var loadedJob = new JobProfitLossCalculation(jobToSave);
			AssertEquals(155m, loadedJob.TotalRevenue);
			AssertEquals(-205m, loadedJob.TotalCost);
			AssertEquals(485m, loadedJob.TotalWIP);
			AssertEquals(-390m, loadedJob.TotalAccrual);
			AssertEquals(45m, loadedJob.TotalLineAmount);
			AssertEquals(120m, loadedJob.RevRecognized);
			AssertEquals(35m, loadedJob.RevNotRecognized);
			AssertEquals(-140m, loadedJob.CstRecognized);
			AssertEquals(-65m, loadedJob.CstNotRecognized);
			AssertEquals(185m, loadedJob.WipRecognized);
			AssertEquals(300m, loadedJob.WipNotRecognized);
			AssertEquals(-90m, loadedJob.AcrRecognized);
			AssertEquals(-300m, loadedJob.AcrNotRecognized);
			AssertEquals(75m, loadedJob.ProfitLossRecognized);
			AssertEquals(-30m, loadedJob.ProfitLossNotRecognized);
		}

		public void TestTotalPLValuesWithReversedWIP()
		{
			try
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				JobHeader jobToSave = Factory.NewJobForTesting<JobHeader>();
				jobToSave.JH_JobNum = "TESTJOB1";
				jobToSave.JH_GB = GlbBranch.CurrentBranch.PK;
				jobToSave.JH_GE = GlbDepartment.CurrentDepartment.PK;

				BaseLineSetup(jobToSave);

				WIP wIP1 = Factory.New<WIP>();
				wIP1.AL_OSExTaxAmount = 135m;
				wIP1.AL_JH = jobToSave.PK;
				wIP1.AL_ReverseDate = ZDateTime.Now;
				wIP1.AL_AG = TestObjectCreator.GLHeader1.PK;

				Accrual accrual1 = Factory.New<Accrual>();
				accrual1.AL_OSExTaxAmount = 55m;
				accrual1.AL_JH = jobToSave.PK;
				accrual1.AL_ReverseDate = ZDateTime.Now;
				accrual1.AL_AG = TestObjectCreator.GLHeader1.PK;

				Factory.Save();

				var loadedJob = new JobProfitLossCalculation(jobToSave);
				AssertEquals(155m, loadedJob.TotalRevenue);
				AssertEquals(-205m, loadedJob.TotalCost);
				AssertEquals(185m, loadedJob.TotalWIP);
				AssertEquals(-90m, loadedJob.TotalAccrual);
				AssertEquals(45m, loadedJob.TotalLineAmount);
				AssertEquals(new ZDecimal(45m / 340m * 100m).Round(2), loadedJob.TotalMargin);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
		}

		public void TestEmptyMarginValues()
		{
			JobHeader jobToSave = Factory.NewJobForTesting<JobHeader>();
			jobToSave.JH_JobNum = "TESTJOB1";
			jobToSave.JH_GB = GlbBranch.CurrentBranch.PK;
			jobToSave.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			var loadedJob = new JobProfitLossCalculation(jobToSave);
			AssertEquals(0m, loadedJob.TotalRevenue);
			AssertEquals(0m, loadedJob.TotalCost);
			AssertEquals(0m, loadedJob.TotalWIP);
			AssertEquals(0m, loadedJob.TotalAccrual);
			AssertEquals(0m, loadedJob.TotalLineAmount);
		}

		public void TestMarginValues()
		{
			JobHeader jobToSave = Factory.NewJobForTesting<JobHeader>();
			jobToSave.JH_JobNum = "TESTJOB1";
			jobToSave.JH_GB = GlbBranch.CurrentBranch.PK;
			jobToSave.JH_GE = GlbDepartment.CurrentDepartment.PK;

			BaseLineSetup(jobToSave);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var loadedJob = new JobProfitLossCalculation(jobToSave);
			AssertEquals("TotalRevenue (Include DSB = YES)", 155m, loadedJob.TotalRevenue);
			AssertEquals("TotalCost (Include DSB = YES)", -205m, loadedJob.TotalCost);
			AssertEquals("TotalWIP (Include DSB = YES)", 185m, loadedJob.TotalWIP);
			AssertEquals("TotalAccrual (Include DSB = YES)", -90m, loadedJob.TotalAccrual);
			AssertEquals("TotalLineAmount (Include DSB = YES)", 45m, loadedJob.TotalLineAmount);
			AssertEquals("TotalMargin (Include DSB = YES)", 13.24m, loadedJob.TotalMargin);

			AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			loadedJob = new JobProfitLossCalculation(jobToSave);
			AssertEquals("TotalRevenue (Include DSB = NO)", 155m, loadedJob.TotalRevenue);
			AssertEquals("TotalCost (Include DSB = NO)", -205m, loadedJob.TotalCost);
			AssertEquals("TotalWIP (Include DSB = NO)", 185m, loadedJob.TotalWIP);
			AssertEquals("TotalAccrual (Include DSB = NO)", -90m, loadedJob.TotalAccrual);
			AssertEquals("TotalLineAmount (Include DSB = NO)", 45m, loadedJob.TotalLineAmount);
			AssertEquals("TotalMargin (Include DSB = NO)", 24.59m, loadedJob.TotalMargin);
		}

		public void TestJobWithColoadShipments()
		{
			var job1 = CreateJobWithColoadShipments(2);
			var job2 = CreateJobWithColoadShipments(4, "S0001");
			var loadedJob1 = new JobProfitLossCalculation(job1);
			var loadedJob2 = new JobProfitLossCalculation(job2);
			AssertEquals("TotalRevenue", 20m, loadedJob1.TotalRevenue);
			AssertEquals("TotalRevenue", 40m, loadedJob2.TotalRevenue);
			var (sqlQuery1, parameters1) = loadedJob1.GetSqlQueryParameters_ForTestOnly(job1);
			var (sqlQuery2, parameters2) = loadedJob2.GetSqlQueryParameters_ForTestOnly(job2);
			AssertEquals("Parameters are same though they have different sub shipments", parameters1.Length, parameters2.Length);

			CombineAssertions(() =>
			{
				AssertEquals(6, parameters1.Length);
				Assert(parameters1.Any(x => x.LiteralTextADO == $"AL_JH = CONVERT('{job1.PK}', 'System.Guid')" && x.ParameterName == "@JobPK"));
				Assert(parameters1.Any(x => x.LiteralTextADO == "AL_LineType = 'REV'" && x.ParameterName == "@REV"));
				Assert(parameters1.Any(x => x.LiteralTextADO == "AL_LineType = 'WIP'" && x.ParameterName == "@WIP"));
				Assert(parameters1.Any(x => x.LiteralTextADO == "AL_LineType = 'CST'" && x.ParameterName == "@CST"));
				Assert(parameters1.Any(x => x.LiteralTextADO == "AL_LineType = 'ACR'" && x.ParameterName == "@ACR"));
				Assert(parameters1.Any(x => x.LiteralTextADO == "JH_GC = CONVERT('878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 'System.Guid')" && x.ParameterName == "@CurrentCompany"));
			});

			Job CreateJobWithColoadShipments(int subShipmentCount, string jobNum = "S0000")
			{
				var shipment = TestObjectCreator.CreateShipment(jobNum);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
				var job = TestObjectCreator.CreateJob(shipment, false);
				job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.Addresses[0].PK;
				job.LocalCharges.CompanyData.OB_ARBuyersConsolInvoicingStyle = Core.Constants.ConsolInvoicingStyles.Master;
				TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10, 10);
				for (var i = 0; i < subShipmentCount; i++)
				{
					var childShipment = TestObjectCreator.CreateShipment(jobNum + i);
					var childJob = TestObjectCreator.CreateJob(childShipment, false);
					TestObjectCreator.CreateCharge(childJob, TestObjectCreator.CC1, 1, 1);
					var aRInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("123654" + childJob.JH_JobNum, GlbCompany.CurrentCompany.LocalCurrency, 1, TestObjectCreator.AALSHI);
					var rev1 = aRInvoice.Lines.AddNew();
					SetLine(childJob, rev1, TransactionLineTypes.Revenue, 10m, TestObjectCreator.CC1);
					TestObjectCreator.CreateJobCharge(rev1, childJob, rev1.ChargeCode, rev1.TransactionCurrency);
					shipment.CoLoadShipments.Add(childShipment);
				}
				AssertEquals("Precondition: Count of AdditionalJobsToShowChargesFor", subShipmentCount, ((IJobInvoicingPlugInAdditionalJobs)shipment).AdditionalJobsToShowChargesFor.Length);
				Factory.Save();
				return job;
			}
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

