using System;
using System.Threading.Tasks;
using CargoWise.Main.Navigation.ViewModels;

namespace CargoWise.Main.Navigation;

public interface IMainPageInvokeService
{
	Task InvokeAsync(Action action);
}
public interface IMainPageModelService : IMainPageInvokeService
{
	NavigationViewModel? NavigationViewModel { get; }
	SessionContextViewModel? SessionContextViewModel { get; }
	INewsViewModel? NewsViewModelTop { get; set; }
	INewsViewModel? NewsViewModelBottom { get; set; }
	IMyTasksViewModel? MyTasksViewModel { get; set; }

	bool IsSnapshotsEnabled { get; }
}
