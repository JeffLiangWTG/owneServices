using Bunit;
using CargoWise.Main.Navigation.Pages;
using CargoWiseNext.Blazor.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test;

public class NavBarToolStripTest : BunitTestContext
{
	[Test]
	public void NavBarToolStrip_RenderTest()
	{
		var mockNavBarToolStripService = new Mock<INavBarToolStripService>();
		Services.AddSingleton(new Mock<INavBarToolStripMenuInterop>().Object);

		_ = mockNavBarToolStripService.SetupGet(x => x.PopupMenus)
			.Returns(
			[
				new() { Title = "Options" },
				new() { Title = "Help" }
			]);

		var cut = RenderComponent<NavBarToolStrip>(parameters =>
			parameters.Add(p => p.Service, mockNavBarToolStripService.Object));

		Assert.That(cut.FindAll(".cwn-button-group").Count, Is.EqualTo(1));
		Assert.That(cut.FindAll(".cwn-button").Count, Is.EqualTo(2));
	}

	[Test]
	public void IsSeparatorTest()
	{
		var mockNavBarToolStripService = new Mock<INavBarToolStripService>();
		Services.AddSingleton(new Mock<INavBarToolStripMenuInterop>().Object);

		var options = new PopupMenu { Title = "Options" };
		var optionsSub1 = new PopupMenu { Title = "Sub1" };
		var optionsSub2 = new PopupMenu { IsSeparator = true };
		var optionsSub3 = new PopupMenu { Title = "Sub3" };

		var optionsSub31 = new PopupMenu { Title = "Sub31" };
		optionsSub3.SubMenuItems.Add(optionsSub31);

		options.SubMenuItems.Add(optionsSub1);
		options.SubMenuItems.Add(optionsSub2);
		options.SubMenuItems.Add(optionsSub3);
		_ = mockNavBarToolStripService.SetupGet(x => x.PopupMenus)
			.Returns(
			[
				options,
				new() { Title = "Help" }
			]);

		var cut = RenderComponent<NavBarToolStrip>(parameters =>
			parameters.Add(p => p.Service, mockNavBarToolStripService.Object));

		Assert.That(cut.FindAll("hr").Count, Is.EqualTo(1));
		Assert.That(cut.FindAll(".cwn-submenu").Count, Is.EqualTo(2));
	}

	[Test]
	public void CommandTest()
	{
		Services.AddSingleton(new Mock<INavBarToolStripMenuInterop>().Object);
		var clicked = false;
		var menu1 = new PopupMenu
		{
			Title = "Sub1",
			Command = new ClickCommand(() =>
			{
				clicked = true;
			})
		};

		var cut = RenderComponent<NavBarMenuItem>(parameters =>
		{
			parameters.Add(p => p.Item, menu1);
			parameters.Add(p => p.MainPageInvokeService, new MockMainPageInvokeService());
		});
		var button = cut.Find(".cwn-list-item a");
		button.Click();
		Assert.That(clicked, Is.True);

		var menu2 = new PopupMenu
		{
			Title = "Sub2",
		};
		cut = RenderComponent<NavBarMenuItem>(parameters =>
		{
			parameters.Add(p => p.Item, menu2);
			parameters.Add(p => p.MainPageInvokeService, new MockMainPageInvokeService());
		});
		button = cut.Find(".cwn-list-item a");
		button.Click();
		Assert.DoesNotThrow(() =>
		{
			button.Click();
		});
	}

	[Test]
	public void GestureTextTest()
	{
		var mockNavBarToolStripService = new Mock<INavBarToolStripService>();
		Services.AddSingleton(new Mock<INavBarToolStripMenuInterop>().Object);

		var options = new PopupMenu { Title = "Options" };
		var optionsSub1 = new PopupMenu
		{
			Title = "Sub1",
			InputGestureText = "Ctrl+Shift+T",
		};
		var optionsSub2 = new PopupMenu { Title = "Sub3" };

		options.SubMenuItems.Add(optionsSub1);
		options.SubMenuItems.Add(optionsSub2);
		_ = mockNavBarToolStripService.SetupGet(x => x.PopupMenus)
			.Returns(
			[
				options,
				new() { Title = "Help" }
			]);

		var cut = RenderComponent<NavBarToolStrip>(parameters =>
			parameters.Add(p => p.Service, mockNavBarToolStripService.Object));
		var sortcuts = cut.FindComponents<CwnSearchShortcut>();
		Assert.That(sortcuts.Count, Is.EqualTo(3));
		Assert.That(sortcuts[0].Markup, Does.Contain("<span>Ctrl</span>"));
		Assert.That(sortcuts[1].Markup, Does.Contain("<span>Shift</span>"));
		Assert.That(sortcuts[2].Markup, Does.Contain("<span>T</span>"));
	}

	class MockMainPageInvokeService : IMainPageInvokeService
	{
		public Task InvokeAsync(Action action)
		{
			action.Invoke();
			return Task.CompletedTask;
		}
	}
}
