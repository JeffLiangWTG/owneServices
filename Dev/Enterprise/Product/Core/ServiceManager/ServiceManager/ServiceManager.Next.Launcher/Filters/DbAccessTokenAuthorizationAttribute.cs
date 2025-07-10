using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CargoWise.ServiceManager.Next.Launcher.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class DbAccessTokenAuthorizationAttribute : Attribute, IAuthorizationFilter
{
	const string ApiKeyIsNotValid = "Api key is not valid.";
	const string ApiKeyIsMissing = "Api key is missing.";
	const string HeaderPrefix = "ApiKey ";

	public void OnAuthorization(AuthorizationFilterContext context)
	{
		var authorizationError = Authorize(context.HttpContext);
		if (authorizationError != null)
		{
			var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
			var problemDetails = problemDetailsFactory.CreateProblemDetails(
				context.HttpContext,
				statusCode: StatusCodes.Status401Unauthorized,
				detail: authorizationError);
			context.Result = new UnauthorizedObjectResult(problemDetails);
		}
	}

	string? Authorize(HttpContext httpContext)
	{
		var logger = httpContext.RequestServices.GetRequiredService<ILogger<DbAccessTokenAuthorizationAttribute>>();
		var accessTokenService = httpContext.RequestServices.GetRequiredService<IAccessTokenService>();
		try
		{
			var authorizations = httpContext.Request.Headers.Authorization;
			if (authorizations.Count == 0)
			{
				logger.LogInformation("Authorization is missing");
				return ApiKeyIsMissing;
			}

			var apiAuthorization = authorizations.FirstOrDefault(auth => auth is not null && auth.StartsWith(HeaderPrefix, StringComparison.OrdinalIgnoreCase));
			if (apiAuthorization is null)
			{
				logger.LogInformation("Invalid authorization method starting with {prefix}", Truncate(authorizations.ToString(), 10));
				return ApiKeyIsMissing;
			}

			var apiKey = apiAuthorization[HeaderPrefix.Length..];
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				logger.LogInformation($"Api key is missing in the {HeaderPrefix} header");
				return ApiKeyIsMissing;
			}

			if (!accessTokenService.CheckTokenValidity(apiKey))
			{
				logger.LogInformation("Api key starting with {apiKeySubstring} is not valid", Truncate(apiKey, 10));
				return ApiKeyIsNotValid;
			}

			return null;
		}
		catch (Exception e)
		{
			logger.LogError(e, "Unable to authorize request");
		}

		return ApiKeyIsNotValid;
	}

	string Truncate(string value, int maxLength)
	{
		return string.IsNullOrEmpty(value) || value.Length <= maxLength ? value : value[..maxLength];
	}
}
