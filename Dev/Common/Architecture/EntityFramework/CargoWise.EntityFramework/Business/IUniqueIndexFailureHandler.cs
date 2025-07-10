using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public interface IUniqueIndexFailureHandler
	{
		IEnumerable<string> HandledUniqueIndexNames { get; }
		void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName);
	}
}
