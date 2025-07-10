
using Bunit;
using CargoWise.Main.Navigation.JsInterop;
using CargoWise.Main.Navigation.Pages;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace CargoWise.Main.Navigation.Winzor.Test;

internal class NextAppBarTest : BunitTestContext
{
	[Test]
	public void TestNextAppBar()
	{
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		Services.AddSingleton(new Mock<IQuickSearchJsInterop>().Object);
		Services.AddSingleton(new Mock<INavBarToolStripMenuInterop>().Object);

		var cut = RenderComponent<NextAppBar>(parameters =>
			parameters.Add(p => p.NavigationViewModel, new MockMainPageModelService().NavigationViewModel));

		var headNav = cut.FindComponent<HeadNav>();
		var quickSearch = cut.FindComponent<QuickSearch>();
		var navBarToolStrip = cut.FindComponent<NavBarToolStrip>();
		Assert.Multiple(() =>
		{
			Assert.That(headNav, Is.Not.Null);
			Assert.That(quickSearch, Is.Not.Null);
			Assert.That(navBarToolStrip, Is.Not.Null);
		});
	}

	[Test, WithPlaywrightPage]
	public async Task ArrowKeysNavigateText()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		Services.AddSingleton(new Mock<IPopoverService>().Object);
		Services.AddSingleton(new Mock<IQuickSearchJsInterop>().Object);
		Services.AddSingleton(new Mock<INavBarToolStripMenuInterop>().Object);

		var (page, component) = await ctx.LoadComponentOnFormAsync<NextAppBar>(parameters =>
		{
			parameters.Add(p => p.NavigationViewModel, new MockMainPageModelService().NavigationViewModel);
		});

		var input = await page.WaitForSelectorAsync(".cwn-search--input");

		await input!.ClickAsync();
		await input.FillAsync("Hello world!!");

		var value = input.InputValueAsync().Result;
		Assert.That(value, Is.EqualTo("Hello world!!"));

		await input.PressAsync("Delete");

		value = input.InputValueAsync().Result;
		Assert.That(value, Is.EqualTo("ello world!!"));

		await input.PressAsync("ArrowRight");
		await input.PressAsync("ArrowRight");
		await input.PressAsync("Delete");

		value = input.InputValueAsync().Result;
		Assert.That(value, Is.EqualTo("elo world!!"));

		await input.PressAsync("ArrowLeft");
		await input.PressAsync("Delete");

		value = input.InputValueAsync().Result;
		Assert.That(value, Is.EqualTo("eo world!!"));
	}

	public class MockMainPageModelService : IMainPageModelService
	{
		public NavigationViewModel? NavigationViewModel { get; }
		public SessionContextViewModel? SessionContextViewModel { get; }
		public INewsViewModel? NewsViewModelTop { get; set; }
		public INewsViewModel? NewsViewModelBottom { get; set; }
		public IMyTasksViewModel? MyTasksViewModel { get; set; }
		public RecentMessagesViewModel? RecentMessagesViewModel { get; init; }

		public bool IsSnapshotsEnabled => true;

		public Task InvokeAsync(Action action)
		{
			throw new NotImplementedException();
		}
	}
}

