using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;
public partial class RecentMessages : CwnComponentBase
{
	[CascadingParameter]
	public IRecentMessagesService? Service { get; set; }

	public string Classname => new CssBuilder()
		.AddClass("cwn-recent-messages")
		.AddClass(Class)
		.Build();

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		if (Service is not null)
		{
			await Service.RefreshAsync();
		}
	}

	bool HasRecentMessages => Service?.RecentMessages?.Any() ?? false;

	ObservableCollection<RecentMessage> Messages => Service?.RecentMessages ?? [];

	Task OpenJobAsync(RecentMessage message) =>
		Service?.OpenJobAsync(jobConversationId: message.Id) ?? Task.CompletedTask;

	Task RefreshAsync() => Service?.RefreshAsync() ?? Task.CompletedTask;
}
