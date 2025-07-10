using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoWise.Main.Navigation;

public interface IRecentMessagesRepository
{
	IEnumerable<RecentMessage> GetLatestMessages(int limit);
	Task<IEnumerable<RecentMessage>> GetLatestMessagesAsync(int limit);
}
