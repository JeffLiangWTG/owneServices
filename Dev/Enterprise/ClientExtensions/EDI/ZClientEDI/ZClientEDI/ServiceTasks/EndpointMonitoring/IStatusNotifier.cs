using System.Collections.Generic;

namespace Enterprise.Client.EDI.ServiceTasks
{
	public interface IStatusNotifier
	{
		void Notify(string title, string message, IDictionary<string, string> attachments);
	}
}
