using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.IO;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.RemotePrinting.Server.JobPrinting
{
	public class PrintServer
	{
		public void SetPrintQueues(string printServerName, IEnumerable<string> printQueueNames)
		{
			using var conn = NewConnection();
			SetPrintQueues(printServerName, printQueueNames, conn);
		}

		public void SetPrintQueues(string printServerName, IEnumerable<string> printQueueNames, DbConnection conn)
		{
			Argument.NotNull(printServerName, nameof(printServerName));
			Argument.NotNull(conn, nameof(conn));

			lock (printQueuesLock)
			{
				SetPrintQueuesCore(printServerName, printQueueNames.Select(name => new PrintQueueInfo { Name = name, IsSuspectedSurrogate = false }), conn);
			}
		}

		static readonly object printQueuesLock = new ();

		public void SetPrintQueuesEx(string printServerName, IEnumerable<PrintQueueInfo> printQueues)
		{
			using DbConnection conn = NewConnection();
			SetPrintQueuesEx(printServerName, printQueues, conn);
		}

		public void SetPrintQueuesEx(string printServerName, IEnumerable<PrintQueueInfo> printQueues, DbConnection conn)
		{
			Argument.NotNull(printServerName, nameof(printServerName));
			Argument.NotNull(conn, nameof(conn));

			lock (printQueuesLock)
			{
				SetPrintQueuesCore(printServerName, printQueues, conn);
			}
		}

		public void SetPrintJobSuccess(List<Guid> processedPrintJobPks)
		{
			Argument.NotNull(processedPrintJobPks, nameof(processedPrintJobPks)); // Suggested By ReviewBot 

			using var conn = NewConnection();
			SetPrintJobSuccessCore(processedPrintJobPks, conn);
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void SetPrintJobFailure(List<PrintJobFailed> failedPrintJobWithFailureReason)
		{
			using var conn = NewConnection();
			SetPrintJobFailureCore(failedPrintJobWithFailureReason, conn);
		}

		protected virtual DbConnection NewConnection()
		{
			return DbHelper.NewConnection();
		}

		protected virtual IEmailSender CreateEmailSender(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			var emailAddress = RegistryData.SMTPDefaultReturnEmailAddress(connection);

			return new EmailSender(connection, emailAddress);
		}

		public IAsyncResult BeginGetPrintJobs<T>(string printServerName, AsyncCallback cb) where T : ServerPrintJob, new()
		{
			Argument.NotNull(printServerName, nameof(printServerName));
			Argument.NotNull(cb, nameof(cb));

			PrintJobAsyncResult result;
			var jobs = GetPrintJobsCore<T>(printServerName);
			if (jobs.Count > 0)
			{
				result = PrintJobAsyncResult.Synchronous(jobs);
				cb.Invoke(result);
			}
			else
			{
				result = new PrintJobAsyncResult(printServerName);
				var waiter = new JobWaiter(cb, result);
				PrintJobPoller.Instance.BeginWaitForJobs(waiter);
			}
			return result;
		}

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public List<T> EndGetPrintJobs<T>(IAsyncResult call) where T : ServerPrintJob, new()
		{
			Argument.NotNull(call, nameof(call)); // Suggested By ReviewBot 
			if (!(call.AsyncState is not bool || ((PrintJobAsyncResult)call).ServerName != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(call));
			}

			List<T> result;
			if (call.AsyncState is List<T> jobs)
			{
				result = jobs;
			}
			else if (call.AsyncState is true)
			{
				result = GetPrintJobsCore<T>(((PrintJobAsyncResult)call).ServerName);
			}
			else
			{
				result = new List<T>();
			}

			return result;
		}

		public List<T> GetPrintJobs<T>(string printServerName) where T : ServerPrintJob, new()
		{
			var jobs = GetPrintJobsCore<T>(printServerName);
			return jobs;
		}

		public List<ServerPrintQueue> GetChangedPrintQueues(string serverName, List<string> changedQueueNames)
		{
			Argument.NotNull(serverName, nameof(serverName));
			Argument.NotNull(changedQueueNames, nameof(changedQueueNames));

			using var conn = NewConnection();
			var result = GetChangedPrintQueuesCore(serverName, changedQueueNames, conn);

			return result;
		}

		public ServerWatermark GetWatermark()
		{
			using var conn = NewConnection();
			var result = GetWatermarkCore(conn);

			return result;
		}

		protected void SetPrintQueuesCore(string printServerName, IEnumerable<PrintQueueInfo> printQueues, DbConnection conn)
		{
			Argument.NotNull(printServerName, nameof(printServerName));
			Argument.NotNull(conn, nameof(conn)); // Suggested By ReviewBot 

			conn.RunTransactioned(() =>
				{
					UpdateExistingPrintQueuesDeleteStatus(printServerName, printQueues, conn);
					UpdateSuspectedSurrogateQueuesAllowPrintingStatus(printServerName, printQueues, conn);
					CreateNewPrintQueues(printServerName, printQueues, conn);
				}
			);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sql query string")]
		protected void UpdateExistingPrintQueuesDeleteStatus(string printServerName, IEnumerable<PrintQueueInfo> printQueues, DbConnection conn)
		{
			Argument.NotNull(printServerName, nameof(printServerName));
			Argument.NotNull(conn, nameof(conn)); // Suggested By ReviewBot 
			Argument.NotNull(printQueues, nameof(printQueues)); // Suggested By ReviewBot

			var logForOfflineSqlText = @"
				INSERT INTO dbo.StmALog (SL_Table, SL_Parent, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_GS_NKUser)
					SELECT 'StmPrintQueue', SQ_PK, SUBSTRING(@OfflineLog, 0, 1023), 'EDT', SYSUTCDATETIME(), 'ZZ'
					FROM dbo.StmPrintQueue
					LEFT JOIN dbo.StmPrintServer ON SPS_PK = SQ_SPS_Server
					WHERE
						SPS_ServerName = @ServerName
						AND SQ_QueueDeleted is null";

			var sqlText = @"
				UPDATE dbo.StmPrintQueue
				SET
					SQ_QueueDeleted = SYSUTCDATETIME(),
					SQ_SystemLastEditTimeUtc = SYSUTCDATETIME(),
					SQ_SystemLastEditUser = 'ZZ'
				FROM dbo.StmPrintQueue
				LEFT JOIN dbo.StmPrintServer ON SPS_PK = SQ_SPS_Server
				WHERE
					SPS_ServerName = @ServerName
					AND SQ_QueueDeleted is null";

			if (printQueues.Any())
			{
				logForOfflineSqlText += @"
					AND SQ_QueueName not in (select value from @TVP)
				";

				sqlText += @"
					AND SQ_QueueName not in (select value from @TVP)

				IF (@@error != 0) RETURN

				INSERT INTO dbo.StmALog (SL_Table, SL_Parent, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_GS_NKUser)
					SELECT 'StmPrintQueue', SQ_PK, SUBSTRING(@OnlineLog, 0, 1023), 'EDT', SYSUTCDATETIME(), 'ZZ'
					FROM dbo.StmPrintQueue
					LEFT JOIN dbo.StmPrintServer ON SPS_PK = SQ_SPS_Server
					WHERE
						SPS_ServerName = @ServerName
						AND SQ_QueueDeleted is not null
						AND SQ_QueueName in (select value from @TVP)

				UPDATE dbo.StmPrintQueue
				SET
					SQ_QueueDeleted = null,
					SQ_SystemLastEditTimeUtc = SYSUTCDATETIME(),
					SQ_SystemLastEditUser = 'ZZ'
				FROM dbo.StmPrintQueue
				LEFT JOIN dbo.StmPrintServer ON SPS_PK = SQ_SPS_Server
				WHERE
					SPS_ServerName = @ServerName
					AND SQ_QueueDeleted is not null
					AND SQ_QueueName in (select value from @TVP)
				";
			}

			sqlText = logForOfflineSqlText + sqlText;

			using (var cmd = conn.Command(sqlText))
			{
				var queueNames = printQueues.Select(queue => queue.Name);
				cmd.AddParameter("@OfflineLog", SqlDbType.NVarChar, string.Format("Set it to offline with printers {0}", string.Join(",", queueNames)));
				cmd.AddParameter("@OnlineLog", SqlDbType.NVarChar, string.Format("Set it to online with printers {0}", string.Join(",", queueNames)));
				cmd.AddParameterBasedOnDbColumn("@ServerName", printServerName, StmPrintServerSchema.SPS_ServerName);
				cmd.AddTableValuedParameter("@TVP", StmPrintQueueSchema.SQ_QueueName, queueNames);
				cmd.ExecuteNonQuery();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected void UpdateSuspectedSurrogateQueuesAllowPrintingStatus(string printServerName, IEnumerable<PrintQueueInfo> printQueues, DbConnection conn)
		{
			Argument.NotNull(printServerName, nameof(printServerName));
			Argument.NotNull(printQueues, nameof(printQueues));
			Argument.NotNull(conn, nameof(conn));

			var queuesToUpdate = printQueues.Where(queue => queue.IsSuspectedSurrogate).ToArray();
			if (!queuesToUpdate.Any())
			{
				return;
			}

			const string sqlText = @"
				UPDATE dbo.StmPrintQueue
				SET
					SQ_AllowPrinting = 0,
					SQ_SystemLastEditTimeUtc = SYSUTCDATETIME(),
					SQ_SystemLastEditUser = 'ZZ'
				FROM dbo.StmPrintQueue
				LEFT JOIN dbo.StmPrintServer ON SPS_PK = SQ_SPS_Server
				WHERE SQ_AllowPrinting = 1
					AND SPS_ServerName = @ServerName
					AND SQ_QueueName in (select value from @TVP)
					AND not exists (
						select 1
							from dbo.StmALog
						where SL_Parent = SQ_PK
							and SL_SE_NKEvent = 'EDT'
							and SL_GS_NKUser != '~BP'
					)";

			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameterBasedOnDbColumn("@ServerName", printServerName, StmPrintServerSchema.SPS_ServerName);
				cmd.AddTableValuedParameter("@TVP", StmPrintQueueSchema.SQ_QueueName, queuesToUpdate.Select(queue => queue.Name));
				cmd.ExecuteNonQuery();
			}
		}

		protected void CreateNewPrintQueues(string printServerName, IEnumerable<PrintQueueInfo> printQueues, DbConnection conn)
		{
			try
			{
				CreateNewPrintQueuesCore(printServerName, printQueues, conn);
			}
			catch (SqlException)
			{
				// May fail to insert new records due to race condition - retry again
				CreateNewPrintQueuesCore(printServerName, printQueues, conn);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void CreateNewPrintQueuesCore(string printServerName, IEnumerable<PrintQueueInfo> printQueues, DbConnection conn)
		{
			Argument.NotNull(printServerName, nameof(printServerName));
			Argument.NotNull(printQueues, nameof(printQueues)); // Suggested By ReviewBot 

			if (printQueues.Any(x => x == null))
			{
				throw new ArgumentException("Invalid argument.", nameof(printQueues));
			}

			Argument.NotNull(conn, nameof(conn));

			if (printQueues.Any())
			{
				string sqlText = @"
					DECLARE @ServerPk UNIQUEIDENTIFIER;
					SET @ServerPk = (SELECT TOP(1) SPS_PK FROM dbo.StmPrintServer WHERE SPS_ServerName = @ServerName);

					IF @ServerPk IS NULL
					BEGIN
						SET @ServerPk = NEWID()
						INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName)
							VALUES (@ServerPk, @ServerName)
					END

					IF NOT EXISTS (SELECT null FROM dbo.StmPrintQueue WHERE SQ_SPS_Server = @ServerPk AND SQ_QueueName = @QueueName)
					BEGIN
						INSERT dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_QueueName, SQ_DisplayName, SQ_AllowPrinting, SQ_SystemCreateUser, SQ_SystemCreateTimeUtc, SQ_SystemLastEditUser, SQ_SystemLastEditTimeUtc)
							VALUES (@QueuePk, @ServerPk, @QueueName, @DisplayName, @AllowPrinting, 'ZZ', SYSUTCDATETIME(), 'ZZ', SYSUTCDATETIME())
					END";

				foreach (var queue in printQueues)
				{
					using (var cmd = conn.Command(sqlText))
					{
						cmd.AddParameterBasedOnDbColumn("@QueuePk", Guid.NewGuid(), StmPrintQueueSchema.PK);
						cmd.AddParameterBasedOnDbColumn("@ServerName", printServerName, StmPrintServerSchema.SPS_ServerName);
						cmd.AddParameterBasedOnDbColumn("@QueueName", queue.Name, StmPrintQueueSchema.SQ_QueueName);
						cmd.AddParameterBasedOnDbColumn("@DisplayName", GetDisplayNameFromQueueName(queue.Name), StmPrintQueueSchema.SQ_DisplayName);
						cmd.AddParameterBasedOnDbColumn("@AllowPrinting", !queue.IsSuspectedSurrogate, StmPrintQueueSchema.SQ_AllowPrinting);
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		/// <summary>
		/// Update StmPrintQueue.SQ_WebPrintServiceAddress with address of managing web service.
		/// </summary>
		/// <remarks>Only update print queue web service address from SignalR controller, not from web API.</remarks>
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public void UpdatePrintQueuesWebPrintServerAddress(string printServerName, IEnumerable<string> printQueueNames, string webPrintServiceAddress, string oldWebPrintServiceAddress, DbConnection conn)
		{
			var sqlText = @"
				UPDATE dbo.StmPrintQueue
				SET
					SQ_WebPrintServiceAddress = @WebPrintServiceAddress,
					SQ_SystemLastEditTimeUtc = SYSUTCDATETIME(),
					SQ_SystemLastEditUser = 'ZZ'
				FROM dbo.StmPrintQueue
				LEFT JOIN dbo.StmPrintServer ON SPS_PK = SQ_SPS_Server
				WHERE SPS_ServerName = @ServerName
					AND SQ_QueueName in (select value from @TVP)";

			if (!string.IsNullOrEmpty(oldWebPrintServiceAddress))
			{
				sqlText += @"
					AND SQ_WebPrintServiceAddress = @OldWebPrintServiceAddress";
			}

			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameterBasedOnDbColumn("@WebPrintServiceAddress", webPrintServiceAddress, StmPrintQueueSchema.SQ_WebPrintServiceAddress);
				cmd.AddParameterBasedOnDbColumn("@ServerName", printServerName, StmPrintServerSchema.SPS_ServerName);
				cmd.AddTableValuedParameter("@TVP", StmPrintQueueSchema.SQ_QueueName, printQueueNames);

				if (!string.IsNullOrEmpty(oldWebPrintServiceAddress))
				{
					cmd.AddParameterBasedOnDbColumn("@OldWebPrintServiceAddress", oldWebPrintServiceAddress, StmPrintQueueSchema.SQ_WebPrintServiceAddress);
				}

				cmd.ExecuteNonQuery();
			}
		}

		protected void SetPrintJobSuccessCore(List<Guid> processedPrintJobPks, DbConnection conn)
		{
			Argument.NotNull(processedPrintJobPks, nameof(processedPrintJobPks));
			Argument.NotNull(conn, nameof(conn));

			if (processedPrintJobPks.Count > 0)
			{
				foreach (var successfulJobPk in processedPrintJobPks)
				{
					UpdateJobStatus(conn, successfulJobPk, string.Empty, true);
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void SetPrintJobFailureCore(List<PrintJobFailed> failedPrintJobWithFailureReason, DbConnection conn)
		{
			Argument.NotNull(conn, nameof(conn));
			// failedPrintJobWithFailureReason can be null when this method is called by older Web Print clients
			if (failedPrintJobWithFailureReason?.Count > 0)
			{
				List<FailedJob> failedJobsWithNoRetries;
				var failedJobsPks = new List<Guid>();

				foreach (var failedJob in failedPrintJobWithFailureReason)
				{
					if (failedJob != null && !failedJobsPks.Contains(failedJob.JobPk))
					{
						UpdateJobStatus(conn, failedJob.JobPk, failedJob.FailureReason, false);
						failedJobsPks.Add(failedJob.JobPk);
					}
				}

				using (var dataTable = GetAsDataTable(failedJobsPks))
				{
					failedJobsWithNoRetries = GetFailedJobsWithNoMoreRetries(conn, dataTable).ToList();
				}

				if (failedJobsWithNoRetries.Count > 0)
				{
					NotifyOfFailedJobs(conn, failedJobsWithNoRetries);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void NotifyOfFailedJobs(DbConnection connection, IEnumerable<FailedJob> failedJobs)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(failedJobs, nameof(failedJobs));

			var emailSender = CreateEmailSender(connection);

			var jobsToEmailPostmastersAbout = new List<FailedJob>();
			foreach (var failedJob in failedJobs)
			{
				bool emailWasSent = false;
				if (!string.IsNullOrEmpty(failedJob.SP_GS_NKJobSubmittedBy))
				{
					emailWasSent = emailSender.SendEmailToUser(failedJob.SP_GS_NKJobSubmittedBy, "Print Job Failed", GetJobDetail(failedJob));
				}

				if (!emailWasSent)
				{
					jobsToEmailPostmastersAbout.Add(failedJob);
				}
			}

			if (jobsToEmailPostmastersAbout.Count > 0)
			{
				var emailBody = new StringBuilder("Failed to print the following documents:");
				foreach (var job in jobsToEmailPostmastersAbout)
				{
					emailBody.AppendFormat(CultureInfo.InvariantCulture, "\r\n\t*" + GetJobDetail(job));
				}

				var postmastersGroupPk = RegistryData.WebPrintNotificationGroup(connection);
				emailSender.SendEmailToGroup(postmastersGroupPk, "Print Job(s) Failed", emailBody.ToString());
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static string GetJobDetail(FailedJob failedJob)
		{
			return string.Format(CultureInfo.CurrentCulture, @"Failed to print document '{0}' on printer '{1}' after three attempts:

Error Message: {2}

Job Type: [{3}]
Document Name: [{4}]
No. of Copies: [{5}]
Email/Fax Destination: [{6}]
Email Subject: [{7}]
Email Attachments: [{8}]
Parent Table: [{9}]", failedJob.SP_DocumentName, failedJob.SQ_DisplayName, failedJob.SP_FailureReason, failedJob.SP_JobType, failedJob.SP_DocumentName, failedJob.SP_Copies, failedJob.SP_FaxDestination, failedJob.SP_EmailSubjectLine, failedJob.SP_EmailAttachments, failedJob.SP_ParentTableName);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static void UpdateJobStatus(DbConnection conn, Guid jobPk, string failureReason, bool markAsSuccessful)
		{
			Argument.NotNull(conn, nameof(conn));
			Argument.NotNull(jobPk, nameof(jobPk)); // Suggested By ReviewBot 

			if (string.IsNullOrEmpty(failureReason))
			{
				failureReason = "Failed printing in WebPrint.";
			}

			var hasFailureReason = !string.IsNullOrEmpty(failureReason);

			var commandText = new StringBuilder();
			commandText.AppendLine("UPDATE dbo.StmPrintJob");

			if (markAsSuccessful)
			{
				commandText.AppendLine(@"
SET SP_RetryAttempts = 0, SP_JobType = 'PRS'");
			}
			else
			{
				commandText.AppendLine(@"
SET SP_RetryAttempts =
CASE
	WHEN SP_RetryAttempts = 0 THEN 1 -- If retry counter was not increased when sending job to print server (e.g. if it was sent directly via SignalR)
	ELSE SP_RetryAttempts
END");

				if (hasFailureReason)
				{
					commandText.AppendLine(@",
SP_FailureReason =
CASE
	WHEN SP_RetryAttempts >= 3 THEN SUBSTRING(@FailureReason, 1, 500)
	ELSE SP_FailureReason
END");
				}

				commandText.AppendLine(@",
SP_Status =
CASE
	WHEN SP_RetryAttempts >= 3 THEN 'FAL' -- SP_RetryAttempts should be already increased in GetPrintJobsCore(), so compare with max value 3
	ELSE 'QUE'
END");
			}

			commandText.AppendLine("WHERE SP_PK = @JobPk");

			conn.RunTransactioned(delegate
			{
				using (var cmd = conn.Command(commandText.ToString()))
				{
					if (hasFailureReason)
					{
						cmd.AddParameter("@FailureReason", SqlDbType.NVarChar, failureReason);
					}
					cmd.AddParameter("@JobPk", SqlDbType.UniqueIdentifier, jobPk);
					cmd.ExecuteNonQuery();
				}
			});
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static void UpdateJobRetryCounter(DbConnection conn, Guid jobPk)
		{
			Argument.NotNull(conn, nameof(conn));
			Argument.NotNull(jobPk, nameof(jobPk)); // Suggested By ReviewBot 

			const string commandText = @"
UPDATE dbo.StmPrintJob
SET SP_RetryAttempts = 
CASE 
	WHEN SP_RetryAttempts >= 3 THEN SP_RetryAttempts
	ELSE SP_RetryAttempts + 1
END
WHERE SP_PK = @JobPk";

			conn.RunTransactioned(delegate
			{
				using (var cmd = conn.Command(commandText))
				{
					cmd.AddParameter("@JobPk", SqlDbType.UniqueIdentifier, jobPk);
					cmd.ExecuteNonQuery();
				}
			});
		}

		struct FailedJob
		{
			public string SP_JobType;
			public string SP_DocumentName;
			public string SP_Copies;
			public string SP_FaxDestination;
			public string SP_EmailSubjectLine;
			public string SP_EmailAttachments;
			public string SP_ParentTableName;
			public string SP_GS_NKJobSubmittedBy;
			public string SP_FailureReason;
			public string SQ_DisplayName;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static IEnumerable<FailedJob> GetFailedJobsWithNoMoreRetries(DbConnection conn, DataTable failedJobsList)
		{
			const string failedJobsQuery = "EXEC dbo.GetFailedPrintJobForRemotePrinting @FailedJobs";

			using (var cmd = conn.Command(failedJobsQuery))
			{
				cmd.AddTableValuedParameter("@FailedJobs", TVPHelper.TVP_uniqueidentifier, failedJobsList);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return new FailedJob
						{
							SP_JobType = reader["SP_JobType"].ToString(),
							SP_DocumentName = reader["SP_DocumentName"].ToString(),
							SP_Copies = reader["SP_Copies"].ToString(),
							SP_FaxDestination = reader["SP_FaxDestination"].ToString(),
							SP_EmailSubjectLine = reader["SP_EmailSubjectLine"].ToString(),
							SP_EmailAttachments = reader["SP_EmailAttachments"].ToString(),
							SP_ParentTableName = reader["SP_ParentTableName"].ToString(),
							SP_GS_NKJobSubmittedBy = reader["SP_GS_NKJobSubmittedBy"].ToString(),
							SP_FailureReason = reader["SP_FailureReason"].ToString(),
							SQ_DisplayName = reader["SQ_DisplayName"].ToString()
						};
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant String")]
		DataTable GetAsDataTable(List<Guid> items)
		{
			Argument.NotNull(items, nameof(items));

			const string columnName = "Value";

			DataTable table = null;
			try
			{
				table = new DataTable();
				table.Locale = CultureInfo.InvariantCulture;

				table.Columns.Add(columnName, typeof(Guid));
				foreach (var item in items)
				{
					table.Rows.Add(new object[] { item });
				}

				return table;
			}
			catch
			{
				table?.Dispose();
				throw;
			}
		}

		List<T> GetPrintJobsCore<T>(string printServerName) where T : ServerPrintJob, new()
		{
			Argument.NotNull(printServerName, nameof(printServerName));

			using var conn = NewConnection();
			return GetPrintJobsCore<T>(printServerName, conn);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected List<T> GetPrintJobsCore<T>(string printServerName, DbConnection conn) where T : ServerPrintJob, new()
		{
			Argument.NotNull(printServerName, nameof(printServerName));
			Argument.NotNull(conn, nameof(conn)); // Suggested By ReviewBot 

			const string sqlText = "EXEC dbo.GetNextPrintJobForRemotePrinting @ServerName";

			var result = new List<T>();

			var maxBatchSize = GetMaxBatchSize(conn);
			var batchSize = 0;

			using (var command = conn.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@ServerName", printServerName, StmPrintServerSchema.SPS_ServerName);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						T jobDetail = new T();
						var jobDetailEx = jobDetail as ServerPrintJobEx;

						var jobPKObj = reader["SP_PK"];
						jobDetail.JobPk = (Guid)jobPKObj;

						var emailAttachmentsObj = reader["SP_EmailAttachments"];
						var safeBlobFileName = PathValidation.GetSafeFilename(emailAttachmentsObj.ToString());
						jobDetail.BlobType = Path.GetExtension(safeBlobFileName).Trim('.');

						var emailSubjectLineObj = reader["SP_EmailSubjectLine"];
						jobDetail.EmailSubjectLine = emailSubjectLineObj.ToString();

						var watermarkImageObj = reader["SP_WatermarkImage"];
						var watermarkTextObj = reader["SP_WatermarkText"];
						jobDetail.HasWatermark = watermarkImageObj != DBNull.Value || !string.IsNullOrEmpty(watermarkTextObj.ToString());

						if (jobDetailEx != null)
						{
							var faxDestination = reader["SP_FaxDestination"];
							jobDetailEx.EmailFaxDestination = faxDestination.ToString();
						}

						var copiesObj = reader["SP_Copies"];
						if (copiesObj != DBNull.Value)
						{
							jobDetail.Copies = (short)copiesObj;
						}

						var queueNameObj = reader["SQ_QueueName"];
						if (queueNameObj != DBNull.Value)
						{
							jobDetail.QueueName = queueNameObj.ToString();
						}

						var queueStateChangedStampObj = reader["SQ_PrintQueueStateChanged"];
						if (queueStateChangedStampObj != DBNull.Value)
						{
							jobDetail.QueueStateChangedStamp = (Guid)queueStateChangedStampObj;
						}

						var customPropertiesObj = reader["SP_CustomProperties"];
						if (customPropertiesObj == DBNull.Value)
						{
							jobDetail.Contents = Array.Empty<byte>();
						}
						else if (jobDetailEx != null) // Should be compressed
						{
							jobDetail.Contents = (byte[])customPropertiesObj;
						}
						else
						{
							var customPropertiesBytes = (byte[])customPropertiesObj;
							jobDetail.Contents = GetUncompressedByteArray(customPropertiesBytes);
						}

						var escapeSequenceObj = reader["SP_EscapeSequence"];
						if (escapeSequenceObj == DBNull.Value)
						{
							jobDetail.EscapeSequence = Array.Empty<byte>();
						}
						else if (jobDetailEx != null) // Should be compressed
						{
							jobDetail.EscapeSequence = (byte[])escapeSequenceObj;
						}
						else
						{
							var escapeSequenceObjBytes = (byte[])escapeSequenceObj;
							jobDetail.EscapeSequence = GetUncompressedByteArray(escapeSequenceObjBytes);
						}

						if (maxBatchSize > 0)
						{
							batchSize += jobDetail.Contents.Length;
							if (batchSize > maxBatchSize // Current job will exceed max batch size
								&& result.Count > 0 // Should have at least one job in the batch
								)
							{
								break;
							}
						}

						result.Add(jobDetail);
					}
				}
			}

			foreach (var job in result.ToArray())
			{
				UpdateJobRetryCounter(conn, job.JobPk);
				if (JobHasChangedFromQueOrPrn(conn, job.JobPk))
				{
					result.Remove(job);
				}
			}

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual bool JobHasChangedFromQueOrPrn(DbConnection conn, Guid jobPk)
		{
			Argument.NotNull(conn, nameof(conn));
			Argument.NotNull(jobPk, nameof(jobPk)); // Suggested By ReviewBot

			const string commandText = @"
SELECT COUNT(SP_PK) FROM dbo.StmPrintJob
WHERE SP_PK = @JobPk AND SP_Status = 'QUE' AND SP_JobType = 'PRN'";
			var hasChanged = false;

			conn.RunTransactioned(delegate
			{
				using var cmd = conn.Command(commandText);
				cmd.AddParameter("@JobPk", SqlDbType.UniqueIdentifier, jobPk);
				hasChanged = (int)cmd.ExecuteScalar() == 0;
			});

			return hasChanged;
		}

		protected virtual int GetMaxBatchSize(DbConnection conn)
		{
			const int Megabyte = 1024 * 1024;
			return RegistryData.WebPrintDocumentPackMaxSize(conn) * Megabyte;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected List<ServerPrintQueue> GetChangedPrintQueuesCore(string serverName, List<string> changedQueueNames, DbConnection conn)
		{
			Argument.NotNull(serverName, nameof(serverName));
			Argument.NotNull(conn, nameof(conn));
			Argument.NotNull(changedQueueNames, nameof(changedQueueNames));

			var result = new List<ServerPrintQueue>();

			if (changedQueueNames.Count > 0)
			{
				string sqlText = "SELECT * FROM dbo.StmPrintQueue LEFT JOIN dbo.StmPrintServer ON SPS_PK = SQ_SPS_Server WHERE SPS_ServerName = @ServerName AND SQ_QueueName in (select VALUE from @TVP)";

				using (var cmd = conn.Command(sqlText))
				{
					cmd.AddParameterBasedOnDbColumn("@ServerName", serverName, StmPrintServerSchema.SPS_ServerName);
					cmd.AddTableValuedParameter("@TVP", StmPrintQueueSchema.SQ_QueueName, changedQueueNames);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var queueDetail = new ServerPrintQueue();

							var jobPKObj = reader["SQ_PK"];
							queueDetail.QueuePk = (Guid)jobPKObj;

							var queueNameObj = reader["SQ_QueueName"];
							queueDetail.Name = queueNameObj.ToString();

							var displayNameObj = reader["SQ_DisplayName"];
							queueDetail.DisplayName = displayNameObj.ToString();

							var printLanguageObj = reader["SQ_PrintLanguage"];
							queueDetail.PrintLanguage = printLanguageObj.ToString();

							var supressLetterHeadObj = reader["SQ_SupressLetterhead"];
							queueDetail.SuppressLetterhead = (bool)supressLetterHeadObj;

							object leftMarginObj = reader["SQ_LeftMargin"];
							if (leftMarginObj != DBNull.Value)
							{
								queueDetail.LeftMargin = (short)leftMarginObj;
							}

							object topMarginObj = reader["SQ_TopMargin"];
							if (topMarginObj != DBNull.Value)
							{
								queueDetail.TopMargin = (short)topMarginObj;
							}

							object scaleObj = reader["SQ_Scale"];
							if (scaleObj != DBNull.Value)
							{
								queueDetail.Scale = (decimal)scaleObj;
							}

							object rowScaleObj = reader["SQ_RowScale"];
							if (rowScaleObj != DBNull.Value)
							{
								queueDetail.RowScale = (decimal)rowScaleObj;
							}

							object columnScaleObj = reader["SQ_ColumnScale"];
							if (columnScaleObj != DBNull.Value)
							{
								queueDetail.ColumnScale = (decimal)columnScaleObj;
							}

							object xlsTemplateObj = reader["SQ_XLSTemplateForPrintSettings"];
							if (xlsTemplateObj == DBNull.Value)
							{
								queueDetail.XlsTemplate = Array.Empty<byte>();
							}
							else
							{
								var xlsTemplateObjBytes = (byte[])xlsTemplateObj;
								queueDetail.XlsTemplate = GetUncompressedByteArray(xlsTemplateObjBytes);
							}

							object stateChangedStampObj = reader["SQ_PrintQueueStateChanged"];
							if (stateChangedStampObj != DBNull.Value)
							{
								queueDetail.StateChangedStamp = (Guid)stateChangedStampObj;
							}

							object isRollPaper = reader["SQ_IsRollPaper"];
							if (isRollPaper != DBNull.Value)
							{
								queueDetail.IsRollPaper = (bool)isRollPaper;
							}

							result.Add(queueDetail);
						}
					}
				}
			}

			return result;
		}

		protected ServerWatermark GetWatermarkCore(DbConnection conn)
		{
			Argument.NotNull(conn, nameof(conn)); // Suggested By ReviewBot 

			string sqlText = string.Format(
				"SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = '{0}' AND SD_Owner is null AND SD_DepartmentGuid is null"
				, WatermarkRegistryItemName);

			object binaryValue = conn.ExecuteScalar(sqlText);
			byte[] binaryXml = binaryValue as byte[];

			if (binaryXml != null)
			{
				binaryXml = GetUncompressedByteArray(binaryXml);
			}

			ServerWatermark result = new ServerWatermark(binaryXml);

			return result;
		}

		string GetDisplayNameFromQueueName(string queueName)
		{
			Argument.NotNull(queueName, nameof(queueName));

			string result = queueName;
			int displayNameStart = result.LastIndexOf("\\") + 1;

			if (displayNameStart > 0 && displayNameStart < result.Length)
			{
				result = result.Substring(displayNameStart);
			}

			return result;
		}

		byte[] GetUncompressedByteArray(byte[] rawValue)
		{
			Argument.NotNull(rawValue, nameof(rawValue));

			return Compressor.Uncompress(rawValue);
		}

		public void SendNotificationEmail(string subject, string body)
		{
			using (DbConnection connection = NewConnection())
			{
				var postmastersGroupPk = RegistryData.WebPrintNotificationGroup(connection);
				var emailSender = CreateEmailSender(connection);
				emailSender.SendEmailToGroup(postmastersGroupPk, subject, body);
			}
		}

		public void SendLogsFilesEmail(string recipientEmail, string fileName, byte[] fileData, string comments)
		{
			using (DbConnection connection = NewConnection())
			{
				var emailSender = CreateEmailSender(connection);
				emailSender.SendLogsEmail(recipientEmail, fileData, fileName, comments);
			}
		}

		public const string WatermarkRegistryItemName = "DocumentWatermark";
	}
}
