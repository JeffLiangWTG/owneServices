using eServices.eHubPortal.Services;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace eServices.eHubPortal.Tests.CodeMaps;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CodeMapsIndexTests : Bunit.TestContext
{
	[Test]
	public void CodeMapsIndexGeneral()
	{
		using var context = Services.SetupInMemoryDatabase();
		context.LoadInMemoryData("Content\\CodeMaps\\ForwardToProxyGateway.json");
		context.LoadInMemoryData("Content\\CodeMaps\\ContainerQualityCode.json");

		SetupCodeMapsIndex();

		var cut = RenderComponent<Components.Pages.CodeMaps.Index>();
		cut.WaitForElements("tr", 2);

		var table = cut.Find("tbody");
		var expected = HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsIndexGeneral.htm");
		table.MarkupMatches(expected);
	}

	[Test]
	public void CodeMapsIndexOwner()
	{
		using var context = Services.SetupInMemoryDatabase();
		context.LoadInMemoryData("Content\\CodeMaps\\ForwardToProxyGateway.json");
		context.LoadInMemoryData("Content\\CodeMaps\\ContainerQualityCode.json");

		SetupCodeMapsIndex("OCM", true);

		var cut = RenderComponent<Components.Pages.CodeMaps.Index>(parameters => parameters.Add(p => p.Owner, "OCM"));
		cut.WaitForElements("tr", 2);

		var table = cut.Find("tbody");
		var expected = HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsIndexOwner.htm");
		table.MarkupMatches(expected);
	}

	private void SetupCodeMapsIndex(string? owner = null, bool writeAuth = false)
	{
		var accessGroupService = new Mock<IAccessGroupService>();
		accessGroupService.Setup(x => x.IsAuthorizedForRead(owner)).ReturnsAsync(true);
		accessGroupService.Setup(x => x.IsAuthorizedForWrite(owner)).ReturnsAsync(writeAuth);
		Services.AddScoped(_ => accessGroupService.Object);
		Services.AddScoped(_ => new FakeLogger<Components.Pages.CodeMaps.Index>(NUnit.Framework.TestContext.WriteLine));
		JSInterop.SetupModule("./_content/Microsoft.AspNetCore.Components.QuickGrid/QuickGrid.razor.js").SetupModule("init", _ => true);
		this.AddTestAuthorization();
	}
}