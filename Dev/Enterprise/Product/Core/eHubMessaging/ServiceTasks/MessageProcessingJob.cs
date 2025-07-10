using CargoWise.ComponentModel;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	abstract class MessageProcessingJob : ServiceTaskJob
	{
		protected MessageProcessingJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier)
			: base(serviceTaskSupport, notifier)
		{
		}

		#region Execution

		internal override void ExecuteInternal() => ProcessMessages();

		protected virtual void ProcessMessages()
		{
			NotifyVerbose(Res.GetString("b0cb4860-7978-4d54-b8e5-96ca71169c43", "Calling Process Messages ()"));
			ProcessMessagesCore();
			NotifyVerbose(Res.GetString("d0217fee-770c-4673-b32f-eee0d0c3de50", "Successfully called Process Messages ()"));
		}

		internal abstract void ProcessMessagesCore();

		#endregion // Execution

		#region Misc

		internal abstract string MutexPrefix { get; }

		#endregion // Misc
	}
}
