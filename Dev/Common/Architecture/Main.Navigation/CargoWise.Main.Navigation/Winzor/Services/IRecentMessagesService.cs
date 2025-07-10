using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CargoWise.Main.Navigation;

public interface IRecentMessagesService
{
	string Title { get; }
	string NoRecentMessagesLabel { get; }
	string RefreshLabel { get; }

	ObservableCollection<RecentMessage> RecentMessages { get; }

	Task OpenJobAsync(Guid jobConversationId);
	Task RefreshAsync();
}
