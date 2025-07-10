using CargoWise.ServiceManager.Next.Launcher.Controllers;
using CargoWise.ServiceManager.Next.Launcher.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Launcher.Test.Controllers;

public class ApiControllerTest
{
	[Test]
	public void ApiController_HasCorrectAttribute()
	{
		var type = typeof(ApiController);
		var attributes = type.GetCustomAttributes(inherit: false);
		Assert.That(
			attributes.Select(x => x.GetType()),
			Is.SupersetOf(new[] { typeof(ApiControllerAttribute), typeof(DbAccessTokenAuthorizationAttribute), typeof(RouteAttribute) }));
		Assert.That(attributes.OfType<RouteAttribute>().Select(x => x.Template), Is.EquivalentTo(new[] { "[controller]" }));
	}

	[TestCase(null, null, null)]
	[TestCase("runnerCode", "slug", null)]
	[TestCase("token", "signCW", null)]
	[TestCase("token", "signCW", "?audience=123")]
	[TestCase("token", "resetAccessToken", null)]
	public void Default_ThrowNotImplementedException(string runnerCode, string slug, string? queryParameters)
	{
		var cancellationToken = new CancellationToken(canceled: true);
		var httpContext = new DefaultHttpContext();
		if (queryParameters != null)
		{
			httpContext.Request.QueryString = new QueryString(queryParameters);
		}
		var controllerContext = new ControllerContext
		{
			HttpContext = httpContext,
		};
		var apiController = new ApiController
		{
			ControllerContext = controllerContext,
		};
		var e = Assert.Throws<NotImplementedException>(() => apiController.Default(runnerCode, slug, cancellationToken));
		Assert.That(e?.Message, Is.EqualTo($"Invalid request: {runnerCode}/{slug}{queryParameters}."));
	}
}
