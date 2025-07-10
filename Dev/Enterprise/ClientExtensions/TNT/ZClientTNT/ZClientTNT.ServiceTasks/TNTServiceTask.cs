using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Client.TNT.ServiceTasks
{
	abstract class TNTServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			NotificationBuffer notify = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber());

			if (!IsEnvironmentValid)
			{
				notify.Notify(new ErrorNotification(ErrorType.Error, RegistriesNotSetErrMesg));
				return;
			}

			try
			{
				Execute(notify, token);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notify.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			}
		}

		protected abstract bool IsEnvironmentValid { get; }
		protected abstract ZString RegistriesNotSetErrMesg { get; }
		protected abstract void Execute(NotificationBuffer notify, CancellationToken token);
	}
}
