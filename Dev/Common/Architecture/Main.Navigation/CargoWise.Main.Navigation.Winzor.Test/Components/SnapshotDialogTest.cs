using AngleSharp.Dom;
using Bunit;
using CargoWise.Main.Navigation.Pages;
using CargoWiseNext.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test.Components;

public class SnapshotDialogTest : BunitTestContext
{
	Mock<ISnapshotsService> mockService;

	[SetUp]
	public void SetUp()
	{
		mockService = new Mock<ISnapshotsService>();
		mockService.Setup(s => s.FindModulesAsync()).ReturnsAsync([]);
		mockService.Setup(s => s.FindModuleFiltersAsync(It.IsAny<SnapshotModule>())).ReturnsAsync([]);
	}

	[Test]
	public async Task SnapshotDialog_ShowModal_Delegates_to_Native_JS()
	{
		var cut = RenderComponent<SnapshotDialog>(parameters => parameters
			.AddCascadingValue(mockService.Object));
		var jsInterop = JSInterop.SetupVoid("showModal", _ => true).SetVoidResult();

		await cut.Instance.ShowModalAsync();

		jsInterop.VerifyInvoke("showModal");
	}

	[Test]
	public async Task SnapshotDialog_Close_Delegates_to_Native_JS()
	{
		var cut = RenderComponent<SnapshotDialog>();
		var jsInterop = JSInterop.SetupVoid("close", _ => true).SetVoidResult();

		await cut.Instance.CloseAsync();

		jsInterop.VerifyInvoke("close");
	}

	[Test]
	public async Task SnapshotDialog_IsOpen_Delegates_to_Native_JS([Values(true, false)] bool expectedResult)
	{
		var cut = RenderComponent<SnapshotDialog>();
		JSInterop
			.Setup<bool>("isOpen", _ => true)
			.SetResult(expectedResult);

		var isOpen = await cut.Instance.IsOpenAsync();

		Assert.That(isOpen, Is.EqualTo(expectedResult));
	}

	[Test]
	public void SnapshotDialog_ClassContent()
	{
		var cut = RenderComponent<SnapshotDialog>(parameters => parameters
			.Add(c => c.ClassContent, "class-test")
		);

		var uut = cut.Find(".class-test");

		Assert.That(uut.GetAttribute("class"), Is.EqualTo("cwn-dialog__content class-test"));
	}

	[Test]
	public async Task SnapshotDialog_OnCancelClickAsync()
	{
		var count = 0;
		var cut = RenderComponent<SnapshotDialog>(parameters => parameters
			.Add(c => c.OnCancelClick, EventCallback.Factory.Create(this, s => count++))
		);

		var button = cut.Find("[data-testid='cancel-btn']");
		await button.ClickAsync(new MouseEventArgs());

		Assert.That(count, Is.EqualTo(1));
	}

	[Test]
	public async Task SnapshotDialog_OnAddSnapshotClickAsync()
	{
		mockService.Setup(s => s.FindModulesAsync()).ReturnsAsync([new SnapshotModule() { ModuleName = "Work Item" }]);
		mockService.Setup(s => s.FindModuleFiltersAsync(It.IsAny<SnapshotModule>())).ReturnsAsync([new SnapshotModuleFilter { ModuleName = "Work Item", ModuleFilterName = "Done" }]);

		var snapshots = new List<Snapshot>();
		var cut = RenderComponent<SnapshotDialog>(parameters => parameters
			.AddCascadingValue(mockService.Object)
			.Add(c => c.SelectedModuleName, "Work Item")
			.Add(c => c.SelectedModuleFilterName, "Done")
			.Add(c => c.OnAddSnapshotClick, EventCallback.Factory.Create<Snapshot>(this, s => snapshots.Add(s)))
		);

		JSInterop.SetupVoid("showModal", _ => true).SetVoidResult();
		JSInterop.SetupVoid("close", _ => true).SetVoidResult();

		await cut.Instance.ShowModalAsync();
		cut.Render();

		await Assert.MultipleAsync(async () =>
		{
			var inputs = cut.FindAll("input");
			Assert.That(inputs, Has.Count.EqualTo(2));
			Assert.That(inputs[0].GetAttribute("value"), Is.EqualTo("Work Item"));
			Assert.That(inputs[1].GetAttribute("value"), Is.EqualTo("Done"));
			Assert.That(inputs[1].IsDisabled, Is.False);

			var button = cut.Find("[data-testid='ok-btn']");
			Assert.That(button.IsDisabled, Is.False);

			await button.ClickAsync(new MouseEventArgs());

			Assert.That(snapshots, Has.Count.EqualTo(1));
		});
	}

	[Test]
	public async Task SnapshotDialog_WhenModuleFiltersNotFoundAsync()
	{
		var modules = new List<SnapshotModule>();
		mockService.Setup(s => s.OpenModuleAsync(It.IsAny<SnapshotModule>())).Callback<SnapshotModule>(modules.Add);
		mockService.Setup(s => s.FindModulesAsync()).ReturnsAsync([new SnapshotModule() { ModuleName = "Work Item" }]);
		mockService.Setup(s => s.FindModuleFiltersAsync(It.IsAny<SnapshotModule>())).ReturnsAsync([]);
		mockService.SetupGet(s => s.HowToCreateSnapshotText).Returns("How to create snapshot text.");
		mockService.SetupGet(s => s.HowToCreateSnapshotLinkText).Returns("How to create snapshot link text.");

		var cut = RenderComponent<SnapshotDialog>(parameters => parameters
			.AddCascadingValue(mockService.Object)
			.Add(c => c.SelectedModuleName, "Work Item")
		);

		JSInterop.SetupVoid("showModal", _ => true).SetVoidResult();
		JSInterop.SetupVoid("close", _ => true).SetVoidResult();

		await cut.Instance.ShowModalAsync();
		cut.Render();

		var inputs = cut.FindAll("input");

		Assert.Multiple(() =>
		{
			Assert.That(inputs, Has.Count.EqualTo(1));
			Assert.That(inputs[0].GetAttribute("value"), Is.EqualTo("Work Item"));

			Assert.That(cut.Find(".cwn-callout").GetAttribute("class"), Does.Contain("cwn-callout--info"));
			Assert.That(cut.Find(".cwn-callout__icon--info").GetAttribute("class"), Does.Contain("cwn-icon--status-info"));
			Assert.That(cut.Find(".cwn-callout__content").TextContent, Does.Contain("How to create snapshot text."));
			Assert.That(cut.Find(".cwn-snapshots__dialog-link").TextContent, Does.Contain("How to create snapshot link text."));

			Assert.That(cut.Find("[data-testid='ok-btn']").IsDisabled, Is.True);
		});
	}

	[Test]
	public async Task SnapshotDialog_WhenModuleFiltersNotFound_WhenOpenModuleClickAsync()
	{
		var modules = new List<SnapshotModule>();
		mockService.Setup(s => s.OpenModuleAsync(It.IsAny<SnapshotModule>())).Callback<SnapshotModule>(modules.Add);
		mockService.Setup(s => s.FindModulesAsync()).ReturnsAsync([new SnapshotModule() { ModuleName = "Work Item" }]);
		mockService.Setup(s => s.FindModuleFiltersAsync(It.IsAny<SnapshotModule>())).ReturnsAsync([]);

		var cut = RenderComponent<SnapshotDialog>(parameters => parameters
			.AddCascadingValue(mockService.Object)
			.Add(c => c.SelectedModuleName, "Work Item")
		);

		JSInterop.SetupVoid("showModal", _ => true).SetVoidResult();
		JSInterop.SetupVoid("close", _ => true).SetVoidResult();

		await cut.Instance.ShowModalAsync();
		cut.Render();

		var link = cut.Find(".cwn-snapshots__dialog-link");

		Assert.That(modules, Is.Empty);

		link.Click();

		Assert.That(modules, Has.Count.EqualTo(1));
		Assert.That(modules[0].ModuleName, Is.EqualTo("Work Item"));
	}

	[Test]
	public void SnapshotDialog_Buttons()
	{
		var cut = RenderComponent<SnapshotDialog>(parameters => parameters
			.Add(c => c.OkButtonText, "OK caption")
			.Add(c => c.CancelButtonText, "Cancel caption")
		);

		var buttons = cut.FindAll("button");

		Assert.Multiple(() =>
		{
			Assert.That(buttons, Has.Count.EqualTo(2));
			Assert.That(buttons[0].TextContent, Does.StartWith("OK caption"));
			Assert.That(buttons[0].IsDisabled, Is.True);
			Assert.That(buttons[0].GetAttribute("class"), Does.Contain("cwn-snapshots__dialog-button"));
			Assert.That(buttons[1].TextContent, Does.StartWith("Cancel caption"));
			Assert.That(buttons[1].IsDisabled, Is.False);
			Assert.That(buttons[1].GetAttribute("class"), Does.Contain("cwn-snapshots__dialog-button"));
		});
	}

	[Test]
	public void SnapshotDialog_Labels()
	{
		mockService.SetupGet(s => s.ModuleLabel).Returns("Module Label");
		mockService.SetupGet(s => s.ModuleFilterLabel).Returns("Layout Label");

		var cut = RenderComponent<SnapshotDialog>(parameters => parameters
			.AddCascadingValue(mockService.Object)
		);

		var labels = cut.FindAll("label");

		Assert.Multiple(() =>
		{
			Assert.That(labels, Has.Count.EqualTo(2));
			Assert.That(labels[0].TextContent, Does.StartWith("Module Label"));
			Assert.That(labels[1].TextContent, Does.StartWith("Layout Label"));
		});
	}

	[Test]
	public void SnapshotDialog_Placeholders()
	{
		mockService.SetupGet(s => s.ModulePlaceholder).Returns("Module placeholder");
		mockService.SetupGet(s => s.ModuleFilterPlaceholder).Returns("Layout placeholder");

		var cut = RenderComponent<SnapshotDialog>(parameters => parameters
			.AddCascadingValue(mockService.Object)
		);

		var inputs = cut.FindAll("input");

		Assert.Multiple(() =>
		{
			Assert.That(inputs, Has.Count.EqualTo(2));
			Assert.That(inputs[0].GetAttribute("placeholder"), Is.EqualTo("Module placeholder"));
			Assert.That(inputs[1].GetAttribute("placeholder"), Is.EqualTo("Layout placeholder"));
		});
	}

	[Test]
	public void SnapshotDialog_WhenEscapeKeyPressed()
	{
		mockService.SetupGet(s => s.ModulePlaceholder).Returns("Module placeholder");
		mockService.SetupGet(s => s.ModuleFilterPlaceholder).Returns("Layout placeholder");
		var keyboardService = new KeyboardService();

		var canceled = false;
		RenderComponent<SnapshotDialog>(parameters => parameters
			.AddCascadingValue(keyboardService)
			.Add(c => c.OnCancelClick, EventCallback.Factory.Create(this, () => canceled = true))
		);

		keyboardService.NotifyKeyDown(null!, new KeyboardEventArgs { Code = "Escape" });

		Assert.That(canceled, Is.True);
	}
}
