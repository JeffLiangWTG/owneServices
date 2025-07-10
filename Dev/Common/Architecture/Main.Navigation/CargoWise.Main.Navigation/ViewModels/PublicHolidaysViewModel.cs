using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Navigation;

#nullable disable
public class PublicHolidaysViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	public MultilingualString Title { get; } = ResString.GetMultilingualString("Main.Home.PublicHolidays.Title", "Public Holiday Calendar");
	public MultilingualString EmptyLabel { get; } = ResString.GetMultilingualString("Main.Home.PublicHolidays.EmptyLabel", "No Public Holidays");
	public IPublicHolidaysRepository Repository { get; }
	public DateTime Date { get; }
	public Action OpenHolidayModuleForm { get; set; }

	public ICommand OpenHolidayFormCommand { get; }

	public PublicHolidaysViewModel(IPublicHolidaysRepository repository, DateTime dateFrom)
	{
		Repository = repository;
		Date = dateFrom;
		OpenHolidayFormCommand = new ClickCommand(() =>
		{
			OpenHolidayModuleForm?.Invoke();
		});
	}

	ObservableCollection<PublicHoliday> publicHolidays = [];
	public ObservableCollection<PublicHoliday> PublicHolidays
	{
		get
		{
			return publicHolidays;
		}
		private set
		{
			if (publicHolidays != value)
			{
				publicHolidays = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PublicHolidays)));
			}
		}
	}

	public void Refresh()
	{
		PublicHolidays = new ObservableCollection<PublicHoliday>(
			Repository.GetPublicHolidays(startDate: Date.AddDays(-2), endDate: Date.AddDays(14)));
	}
}
