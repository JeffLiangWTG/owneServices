using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class DsbJobBatchCloseProcessor
	{
		public DsbJobBatchCloseProcessor(IJCSLogger logger)
		{
			Logger = logger;
		}

		public void Process(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			Logger.LogInformation("Starting DSBs Job Batch Closure process.");

			var factory = new BusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = $"JBC Factory"
			};

			var errorBatchPKs = new List<ZGuid>();
			DsbJobCloseBatch batch;

			while ((batch = GetNextBatch(factory, errorBatchPKs.ToArray())) != null)
			{
				token.ThrowIfCancellationRequested();
				var isSuccessfullyProcessed = true;

				try
				{
					var company = factory.Load<GlbCompany>(batch.JBB_GC);
					var branch = AccountingUtils.GetTopOneActiveBranchOfCompany(company.PK, factory);

					if (branch == null)
					{
						Logger.LogError($"Cannot process batch {batch.JBB_BatchNumber} because batch company {company.GC_Code} does not have active branch.");
						isSuccessfullyProcessed = false;
						continue;
					}

					using (Logger.SetLogPrefix($"[{company.GC_Code}]|[{batch.JBB_BatchNumber}]"))
					using (DisposableEnvironment.ForCompany(company.GC_Code))
					{
						var beforeBatchStatus = batch.JBB_BatchStatus;
						Logger.LogInformation($"Starting to process DSB job close batch, with status [{beforeBatchStatus}]");

						var jobInfosAll = GetJobInfos(batch.Factory, batch.PK);
						if (jobInfosAll.IsNullOrEmpty())
						{
							batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Cancel;
							Logger.LogInformation($"DSB job close batch is cancelled as it has no related jobs.");
						}

						if (batch.JBB_BatchStatus == AccountingConstants.DsbJobBatchStatus.Open)
						{
							batch.JBB_TotalAmount = jobInfosAll.Sum(x => x.DisbursementBalance);
							batch.JBB_LargestAmount = jobInfosAll.MaxOrDefault(x => x.DisbursementBalance);
							batch.JBB_SmallestAmount = jobInfosAll.MinOrDefault(x => x.DisbursementBalance);
							batch.JBB_BatchStatus = CheckIsBatchAmountWithinThreshold(batch.JBB_TotalAmount);

							if (batch.JBB_BatchStatus == AccountingConstants.DsbJobBatchStatus.Approve)
							{
								batch.JBB_ApprovalTimeUtc = ZDateTime.UtcNow;
								batch.JBB_GS_NKApprovingUser = User.ServiceUserCode;
							}
						}

						if (batch.JBB_BatchStatus == AccountingConstants.DsbJobBatchStatus.Approve)
						{
							Logger.LogInformation($"Attempting to close approved batch.");

							var jobs = factory.Load<Job>(new ZQuery(JobHeaderSchema.PK, jobInfosAll.Select(jobInfo => jobInfo.JobPK)));
							var jobPKToClose = jobInfosAll.Where(x => !x.HasUnBatchedLine && !x.HasOtherUnClosedBatch).Select(x => x.JobPK).ToHashSet();

							var jobsAlreadyBeenClosed = jobs.Where(x => x.JH_Status == JobHeaderStatus.Closed.Code).ToArray();
							var jobsToClose = jobs.Where(x => jobPKToClose.Contains(x.PK) && x.JH_Status != JobHeaderStatus.Closed.Code).ToArray();
							var jobsNotToClose = jobs.Where(x => !jobPKToClose.Contains(x.PK) && x.JH_Status != JobHeaderStatus.Closed.Code).ToArray();

							HandleJobsNotClosed(jobsNotToClose);

#if NETFRAMEWORK
							var chunkOnes = jobsAlreadyBeenClosed.Chunk(50);
#else
							var chunkOnes = System.Linq.Enumerable.Chunk(jobsAlreadyBeenClosed, 50);
#endif
							chunkOnes.ForEach(chunkJobs =>
								{
									Logger.LogInformation($"Jobs have already been closed. List:{string.Join(", ", chunkJobs.Select(job => $"[{job.JH_JobNum}]").ToArray())}");
								}
							);

							if (CloseJobs(jobsToClose))
							{
								if (CreateGLJournalAndAttachBatchDocument(batch, out var journal))
								{
									Logger.LogInformation($"Create GL Journal successfully.");
									batch.JBB_AH_Journal = journal.PK;
									batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Close;
								}
								else
								{
									Logger.LogError($"Fail to create GL Journal for the batch.");
									isSuccessfullyProcessed = false;
								}
							}
							else
							{
								isSuccessfullyProcessed = false;
							}
						}

						if (isSuccessfullyProcessed)
						{
							using (factory.SetTempContext(BusinessContext.SavingDsbJobBatch))
							{
								Logger.LogInformation($"Attempting to save batch changes, with status changed from [{beforeBatchStatus}] to [{batch.JBB_BatchStatus}].");
								factory.Save();
								Logger.LogInformation($"DSB job close batch saved successfully.");
							}
						}
						else
						{
							Logger.LogError($"Abort DSB job close batch processing due to errors.");
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					isSuccessfullyProcessed = false;
					Logger.LogError($"[{batch.JBB_BatchNumber}]Abort DSB job close batch processing. {ex.Message}");
				}
				finally
				{
					if (!isSuccessfullyProcessed)
					{
						errorBatchPKs.Add(batch.PK);
					}
				}
			}

			Logger.LogInformation("DSB Job Batch Closure accomplished.");
		}

		bool CreateGLJournalAndAttachBatchDocument(DsbJobCloseBatch batch, out GLJournal journal)
		{
#if DEBUG
			if (Globals.IsTest && IsForceToReturnInvalidGLJournal_ForTestOnly)
			{
				journal = null;
				return false;
			}
#endif
			journal = new DSBJobCloseGJLHelper(batch.Factory).CreateGLJournal(batch.PK);
			if (journal != null)
			{
				new DSBJobCloseDocHelper(batch.Factory).CreateAndAttachBatchDocument(batch, journal);
			}
			return journal != null;
		}

		bool CloseJobs(IEnumerable<Job> jobs)
		{
			if (jobs.IsNullOrEmpty())
			{
				return true;
			}

			var (isDeleted, message) = JobConsolCost.DeleteUnpostedConsolCostsLinkedWithJobs(jobs);
			if (!isDeleted && !string.IsNullOrEmpty(message))
			{
				Logger.LogError($"Fail to delete unposted consol cost. \r\n{message}");
				return false;
			}

			foreach (var jobToClose in jobs)
			{
				jobToClose.Close(
					(job, errorMessage) =>
					{
						Logger.LogError(FormattableString.Invariant($"[{job.Company.GC_Code}]: An error occured. Could not close {job.JH_JobNum}. \r\n{errorMessage}"));
					},
					(sender, e) =>
					{
						e.Response = true;
					});
			}

			return true;
		}

		void HandleJobsNotClosed(IEnumerable<Job> jobsWithLinesLinkingToOtherUnclosedBatch)
		{
#if NETFRAMEWORK
			var chunks = jobsWithLinesLinkingToOtherUnclosedBatch.OrderBy(x => x.JH_JobNum).Chunk(10);
#else
			var chunks = System.Linq.Enumerable.Chunk(jobsWithLinesLinkingToOtherUnclosedBatch.OrderBy(x => x.JH_JobNum), 10);
#endif
			foreach (var chunk in chunks)
			{
				var builder = new ZStringBuilder();
				chunk.ForEach(x => builder.Append(x.JH_JobNum));
				Logger.LogInformation($"Cannot close Job [{builder.ToStringWithDelimiterBetweenAppends(", ").Trim(' ', ',')}], as there are Posted Disbursement Charge linked to other disbursement open job close batch or did not link to any disbursement job close batch.");
			}
		}

		string CheckIsBatchAmountWithinThreshold(ZDecimal batchTotalAmount)
		{
			var threshold = AccountingConfigurationRegistry.Instance.DisbursementJobsClosureConfiguration.Value;

			var isEmptyThresholdSetting = threshold.AggregatedLevelOfSurplusUpTo == 0 && threshold.AggregatedLevelOfShortfallUpTo == 0;
			if (isEmptyThresholdSetting)
			{
				Logger.LogInformation($"DSB job close batch is automatically approved as no aggregated threshold has been specified.");
				return AccountingConstants.DsbJobBatchStatus.Approve;
			}

			var result =
				batchTotalAmount <= threshold.AggregatedLevelOfSurplusUpTo
				&& batchTotalAmount >= -threshold.AggregatedLevelOfShortfallUpTo;
			if (result)
			{
				Logger.LogInformation($"DSB job close batch is automatically approved as batch total [{batchTotalAmount}] is within the aggregated threshold (SurplusUpTo [{threshold.AggregatedLevelOfSurplusUpTo}], ShortfallUpTo: [{threshold.AggregatedLevelOfShortfallUpTo}]).");
				return AccountingConstants.DsbJobBatchStatus.Approve;
			}
			else
			{
				Logger.LogInformation($"DSB job close batch approval request is created as batch total [{batchTotalAmount}] exceeded the aggregate threshold (SurplusUpTo [{threshold.AggregatedLevelOfSurplusUpTo}], ShortfallUpTo: [{threshold.AggregatedLevelOfShortfallUpTo}]).");
				return AccountingConstants.DsbJobBatchStatus.RequireApproval;
			}
		}

		DsbJobCloseBatch GetNextBatch(BusinessObjectFactory factory, ZGuid[] excludedPKs)
		{
			var query = new ZDBOnlyQuery(typeof(DsbJobCloseBatch));
			query.AddToFilter(DsbJobCloseBatchSchema.JBB_BatchStatus, new string[] { AccountingConstants.DsbJobBatchStatus.Open, AccountingConstants.DsbJobBatchStatus.Approve });
			query.AddToFilter(DsbJobCloseBatchSchema.JBB_SystemCreateTimeUtc, SQLComparisonOperator.LessThan, ZDateTime.UtcToday.Date);
			query.AddToFilter(DsbJobCloseBatchSchema.PK, SQLComparisonOperator.NotEqual, excludedPKs);
			query.MaximumRows = 1;

			return factory.LoadTop1<DsbJobCloseBatch>(query);
		}

		IEnumerable<(ZGuid JobPK, ZDecimal DisbursementBalance, ZBool HasUnBatchedLine, ZBool HasOtherUnClosedBatch)> GetJobInfos(BusinessObjectFactory factory, ZGuid batchPK)
		{
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load("SELECT * FROM GetDsbJobAmount(@BatchPK)", new[] { ZSqlParameter.New("@BatchPK", batchPK, DsbJobCloseBatchSchema.PK) });

			var result = new List<(ZGuid JobPK, ZDecimal DisbursementBalance, ZBool HasUnBatchedLine, ZBool HasOtherUnClosedBatch)>();

			foreach (DynamicBusinessObject row in collection)
			{
				var jobPK = new ZGuid(row["JobPK"]);
				var disbursementBalance = new ZDecimal(row["DisbursementBalance"]);
				var hasUnBatchedLine = new ZBool(row["HasUnBatchedLine"]);
				var hasOtherUnClosedBatch = new ZBool(row["HasOtherUnClosedBatch"]);
				result.Add((jobPK, disbursementBalance, hasUnBatchedLine, hasOtherUnClosedBatch));
			}

			return result;
		}

		IJCSLogger Logger { get; }

#if DEBUG
		public bool IsForceToReturnInvalidGLJournal_ForTestOnly;
#endif
	}
}
