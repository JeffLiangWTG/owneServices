using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Runtime.ExceptionServices;
using System.Threading;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	abstract class eHubServiceTask : ServiceProviderImpl, IeHubServiceTaskSupport
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public override void RunTask(CancellationToken token)
		{
			CancellationToken = token;
			if (!IsEnabled(out var message))
			{
				Notifier.Notify(new InfoNotification(message));
			}
			else
			{
				ExecuteAllJobs();
			}
		}

		public virtual bool IsProduction => CompanyTypeHelper.IsProductionSystem();

		protected virtual bool IsEnabled(out string message)
		{
			message = string.Empty;
			if (IsCargoWiseDomain && !IsEHubTestingEnabled)
			{
				if (IsClientEnterpriseCode)
				{
					message = Res.GetString("51A3A976-A269-4a2f-A1BD-EB74711B1A07", $"{BrandingFactory.Instance.ProductName} has detected that this installation belongs to a CargoWise client, but is currently run within CargoWise corporate network for testing. In this case, all eHub Service tasks are disabled to prevent accidentally sending or retrieving live messages for this client.");
				}
				else
				{
					message = Res.GetString("AB183FAC-F94E-4962-8C38-13DA38D1B025", $"{BrandingFactory.Instance.ProductName} has detected that this installation is a CargoWise test or internal production system, with Enterprise code HYE, EDI, EHW or WTL. Service Task is therefore disabled by default. Please turn on System->Testing->eHub Testing registry explicitly if you want to test eHub messaging.");
				}

				return false;
			}

			return true;
		}

		void ExecuteAllJobs()
		{
			var nextExecuteIterationIsRequired = true;
			var exceptions = new List<Exception>();
			var allJobsCompletedSuccessfully = true;

			do
			{
				nextExecuteIterationIsRequired = false;
				knownExceptions.Clear();
				CancellationToken.ThrowIfCancellationRequested();
				foreach (var job in GetJobs())
				{
					try
					{
						if (!ExecuteJob(job))
						{
							allJobsCompletedSuccessfully = false;
						}
						if (job.NextExecuteIterationIsScheduled)
						{
							nextExecuteIterationIsRequired = true;
						}
					}
					catch (OperationCanceledException)
					{
						Notifier.Notify(new InfoNotification(Res.GetString("5FB85D14-8108-4667-900D-F96BE6074A77", "Service task was canceled.")));
						nextExecuteIterationIsRequired = false;
						allJobsCompletedSuccessfully = false;
						break;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						allJobsCompletedSuccessfully = false;
						exceptions.Add(ex);
					}
				}
				if (exceptions.Count == 1)
				{
					ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
				}
				else if (exceptions.Count > 1)
				{
					throw new AggregateException(exceptions);
				}
				if (ReportKnownExceptionsAsIssues && knownExceptions.Count <= 0 && allJobsCompletedSuccessfully && OutageStartTime != DateTime.MinValue)
				{
					OutageStartTime = DateTime.MinValue;
				}
			} while (allJobsCompletedSuccessfully && nextExecuteIterationIsRequired && RunContinuously);
		}

		protected virtual bool ExecuteJob(IeHubServiceTaskJob job)
		{
			try
			{
				job.Execute(CancellationToken);
				return true;
			}
			catch (ConfigurationErrorsException ex) when (ex.IsExceptionPresentIncludingInner<DllNotFoundException>() || ex.IsExceptionPresentIncludingInner<TypeInitializationException>())
			{
				ReportKnownException(ex);
			}
			catch (FailedToConnectException ex)
			{
				ReportKnownException(ex);
			}
			catch (InvalidOperationException ex) when (ex.Message.Contains((NoResString)"Timeouts are not supported on this stream.") || (ex.InnerException != null && ex.InnerException.Message.Contains((NoResString)"Timeouts are not supported on this stream.")))
			{
				// This error happens because  failed to convert to Json from a stream e.g. (JsonConvert.SerializeObject(messageStream, Formatting.Indented);), but I couldn't find the message actually sent, Improved the logging in GetErrorReportKey so we can fix it in the future.
				ReportKnownException(ex, $"InvalidOperationException caused by Json Convert in Company : {Env.CurrentCompany.Code}, CompanyPK '{Env.CurrentCompanyPK}'");
			}
			catch (InvalidOperationException ex) when (ex.Message.Contains((NoResString)"The transaction no longer has an active connection."))
			{
				ReportKnownException(ex);
			}
			catch (SqlException ex)
			{
				ReportKnownException(ex);
			}
			return false;
		}

		internal abstract DateTime OutageStartTime { get; set; }
		internal abstract bool ReportKnownExceptionsAsIssues { get; }
		TimeSpan OutageExpiryTimeSpan => TimeSpan.FromMinutes(eHubMessagingRegistry.Instance.eHubOutageExpiryTimeInMinutes.Value);
		internal virtual string EndpointNotFoundMessage => LogMessages.EHubEndpointNotFoundMessage;

		readonly List<Exception> knownExceptions = new List<Exception>();

		public void ReportKnownException(Exception ex, string message = null)
		{
			message = message ?? ex.Message;

			if (!ReportKnownExceptionsAsIssues)
			{
				Notifier.AddWarning(GetErrorLog(ex, message));
				return;
			}

			knownExceptions.Add(ex);
			var utcNow = ZDateTime.UtcNow.ToDateTime();
			if (OutageStartTime == DateTime.MinValue)
			{
				OutageStartTime = utcNow;
			}

			if (utcNow > OutageStartTime.Add(OutageExpiryTimeSpan))
			{
				Notifier.AddError(GetErrorLog(ex, message));
				ErrorReporter.ReportOnce(GetErrorReportKey(ex), message, ex);
			}
			else
			{
				Notifier.AddWarning(GetErrorLog(ex, message));
			}
		}

		public static string GetErrorLog(Exception ex, string message) => message + (NoResString)"\r\nInternal Exception: " + ex.ToString();
		public string GetErrorReportKey(Exception ex) => $"Error in '{ServiceTaskName}' service task : {ex.GetType()}";

		internal virtual INotifications Notifier
		{
			get { return notifier ?? (notifier = ServiceLogger.GetTaskNotificationSubscriber()); }
			set { notifier = value; }
		}
		protected INotifications notifier;

		internal CancellationToken CancellationToken
		{
			get; private set;
		}

		internal virtual string DomainName
		{
			get { return IPGlobalProperties.GetIPGlobalProperties().DomainName; }
		}

		internal virtual bool IsCargoWiseDomain
		{
			get
			{
				return Env.IsCargoWiseDomain(DomainName);
			}
		}

		internal virtual bool IsEHubTestingEnabled
		{
			get { return DataRegistry.Instance.EHubTesting; }
		}

		internal virtual bool IsClientEnterpriseCode
		{
			get { return CompanyTypeHelper.IsClientEnterpriseCode(); }
		}

		public virtual ICompanySettingsManager CompanySettingsManager
		{
			get { return new eHubMessagingCompanySettingsManager(); }
		}

		public abstract string ServiceTaskName { get; }
		public abstract string DefaultServerAddress { get; }

		internal abstract IEnumerable<IeHubServiceTaskJob> GetJobs();

		internal virtual bool RunContinuously
		{
			get { return true; }
		}
	}
}
