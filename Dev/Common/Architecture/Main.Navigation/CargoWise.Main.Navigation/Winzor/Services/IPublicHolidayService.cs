using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CargoWise.Main.Navigation;

public interface IPublicHolidayService
{
	string Title { get; }
	string NoPublicHolidaysLabel { get; }

	ObservableCollection<PublicHoliday> PublicHolidays { get; }

	Task RefreshAsync();
	Task OpenHolidayModuleAsync();
}
