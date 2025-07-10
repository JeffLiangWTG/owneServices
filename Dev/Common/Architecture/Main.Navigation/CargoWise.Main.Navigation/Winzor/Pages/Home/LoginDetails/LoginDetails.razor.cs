using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in LoginDetails.razor")]
public partial class LoginDetails
{
	[CascadingParameter(Name = "MainPageModelService")]
	public IMainPageModelService? MainPageModelService { get; set; }

	string UserImageBase64String()
	{
		if (MainPageModelService?.SessionContextViewModel?.UserImage is not null)
		{
			return $"data:image/png;base64,{Convert.ToBase64String(MainPageModelService.SessionContextViewModel.UserImage)}";
		}
		return string.Empty;
	}
}
