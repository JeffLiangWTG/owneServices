using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CargoWise.Main.Navigation;

public class PublicHolidayService : IPublicHolidayService
{
	readonly PublicHolidaysViewModel viewModel;
	readonly IWinzorControl winzorControl;

	public PublicHolidayService(PublicHolidaysViewModel viewModel, IWinzorControl winzorControl)
	{
		this.viewModel = viewModel;
		this.winzorControl = winzorControl;
	}

	public string Title => viewModel.Title;

	public string NoPublicHolidaysLabel => viewModel.EmptyLabel;

	public ObservableCollection<PublicHoliday> PublicHolidays => viewModel.PublicHolidays;

	public async Task RefreshAsync()
	{
		await winzorControl.InvokeAsync(viewModel.Refresh);
	}
	public async Task OpenHolidayModuleAsync()
	{
		await winzorControl.InvokeAsync(() => viewModel.OpenHolidayFormCommand?.Execute(null));
	}
}
