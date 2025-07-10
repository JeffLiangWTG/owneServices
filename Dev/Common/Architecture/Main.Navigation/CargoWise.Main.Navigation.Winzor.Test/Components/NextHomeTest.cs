using System.Collections.ObjectModel;
using System.Windows.Forms;
using Bunit;
using CargoWise.Main.Navigation.Pages;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Windows.UI;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

#nullable enable
namespace CargoWise.Main.Navigation.Winzor.Test;

public class NextHomeTest : BunitTestContext
{
	[Test]
	public async Task HomeRenderTestAsync()
	{
		// Setup
		using var context = new EnterpriseTestContext();

		// Act
		var cut = await context.RenderControlOnFormAsync(() => new NextHomeUserControl());

		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll(".cwn-layout").Count, Is.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-layout__main-content").Count, Is.EqualTo(1));

			Assert.That(cut.FindAll(".cwn-home__favorites").Count, Is.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-home__recent-modules").Count, Is.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-home__main__content").Count, Is.EqualTo(2));
			Assert.That(cut.FindAll(".cwn-home__top").Count, Is.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-home__bottom").Count, Is.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-home__news").Count, Is.EqualTo(0));
			Assert.That(cut.FindAll(".cwn-layout__footer").Count, Is.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-home__widget").Count, Is.EqualTo(3));
		});
	}

	[Test]
	public async Task HomeRenderWithThemeAsync()
	{
		// Setup
		var mockColorTheme = new Mock<IColorTheme>();
		mockColorTheme.Setup((m) => m.MainFormBackgroundColor).Returns(System.Drawing.Color.Blue);
		mockColorTheme.Setup((m) => m.TitleBarBackground).Returns(System.Drawing.Color.Yellow);
		mockColorTheme.Setup((m) => m.TitleBarText).Returns(System.Drawing.Color.Red);
		mockColorTheme.Setup((m) => m.NavBarRecentPanelBackground).Returns(System.Drawing.Color.Green);

		using var context = new EnterpriseTestContext();

		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var control = new NextHomeUserControl();
			control.SetTheme(mockColorTheme.Object);
			return control;
		});

		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll("Style").Count, Is.EqualTo(1));
			var styleMarkup = cut.Find("Style").ToMarkup();
			Assert.That(styleMarkup, Does.Contain("--s-brand-bg-default:rgba(255, 255, 0, 1);"));
			Assert.That(styleMarkup, Does.Contain("--s-brand-txt-inv-hover:rgba(255, 255, 0, 1);"));
			Assert.That(styleMarkup, Does.Contain("--s-brand-txt-inv-active:rgba(255, 255, 0, 1);"));
			Assert.That(styleMarkup, Does.Contain("--s-brand-txt-inv-default:rgba(255, 0, 0, 1);"));
			//Assert.That(styleMarkup, Does.Contain("--s-neutral-bg-weak-default:rgba(0, 0, 255, 1);"));
			//Assert.That(styleMarkup, Does.Contain("--s-neutral-bg-default:rgba(0, 128, 0, 1);"));
		});
	}

	[TestCase(SectionType.Favorite, "favorites", "", TestName = "{m}_Favorites")]
	public async Task HomeSectionsAsync(SectionType section, string sectionClass, string noDataError)
	{
		// setup
		using var context = new EnterpriseTestContext();
		var section1 = new MenuSection("section1", "section1", (NoResString)"section1", section);
		var navigationViewModel = new NavigationViewModel();
		var menu = new NavigationMenuViewModel((NoResString)"Jump", "Jump", 1, true);

		if (string.IsNullOrEmpty(noDataError))
		{
			navigationViewModel.AddCategory(menu);
			menu.Buttons.Add(section1);
			section1.Items.Add(new MenuItem("item1", (NoResString)"item1"));
		}

		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();
			home.NavigationViewModel = navigationViewModel;

			return home;
		});

		// Assert
		if (string.IsNullOrEmpty(noDataError))
		{
			Assert.That(cut.FindAll($".cwn-home__{sectionClass} ul li").Count, Is.EqualTo(1));
		}
		else
		{
			Assert.That(cut.Markup, Does.Contain(noDataError));
		}
	}

	[Test]
	public async Task HomeRecentModules_WhenHasNoItemsAsync()
	{
		// setup
		using var context = new EnterpriseTestContext();
		var navigationViewModel = new NavigationViewModel();
		var menu = new NavigationMenuViewModel((NoResString)"Jump", "Jump", 1, true);

		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();
			home.NavigationViewModel = navigationViewModel;

			return home;
		});

		var uut = cut.FindComponent<RecentModules>();

		// Assert
		Assert.That(uut, Is.Not.Null);
		Assert.That(uut.Find(".cwn-home__recent-modules"), Is.Not.Null);
		Assert.That(uut.FindAll(".cwn-recent-modules__item"), Is.Empty);
	}

	[Test]
	public async Task HomeRecentModules_WhenHasItemsAsync()
	{
		// setup
		using var context = new EnterpriseTestContext();
		var section1 = new MenuSection("section1", "section1", (NoResString)"section1", SectionType.RecentModule);
		var navigationViewModel = new NavigationViewModel();
		var menu = new NavigationMenuViewModel((NoResString)"Jump", "Jump", 1, true);

		navigationViewModel.AddCategory(menu);
		menu.Buttons.Add(section1);
		section1.Items.Add(new MenuItem("item1", (NoResString)"item1"));

		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();
			home.NavigationViewModel = navigationViewModel;

			return home;
		});

		var uut = cut.FindComponent<RecentModules>();
		uut.WaitForElement(".cwn-list-item");

		// Assert
		Assert.That(uut, Is.Not.Null);
		Assert.That(uut.Find(".cwn-home__recent-modules"), Is.Not.Null);
		Assert.That(uut.FindAll(".cwn-list-item.cwn-recent-modules__item").Count, Is.EqualTo(1));
		Assert.That(uut.Markup, Does.Contain("item1"));
	}

	[Test]
	public async Task HomeNewsAsync()
	{
		// setup
		using var context = new EnterpriseTestContext();

		// Act
		var cut = await context.RenderControlOnFormAsync(() => new NextHomeUserControl());

		// Assert
		Assert.That(cut.Markup, Does.Not.Contain("No news"));
	}

	[Test, WithTransaction]
	public async Task HomeNewsValuesPopulatedCorrectlyAsync()
	{
		// setup
		using var context = new EnterpriseTestContext();

		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();
			var newsViewModelTop = new MockNewsViewModel
			{
				Sections = [new NewsSectionViewModel("AAA", "All")]
			};

			newsViewModelTop.FilteredNewsItems.Add(new NewsTest.MockNewsItemViewModel
			{
				Summary = "Summary 1",
				SectionName = "Section 1",
				ReleaseDate = new DateTime(2025, 1, 1),
			});

			home.NewsViewModelTop = newsViewModelTop;

			var newsViewModelTopBottom = new MockNewsViewModel
			{
				Sections = [new NewsSectionViewModel("BBB", "All")]
			};
			newsViewModelTopBottom.FilteredNewsItems.Add(new NewsTest.MockNewsItemViewModel
			{
				Summary = "Summary 2",
				SectionName = "Section 2",
				ReleaseDate = new DateTime(2025, 1, 1),
			});

			home.NewsViewModelBottom = newsViewModelTopBottom;

			WinzorDispatcher.Current.DoEvents();
			return home;
		});

		var news = cut.FindComponent<News>();

		// Assert
		Assert.That(news, Is.Not.Null);
		Assert.That(news.RenderCount, Is.EqualTo(1));
		Assert.That(cut.FindAll(".cwn-news .cwn-list-item__info-link a").Count, Is.GreaterThan(0));
	}

	[Test]
	public async Task HomeFooterLoginDetailsAsync([Values] bool hasData)
	{
		// setup
		using var context = new EnterpriseTestContext();
		var model = new SessionContextViewModel()
		{
			UserName = "username",
			Branch = "branch",
			Company = "company",
			Department = "department"
		};

		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();

			if (hasData)
			{
				home.SessionContextViewModel = model;
			}

			return home;
		});

		// Assert
		Assert.That(cut.FindAll(".cwn-home__login-details").Count, Is.EqualTo(1));

		var footerContent = cut.Find(".cwn-home__login-details").TextContent;

		if (hasData)
		{
			Assert.That(footerContent, Does.Contain("username"));
			Assert.That(footerContent, Does.Contain("Branch branch"));
			Assert.That(footerContent, Does.Contain("Company company"));
			Assert.That(footerContent, Does.Contain("Department department"));
		}
		else
		{
			Assert.That(footerContent, Does.Not.Contain("username"));
			Assert.That(footerContent, Does.Not.Contain("Branch branch"));
			Assert.That(footerContent, Does.Not.Contain("Company company"));
			Assert.That(footerContent, Does.Not.Contain("Department department"));
		}
	}

	[Test]
	public async Task HomeFooterLoginDetailsUserImageAsync([Values] bool hasUserImage)
	{
		// Data
		const string UserImageContent =
			"iVBORw0KGgoAAAANSUhEUgAAADUAAAA1CAIAAABuhDQnAAAACXBIWXMAABYlAAAWJQFJUiTwAAAAEXRFWHRTb2Z0d2FyZQBTbmlwYXN0ZV0Xzt0AAACQSURBVGiB7c4xDoJAFEBB1o/3vxuxQCpINhgxUooaUU9gLF7BFm9OMKnrmqpgu60Df/hj/DH+GH+MP8Yf44/xx/hj/DH+GH+MP8Yf44/xx/hj/DH+GH+MP8Yf448p/VdvHfgp3Zc0XYr8ra90W2LM0bXF/dK67ua5PjRxbOOUS/q9P3Eeq6GPoY+c6+u0fz6+ocokHgnsFA4AAAAASUVORK5CYII=";

		// Setup
		using var context = new EnterpriseTestContext();
		var model = new SessionContextViewModel()
		{
			UserName = "username",
			Branch = "branch",
			Company = "company",
			Department = "department"
		};

		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();

			if (hasUserImage)
			{
				model.UserImage = Convert.FromBase64String(UserImageContent);
			}

			home.SessionContextViewModel = model;
			return home;
		});

		// Assert
		if (hasUserImage)
		{
			Assert.That(cut.FindAll(".cwn-home__login-details img").Count, Is.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-home__login-details img")[0].GetAttribute("src"),
				Is.EqualTo($"data:image/png;base64,{UserImageContent}"));
		}
		else
		{
			Assert.That(cut.FindAll(".cwn-home__login-details img").Count, Is.EqualTo(0));
		}
	}

	[Test]
	public async Task NextNavigationDataAsync()
	{
		// setup
		using var context = new EnterpriseTestContext();
		var model = new SessionContextViewModel()
		{
			UserName = "username",
			Branch = "branch",
			Company = "company",
			Department = "department"
		};
		NextNavigation? home = null;

		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			home = new NextNavigation();
			return home;
		});

		var elementHost = (KElementHost)home!.Controls.First(c => c is KElementHost);
		var homeControl = elementHost.Child as NextHomeUserControl;

		// Assert

		Assert.That(homeControl, Is.Not.Null);
		Assert.That(homeControl.NavigationViewModel, Is.Not.Null);
		Assert.That(homeControl.NavigationViewModel.MainMenuCategories, Is.Not.Null);
		Assert.That(homeControl.SessionContextViewModel, Is.Not.Null);
	}

	[Test]
	public async Task Favorites_WhenHasNoItemsAsync()
	{
		// setup
		using var context = new EnterpriseTestContext();
		var navigationViewModel = new NavigationViewModel();
		var menu = new NavigationMenuViewModel((NoResString)"Jump", "Jump", 1, true);
		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();
			home.NavigationViewModel = navigationViewModel;
			return home;
		});
		var uut = cut.FindComponent<Favorites>();
		// Assert
		Assert.That(uut, Is.Not.Null);
		Assert.That(uut.Find(".cwn-home__favorites"), Is.Not.Null);
		Assert.That(uut.FindAll(".cwn-favorites__item"), Is.Empty);
	}

	[Test]
	public async Task Favorites_WhenHasItemsAsync()
	{
		// setup
		using var context = new EnterpriseTestContext();
		var section1 = new MenuSection("section1", "section1", (NoResString)"section1", SectionType.Favorite);
		var navigationViewModel = new NavigationViewModel();
		var menu = new NavigationMenuViewModel((NoResString)"Jump", "Jump", 1, true);
		navigationViewModel.AddCategory(menu);
		menu.Buttons.Add(section1);
		section1.Items.Add(new MenuItem("item1", (NoResString)"item1"));
		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();
			home.NavigationViewModel = navigationViewModel;
			return home;
		});
		var uut = cut.FindComponent<Favorites>();
		uut.WaitForElement(".cwn-list-item");
		// Assert
		Assert.That(uut, Is.Not.Null);
		Assert.That(uut.Find(".cwn-home__favorites"), Is.Not.Null);
		Assert.That(uut.FindAll(".cwn-favorites__item").Count, Is.EqualTo(1));
		Assert.That(uut.Markup, Does.Contain("item1"));
	}

	[Test]
	public async Task RecentItems_WhenEmptyAsync()
	{
		// setup
		using var context = new EnterpriseTestContext();
		var navigationViewModel = new NavigationViewModel();
		var menu = new NavigationMenuViewModel((NoResString)"Jump", "Jump", 1, true);
		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();
			home.NavigationViewModel = navigationViewModel;
			return home;
		});
		var uut = cut.FindComponent<RecentItems>();
		// Assert
		Assert.That(uut, Is.Not.Null);
		Assert.That(uut.Find(".cwn-recent-items"), Is.Not.Null);
		Assert.That(uut.FindAll(".cwn-recent-items__item"), Is.Empty);
		Assert.That(uut.Find(".cwn-recent-items__empty"), Is.Not.Null);
	}

	[Test]
	public async Task RecentItems_WhenNotEmptyAsync()
	{
		// setup
		using var context = new EnterpriseTestContext();
		var section1 = new MenuSection("section1", "section1", (NoResString)"section1", SectionType.RecentItem);
		var navigationViewModel = new NavigationViewModel();
		var menu = new NavigationMenuViewModel((NoResString)"Jump", "Jump", 1, true);
		navigationViewModel.AddCategory(menu);
		menu.Buttons.Add(section1);
		section1.Items.Add(new MenuItem("item1", (NoResString)"item1"));
		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();
			home.NavigationViewModel = navigationViewModel;
			return home;
		});
		var uut = cut.FindComponent<RecentItems>();
		// Assert
		Assert.That(uut, Is.Not.Null);
		Assert.That(uut.Find(".cwn-recent-items"), Is.Not.Null);
		Assert.That(uut.FindAll(".cwn-recent-items__item").Count, Is.EqualTo(1));
		Assert.That(uut.Markup, Does.Contain("item1"));
		Assert.That(uut.FindAll(".cwn-recent-items__cell--compact").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task TestKeyboardServiceAsync()
	{
		using var context = new EnterpriseTestContext();
		NextHomeUserControl? home = null;
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			home = new NextHomeUserControl();
			return home;
		});

		var control = cut.Find($"div[data-winzor-control-id=\"{home?.WinzorControlId}\"]");
		var headNav = cut.FindComponent<HeadNav>();

		var keyDownEventTriggered = false;
		if (headNav.Instance.KeyboardService != null)
		{
			headNav.Instance.KeyboardService.OnKeyDown += (sender, args) =>
			{
				keyDownEventTriggered = true;
			};
		}

		await cut.KeyPressAsync(Keys.A | Keys.D1, control);
		Assert.That(keyDownEventTriggered, Is.True);
	}

	[Test]
	public async Task TestNextAppBarAsync()
	{
		using var context = new EnterpriseTestContext();
		var cut = await context.RenderControlOnFormAsync(() => new NextHomeUserControl());

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

	[Test]
	public async Task RecentItems_ToggleFavoriteState_ShouldUpdateUIAsync()
	{
		// setup
		var favoritesService = new Mock<IFavoritesService>();
		using var context = new EnterpriseTestContext();
		var section1 = new MenuSection("section1", "section1", (NoResString)"section1", SectionType.RecentItem);
		var navigationViewModel = new NavigationViewModel();
		var menu = new NavigationMenuViewModel((NoResString)"Jump", "Jump", 1, true);
		var menuItem = new MenuItem("item1", (NoResString)"item1");
		menuItem.SetFavoriteAction(() => menuItem.IsInFavorites = !menuItem.IsInFavorites);

		navigationViewModel.AddCategory(menu);
		menu.Buttons.Add(section1);
		section1.Items.Add(menuItem);
		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();
			home.NavigationViewModel = navigationViewModel;
			return home;
		});
		var uut = cut.FindComponent<RecentItems>();
		var button = uut.Find(".cwn-recent-items__icon--row button");

		Assert.That(button.Attributes["title"]?.Value, Is.EqualTo("Add to Favorites"));
		Assert.That(uut.FindAll(".cwn-recent-items__icon .cwn-icon--star-empty").Count, Is.EqualTo(1));
		Assert.That(uut.FindAll(".cwn-recent-items__icon .cwn-icon--star-filled__hover").Count, Is.EqualTo(1));

		await button.ClickAsync(new WebMouseEventArgs());

		Assert.That(button.Attributes["title"]?.Value, Is.EqualTo("Remove from Favorites"));
		Assert.That(uut.FindAll(".cwn-recent-items__icon .cwn-icon--star-filled").Count, Is.EqualTo(1));
		Assert.That(uut.FindAll(".cwn-recent-items__icon .cwn-icon--star-filled__hover").Count, Is.EqualTo(1));

		await button.ClickAsync(new WebMouseEventArgs());

		Assert.That(button.Attributes["title"]?.Value, Is.EqualTo("Add to Favorites"));
		Assert.That(uut.FindAll(".cwn-recent-items__icon .cwn-icon--star-empty").Count, Is.EqualTo(1));
		Assert.That(uut.FindAll(".cwn-recent-items__icon .cwn-icon--star-filled__hover").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task TestToggleRightContentView()
	{
		using var context = new EnterpriseTestContext();
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();
			home.NavigationViewModel = new NavigationViewModel();
			return home;
		});
		var uut = cut.FindComponent<NextHome>();
		var button = uut.Find(".cwn-toggle-button");
		Assert.That(button.Attributes["title"]?.Value, Is.EqualTo("Hide right content"));
		Assert.That(uut.FindAll(".cwn-home__right-sidebar").Count, Is.EqualTo(1));

		await button.ClickAsync(new WebMouseEventArgs());

		Assert.That(button.Attributes["title"]?.Value, Is.EqualTo("Show right content"));
		Assert.That(uut.FindAll(".cwn-home__right-sidebar").Count, Is.EqualTo(0));
	}

	internal class MockNewsViewModel : INewsViewModel
	{
		public ObservableCollection<INewsItemViewModel> FilteredNewsItems { get; set; } = [];

		public INewsSectionViewModel SelectedSection { get; set; } = new NewsSectionViewModel("section1", "section1");

		public ObservableCollection<INewsSectionViewModel> Sections { get; set; } = [];

		public void ExecuteShowItemCommand(object parameter)
		{
			throw new NotImplementedException();
		}

		public string NameAll => "All";
	}
}
