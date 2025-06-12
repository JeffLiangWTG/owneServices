using System;
using System.IO;
using System.Linq;
using eServices.eHubPortal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.JSInterop;
using Moq;

namespace eServices.eHubPortal.Tests.CodeMaps;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CodeMapsDetailsTests : Bunit.TestContext
{
	[Test]
	public void CodeMapsDetailsNotFound()
	{
		using var context = Services.SetupInMemoryDatabase();

		SetupCodeMapsDetails();

		var cut = RenderComponent<Components.Pages.CodeMaps.Details>();

		var navMan = Services.GetRequiredService<NavigationManager>();
		Assert.That(navMan.Uri, Is.EqualTo($"http://localhost/notfound"));
	}

	[Test]
	public void CodeMapsDetailsGeneral()
	{
		using var context = Services.SetupInMemoryDatabase();

		SetupCodeMapsDetails();
		context.LoadInMemoryData("Content\\CodeMaps\\ForwardToProxyGateway.json");

		var downloadHandler = JSInterop.SetupVoid("downloadFileFromStream", _ => true);

		var cut = RenderComponent<Components.Pages.CodeMaps.Details>(parameters => parameters.Add(p => p.Id, new Guid("5e57492c-6af2-4e82-8ad1-188ece0cda95")));

		Assert.Multiple(() =>
		{
			var table = cut.Find("tbody");
			var expected = HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsDetailsGeneral.htm");
			table.MarkupMatches(expected);

			var links = cut.FindAll("a");
			links.MarkupMatches("""
				<a href="/code-maps/">Back to List</a>
				""");

			var download = cut.Find("button");
			download.Click();
			var dlInvocation = downloadHandler.Invocations.First();

			Assert.That(dlInvocation.Arguments[0], Is.EqualTo("eHub-eHub-ForwardToProxyGateway-ForwardToProxyGateway.csv"));
			var csv = dlInvocation.Arguments[1] is DotNetStreamReference csvStream ? new StreamReader(csvStream.Stream).ReadToEnd() : string.Empty;
			Assert.That(csv, Is.EqualTo(HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsDetailsGeneral.csv")));
		});
	}

	[Test]
	public void CodeMapsDetailsOwner()
	{
		using var context = Services.SetupInMemoryDatabase();

		SetupCodeMapsDetails("OCM", true);
		context.LoadInMemoryData("Content\\CodeMaps\\ContainerQualityCode.json");

		var downloadHandler = JSInterop.SetupVoid("downloadFileFromStream", _ => true);

		var cut = RenderComponent<Components.Pages.CodeMaps.Details>(parameters => parameters.Add(p => p.Id, new Guid("7905ea8c-0bf4-426e-a363-b1addf7e33c3")));

		Assert.Multiple(() =>
		{
			var table = cut.Find("tbody");
			var expected = HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsDetailsOwner.htm");
			table.MarkupMatches(expected);

			var links = cut.FindAll("a");
			links.MarkupMatches("""
				<a href="/code-maps/edit/7905ea8c-0bf4-426e-a363-b1addf7e33c3">Edit</a>
				<a href="/code-maps/OCM">Back to List</a>
				""");

			var download = cut.Find("button");
			download.Click();
			var dlInvocation = downloadHandler.Invocations.First();

			Assert.That(dlInvocation.Arguments[0], Is.EqualTo("SHIPPING_INSTRUCTION-SHIPPING_INSTRUCTION-OCMSystemConfiguration-ContainerQualityCode.csv"));
			var csv = dlInvocation.Arguments[1] is DotNetStreamReference csvStream ? new StreamReader(csvStream.Stream).ReadToEnd() : string.Empty;
			Assert.That(csv, Is.EqualTo(HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsDetailsOwner.csv")));
		});
	}

	private void SetupCodeMapsDetails(string? owner = null, bool writeAuth = false)
	{
		var accessGroupService = new Mock<IAccessGroupService>();
		accessGroupService.Setup(x => x.IsAuthorizedForRead(owner)).ReturnsAsync(true);
		accessGroupService.Setup(x => x.IsAuthorizedForWrite(owner)).ReturnsAsync(writeAuth);
		Services.AddScoped(_ => accessGroupService.Object);
		Services.AddScoped(_ => new FakeLogger<Components.Pages.CodeMaps.Index>(NUnit.Framework.TestContext.WriteLine));
		JSInterop.SetupModule("./_content/Microsoft.AspNetCore.Components.QuickGrid/QuickGrid.razor.js").SetupModule("init", _ => true);
		JSInterop.SetupModule("./js/downloadFileFromStream.js").SetupModule("downloadFileFromStream", _ => true);
		this.AddTestAuthorization();
	}
}