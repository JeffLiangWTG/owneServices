using CargoWise.ServiceManager.Next.Launcher.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.ServiceManager.Next.Launcher.Controllers;

[ApiController]
[DbAccessTokenAuthorization]
[Route("[controller]")]
public class ApiController : ControllerBase
{
	/// <summary>
	/// Catch invalid requests that are not handled by a dedicated controller.
	/// </summary>
	[HttpPost]
	[Route("{runnerCode}/{**slug}")]
	public IActionResult Default(string runnerCode, string slug, CancellationToken cancellationToken)
	{
		throw new NotImplementedException($"Invalid request: {runnerCode}/{slug}{QueryString.Create(HttpContext.Request.Query)}.");
	}
}
