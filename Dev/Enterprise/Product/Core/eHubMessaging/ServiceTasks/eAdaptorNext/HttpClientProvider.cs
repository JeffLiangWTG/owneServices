using System;
using System.Net.Http;
using System.Threading;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public static class HttpClientProvider
	{
		const int DefaultTimeout = 120;

		[ThreadSafe]
		static readonly AsyncLocal<TimeSpan?> timeout = new AsyncLocal<TimeSpan?>();

		[ThreadSafe]
		static readonly AsyncLocal<Func<HttpMessageHandler>> messageHandlerFactory = new AsyncLocal<Func<HttpMessageHandler>>();

		public static HttpClient GetClient()
		{
			var result = messageHandlerFactory.Value == null ? new HttpClient() : new HttpClient(messageHandlerFactory.Value());
			result.Timeout = timeout.Value ?? TimeSpan.FromSeconds(DefaultTimeout);
			return result;
		}

		public static IDisposable TemporaryOverrideTimeout(TimeSpan newTimeout)
		{
			return new DisposableAction(() => timeout.Value = newTimeout, () => timeout.Value = null);
		}

		public static IDisposable TemporaryOverrideHandler(Func<HttpMessageHandler> newHandlerFactory)
		{
			return new DisposableAction(() => messageHandlerFactory.Value = newHandlerFactory, () => messageHandlerFactory.Value = null);
		}
	}
}
