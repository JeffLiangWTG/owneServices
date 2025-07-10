using System;
using System.Collections.Generic;
using System.Net;
using CargoWise.Common;
using Enterprise.ServiceManager.Host.Queue;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;

namespace Enterprise.ServiceManager.Host
{
	class ResponseStringBuilder
	{
		public ResponseStringBuilder(string dbServer, string dbName, ITaskScheduler taskScheduler, ITaskStatusProvider taskStatusProvider, IQueueStatusProviderFactory statusProviderFactory, IHostServiceStatusProvider hostServiceStatusProvider, IJsonConverter jsonConverter, IServiceHostRequestProvider serviceHostRequestProvider)
		{
			_ = dbServer ?? throw new ArgumentNullException(nameof(dbServer));
			_ = dbName ?? throw new ArgumentNullException(nameof(dbName));
			_ = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
			_ = taskStatusProvider ?? throw new ArgumentNullException(nameof(taskStatusProvider));
			_ = statusProviderFactory ?? throw new ArgumentNullException(nameof(statusProviderFactory));
			_ = hostServiceStatusProvider ?? throw new ArgumentNullException(nameof(hostServiceStatusProvider));
			_ = jsonConverter ?? throw new ArgumentNullException(nameof(jsonConverter));
			_ = serviceHostRequestProvider ?? throw new ArgumentNullException(nameof(serviceHostRequestProvider));

			var hostName = Dns.GetHostName();
			requestHandlers = GetRequestHandlers(taskScheduler, taskStatusProvider, hostName, dbServer, dbName, statusProviderFactory, hostServiceStatusProvider, jsonConverter, serviceHostRequestProvider)
			.ToSortedDictionary(k => k.Uri, new RequestHandler.UriComparer());
			logFilesRequestHandler = new LogFilesRequestHandler(hostName, dbServer, dbName);
		}

		readonly SortedDictionary<Uri, RequestHandler> requestHandlers;
		readonly LogFilesRequestHandler logFilesRequestHandler;

		static IEnumerable<RequestHandler> GetRequestHandlers(ITaskScheduler taskScheduler, ITaskStatusProvider taskStatusProvider, string hostName, string dbServer, string dbName, IQueueStatusProviderFactory statusProviderFactory, IHostServiceStatusProvider hostServiceStatusProvider, IJsonConverter jsonConverter, IServiceHostRequestProvider provider)
		{
			yield return new TasksStatusRequestHandler(taskStatusProvider, jsonConverter, hostName);
			yield return new TaskStatusRequestHandler(taskStatusProvider, jsonConverter, hostName);
			yield return new IsActiveRequestHandler(hostServiceStatusProvider, jsonConverter, hostName);
			yield return new BindingListRequestHandler(jsonConverter, hostName);
			yield return new QueueRequestHandler(statusProviderFactory.Create(taskStatusProvider), jsonConverter, hostName);
			yield return new CommandRequestHandler(taskScheduler, jsonConverter, hostName);

			foreach (var requestHandler in provider.ServiceHostRequests)
			{
				yield return ServiceHostRequestHandlerFactory.GetHandler(requestHandler, jsonConverter, requestHandler.GetUri(hostName, dbServer, dbName));
			}
		}

		public bool TryGetRequestHandler(WebRequestInfo request, out RequestHandler handler)
		{
			if (requestHandlers.TryGetValue(request.Uri, out handler))
			{
				return true;
			}
			else if (request.Uri.PathAndQuery.StartsWith(logFilesRequestHandler.Uri.PathAndQuery, StringComparison.OrdinalIgnoreCase)) // TODO : URL tree's.
			{
				handler = logFilesRequestHandler;
				return true;
			}
			return false;
		}

		public string GetResponseString(WebRequestInfo request)
		{
			string result = null;
			if (TryGetRequestHandler(request, out var handler))
			{
				result = handler.Handle(request);
			}

			return result ?? "The page cannot be found.";
		}
	}
}

