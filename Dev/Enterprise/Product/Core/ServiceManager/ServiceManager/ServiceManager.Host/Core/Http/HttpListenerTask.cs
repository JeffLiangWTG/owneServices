using System;
using System.Net;
using System.Threading;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using Async = System.Threading.Tasks;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ServiceManager.Host
{
	sealed class HttpListenerTask : IHttpListenerTask, IDisposable
	{
		public HttpListenerTask(IHostLogger hostLogger,
			IHttpListenerFactory httpListenerFactory,
			IRequestQueueProduceable requestQueue,
			IHttpListenerExceptionHandler httpListenerExceptionHandler,
			ICancellationTokenProvider cancellationTokenProvider,
			IErrorReporterProxy errorReporterProxy)
		{
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			listenerFactory = httpListenerFactory ?? throw new ArgumentNullException(nameof(httpListenerFactory));
			this.requestQueue = requestQueue ?? throw new ArgumentNullException(nameof(requestQueue));
			this.httpListenerExceptionHandler = httpListenerExceptionHandler ?? throw new ArgumentNullException(nameof(httpListenerExceptionHandler));
			this.cancellationTokenProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
		}

		void ListenForRequest(CancellationToken cancellationToken)
		{
			try
			{
				var result = listener.BeginGetContext(ListenerContextCallback, listener);
				WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle, result.AsyncWaitHandle });
			}
			catch (HttpListenerException listenerException)
				when (listenerException.NativeErrorCode == (int)HttpListenerErrorCodes.HandleIsInvalid)
			{
				DisposeQueuedRequests(cancellationToken);
				listener.Dispose();
				throw new InitialisationRequestException(listenerException);
			}

			void ListenerContextCallback(IAsyncResult asyncResult)
			{
				if (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						requestQueue.Add(new HttpListenerContextWrapper(listener.EndGetContext(asyncResult)), cancellationToken);
					}
					catch (Exception ex)
					{
						errorReporterProxy.ReportOnce(ex.Message, ex);
					}
				}
			}
		}

		bool DisposeQueuedRequests(CancellationToken cancellationToken)
			=> WaitForQueuedRequests(TimeSpan.FromSeconds(5), cancellationToken);

		internal bool WaitForQueuedRequests(TimeSpan timeout, CancellationToken cancellationToken)
		{
			var task = Async.Task.Run(() =>
			{
				while (requestQueue.Count > 0
						&& !cancellationToken.IsCancellationRequested)
				{
					Thread.Sleep(TimeSpan.FromMilliseconds(100));
				}
			});

			return task.Wait(timeout);
		}

		public void Dispose()
		{
			if (!disposed)
			{
				listener?.Dispose();
				DisposeQueuedRequests(cancellationTokenProvider.Token);
				disposed = true;
			}
		}

		public void Initialise(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}

			var uriPrefix = ServiceManagerHelper.GetListenerStrongBinding(ServiceType.ProcessController);
			try
			{
				listener = listenerFactory.Create(uriPrefix);
			}
			catch (HttpListenerException listenerException)
			{
				switch (listenerException.NativeErrorCode)
				{
					case (int)HttpListenerErrorCodes.ErrorAccessDenied:
						var username = System.Environment.UserDomainName + "\\" + System.Environment.UserName;
						var message = $@"User {username} does not have access rights to listen HTTP prefix {uriPrefix}.
Please contact your admin to check all access rights required are granted and http protocol/port/prefix are enabled,
or use other account for Service Tasks with required access rights.";
						throw new HttpCriticalException(message, listenerException);

					case (int)HttpListenerErrorCodes.ErrorAlreadyExists:
						throw new HttpCriticalException("Other instance of a process controller is running already.", listenerException);
					default:
						throw new HttpCriticalException($"Could not initialise HTTP listener: {listenerException.Message}.", listenerException);
				}
			}

			hostLogger.Log(LogLevel.Information, $"Http server is initialized at [{uriPrefix}]");
		}

		public string Name { get; } = typeof(HttpListenerTask).FullName;
		public TimeSpan RunDelay { get; } = TimeSpan.Zero;
		public TimeSpan ErrorDelay { get; } = TimeSpan.FromSeconds(10);

		public void Run(CancellationToken cancellationToken)
		{
			try
			{
				ListenForRequest(cancellationToken);
			}
			catch (HttpListenerException httpListenerException) when (!httpListenerExceptionHandler.HandleException(httpListenerException, hostLogger))
			{
				throw;
			}
		}

		readonly IHostLogger hostLogger;
		readonly IHttpListenerFactory listenerFactory;
		readonly IRequestQueueProduceable requestQueue;
		readonly IHttpListenerExceptionHandler httpListenerExceptionHandler;
		readonly ICancellationTokenProvider cancellationTokenProvider;
		readonly IErrorReporterProxy errorReporterProxy;
		IHttpListener listener;
		bool disposed;
	}
}
