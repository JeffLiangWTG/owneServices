using CargoWise.ServiceManager.Next.Launcher.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Launcher.Test.Controllers;

public class WtgControllerTest
{
	[Test]
	public void WtgController_HasCorrectAttribute()
	{
		var type = typeof(WtgController);
		var attributes = type.GetCustomAttributes(inherit: false);
		Assert.That(
		attributes.Select(x => x.GetType()),
			Is.SupersetOf(new[] { typeof(ApiControllerAttribute), typeof(AllowAnonymousAttribute), typeof(RouteAttribute) }));
		Assert.That(attributes.OfType<RouteAttribute>().Select(x => x.Template), Is.EquivalentTo(new[] { "[controller]" }));
	}

	[Test]
	public void Status_ReturnOk()
	{
		var wtgController = new WtgController();
		Assert.That(wtgController.Status(), Is.InstanceOf<OkResult>());
	}

	[Test]
	public void Ready_ReturnOk()
	{
		var wtgController = new WtgController();
		Assert.That(wtgController.Ready(), Is.InstanceOf<OkResult>());
	}
}
