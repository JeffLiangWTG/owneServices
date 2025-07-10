using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CargoWise.Main.Navigation;

public class RecentMessagesService : IRecentMessagesService
{
	readonly RecentMessagesViewModel viewModel;
	readonly IWinzorControl winzorControl;

	public RecentMessagesService(RecentMessagesViewModel viewModel, IWinzorControl winzorControl)
	{
		this.viewModel = viewModel;
		this.winzorControl = winzorControl;
	}

	public string Title => viewModel.Title;

	public string NoRecentMessagesLabel => viewModel.NoRecentMessagesLabel;

	public string RefreshLabel => viewModel.RefreshLabel;

	public ObservableCollection<RecentMessage> RecentMessages => viewModel.RecentMessages;

	public async Task OpenJobAsync(Guid jobConversationId)
	{
		await winzorControl.InvokeAsync(() => viewModel.OpenJobCommand?.Execute(jobConversationId));
	}

	public async Task RefreshAsync()
	{
		await winzorControl.InvokeAsync(viewModel.Refresh);
	}
}
