using System;
using eServices.eHubPortal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace eServices.eHubPortal.Tests.CodeMaps;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CodeMapsEditTests : Bunit.TestContext
{
	[Test]
	public void CodeMapsEditNotFound()
	{
		using var context = Services.SetupInMemoryDatabase();

		SetupCodeMapsEdit();

		var cut = RenderComponent<Components.Pages.CodeMaps.Edit>();

		var navMan = Services.GetRequiredService<NavigationManager>();
		Assert.That(navMan.Uri, Is.EqualTo($"http://localhost/notfound"));
	}

	[Test]
	public void CodeMapsEditUpload()
	{
		using var context = Services.SetupInMemoryDatabase();

		SetupCodeMapsEdit("OCM");
		context.LoadInMemoryData("Content\\CodeMaps\\ContainerQualityCode.json");

		var cut = RenderComponent<Components.Pages.CodeMaps.Edit>(parameters => parameters.Add(p => p.Id, new Guid("7905ea8c-0bf4-426e-a363-b1addf7e33c3")));

		Assert.Multiple(() =>
		{
			cut.Find("tbody").MarkupMatches(HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsEditUploadBefore.htm"));

			cut.FindAll("a").MarkupMatches("""
				<a href="/code-maps/details/7905ea8c-0bf4-426e-a363-b1addf7e33c3">Details</a>
				<a href="/code-maps/OCM">Back to List</a>
				""");
		});

		JSInterop.SetupVoid("clickElement", _ => true).SetVoidResult();

		var fileToUpload = InputFileContent.CreateFromText(HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsEditUpload.csv"));
		IRenderedComponent<InputFile> inputFile = cut.FindComponent<InputFile>();
		inputFile.UploadFiles(fileToUpload);
		cut.WaitForState(() => cut.Find("tbody").InnerHtml.Contains("XXX"));

		cut.Find("tbody").MarkupMatches(HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsEditUploadAfter.htm"));
	}

	[Test]
	public void CodeMapsEditEdit()
	{
		using var context = Services.SetupInMemoryDatabase();

		SetupCodeMapsEdit("OCM");
		context.LoadInMemoryData("Content\\CodeMaps\\ContainerQualityCode.json");

		JSInterop.SetupVoid("clickElement", _ => true).SetVoidResult();

		var cut = RenderComponent<Components.Pages.CodeMaps.Edit>(parameters => parameters.Add(p => p.Id, new Guid("7905ea8c-0bf4-426e-a363-b1addf7e33c3")));
		cut.Find("#itemEdit").Click();

		var itemModal = cut.Find("#itemModal");
		itemModal.MarkupMatches(HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsEditEdit.htm"));

		var input = cut.FindAll("input")[4];
		input.Change("XXX");
		cut.Find("button[type=submit]").Click();

		cut.Find("tbody").MarkupMatches(HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsEditEditAfter.htm"));
	}

	[Test]
	public void CodeMapsEditInsert()
	{
		using var context = Services.SetupInMemoryDatabase();

		SetupCodeMapsEdit("OCM");
		context.LoadInMemoryData("Content\\CodeMaps\\ContainerQualityCode.json");

		JSInterop.SetupVoid("clickElement", _ => true).SetVoidResult();

		var cut = RenderComponent<Components.Pages.CodeMaps.Edit>(parameters => parameters.Add(p => p.Id, new Guid("7905ea8c-0bf4-426e-a363-b1addf7e33c3")));
		cut.Find("#itemInsert").Click();

		var itemModal = cut.Find("#itemModal");
		itemModal.MarkupMatches(HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsEditInsert.htm"));

		cut.FindAll("input")[1].Change("AAA");
		cut.FindAll("input")[2].Change("BBB");
		cut.FindAll("input")[3].Change("CCC");
		cut.FindAll("input")[4].Change("DDD");
		cut.Find("button[type=submit]").Click();

		cut.Find("tbody").MarkupMatches(HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsEditInsertAfter.htm"));
	}

	[Test]
	public void CodeMapsEditDelete()
	{
		using var context = Services.SetupInMemoryDatabase();

		SetupCodeMapsEdit("OCM");
		context.LoadInMemoryData("Content\\CodeMaps\\ContainerQualityCode.json");

		JSInterop.SetupVoid("clickElement", _ => true).SetVoidResult();

		var cut = RenderComponent<Components.Pages.CodeMaps.Edit>(parameters => parameters.Add(p => p.Id, new Guid("7905ea8c-0bf4-426e-a363-b1addf7e33c3")));
		cut.Find("#itemDelete").Click();

		var itemModal = cut.Find("#itemModal");
		itemModal.MarkupMatches(HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsEditDelete.htm"));

		cut.Find("button[type=submit]").Click();

		cut.Find("tbody").MarkupMatches(HelperExtensions.GetContentFileAsString("Content\\CodeMaps\\CodeMapsEditDeleteAfter.htm"));
	}

	private void SetupCodeMapsEdit(string? owner = null, bool writeAuth = true)
	{
		var accessGroupService = new Mock<IAccessGroupService>();
		accessGroupService.Setup(x => x.IsAuthorizedForWrite(owner)).ReturnsAsync(writeAuth);
		Services.AddScoped(_ => accessGroupService.Object);
		Services.AddScoped(_ => new FakeLogger<Components.Pages.CodeMaps.Index>(NUnit.Framework.TestContext.WriteLine));
		JSInterop.SetupModule("./_content/Microsoft.AspNetCore.Components.QuickGrid/QuickGrid.razor.js").SetupModule("init", _ => true);
		this.AddTestAuthorization();
	}
}