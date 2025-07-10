#region Test
#if DEBUG

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public static class ControllerTestHelper
	{
		public static HttpResponseMessage Execute(IHttpController controller, HttpRequestMessage request, Type controllerTargetType = null)
		{
			using (var configuration = new HttpConfiguration())
			{
				configuration.MapHttpAttributeRoutes();
				configuration.EnsureInitialized();

				var requestContext = new HttpRequestContext
				{
					Configuration = configuration,
					Url = new UrlHelper(request)
				};

				request.Properties[HttpPropertyKeys.RequestContextKey] = requestContext;

				var controllerType = controllerTargetType ?? controller.GetType();
				var controllerDescriptor = new HttpControllerDescriptor(configuration, controllerType.Name, controllerType);

				var context = new HttpControllerContext(requestContext, request, controllerDescriptor, controller)
				{
					RouteData = configuration.Routes.GetRouteData(request)
				};

				var task = controller.ExecuteAsync(context, CancellationToken.None);
				var result = task.Result;
				return result;
			}
		}

		public static async Task<HttpResponseMessage> ExecuteAsync(IHttpController controller, HttpRequestMessage request, Type controllerTargetType = null)
		{
			using (var configuration = new HttpConfiguration())
			{
				configuration.MapHttpAttributeRoutes();
				configuration.EnsureInitialized();

				var requestContext = new HttpRequestContext
				{
					Configuration = configuration,
					Url = new UrlHelper(request)
				};

				request.Properties[HttpPropertyKeys.RequestContextKey] = requestContext;

				var controllerType = controllerTargetType ?? controller.GetType();
				var controllerDescriptor = new HttpControllerDescriptor(configuration, controllerType.Name, controllerType);

				var context = new HttpControllerContext(requestContext, request, controllerDescriptor, controller)
				{
					RouteData = configuration.Routes.GetRouteData(request)
				};

				return await controller.ExecuteAsync(context, CancellationToken.None);
			}
		}
	}
}

#endif
#endregion