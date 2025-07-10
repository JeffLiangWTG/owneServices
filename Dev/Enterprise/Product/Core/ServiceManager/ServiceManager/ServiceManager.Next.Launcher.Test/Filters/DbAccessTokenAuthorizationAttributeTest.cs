using CargoWise.ServiceManager.Next.Launcher.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Launcher.Test.Filters;

[TestFixture]
[TestOf(typeof(DbAccessTokenAuthorizationAttribute))]
public class DbAccessTokenAuthorizationAttributeTest
{
	Mock<IAccessTokenService>? accessTokenServiceMock;

	[SetUp]
	public void SetUp()
	{
		accessTokenServiceMock = new Mock<IAccessTokenService>(MockBehavior.Strict);
		accessTokenServiceMock!
			.Setup(m => m.CheckTokenValidity(It.IsAny<string>()))
			.Returns<string>(apiKey => apiKey == "Exception" ? throw new InvalidOperationException("Error for unit test") : apiKey == "ValidApiKey");
	}

	[TearDown]
	public void TearDown()
	{
		accessTokenServiceMock?.VerifyNoOtherCalls();
	}

	[Test]
	public void OnAuthorization_NoApiKey_ReturnUnauthorized()
	{
		var attribute = new DbAccessTokenAuthorizationAttribute();
		var context = BuildAuthorizationFilterContext();

		attribute.OnAuthorization(context);

		AssertUnauthorized(context.Result, "Api key is missing.");
	}

	[Test]
	public void OnAuthorization_InvalidAuthorizationMethod_ReturnUnauthorized()
	{
		var attribute = new DbAccessTokenAuthorizationAttribute();
		var context = BuildAuthorizationFilterContext();
		context.HttpContext.Request.Headers.Append("Authorization", "Bearer InvalidApiKey");

		attribute.OnAuthorization(context);

		AssertUnauthorized(context.Result, "Api key is missing.");
	}

	[TestCase("ApiKey")]
	[TestCase("ApiKey ")]
	[TestCase("ApiKey   ")]
	public void OnAuthorization_BlankApiKey_ReturnUnauthorized(string apikey)
	{
		var attribute = new DbAccessTokenAuthorizationAttribute();
		var context = BuildAuthorizationFilterContext();
		context.HttpContext.Request.Headers.Append("Authorization", apikey);

		attribute.OnAuthorization(context);

		AssertUnauthorized(context.Result, "Api key is missing.");
	}

	[Test]
	public void OnAuthorization_InvalidApiKey_ReturnUnauthorized()
	{
		var attribute = new DbAccessTokenAuthorizationAttribute();
		var context = BuildAuthorizationFilterContext();
		context.HttpContext.Request.Headers.Append("Authorization", "ApiKey InvalidApiKey");

		attribute.OnAuthorization(context);

		accessTokenServiceMock!.Verify(m => m.CheckTokenValidity("InvalidApiKey"), Times.Once);
		AssertUnauthorized(context.Result, "Api key is not valid.");
	}

	[Test]
	public void OnAuthorization_Exception_ReturnUnauthorized()
	{
		var attribute = new DbAccessTokenAuthorizationAttribute();
		var context = BuildAuthorizationFilterContext();
		context.HttpContext.Request.Headers.Append("Authorization", "ApiKey Exception");

		attribute.OnAuthorization(context);

		accessTokenServiceMock!.Verify(m => m.CheckTokenValidity("Exception"), Times.Once);
		AssertUnauthorized(context.Result, "Api key is not valid.");
	}

	[TestCase("ApiKey ValidApiKey")]
	[TestCase("APIKEY ValidApiKey")]
	[TestCase("apikey ValidApiKey")]
	public void OnAuthorization_ValidApiKey_ReturnNull(string authorization)
	{
		var attribute = new DbAccessTokenAuthorizationAttribute();
		var context = BuildAuthorizationFilterContext();
		context.HttpContext.Request.Headers.Append("Authorization", authorization);

		attribute.OnAuthorization(context);

		accessTokenServiceMock!.Verify(m => m.CheckTokenValidity("ValidApiKey"), Times.Once);
		Assert.That(context.Result, Is.Null);
	}

	[Test]
	public void OnAuthorization_ValidApiKeyAndOtherAuthorization_ReturnNull()
	{
		var attribute = new DbAccessTokenAuthorizationAttribute();
		var context = BuildAuthorizationFilterContext();
		context.HttpContext.Request.Headers.Append("Authorization", "Basic username:password");
		context.HttpContext.Request.Headers.Append("Authorization", "ApiKey ValidApiKey");
		context.HttpContext.Request.Headers.Append("Authorization", "Bearer Token");

		attribute.OnAuthorization(context);

		accessTokenServiceMock!.Verify(m => m.CheckTokenValidity("ValidApiKey"), Times.Once);
		Assert.That(context.Result, Is.Null);
	}

	static void AssertUnauthorized(IActionResult? contextResult, string expectedDetails)
	{
		Assert.That(contextResult, Is.InstanceOf<UnauthorizedObjectResult>());
		var result = (UnauthorizedObjectResult)contextResult!;
		Assert.Multiple(() =>
		{
			Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
			Assert.That(result.Value, Is.InstanceOf<ProblemDetails>());
		});
		var problemDetails = (ProblemDetails)result.Value!;
		Assert.Multiple(() =>
		{
			Assert.That(problemDetails.Detail, Is.EqualTo(expectedDetails));
			Assert.That(problemDetails.Status, Is.EqualTo(StatusCodes.Status401Unauthorized));
			Assert.That(problemDetails.Type, Is.EqualTo("https://tools.ietf.org/html/rfc9110#section-15.5.2"));
			Assert.That(problemDetails.Title, Is.EqualTo("Unauthorized"));
		});
	}

	AuthorizationFilterContext BuildAuthorizationFilterContext()
	{
		var httpContext = new DefaultHttpContext();
		var serviceCollection = new ServiceCollection();
		_ = serviceCollection
			.AddScoped(_ => accessTokenServiceMock!.Object)
			.AddLogging()
			.AddMvcCore();
		httpContext.RequestServices = serviceCollection.BuildServiceProvider();
		var routeData = new Microsoft.AspNetCore.Routing.RouteData();
		var controllerActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor();
		var actionContext = new ActionContext(httpContext, routeData, controllerActionDescriptor);
		var filters = new List<IFilterMetadata>();
		var context = new AuthorizationFilterContext(actionContext, filters);
		return context;
	}
}
