using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.RemotePrinting.Server.RPSCore;

namespace Enterprise.RemotePrinting.Server.JobPrinting
{
	public class PrintJobPoller
	{
#if DEBUG
		protected
#endif
 PrintJobPoller()
		{
		}

		[System.Runtime.CompilerServices.SpecialName] // Using this so that Static Sniffer doesn't pick this up without us requiring a reference to Enterprise.Core.
		public static readonly PrintJobPoller Instance = new PrintJobPoller();

		readonly Dictionary<string, JobWaiter> jobWaiters = new Dictionary<string, JobWaiter>();
		volatile bool isPolling;
		readonly object isPollingLock = new object();
		const int timeBetweenChecksMs = 10000;

		public void BeginWaitForJobs(JobWaiter waiter)
		{
			Argument.NotNull(waiter, nameof(waiter)); // Suggested By ReviewBot 
			Argument.NotNull(waiter.ServerName, nameof(waiter.ServerName));

			AddJobWaiter(waiter);
			EnsureIsPollingForJobs();
		}

		void CancelWaitForJobs(JobWaiter waiter)
		{
			Argument.NotNull(waiter, nameof(waiter));
			Argument.NotNull(waiter.ServerName, nameof(waiter.ServerName));

			lock (jobWaiters)
			{
				if (jobWaiters.ContainsKey(waiter.ServerName) && Object.ReferenceEquals(jobWaiters[waiter.ServerName], waiter))
				{
					jobWaiters[waiter.ServerName].Signal(false);
					jobWaiters.Remove(waiter.ServerName);
				}
			}
		}

		void AddJobWaiter(JobWaiter waiter)
		{
			Argument.NotNull(waiter, nameof(waiter));
			Argument.NotNull(waiter.ServerName, nameof(waiter.ServerName));

			lock (jobWaiters)
			{
				if (jobWaiters.ContainsKey(waiter.ServerName))
				{
					var jobWaiter = jobWaiters[waiter.ServerName];
					CancelWaitForJobs(jobWaiter);
				}

				jobWaiters[waiter.ServerName] = waiter;
			}
		}

		void EnsureIsPollingForJobs()
		{
			if (!isPolling)
			{
				lock (isPollingLock)
				{
					if (!isPolling)
					{
						ThreadPool.QueueUserWorkItem(OnJobPollingThreadStart);
						isPolling = true;
					}
				}
			}
		}

		#region NOT ON THE MAIN THREAD

		void OnJobPollingThreadStart(object state)
		{
			try
			{
				while (true)
				{
					List<string> serverList = new List<string>();

					lock (jobWaiters)
					{
						foreach (string serverName in jobWaiters.Keys)
						{
							serverList.Add(serverName);
						}
					}

					if (serverList.Count == 0)
					{
						break;
					}

					CheckForJobs(serverList);
					CheckForTimeouts();
					Thread.Sleep(timeBetweenChecksMs);
				}
			}
			catch (Exception ex)
			{
				HandlePollingThreadException(ex);
			}
			finally
			{
				lock (isPollingLock)
				{
					isPolling = false;
				}
			}
		}

		/// <summary>
		/// NOT running in the main request thread.
		///   If it fails to connect to the DB, catches the exception and
		///   instead of rethrowing it straigh away, it sets the PollingException property of the JobWaiter instance of the request
		///   which send the proper error notification to the client.
		/// </summary>
		/// <param name="ex">Exception to Handle</param>
		void HandlePollingThreadException(Exception ex)
		{
			lock (jobWaiters)
			{
				if (ex is SqlException && RemotePrintingDbConnectionException.IsGeneralNetworkError(ex))
				{
					ex = RemotePrintingDbConnectionException.New((SqlException)ex);
				}

				foreach (JobWaiter waiter in jobWaiters.Values)
				{
					waiter.Signal(ex);
				}

				jobWaiters.Clear();
			}
		}

		protected virtual DbConnection NewConnection()
		{
			return DbHelper.NewConnection();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected void CheckForJobs(List<string> serverList)
		{
			Argument.NotNull(serverList, nameof(serverList)); // Suggested By ReviewBot 

			if (serverList.Count == 0)
			{
				return;
			}

			using (var dataTable = new DataTable())
			{
				dataTable.Locale = CultureInfo.InvariantCulture;
				dataTable.Columns.Add("Value", typeof(string));

				foreach (var item in serverList)
				{
					dataTable.Rows.Add(new object[] { item });
				}

				var commandText = @"DECLARE @CurrentUtc DateTime = SYSUTCDATETIME();
					SELECT DISTINCT SPS_ServerName
					FROM
						dbo.StmPrintJob WITH (READPAST, READCOMMITTEDLOCK)
						INNER JOIN dbo.StmDeliveryGroup WITH (READPAST, READCOMMITTEDLOCK) ON SB_PK = SP_SB_DeliveryGroup
						LEFT JOIN dbo.StmPrintQueue WITH (READPAST, READCOMMITTEDLOCK) ON SQ_PK = SP_SQ
						LEFT JOIN dbo.StmPrintServer WITH (READPAST, READCOMMITTEDLOCK) ON SPS_PK = SQ_SPS_Server
					WHERE 
						SP_JobType = 'PRN'
						AND SPS_ServerName IN (select Value from @ServerList)
						AND SQ_QueueDeleted IS NULL
						AND SP_RunDateTime < @CurrentUtc
						AND SP_RetryAttempts < 3
						AND SB_IsProcessed = 1;";

				using (var conn = NewConnection())
				using (var cmd = conn.Command(commandText))
				{
					cmd.AddTableValuedParameter("@ServerList", "TVP_varchar_128", dataTable);

					using (IDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							string serverName = reader.GetString(0);
							SignalJobWaiter(serverName);
						}
					}
				}
			}
		}

		void CheckForTimeouts()
		{
			lock (jobWaiters)
			{
				foreach (var jobWaiter in jobWaiters.Values)
				{
					if (jobWaiter.WaitTimeExpired)
					{
						CancelWaitForJobs(jobWaiter);
					}
				}
			}
		}

#if DEBUG
		protected virtual
#endif
 void SignalJobWaiter(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName)); // Suggested By ReviewBot 

			lock (jobWaiters)
			{
				if (jobWaiters.ContainsKey(serverName))
				{
					var jobWaiter = jobWaiters[serverName];
					jobWaiter.Signal(true);
					jobWaiters.Remove(serverName);
				}
			}
		}

		#endregion
	}
}
