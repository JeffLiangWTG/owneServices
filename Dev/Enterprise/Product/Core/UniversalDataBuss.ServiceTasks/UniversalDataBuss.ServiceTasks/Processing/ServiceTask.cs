using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Data;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.UniversalDataBuss.ServiceTasks
{
	public abstract class ServiceTask : ServiceProviderImpl
	{
		#region New Members

		public abstract IEnumerable<string> SupportedMessageSubtypes { get; }
		public abstract string MasterServiceTaskCode { get; }
		public abstract string CurrentServiceTaskCode { get; }
		public virtual GrEngineServiceSetting ServiceSetting => GrEngineServiceSetting.Disabled;
		public virtual IEnumerable<string> ExcludedMessageSubtypes => Enumerable.Empty<string>();

		#endregion

		#region Implementation

		public override sealed void RunTask(CancellationToken token)
		{
			try
			{
				var grEngineEnabled = IsGrEngineEnabled;
				var serviceSetting = ServiceSetting;
				if (grEngineEnabled && serviceSetting == GrEngineServiceSetting.NonGrengineOnly)
				{
					// This service task is disabled whilst UMI is using parallelisation turned on. Messages processed by this service will be processed by UMK, UMI and UMQ service tasks.
				}
				else if (grEngineEnabled || !serviceSetting.IsWorker())
				{
					RunCore(grEngineEnabled ? serviceSetting : GrEngineServiceSetting.Disabled, token);
				}
			}
			catch (ApplicationException ex) when (ex.InnerException is TransactionException)
			{
				ServiceLogger?.Log(LogType.Information, ServiceTaskLogs.ServiceTaskShutdown(GetExceptionMessagesForLog(ex, token)));
			}
			catch (TransactionException ex)
			{
				ServiceLogger?.Log(LogType.Information, ServiceTaskLogs.ServiceTaskShutdown(GetExceptionMessagesForLog(ex, token)));
			}
			catch (System.Data.Common.DbException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.LockTimeoutExpired)
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

		void RunCore(GrEngineServiceSetting grengineSetting, CancellationToken token)
		{
			using (var processingManager = CreateUniversalProcessingManager(grengineSetting))
			{
				var logger = processingManager.Logger;
				try
				{
					logger.OnLogInfoAdded += Logger_OnLogInfoAdded;
					if (grengineSetting == GrEngineServiceSetting.Flipper)
					{
						processingManager.FlipGrEngine();
					}
					else
					{
						processingManager.ExecuteBatch(token);
					}
				}
				finally
				{
					logger.OnLogInfoAdded -= Logger_OnLogInfoAdded;
				}
			}
		}

		internal virtual IUniversalProcessingManager CreateUniversalProcessingManager(GrEngineServiceSetting grengineSetting)
		{
			return new UniversalProcessingManager(SupportedMessageSubtypes, ExcludedMessageSubtypes, grengineSetting);
		}

		protected bool IsGrEngineEnabled => Enterprise.Registry.Business.eServices.eAdaptorRegistry.Instance.AllowParallelUMI.Value;

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger?.Log(logType, log.Trim());
		}

		#endregion
	}
}
