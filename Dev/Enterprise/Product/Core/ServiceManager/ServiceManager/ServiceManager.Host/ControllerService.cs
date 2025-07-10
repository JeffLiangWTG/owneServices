using System;
using System.ServiceProcess;
using System.Threading;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	sealed partial class ControllerService : ServiceBase, IControllerService
	{
		public ControllerService(Lazy<IServiceManagerApplication> serviceManagerApplication, IHostLogger hostLogger, IApplicationEmergencyExit applicationEmergencyExit, IServiceNameProvider serviceNameProvider, ICancellationRequester cancellationRequester, ICancellationTokenProvider cancellationTokenProvider)
		{
			this.serviceManagerApplication = serviceManagerApplication ?? throw new ArgumentNullException(nameof(serviceManagerApplication));
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.applicationEmergencyExit = applicationEmergencyExit ?? throw new ArgumentNullException(nameof(applicationEmergencyExit));
			this.cancellationRequester = cancellationRequester ?? throw new ArgumentNullException(nameof(cancellationRequester));
			this.cancellationTokenProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));

			serviceNameProvider = serviceNameProvider ?? throw new ArgumentNullException(nameof(serviceNameProvider));

			InitializeComponent();

			ServiceName = serviceNameProvider.GetServiceName();
			CanShutdown = true;
		}

		protected override void OnStart(string[] args)
		{
			try
			{
				hostLogger.Log(LogLevel.Information, nameof(OnStart));

				dispatchingThread = new Thread(RunApplication)
				{
					Name = nameof(ControllerService),
				};
				dispatchingThread.Start();

				base.OnStart(args);
			}
			catch (Exception ex)
			{
				applicationEmergencyExit.ExitApplicationUnsafe(nameof(OnStart), ex);
			}
		}

		void RunApplication()
		{
			try
			{
				serviceManagerApplication.Value.Run(cancellationTokenProvider.Token);
			}
			catch (Exception ex)
			{
				applicationEmergencyExit.ExitApplicationUnsafe(nameof(RunApplication), ex);
			}
		}

		protected override void OnStop()
		{
			try
			{
				hostLogger.Log(LogLevel.Information,nameof(OnStop));

				StopApplication();

				base.OnStop();
			}
			catch (Exception ex)
			{
				applicationEmergencyExit.ExitApplicationUnsafe(nameof(OnStop), ex);
			}
		}

		protected override void OnShutdown()
		{
			try
			{
				hostLogger.Log(LogLevel.Information,nameof(OnShutdown));

				StopApplication();

				base.OnShutdown();
			}
			catch (Exception ex)
			{
				applicationEmergencyExit.ExitApplicationUnsafe(nameof(OnShutdown), ex);
			}
		}

		void StopApplication()
		{
			cancellationRequester.Cancel();

			if (dispatchingThread != null)
			{
				_ = dispatchingThread.Join(TimeSpan.FromMinutes(2));
			}
		}

		#region -console

		public void ConsoleRun(Action wait = null)
		{
			wait ??= () => { };

			OnStart(Array.Empty<string>());

			wait();

			OnShutdown();
		}

		#endregion -console

		public TimeSpan RequireSwitchOffTime()
		{
			try
			{
				var weCanWait = TimeSpan.FromSeconds(60);
				RequestAdditionalTime((int)weCanWait.Add(TimeSpan.FromSeconds(10)).TotalMilliseconds); // Can't call it on shut down
				return weCanWait;
			}
			catch (InvalidOperationException)
			{
				return TimeSpan.FromSeconds(20);
			}
		}

		public ServiceBase GetService() => this;

		readonly IApplicationEmergencyExit applicationEmergencyExit;
		readonly ICancellationRequester cancellationRequester;
		readonly ICancellationTokenProvider cancellationTokenProvider;
		readonly IHostLogger hostLogger;
		readonly Lazy<IServiceManagerApplication> serviceManagerApplication;

		Thread dispatchingThread;
	}
}
