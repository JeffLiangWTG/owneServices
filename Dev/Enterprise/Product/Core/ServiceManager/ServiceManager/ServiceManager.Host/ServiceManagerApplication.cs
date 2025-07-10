using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class ServiceManagerApplication : IServiceManagerApplication
	{
		public ServiceManagerApplication(IServiceManagerApplicationInitializer serviceManagerApplicationInitializer,
			IDelayProvider delayProvider,
			IEnumerable<IServiceManagerTask> serviceManagerTasks,
			IApplicationEmergencyExit applicationEmergencyExit,
			IErrorReporterProxy errorReporterProxy)
		{
			this.serviceManagerApplicationInitializer = serviceManagerApplicationInitializer ?? throw new ArgumentNullException(nameof(serviceManagerApplicationInitializer));
			this.delayProvider = delayProvider ?? throw new ArgumentNullException(nameof(delayProvider));
			this.serviceManagerTasks = serviceManagerTasks ?? throw new ArgumentNullException(nameof(serviceManagerTasks));
			this.applicationEmergencyExit = applicationEmergencyExit ?? throw new ArgumentNullException(nameof(applicationEmergencyExit));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
		}

		void Run(IServiceManagerTask serviceManagerTask, CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						serviceManagerTask.Initialise(cancellationToken);

						while (!cancellationToken.IsCancellationRequested)
						{
							try
							{
								serviceManagerTask.Run(cancellationToken);
								WaitIgnoringExceptions(serviceManagerTask.RunDelay, cancellationToken);
							}
							catch (TaskCanceledException)
							{
								// Ignore
							}
							catch (OperationCanceledException)
							{
								// Ignore
							}
							catch (InitialisationRequestException exception)
							{
								errorReporterProxy.ReportOnce(exception.Message, exception);
								break;
							}
							catch (HostInternalException)
							{
								throw;
							}
							catch (Exception exception) when (!exception.IsCriticalException())
							{
								if (!ReportOnceSafe(exception))
								{
									throw;
								}

								WaitIgnoringExceptions(serviceManagerTask.ErrorDelay, cancellationToken);
							}
						}
					}
					catch (TaskCanceledException)
					{
						// Ignore
					}
					catch (OperationCanceledException)
					{
						// Ignore
					}
					catch (HostInternalException)
					{
						throw;
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						if (!ReportOnceSafe(exception))
						{
							throw;
						}

						WaitIgnoringExceptions(serviceManagerTask.ErrorDelay, cancellationToken);
					}
				}
			}
			catch (Exception unhandledException)
			{
				applicationEmergencyExit.ExitApplicationUnsafe(nameof(Run), unhandledException);
			}

			bool ReportOnceSafe(Exception exception)
			{
				try
				{
					errorReporterProxy.ReportOnce(exception.Message, exception);
					return true;
				}
				catch
				{
					// ignore exceptions
					return false;
				}
			}

			void WaitIgnoringExceptions(TimeSpan timeSpan, CancellationToken token)
			{
				try
				{
					delayProvider.Delay(timeSpan, token);
				}
				catch
				{
					// ignore exceptions
				}
			}
		}

		public void Run(CancellationToken cancellationToken)
		{
			Db.DisableThreadSchemaVersionCheckPermanently();

			using (Db.DisposableActionForDbConnection())
			{
				serviceManagerApplicationInitializer.Initialize();
			}

			using (var countdownEvent = new CountdownEvent(0))
			{
				var runningTaskArray = serviceManagerTasks
					.Select(serviceManagerTask => new Thread(() =>
					{
						try
						{
							// Note, deliberately not using Db.DisposableActionForDbConnection() here.
							// Most tasks should not be using a connection. Those that do should manage their own.
							Run(serviceManagerTask, cancellationToken);
						}
						finally
						{
							countdownEvent.Signal();
						}
					})
					{
						Name = serviceManagerTask.Name,
					})
					.ToArray();
				countdownEvent.Reset(runningTaskArray.Length);
				runningTaskArray.ForEach(thread => thread.Start());
				countdownEvent.Wait();
			}

			if (cancellationToken != CancellationToken.None && !cancellationToken.IsCancellationRequested)
			{
				applicationEmergencyExit.ExitApplicationUnsafe($"{nameof(ServiceManagerApplication)} ran to completion without cancellation");
			}
		}

		readonly IDelayProvider delayProvider;
		readonly IServiceManagerApplicationInitializer serviceManagerApplicationInitializer;
		readonly IEnumerable<IServiceManagerTask> serviceManagerTasks;
		readonly IApplicationEmergencyExit applicationEmergencyExit;
		readonly IErrorReporterProxy errorReporterProxy;
	}
}
