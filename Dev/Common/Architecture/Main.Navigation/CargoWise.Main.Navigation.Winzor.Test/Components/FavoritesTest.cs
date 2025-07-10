using System.Collections.ObjectModel;
using AngleSharp.Dom;
using Bunit;
using CargoWise.Main.Navigation.Pages;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test;

public class FavoritesTest : BunitTestContext
{
	[Test]
	public async Task Favorites_CollectionUpdateDuringRenderShouldNotThrowExceptionAsync()
	{
		// Setup
		var favoritesService = new Mock<IFavoritesService>();
		var jsInterop = new Mock<IFavoritesJsInterop>();

		Services.AddSingleton(jsInterop.Object);

		var favoriteItems = new ObservableCollection<MenuItem>
		{
			new ((NoResString)"Menu Item 1")
		};

		favoritesService.SetupGet(x => x.Items).Returns(favoriteItems);
		favoritesService.SetupGet(x => x.DragToReorderText).Returns("Drag to order");

		// Act
		var cut = RenderComponent<Favorites>(parameters =>
					parameters
					.Add(p => p.Service, favoritesService.Object));

		// Assert
		Assert.That(cut.FindAll(".cwn-favorites__item").Count, Is.EqualTo(1));
		Assert.That(cut.Find(".cwn-favorites__item .cwn-favorites__item_drag_container").Attributes.First(a => a.Name == "title").Value, Is.EqualTo("Drag to order"));
		Assert.That(cut.FindAll(".cwn-favorites__item").Count, Is.EqualTo(1));

		var updateTask = Task.Run(() =>
		{
			favoriteItems.Add(new MenuItem((NoResString)"Menu Item 2"));
		});
		cut.Render();
		await updateTask;

		Assert.That(cut.FindAll(".cwn-favorites__item").Count, Is.EqualTo(2));
	}

	[Test]
	public void Favorites_DragDropReorder()
	{
		// Setup
		var favoritesService = new Mock<IFavoritesService>();
		var jsInterop = new Mock<IFavoritesJsInterop>();
		Services.AddSingleton(jsInterop.Object);

		var favoriteItems = GetTestMenuItems();
		favoritesService.SetupGet(x => x.Items).Returns(favoriteItems);

		// Act
		var cut = RenderComponent<Favorites>(parameters => parameters.Add(p => p.Service, favoritesService.Object));

		// Assert
		Assert.That(favoritesService.Invocations.Count(i => i.Method.Name == "DropAsync"), Is.EqualTo(0));

		DragDropFavoriteItem(cut, dragFrom: 2, dropTo: 1);
		AssertLastServiceCall(favoritesService, favoriteItems[1], 0, true);

		DragDropFavoriteItem(cut, dragFrom: 1, dropTo: 3);
		AssertLastServiceCall(favoritesService, favoriteItems[0], 2, false);

		DragDropFavoriteItem(cut, dragFrom: 3, dropTo: 3);

		// Drag drop onto the same item, should not register additional call to DropAsync
		Assert.That(favoritesService.Invocations.Count(i => i.Method.Name == "DropAsync"), Is.EqualTo(2));
	}

	[Test]
	public void Favorites_WithNullOrEmptyItems()
	{
		var favoritesService = new Mock<IFavoritesService>();
		var jsInterop = new Mock<IFavoritesJsInterop>();
		Services.AddSingleton(jsInterop.Object);

		favoritesService.SetupGet(x => x.Items).Returns(() => new());
		var cut = RenderComponent<Favorites>(parameters => parameters.Add(p => p.Service, favoritesService.Object));
		Assert.That(cut.Find(".cwn-favorites__empty"), Is.Not.Null);

		favoritesService.SetupGet(x => x.Items).Returns(() => null);
		cut = RenderComponent<Favorites>(parameters => parameters.Add(p => p.Service, favoritesService.Object));
		Assert.That(cut.Find(".cwn-favorites__empty"), Is.Not.Null);
	}

	ObservableCollection<MenuItem> GetTestMenuItems()
	{
		return new ObservableCollection<MenuItem>
		{
			new ((NoResString)"1"),
			new ((NoResString)"2"),
			new ((NoResString)"3")
		};
	}

	void AssertLastServiceCall(Mock<IFavoritesService> favoritesService, MenuItem dragFromItem, int dropToIndex, bool insertAbove)
	{
		var lastInvocation = favoritesService.Invocations.Last(i => i.Method.Name == "DropAsync");
		Assert.That(lastInvocation.Arguments[0], Is.EqualTo(dragFromItem));
		Assert.That(lastInvocation.Arguments[1], Is.EqualTo(dropToIndex));
		Assert.That(lastInvocation.Arguments[2], Is.EqualTo(insertAbove));
	}

	void DragDropFavoriteItem(IRenderedComponent<Favorites> component, int dragFrom, int dropTo)
	{
		component.FindAll($".cwn-favorites__item .cwn-favorites__item_drag_container[draggable='true']")[dragFrom - 1].TriggerEvent("ondragstart", new WebDragEventArgs());
		component.FindAll($".cwn-favorites__item")[dropTo - 1].TriggerEvent("ondrop", new WebDragEventArgs());
	}

	[Test]
	public void Favorites_Unfavorite()
	{
		// Setup
		var favoritesService = new Mock<IFavoritesService>();
		var jsInterop = new Mock<IFavoritesJsInterop>();
		Services.AddSingleton(jsInterop.Object);

		var favoriteItems = GetTestMenuItems();
		favoritesService.SetupGet(x => x.Items).Returns(favoriteItems);

		// Act
		var cut = RenderComponent<Favorites>(parameters => parameters.Add(p => p.Service, favoritesService.Object));
		cut.Render();
		Assert.That(cut.FindAll(".cwn-favorites__item").Count, Is.EqualTo(3));

		var divElement = cut.FindAll(".cwn-favorites__item_star").FirstOrDefault();
		divElement?.Click();
		Assert.That(favoritesService.Invocations.Count(i => i.Method.Name == "PerformFavoriteClickAsync"), Is.EqualTo(1));
	}
}
