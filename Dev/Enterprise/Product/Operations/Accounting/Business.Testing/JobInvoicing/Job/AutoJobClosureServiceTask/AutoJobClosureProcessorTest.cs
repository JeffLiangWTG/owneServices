using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public partial class AutoJobClosureProcessorTest : JobClosureProcessorTestHelper
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRunOperationCycle_DSB_NotProcessByChargeTypeOverrideToOther()
		{
			var companies = CreateCompany(1);
			var branches = CreateBranch(companies);

			var chargeCodeDSB = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
			chargeCodeDSB.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCodeDSB.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

			var overrideSetting = chargeCodeDSB.ChargeTypeOverrides.AddNew();
			overrideSetting.AN_ChargeType = Core.Constants.ChargeType.Margin;
			overrideSetting.AN_MarginPercentage = 100m;
			overrideSetting.AN_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			overrideSetting.AN_JobDirection = "ALL";

			Factory.Save();

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var existedDsbBatchsCount = Factory.Load<DsbJobCloseBatch>(new ZQuery()).Length;

			var jobTypes = JobInvoicingConsumerTypes.New();
			var notDsbJob_ChargeTypeOverride = new List<AccTransactionLines>();

			string loggerMSg;
			var limitedJobTypeList = AllowedJobTypeList
				.Where(code => code == JobInvoicingConsumerTypes.Shipment.Code)
				.ToArray();
			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(companies[0].PK.ToGuid(), Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 10,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 10,
				JobLevelOfSurplusUpTo = 1000
			}))
			{
				foreach (string jobTypeCode in limitedJobTypeList)
				{
					var jobType = jobTypes[jobTypeCode];

					var job = CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
					notDsbJob_ChargeTypeOverride.AddRange(
						SetTestDsbJob_Valid_PassThreshold_SurplusUpTo(job, chargeCodeDSB, 1)
					);
				}

				Factory.Save();
				var logger = new LoggerForTesting();
				var processor = new AutoJobStatusUpdateProcessor(logger);
				processor.Process(CancellationToken.None);
				loggerMSg = logger.ToString();
			}

			var newFactory = new BusinessObjectFactory();
			var reloadedJobs = newFactory.Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", limitedJobTypeList.Length * 1, reloadedJobs.Length);

			AssertEquals("No extra batch is created", existedDsbBatchsCount, newFactory.Load<DsbJobCloseBatch>(new ZQuery()).Length);

			var notDsbJob_ChargeTypeOverride_Reload = newFactory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.PK, notDsbJob_ChargeTypeOverride.Select(al => al.PK).ToArray()));
			AssertEquals(notDsbJob_ChargeTypeOverride.Count, notDsbJob_ChargeTypeOverride_Reload.Length);
			Assert("all the job are not passing the restriction", notDsbJob_ChargeTypeOverride_Reload.All(al => al.AL_JBB.IsEmpty));

			var notDsbJob_ChargeTypeOverride_Reload_Jobs = newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, notDsbJob_ChargeTypeOverride.Select(line => line.AL_JH).Distinct().ToArray()));
			foreach (var notDsbJob_ChargeTypeOverride_Reload_Job in notDsbJob_ChargeTypeOverride_Reload_Jobs)
			{
				AssertEquals("those data are not dsb job , because lines' charge type has been override to other. And they need go to normal JCS for closing process"
					, JobHeaderStatus.Closed.Code, notDsbJob_ChargeTypeOverride_Reload_Job.JH_Status);
			}
		}

		[NUnit.Framework.TestDate(2020, 10, 20, 23, 59, 00)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRunOperationCycle_DSB_NotProcessByRegistrySetting()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Enterprise.Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

			var companies = CreateCompany(4);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var existedDsbBatchsCount = Factory.Load<DsbJobCloseBatch>(new ZQuery()).Length;

			var jobTypes = JobInvoicingConsumerTypes.New();
			var invalidDsbJobLines_GC1_Offset = new List<AccTransactionLines>();

			string loggerMSg;
			var limitedJobTypeList = AllowedJobTypeList
				.Where(code => code == JobInvoicingConsumerTypes.Shipment.Code)
				.ToArray();
			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(companies[0].PK.ToGuid(), Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 10,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 10,
				JobLevelOfSurplusUpTo = 1000
			}))
			{
				foreach (string jobTypeCode in limitedJobTypeList)
				{
					var jobType = jobTypes[jobTypeCode];
					invalidDsbJobLines_GC1_Offset.AddRange(SetTestDsbJob_Valid_PassThreshold_SurplusUpTo(
						CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-9)),
						chargeCode,
						1));
				}

				Factory.Save();
				var logger = new LoggerForTesting();
				var processor = new AutoJobStatusUpdateProcessor(logger);
				processor.Process(CancellationToken.None);
				loggerMSg = logger.ToString();
			}

			var newFactory = new BusinessObjectFactory();
			var reloadedJobs = newFactory.Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", limitedJobTypeList.Length * 1, reloadedJobs.Length);

			var invalidDsbJobLines_GC1_Offset_Reload = newFactory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.PK, invalidDsbJobLines_GC1_Offset.Select(al => al.PK).ToArray()));
			AssertEquals(invalidDsbJobLines_GC1_Offset.Count, invalidDsbJobLines_GC1_Offset_Reload.Length);
			Assert("all the job are not passing the restriction", invalidDsbJobLines_GC1_Offset_Reload.All(al => al.AL_JBB.IsEmpty));

			var invalidDsbJobLines_GC1_Offset_Reload_Jobs = newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, invalidDsbJobLines_GC1_Offset.Select(al => al.AL_JH).Distinct().ToArray()));
			foreach (var invalidDsbJobLines_GC1_Offset_Reload_Job in invalidDsbJobLines_GC1_Offset_Reload_Jobs)
			{
				AssertContains($"[{companies[0].GC_Code}][{invalidDsbJobLines_GC1_Offset_Reload_Job.JH_JobNum}][DSB]: This job cannot be automatically closed. \r\nFollowing job(s) are not old enough to be automatically closed: {invalidDsbJobLines_GC1_Offset_Reload_Job.JH_JobNum}", loggerMSg);
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRunOperationCycle_DSB_NotProcessByThreshold()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Enterprise.Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

			var companies = CreateCompany(4);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var jobTypes = JobInvoicingConsumerTypes.New();
			var invalidDsbJobLines_Thresholds = new List<AccTransactionLines>();

			string loggerMSg;
			var limitedJobTypeList = AllowedJobTypeList
				.Where(code => code == JobInvoicingConsumerTypes.Shipment.Code)
				.ToArray();
			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 10,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 500,
				JobLevelOfSurplusUpTo = 500
			}))
			{
				foreach (string jobTypeCode in limitedJobTypeList)
				{
					var jobType = jobTypes[jobTypeCode];
					invalidDsbJobLines_Thresholds.AddRange(SetTestDsbJob_Valid_NotPassThreshold_ShortfallUpTo(
						CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11)),
						chargeCode,
						1));
					invalidDsbJobLines_Thresholds.AddRange(SetTestDsbJob_Valid_NotPassThreshold_SurplusUpTo(
						CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11)),
						chargeCode,
						1));
				}

				Factory.Save();

				var logger = new LoggerForTesting();
				var processor = new AutoJobStatusUpdateProcessor(logger);
				processor.Process(CancellationToken.None);
				loggerMSg = logger.ToString();
			}

			var newFactory = new BusinessObjectFactory();
			var reloadedJobs = newFactory.Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", limitedJobTypeList.Length * 2, reloadedJobs.Length);

			var dsbBatchs = newFactory.Load<DsbJobCloseBatch>(new ZQuery());

			var invalidDsbJobLines_Thresholds_Reload = newFactory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.PK, invalidDsbJobLines_Thresholds.Select(al => al.PK).ToArray()));
			AssertEquals(invalidDsbJobLines_Thresholds.Count, invalidDsbJobLines_Thresholds_Reload.Length);
			Assert("the job did not pass the threshold", invalidDsbJobLines_Thresholds_Reload.All(al => !al.AL_JBB.IsValid));

			var invalidDsbJobLines_Thresholds_Reload_Jobs = newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, invalidDsbJobLines_Thresholds_Reload.Select(al => al.AL_JH).Distinct().ToArray()));
			foreach (var invalidDsbJobLines_Thresholds_Reload_Job in invalidDsbJobLines_Thresholds_Reload_Jobs)
			{
				var totalAmount = invalidDsbJobLines_Thresholds_Reload
					.Where(line => line.AL_JH == invalidDsbJobLines_Thresholds_Reload_Job.PK)
					.Where(line => line.AL_LineType == "REV" || line.AL_LineType == "CST")
					.Sum(line => line.AL_LineAmount);
				AssertContains($"[{companies[0].GC_Code}][{invalidDsbJobLines_Thresholds_Reload_Job.JH_JobNum}][DSB]: Job Disbursement Balance {totalAmount:0.000000} is not within the threshold range", loggerMSg);
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRunOperationCycle_DSB_EmptyRegistry()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Enterprise.Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

			var companies = CreateCompany(1);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var jobTypes = JobInvoicingConsumerTypes.New();
			var invalidDsbJobLines_Thresholds = new List<AccTransactionLines>();

			string loggerMSg;
			var limitedJobTypeList = AllowedJobTypeList
				.Where(code => code == JobInvoicingConsumerTypes.Shipment.Code)
				.ToArray();
			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 10,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 0,
				JobLevelOfSurplusUpTo = 0
			}))
			{
				foreach (string jobTypeCode in limitedJobTypeList)
				{
					var jobType = jobTypes[jobTypeCode];
					invalidDsbJobLines_Thresholds.AddRange(SetTestDsbJob_Valid_NotPassThreshold_ShortfallUpTo(
						CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11)),
						chargeCode,
						1));
					invalidDsbJobLines_Thresholds.AddRange(SetTestDsbJob_Valid_NotPassThreshold_SurplusUpTo(
						CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11)),
						chargeCode,
						1));
					invalidDsbJobLines_Thresholds.AddRange(SetTestDsbJob_LinesReversed(
						CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11)),
						chargeCode));
				}

				Factory.Save();

				var logger = new LoggerForTesting();
				var processor = new AutoJobStatusUpdateProcessor(logger);
				processor.Process(CancellationToken.None);
				loggerMSg = logger.ToString();
			}

			var newFactory = new BusinessObjectFactory();
			var reloadedJobs = newFactory.Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", limitedJobTypeList.Length * 3, reloadedJobs.Length);

			var dsbBatchs = newFactory.Load<DsbJobCloseBatch>(new ZQuery());

			var validDsbJobLines_Thresholds_Reload = newFactory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.PK, invalidDsbJobLines_Thresholds.Select(al => al.PK).ToArray()));
			AssertEquals(invalidDsbJobLines_Thresholds.Count, validDsbJobLines_Thresholds_Reload.Length);
			Assert("the job pass the threshold because of no restriction",
				validDsbJobLines_Thresholds_Reload.All(al => al.AL_JBB.IsValid) && validDsbJobLines_Thresholds_Reload.AllSame(al => al.AL_JBB)
			);

			var batch_GC1 = dsbBatchs.FirstOrDefault(batch => batch.PK == validDsbJobLines_Thresholds_Reload.First().AL_JBB);
			Assert(batch_GC1 != null);
			AssertEquals("OPN", batch_GC1.JBB_BatchStatus);

			var validDsbJobLines_Thresholds_Reload_Jobs = newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, validDsbJobLines_Thresholds_Reload.Select(al => al.AL_JH).Distinct().ToArray()));
			foreach (var validDsbJobLines_Thresholds_Reload_Job in validDsbJobLines_Thresholds_Reload_Jobs)
			{
				AssertContains($"[{companies[0].GC_Code}][{validDsbJobLines_Thresholds_Reload_Job.JH_JobNum}][DSB]: Added to DSB batch {batch_GC1.JBB_BatchNumber} successfully", loggerMSg);
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRunOperationCycle_DSB()
		{
			var existedBatchButClosed = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			existedBatchButClosed.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Close;
			existedBatchButClosed.JBB_AH_Journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now).PK;

			var existedOpeningBatch = Factory.NewWithValidTestData<DsbJobCloseBatch>();

			var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Enterprise.Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

			var companies = CreateCompany(4);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			SetRegistryValue(companies[1].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 15, true, true));

			AssertEquals("JobHeader table should be empty", false, TestConnection.Exists("FROM dbo.JobHeader"));

			var jobTypes = JobInvoicingConsumerTypes.New();
			var dsbJobLinesInBatch_GC1 = new List<AccTransactionLines>();
			var dsbJobLinesInBatch_GC1_HasInOtherClosedBatch = new List<AccTransactionLines>();
			var dsbJobLinesInBatch_GC2 = new List<AccTransactionLines>();
			var dsbJobLinesNotInBatch_GC1_ClosedInOtherPlace = new List<AccTransactionLines>();
			var jobsNotDsb = new HashSet<Job>();

			string loggerMSg;
			var limitedJobTypeList = AllowedJobTypeList
				.Where(code => code == JobInvoicingConsumerTypes.Shipment.Code)
				.ToArray();
			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 10,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 10,
				JobLevelOfSurplusUpTo = 1000
			}))
			{
				foreach (string jobTypeCode in limitedJobTypeList)
				{
					var jobType = jobTypes[jobTypeCode];

					var validateAutoCloseJob_GC1_1 = CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
					dsbJobLinesInBatch_GC1.AddRange(SetTestDsbJob_Valid_PassThreshold_SurplusUpTo(validateAutoCloseJob_GC1_1, chargeCode, 1));

					var validateAutoCloseJob_GC1_2 = CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-12));
					dsbJobLinesInBatch_GC1.AddRange(SetTestDsbJob_Valid_PassThreshold_ShortfallUpTo(validateAutoCloseJob_GC1_2, chargeCode, 2));

					var validateAutoCloseJob_GC1_LinesReversed = CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
					dsbJobLinesInBatch_GC1.AddRange(SetTestDsbJob_LinesReversed(validateAutoCloseJob_GC1_LinesReversed, chargeCode));

					var validateAutoCloseJob_GC1_CLosed = CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
					var validateAutoCloseJob_GC1_CLosed_Lines = SetTestDsbJob_Valid_PassThreshold_SurplusUpTo(validateAutoCloseJob_GC1_CLosed, chargeCode, 3);
					validateAutoCloseJob_GC1_CLosed.JH_Status = JobHeaderStatus.Closed.Code;
					dsbJobLinesNotInBatch_GC1_ClosedInOtherPlace.AddRange(validateAutoCloseJob_GC1_CLosed_Lines);

					var validateAutoCloseJob_GC2 = CreateJob(jobType, companies[1].PK, branches[1].PK, Env.Time.CurrentUtcDate.AddDays(-16));
					dsbJobLinesInBatch_GC2.AddRange(SetTestDsbJob_Valid_PassThreshold_SurplusUpTo(validateAutoCloseJob_GC2, chargeCode, 4));

					var validateAutoCloseJob_GC1_HasOtherOpeningBatch = CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
					SetTestDsbJob_Invalid_HasInOtherBatch(validateAutoCloseJob_GC1_HasOtherOpeningBatch, chargeCode, existedOpeningBatch.PK, 2);
					var validateAutoCloseJob_GC1_HasOtherOpeningBatch_Lines = SetTestDsbJob_Valid_PassThreshold_SurplusUpTo(validateAutoCloseJob_GC1_HasOtherOpeningBatch, chargeCode, 6);
					dsbJobLinesInBatch_GC1_HasInOtherClosedBatch.AddRange(validateAutoCloseJob_GC1_HasOtherOpeningBatch_Lines);
					dsbJobLinesInBatch_GC1.AddRange(validateAutoCloseJob_GC1_HasOtherOpeningBatch_Lines);

					var validateAutoCloseJob_GC1_NotAnyAboutDsb = CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
					jobsNotDsb.Add(validateAutoCloseJob_GC1_NotAnyAboutDsb);

					var validateAutoCloseJob_GC1_AllTheDsbLinesAreProcessed = CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
					SetTestDsbJob_Invalid_HasInOtherBatch(validateAutoCloseJob_GC1_AllTheDsbLinesAreProcessed, chargeCode, existedBatchButClosed.PK, 3);
					jobsNotDsb.Add(validateAutoCloseJob_GC1_AllTheDsbLinesAreProcessed);
				}
				Factory.Save();

				var logger = new LoggerForTesting();
				var processor = new AutoJobStatusUpdateProcessor(logger);
				processor.Process(CancellationToken.None);
				loggerMSg = logger.ToString();
			}

			var newFactory = new BusinessObjectFactory();
			var reloadedJobs = newFactory.Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", limitedJobTypeList.Length * 8, reloadedJobs.Length);
			var closeJobCount = jobsNotDsb.Count
				+ dsbJobLinesNotInBatch_GC1_ClosedInOtherPlace.Select(line => line.AL_JH).Distinct().Count();
			AssertEquals("only non DSB Job need be closed here.(plus DSB jobs closed other place)", closeJobCount, reloadedJobs.Where(job => job.JH_Status == JobHeaderStatus.Closed.Code).Count());

			var dsbBatchs = newFactory.Load<DsbJobCloseBatch>(new ZQuery());

			var dsbJobLinesInBatch_GC1_Reload = newFactory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.PK, dsbJobLinesInBatch_GC1.Select(al => al.PK).ToArray()));
			AssertEquals(dsbJobLinesInBatch_GC1.Count, dsbJobLinesInBatch_GC1_Reload.Length);
			Assert("lines should be same batch , because of linking job", dsbJobLinesInBatch_GC1_Reload.AllSame(al => al.AL_JBB) && dsbJobLinesInBatch_GC1_Reload.All(al => al.AL_JBB.IsValid));

			var batch_GC1 = dsbBatchs.FirstOrDefault(batch => batch.PK == dsbJobLinesInBatch_GC1_Reload.First().AL_JBB);
			Assert(batch_GC1 != null);
			AssertEquals("OPN", batch_GC1.JBB_BatchStatus);

			var dsbJobLinesInBatch_GC1_Reload_Jobs = newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, dsbJobLinesInBatch_GC1.Select(al => al.AL_JH).Distinct().ToArray()));
			foreach (var dsbJobLinesInBatch_GC1_Reload_Job in dsbJobLinesInBatch_GC1_Reload_Jobs)
			{
				AssertContains($"[{companies[0].GC_Code}][{dsbJobLinesInBatch_GC1_Reload_Job.JH_JobNum}][DSB]: Added to DSB batch {batch_GC1.JBB_BatchNumber} successfully", loggerMSg);
			}

			var dsbJob_GC1_HasInOtherClosedBatch_Reload = newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK,
				dsbJobLinesInBatch_GC1_HasInOtherClosedBatch.Select(al => al.AL_JH).Distinct().ToArray())
			);
			foreach (var dsbJob_GC1_HasInOtherClosedBatch_Reload_PerJob in dsbJob_GC1_HasInOtherClosedBatch_Reload)
			{
				AssertContains("Job with DSB line related closed batch , can be processed.", $"[{companies[0].GC_Code}][{dsbJob_GC1_HasInOtherClosedBatch_Reload_PerJob.JH_JobNum}][DSB]: Added to DSB batch {batch_GC1.JBB_BatchNumber} successfully", loggerMSg);
			}

			var dsbJobLinesInBatch_GC2_Reload = newFactory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.PK, dsbJobLinesInBatch_GC2.Select(al => al.PK).ToArray()));
			AssertEquals(dsbJobLinesInBatch_GC2.Count, dsbJobLinesInBatch_GC2_Reload.Length);
			Assert("lines should be same batch , because of linking job", dsbJobLinesInBatch_GC2_Reload.AllSame(al => al.AL_JBB) && dsbJobLinesInBatch_GC2_Reload.All(al => al.AL_JBB.IsValid));
			var batch_GC2 = dsbBatchs.FirstOrDefault(batch => batch.PK == dsbJobLinesInBatch_GC2_Reload.First().AL_JBB);
			Assert(batch_GC2 != null);
			AssertEquals("OPN", batch_GC2.JBB_BatchStatus);

			var dsbJobLinesInBatch_GC2_Reload_Jobs = newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, dsbJobLinesInBatch_GC2.Select(al => al.AL_JH).Distinct().ToArray()));
			foreach (var dsbJobLinesInBatch_GC2_Reload_Job in dsbJobLinesInBatch_GC2_Reload_Jobs)
			{
				AssertContains($"[{companies[1].GC_Code}][{dsbJobLinesInBatch_GC2_Reload_Job.JH_JobNum}][DSB]: Added to DSB batch {batch_GC2.JBB_BatchNumber} successfully", loggerMSg);
			}

			AssertNotEquals("every company own its batch", dsbJobLinesInBatch_GC1_Reload.First().AL_JBB, dsbJobLinesInBatch_GC2_Reload.First().AL_JBB);

			var dsbJobLinesNotInBatch_GC1_ClosedInOtherPlace_Reload = newFactory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.PK, dsbJobLinesNotInBatch_GC1_ClosedInOtherPlace.Select(al => al.PK).ToArray()));
			AssertEquals(dsbJobLinesNotInBatch_GC1_ClosedInOtherPlace.Count, dsbJobLinesNotInBatch_GC1_ClosedInOtherPlace_Reload.Length);
			Assert("the job has closed in other place , no need be put in batch", dsbJobLinesNotInBatch_GC1_ClosedInOtherPlace_Reload.All(al => !al.AL_JBB.IsValid));
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRunOperationCycle_JCS()
		{
			var companies = CreateCompany(4);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			SetRegistryValue(companies[1].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 15, true, true));

			SetRegistryValue(companies[3].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			AssertEquals("JobHeader table should be empty", false, TestConnection.Exists("FROM dbo.JobHeader"));

			var jobTypes = JobInvoicingConsumerTypes.New();
			var allJobsToClose = new HashSet<ZGuid>();

			foreach (string jobTypeCode in AllowedJobTypeList)
			{
				var jobType = jobTypes[jobTypeCode];
				allJobsToClose.Add(CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11)).PK); //Jobs should be auto closed
				CreateJob(jobType, companies[1].PK, branches[1].PK, Env.Time.CurrentUtcDate.AddDays(-11)); //Job Should not be closed as jobs are not old enough (i.e JOP date is < 15 days old)
				CreateJob(jobType, companies[2].PK, branches[2].PK, Env.Time.CurrentUtcDate.AddDays(-25)); //Job Should not be closed for this company as there is no config
				CreateJob(jobType, companies[3].PK, branches[3].PK, Env.Time.CurrentUtcDate.AddDays(-25)); //Job Should not be closed for this company as company is inactive
			}

			Factory.Save();

			companies[3].GC_IsActive = false;
			companies[3].Factory.Save(); // No job for this deactivated company should be auto closed

			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals($"Total Number of Jobs in the JobHeader Table ({AllowedJobTypeList.Length} Job type and 4 companies)", AllowedJobTypeList.Length * companies.Length, reloadedJobs.Length);

			var closedJobs = reloadedJobs.Where(j => j.IsClosed).ToArray();
			AssertEquals($"Only {AllowedJobTypeList.Length} jobs that belong to company 1 should be closed", AllowedJobTypeList.Length, closedJobs.Length);
			AssertJobs(allJobsToClose, reloadedJobs.Where(j => j.IsClosed));
			AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(logger, reloadedJobs, allJobsToClose);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRunOperationCycle_JFC()
		{
			var companies = CreateCompany(5);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
				TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD"),
				TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 20, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			SetRegistryValue(companies[1].PK.ToGuid(),
				TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 15, true, true, configurationType: "UPD"),
				TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 15, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			SetRegistryValue(companies[3].PK.ToGuid(),
				TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD"),
				TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			SetRegistryValue(companies[4].PK.ToGuid(),
				TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 30, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			AssertEquals("JobHeader table should be empty", false, TestConnection.Exists("FROM dbo.JobHeader"));

			var jobTypes = JobInvoicingConsumerTypes.New();
			var allJobsToUpdate = new HashSet<ZGuid>();

			foreach (string jobTypeCode in AllowedJobTypeList)
			{
				var jobType = jobTypes[jobTypeCode];
				allJobsToUpdate.Add(CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11)).PK); //Jobs should be auto updated to JFC
				CreateJob(jobType, companies[1].PK, branches[1].PK, Env.Time.CurrentUtcDate.AddDays(-11)); //Job Should not be updated as jobs are not old enough (i.e JOP date is < 15 days old)
				CreateJob(jobType, companies[2].PK, branches[2].PK, Env.Time.CurrentUtcDate.AddDays(-25)); //Job Should not be updated for this company as there is no config
				CreateJob(jobType, companies[3].PK, branches[3].PK, Env.Time.CurrentUtcDate.AddDays(-25)); //Job Should not be updated for this company as company is inactive
				CreateJob(jobType, companies[4].PK, branches[4].PK, Env.Time.CurrentUtcDate.AddDays(-25)); //Job Should not be updated for configuration no matching update row
			}

			Factory.Save();

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table (39 Job type and 5 companies)", AllowedJobTypeList.Length * companies.Length, reloadedJobs.Length);

			companies[3].GC_IsActive = false;
			companies[3].Factory.Save(); // No job for this deactivated company should be auto updated and closed

			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);

			reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			var updatedJobs = reloadedJobs.Where(j => j.IsReadyForFinancialClosure).ToArray();
			AssertEquals($"Only {AllowedJobTypeList.Length} jobs that belong to company 0 should be updated", AllowedJobTypeList.Length, updatedJobs.Length);
			AssertJobs(allJobsToUpdate, reloadedJobs.Where(j => j.IsReadyForFinancialClosure));
			AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(logger, reloadedJobs, allJobsToUpdate);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRunOperationCycle_FirstJFCThenJCS()
		{
			var companies = CreateCompany(1);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
				TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD"),
				TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			AssertEquals("JobHeader table should be empty", false, TestConnection.Exists("FROM dbo.JobHeader"));

			var jobTypes = JobInvoicingConsumerTypes.New();
			var allJobsToClose = new HashSet<ZGuid>();

			foreach (string jobTypeCode in AllowedJobTypeList)
			{
				var jobType = jobTypes[jobTypeCode];
				allJobsToClose.Add(CreateJob(jobType, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11)).PK); //Jobs should be auto updated to JFC and then auto closed
			}

			Factory.Save();

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table (39 Job type and 1 companies)", AllowedJobTypeList.Length * companies.Length, reloadedJobs.Length);

			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);

			reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			var updatedJobs = reloadedJobs.Where(j => j.IsReadyForFinancialClosure).ToArray();
			var closedJobs = reloadedJobs.Where(j => j.IsClosed).ToArray();
			AssertEquals("After update to JFC auto closed, so 0", 0, updatedJobs.Length);
			AssertEquals($"Only {AllowedJobTypeList.Length} jobs that belong to company 0 should be closed", AllowedJobTypeList.Length, closedJobs.Length);
			AssertJobs(allJobsToClose, reloadedJobs.Where(j => j.IsClosed));
		}

		public void TestUserContextSwitchLog()
		{
			var companies = CreateCompany(2);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
				TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD"),
				TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			AssertEquals("JobHeader table should be empty", false, TestConnection.Exists("FROM dbo.JobHeader"));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var jobTypes = JobInvoicingConsumerTypes.New();
				var allJobsToClose = new HashSet<ZGuid>();

				var job = CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
				Factory.Save();

				var mockStatusUpdater = new Mock<IJCSSubscriber>();
				mockStatusUpdater.Setup(u => u.CanProcess(It.IsAny<Job>())).Returns(true);
				mockStatusUpdater.Setup(u => u.CanChainToNextSubscriber()).Returns(false);
				mockStatusUpdater.Setup(u => u.Process(It.IsAny<Job>(), true)).Returns(true).Callback
					(
						() =>
						{
							using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branches[1].PK.ToGuid(), Env.CurrentDepartmentPK))
							{
								job.JH_Description = "Test";
								job.Factory.Save();
							}
						}
					);

				var logger = new LoggerForTesting();
				var processor = new AutoJobStatusUpdateProcessor(logger);
				processor.SubstituteJobStatusUpdaters_ForTestOnly(new IJCSSubscriber[] { mockStatusUpdater.Object });
				processor.Process(CancellationToken.None);
			}

			AssertContainsInOrder("UserContextSwitchLog Test"
				, ExceptionReporterTestListener.Instance.GetExceptionMessage(0)
				, "OldUserContext Company: CO0, Branch: B0, User: CWServiceNewUserContext Company: CO1, Branch: B1, User: CWService");
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestRunOperationCycle_JobsWithUnpostedConsolCost()
		{
			var mockWatermarkUpdater = new Mock<IJCSWatermarkUpdater>();
			mockWatermarkUpdater.Setup(u => u.CanMoveForwardWatermark).Returns(true);

			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var consol = SetUpConsol();

			var allJobsToClose = new HashSet<ZGuid>();

			var job1 = consol.Shipments[0].Job as Job;
			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-5);
			job1.JH_GC = companies[0].PK;

			var job2 = consol.Shipments[1].Job as Job;
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
			job2.JH_GC = companies[0].PK;
			allJobsToClose.Add(job2.PK);

			var job3 = CreateJob(JobInvoicingConsumerTypes.LocalCartage, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-15));
			allJobsToClose.Add(job3.PK);

			var job4 = CreateJob(JobInvoicingConsumerTypes.FCLStorage, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-16));
			allJobsToClose.Add(job4.PK);

			Factory.Save();

			var logger1 = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger1);
			processor.Process(CancellationToken.None);

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 4, reloadedJobs.Length);
			AssertJobs(allJobsToClose, reloadedJobs.Where(j => j.IsClosed), message: "Job 2 should be closed even though it's peer job [Job 1] cannot be closed as it is not old enough.");

			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-11);
			allJobsToClose.Add(job1.PK);
			Factory.Save();

			var logger2 = new LoggerForTesting();
			processor = new AutoJobStatusUpdateProcessor(logger2);
			processor.SubstituteWatermarkUpdater_ForTestOnly(mockWatermarkUpdater.Object);
			processor.Process(CancellationToken.None);

			reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 4, reloadedJobs.Length);
			AssertJobs(allJobsToClose, reloadedJobs.Where(j => j.IsClosed), message: "Job 1 should be closed now as it is old enough.");

			AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(new[] { logger1, logger2 }, reloadedJobs, allJobsToClose);
		}

		public void TestRunOperationCycle_JobsWithUnpostedConsolCost_MultipleConsol()
		{
			var mockWatermarkUpdater = new Mock<IJCSWatermarkUpdater>();
			mockWatermarkUpdater.Setup(u => u.CanMoveForwardWatermark).Returns(true);

			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var shipment1 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			shipment1.JS_ActualWeight = 151.73m;
			shipment1.JS_ActualChargeable = 13m;

			var shipment2 = TestObjectCreator.CreateShipment("S0002", "AUSYD", "USLAX");
			shipment2.JS_ActualWeight = 251.73m;
			shipment2.JS_ActualChargeable = 23m;

			var shipment3 = TestObjectCreator.CreateShipment("S0003", "AUSYD", "USLAX");
			shipment3.JS_ActualWeight = 351.73m;
			shipment3.JS_ActualChargeable = 33m;

			SetUpConsol(shipment1, shipment2, TestObjectCreator.CC1);
			SetUpConsol(shipment2, shipment3, TestObjectCreator.CC3);

			Factory.Save();

			var expectedClosedJobs = new HashSet<ZGuid>();

			var job1 = shipment1.Job;
			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
			job1.JH_GC = companies[0].PK;
			expectedClosedJobs.Add(job1.PK);

			var job2 = shipment2.Job;
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-5);
			job2.JH_GC = companies[0].PK;

			var job3 = shipment3.Job;
			job3.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-3);
			job3.JH_GC = companies[0].PK;

			var job4 = CreateJob(JobInvoicingConsumerTypes.LocalCartage, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-15));
			expectedClosedJobs.Add(job4.PK);

			var job5 = CreateJob(JobInvoicingConsumerTypes.FCLStorage, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-16));
			expectedClosedJobs.Add(job5.PK);

			Factory.Save();

			//First Run
			var logger1 = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger1);
			processor.Process(CancellationToken.None);

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 5, reloadedJobs.Length);
			AssertJobs(expectedClosedJobs, reloadedJobs.Where(j => j.IsClosed), message: "Job 1 should be closed even though it's peer job [Job 2] cannot be closed as it is not old enough.");

			//Modify JOP date to make the job old enough to be auto closed
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-11);
			expectedClosedJobs.Add(job2.PK);

			job3.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-11);
			expectedClosedJobs.Add(job3.PK);

			Factory.Save();

			//Second run
			var logger2 = new LoggerForTesting();
			processor = new AutoJobStatusUpdateProcessor(logger2);
			processor.SubstituteWatermarkUpdater_ForTestOnly(mockWatermarkUpdater.Object);
			processor.Process(CancellationToken.None);

			reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 5, reloadedJobs.Length);
			AssertJobs(expectedClosedJobs, reloadedJobs.Where(j => j.IsClosed), message: "Job 1 should be closed even though it's peer job [Job 2] cannot be closed as it is not old enough.");

			AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(new[] { logger1, logger2 }, reloadedJobs, expectedClosedJobs);
		}

		public void TestRunOperationCycle_JobsThatRequiresProfitAndLossReasonCode()
		{
			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(companies);

			//Registry Setup

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 15, true, true));

			var plReasonCodes = new JobProfitLossReasonCodeCollection();
			var plReasonCode = plReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

			var plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 5M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.Closed.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

			//Create Jobs
			var expectedClosedJobs = new HashSet<ZGuid>();

			var job1 = CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-15));
			var testCharge = job1.Charges.AddNew();
			testCharge.JR_AC = TestObjectCreator.CC1.PK;
			testCharge.JR_LocalCostAmt = 200;
			testCharge.JR_LocalSellAmt = 211;
			job1.JH_ProfitLossReasonCode = string.Empty;

			var job2 = CreateJob(JobInvoicingConsumerTypes.MasterAWB, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-16));
			expectedClosedJobs.Add(job2.PK);

			Factory.Save();

			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 2, reloadedJobs.Length);
			AssertJobs(expectedClosedJobs, reloadedJobs.Where(j => j.IsClosed), message: "Only job 2 should be closed");
			AssertContains(job1.JH_JobNum + ": Couldn't close this job. Error:Requires a Profit-Loss Reason Code", $"[{job1.Company.GC_Code}][{job1.JH_JobNum}][JCS]: This job cannot be closed. \r\nFollowing job(s) require a Profit-Loss reason code for closing: {job1.JH_JobNum}", logger.ToString());

			AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(logger, reloadedJobs, expectedClosedJobs);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRunOperationCycle_JobIsNotQueuedWhenWatermarkCannotMoveForward()
		{
			var mockWatermarkUpdater = new Mock<IJCSWatermarkUpdater>();
			mockWatermarkUpdater.Setup(u => u.CanMoveForwardWatermark).Returns(false);

			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 5, false, false));

			SetRegistryValue(companies[1].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 7, false, false));

			CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-10));
			CreateJob(JobInvoicingConsumerTypes.Shipment, companies[1].PK, branches[1].PK, Env.Time.CurrentUtcDate.AddDays(-10));

			Factory.Save();

			var logger1 = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger1);
			processor.SubstituteWatermarkUpdater_ForTestOnly(mockWatermarkUpdater.Object);
			processor.Process(CancellationToken.None);

			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader", "Total Number of Jobs in the JobHeader Table", 2);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Total Number of Closed Jobs in the JobHeader Table", 0);
		}

		public void TestRunOperationCycle_JobsWithBothPostedConsolCostsAndUnpostedConsolCosts()
		{
			var mockWatermarkUpdater = new Mock<IJCSWatermarkUpdater>();
			mockWatermarkUpdater.Setup(u => u.CanMoveForwardWatermark).Returns(true);

			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var consol = SetUpConsol();

			var expectedClosedJobs = new HashSet<ZGuid>();

			var job1 = consol.Shipments[0].Job as Job;
			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-5);
			job1.JH_GC = companies[0].PK;

			var job2 = consol.Shipments[1].Job as Job;
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
			job2.JH_GC = companies[0].PK;
			expectedClosedJobs.Add(job2.PK);

			var job3 = CreateJob(JobInvoicingConsumerTypes.LocalCartage, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-15));
			expectedClosedJobs.Add(job3.PK);

			var job4 = CreateJob(JobInvoicingConsumerTypes.FCLStorage, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-16));
			expectedClosedJobs.Add(job4.PK);

			Factory.Save();

			//Post Consol Costs
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("10001", TestObjectCreator.AUD, 1m, 50m, 0m, 0m, 50m, 0m, 0m);
			consol.GetApportionments().CostsCollection[0].E6_AH_APInvoice = apInvoice.PK;

			var apline1 = TestObjectCreator.CreateAPInvoiceLine(apInvoice, job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "", 0m);
			job1.Charges[0].ReverseAccrual(ZDateTime.Now);
			job1.Charges[0].JR_AL_APLine = apline1.PK;
			job1.Charges[0].SetAmountsToLinkedLinesForTests();

			var apline2 = TestObjectCreator.CreateAPInvoiceLine(apInvoice, job2, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "", 0m);
			job2.Charges[0].ReverseAccrual(ZDateTime.Now);
			job2.Charges[0].JR_AL_APLine = apline2.PK;
			job2.Charges[0].SetAmountsToLinkedLinesForTests();

			var unpostedConsolCost = consol.GetApportionments().CostsCollection.TryAddNew();
			unpostedConsolCost.E6_AC_ChargeCode = TestObjectCreator.CC3.PK;
			unpostedConsolCost.E6_OSCostAmount = 100m;

			unpostedConsolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			unpostedConsolCost.ApportionmentCharges[0].JR_E6 = unpostedConsolCost.PK;
			AddJobCharge(job1, unpostedConsolCost);

			unpostedConsolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			unpostedConsolCost.ApportionmentCharges[1].JR_E6 = unpostedConsolCost.PK;
			AddJobCharge(job2, unpostedConsolCost);

			Factory.Save();

			var logger1 = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger1);
			processor.Process(CancellationToken.None);

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 4, reloadedJobs.Length);
			AssertJobs(expectedClosedJobs, reloadedJobs.Where(j => j.IsClosed), message: "Job 2 should be closed even though it's peer job [Job 1] cannot be closed as it is not old enough.");
			AssertContains("Job1 Should not be closed", $"[{job1.Company.GC_Code}][{job1.JH_JobNum}][JCS]: This job cannot be closed. \r\nFollowing job(s) are not old enough to be automatically closed: {job1.JH_JobNum}", logger1.ToString());

			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-11);
			Factory.Save();

			expectedClosedJobs.Add(job1.PK);

			var logger2 = new LoggerForTesting();
			processor = new AutoJobStatusUpdateProcessor(logger2);
			processor.SubstituteWatermarkUpdater_ForTestOnly(mockWatermarkUpdater.Object);
			processor.Process(CancellationToken.None);

			reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 4, reloadedJobs.Length);
			AssertJobs(expectedClosedJobs, reloadedJobs.Where(j => j.IsClosed), message: "Job 1 should be closed now.");

			AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(new[] { logger1, logger2 }, reloadedJobs, expectedClosedJobs);
		}

		public void TestRunOperationCycle_SingleJobWithBothPostedConsolCostsAndUnpostedConsolCosts()
		{
			var mockWatermarkUpdater = new Mock<IJCSWatermarkUpdater>();
			mockWatermarkUpdater.Setup(u => u.CanMoveForwardWatermark).Returns(true);

			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var shipment1 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			shipment1.JS_ActualWeight = 151.73m;
			shipment1.JS_ActualChargeable = 13m;
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			job1.Charges.RemoveAll();

			//Creating Consol Test
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment1);

			var listing = new ApportionmentListing(Factory, consol);

			//ConsolCost1
			var costCC1 = listing.CostsCollection.TryAddNew();
			costCC1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			costCC1.E6_OSCostAmount = 50m;

			//s1
			costCC1.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			costCC1.ApportionmentCharges[0].JR_E6 = costCC1.PK;
			AddJobCharge((shipment1.Job as Job), costCC1);

			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-15);
			job1.JH_GC = companies[0].PK;

			Factory.Save();

			//Post Consol Costs
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("10001", TestObjectCreator.AUD, 1m, 50m, 0m, 0m, 50m, 0m, 0m);
			consol.GetApportionments().CostsCollection[0].E6_AH_APInvoice = apInvoice.PK;

			var apline1 = TestObjectCreator.CreateAPInvoiceLine(apInvoice, job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "", 0m);
			job1.Charges[0].ReverseAccrual(ZDateTime.Now);
			job1.Charges[0].JR_AL_APLine = apline1.PK;
			job1.Charges[0].SetAmountsToLinkedLinesForTests();

			var unpostedConsolCost = consol.GetApportionments().CostsCollection.TryAddNew();
			unpostedConsolCost.E6_AC_ChargeCode = TestObjectCreator.CC3.PK;
			unpostedConsolCost.E6_OSCostAmount = 100m;

			unpostedConsolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			unpostedConsolCost.ApportionmentCharges[0].JR_E6 = unpostedConsolCost.PK;
			AddJobCharge(job1, unpostedConsolCost);

			Factory.Save();

			var expectedClosedJobs = new HashSet<ZGuid>();
			expectedClosedJobs.Add(job1.PK);

			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 1, reloadedJobs.Length);
			AssertJobs(expectedClosedJobs, reloadedJobs.Where(j => j.IsClosed), message: "Job 1 should be closed");

			AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(logger, reloadedJobs, expectedClosedJobs);
		}

		public void TestRunOperationCycle_OnlyMaximumNumberofJobsAreProcessedInEachRun()
		{
			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(companies);

			SetRegistryValue(Guid.Empty, TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 8, true, true));

			var allJobsToclose = new List<Job>();
			for (int i = 0; i < 60; i++)
			{
				allJobsToclose.Add(CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-10 + (-1) * i)));
			}

			Factory.Save();

			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader", "Total Number of Jobs in the JobHeader Table", 60);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobToCloseQueue", "Total Number of Jobs in the Queue", 0);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Total Number of Closed Jobs", 0);

			var maximumNumberOfJobsToProcess = 20;
			AccountingConfigurationRegistry.Instance.AutoJobClosureProcessUnboundedBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.AutoJobClosureProcessBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, maximumNumberOfJobsToProcess);

			//first run
			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);

			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Number of closed jobs", maximumNumberOfJobsToProcess);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobToCloseQueue", "Number of Jobs in the Queue.", allJobsToclose.Count);

			//second run
			processor.Process(CancellationToken.None);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Number of closed jobs", 2 * maximumNumberOfJobsToProcess);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobToCloseQueue", "Number of Jobs in the Queue. It should not change untill all jobs in the queue are processed.", allJobsToclose.Count);

			//third run
			processor.Process(CancellationToken.None);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Number of closed jobs", 3 * maximumNumberOfJobsToProcess);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobToCloseQueue", "Number of Jobs in the Queue. It should not change untill all jobs in the queue are processed.", allJobsToclose.Count);

			//fourth run
			processor.Process(CancellationToken.None);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Number of closed jobs", 3 * maximumNumberOfJobsToProcess);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobToCloseQueue", "Number of Jobs in the Queue is zero, as there are no open jobs in JobHeader table.", 0);
		}

		public void TestRunOperationCycle_AllJobsAreProcessedInEachRun()
		{
			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(companies);

			SetRegistryValue(Guid.Empty,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 8, true, true));

			var allJobsToclose = new List<Job>();
			var numberOfJobsToClose = 100;

			for (int i = 0; i < numberOfJobsToClose; i++)
			{
				allJobsToclose.Add(CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-10 + (-1) * i)));
			}

			Factory.Save();

			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader", "Total Number of Jobs in the JobHeader Table", numberOfJobsToClose);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobToCloseQueue", "Total Number of Jobs in the Queue", 0);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Total Number of Closed Jobs", 0);

			// first and only needed run
			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);

			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Number of closed jobs", numberOfJobsToClose);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobToCloseQueue", "Number of Jobs in the Queue still waiting to be processed", 0);
		}

		public void TestQueueCurrentRowPositionMarkerMovesForwardWhenThereIsABreakInTheRowNumber()
		{
			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(companies);

			SetRegistryValue(Guid.Empty, TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 8, true, true));

			var allJobsToclose = new List<Job>();
			for (int i = 0; i < 60; i++)
			{
				allJobsToclose.Add(CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-10 + (-1) * i)));
			}

			Factory.Save();

			AssertNumbeOfRows("SELECT COUNT(*) FROM JobHeader", "Total Number of Jobs in the JobHeader Table", 60);
			AssertNumbeOfRows("SELECT COUNT(*) FROM JobToCloseQueue", "Total Number of Jobs in the Queue", 0);
			AssertNumbeOfRows("SELECT COUNT(*) FROM JobHeader WHERE JH_Status = 'CLS'", "Total Number of Closed Jobs", 0);

			var maximumNumberOfJobsToProcess = 1;
			AccountingConfigurationRegistry.Instance.AutoJobClosureProcessUnboundedBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.AutoJobClosureProcessBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, maximumNumberOfJobsToProcess);

			//first run
			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);

			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Number of closed jobs", maximumNumberOfJobsToProcess);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobToCloseQueue", "Number of Jobs in the Queue.", allJobsToclose.Count);

			//Now create break in the row sequence number
			TestConnection.ExecuteNonQuery("DELETE FROM dbo.JobToCloseQueue WHERE JHC_RowNumber IN (15, 35, 48, 60)");

			AccountingConfigurationRegistry.Instance.AutoJobClosureProcessUnboundedBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			//second run
			processor.Process(CancellationToken.None);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Number of closed jobs", 56);
			AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobToCloseQueue", "Number of Jobs in the Queue is zero, as there are no open jobs in JobHeader table.", 0);
		}

		public void TestInvalidBookingParentLogError()
		{
			SetRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 15, true, true));

			var booking = Factory.New<CommonShipment>();
			booking.JS_IsForwardRegistered = false;
			booking.JS_IsCFSRegistered = false;
			booking.JS_IsShipping = false;
			booking.JS_IsBooking = true;
			booking.JS_UniqueConsignRef = "S0001";

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, Factory);
			var quote = quotedBooking.Quote as IQuote;
			quote.TH_QuoteNumber = "Q0001";

			var bookingWithQuote = (IJobInvoicingPlugIn)ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(((BusinessObject)quote).PK, booking.PK, Factory);

			Factory.Save();

			var job1 = TestObjectCreator.CreateJob(bookingWithQuote);
			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-16);
			var job2 = TestObjectCreator.CreateJob(booking);
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-16);

			Factory.Save();

			booking.JS_UniqueConsignRef = job1.JH_JobNum;
			Factory.Save();

			//first run
			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);

			AssertContains($"This Job Billing header ({booking.JS_UniqueConsignRef}) is not currently valid as the Booking has not been converted into a Shipment. Consolidate the Booking before proceeding with Job Closure.", logger.ToString());
		}

		public void TestCriticalValidationExceptionLoggedAsError()
		{
			SetRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 15, true, true));

			var shipment = TestObjectCreator.CreateShipment("S0001");
			Factory.Save();

			var job1 = TestObjectCreator.CreateJob(shipment);
			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-16);
			Factory.Save();

			//mock updater
			var mockStatusUpdater = new Mock<IJCSSubscriber>();
			mockStatusUpdater.Setup(u => u.CanProcess(It.IsAny<Job>())).Returns(true);
			mockStatusUpdater.Setup(u => u.CanChainToNextSubscriber()).Returns(false);
			mockStatusUpdater.Setup(u => u.Process(It.IsAny<Job>(), true)).Returns(false).Callback
				(
					() =>
					{
						var e = new OnSavingCriticalCheckException<Job>(job1, CriticalValidationErrorType.AbortOnSavingProcess, "Critical validation exception occurred when JCS service task tried to close the job", "Dev message");
						(e as IHasErrorReportID).ErrorReportID = "E-99999";
						throw e;
					}
				);

			//first run
			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.SubstituteJobStatusUpdaters_ForTestOnly(new IJCSSubscriber[] { mockStatusUpdater.Object });
			processor.Process(CancellationToken.None);

			AssertContains($"A critical validation exception is reported with error ID: E-99999\r\nError Details:->\r\nCritical validation exception occurred when JCS service task tried to close the job\r\n", logger.ToString());
		}

		public void TestProfitShareCreationExceptionLoggedAndReportedAsErrorWithoutStoppingJCS()
		{
			SetRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 15, true, true));

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var shipment2 = TestObjectCreator.CreateShipment("S0002");//profit share charge creation exception will be thrown for this job.
			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			Factory.Save();

			var job1 = TestObjectCreator.CreateJob(shipment1);
			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-16);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-16);
			var job3 = TestObjectCreator.CreateJob(shipment3);
			job3.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-16);
			Factory.Save();

			var errorMessage = "Cannot close the job when new profit share charges are created automatically. After you close and re-open the form, please re-enter the data, save all changes before closing the job.";

			//mock updater
			var mockStatusUpdater = new Mock<IJCSSubscriber>();
			mockStatusUpdater.Setup(u => u.Code).Returns("DUP");
			mockStatusUpdater.Setup(u => u.CanProcess(It.IsAny<Job>())).Returns(true);
			mockStatusUpdater.Setup(u => u.CanChainToNextSubscriber()).Returns(false);
			mockStatusUpdater
				.Setup(u => u.Process(It.IsAny<Job>(), true))
				.Returns((Job job, bool canSave) => (job.PK != job2.PK))
				.Callback
					(
						(Job job, bool canSave) =>
						{
							if (job.PK == job2.PK)
							{
								throw new InvalidOperationException(errorMessage);
							}
							else
							{
								job.Close(null, null);
							}
						}
					);

			//run JCS
			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.SubstituteJobStatusUpdaters_ForTestOnly(new IJCSSubscriber[] { mockStatusUpdater.Object });
			processor.Process(CancellationToken.None);

			job1.Reload();
			Assert("Job1 should be closed", job1.IsClosed);
			job2.Reload();
			Assert("Job2 should not be closed due to the exception", !job2.IsClosed);
			job3.Reload();
			Assert("Job3 should be closed", job3.IsClosed);
			AssertEquals("ProfitShareChargesCreatedByJCS", ErrorReporter.LastKeyReported);
			AssertEquals(FormattableString.Invariant($"[{job2.Company.GC_Code}][{job2.JH_JobNum}][DUP]: {errorMessage}"), ErrorReporter.LastMessageReported);
			var expectedErrorLogText = FormattableString.Invariant($"Job could not be closed due to the following error. You can try manually close the job from Job Management module.\r\nError: {errorMessage}");
			AssertContains(expectedErrorLogText, logger.ToString());
			ErrorReporter.Instance.Clear();
		}

		public void TestExceptionLoggedAsError()
		{
			SetRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 15, true, true));

			var shipment = TestObjectCreator.CreateShipment("S0001");
			Factory.Save();

			var job1 = TestObjectCreator.CreateJob(shipment);
			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-16);
			Factory.Save();

			//mock updater
			var mockStatusUpdater = new Mock<IJCSSubscriber>();
			mockStatusUpdater.Setup(u => u.CanProcess(It.IsAny<Job>())).Returns(true);
			mockStatusUpdater.Setup(u => u.CanChainToNextSubscriber()).Returns(false);
			mockStatusUpdater.Setup(u => u.Process(It.IsAny<Job>(), true)).Returns(false).Callback(() => throw new InvalidOperationException("Sequence contains no elements"));

			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.SubstituteJobStatusUpdaters_ForTestOnly(new IJCSSubscriber[] { mockStatusUpdater.Object });
			processor.Process(CancellationToken.None);
			AssertContains($"Stack trace: ", logger.ToString());
		}

		public void TestHandlingOfConcurrencyException_Successful_JCS()
		{
			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(new GlbCompany[] { companies[0] });

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var job = CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
			Factory.Save();
			AssertHandlingOfConcurrencyException_Successful("JCS", job);
		}

		public void TestHandlingOfConcurrencyException_Successful_JFC()
		{
			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(new GlbCompany[] { companies[0] });

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD"),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 20, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			var job = CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
			Factory.Save();
			AssertHandlingOfConcurrencyException_Successful("JFC", job);
		}

		void AssertHandlingOfConcurrencyException_Successful(string updaterCode, Job job)
		{
			var concurrencyExceptionCount = 0;
			var mockLogger = new Mock<IJCSLogger>();
			mockLogger
				.Setup(m => m.SetLogPrefix(It.Is<string>((s) => s.Contains(updaterCode))))
				.Callback(() =>
				{
					//To cause concurrency exception
					job.JH_UniqueJobInvoiceNumber = 102;
					Factory.Save();
				});

			mockLogger
				.Setup(m => m.LogDebug(It.Is<string>((s) => HasConcurrencyExceptionOccurred(s))))
				.Callback(() => concurrencyExceptionCount++);

			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.SubstituteLogger_ForTestOnly(mockLogger.Object);
			processor.Process(CancellationToken.None);

			AssertEquals(1, concurrencyExceptionCount);
			mockLogger.Verify(m => m.SetLogPrefix(It.IsAny<string>()), Times.Exactly(4));

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 1, reloadedJobs.Length);
			AssertJobs(new HashSet<ZGuid>() { job.PK }, reloadedJobs.Where(j => (j.IsClosed && updaterCode == "JCS") || (j.IsReadyForFinancialClosure && updaterCode == "JFC")), message: "Job should be updated");
		}

		public void TestHandlingOfConcurrencyException_NotSuccessful_JCS()
		{
			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(new GlbCompany[] { companies[0] });

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var job = CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
			Factory.Save();
			AssertHandlingOfConcurrencyException_NotSuccessful("JCS", job);
		}

		public void TestHandlingOfConcurrencyException_NotSuccessful_JFC()
		{
			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(new GlbCompany[] { companies[0] });

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD"),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 20, true, true, configurationType: "CLS", fromJobStatus: "JFC"));

			var job = CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
			Factory.Save();
			AssertHandlingOfConcurrencyException_NotSuccessful("JFC", job);
		}

		public void AssertHandlingOfConcurrencyException_NotSuccessful(string updaterCode, Job job)
		{
			ZShort tryCount = 1;
			var concurrencyExceptionCount = 0;
			var mockLogger = new Mock<IJCSLogger>();
			mockLogger
				.Setup(m => m.SetLogPrefix(It.Is<string>((s) => s.Contains(updaterCode))))
				.Callback(() =>
				{
					//To cause concurrency exception and thus fail each retry
					job.JH_UniqueJobInvoiceNumber = 100 + tryCount;
					Factory.Save();
					tryCount++;
				});

			mockLogger
				.Setup(m => m.LogDebug(It.Is<string>((s) => HasConcurrencyExceptionOccurred(s))))
				.Callback(() => concurrencyExceptionCount++);

			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.SubstituteLogger_ForTestOnly(mockLogger.Object);
			processor.Process(CancellationToken.None);

			AssertEquals(3, concurrencyExceptionCount);
			mockLogger.Verify(m => m.SetLogPrefix(It.IsAny<string>()), Times.Exactly(5));

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 1, reloadedJobs.Length);
			AssertJobs(new HashSet<ZGuid>() { }, reloadedJobs.Where(j => (j.IsClosed && updaterCode == "JCS") || (j.IsReadyForFinancialClosure && updaterCode == "JFC")), message: "Job should not be updated");
		}

		bool HasConcurrencyExceptionOccurred(string message)
		{
			return message.Contains("Concurrency exception occurred while saving.");
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestJobIsNotAutoClosedWhenNoMatchingConfigurationIsFound()
		{
			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("CLL", "ALL", "ALL", "JOP", 10, true, true));

			SetRegistryValue(companies[1].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "JOP", 15, true, true));

			var job1 = CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-10));
			var job2 = CreateJob(JobInvoicingConsumerTypes.Shipment, companies[1].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-16));

			Factory.Save();

			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 2, reloadedJobs.Length);
			AssertJobs(new HashSet<ZGuid> { job2.PK }, reloadedJobs.Where(j => j.IsClosed), message: "Job 1 should not be closed, as there is no matching configuration. Job 2 should be closed.");
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestJobIsNotAutoClosedWhenJobClosureCriteriaAreNotSatisfied()
		{
			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "JOP", 10, true, true));

			SetRegistryValue(companies[1].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "JOP", 15, true, true));

			var job1 = CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-11));
			var job2 = CreateJob(JobInvoicingConsumerTypes.Shipment, companies[1].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-12));

			Factory.Save();

			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);

			var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery());
			AssertEquals("Total Number of Jobs in the JobHeader Table", 2, reloadedJobs.Length);
			AssertJobs(new HashSet<ZGuid> { job1.PK }, reloadedJobs.Where(j => j.IsClosed), message: "Job 2 should not be closed, matched configuration expects JOP date is at least 15 days ago. Job 1 should be closed.");
			AssertContains($"Following job(s) are not old enough to be automatically closed: {job2.JH_JobNum}", logger.ToString());
		}

		public void TestPopulateQueueAndMoveWatermarkForwardAreRunAsLocked()
		{
			SetRegistryValue(Guid.Empty,
								TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "JOP", 10, true, true));

			var mockWatermarkUpdater = new Mock<IJCSWatermarkUpdater>();
			mockWatermarkUpdater.Setup(m => m.CanMoveForwardWatermark).Returns(true);
			mockWatermarkUpdater.Setup(m => m.Update());

			var mockQueuePopulator = new Mock<IJCSQueuePopulator>();
			mockQueuePopulator.Setup(m => m.Populate(It.IsAny<DateTime>(), It.IsAny<IEnumerable<ZGuid>>())).Returns(false);

			SqlApplicationLock appLock = null;
			var lockActionsWithExpectedResults = new[]
				{
					new { LockAction = new Func<IDisposable>(() => new DisposableAction(() => Db.NewExtraConnectionToMainDb().TryGetLock("JCSQueueStatus", out appLock), () => appLock?.Dispose())), ExpectedCallCount = 0, ExpectedClosedJobs = 0 },
					new { LockAction = new Func<IDisposable>(() => DisposableAction.NoAction), ExpectedCallCount = 1, ExpectedClosedJobs = 2 }
				};

			foreach (var lockActionWithExpectedResult in lockActionsWithExpectedResults)
			{
				using (lockActionWithExpectedResult.LockAction.Invoke())
				{
					var logger = new LoggerForTesting();
					var processor = new AutoJobStatusUpdateProcessor(logger);
					processor.SubstituteWatermarkUpdater_ForTestOnly(mockWatermarkUpdater.Object);
					processor.SubstituteQueuePopulator_ForTestOnly(mockQueuePopulator.Object);
					processor.Process(CancellationToken.None);

					mockWatermarkUpdater.Verify(m => m.Update(), Times.Exactly(lockActionWithExpectedResult.ExpectedCallCount));
					mockQueuePopulator.Verify(m => m.Populate(It.IsAny<DateTime>(), It.IsAny<IEnumerable<ZGuid>>()), Times.Exactly(lockActionWithExpectedResult.ExpectedCallCount));
					Assert("To Avoid Empty test failure", true);
				}
			}
		}

		public void TestNoLogIsCreatedWhenThereIsNoJCSConfiguration()
		{
			var logger = new LoggerForTesting();
			var processor = new AutoJobStatusUpdateProcessor(logger);
			processor.Process(CancellationToken.None);
			AssertEquals("Message", string.Empty, logger.ToString());
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestNotAllowedJobTypesAreNotProcessedByJCS()
		{
			var companies = CreateCompany(1);
			var branches = CreateBranch(companies);

			SetRegistryValue(companies[0].PK.ToGuid(),
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var helper = new JCSTestHelper(TestObjectCreator);
			var thJob = helper.CreateTHJob(branches[0].PK, companies[0].PK, "TH001");
			thJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-15);
			var shpJob = CreateJob(JobInvoicingConsumerTypes.Shipment, companies[0].PK, branches[0].PK, Env.Time.CurrentUtcDate.AddDays(-15));
			Factory.Save();

			var logger = new LoggerForTesting();
			IJCSSubscriber jcsSubscriber = new JobStatusUpdaterForClosingJobs(new JCSLogger(logger));
			Assert("Should not close job", !jcsSubscriber.CanProcess(thJob));
			Assert("Should close job", jcsSubscriber.CanProcess(shpJob));
		}

		public void TestListOfJobTypesThatCanBeProcessedByJCS()
		{
			var allowedJobTypeList = new List<string>();
			allowedJobTypeList.AddRange(AllowedJobTypeList);
			AssertContainsExactElementsInAnyOrder(allowedJobTypeList, JobClosureConfigurationLookups.GetAllJobTypesThatCanBeProcessedByJCS().GetAllCodes());
		}

		AccTransactionLines[] SetTestDsbJob_Valid_PassThreshold_SurplusUpTo(Job job, AccChargeCode chargeCode, int seq)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, job.JH_GB.ToGuid(), Env.CurrentDepartmentPK))
			{
				var sellAmount = AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.Value.JobLevelOfSurplusUpTo + 0;
				var costAmount = 1;

				var invoiceAR = TestObjectCreator.CreateARInvoice<ARInvoice>($"A1_{seq}_INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
				var lineAR = TestObjectCreator.CreateARInvoiceLine(invoiceAR, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", sellAmount);

				var invoiceAP = TestObjectCreator.CreateAPInvoice<APInvoice>($"A1_{seq}_INV002", TestObjectCreator.CNY, 1m, costAmount, 0m, 0m, costAmount, 0m, 0m, TestObjectCreator.AALSHI);
				var lineAP = TestObjectCreator.CreateAPInvoiceLine(invoiceAP, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", costAmount);

				var charge = TestObjectCreator.CreateCharge(job, chargeCode, lineAP, lineAR);

				return new AccTransactionLines[] {
					lineAR,
					lineAP
				};
			}
		}

		AccTransactionLines[] SetTestDsbJob_Valid_PassThreshold_ShortfallUpTo(Job job, AccChargeCode chargeCode, int seq)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, job.JH_GB.ToGuid(), Env.CurrentDepartmentPK))
			{
				var sellAmount = 1;
				var costAmount = AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.Value.JobLevelOfShortfallUpTo;

				var invoiceAR = TestObjectCreator.CreateARInvoice<ARInvoice>($"A2_{seq}_INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
				var lineAR = TestObjectCreator.CreateARInvoiceLine(invoiceAR, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", sellAmount);

				var invoiceAP = TestObjectCreator.CreateAPInvoice<APInvoice>($"A2_{seq}_INV002", TestObjectCreator.CNY, 1m, costAmount, 0m, 0m, costAmount, 0m, 0m, TestObjectCreator.AALSHI);
				var lineAP = TestObjectCreator.CreateAPInvoiceLine(invoiceAP, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", costAmount);

				var charge = TestObjectCreator.CreateCharge(job, chargeCode, lineAP, lineAR);

				return new AccTransactionLines[] {
					lineAR,
					lineAP
				};
			}
		}

		AccTransactionLines[] SetTestDsbJob_Valid_NotPassThreshold_SurplusUpTo(Job job, AccChargeCode chargeCode, int seq)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, job.JH_GB.ToGuid(), Env.CurrentDepartmentPK))
			{
				var sellAmount = AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.Value.JobLevelOfSurplusUpTo + 20;
				var costAmount = 1;

				var invoiceAR = TestObjectCreator.CreateARInvoice<ARInvoice>($"B1_{seq}_INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
				var lineAR = TestObjectCreator.CreateARInvoiceLine(invoiceAR, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", sellAmount);

				var invoiceAP = TestObjectCreator.CreateAPInvoice<APInvoice>($"B1_{seq}_INV002", TestObjectCreator.CNY, 1m, costAmount, 0m, 0m, costAmount, 0m, 0m, TestObjectCreator.AALSHI);
				var lineAP = TestObjectCreator.CreateAPInvoiceLine(invoiceAP, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", costAmount);

				var charge = TestObjectCreator.CreateCharge(job, chargeCode, lineAP, lineAR);

				return new AccTransactionLines[] {
					lineAR,
					lineAP
				};
			}
		}

		AccTransactionLines[] SetTestDsbJob_Valid_NotPassThreshold_ShortfallUpTo(Job job, AccChargeCode chargeCode, int seq)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, job.JH_GB.ToGuid(), Env.CurrentDepartmentPK))
			{
				var sellAmount = 1;
				var costAmount = AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.Value.JobLevelOfShortfallUpTo + 20;

				var invoiceAR = TestObjectCreator.CreateARInvoice<ARInvoice>($"B2_{seq}_INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
				var lineAR = TestObjectCreator.CreateARInvoiceLine(invoiceAR, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", sellAmount);

				var invoiceAP = TestObjectCreator.CreateAPInvoice<APInvoice>($"B2_{seq}_INV002", TestObjectCreator.CNY, 1m, costAmount, 0m, 0m, costAmount, 0m, 0m, TestObjectCreator.AALSHI);
				var lineAP = TestObjectCreator.CreateAPInvoiceLine(invoiceAP, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", costAmount);

				var charge = TestObjectCreator.CreateCharge(job, chargeCode, lineAP, lineAR);

				return new AccTransactionLines[] {
					lineAR,
					lineAP
				};
			}
		}

		AccTransactionLines[] SetTestDsbJob_LinesReversed(Job job, AccChargeCode chargeCode)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, job.JH_GB.ToGuid(), Env.CurrentDepartmentPK))
			{
				var sellAmount = AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.Value.JobLevelOfSurplusUpTo + 0;
				var costAmount = 1;

				var invoiceAR = TestObjectCreator.CreateARInvoice<ARInvoice>("C1_INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
				var lineAR = TestObjectCreator.CreateARInvoiceLine(invoiceAR, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", sellAmount);

				var invoiceAP = TestObjectCreator.CreateAPInvoice<APInvoice>("C1_INV002", TestObjectCreator.CNY, 1m, costAmount, 0m, 0m, costAmount, 0m, 0m, TestObjectCreator.AALSHI);
				var lineAP = TestObjectCreator.CreateAPInvoiceLine(invoiceAP, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", costAmount);

				var charge = TestObjectCreator.CreateCharge(job, chargeCode, lineAP, lineAR);

				TestObjectCreator.ReverseTransaction(invoiceAR, out var errorMsg1).TransactionNumber = "Reverse_B1_INV001";
				AssertNullOrEmpty(errorMsg1);
				TestObjectCreator.ReverseTransaction(invoiceAP, out var errorMsg2).TransactionNumber = "Reverse_B1_INV002";
				AssertNullOrEmpty(errorMsg2);
				return new AccTransactionLines[] {
					lineAR,
					lineAP
				};
			}
		}

		AccTransactionLines[] SetTestDsbJob_Invalid_HasInOtherBatch(Job job, AccChargeCode chargeCode, ZGuid guidJBB, int seq)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, job.JH_GB.ToGuid(), Env.CurrentDepartmentPK))
			{
				var sellAmount = AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.Value.JobLevelOfSurplusUpTo + 0;
				var costAmount = 1;

				var invoiceAR = TestObjectCreator.CreateARInvoice<ARInvoice>($"D1_{seq}_INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
				var lineAR = TestObjectCreator.CreateARInvoiceLine(invoiceAR, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", sellAmount);
				lineAR.AL_JBB = guidJBB;

				var invoiceAP = TestObjectCreator.CreateAPInvoice<APInvoice>($"D1_{seq}_INV002", TestObjectCreator.CNY, 1m, costAmount, 0m, 0m, costAmount, 0m, 0m, TestObjectCreator.AALSHI);
				var lineAP = TestObjectCreator.CreateAPInvoiceLine(invoiceAP, job, chargeCode, TestObjectCreator.CNY, 1M, "tEST", costAmount);
				lineAP.AL_JBB = guidJBB;

				var charge = TestObjectCreator.CreateCharge(job, chargeCode, lineAP, lineAR);

				return new AccTransactionLines[] {
					lineAR,
					lineAP
				};
			}
		}
	}
}
