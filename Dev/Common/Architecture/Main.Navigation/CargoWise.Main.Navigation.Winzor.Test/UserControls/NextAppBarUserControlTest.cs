using System.Collections.ObjectModel;
using Bunit;
using CargoWise.Main.Navigation.Pages;
using CargoWise.Main.Navigation.ViewModels;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test;

public class NextAppBarUserControlTest : BunitTestContext
{
	[Test]
	public async Task RenderTestAsync()
	{
		// setup
		using var context = new EnterpriseTestContext();
		var section1 = new MenuSection("section1", "section1", (NoResString)"section1", SectionType.GlobalSearch);
		var navigationViewModel = new NavigationViewModel();
		var menu = new NavigationMenuViewModel((NoResString)"Jump", "Jump", 1, true);

		navigationViewModel.AddCategory(menu);
		menu.Buttons.Add(section1);
		section1.Items.Add(new MenuItem("item1", (NoResString)"item1"));

		// Act
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var appBar = new NextAppBarUserControl(navigationViewModel);
			return appBar;
		});

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
}
