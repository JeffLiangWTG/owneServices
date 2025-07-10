using AngleSharp.Dom;
using Bunit;
using CargoWise.Main.Navigation.JsInterop;
using CargoWise.Main.Navigation.Pages;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test;

public class QuickSearchTest : BunitTestContext
{
	[SetUp]
	public override void Setup()
	{
		base.Setup();
		ViewModelWithNotification.IsInUnitTesting = true;
	}

	[TearDown]
	public override void TearDown()
	{
		base.TearDown();
		ViewModelWithNotification.IsInUnitTesting = false;
	}

	[Test]
	public void QuickSearch_RenderTest()
	{
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		Services.AddSingleton(new Mock<IQuickSearchJsInterop>().Object);
		var navigationViewModel = new NavigationViewModel();
		var cut = RenderComponent<QuickSearch>(parameters =>
					parameters.Add(p => p.NavigationViewModel, navigationViewModel));

		Assert.That(cut.Find(".cwn-quick-search").Children.Length, Is.EqualTo(2));
		Assert.That(cut.FindAll(".cwn-search--input").Count, Is.EqualTo(1));
		Assert.That(cut.FindAll(".cwn-search--input")[0]?.Attributes["placeholder"]?.Value, Is.EqualTo("Search CargoWise Next"));
	}

	[Test]
	public async Task QuickSearch_WhenHasNoResultAsync()
	{
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		Services.AddSingleton(new Mock<IQuickSearchJsInterop>().Object);
		var navigationViewModel = new NavigationViewModel();
		InitializeSearch(navigationViewModel);

		var cut = RenderComponent<QuickSearch>(parameters =>
							parameters.Add(p => p.NavigationViewModel, navigationViewModel));

		cut.Find(".cwn-search--input").Focus();
		navigationViewModel.IsInSearchMode = true;
		navigationViewModel.SearchViewModel.SearchValue = "testing... testing...";

		cut.WaitForState(() => !navigationViewModel.SearchViewModel.IsBusy, TimeSpan.FromSeconds(10));
		await cut.InvokeAsync(() => cut.Render());
		await Task.Delay(1000);

		Assert.That(cut.FindAll(".cwn-quick-search__footer").Count, Is.EqualTo(1));
		Assert.That(cut.FindAll("cwn-quick-search__result-section").Count, Is.EqualTo(0));
		Assert.That(cut.FindAll(".cwn-quick-search__result-item").Count, Is.EqualTo(0));
		Assert.That(cut.FindAll(".cwn-quick-search__no-result-hint").Count, Is.EqualTo(1));
		Assert.That(cut.FindAll(".cwn-quick-search__no-result-hint-info").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task QuickSearch_WhenHasSeveralResultsAsync()
	{
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		Services.AddSingleton(new Mock<IQuickSearchJsInterop>().Object);
		var navigationViewModel = new NavigationViewModel();
		InitializeSearch(navigationViewModel);

		var cut = RenderComponent<QuickSearch>(parameters =>
							parameters.Add(p => p.NavigationViewModel, navigationViewModel));

		cut.Find(".cwn-search--input").Focus();
		Assert.That(cut.FindAll(".cwn-quick-search__footer").Count, Is.EqualTo(1));
		Assert.That(cut.FindAll("cwn-quick-search__result-section").Count, Is.EqualTo(0));

		navigationViewModel.IsInSearchMode = true;
		navigationViewModel.SearchViewModel.SearchValue = "t";

		cut.WaitForState(() => navigationViewModel.SearchViewModel.SearchResults.Count == 4, TimeSpan.FromSeconds(5));
		await cut.InvokeAsync(() => cut.Render());

		Assert.That(cut.FindAll(".cwn-quick-search__result-section").Count, Is.EqualTo(4));
		Assert.That(cut.Find(".cwn-quick-search__result-section .cwn-quick-search__result-title").Text, Is.EqualTo("Favorites (1)"));
		Assert.That(cut.Find(".cwn-quick-search__result-section .cwn-quick-search__result-title span").Text, Is.EqualTo("(1)"));

		Assert.That(cut.FindAll(".cwn-quick-search__result-item").Count, Is.EqualTo(7));

		var firstItem = cut.FindAll(".cwn-quick-search__result-item")[0];
		Assert.That(firstItem.InnerHtml,
						Does.Contain("cwn-icon--star-filled"));
		var lastItem = cut.FindAll(".cwn-quick-search__result-item")[6];
		Assert.That(lastItem.InnerHtml,
									Does.Contain("cwn-icon--vector"));
	}

	[Test]
	public async Task QuickSearch_ClearsSearchResultsWhenInputIsClearedAsync()
	{
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		Services.AddSingleton(new Mock<IQuickSearchJsInterop>().Object);
		var navigationViewModel = new NavigationViewModel();
		InitializeSearch(navigationViewModel);

		var cut = RenderComponent<QuickSearch>(parameters =>
							parameters.Add(p => p.NavigationViewModel, navigationViewModel));

		var input = cut.Find(".cwn-search--input");
		input.Focus();

		var changeEvent = new ChangeEventArgs { Value = "t" };
		await input.InputAsync(changeEvent);

		cut.WaitForState(() => navigationViewModel.SearchViewModel.SearchResults.Count == 4, TimeSpan.FromSeconds(5));

		Assert.That(navigationViewModel.SearchViewModel.SearchResults.Count, Is.EqualTo(4));
		Assert.That(cut.FindAll(".cwn-quick-search__result-item").Count, Is.EqualTo(7));

		changeEvent = new ChangeEventArgs { Value = string.Empty };
		await input.InputAsync(changeEvent);

		await Task.Delay(500);

		Assert.That(navigationViewModel.SearchViewModel.SearchValue, Is.EqualTo(string.Empty));
		Assert.That(navigationViewModel.SearchViewModel.SearchResults.Count, Is.EqualTo(0));
		Assert.That(cut.FindAll(".cwn-quick-search__result-item").Count, Is.EqualTo(0));
	}

	[Test]
	public void QuickSearch_ShortcutKey()
	{
		var mockPopoverService = new Mock<IPopoverService>();
		var mockQuickSearchJsInterop = new Mock<IQuickSearchJsInterop>();
		Services.AddSingleton(mockPopoverService.Object);
		Services.AddSingleton(mockQuickSearchJsInterop.Object);
		var navigationViewModel = new NavigationViewModel();
		InitializeSearch(navigationViewModel);

		var searchPopoverId = "search-popover-" + Guid.NewGuid().ToString();
		var cut = RenderComponent<QuickSearch>(parameters =>
			{
				parameters.Add(p => p.NavigationViewModel, navigationViewModel);
				parameters.Add(p => p.SearchPopoverId, searchPopoverId);
			});

		mockQuickSearchJsInterop.Verify(x => x.RegisterFocusShortcutAsync(It.IsAny<DotNetObjectReference<QuickSearch>>()), Times.Once);
		navigationViewModel.IsInSearchMode = true;
		navigationViewModel.SearchViewModel.SearchValue = "t";

		cut.WaitForState(() => navigationViewModel.SearchViewModel.SearchResults.Count == 4, TimeSpan.FromSeconds(5));
		cut.WaitForState(() => !navigationViewModel.SearchViewModel.IsBusy, TimeSpan.FromSeconds(5));

		var shortCutComponent = cut.FindAll(".cwn-search > .cwn-search--shortcut > .cwn-search-shortcut");
		Assert.That(shortCutComponent[0].InnerHtml, Is.EqualTo("<span>CTRL</span>"));
		Assert.That(shortCutComponent[1].InnerHtml, Is.EqualTo("<span>Q</span>"));
		mockQuickSearchJsInterop.Verify(x => x.UpdateResultsAsync(searchPopoverId), Times.Once);
	}

	[Test]
	public async Task QuickSearch_ResultSectionNameRenderTestAsync()
	{
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		Services.AddSingleton(new Mock<IQuickSearchJsInterop>().Object);
		var navigationViewModel = new NavigationViewModel();
		InitializeSearch(navigationViewModel);

		var cut = RenderComponent<QuickSearch>(parameters =>
							parameters.Add(p => p.NavigationViewModel, navigationViewModel));

		cut.Find(".cwn-search--input").Focus();
		navigationViewModel.IsInSearchMode = true;
		navigationViewModel.SearchViewModel.SearchValue = "t";

		cut.WaitForState(() => navigationViewModel.SearchViewModel.SearchResults.Count == 4, TimeSpan.FromSeconds(5));
		await cut.InvokeAsync(() => cut.Render());

		Assert.That(cut.FindAll(".cwn-quick-search__result-section").Count, Is.EqualTo(4));
		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll(".cwn-quick-search__result-section > .cwn-quick-search__result-title")[0].Text, Is.EqualTo("Favorites (1)"));
			Assert.That(cut.FindAll(".cwn-quick-search__result-section > .cwn-quick-search__result-title")[1].Text, Is.EqualTo("Recent Items (2)"));
			Assert.That(cut.FindAll(".cwn-quick-search__result-section > .cwn-quick-search__result-title")[2].Text, Is.EqualTo("Recent Modules (2)"));
			Assert.That(cut.FindAll(".cwn-quick-search__result-section > .cwn-quick-search__result-title")[3].Text, Is.EqualTo("Test (2)"));
		});
	}

	[Test]
	public async Task QuickSearch_ClearValueWhenPopoverClosedAsync()
	{
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		Services.AddSingleton(new Mock<IQuickSearchJsInterop>().Object);
		var navigationViewModel = new NavigationViewModel();
		InitializeSearch(navigationViewModel);

		var cut = RenderComponent<QuickSearch>(parameters =>
							parameters.Add(p => p.NavigationViewModel, navigationViewModel));

		var input = cut.Find(".cwn-search--input");
		input.Focus();

		var changeEvent = new ChangeEventArgs();
		changeEvent.Value = "t";
		await input.InputAsync(changeEvent);
		Assert.That(() => navigationViewModel.IsInSearchMode, Is.True.After(2000, 100));
		Assert.That(() => navigationViewModel.SearchViewModel.SearchValue, Is.EqualTo("t").After(2000, 100));

		var popover = cut.FindComponent<CwnPopover>();
		await cut.InvokeAsync(() => popover.Instance.OnToggle.InvokeAsync(false));

		var inputComponent = cut.FindComponent<CwnTextField>();
		Assert.That(inputComponent.Instance.Value, Is.EqualTo(string.Empty));
		Assert.That(() => navigationViewModel.IsInSearchMode, Is.False.After(2000, 100));
		Assert.That(() => navigationViewModel.SearchViewModel.SearchValue, Is.EqualTo(string.Empty).After(2000, 100));
	}

	[Test]
	public async Task QuickSearch_NextAppBarUserControlAsync()
	{
		using var context = new EnterpriseTestContext();
		var navigationViewModel = new NavigationViewModel();
		InitializeSearch(navigationViewModel);

		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var appBar = new NextAppBarUserControl(navigationViewModel);
			return appBar;
		});
		var quickSearch = cut.FindComponent<QuickSearch>();
		var searchPopoverId = quickSearch.Instance.SearchPopoverId;

		navigationViewModel.IsInSearchMode = true;
		navigationViewModel.SearchViewModel.SearchValue = "t";

		cut.WaitForState(() => navigationViewModel.SearchViewModel.SearchResults.Count == 4, TimeSpan.FromSeconds(5));
		cut.WaitForState(() => !navigationViewModel.SearchViewModel.IsBusy, TimeSpan.FromSeconds(5));

		Assert.That(searchPopoverId, Is.Not.Null);
		var resultCount = cut.FindAll(".cwn-quick-search__result-item-text").Count;
		Assert.That(resultCount, Is.EqualTo(7));
	}

	[Test]
	public async Task TwoNextAppBarUserControlShareTheSameModelAsync()
	{
		using var context = new EnterpriseTestContext();
		var navigationViewModel = new NavigationViewModel();
		InitializeSearch(navigationViewModel);

		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var appBar = new NextAppBarUserControl(navigationViewModel);
			return appBar;
		});

		var cut1 = await context.RenderControlOnFormAsync(() =>
		{
			var appBar = new NextAppBarUserControl(navigationViewModel);
			return appBar;
		});
		var quickSearch = cut.FindComponent<QuickSearch>();
		var searchPopoverId = quickSearch.Instance.SearchPopoverId;

		var quickSearch1 = cut1.FindComponent<QuickSearch>();
		var searchPopoverId1 = quickSearch1.Instance.SearchPopoverId;

		navigationViewModel.IsInSearchMode = true;
		navigationViewModel.SearchViewModel.SearchValue = "t";
		cut.WaitForState(() => navigationViewModel.SearchViewModel.SearchResults.Count == 4, TimeSpan.FromSeconds(5));
		cut.WaitForState(() => !navigationViewModel.SearchViewModel.IsBusy, TimeSpan.FromSeconds(5));

		Assert.That(searchPopoverId, Is.Not.Null);
		var resultCount = cut.FindAll(".cwn-quick-search__result-item-text").Count;
		Assert.That(resultCount, Is.EqualTo(7));

		Assert.That(searchPopoverId1, Is.Not.Null);
		var resultCount1 = cut1.FindAll(".cwn-quick-search__result-item-text").Count;
		Assert.That(resultCount1, Is.EqualTo(7));
	}

	[Test]
	public void WhenOnInitialized_ShouldRegisterFocusOutHandler()
	{
		var mockQuickSearchJsInterop = new Mock<IQuickSearchJsInterop>();
		Services.AddSingleton(Mock.Of<IPopoverService>());
		Services.AddSingleton(mockQuickSearchJsInterop.Object);
		var navigationViewModel = new NavigationViewModel();

		var cut = RenderComponent<QuickSearch>(parameters =>
			parameters.Add(p => p.NavigationViewModel, navigationViewModel));

		mockQuickSearchJsInterop.Verify(x => x.RegisterFocusOutHandlerAsync(
			It.IsAny<DotNetObjectReference<QuickSearch>>(),
			It.IsAny<ElementReference?>(),
			nameof(QuickSearch.HandleFocusOutAsync)), Times.Once);
	}

	[Test]
	public async Task WhenHandleFocusOut_ShouldCallPopoverServiceHide()
	{
		var expectedSearchPopoverId = Guid.NewGuid().ToString();
		var mockPopoverService = new Mock<IPopoverService>();
		Services.AddSingleton(mockPopoverService.Object);
		Services.AddSingleton(Mock.Of<IQuickSearchJsInterop>());
		var navigationViewModel = new NavigationViewModel();
		var cut = RenderComponent<QuickSearch>(parameters =>
			parameters
				.Add(p => p.NavigationViewModel, navigationViewModel)
				.Add(p => p.SearchPopoverId, expectedSearchPopoverId));

		await cut.InvokeAsync(() => cut.Instance.HandleFocusOutAsync());

		mockPopoverService.Verify(x => x.HideAsync(expectedSearchPopoverId), Times.Once);
	}

	void InitializeSearch(NavigationViewModel navigationViewModel)
	{
		var favoritesSection = new MenuSection("Favorites", "Favorites", (NoResString)"Favorites", SectionType.Favorite);
		favoritesSection.Items.Add(new MenuItem("Favorite 1", (NoResString)"Favorite 1"));

		var recentItemsSection = new MenuSection("Recent Items", "Recent Items", (NoResString)"Recent Items", SectionType.RecentItem);
		recentItemsSection.Items.Add(new MenuItem("Recent Item 1", (NoResString)"Recent Item 1"));
		recentItemsSection.Items.Add(new MenuItem("Recent Item 2", (NoResString)"Recent Item 2"));

		var recentModuleSection = new MenuSection("Recent Modules", "Recent Modules", (NoResString)"Recent Modules", SectionType.RecentModule);
		recentModuleSection.Items.Add(new MenuItem("Recent Module 1", (NoResString)"Recent Module 1"));
		recentModuleSection.Items.Add(new MenuItem("Recent Module 2", (NoResString)"Recent Module 2"));

		var testSection = new MenuSection("Test", "Test", (NoResString)"Test", SectionType.Module);
		testSection.Items.Add(new MenuItem("Test 1", (NoResString)"Test 1"));
		testSection.Items.Add(new MenuItem("Test 2", (NoResString)"Test 2"));

		var menu = new NavigationMenuViewModel((NoResString)"Jump", "Jump", 1, true);
		menu.Buttons.Add(favoritesSection);
		menu.Buttons.Add(recentItemsSection);
		menu.Buttons.Add(recentModuleSection);
		menu.Buttons.Add(testSection);

		navigationViewModel.AddCategory(menu);
		navigationViewModel.InitializeSearch(null);
	}
}
