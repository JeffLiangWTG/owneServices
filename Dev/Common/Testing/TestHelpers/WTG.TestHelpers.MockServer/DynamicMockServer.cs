using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace WTG.TestHelpers.MockServer
{
	public class DynamicMockServer : MockServer
	{
		public List<IMockRequest> Requests { get; }

		public Dictionary<string, Func<IMockRequest, IMockResponse>> Routes { get; }

		public DynamicMockServer() : this("Dynamic Mock Server")
		{
		}

		public DynamicMockServer(string serviceName) : base(serviceName)
		{
			Routes = new Dictionary<string, Func<IMockRequest, IMockResponse>>();
			Requests = new List<IMockRequest>();
		}

		public void AddRoute(string method, string route, Func<IMockRequest, IMockResponse> action)
		{
			var key = $"{method}:{route}";
			Routes[key] = action;
		}

		protected override IMockResponse CreateMockResponse(HttpListenerRequest request)
		{
			var conentRecieved = GetRequestBody(request);

			var mockRequest = new MockRequest
			{
				Content = conentRecieved,
				Headers = request.Headers.AllKeys.ToDictionary(k => k, k => request.Headers[k])
			};

			Requests.Add(mockRequest);

			var key = $"{request.HttpMethod}:{request.Url.AbsolutePath}";
			if (Routes.TryGetValue(key, out var action))
			{
				return action(mockRequest);
			}
			else
			{
				return new MockResponseText("Not Found", 404);
			}
		}
	}
}
