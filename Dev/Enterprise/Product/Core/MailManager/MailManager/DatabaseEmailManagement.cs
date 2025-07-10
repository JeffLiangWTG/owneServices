
using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MailManager
{
	public class DatabaseEmailManagement
	{
		public static DatabaseEmailManagement Create()
		{
			return OverridableInstance.Value;
		}

		public static readonly LazyOverridable<DatabaseEmailManagement> OverridableInstance = new LazyOverridable<DatabaseEmailManagement>(() =>
		{
			Type type = typeof(DatabaseEmailManagement);
			type = TypeDecider.GetTypeForBinding(type);
			return (DatabaseEmailManagement)Activator.CreateInstance(type);
		});

		public virtual int PurgeProcessedOutgoingEmailOlderThan(int daysOld)
		{
			var additionalWhereClause = string.Format(Culture.Invariant, (NoResString)"{0} = '{1}' AND (({2} = '{3}') OR ({2} = '{4}'))",
				MI_Direction, Transmit, MI_Status, Sent, Failed);

			return CleanOldMail(() => GetDeleteCommand(daysOld, additionalWhereClause));
		}

		public virtual int PurgeUnsentOutgoingEmailOlderThan(int daysOld)
		{
			var additionalWhereClause = string.Format(Culture.Invariant, (NoResString)"{0} = '{1}' AND {2} = '{3}'",
				MI_Direction, Transmit, MI_Status, Queued);

			return CleanOldMail(() => GetDeleteCommand(daysOld, additionalWhereClause));
		}

		public virtual int PurgeProcessedIncomingEmailOlderThan(int daysOld)
		{
			var additionalWhereClause = string.Format(Culture.Invariant, (NoResString)"{0} = '{1}' AND {2} = '{3}'",
				MI_Direction, Receive, MI_Status, Processed);

			return CleanOldMail(() => GetDeleteCommand(daysOld, additionalWhereClause));
		}

		public virtual int PurgeIncomingEmailOlderThan(int daysOld)
		{
			var additionalWhereClause = string.Format(Culture.Invariant, "{0} = '{1}'",
				MI_Direction, Receive);

			return CleanOldMail(() => GetDeleteCommand(daysOld, additionalWhereClause));
		}

		public virtual int SetToFailedQueuedWithAckOutgoingEmailOlderThan(int daysOld)
		{
			var setClause = string.Format(Culture.Invariant, (NoResString)"SET {0} = '{1}'",
				MI_Status, Failed);

			var additionalWhereClause = string.Format(Culture.Invariant, (NoResString)"{0} = '{1}' AND {2} = '{3}'",
				MI_Direction, Transmit, MI_Status, QueuedWithAcknowledgement);

			return CleanOldMail(() => GetUpdateCommand(daysOld, setClause, additionalWhereClause));
		}

		public virtual int TruncateBodyOutgoingUpgradeEmailOlderThan(int daysOld)
		{
			return 0;
		}

		protected int CleanOldMail(Func<DbCommand> commandGenerator)
		{
			int rowsAffectedOnThisCommand;
			int totalRowsPurged = 0;
			var totalBatchCount = 0;
			var waiter = new LowPriorityProcessPauser();
			do
			{
				if (!Globals.IsTest)
				{
					waiter.Wait();
				}

				using (var manager = Db.Connection.BeginTransactionWithManager())
				{
					using (var command = commandGenerator())
					{
						rowsAffectedOnThisCommand = (int)command.ExecuteScalar();
					}
					totalRowsPurged += rowsAffectedOnThisCommand;
					manager.CommitTransaction();
				}
				++totalBatchCount;
			} while (rowsAffectedOnThisCommand > 0 && (MaximumBatchCount <= 0 || totalBatchCount < MaximumBatchCount));

			return totalRowsPurged;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand GetDeleteCommand(int daysToKeep, string additionalWhereClause)
		{
			var oldestTimeToKeep = GetOldestTimeToKeep(daysToKeep);

			var deleteSql = string.Format(Culture.Invariant, @"
				DELETE TOP ({3})
					{0} WITH (READPAST, ROWLOCK, READCOMMITTEDLOCK)
				FROM {0}
				WHERE {1} < @OldestTimeToKeep AND ({2})
				SELECT @@RowCount", MailDBItemsTableName, MI_ReceivedDateTime, additionalWhereClause, MaximumBatchSize);

			var command = Db.Connection.Command(deleteSql);
			command.AddParameter("@OldestTimeToKeep", SqlDbType.DateTime, oldestTimeToKeep);
			return command;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected DbCommand GetUpdateCommand(int daysToKeep, string setClause, string additionalWhereClause)
		{
			var oldestTimeToKeep = GetOldestTimeToKeep(daysToKeep);

			var updateSql = string.Format(@"
				UPDATE TOP ({4}) {0}
				{1}
				WHERE {2} < @OldestTimeToKeep AND ({3})
				SELECT @@RowCount", MailDBItemsTableName, setClause, MI_SendDateTime, additionalWhereClause, MaximumBatchSize);

			var command = Db.Connection.Command(updateSql);
			command.AddParameter("@OldestTimeToKeep", SqlDbType.DateTime, oldestTimeToKeep);
			return command;
		}

		DateTime GetOldestTimeToKeep(int daysToKeep)
		{
			return ZDateTime.UtcNow.AddDays(-daysToKeep).ToDateTime();
		}

		public virtual int StopUnsuccessfulUpgradeSending()
		{
			return 0;
		}

		public int MaximumBatchSize { get; set; } = 200;

		public int MaximumBatchCount { get; set; } = 5;

		const string MailDBItemsTableName = MailDBItemsSchema.Constants.TableName;
		const string MI_ReceivedDateTime = MailDBItemsSchema.Constants.MI_ReceivedDateTime;
		const string MI_SendDateTime = MailDBItemsSchema.Constants.MI_SendDateTime;
		const string MI_Status = MailDBItemsSchema.Constants.MI_Status;
		const string MI_Direction = MailDBItemsSchema.Constants.MI_Direction;
		const string Sent = MailStatus.Sent;
		const string Failed = MailStatus.Failed;
		const string Queued = MailStatus.Queued;
		const string QueuedWithAcknowledgement = MailStatus.QueuedWithAck;
		const string Processed = MailStatus.Processed;
		const string Receive = MailDirection.Receive;
		const string Transmit = MailDirection.Transmit;
	}
}
