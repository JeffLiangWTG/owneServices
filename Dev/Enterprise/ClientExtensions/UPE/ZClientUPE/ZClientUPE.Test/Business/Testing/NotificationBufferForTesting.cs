using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class NotificationBufferForTesting : NotificationBuffer
	{
		public NotificationBufferForTesting()
		{
		}

		public NotificationBufferForTesting(INotifications inner) : base(inner)
		{
		}

		public INotification LastOne
		{
			get
			{
				return Events[Events.Length - 1];
			}
		}
	}
}
