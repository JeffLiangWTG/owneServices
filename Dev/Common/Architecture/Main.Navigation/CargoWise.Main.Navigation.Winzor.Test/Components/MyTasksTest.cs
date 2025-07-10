using System.Collections.ObjectModel;
using System.Windows.Input;
using Bunit;
using CargoWise.Main.Navigation.Pages;
using CargoWise.Main.Navigation.ViewModels;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test;

public class MyTasksTest : BunitTestContext
{
	[Test]
	public async Task RefreshMyTasksClick_ShouldCallRefresh_WhenValidParamsAsync()
	{
		// Setup
		var mockMyTasksViewModel = new MockMyTasksViewModel();
		var mockMainPageModelService = new Mock<IMainPageModelService>();
		mockMainPageModelService.Setup(s => s.InvokeAsync(It.IsAny<Action>()))
			.Callback<Action>(func => func.Invoke());

		// Act
		var cut = RenderComponent<MyTasks>(parameters =>
			parameters.Add(p => p.MainPageModelService, mockMainPageModelService.Object)
				.Add(p => p.MyTasksViewModel, mockMyTasksViewModel));

		Assert.That(mockMyTasksViewModel.Executed, Is.False);

		var refreshBtn = cut.Find(".cwn-my-tasks__refresh-btn");
		await refreshBtn.ClickAsync(new WebMouseEventArgs());

		// Assert
		Assert.That(mockMyTasksViewModel.Executed, Is.True);
	}

	[Test]
	public async Task RefreshMyTasksClick_ShouldNotCallRefresh_WhenMainPageModelServiceIsNullAsync()
	{
		// Setup
		var mockMyTasksViewModel = new MockMyTasksViewModel();

		// Act
		var cut = RenderComponent<MyTasks>(parameters =>
			parameters.Add(p => p.MyTasksViewModel, mockMyTasksViewModel));

		Assert.That(mockMyTasksViewModel.Executed, Is.False);

		var refreshBtn = cut.Find(".cwn-my-tasks__refresh-btn");
		await refreshBtn.ClickAsync(new WebMouseEventArgs());

		// Assert
		Assert.That(mockMyTasksViewModel.Executed, Is.False);
	}

	[Test]
	public void MyTasks_RefreshIconPresent()
	{
		// Setup
		var mockMyTasksViewModel = new MockMyTasksViewModel();

		// Act
		var cut = RenderComponent<MyTasks>(parameters =>
			parameters.Add(p => p.MyTasksViewModel, mockMyTasksViewModel));

		// Assert
		Assert.That(cut.Find(".cwn-my-tasks__refresh-btn"), Is.Not.Null);
	}

	[Test]
	public async Task TitleClick_ShouldExecuteOpenTaskFormCommandAsync()
	{
		// Setup
		var mockMyTasksViewModel = new MockMyTasksViewModel();
		var mockMainPageModelService = new Mock<IMainPageModelService>();
		mockMainPageModelService.Setup(s => s.InvokeAsync(It.IsAny<Action>()))
			.Callback<Action>(func => func.Invoke());

		// Act
		var cut = RenderComponent<MyTasks>(parameters =>
			parameters.Add(p => p.MainPageModelService, mockMainPageModelService.Object)
				.Add(p => p.MyTasksViewModel, mockMyTasksViewModel));

		Assert.That(mockMyTasksViewModel.IsOpenTaskFormCommandExecuted, Is.False);

		var titleElement = cut.Find(".cwn-my-tasks__title");
		await titleElement.ClickAsync(new WebMouseEventArgs());

		// Assert
		Assert.That(mockMyTasksViewModel.IsOpenTaskFormCommandExecuted, Is.True);
	}

	[Test]
	public async Task ClickAsync_ShouldExecuteItemLinkAction_WhenClicked()
	{
		//Setup
		var isExecuted = false;
		var mockItemViewModel = new Mock<IMyTasksItemViewModel>();
		mockItemViewModel
		 .SetupGet(m => m.LinkAction)
		 .Returns(new ClickCommand(() => { isExecuted = true; }));

		var mockMyTasksViewModel = new Mock<IMyTasksViewModel>();
		mockMyTasksViewModel
		 .SetupGet(m => m.MyTasks)
		 .Returns([mockItemViewModel.Object]);

		var mockMainPageModelService = new Mock<IMainPageModelService>();
		mockMainPageModelService
		 .Setup(s => s.InvokeAsync(It.IsAny<Action>()))
		 .Callback<Action>(func => func.Invoke());

		//Act
		var cut = RenderComponent<MyTasks>(parameters =>
		 parameters.Add(p => p.MainPageModelService, mockMainPageModelService.Object)
		  .Add(p => p.MyTasksViewModel, mockMyTasksViewModel.Object));

		Assert.That(isExecuted, Is.False);

		var itemElement = cut.Find(".cwn-my-tasks__item");
		await itemElement.ClickAsync(new WebMouseEventArgs());

		//Assert
		Assert.That(isExecuted, Is.True);
	}

	internal class MockMyTasksViewModel : IMyTasksViewModel
	{
		internal bool Executed;
		internal bool IsOpenTaskFormCommandExecuted;
		public ObservableCollection<IMyTasksItemViewModel> MyTasks { get; set; } = [];
		public string NoMyTasksText => string.Empty;
		public string IdHeaderText => string.Empty;
		public string NameHeaderText => string.Empty;
		public string TaskDescriptionHeaderText => string.Empty;
		public string StatusHeaderText => string.Empty;
		public string DisplayName => string.Empty;
		public ICommand OpenTaskFormCommand => new ClickCommand(() => IsOpenTaskFormCommandExecuted = true);

		public void Refresh()
		{
			Executed = !Executed;
		}
	}
}
