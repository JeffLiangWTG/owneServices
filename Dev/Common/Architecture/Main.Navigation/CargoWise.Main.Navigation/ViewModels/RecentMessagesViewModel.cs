using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Navigation;

#nullable disable
public class RecentMessagesViewModel : INotifyPropertyChanged
{
	public RecentMessagesViewModel(IRecentMessagesRepository repository)
	{
		Repository = repository;
	}

	public event PropertyChangedEventHandler PropertyChanged;
	public MultilingualString Title { get; } = ResString.GetMultilingualString("Main.Home.RecentMessages", "Recent Messages");
	public MultilingualString NoRecentMessagesLabel { get; } = ResString.GetMultilingualString("Main.Home.RecentMessages.NoRecentMessagesLabel", "No Recent Messages");
	public MultilingualString RefreshLabel { get; } = ResString.GetMultilingualString("Main.Home.RecentMessages.Refresh", "Refresh Recent Messages");

	ObservableCollection<RecentMessage> recentMessages = [];
	public ObservableCollection<RecentMessage> RecentMessages
	{
		get
		{
			return recentMessages;
		}
		set
		{
			if (recentMessages != value)
			{
				recentMessages = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecentMessages)));
			}
		}
	}

	IRecentMessagesRepository Repository { get; }

	public void Refresh()
	{
		RecentMessages = new ObservableCollection<RecentMessage>(Repository.GetLatestMessages(limit: 25));
	}

	public ICommand OpenJobCommand { get; set; }
}
