using System;
using System.Text;
using System.Threading;
using CargoWise.Data;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.GraphEngine.ServiceTasks
{
	public abstract class ServiceTask : ServiceProviderImpl
	{
		#region Implementation

		public sealed override void RunTask(CancellationToken token)
		{
			try
			{
				RunCore(token);
			}
			catch (ApplicationException ex) when (ex.InnerException is TransactionException)
			{
				ServiceLogger?.Log(LogType.Information, ServiceTaskLogs.ServiceTaskShutdown(GetExceptionMessagesForLog(ex, token)));
			}
			catch (TransactionException ex)
			{
				ServiceLogger?.Log(LogType.Information, ServiceTaskLogs.ServiceTaskShutdown(GetExceptionMessagesForLog(ex, token)));
			}
			catch (Exception ex)
			{
				ServiceLogger?.Log(LogType.Information, ServiceTaskLogs.ServiceTaskShutdown(GetExceptionMessagesForLog(ex, token)));
				throw;
			}
		}

		string GetExceptionMessagesForLog(Exception ex, CancellationToken token)
		{
			var builder = new StringBuilder();
			var innerEx = ex;
			while (innerEx != null)
			{
				builder.AppendLine(innerEx.Message);
				innerEx = innerEx.InnerException;
			}

			return builder.ToString();
		}

		protected abstract void RunCore(CancellationToken token);

		protected void RunWithLogger(CancellationToken token, IProcessingManager processingManager)
		{
			var logger = processingManager.Logger;
			try
			{
				logger.OnLogInfoAdded += Logger_OnLogInfoAdded;
				processingManager.ExecuteBatch(token);
			}
			finally
			{
				logger.OnLogInfoAdded -= Logger_OnLogInfoAdded;
			}
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger?.Log(logType, log.Trim());
		}

		#endregion
	}
}
