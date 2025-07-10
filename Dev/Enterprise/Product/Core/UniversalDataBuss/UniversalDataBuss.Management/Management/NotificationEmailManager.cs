using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalDataBuss.Management
{
	public sealed class NotificationEmailManager : INotificationEmailManager
	{
		public NotificationEmailManager()
		{
			processorHashSet = new HashSet<INotificationEmailProcessor>();
		}

		readonly HashSet<INotificationEmailProcessor> processorHashSet;

		public void Register(INotificationEmailProcessor processor)
		{
			if (processor != null)
			{
				processorHashSet.Add(processor);
			}
		}

		public HtmlEmailDef Process(IEDIMessage message, HtmlEmailDef email)
		{
			if (message != null && email != null)
			{
				processorHashSet.ForEach(c => email = c.Process(message, email));
			}

			return email;
		}

		public void Clear()
		{
			processorHashSet.Clear();
		}
	}
}
