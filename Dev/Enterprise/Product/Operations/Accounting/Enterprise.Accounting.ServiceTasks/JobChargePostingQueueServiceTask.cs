using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.ServiceTasks;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	JobChargePostingQueueServiceTask.Code,
	JobChargePostingQueueServiceTask.Description,
	"ACC",
	typeof(JobChargePostingQueueServiceTask),
	IsMandatory = true,
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "10minutes",
	ActiveByDefault = true)
]

namespace Enterprise.Accounting.ServiceTasks
{
	public class JobChargePostingQueueServiceTask : ServiceProviderImpl
	{
		public const string Code = "JPQ";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Job Charge Posting Queue Service Task";

		const int MaxRetrysForConcurrency = 3;

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "Job Charge Posting Queue processing is starting."));

			if (!token.IsCancellationRequested)
			{
				RunJobChargesPostingQueue(token);
			}

			if (!token.IsCancellationRequested && ExistsJobChargePostingQueueRecords())
			{
				try
				{
					ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("JPQ");
					ServiceLogger.Log(LogType.Information, "Service Task has been nudged successfully");
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServiceLogger.Log(LogType.Error, FormattableString.Invariant($"Failed to Nudge. Error:\r\n{ex.Message}"));
				}
			}

			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "Job Charge Posting Queue processing has finished."));
		}

		void RunJobChargesPostingQueue(CancellationToken token)
		{
			var processQueue = new List<Func<BusinessObjectFactory, int, ProcessResult>>();
			var connection = Db.Connection;
			var branchPk = GlbBranch.GetFirstActiveBranch()?.PK ?? ZGuid.Empty;

			using (DisposableEnvironment.ForBranch(branchPk.ToGuid(), false))
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				var postingQueues = GetPostingQueues(connection);
				if (!postingQueues.Any())
				{
					ServiceLogger.Log(LogType.Debug, "No Job Charge Posting Queue records were found.");
					return;
				}

				var factory = new BusinessObjectFactory();
				foreach (var postingInstruction in postingQueues.Select(x => x.PostingInstruction).Distinct())
				{
					var postingInstructionQueue = postingQueues.Where(x => x.PostingInstruction == postingInstruction).ToArray();
					var chargeQuery = GetQueryForChargePKs(postingInstructionQueue);
					var charge = LoadChargeForUserContext(factory, chargeQuery, connection);

					if (charge == null)
					{
						continue;
					}

					processQueue.Add((businessObjectFactory, retryCount) =>
					{
						var userContext = FetchUserContext(charge, Env.CurrentUser);
						using (Env.SetTemporaryUserContext(userContext))
						{
							return ProcessSingleGroup(businessObjectFactory, postingInstruction, postingInstructionQueue, chargeQuery, retryCount, token);
						}
					});
				}

				transactionManager.CommitTransaction();
			}

			processQueue.ForEach(queue =>
			{
				var processResult = ProcessResult.Next;
				var retryCount = 1;
				do
				{
					processResult = queue.Invoke(new BusinessObjectFactory(), retryCount);
					retryCount++;
				} while (processResult == ProcessResult.Retry && retryCount <= MaxRetrysForConcurrency);
			});
		}

		ProcessResult ProcessSingleGroup(
			BusinessObjectFactory factory,
			string postingInstruction,
			IEnumerable<IJobChargePostingQueue> postingInstructionQueue,
			ZDBOnlyQuery chargeQuery,
			int retryCount,
			CancellationToken token)
		{
			var groupID = postingInstructionQueue.First().GroupID;
			string chargeType = string.Empty;

			try
			{
				IProcessor processor;
				var firstQueueEntry = postingInstructionQueue.First();
				var parent = factory.Load(firstQueueEntry.ParentTableCode, firstQueueEntry.ParentID) as IJobInvoicingPlugIn;
				var charges = factory.Load<Charge>(chargeQuery);
				switch (postingInstruction)
				{
					case JobChargePostingQueueLookups.PostCost:
						chargeType = (NoResString)"Cost";
						processor = AccountingDependencyFactory.GetJobCostQueuePoster(parent, charges, postingInstructionQueue);
						break;
					case JobChargePostingQueueLookups.PostRevenue:
						chargeType = (NoResString)"Sell";
						processor = AccountingDependencyFactory.GetJobRevenueQueuePoster(parent, charges, postingInstructionQueue);
						break;
					default:
						throw new ArgumentOutOfRangeException(FormattableString.Invariant($"Invalid charge posting instruction {postingInstruction}"));
				}

				var notification = new NotificationCollection();
				try
				{
					processor.Process(notification, token);
					factory.Save();
				}
				catch (LogSubscriberToAbortLogGroupProcessingSilentlyException ex)
				{
					var newFactory = new BusinessObjectFactory();
					ex.Emails?.ForEach(x => x.Create(newFactory));
					newFactory.Save();
				}
				catch (ComplianceSequenceRelatedException ex)
				{
					notification.AddError(ex.UserFriendlyMessage);
				}
				catch (ZSaveConcurrencyException ex)
				{
					var isLastAttempt = retryCount >= MaxRetrysForConcurrency;
					var logType = isLastAttempt ? LogType.Error : LogType.Debug;
					ServiceLogger.Log(logType, FormattableString.Invariant($"{chargeType} charges from the job {parent.JobNumber} with GroupID {groupID} can not be posted due to concurrency error. Attempt number {retryCount} of {MaxRetrysForConcurrency}."));
					if (!isLastAttempt)
					{
						return ProcessResult.Retry;
					}
					else
					{
						ServiceLogger.Log(LogType.Error, ex.Message);
						return ProcessResult.Next;
					}
				}

				var errorMessage = new ZStringBuilder();
				if (notification.HasErrors())
				{
					foreach (var item in notification.GetErrors())
					{
						errorMessage.AppendLine(item.Message);
					}
				}

				if (!errorMessage.IsEmpty)
				{
					ServiceLogger.Log(LogType.Error, FormattableString.Invariant($"{chargeType} charges from the job {parent.JobNumber} with GroupID {groupID} can not be posted. Error message:\r\n{errorMessage}"));
				}
				else
				{
					ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"{chargeType} charges from the job {parent.JobNumber} with GroupID {groupID} were posted successfully"));
				}
			}
			catch (ZException ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Log(LogType.Error, FormattableString.Invariant($"Posting {chargeType} charges with GroupID {groupID} failed with an error:\r\n{ex.Message}"));
			}

			return ProcessResult.Next;
		}

		enum ProcessResult
		{
			/// <summary>
			/// Move to process next item in queue.
			/// </summary>
			Next,
			/// <summary>
			/// Retry current item in queue.
			/// </summary>
			Retry
		}

		Charge LoadChargeForUserContext(BusinessObjectFactory factory, ZDBOnlyQuery query, DbConnection connection)
		{
			var chargeForUserContext = factory.LoadTop1<Charge>(query);
			if (chargeForUserContext == null)
			{
				var msg = GetConstraintErrorMessage(connection);
				ErrorReporter.ReportDeveloperExceptionOnce(msg, null);
				return null;
			}

			return chargeForUserContext;
		}

		IUserContext FetchUserContext(Charge chargeForUserContext, IUser currentUser)
		{
			return new UserContext(currentUser, chargeForUserContext.JR_GB.ToGuid(), chargeForUserContext.JR_GE.ToGuid());
		}

		ZDBOnlyQuery GetQueryForChargePKs(IEnumerable<IJobChargePostingQueue> queue)
		{
			var query = new ZDBOnlyQuery(typeof(Charge));
			query.AddToFilter(JobChargeSchema.PK, queue.Select(x => x.ChargePK));
			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		List<IJobChargePostingQueue> GetPostingQueues(DbConnection connection)
		{
			var result = new List<IJobChargePostingQueue>();

			var sqlText = @"DELETE FROM dbo.JobChargePostingQueue 
OUTPUT DELETED.JPQ_ParentID, DELETED.JPQ_ParentTableCode,
DELETED.JPQ_GroupID, DELETED.JPQ_JR, DELETED.JPQ_PostingInstruction,
DELETED.JPQ_HashVersion, DELETED.JPQ_ChargeValuesHash
FROM dbo.JobChargePostingQueue WITH (READPAST, READCOMMITTEDLOCK)
WHERE JPQ_GroupID = (SELECT TOP 1 topGroup.JPQ_GroupID FROM dbo.JobChargePostingQueue topGroup WITH (READPAST, READCOMMITTEDLOCK) ORDER BY topGroup.JPQ_GroupID)";

			using (var reader = connection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var groupID = (int)reader[JobChargePostingQueue.Schema.JPQ_GroupID];
					var parentID = (Guid)reader[JobChargePostingQueue.Schema.JPQ_ParentID];
					var parentTableCode = (string)reader[JobChargePostingQueue.Schema.JPQ_ParentTableCode];
					var postingInstruction = (string)reader[JobChargePostingQueue.Schema.JPQ_PostingInstruction];
					var chargePK = (Guid)reader[JobChargePostingQueue.Schema.JPQ_JR];
					var hashVersion = (byte)reader[JobChargePostingQueue.Schema.JPQ_HashVersion];
					var valueHash = new ZBlob((byte[])reader[JobChargePostingQueue.Schema.JPQ_ChargeValuesHash]);

					var queueData = new JobChargePostingQueueData(groupID, parentID, parentTableCode, postingInstruction, chargePK, hashVersion, valueHash);
					result.Add(queueData);
				}
			}

			return result;
		}

		#region Check Constraint

		string GetConstraintErrorMessage(DbConnection connection)
		{
			const string constraintName = "JobChargePostingQueue_JPQ_JR_FK2_JobCharge_RRR_120N";

			var constraintRemoved = !DbObjectCreator.ObjectExists(connection, constraintName);
			if (constraintRemoved)
			{
				return string.Format(CultureInfo.CurrentCulture, $"Constraint {constraintName} was removed.");
			}
			else if (IsConstraintDisabled(connection, constraintName))
			{
				return string.Format(CultureInfo.CurrentCulture, $"Constraint {constraintName} was disabled.");
			}

			return string.Format(CultureInfo.CurrentCulture, $"Constraint {constraintName} was valid.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		bool IsConstraintDisabled(DbConnection connection, string constraintName)
		{
			var sqlText = $"SELECT is_disabled FROM sys.foreign_keys WHERE NAME = '{constraintName}'";

			using (var reader = connection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					return (bool)reader["is_disabled"];
				}
			}

			return false;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		bool ExistsJobChargePostingQueueRecords()
		{
			// need to access db directly
			string sql = $@"IF EXISTS (SELECT NULL FROM {JobChargePostingQueueSchema.Constants.SqlSchemaName}.{JobChargePostingQueueSchema.Constants.TableName}) SELECT 1 ELSE SELECT 0";
			using (var command = Db.Connection.Command(sql))
			{
				return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
			}
		}

		IAccountingDependencyFactory AccountingDependencyFactory { get; } = ObjectFactory.Get<IAccountingDependencyFactory>();
	}
}
