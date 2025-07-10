using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public abstract class WorkflowBatchLoader<T> : IWorkflowBatchLoader where T : ServiceTaskFactoryProviderWrapper
	{
		public WorkflowBatchLoader(T factoryProvider, ZQuery query, string queryName, bool shouldUseSecondaryServerIfAllowed = false)
			: this(factoryProvider, () => (query, queryName), shouldUseSecondaryServerIfAllowed)
		{
		}

		public WorkflowBatchLoader(T factoryProvider, Func<(ZQuery query, string queryName)> queryProvider, bool shouldUseSecondaryServerIfAllowed = false)
		{
			Argument.NotNull(factoryProvider, nameof(factoryProvider));
			Argument.NotNull(queryProvider, nameof(queryProvider));

			FactoryProvider = factoryProvider;
			this.queryProvider = queryProvider;
			this.shouldUseSecondaryServerIfAllowed = shouldUseSecondaryServerIfAllowed;
		}

		#region Data Access

		protected T FactoryProvider { get; }
		readonly Func<(ZQuery query, string queryName)> queryProvider;

		IDbConnectionForReportingWrapper SecondaryServerConnectionWrapper => secondaryServerConnectionWrapper ?? (secondaryServerConnectionWrapper = GetSecondaryServerConnectionWrapper());
		IDbConnectionForReportingWrapper secondaryServerConnectionWrapper;

		IDbConnectionForReportingWrapper GetSecondaryServerConnectionWrapper()
		{
			return ShouldUseSecondaryServer ? SecondaryServerConnectionProviderProvider.GetProvider().GetNewConnectionWrapper() : null;
		}

		public BusinessObjectFactoryProvider GetFactoryProvider()
		{
			return ShouldUseSecondaryServer
				? FactoryProvider.GetSecondaryServerFactoryProvider(SecondaryServerConnectionWrapper.Connection)
				: FactoryProvider.InnerProvider;
		}

		readonly bool shouldUseSecondaryServerIfAllowed;

		bool ShouldUseSecondaryServer => shouldUseSecondaryServerIfAllowed && BMSRegistry.Instance.UseSecondaryServerForTransferRules.Value;

		FilteredBusinessObjectReaderWithLogger WorkflowReader => workflowReader ?? (workflowReader = GetWorkflowReader());
		FilteredBusinessObjectReaderWithLogger workflowReader;

		protected virtual FilteredBusinessObjectReaderWithLogger GetWorkflowReader()
		{
			var factoryProvider = GetFactoryProvider();
			var (query, queryName) = queryProvider.Invoke();
			var batchSize = GetWorkflowBatchSize();

			using (Db.DisposableActionForDbConnection())
			{
				return new FilteredBusinessObjectReaderWithLogger(factoryProvider, query, typeof(ProcessHeader), BatchLogger) { BatchSize = batchSize, QueryName = queryName };
			}
		}

		#endregion

		#region Batch Size

		public int GetWorkflowBatchSize() => GetWorkflowBatchSizeCore();

		protected abstract int GetWorkflowBatchSizeCore();

		#endregion

		#region Batch Logging

		BatchLogger BatchLogger => batchLogger ?? (batchLogger = GetBatchLogger());
		BatchLogger batchLogger;

		BatchLogger GetBatchLogger()
		{
			return ShouldLogPerformanceStats ? GetBatchLoggerCore() : null;
		}

		protected abstract BatchLogger GetBatchLoggerCore();

		bool ShouldLogPerformanceStats => BMSRegistry.Instance.LogWorkflowLoadTime.Value;

		void FlushBatchLogger()
		{
			batchLogger?.Flush();
		}

		#endregion

		#region IWorkflowBatchLoader Members

		public IEnumerable<ITransferrableProcessHeader> LoadNextBatch()
		{
			try
			{
				return LoadNextBatchCore();
			}
			catch (SqlException ex)
			{
				if (ex.Number == -2) // timeout expired
				{
					LogAndEmailBMSNotificationGroupAboutSQLException(ex);
					return Enumerable.Empty<ITransferrableProcessHeader>();
				}

				throw;
			}
		}

		IEnumerable<ITransferrableProcessHeader> LoadNextBatchCore()
		{
			var workflowBatch = WorkflowReader.LoadNextBatchInANewFactory((BusinessObject)lastProcessHeaderRead).Cast<ITransferrableProcessHeader>().ToArray();

#if DEBUG
			NotifyBatchLoaded_ForTest(ref workflowBatch, ref lastProcessHeaderRead);
#endif

			if (lastProcessHeaderRead == null || !workflowBatch.Contains(lastProcessHeaderRead))
			{
				lastProcessHeaderRead = workflowBatch.LastOrDefault();
			}

			return workflowBatch;
		}

		ITransferrableProcessHeader lastProcessHeaderRead;

		public string LastStatementExecuted => WorkflowReader.LastStatementExecuted;

		#endregion

		#region SQL Exception Processing

		void LogAndEmailBMSNotificationGroupAboutSQLException(SqlException ex)
		{
			var emailIntro = GetEmailIntroForNotificationGroupAboutSQLException();
			var logHeader = GetLogHeaderForNotificationGroupAboutSQLException();
			LogAndEmailBMSNotificationGroupAboutSQLException(ex, emailIntro, logHeader, LastStatementExecuted);
		}

		protected abstract string GetEmailIntroForNotificationGroupAboutSQLException();
		protected abstract string GetLogHeaderForNotificationGroupAboutSQLException();

		void LogAndEmailBMSNotificationGroupAboutSQLException(SqlException ex, string emailIntro, string logHeader, string executedStatement = null)
		{
			var subject = Res.GetString("33852930-dc21-473d-b6c9-5c60e1ad96aa", "An exception has occurred whilst running a task");

			ZStringBuilder sb = new ZStringBuilder((NoResString)"The following SQL Exception(s) were caught:"); // Error description
			for (int i = 0; i < ex.Errors.Count; i++)
			{
				sb.Append((NoResString)"Index #" + i); // Error description
				sb.Append((NoResString)"Message: " + ex.Errors[i].Message); // Error description
				sb.Append((NoResString)"Error Number: " + ex.Errors[i].Number); // Error description
				sb.Append((NoResString)"Line Number: " + ex.Errors[i].LineNumber); // Error description
				sb.Append((NoResString)"Source: " + ex.Errors[i].Source); // Error description
				sb.Append((NoResString)"Procedure: " + ex.Errors[i].Procedure + (NoResString)"\n"); // Error description
			}

			if (!string.IsNullOrEmpty(executedStatement))
			{
				sb.Append((NoResString)"Statement:"); // Error description
				sb.Append(executedStatement);
			}

			var message = DbCommand.SanitizeExecuteAsReaderFlags(sb.ToStringWithNewLineBetweenAppends());
			var body = string.Format(CultureInfo.InvariantCulture, @"{0}: 

{1}", emailIntro, message);

			new BMSEmailDef(subject, body).Send();

			BatchLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, @"{0}: 

{1}", logHeader, message)); // Error description
		}

		#endregion

		#region Disposable

		bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					secondaryServerConnectionWrapper?.Dispose();
					FlushBatchLogger();
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			System.GC.SuppressFinalize(this);
		}

		#endregion

		#region For Tests
#if DEBUG

		protected virtual void NotifyBatchLoaded_ForTest(ref ITransferrableProcessHeader[] workflowBatch, ref ITransferrableProcessHeader lastProcessHeaderRead)
		{
		}

#endif
		#endregion
	}
}
