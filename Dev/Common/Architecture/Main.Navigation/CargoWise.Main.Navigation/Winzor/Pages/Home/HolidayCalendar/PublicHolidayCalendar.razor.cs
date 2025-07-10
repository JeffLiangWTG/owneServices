using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;

public partial class PublicHolidayCalendar : CwnComponentBase
{
	[CascadingParameter]
	public IPublicHolidayService? Service { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		if (Service is not null)
		{
			await Service.RefreshAsync();
		}
	}

	bool HasPublicHolidays => Service?.PublicHolidays?.Any() ?? false;

	ObservableCollection<PublicHoliday> PublicHolidays => Service?.PublicHolidays ?? [];
	Task TitleClickAsync() =>
		Service?.OpenHolidayModuleAsync() ?? Task.CompletedTask;
}
