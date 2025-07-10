using System;
using System.ComponentModel;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class DbTaskManager : IErrorReporter
	{
		public event InformationEvent OnTaskStarted;
		public event ProgressEvent OnSubtaskStarted;
		public event InformationEvent OnTaskCompleted;
		public event InformationEvent OnTaskFailed;
		public event InformationEvent OnShowInfoMessage;
		public ConfirmationPromptDelegate OnConfirmationPrompt;

		public ISynchronizeInvoke SyncInvoke
		{
			get { return fSyncInvoke; }
			set { fSyncInvoke = value; }
		}
		ISynchronizeInvoke fSyncInvoke;

		protected string GetDropAllStaffAssignedLoginsFromDbSql(string dbName)
		{
			return $@"
DECLARE @sqlCmd NVARCHAR(MAX) = N'';
SELECT @sqlCmd = @sqlCmd + N'DROP LOGIN ' + quotename(name) + N'; '
FROM 
	(
		SELECT name
		FROM sys.server_principals 
		WHERE name LIKE N'{DataUtils.ReplaceSqlLikeWildcard(DbUserRepository.GetStaffDbLoginFullPrefix(dbName))}%'
		UNION
		SELECT name
		FROM sys.server_principals 
		WHERE default_database_name = '{dbName}'
	) tbl
IF (@sqlCmd != '') EXEC (@sqlCmd);"
;
		}

		protected void FireOnTaskStarted(string message)
		{
			if (OnTaskStarted != null)
			{
				if (fSyncInvoke == null)
				{
					OnTaskStarted(message);
				}
				else
				{
					fSyncInvoke.BeginInvoke(new InformationEvent(OnTaskStarted), new object[] { message });
				}
			}

			LogMessage("Task Started", message);
		}

		protected void FireOnSubtaskStarted(string message, int stepNumber)
		{
			if (OnSubtaskStarted != null)
			{
				if (fSyncInvoke == null)
				{
					OnSubtaskStarted(message, stepNumber);
				}
				else
				{
					fSyncInvoke.BeginInvoke(new ProgressEvent(OnSubtaskStarted), new object[] { message, stepNumber });
				}
			}

			LogMessage("Subtask Started", message);
		}

		protected void FireOnTaskCompleted(string message)
		{
			if (OnTaskCompleted != null)
			{
				if (fSyncInvoke == null)
				{
					OnTaskCompleted(message);
				}
				else
				{
					fSyncInvoke.BeginInvoke(new InformationEvent(OnTaskCompleted), new object[] { message });
				}
			}

			LogMessage("Task Completed", message);
		}

		protected void FireOnTaskFailed(string errorMessage)
		{
			if (OnTaskFailed != null)
			{
				if (fSyncInvoke == null)
				{
					OnTaskFailed(errorMessage);
				}
				else
				{
					fSyncInvoke.BeginInvoke(new InformationEvent(OnTaskFailed), new object[] { errorMessage });
				}
			}

			LogMessage("Task Failed", errorMessage);
		}

		protected void FireOnShowInfoMessage(string message)
		{
			try
			{
				if (OnShowInfoMessage != null)
				{
					if (fSyncInvoke == null)
					{
						OnShowInfoMessage(message);
					}
					else
					{
						fSyncInvoke.BeginInvoke(new InformationEvent(OnShowInfoMessage), new object[] { message });
					}
				}
			}
			catch (Exception ex)
			{
				LogMessage("Error", $"{ex}");
			}

			LogMessage("Information", message);
		}

		void LogMessage(string eventType, string message)
		{
			Logger.Instance.LogMessage(string.Format(CultureInfo.InvariantCulture, "Event: {0}\t{1}", eventType, message));
		}

		void IErrorReporter.Clear() { }

		void IErrorReporter.Report(string key, string message, Exception exception)
		{
			ReportCore(key, message, exception);
		}

		void IErrorReporter.ReportDeveloperExceptionOrHandleSilently(string key, string message, Exception exception)
		{
			ReportCore(key, message, exception);
		}

		void ReportCore(string key, string message, Exception ex)
		{
			// Error being reported won't fail the task, so just show it as info
			FireOnShowInfoMessage($@"Error occurred: {message} with key: {key}.
Exception: {ex}
");
		}
	}
}
