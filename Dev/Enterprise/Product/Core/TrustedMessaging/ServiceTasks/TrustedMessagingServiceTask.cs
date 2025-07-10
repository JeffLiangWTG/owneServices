using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"TMS",
	"Trusted Messaging Service Task",
	"SYS",
	typeof(Enterprise.TrustedMessaging.ServiceTasks.TrustedMessagingServiceTask),
	IsMandatory = true,
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Minutes",
	MaximumPeriod = "30Minutes",
	DefaultScheduleRunEvery = "30Minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.TrustedMessaging.ServiceTasks
{
	public class TrustedMessagingServiceTask : ServiceProviderImpl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Reference Values should be in English only")]
		public override void RunTask(CancellationToken token)
		{
			Log(LogType.Debug, "Service task has started.");
			using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
			{
				cts.CancelAfter(TaskTimeout);
				try
				{
					var apiService = NewRemoteApiService();
					if (!AreCertificatesReady)
					{
						Log(LogType.Debug, "Starting Certificates download...");
						var result = RunWithTimeout(() => apiService.DownloadCertificatesAsync(cts.Token), cts.Token);
						if (result.Success)
						{
							Log(LogType.Information, "Certificates Downloaded.");
						}
						else if (result.Messages?.Any() ?? false)
						{
							result.Log(ServiceLogger);
						}
					}

					if (AreCertificatesReady)
					{
						Log(LogType.Debug, "Starting Feature Control download...");
						RunWithTimeout(() => apiService.DownloadFeatureControlRuleAsync(ServiceLogger, cts.Token), cts.Token);
					}
				}
				catch (OperationCanceledException)
				{
					Log(LogType.Error, "Task was canceled or timed out.");
				}
				finally
				{
					cts.Cancel();
				}
				Log(LogType.Debug, "Service task has completed.");
			}
		}

		static T RunWithTimeout<T>(Func<Task<T>> asyncFunc, CancellationToken token)
		{
			var task = Task.Run(asyncFunc, token);
			if (Task.WhenAny(task, Task.Delay(Timeout.InfiniteTimeSpan, token)).Result == task)
			{
				return task.GetAwaiter().GetResult(); // Safe unwrapping
			}

			throw new OperationCanceledException(token); // timeout or cancel
		}

		bool AreCertificatesReady
			=> !IsNullOrEmpty(WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.Value) &&
				!string.IsNullOrWhiteSpace(WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.Value) &&
				!IsNullOrEmpty(WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.Value);

		bool IsNullOrEmpty(byte[] bytes) => bytes == null || !bytes.Any();

		void Log(LogType type, string message) => ServiceLogger?.Log(type, message);

		protected virtual RemoteApiService NewRemoteApiService() => new(new UserPortalClientConfiguration());

		protected virtual TimeSpan TaskTimeout { get; } = TimeSpan.FromMinutes(5);
	}
}
