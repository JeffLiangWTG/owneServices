using System.Threading;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public abstract class XMLDirector
	{
		public XMLDirector(INotifications notifications)
		{
			notify = new NotificationBuffer(notifications);
		}

#if DEBUG

		public void Run()
		{
			Run(CancellationToken.None);
		}

#endif

		public void Run(CancellationToken token)
		{
			RunCore(token);
		}

		public void ResetNotifications()
		{
			Notify.Clear();
		}

		protected abstract void RunCore(CancellationToken token);

		#region Notify

#if DEBUG
		public
#else
		protected 
#endif
 NotificationBuffer Notify
		{
			get { return notify ?? (notify = new NotificationBuffer()); }
		}
		NotificationBuffer notify;

		#endregion
	}
}
