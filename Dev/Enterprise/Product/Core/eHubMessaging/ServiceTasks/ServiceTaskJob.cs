using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Data;
using Enterprise.eHubMessaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	abstract class ServiceTaskJob : IeHubServiceTaskJob
	{
		protected ServiceTaskJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier)
		{
			ServiceTaskSupport = serviceTaskSupport;
			Notifier = notifier;
		}

		#region Execution

		public void Execute(CancellationToken cancellationToken)
		{
			atLeastOneMessageProcessed = false;

			if (CanExecute)
			{
				token = cancellationToken;
				ExecuteInternal();
			}
		}

		internal virtual bool CanExecute
		{
			get { return true; }
		}

		internal abstract void ExecuteInternal();

		public virtual bool NextExecuteIterationIsScheduled => atLeastOneMessageProcessed;

		internal void OnAtLeastOneMessageProcessed() => atLeastOneMessageProcessed = true;

		protected void ThrowIfCancellationRequested()
		{
			token.ThrowIfCancellationRequested();
		}

		bool atLeastOneMessageProcessed;
		CancellationToken token;

		#endregion // Execution

		#region Notifications

		internal INotifications Notifier { get; private set; }

		internal virtual void NotifyVerbose(string verboseMessage)
		{
			Notifier.Notify(new VerboseInfoNotification(verboseMessage));
		}

		#endregion // Notifications

		#region Misc

		internal virtual DbConnection DbConnection
		{
			get { return Db.Connection; }
		}

		protected IeHubServiceTaskSupport ServiceTaskSupport { get; private set; }

		protected virtual string DynamicServerAddress { get; }

		protected string ServerAddress => DynamicServerAddress ?? ServiceTaskSupport.DefaultServerAddress;

		protected virtual string ServerDescription
		{
			get { return Res.GetString("4B15008E-9F5B-4726-9EDF-0A1737AE5761", "eHub Server"); }
		}

		#endregion // Misc
	}
}
