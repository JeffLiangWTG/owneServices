using CargoWise.Main.Navigation.Pages;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test;

public class HeadNavTest : BunitTestContext
{
	[TestCase(true, false, TestName = "{m}_ShouldHideMainNav_WhenMainNavVisible")]
	[TestCase(false,false, TestName = "{m}_ShouldNotHideMainNav_WhenMainNavNotVisible")]
	public void HandleEscapeKeyPress(bool initialIsMainNavVisibleValue, bool finalIsManNavVisibleValue)
	{
		// Arrange
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		var navigationViewModel = new NavigationViewModel();
		var headNav = RenderComponent<HeadNav>(parameters => parameters
			.Add(p => p.NavigationViewModel, navigationViewModel));
		headNav.Instance.GetType().GetProperty("isMainNavVisible")!.SetValue(headNav.Instance, initialIsMainNavVisibleValue);

		// Act
		var escapeKeyEvent = new WebKeyboardEventArgs { Key = "\u001b" };
		headNav.Instance.HandleEscapeKeyPress(null, escapeKeyEvent);

		// Assert
		Assert.That(headNav.Instance.isMainNavVisible, Is.EqualTo(finalIsManNavVisibleValue));
	}

	[Test]
	public void HandleEscapeKeyPress_ShouldNotHideMainNav_WhenSomeOtherKeyPressed()
	{
		// Arrange
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		var navigationViewModel = new NavigationViewModel();
		var headNav = RenderComponent<HeadNav>(parameters => parameters
			.Add(p => p.NavigationViewModel, navigationViewModel));
		headNav.Instance.GetType().GetProperty("isMainNavVisible")!.SetValue(headNav.Instance, true);

		// Act
		var keyEvent = new WebKeyboardEventArgs { Key = "A" };
		headNav.Instance.HandleEscapeKeyPress(null, keyEvent);

		// Assert
		Assert.That(headNav.Instance.isMainNavVisible, Is.True);
	}

	[Test]
	public void HandleEscapeKeyPress_ShouldNotHideMainNav_WhenNavigationVMIsInvalid()
	{
		// Arrange
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		var headNav = RenderComponent<HeadNav>();
		headNav.Instance.GetType().GetProperty("isMainNavVisible")!.SetValue(headNav.Instance, true);

		// Act
		var keyEvent = new WebKeyboardEventArgs { Key = "\u001b" };
		headNav.Instance.HandleEscapeKeyPress(null, keyEvent);

		// Assert
		Assert.That(headNav.Instance.isMainNavVisible, Is.True);
	}
}
