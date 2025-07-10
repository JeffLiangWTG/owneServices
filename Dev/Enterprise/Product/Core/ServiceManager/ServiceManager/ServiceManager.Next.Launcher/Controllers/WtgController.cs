using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.ServiceManager.Next.Launcher.Controllers;

[ApiController]
[AllowAnonymous]
[Route("[controller]")]
public class WtgController : ControllerBase
{
	[HttpGet]
	[HttpHead]
	[Route("Status")]
	public IActionResult Status() => Ok();

	[HttpGet]
	[HttpHead]
	[Route("Ready")]
	public IActionResult Ready() => Ok();
}
