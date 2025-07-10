using System;
using System.Net;
using System.Threading;
using Enterprise.ServiceManager.Host.Http;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	sealed class RequestQueueProcessor : IRequestQueueProcessor, IDisposable
	{
		public RequestQueueProcessor(
			IHostLogger hostLogger,
			IRequestQueueConsumable requestQueue,
			IRequestProcessorFactory requestProcessorFactory,
			IHttpListenerExceptionHandler httpListenerExceptionHandler,
			IErrorReporterProxy errorReporterProxy,
			IHostRegistrySettings hostRegistry)
		{
			_ = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.requestQueue = requestQueue ?? throw new ArgumentNullException(nameof(requestQueue));
			this.requestProcessorFactory = requestProcessorFactory ?? throw new ArgumentNullException(nameof(requestProcessorFactory));
			this.httpListenerExceptionHandler = httpListenerExceptionHandler ?? throw new ArgumentNullException(nameof(httpListenerExceptionHandler));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));

			requestProcessorConfiguredEvent = new ManualResetEvent(false);
			threadControlSemaphore = new Lazy<SemaphoreSlim>(() =>
			{
				// Avoiding database calls in constructors
				var maxThreads = hostRegistry.ServiceTaskHttpProcessorMaxThreads;
				return new SemaphoreSlim(maxThreads, maxThreads);
			});
		}

		public void Run(CancellationToken cancellationToken)
		{
			if (WaitHandle.WaitAny(new [] { requestProcessorConfiguredEvent, cancellationToken.WaitHandle }) == 1)
			{
				return;
			}

			if (!disposed && !cancellationToken.IsCancellationRequested)
			{
				threadControlSemaphore.Value.Wait(cancellationToken);

				var request = requestQueue.Take(cancellationToken);

				ThreadPool.QueueUserWorkItem(o =>
				{
					try
					{
						requestProcessor.ProcessRequest(request.Context, cancellationToken);
					}
					catch (HttpListenerException httpListenerException)
						when (httpListenerExceptionHandler.HandleException(httpListenerException, hostLogger))
					{
						// exception is handled and won't be reported
					}
					catch (Exception ex)
					{
						errorReporterProxy.ReportOnce(ex.Message, ex);
					}
					finally
					{
						if (!disposed)
						{
							threadControlSemaphore.Value.Release();
						}
					}
				});
			}
		}

		public void ConfigureHttpRequestProcessor(ITaskScheduler scheduler, ITaskStatusProvider statusProvider, IActionQueue actionQueue)
		{
			if (scheduler == null)
			{
				throw new ArgumentNullException(nameof(scheduler));
			}

			if (statusProvider == null)
			{
				throw new ArgumentNullException(nameof(statusProvider));
			}

			if (actionQueue == null)
			{
				throw new ArgumentNullException(nameof(actionQueue));
			}

			requestProcessor = requestProcessorFactory.CreateRequestProcessor(actionQueue, scheduler, statusProvider);
			requestProcessorConfiguredEvent.Set();
		}

		public void Dispose()
		{
			if (!disposed)
			{
				threadControlSemaphore.Value.Dispose();
				disposed = true;
			}
		}

		public void Initialise(CancellationToken cancellationToken)
		{
		}

		public string Name { get; } = typeof(RequestQueueProcessor).FullName;
		public TimeSpan RunDelay { get; } = TimeSpan.Zero;
		public TimeSpan ErrorDelay { get; } = TimeSpan.Zero;

		bool disposed;
		readonly Lazy<SemaphoreSlim> threadControlSemaphore;

		readonly IHostLogger hostLogger;
		readonly IRequestQueueConsumable requestQueue;
		readonly IRequestProcessorFactory requestProcessorFactory;
		readonly IHttpListenerExceptionHandler httpListenerExceptionHandler;
		readonly ManualResetEvent requestProcessorConfiguredEvent;
		readonly IErrorReporterProxy errorReporterProxy;
		IRequestProcessor requestProcessor;
	}
}
