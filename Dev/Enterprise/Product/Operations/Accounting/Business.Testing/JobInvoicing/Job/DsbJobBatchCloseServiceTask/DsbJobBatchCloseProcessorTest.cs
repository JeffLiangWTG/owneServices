using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class DsbJobBatchCloseProcessorTest : TestCaseWithFactory
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestProcess_AutoClose_APP()
		{
			var companies = CreateCompany(4);
			var branches = CreateBranch(companies);

			var batches = new List<DsbJobCloseBatch>();
			var jobsNeedClose = new List<Job>();

			var chargeNeedReverse = new List<Charge>();
			var consolCostsNeedClear = new List<JobConsolCost>();
			var jobsNeedKeep = new List<Job>();
			var chargeNeedKeep = new List<Charge>();
			var consolCostsNeedKeep = new List<JobConsolCost>();

			foreach (var branch in branches)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
					chargeCode.AC_ChargeGroup = Core.Constants.ChargeType.Disbursement;
					chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
					chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

					var consol1 = TestObjectCreator.CreateConsol(consolNum: $"{Env.CurrentCompany.Code}_C01");
					var batch_APP_OutThreshold_Surplus = CreateDsbJobCloseBatch(1);
					batch_APP_OutThreshold_Surplus.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;
					batch_APP_OutThreshold_Surplus.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment1_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_1", "AUSYD", "USLAX");
					var job1_1 = TestObjectCreator.CreateJob(shipment1_1, false);
					var job1_1_sellAmount = 10000m;
					var job1_1_costAmount = 200m;

					var invoiceAR1_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_sellAmount);
					lineAR1_1.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_1, job1_1);

					var invoiceAP1_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV002", TestObjectCreator.CNY, 1m, job1_1_costAmount, 0m, 0m, job1_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP1_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_costAmount);
					lineAP1_1.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAP1_1, job1_1);

					var chargeWipAcr1_1 = TestObjectCreator.CreateCharge(job1_1, TestObjectCreator.CC1, 50m, 50m);
					chargeNeedReverse.Add(chargeWipAcr1_1);

					var shipment1_2 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_2", "AUSYD", "USLAX");
					var job1_2 = TestObjectCreator.CreateJob(shipment1_2, false);
					var invoiceAR1_2 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV003", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_2 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_2, job1_2, chargeCode, TestObjectCreator.CNY, 1M, "tEST", 0);
					lineAR1_2.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_2, job1_2);

					var chargeWipAcr1_2 = TestObjectCreator.CreateCharge(job1_2, TestObjectCreator.CC1, 50m, 50m);
					chargeNeedReverse.Add(chargeWipAcr1_2);

					consol1.Shipments.Add(shipment1_1);
					consol1.Shipments.Add(shipment1_2);
					var consolCost1 = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC3, 500);
					Assert(!consolCost1.IsPosted);
					AssertEquals(2, consolCost1.ApportionmentCharges.Count);
					consolCostsNeedClear.Add(consolCost1);

					batches.Add(batch_APP_OutThreshold_Surplus);
					jobsNeedClose.Add(job1_1);
					jobsNeedClose.Add(job1_2);

					var consol2 = TestObjectCreator.CreateConsol(consolNum: $"{Env.CurrentCompany.Code}_C02");
					var batch_APP_OutThreshold_Shortfall = CreateDsbJobCloseBatch(2);
					batch_APP_OutThreshold_Shortfall.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;
					batch_APP_OutThreshold_Shortfall.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment2_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_3", "AUSYD", "USLAX");
					var job2_1 = TestObjectCreator.CreateJob(shipment2_1, false);
					var job2_1_sellAmount = 100m;
					var job2_1_costAmount = 10000m;

					var invoiceAR2_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV004", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR2_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR2_1, job2_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job2_1_sellAmount);
					lineAR2_1.AL_JBB = batch_APP_OutThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAR2_1, job2_1);

					var invoiceAP2_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV005", TestObjectCreator.CNY, 1m, job2_1_costAmount, 0m, 0m, job2_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP2_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP2_1, job2_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job2_1_costAmount);
					lineAP2_1.AL_JBB = batch_APP_OutThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAP2_1, job2_1);

					var chargeWipAcr2_1 = TestObjectCreator.CreateCharge(job2_1, TestObjectCreator.CC1, 50m, 50m);
					chargeNeedReverse.Add(chargeWipAcr2_1);

					var shipment2_2 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_4", "AUSYD", "USLAX");
					var job2_2 = TestObjectCreator.CreateJob(shipment2_2, false);
					var invoiceAR2_2 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV006", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR2_2_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR2_2, job2_2, chargeCode, TestObjectCreator.CNY, 1M, "tEST", 0);
					lineAR2_2_1.AL_JBB = batch_APP_OutThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAR2_2_1, job2_2);

					var lineAR2_2_2 = TestObjectCreator.CreateARInvoiceLine(invoiceAR2_2, job2_2, chargeCode, TestObjectCreator.CNY, 1M, "tEST", 0);
					lineAR2_2_2.AL_JBB = batch_APP_OutThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAR2_2_2, job2_2);

					var chargeWipAcr2_2 = TestObjectCreator.CreateCharge(job2_2, TestObjectCreator.CC1, 50m, 50m);
					chargeNeedReverse.Add(chargeWipAcr2_2);

					consol2.Shipments.Add(shipment2_1);
					consol2.Shipments.Add(shipment2_2);
					var consolCost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC3, 500);
					Assert(!consolCost2.IsPosted);
					AssertEquals(2, consolCost2.ApportionmentCharges.Count);
					consolCostsNeedClear.Add(consolCost2);

					batches.Add(batch_APP_OutThreshold_Shortfall);
					jobsNeedClose.Add(job2_1);
					jobsNeedClose.Add(job2_2);

					var consol3 = TestObjectCreator.CreateConsol(consolNum: $"{Env.CurrentCompany.Code}_C03");
					var batch_APP_OutThreshold_HasUnBatchedLine = CreateDsbJobCloseBatch(3);
					batch_APP_OutThreshold_HasUnBatchedLine.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;
					batch_APP_OutThreshold_HasUnBatchedLine.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment3_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_5", "AUSYD", "USLAX");
					var job3_1 = TestObjectCreator.CreateJob(shipment3_1, false);
					var job3_1_sellAmount = 100m;
					var job3_1_costAmount = 500;

					var invoiceAR3_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV007", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR3_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR3_1, job3_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job3_1_sellAmount);
					lineAR3_1.AL_JBB = batch_APP_OutThreshold_HasUnBatchedLine.PK;
					TestObjectCreator.CreateCharge(lineAR3_1, job3_1);

					var invoiceAP3_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV008", TestObjectCreator.CNY, 1m, job3_1_costAmount, 0m, 0m, job3_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP3_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP3_1, job3_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job3_1_costAmount);
					lineAP3_1.AL_JBB = ZGuid.Empty;
					TestObjectCreator.CreateCharge(lineAP3_1, job3_1);

					var chargeWipAcr3_1 = TestObjectCreator.CreateCharge(job3_1, TestObjectCreator.CC1, 50m, 50m);
					chargeNeedKeep.Add(chargeWipAcr3_1);

					var shipment3_2 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_6", "AUSYD", "USLAX");
					var job3_2 = TestObjectCreator.CreateJob(shipment3_2, false);
					var invoiceAR3_2 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV009", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR3_2 = TestObjectCreator.CreateARInvoiceLine(invoiceAR3_2, job3_2, chargeCode, TestObjectCreator.CNY, 1M, "tEST", 0);
					lineAR3_2.AL_JBB = batch_APP_OutThreshold_HasUnBatchedLine.PK;
					TestObjectCreator.CreateCharge(lineAR3_2, job3_2);

					var batchUnClosed = CreateDsbJobCloseBatch(4);
					batchUnClosed.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.RequireApproval;

					var shipment3_3 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_7", "AUSYD", "USLAX");
					var job3_3 = TestObjectCreator.CreateJob(shipment3_3, false);
					var job3_3_sellAmount = 100m;
					var job3_3_costAmount = 100m;

					var invoiceAR3_3 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV010", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR3_3 = TestObjectCreator.CreateARInvoiceLine(invoiceAR3_3, job3_3, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job3_3_sellAmount);
					lineAR3_3.AL_JBB = batch_APP_OutThreshold_HasUnBatchedLine.PK;
					TestObjectCreator.CreateCharge(lineAR3_3, job3_3);

					var invoiceAP3_3 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV011", TestObjectCreator.CNY, 1m, job3_3_costAmount, 0m, 0m, job3_3_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP3_3 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP3_3, job3_3, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job3_3_costAmount);
					lineAP3_3.AL_JBB = batchUnClosed.PK;
					TestObjectCreator.CreateCharge(lineAP3_3, job3_3);

					consol3.Shipments.Add(shipment3_1);
					var consolCost3 = TestObjectCreator.CreateConsolCost(consol3, TestObjectCreator.CC3, 500);
					Assert(!consolCost3.IsPosted);
					AssertEquals(1, consolCost3.ApportionmentCharges.Count);
					consolCostsNeedKeep.Add(consolCost3);

					batches.Add(batch_APP_OutThreshold_HasUnBatchedLine);
					jobsNeedKeep.Add(job3_1);
					jobsNeedClose.Add(job3_2);
					jobsNeedKeep.Add(job3_3);
				}
			}
			Factory.Save();

			var wipsNeedReverse = new List<AccTransactionLines>();
			var accrualsNeedReverse = new List<AccTransactionLines>();
			var wipsNeedKeep = new List<AccTransactionLines>();
			var accrualsNeedKeep = new List<AccTransactionLines>();

			chargeNeedReverse.ForEach(charge =>
			{
				wipsNeedReverse.Add(charge.WIP);
				accrualsNeedReverse.Add(charge.Accrual);
			});
			chargeNeedKeep.ForEach(charge =>
			{
				wipsNeedKeep.Add(charge.WIP);
				accrualsNeedKeep.Add(charge.Accrual);
			});

			Assert(
				"All the wips&accruals are clean"
				, wipsNeedReverse
				.Union(accrualsNeedReverse)
				.Union(wipsNeedKeep)
				.Union(accrualsNeedKeep)
				.All(line => line.AL_ReverseDate.IsEmpty)
				);

			using (AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 1000,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 0,
				JobLevelOfSurplusUpTo = 0
			}))
			{
				processor.Process(new CancellationToken());

				var newFactory = new BusinessObjectFactory();
				var batches_Reload = newFactory.Load<DsbJobCloseBatch>(new ZQuery(DsbJobCloseBatchSchema.PK, batches.Select(batch => batch.PK).ToArray()));
				AssertEquals(batches.Count, batches_Reload.Length);
				CombineAssertions("Batch Close", () =>
				{
					foreach (var batch in batches_Reload)
					{
						var company = newFactory.Load<GlbCompany>(batch.JBB_GC);
						Assert(batch.JBB_BatchStatus == AccountingConstants.DsbJobBatchStatus.Close);
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]Attempting to close approved batch.", logger.ToString());
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]DSB job close batch saved successfully.", logger.ToString());
					}
				});

				var jobsNeedClose_Reload = newFactory.Load<JobHeader>(new ZQuery(JobHeaderSchema.PK, jobsNeedClose.Select(job => job.PK).ToArray()));
				AssertEquals(jobsNeedClose.Count, jobsNeedClose_Reload.Length);
				CombineAssertions("Job Close", () =>
				{
					foreach (var job in jobsNeedClose_Reload)
					{
						Assert(job.JH_Status == JobHeaderStatus.Closed.Code);
					}
				});

				var wipsNeedReverse_Reload = newFactory.Load<WIPAccrual.WIP>(new ZQuery(AccTransactionLinesSchema.PK, wipsNeedReverse.Select(line => line.PK).ToArray()));
				AssertEquals(wipsNeedReverse.Count, wipsNeedReverse_Reload.Length);
				Assert("Closed Job ,revert wip", wipsNeedReverse_Reload.All(wip => wip.AL_ReverseDate != ZDateTime.Empty));

				var accrualsNeedReverse_Reload = newFactory.Load<WIPAccrual.Accrual>(new ZQuery(AccTransactionLinesSchema.PK, accrualsNeedReverse.Select(line => line.PK).ToArray()));
				AssertEquals(accrualsNeedReverse.Count, accrualsNeedReverse_Reload.Length);
				Assert("Closed Job ,revert acrual", accrualsNeedReverse_Reload.All(acrual => acrual.AL_ReverseDate != ZDateTime.Empty));

				AssertEquals("clean unposted consol cost",
					0,
					newFactory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.PK, consolCostsNeedClear.Select(consolCost => consolCost.PK).ToArray())).Length
				);

				var jobsNeedKeep_Reload = newFactory.Load<JobHeader>(new ZQuery(JobHeaderSchema.PK, jobsNeedKeep.Select(job => job.PK).ToArray()));
				AssertEquals(jobsNeedKeep.Count, jobsNeedKeep_Reload.Length);
				CombineAssertions("Job Keep unClosed", () =>
				{
					foreach (var job in jobsNeedKeep_Reload)
					{
						Assert(job.JH_Status != JobHeaderStatus.Closed.Code);
						Assert(System.Text.RegularExpressions.Regex.IsMatch(
							logger.ToString()
							, $@"Cannot close Job \[[^\[^\]]*{job.JH_JobNum}[^\[^\]]*\], as there are Posted Disbursement Charge linked to other disbursement open job close batch or did not link to any disbursement job close batch."
							, System.Text.RegularExpressions.RegexOptions.IgnoreCase
							));
					}
				});

				var wipsNeedKeep_Reload = newFactory.Load<WIPAccrual.WIP>(new ZQuery(AccTransactionLinesSchema.PK, wipsNeedKeep.Select(line => line.PK).ToArray()));
				AssertEquals(wipsNeedKeep.Count, wipsNeedKeep_Reload.Length);
				Assert("unClosed Job ,keep wip", wipsNeedKeep_Reload.All(wip => wip.AL_ReverseDate.IsEmpty));

				var accrualsNeedKeep_Reload = newFactory.Load<WIPAccrual.Accrual>(new ZQuery(AccTransactionLinesSchema.PK, accrualsNeedKeep.Select(line => line.PK).ToArray()));
				AssertEquals(accrualsNeedKeep.Count, accrualsNeedKeep_Reload.Length);
				Assert("unClosed Job ,keep acrual", accrualsNeedKeep_Reload.All(acrual => acrual.AL_ReverseDate.IsEmpty));

				AssertEquals("keep unposted consol cost",
					consolCostsNeedKeep.Count,
					newFactory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.PK, consolCostsNeedKeep.Select(consolCost => consolCost.PK).ToArray())).Length
				);
			}
		}

		[TestDate(2020, 6, 30)]
		public void TestProcess_AutoClose_APP_CompanyWithoutBranch()
		{
			DsbJobCloseBatch batch1 = null;
			DsbJobCloseBatch batch2 = null;
			Job job1 = null;
			Job job2 = null;
			Charge charge1 = null;
			Charge charge2 = null;
			JobConsolCost consolCost1 = null;
			JobConsolCost consolCost2 = null;

			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = TestObjectCreator.NonCurrentCompanyBranch;
			var nonCurrentCompanyCode = branch2.Company.GC_Code;

			var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				CreateBatchWithContext(1, chargeCode, out batch1, out job1, out charge1, out consolCost1);
				Factory.Save();
			}

			using (DisposableEnvironment.ForBranch(branch2.PK.ToGuid()))
			{
				CreateBatchWithContext(2, chargeCode, out batch2, out job2, out charge2, out consolCost2);
				Factory.Save();
			}

			branch1.Company.Branches.ForEach(x => x.GB_IsActive = false);
			branch1.Company.Factory.Save();

			AssertEquals("Pre-condition", true, new AccTransactionLines[] { charge1.WIP, charge1.Accrual }.All(line => line.AL_ReverseDate.IsEmpty));
			AssertEquals("2 Batches. One with a company without active branch. The other is with standard company", true, branch2.GB_IsActive);

			using (AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 1000,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 0,
				JobLevelOfSurplusUpTo = 0
			}))
			{
				processor.Process(new CancellationToken());

				var newFactory = Factory.CreateNewFactory();
				var batch1Reloaded = newFactory.Load<DsbJobCloseBatch>(batch1.PK);
				var batch2Reloaded = newFactory.Load<DsbJobCloseBatch>(batch2.PK);

				AssertEquals("Batch of which company has no active branch: status unchanged", AccountingConstants.DsbJobBatchStatus.Approve, batch1Reloaded.JBB_BatchStatus);
				AssertEquals("Batch of which company has active branch: batch closed", AccountingConstants.DsbJobBatchStatus.Close, batch2Reloaded.JBB_BatchStatus);

				AssertEquals($@"Starting DSBs Job Batch Closure process.
Cannot process batch JBB_1 because batch company EDI does not have active branch.
[{nonCurrentCompanyCode}]|[JBB_2]Starting to process DSB job close batch, with status [APP]
[{nonCurrentCompanyCode}]|[JBB_2]Attempting to close approved batch.
[{nonCurrentCompanyCode}]|[JBB_2]Create GL Journal successfully.
[{nonCurrentCompanyCode}]|[JBB_2]Attempting to save batch changes, with status changed from [APP] to [CLS].
[{nonCurrentCompanyCode}]|[JBB_2]DSB job close batch saved successfully.
DSB Job Batch Closure accomplished.", logger.ToString().Trim());

				var job1Reloaded = newFactory.Load<JobHeader>(job1.PK);
				var wip1Reloaded = newFactory.Load<WIPAccrual.WIP>(charge1.WIP.PK);
				var acr1Reloaded = newFactory.Load<WIPAccrual.Accrual>(charge1.Accrual.PK);
				var consolCost1Reloaded = newFactory.Load<JobConsolCost>(consolCost1.PK);

				AssertEquals("Job in batch 1", JobHeaderStatus.Working.Code, job1Reloaded.JH_Status);
				AssertEquals("WIP unchanged", ZDateTime.Empty, wip1Reloaded.AL_ReverseDate);
				AssertEquals("ACR unchanged", ZDateTime.Empty, acr1Reloaded.AL_ReverseDate);
				AssertNotNull("Consol cost not removed", consolCost1Reloaded);

				var job2Reloaded = newFactory.Load<JobHeader>(job2.PK);
				var wip2Reloaded = newFactory.Load<WIPAccrual.WIP>(charge2.WIP.PK);
				var acr2Reloaded = newFactory.Load<WIPAccrual.Accrual>(charge2.Accrual.PK);
				var consolCost2Reloaded = newFactory.Load<JobConsolCost>(consolCost2.PK);

				AssertEquals("Job in batch 2", JobHeaderStatus.Closed.Code, job2Reloaded.JH_Status);
				AssertNotEquals("WIP reversed", ZDateTime.Empty, wip2Reloaded.AL_ReverseDate);
				AssertNotEquals("ACR reversed", ZDateTime.Empty, acr2Reloaded.AL_ReverseDate);
				AssertNull("Unposted consol cost removed", consolCost2Reloaded);
			}
		}

		[TestDate(2020, 6, 30)]
		public void TestProcess_AutoClose_APP_ServiceTaskApproveUserAndDate()
		{
			TestObjectCreator.PrepareDsbJobCloseBatchEnvironment(
				out var job1, out var job2, out var batch1,
				out var line11, out var line12, out var line21, out var line22,
				out var chargeCode1, out var chargeCode2,
				out var charge1Cst, out var charge1Rev, out var charge2Cst, out var charge2Rev);

			batch1.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
			Factory.Save();

			processor.Process(new CancellationToken());

			var reloadedBatch1 = Factory.CreateNewFactory().Load<DsbJobCloseBatch>(batch1.PK);

			AssertContains($"[{GlbCompany.CurrentCompany.GC_Code}]|[{reloadedBatch1.JBB_BatchNumber}]DSB job close batch saved successfully.", logger.ToString());
			AssertEquals(AccountingConstants.DsbJobBatchStatus.Close, reloadedBatch1.JBB_BatchStatus);
			AssertEquals("Check user code and approve date of auto-approve batch", User.ServiceUserCode, reloadedBatch1.JBB_GS_NKApprovingUser);
			AssertEquals(ZDateTime.UtcNow, reloadedBatch1.JBB_ApprovalTimeUtc);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestProcess_AutoClose_OPN()
		{
			var companies = CreateCompany(4);
			var branches = CreateBranch(companies);
			Factory.Save();

			var batches = new List<DsbJobCloseBatch>();
			var jobsNeedClose = new List<Job>();

			var chargeNeedReverse = new List<Charge>();
			var consolCostsNeedClear = new List<JobConsolCost>();
			var jobsNeedKeep = new List<Job>();
			var chargeNeedKeep = new List<Charge>();
			var consolCostsNeedKeep = new List<JobConsolCost>();

			foreach (var branch in branches)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
					chargeCode.AC_ChargeGroup = Core.Constants.ChargeType.Disbursement;
					chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
					chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

					var consol1 = TestObjectCreator.CreateConsol(consolNum: $"{Env.CurrentCompany.Code}_C01");
					var batch_OPN_InThreshold_Surplus = CreateDsbJobCloseBatch(1);
					batch_OPN_InThreshold_Surplus.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
					batch_OPN_InThreshold_Surplus.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment1_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_1", "AUSYD", "USLAX");
					var job1_1 = TestObjectCreator.CreateJob(shipment1_1, false);
					var job1_1_sellAmount = 500m;
					var job1_1_costAmount = 200;

					var invoiceAR1_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_sellAmount);
					lineAR1_1.AL_JBB = batch_OPN_InThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_1, job1_1);

					var invoiceAP1_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV002", TestObjectCreator.CNY, 1m, job1_1_costAmount, 0m, 0m, job1_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP1_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_costAmount);
					lineAP1_1.AL_JBB = batch_OPN_InThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAP1_1, job1_1);

					var chargeWipAcr1_1 = TestObjectCreator.CreateCharge(job1_1, TestObjectCreator.CC1, 50m, 50m);
					chargeNeedReverse.Add(chargeWipAcr1_1);

					var shipment1_2 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_2", "AUSYD", "USLAX");
					var job1_2 = TestObjectCreator.CreateJob(shipment1_2, false);
					var invoiceAR1_2 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV003", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_2 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_2, job1_2, chargeCode, TestObjectCreator.CNY, 1M, "tEST", 0);
					lineAR1_2.AL_JBB = batch_OPN_InThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_2, job1_2);

					var chargeWipAcr1_2 = TestObjectCreator.CreateCharge(job1_2, TestObjectCreator.CC1, 50m, 50m);
					chargeNeedReverse.Add(chargeWipAcr1_2);

					consol1.Shipments.Add(shipment1_1);
					consol1.Shipments.Add(shipment1_2);
					var consolCost1 = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC3, 500);
					Assert(!consolCost1.IsPosted);
					AssertEquals(2, consolCost1.ApportionmentCharges.Count);
					consolCostsNeedClear.Add(consolCost1);

					batches.Add(batch_OPN_InThreshold_Surplus);
					jobsNeedClose.Add(job1_1);
					jobsNeedClose.Add(job1_2);

					var consol2 = TestObjectCreator.CreateConsol(consolNum: $"{Env.CurrentCompany.Code}_C02");
					var batch_OPN_InThreshold_Shortfall = CreateDsbJobCloseBatch(2);
					batch_OPN_InThreshold_Shortfall.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
					batch_OPN_InThreshold_Shortfall.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment2_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_3", "AUSYD", "USLAX");
					var job2_1 = TestObjectCreator.CreateJob(shipment2_1, false);
					var job2_1_sellAmount = 100m;
					var job2_1_costAmount = 500;

					var invoiceAR2_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV004", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR2_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR2_1, job2_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job2_1_sellAmount);
					lineAR2_1.AL_JBB = batch_OPN_InThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAR2_1, job2_1);

					var invoiceAP2_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV005", TestObjectCreator.CNY, 1m, job2_1_costAmount, 0m, 0m, job2_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP2_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP2_1, job2_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job2_1_costAmount);
					lineAP2_1.AL_JBB = batch_OPN_InThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAP2_1, job2_1);

					var chargeWipAcr2_1 = TestObjectCreator.CreateCharge(job2_1, TestObjectCreator.CC1, 50m, 50m);
					chargeNeedReverse.Add(chargeWipAcr2_1);

					var shipment2_2 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_4", "AUSYD", "USLAX");
					var job2_2 = TestObjectCreator.CreateJob(shipment2_2, false);
					var invoiceAR2_2 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV006", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR2_2_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR2_2, job2_2, chargeCode, TestObjectCreator.CNY, 1M, "tEST", 0);
					lineAR2_2_1.AL_JBB = batch_OPN_InThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAR2_2_1, job2_2);

					var lineAR2_2_2 = TestObjectCreator.CreateARInvoiceLine(invoiceAR2_2, job2_2, chargeCode, TestObjectCreator.CNY, 1M, "tEST", 0);
					lineAR2_2_2.AL_JBB = batch_OPN_InThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAR2_2_2, job2_2);

					var chargeWipAcr2_2 = TestObjectCreator.CreateCharge(job2_2, TestObjectCreator.CC1, 50m, 50m);
					chargeNeedReverse.Add(chargeWipAcr2_2);

					consol2.Shipments.Add(shipment2_1);
					consol2.Shipments.Add(shipment2_2);
					var consolCost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC3, 500);
					Assert(!consolCost2.IsPosted);
					AssertEquals(2, consolCost2.ApportionmentCharges.Count);
					consolCostsNeedClear.Add(consolCost2);

					batches.Add(batch_OPN_InThreshold_Shortfall);
					jobsNeedClose.Add(job2_1);
					jobsNeedClose.Add(job2_2);

					var consol3 = TestObjectCreator.CreateConsol(consolNum: $"{Env.CurrentCompany.Code}_C03");
					var batch_OPN_InThreshold_HasPeerBatch = CreateDsbJobCloseBatch(3);
					batch_OPN_InThreshold_HasPeerBatch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
					batch_OPN_InThreshold_HasPeerBatch.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment3_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_5", "AUSYD", "USLAX");
					var job3_1 = TestObjectCreator.CreateJob(shipment3_1, false);
					var job3_1_sellAmount = 100m;
					var job3_1_costAmount = 500;

					var invoiceAR3_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV007", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR3_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR3_1, job3_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job3_1_sellAmount);
					lineAR3_1.AL_JBB = batch_OPN_InThreshold_HasPeerBatch.PK;
					TestObjectCreator.CreateCharge(lineAR3_1, job3_1);

					var invoiceAP3_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV008", TestObjectCreator.CNY, 1m, job3_1_costAmount, 0m, 0m, job3_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP3_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP3_1, job3_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job3_1_costAmount);
					lineAP3_1.AL_JBB = ZGuid.Empty;
					TestObjectCreator.CreateCharge(lineAP3_1, job3_1);

					var chargeWipAcr3_1 = TestObjectCreator.CreateCharge(job3_1, TestObjectCreator.CC1, 50m, 50m);
					chargeNeedKeep.Add(chargeWipAcr3_1);

					var shipment3_2 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_6", "AUSYD", "USLAX");
					var job3_2 = TestObjectCreator.CreateJob(shipment3_2, false);
					var invoiceAR3_2 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV009", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR3_2 = TestObjectCreator.CreateARInvoiceLine(invoiceAR3_2, job3_2, chargeCode, TestObjectCreator.CNY, 1M, "tEST", 0);
					lineAR3_2.AL_JBB = batch_OPN_InThreshold_HasPeerBatch.PK;
					TestObjectCreator.CreateCharge(lineAR3_2, job3_2);

					var batchUnClosed = CreateDsbJobCloseBatch(4);
					batchUnClosed.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.RequireApproval;

					var shipment3_3 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_7", "AUSYD", "USLAX");
					var job3_3 = TestObjectCreator.CreateJob(shipment3_3, false);
					var job3_3_sellAmount = 100m;
					var job3_3_costAmount = 100m;

					var invoiceAR3_3 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV010", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR3_3 = TestObjectCreator.CreateARInvoiceLine(invoiceAR3_3, job3_3, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job3_3_sellAmount);
					lineAR3_3.AL_JBB = batch_OPN_InThreshold_HasPeerBatch.PK;
					TestObjectCreator.CreateCharge(lineAR3_3, job3_3);

					var invoiceAP3_3 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV011", TestObjectCreator.CNY, 1m, job3_3_costAmount, 0m, 0m, job3_3_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP3_3 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP3_3, job3_3, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job3_3_costAmount);
					lineAP3_3.AL_JBB = batchUnClosed.PK;
					TestObjectCreator.CreateCharge(lineAP3_3, job3_3);

					consol3.Shipments.Add(shipment3_1);
					var consolCost3 = TestObjectCreator.CreateConsolCost(consol3, TestObjectCreator.CC3, 500);
					Assert(!consolCost3.IsPosted);
					AssertEquals(1, consolCost3.ApportionmentCharges.Count);
					consolCostsNeedKeep.Add(consolCost3);

					batches.Add(batch_OPN_InThreshold_HasPeerBatch);
					jobsNeedKeep.Add(job3_1);
					jobsNeedClose.Add(job3_2);
					jobsNeedKeep.Add(job3_3);
				}
			}
			Factory.Save();

			var wipsNeedReverse = new List<AccTransactionLines>();
			var accrualsNeedReverse = new List<AccTransactionLines>();
			var wipsNeedKeep = new List<AccTransactionLines>();
			var accrualsNeedKeep = new List<AccTransactionLines>();

			chargeNeedReverse.ForEach(charge =>
			{
				wipsNeedReverse.Add(charge.WIP);
				accrualsNeedReverse.Add(charge.Accrual);
			});
			chargeNeedKeep.ForEach(charge =>
			{
				wipsNeedKeep.Add(charge.WIP);
				accrualsNeedKeep.Add(charge.Accrual);
			});

			Assert(
				"All the wips&accruals are clean"
				, wipsNeedReverse
				.Union(accrualsNeedReverse)
				.Union(wipsNeedKeep)
				.Union(accrualsNeedKeep)
				.All(line => line.AL_ReverseDate.IsEmpty)
				);

			using (AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 1000,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 0,
				JobLevelOfSurplusUpTo = 0
			}))
			{
				processor.Process(new CancellationToken());

				var newFactory = new BusinessObjectFactory();
				var batches_Reload = newFactory.Load<DsbJobCloseBatch>(new ZQuery(DsbJobCloseBatchSchema.PK, batches.Select(batch => batch.PK).ToArray()));
				AssertEquals(batches.Count, batches_Reload.Length);
				CombineAssertions("Batch Close", () =>
				{
					foreach (var batch in batches_Reload)
					{
						var company = newFactory.Load<GlbCompany>(batch.JBB_GC);
						Assert(batch.JBB_BatchStatus == AccountingConstants.DsbJobBatchStatus.Close);
						var logMsg = logger.ToString();
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]DSB job close batch is automatically approved as batch total ", logMsg);
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]Attempting to close approved batch.", logMsg);
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]DSB job close batch saved successfully.", logMsg);
					}
				});

				var jobsNeedClose_Reload = newFactory.Load<JobHeader>(new ZQuery(JobHeaderSchema.PK, jobsNeedClose.Select(job => job.PK).ToArray()));
				AssertEquals(jobsNeedClose.Count, jobsNeedClose_Reload.Length);
				CombineAssertions("Job Close", () =>
				{
					foreach (var job in jobsNeedClose_Reload)
					{
						Assert(job.JH_Status == JobHeaderStatus.Closed.Code);
					}
				});

				var wipsNeedReverse_Reload = newFactory.Load<WIPAccrual.WIP>(new ZQuery(AccTransactionLinesSchema.PK, wipsNeedReverse.Select(line => line.PK).ToArray()));
				AssertEquals(wipsNeedReverse.Count, wipsNeedReverse_Reload.Length);
				Assert("Closed Job ,revert wip", wipsNeedReverse_Reload.All(wip => wip.AL_ReverseDate != ZDateTime.Empty));

				var accrualsNeedReverse_Reload = newFactory.Load<WIPAccrual.Accrual>(new ZQuery(AccTransactionLinesSchema.PK, accrualsNeedReverse.Select(line => line.PK).ToArray()));
				AssertEquals(accrualsNeedReverse.Count, accrualsNeedReverse_Reload.Length);
				Assert("Closed Job ,revert acrual", accrualsNeedReverse_Reload.All(acrual => acrual.AL_ReverseDate != ZDateTime.Empty));

				AssertEquals("clean unposted consol cost",
					0,
					newFactory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.PK, consolCostsNeedClear.Select(consolCost => consolCost.PK).ToArray())).Length
				);

				var jobsNeedKeep_Reload = newFactory.Load<JobHeader>(new ZQuery(JobHeaderSchema.PK, jobsNeedKeep.Select(job => job.PK).ToArray()));
				AssertEquals(jobsNeedKeep.Count, jobsNeedKeep_Reload.Length);
				CombineAssertions("Job Keep unClosed", () =>
				{
					foreach (var job in jobsNeedKeep_Reload)
					{
						Assert(job.JH_Status != JobHeaderStatus.Closed.Code);
						Assert(System.Text.RegularExpressions.Regex.IsMatch(
							logger.ToString()
							, $@"Cannot close Job \[[^\[^\]]*{job.JH_JobNum}[^\[^\]]*\], as there are Posted Disbursement Charge linked to other disbursement open job close batch or did not link to any disbursement job close batch."
							, System.Text.RegularExpressions.RegexOptions.IgnoreCase
							));
					}
				});

				var wipsNeedKeep_Reload = newFactory.Load<WIPAccrual.WIP>(new ZQuery(AccTransactionLinesSchema.PK, wipsNeedKeep.Select(line => line.PK).ToArray()));
				AssertEquals(wipsNeedKeep.Count, wipsNeedKeep_Reload.Length);
				Assert("unClosed Job ,keep wip", wipsNeedKeep_Reload.All(wip => wip.AL_ReverseDate.IsEmpty));

				var accrualsNeedKeep_Reload = newFactory.Load<WIPAccrual.Accrual>(new ZQuery(AccTransactionLinesSchema.PK, accrualsNeedKeep.Select(line => line.PK).ToArray()));
				AssertEquals(accrualsNeedKeep.Count, accrualsNeedKeep_Reload.Length);
				Assert("unClosed Job ,keep acrual", accrualsNeedKeep_Reload.All(acrual => acrual.AL_ReverseDate.IsEmpty));

				AssertEquals("keep unposted consol cost",
					consolCostsNeedKeep.Count,
					newFactory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.PK, consolCostsNeedKeep.Select(consolCost => consolCost.PK).ToArray())).Length
				);
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestProcess_SetREQ()
		{
			var companies = CreateCompany(4);
			var branches = CreateBranch(companies);
			Factory.Save();

			var batches = new List<DsbJobCloseBatch>();
			foreach (var branch in branches)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
					chargeCode.AC_ChargeGroup = Core.Constants.ChargeType.Disbursement;
					chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
					chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

					var batch_OPN_OutThreshold_Surplus = CreateDsbJobCloseBatch(1);
					batch_OPN_OutThreshold_Surplus.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
					batch_OPN_OutThreshold_Surplus.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment1_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_1", "AUSYD", "USLAX");
					var job1_1 = TestObjectCreator.CreateJob(shipment1_1, false);
					var job1_1_sellAmount = 10000m;
					var job1_1_costAmount = 200;

					var invoiceAR1_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_sellAmount);
					lineAR1_1.AL_JBB = batch_OPN_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_1, job1_1);

					var invoiceAP1_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV002", TestObjectCreator.CNY, 1m, job1_1_costAmount, 0m, 0m, job1_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP1_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_costAmount);
					lineAP1_1.AL_JBB = batch_OPN_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAP1_1, job1_1);

					batches.Add(batch_OPN_OutThreshold_Surplus);

					var batch_OPN_OutThreshold_Shortfall = CreateDsbJobCloseBatch(2);
					batch_OPN_OutThreshold_Shortfall.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
					batch_OPN_OutThreshold_Shortfall.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment2_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_3", "AUSYD", "USLAX");
					var job2_1 = TestObjectCreator.CreateJob(shipment2_1, false);
					var job2_1_sellAmount = 200m;
					var job2_1_costAmount = 10000m;

					var invoiceAR2_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV004", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR2_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR2_1, job2_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job2_1_sellAmount);
					lineAR2_1.AL_JBB = batch_OPN_OutThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAR2_1, job2_1);

					var invoiceAP2_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV005", TestObjectCreator.CNY, 1m, job2_1_costAmount, 0m, 0m, job2_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP2_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP2_1, job2_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job2_1_costAmount);
					lineAP2_1.AL_JBB = batch_OPN_OutThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAP2_1, job2_1);

					batches.Add(batch_OPN_OutThreshold_Shortfall);
				}
			}
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 1000,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 0,
				JobLevelOfSurplusUpTo = 0
			}))
			{
				processor.Process(new CancellationToken());

				var newFactory = new BusinessObjectFactory();
				var batches_Reload = newFactory.Load<DsbJobCloseBatch>(new ZQuery(DsbJobCloseBatchSchema.PK, batches.Select(batch => batch.PK).ToArray()));
				AssertEquals(batches.Count, batches_Reload.Length);

				CombineAssertions("Batch set as REQ", () =>
				{
					foreach (var batch in batches_Reload)
					{
						var company = newFactory.Load<GlbCompany>(batch.JBB_GC);
						Assert(batch.JBB_BatchStatus == AccountingConstants.DsbJobBatchStatus.RequireApproval);
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]DSB job close batch approval request is created", logger.ToString());
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]DSB job close batch saved successfully.", logger.ToString());
					}
				});
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestProcess_EmptyThreshold()
		{
			var companies = CreateCompany(4);
			var branches = CreateBranch(companies);
			Factory.Save();

			var batches = new List<DsbJobCloseBatch>();
			foreach (var branch in branches)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
					chargeCode.AC_ChargeGroup = Core.Constants.ChargeType.Disbursement;
					chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
					chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

					var batch_OPN_OutThreshold_Surplus = CreateDsbJobCloseBatch(1);
					batch_OPN_OutThreshold_Surplus.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
					batch_OPN_OutThreshold_Surplus.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment1_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_1", "AUSYD", "USLAX");
					var job1_1 = TestObjectCreator.CreateJob(shipment1_1, false);
					var job1_1_sellAmount = 10000m;
					var job1_1_costAmount = 200;

					var invoiceAR1_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_sellAmount);
					lineAR1_1.AL_JBB = batch_OPN_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_1, job1_1);

					var invoiceAP1_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV002", TestObjectCreator.CNY, 1m, job1_1_costAmount, 0m, 0m, job1_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP1_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_costAmount);
					lineAP1_1.AL_JBB = batch_OPN_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAP1_1, job1_1);

					batches.Add(batch_OPN_OutThreshold_Surplus);

					var batch_OPN_OutThreshold_Shortfall = CreateDsbJobCloseBatch(2);
					batch_OPN_OutThreshold_Shortfall.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
					batch_OPN_OutThreshold_Shortfall.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment2_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_3", "AUSYD", "USLAX");
					var job2_1 = TestObjectCreator.CreateJob(shipment2_1, false);
					var job2_1_sellAmount = 200m;
					var job2_1_costAmount = 10000m;

					var invoiceAR2_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV004", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR2_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR2_1, job2_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job2_1_sellAmount);
					lineAR2_1.AL_JBB = batch_OPN_OutThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAR2_1, job2_1);

					var invoiceAP2_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV005", TestObjectCreator.CNY, 1m, job2_1_costAmount, 0m, 0m, job2_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP2_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP2_1, job2_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job2_1_costAmount);
					lineAP2_1.AL_JBB = batch_OPN_OutThreshold_Shortfall.PK;
					TestObjectCreator.CreateCharge(lineAP2_1, job2_1);

					batches.Add(batch_OPN_OutThreshold_Shortfall);
				}
			}
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 0,
				AggregatedLevelOfSurplusUpTo = 0,
				JobLevelOfShortfallUpTo = 0,
				JobLevelOfSurplusUpTo = 0
			}))
			{
				processor.Process(new CancellationToken());

				var newFactory = new BusinessObjectFactory();
				var batches_Reload = newFactory.Load<DsbJobCloseBatch>(new ZQuery(DsbJobCloseBatchSchema.PK, batches.Select(batch => batch.PK).ToArray()));
				AssertEquals(batches.Count, batches_Reload.Length);

				CombineAssertions("Batches are auto approved because of empty threshold", () =>
				{
					foreach (var batch in batches_Reload)
					{
						var company = newFactory.Load<GlbCompany>(batch.JBB_GC);
						Assert(batch.JBB_BatchStatus == AccountingConstants.DsbJobBatchStatus.Close);
						var logMsg = logger.ToString();
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]DSB job close batch is automatically approved as no aggregated threshold has been specified.", logMsg);
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]Attempting to close approved batch.", logMsg);
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]DSB job close batch saved successfully.", logMsg);
					}
				});
			}
		}

		public void TestProcess_Empty()
		{
			AssertEquals("Pre-condition", true, string.IsNullOrEmpty(logger.ToString()));

			processor.Process(new CancellationToken());

			AssertEquals(@"Starting DSBs Job Batch Closure process.
DSB Job Batch Closure accomplished.", logger.ToString());
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestDeletingUnpostedConsolCostFail()
		{
			var companies = CreateCompany(4);
			var branches = CreateBranch(companies);

			var batches = new List<DsbJobCloseBatch>();
			var jobs = new List<Job>();

			foreach (var branch in branches)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
					chargeCode.AC_ChargeGroup = Core.Constants.ChargeType.Disbursement;
					chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
					chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

					var consol1 = TestObjectCreator.CreateConsol(consolNum: $"{Env.CurrentCompany.Code}_C01");
					var batch_APP_OutThreshold_Surplus = CreateDsbJobCloseBatch(1);
					batch_APP_OutThreshold_Surplus.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;
					batch_APP_OutThreshold_Surplus.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment1_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_1", "AUSYD", "USLAX");
					var job1_1 = TestObjectCreator.CreateJob(shipment1_1, false);
					var job1_1_sellAmount = 10000m;
					var job1_1_costAmount = 200m;

					var invoiceAR1_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_sellAmount);
					lineAR1_1.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_1, job1_1);

					var invoiceAP1_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV002", TestObjectCreator.CNY, 1m, job1_1_costAmount, 0m, 0m, job1_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP1_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_costAmount);
					lineAP1_1.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAP1_1, job1_1);

					var chargeWipAcr1_1 = TestObjectCreator.CreateCharge(job1_1, TestObjectCreator.CC1, 50m, 50m);

					var shipment1_2 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_2", "AUSYD", "USLAX");
					var job1_2 = TestObjectCreator.CreateJob(shipment1_2, false);
					var invoiceAR1_2 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV003", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_2 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_2, job1_2, chargeCode, TestObjectCreator.CNY, 1M, "tEST", 0);
					lineAR1_2.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_2, job1_2);

					var chargeWipAcr1_2 = TestObjectCreator.CreateCharge(job1_2, TestObjectCreator.CC1, 50m, 50m);

					consol1.Shipments.Add(shipment1_1);
					consol1.Shipments.Add(shipment1_2);

					jobs.Add(job1_1);
					jobs.Add(job1_2);
					batches.Add(batch_APP_OutThreshold_Surplus);
				}
			}

			Factory.Save();

			using (
				new DisposableAction(
				() => JobConsolCost.IsForceToMakeConsolCostDeleteFail_ForTestOnly = true,
				() => JobConsolCost.IsForceToMakeConsolCostDeleteFail_ForTestOnly = false)
			)
			using (AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 1000,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 0,
				JobLevelOfSurplusUpTo = 0
			}))
			{
				processor.Process(new CancellationToken());

				var newFactory = new BusinessObjectFactory();
				var batches_Reload = newFactory.Load<DsbJobCloseBatch>(new ZQuery(DsbJobCloseBatchSchema.PK, batches.Select(batch => batch.PK).ToArray()));
				AssertEquals(batches.Count, batches_Reload.Length);
				CombineAssertions("Batch close fail since deleting unposted consol cost fail", () =>
				{
					foreach (var batch in batches_Reload)
					{
						var company = newFactory.Load<GlbCompany>(batch.JBB_GC);
						AssertNotEquals(AccountingConstants.DsbJobBatchStatus.Close, batch.JBB_BatchStatus);
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]Fail to delete unposted consol cost.", logger.ToString());
					}
				});
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestClosingJobFail()
		{
			var companies = CreateCompany(4);
			var branches = CreateBranch(companies);

			var batches = new List<DsbJobCloseBatch>();
			var jobs = new List<Job>();

			foreach (var branch in branches)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
					chargeCode.AC_ChargeGroup = Core.Constants.ChargeType.Disbursement;
					chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
					chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

					var consol1 = TestObjectCreator.CreateConsol(consolNum: $"{Env.CurrentCompany.Code}_C01");
					var batch_APP_OutThreshold_Surplus = CreateDsbJobCloseBatch(1);
					batch_APP_OutThreshold_Surplus.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;
					batch_APP_OutThreshold_Surplus.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment1_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_1", "AUSYD", "USLAX");
					var job1_1 = TestObjectCreator.CreateJob(shipment1_1, false);
					var job1_1_sellAmount = 10000m;
					var job1_1_costAmount = 200m;

					var invoiceAR1_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_sellAmount);
					lineAR1_1.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_1, job1_1);

					var invoiceAP1_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV002", TestObjectCreator.CNY, 1m, job1_1_costAmount, 0m, 0m, job1_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP1_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_costAmount);
					lineAP1_1.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAP1_1, job1_1);

					var chargeWipAcr1_1 = TestObjectCreator.CreateCharge(job1_1, TestObjectCreator.CC1, 50m, 50m);

					var shipment1_2 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_2", "AUSYD", "USLAX");
					var job1_2 = TestObjectCreator.CreateJob(shipment1_2, false);
					var invoiceAR1_2 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV003", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_2 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_2, job1_2, chargeCode, TestObjectCreator.CNY, 1M, "tEST", 0);
					lineAR1_2.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_2, job1_2);

					var chargeWipAcr1_2 = TestObjectCreator.CreateCharge(job1_2, TestObjectCreator.CC1, 50m, 50m);

					consol1.Shipments.Add(shipment1_1);
					consol1.Shipments.Add(shipment1_2);

					jobs.Add(job1_1);
					jobs.Add(job1_2);
					batches.Add(batch_APP_OutThreshold_Surplus);
				}
			}

			Factory.Save();

			using (
				new DisposableAction(
				() => Job.IsForceToMakeCloseFail_ForTestOnly = true,
				() => Job.IsForceToMakeCloseFail_ForTestOnly = false)
			)
			using (AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 1000,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 0,
				JobLevelOfSurplusUpTo = 0
			}))
			{
				processor.Process(new CancellationToken());

				var newFactory = new BusinessObjectFactory();
				var batches_Reload = newFactory.Load<DsbJobCloseBatch>(new ZQuery(DsbJobCloseBatchSchema.PK, batches.Select(batch => batch.PK).ToArray()));
				AssertEquals(batches.Count, batches_Reload.Length);
				CombineAssertions("Batch close even job close fail", () =>
				{
					foreach (var batch in batches_Reload)
					{
						var company = newFactory.Load<GlbCompany>(batch.JBB_GC);
						AssertEquals(AccountingConstants.DsbJobBatchStatus.Close, batch.JBB_BatchStatus);
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]Attempting to close approved batch.", logger.ToString());
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]DSB job close batch saved successfully.", logger.ToString());
					}
				});

				var jobs_Reload = newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, jobs.Select(job => job.PK).ToArray()));
				AssertEquals(jobs.Count, jobs_Reload.Length);
				CombineAssertions("all the jobs close fail(test only)", () =>
				{
					foreach (var job in jobs_Reload)
					{
						AssertNotEquals(JobHeaderStatus.Closed.Code, job.JH_Status);
						AssertContains($"[{job.Company.GC_Code}]: An error occured. Could not close {job.JH_JobNum}", logger.ToString());
					}
				});
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestClosedJobInBatch()
		{
			var companies = CreateCompany(4);
			var branches = CreateBranch(companies);

			var batches = new List<DsbJobCloseBatch>();
			var jobsAlreadyClosed = new List<Job>();
			var jobsNeedClose = new List<Job>();

			foreach (var branch in branches)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
					chargeCode.AC_ChargeGroup = Core.Constants.ChargeType.Disbursement;
					chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
					chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

					var batch_APP_OutThreshold_Surplus = CreateDsbJobCloseBatch(1);
					batch_APP_OutThreshold_Surplus.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;
					batch_APP_OutThreshold_Surplus.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

					var shipment1_1 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_1", "AUSYD", "USLAX");
					var job1_1 = TestObjectCreator.CreateJob(shipment1_1, false);
					var job1_1_sellAmount = 10000m;
					var job1_1_costAmount = 200m;

					var invoiceAR1_1 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_1 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_sellAmount);
					lineAR1_1.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_1, job1_1);

					var invoiceAP1_1 = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV002", TestObjectCreator.CNY, 1m, job1_1_costAmount, 0m, 0m, job1_1_costAmount, 0m, 0m, TestObjectCreator.AALSHI);
					var lineAP1_1 = TestObjectCreator.CreateAPInvoiceLine(invoiceAP1_1, job1_1, chargeCode, TestObjectCreator.CNY, 1M, "tEST", job1_1_costAmount);
					lineAP1_1.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAP1_1, job1_1);

					job1_1.Close(
					(job, errorMessage) =>
					{
						Fail("job should be closed successfully , and should NOT go to this line.");
					},
					(sender, e) =>
					{
						e.Response = true;
					});
					jobsAlreadyClosed.Add(job1_1);

					var shipment1_2 = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_2", "AUSYD", "USLAX");
					var job1_2 = TestObjectCreator.CreateJob(shipment1_2, false);
					var invoiceAR1_2 = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV003", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
					var lineAR1_2 = TestObjectCreator.CreateARInvoiceLine(invoiceAR1_2, job1_2, chargeCode, TestObjectCreator.CNY, 1M, "tEST", 0);
					lineAR1_2.AL_JBB = batch_APP_OutThreshold_Surplus.PK;
					TestObjectCreator.CreateCharge(lineAR1_2, job1_2);

					jobsNeedClose.Add(job1_2);

					batches.Add(batch_APP_OutThreshold_Surplus);
				}
			}

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DisbursementJobsClosureConfiguration
			{
				AggregatedLevelOfShortfallUpTo = 1000,
				AggregatedLevelOfSurplusUpTo = 1000,
				JobLevelOfShortfallUpTo = 0,
				JobLevelOfSurplusUpTo = 0
			}))
			{
				processor.Process(new CancellationToken());

				var newFactory = new BusinessObjectFactory();
				var batches_Reload = newFactory.Load<DsbJobCloseBatch>(new ZQuery(DsbJobCloseBatchSchema.PK, batches.Select(batch => batch.PK).ToArray()));
				AssertEquals(batches.Count, batches_Reload.Length);
				CombineAssertions("Batch close even job close fail", () =>
				{
					foreach (var batch in batches_Reload)
					{
						var company = newFactory.Load<GlbCompany>(batch.JBB_GC);
						AssertEquals(AccountingConstants.DsbJobBatchStatus.Close, batch.JBB_BatchStatus);
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]Attempting to close approved batch.", logger.ToString());
						AssertContains($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]DSB job close batch saved successfully.", logger.ToString());
					}
				});

				var jobsAlreadyClosed_Reload = newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, jobsAlreadyClosed.Select(job => job.PK).ToArray()));
				AssertEquals(jobsAlreadyClosed.Count, jobsAlreadyClosed_Reload.Length);
				CombineAssertions("check message about jobs already closed", () =>
				{
					foreach (var job in jobsAlreadyClosed_Reload)
					{
						Assert(CheckJobInMessage(job.JH_JobNum, logger.ToString()));
					}
				});

				var jobsNeedClose_Reload = newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, jobsNeedClose.Select(job => job.PK).ToArray()));
				AssertEquals(jobsNeedClose.Count, jobsNeedClose_Reload.Length);
				CombineAssertions("check message is correct, excluding processing jobs.", () =>
				{
					foreach (var job in jobsNeedClose_Reload)
					{
						Assert(!CheckJobInMessage(job.JH_JobNum, logger.ToString()));
					}
				});
			}

			bool CheckJobInMessage(string jobNum, string logMessage)
			{
				return System.Text.RegularExpressions.Regex.IsMatch(
					logMessage
					, $@"Jobs have already been closed. List:[^\n]*\[{jobNum}\][^\n]*\n"
					, System.Text.RegularExpressions.RegexOptions.IgnoreCase
				);
			}
		}

		[TestDate(2020, 6, 20)]
		public void TestProcess_InvalidJournal()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;
			Factory.Save();

			var batch = CreateDsbJobCloseBatch(1);
			batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;
			batch.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateARInvoiceLine(arInvoice, job, chargeCode, TestObjectCreator.CNY, 1M, "Test", 1000m);
			line.AL_JBB = batch.PK;
			TestObjectCreator.CreateCharge(line, job);
			Factory.Save();

			processor.IsForceToReturnInvalidGLJournal_ForTestOnly = true;

			AssertEquals("Pre-condition", true, string.IsNullOrEmpty(logger.ToString()));

			processor.Process(new CancellationToken());

			AssertContains("Fail to create GL Journal for the batch.", logger.ToString());
		}

		[TestDate(2020, 6, 20)]
		public void TestProcess_WithLineHasNoBatch()
		{
			AssertProcessWithLinesHasNoBatchCore(2, new string[] {
				"Starting DSBs Job Batch Closure process."
				,"[EDI]|[JBB_1]Cannot close Job [S001, S002], as there are Posted Disbursement Charge linked to other disbursement open job close batch or did not link to any disbursement job close batch."
				,"[EDI]|[JBB_1]DSB job close batch saved successfully."
				,"DSB Job Batch Closure accomplished."
			});
		}

		[TestDate(2020, 6, 20)]
		public void TestProcess_WithManyLinesHasNoBatch()
		{
			AssertProcessWithLinesHasNoBatchCore(15, new string[] {
				"Starting DSBs Job Batch Closure process."
				,"[EDI]|[JBB_1]Cannot close Job [S001, S002, S003, S004, S005, S006, S007, S008, S009, S010], as there are Posted Disbursement Charge linked to other disbursement open job close batch or did not link to any disbursement job close batch."
				,"[EDI]|[JBB_1]Cannot close Job [S011, S012, S013, S014, S015], as there are Posted Disbursement Charge linked to other disbursement open job close batch or did not link to any disbursement job close batch."
				,"[EDI]|[JBB_1]DSB job close batch saved successfully."
				,"DSB Job Batch Closure accomplished."
			});
		}

		void AssertProcessWithLinesHasNoBatchCore(int numberOfJobsWithLineNotInBatch, IEnumerable<string> expectedMsgs)
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var chargeCode = testObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, testObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = testObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = testObjectCreator.GLHeader2.PK;
			Factory.Save();

			var batch = CreateDsbJobCloseBatch(1);
			batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
			batch.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			Factory.Save();

			var jobs = new Job[numberOfJobsWithLineNotInBatch];
			for (int i = 0; i < numberOfJobsWithLineNotInBatch; i++)
			{
				var shipment = testObjectCreator.CreateShipment($"S{(i + 1):000}", "AUSYD", "USLAX");
				jobs[i] = testObjectCreator.CreateJob(shipment, false);
			}
			Factory.Save();

			var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.CNY, 1M, testObjectCreator.ABIGAS);

			foreach (var job in jobs)
			{
				var line1 = testObjectCreator.CreateARInvoiceLine(arInvoice, job, chargeCode, testObjectCreator.CNY, 1M, $"Test {job.JH_JobNum}", 1000m);
				var line2 = testObjectCreator.CreateARInvoiceLine(arInvoice, job, chargeCode, testObjectCreator.CNY, 1M, $"Test {job.JH_JobNum} Empty Batch", 1200m);
				line1.AL_JBB = batch.PK;
				line2.AL_JBB = Guid.Empty;
				testObjectCreator.CreateCharge(line1, job);
				testObjectCreator.CreateCharge(line2, job);
			}
			Factory.Save();

			AssertEquals("Pre-condition", true, string.IsNullOrEmpty(logger.ToString()));

			processor.Process(new CancellationToken());

			var newFactory = Factory.CreateNewFactory();
			var reloadedBatch = newFactory.Load<DsbJobCloseBatch>(batch.PK);

			AssertEquals("Batch should be closed", AccountingConstants.DsbJobBatchStatus.Close, reloadedBatch.JBB_BatchStatus);
			CombineAssertions("Warning message recorded", () =>
				expectedMsgs.ForEach(expectedMsg => AssertContains(expectedMsg, logger.ToString()))
			);

			foreach (var job in jobs)
			{
				var reloadedJob = newFactory.Load<Job>(job.PK);
				AssertNotEquals("Job should NOT be closed", JobHeaderStatus.Codes.Closed, reloadedJob.JH_Status);
			}
		}

		public void TestProcess_Fail_BatchHaveNoJobs()
		{
			var companies = CreateCompany(1);
			var branch = CreateBranch(companies)[0];
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var batchAPP = CreateDsbJobCloseBatch(1);
				batchAPP.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;
				batchAPP.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

				var batchOPN = CreateDsbJobCloseBatch(2);
				batchOPN.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
				batchOPN.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

				Factory.Save();

				processor.Process(new CancellationToken());

				var newFactory = new BusinessObjectFactory();

				var batchAPP_Reload = newFactory.Load<DsbJobCloseBatch>(batchAPP.PK);
				AssertEquals(AccountingConstants.DsbJobBatchStatus.Cancel, batchAPP_Reload.JBB_BatchStatus);
				AssertContains($"[{branch.Company.GC_Code}]|[{batchAPP_Reload.JBB_BatchNumber}]DSB job close batch is cancelled as it has no related jobs.", logger.ToString());

				var batchOPN_Reload = newFactory.Load<DsbJobCloseBatch>(batchOPN.PK);
				AssertEquals(AccountingConstants.DsbJobBatchStatus.Cancel, batchOPN_Reload.JBB_BatchStatus);
				AssertContains($"[{branch.Company.GC_Code}]|[{batchOPN_Reload.JBB_BatchNumber}]DSB job close batch is cancelled as it has no related jobs.", logger.ToString());
			}
		}

		public void TestProcess_Empty_BatchNotOldEnough()
		{
			var companies = CreateCompany(2);
			var branches = CreateBranch(companies);
			Factory.Save();

			foreach (var branch in branches)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var batch = CreateDsbJobCloseBatch(1);
					batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
					batch.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow;
				}
			}
			Factory.Save();

			AssertEquals("Pre-condition", true, string.IsNullOrEmpty(logger.ToString()));

			processor.Process(new CancellationToken());

			AssertEquals(@"Starting DSBs Job Batch Closure process.
DSB Job Batch Closure accomplished.", logger.ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();

			logger = new LoggerForTesting();
			processor = new DsbJobBatchCloseProcessor(new JCSLogger(logger));
		}

		#region Implementation

		GlbCompany[] CreateCompany(int noOfCompany)
		{
			var companies = new GlbCompany[noOfCompany];
			var proxies = new OrgHeader[noOfCompany];

			for (int i = 0; i < noOfCompany; i++)
			{
				proxies[i] = TestObjectCreator.CreateOrgHeader("PRX" + i.ToString(), true, true);
			}
			Factory.Save();

			for (int i = 0; i < noOfCompany; i++)
			{
				companies[i] = TestObjectCreator.CreateNewCompany("CO" + i.ToString(), orgProxy: proxies[i]);
			}
			return companies;
		}

		GlbBranch[] CreateBranch(GlbCompany[] companies)
		{
			GlbBranch[] branches = new GlbBranch[companies.Length];
			for (int i = 0; i < companies.Length; i++)
			{
				branches[i] = TestObjectCreator.CreateBranch("B" + i.ToString(), companies[i]);
			}
			return branches;
		}

		DsbJobCloseBatch CreateDsbJobCloseBatch(int batchNumberSalt)
		{
			var batch = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch.JBB_TotalAmount = 4000;
			batch.JBB_GC = Env.CurrentCompanyPK;
			batch.JBB_LargestAmount = 1000;
			batch.JBB_SmallestAmount = 5;
			batch.JBB_BatchNumber = $"JBB_{batchNumberSalt}";
			batch.JBB_SystemCreateTimeUtc = DateTime.UtcNow;
			batch.JBB_SystemCreateUser = "~BP";
			return batch;
		}

		void CreateBatchWithContext(int numberSalt, AccChargeCode chargeCode, out DsbJobCloseBatch batch, out Job job, out Charge charge, out JobConsolCost consolCost)
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: $"{Env.CurrentCompany.Code}{numberSalt}");
			batch = CreateDsbJobCloseBatch(numberSalt);
			batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;
			batch.JBB_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var shipment = TestObjectCreator.CreateShipment($"{Env.CurrentCompany.Code}_1", "AUSYD", "USLAX");
			job = TestObjectCreator.CreateJob(shipment, false);

			var invoiceAR = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV00{numberSalt}01", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
			var lineAR = TestObjectCreator.CreateARInvoiceLine(invoiceAR, job, chargeCode, TestObjectCreator.CNY, 1M, "TestAR", 10000m);
			lineAR.AL_JBB = batch.PK;
			TestObjectCreator.CreateCharge(lineAR, job);

			var invoiceAP = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV00{numberSalt}02", TestObjectCreator.CNY, 1m, 200m, 0m, 0m, 200m, 0m, 0m, TestObjectCreator.AALSHI);
			var lineAP = TestObjectCreator.CreateAPInvoiceLine(invoiceAP, job, chargeCode, TestObjectCreator.CNY, 1M, "TestAP", 200m);
			lineAP.AL_JBB = batch.PK;
			TestObjectCreator.CreateCharge(lineAP, job);

			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 50m, 50m);

			consol.Shipments.Add(shipment);
			consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC3, 500);

			AssertEquals("Pre-condition", false, consolCost.IsPosted);
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;

		DsbJobBatchCloseProcessor processor;
		ILogger logger;

		#endregion
	}
}
