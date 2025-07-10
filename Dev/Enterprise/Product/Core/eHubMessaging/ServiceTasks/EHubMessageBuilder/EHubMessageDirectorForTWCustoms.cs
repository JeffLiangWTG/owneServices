using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageDirectorForTWCustoms
	{
		readonly INotifications notifier;
		readonly EDIInterchange interchange;
		readonly HashSet<string> supportedMessageTypes = new HashSet<string> { "ECD", "ICD", "ADM", "TRA", "IEA", "CAA", "FHM", "NXM", "201", "207", "301", "31A", "31D", "401", "601", "603" };

		public EHubMessageDirectorForTWCustoms(EDIInterchange interchange, INotifications notifier)
		{
			this.notifier = notifier;
			this.interchange = interchange;
		}

		public EHubMessageBuilder CreateBuilder()
		{
			if (supportedMessageTypes.Contains(interchange.EI_InterchangeType))
			{
				return new EHubMessageBuilderForTWCustoms(interchange, notifier);
			}
			else
			{
				return null;
			}
		}
	}
}
