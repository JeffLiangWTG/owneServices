using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class DSBJobBatcher : JCSSubscriber
	{
		public DSBJobBatcher(IJCSLogger logger)
			: base(logger)
		{
			batchPkForJob = new Dictionary<ZGuid, ZGuid>();
		}

		protected override string Code => "DSB";

		protected override bool CanProcess(Job job)
		{
			CurrentJob = job;

			return canProcess ??
				(canProcess =
				base.CanProcess(job)
				&& IsDsbJob()
				&& DSBLinesNeedBatch.Any()
				&& JobWithinThresholds()
				).Value;

			bool JobWithinThresholds()
			{
				var threshold = AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.Value;
				var isEmptyThreshold = threshold.JobLevelOfSurplusUpTo == 0 && threshold.JobLevelOfShortfallUpTo == 0;
				if (isEmptyThreshold)
				{
					return true;
				}
				else
				{
					var result = true;
					var jobDisbursementBalance = DSBLinesNeedBatch.Sum(line => line.AL_LineAmount);
					var jobOutThresholds =
							jobDisbursementBalance > threshold.JobLevelOfSurplusUpTo
						||	jobDisbursementBalance < -threshold.JobLevelOfShortfallUpTo;
					if (jobOutThresholds)
					{
						Logger.LogDiagnostic($"Job Disbursement Balance {jobDisbursementBalance:0.000000} is not within the threshold range (Surplus Up To: {threshold.JobLevelOfSurplusUpTo} and Shortfall UpTo: {threshold.JobLevelOfShortfallUpTo}). Therefore, it cannot be included in a DSB batch.");
						result = false;
					}
					return result;
				}
			}
		}
		bool? canProcess;

		protected override bool Process(Job job, bool changesWillBeSavedToDB)
		{
			if (changesWillBeSavedToDB)
			{
				CurrentJob = job;
				Logger.LogDebug("Starting DSB candidate picking.");

				var batch = job.Factory.Load<DsbJobCloseBatch>(GetBatchPkForJob());
				var linesPK = DSBLinesNeedBatch.Select(line => line.AL_PK.ToGuid()).ToArray();

				DSBBatchHelper.UpdateLinesBatch(linesPK, batch.PK.ToGuid());
				Logger.LogDebug($"Added to DSB batch {batch.JBB_BatchNumber} successfully");
			}
			else
			{
				Logger.LogDiagnostic($"This job is eligible for adding to a DSB batch.");
			}

			return true;
		}

		protected override bool CanChainToNextSubscriber() => !IsDsbJob();

		ZGuid GetBatchPkForJob(bool createWhenEmpty = true)
		{
			if (getBatchPkForJobLastRunningDate.Date != ZDateTime.UtcToday.Date)
			{
				getBatchPkForJobLastRunningDate = ZDateTime.UtcToday.Date;// avoid crossing Date when starting at midnight during job processing.
				batchPkForJob.Clear();
			}

			if (batchPkForJob.TryGetValue(CurrentJob.JH_GC, out var cachedValue))
			{
				return cachedValue;
			}
			else
			{
				var batchPK = DSBBatchHelper.GetBatchPkForJob(CurrentJob, ZDateTime.UtcToday.ToDateTime(), createWhenEmpty);
				if (batchPK.IsValid)
				{
					batchPkForJob[CurrentJob.JH_GC] = batchPK;
					return batchPK;
				}
			}
			return ZGuid.Empty;
		}
		readonly Dictionary<ZGuid, ZGuid> batchPkForJob;
		ZDateTime getBatchPkForJobLastRunningDate;

		Job CurrentJob
		{
			get { return currentJob; }
			set
			{
				if (currentJob != value)
				{
					currentJob = value;
					dsbLines = null;
					canProcess = null;
					isDsbJob = null;
				}
			}
		}
		Job currentJob;

		bool IsDsbJob()
		{
			return isDsbJob
				?? (isDsbJob = AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.Value
				&& CheckJobCanBeAutoClosed()
				&& (DSBLinesNeedBatch.Any() || CheckJobHaveUnClosedBatches())
				).Value;

			bool CheckJobCanBeAutoClosed()
			{
				var result = true;
				var helper = AutoJobClosureHelper.CreateHelperToCloseAJob(CurrentJob);
				if (!helper.JobsThatCanBeClosed.Any())
				{
					Logger.LogDiagnostic(FormattableString.Invariant($"This job cannot be automatically closed. \r\n{helper.GetAllErrorMessages()}"));
					result = false;
				}
				return result;
			}

			bool CheckJobHaveUnClosedBatches()
			{
				var otherBatchPKs = DSBLines
					.Where(line => line.AL_JBB.IsValid)
					.Select(line => line.AL_JBB)
					.Distinct()
					.ToArray();

				var queryOtherUnClosedBatch = new ZQuery(DsbJobCloseBatchSchema.PK, otherBatchPKs)
					.AddToFilter(DsbJobCloseBatchSchema.JBB_BatchStatus,
						SQLComparisonOperator.NotEqual,
						new string[] { AccountingConstants.DsbJobBatchStatus.Close, AccountingConstants.DsbJobBatchStatus.Cancel }
				);

				return CurrentJob.Factory.Exists(typeof(DsbJobCloseBatch), queryOtherUnClosedBatch);
			}
		}
		bool? isDsbJob;

		IEnumerable<(ZGuid AL_PK, ZGuid AL_JBB, ZDecimal AL_LineAmount)> DSBLinesNeedBatch => DSBLines.Where(line => line.AL_JBB.IsEmpty);

		(ZGuid AL_PK, ZGuid AL_JBB, ZDecimal AL_LineAmount)[] DSBLines => dsbLines ?? (dsbLines = GetTransactionLinesWithDSBChargeCodes());
		(ZGuid AL_PK, ZGuid AL_JBB, ZDecimal AL_LineAmount)[] dsbLines;

		(ZGuid AL_PK, ZGuid AL_JBB, ZDecimal AL_LineAmount)[] GetTransactionLinesWithDSBChargeCodes() => CurrentJob != null
			? DSBBatchHelper.GetTransactionLinesWithDSBChargeCodes(CurrentJob).ToArray()
			: Array.Empty<(ZGuid AL_PK, ZGuid AL_JBB, ZDecimal AL_LineAmount)>();
	}
}
