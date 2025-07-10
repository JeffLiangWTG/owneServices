using Bunit;
using CargoWise.Main.Navigation.Pages;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test;

public class MainNavTest : BunitTestContext
{
	[Test]
	public async Task MainNavRenderTestAsync()
	{
		// Setup
		var navigationViewModel = new NavigationViewModel();
		var categoryViewModel = new NavigationMenuViewModel((NoResString)"test", "test", 1, true);
		categoryViewModel.AddSection("A name", "A displayName", (NoResString)"TestA", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "A", (NoResString)"Test");
		categoryViewModel.AddSection("C name", "C displayName", (NoResString)"TestC", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "A", (NoResString)"Test");
		navigationViewModel.AddCategory(categoryViewModel);

		// Act
		using var context = new EnterpriseTestContext();
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			var home = new NextHomeUserControl();
			home.NavigationViewModel = navigationViewModel;
			navigationViewModel.SelectedCategory = categoryViewModel;
			categoryViewModel.SelectedItem = categoryViewModel.MenuSections[0];
			return home;
		});

		Assert.That(cut.FindAll(".cwn-mainnav").Count, Is.EqualTo(1));
		Assert.That(cut.FindAll(".cwn-mainnav__categories").Count, Is.EqualTo(1));
		Assert.That(cut.FindAll(".cwn-mainnav__modules").Count, Is.EqualTo(1));

		// Assert
		var categoryList = cut.FindComponents<CwnList>()[0];
		var categoryListItems = categoryList.FindComponents<CwnListItem>();
		categoryListItems[0].MarkupMatches
			("""
				<li class="cwn-list-item cwn-list-item--selected">
					<button class="cwn-button cwn-icon-button cwn-icon-button--medium cwn-icon-button--text cwn-list-item__key" accesskey="A">A</button>
					<div class="cwn-list-item__text">subcategoryDisplayTextA</div>
				</li>
			""");

		var moduleList = cut.FindComponents<CwnList>()[1];
		var moduleListItems = moduleList.FindComponents<CwnListItem>();
		moduleListItems[0].MarkupMatches
			("""
				<li class="cwn-list-item cwn-mainnav__section">
				A displayName
				</li>
			""");
	}

	[Test]
	public async Task MainNavAccesskeyTestAsync()
	{
		// Setup
		var navigationViewModel = new NavigationViewModel();
		var categoryViewModel = new NavigationMenuViewModel((NoResString)"test", "test", 1, true);
		categoryViewModel.AddSection("A name", "A displayName", (NoResString)"TestA", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "A", (NoResString)"Test");
		categoryViewModel.AddSection("C name", "C displayName", (NoResString)"TestC", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "A", (NoResString)"Test");
		navigationViewModel.AddCategory(categoryViewModel);

		// Act
		using var context = new EnterpriseTestContext();
		NextHomeUserControl? home = null;
		var cut = await context.RenderControlOnFormAsync(() =>
		{
			home = new NextHomeUserControl();
			home.NavigationViewModel = navigationViewModel;
			navigationViewModel.SelectedCategory = categoryViewModel;
			categoryViewModel.SelectedItem = categoryViewModel.MenuSections[0];
			return home;
		});

		var control = cut.Find($"div[data-winzor-control-id=\"{home?.WinzorControlId}\"]");
		var headNav = cut.FindComponent<HeadNav>();

		var button = cut.Find("button[accesskey=\"A\"]");
		button.Focus();

		Assert.That(headNav.Instance.isMainNavVisible, Is.EqualTo(true));
	}
}
