using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core.Environment.Semaphores;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.Exceptions;
using ServiceManager.Shared.Abstractions;
using NudgeFailedEventArgs = ServiceManager.Integration.Abstractions.NudgeFailedEventArgs;

namespace ServiceManager.Integration.CW
{
	class CommunicationFailureNotification : INudgingFailedUserNotification
	{
		public CommunicationFailureNotification()
			: this(
				new Lazy<IOutgoingMailManager>(ObjectFactory.Get<IOutgoingMailManager>),
				new Lazy<IServiceHostsCache>(ObjectFactory.Get<IServiceHostsCache>))
		{
		}

		CommunicationFailureNotification(Lazy<IOutgoingMailManager> outgoingMailManager, Lazy<IServiceHostsCache> serviceHostsCache)
		{
			this.outgoingMailManager = outgoingMailManager;
			this.serviceHostsCache = serviceHostsCache;
		}

		void OnNudgeFailed(object? sender, NudgeFailedEventArgs e)
		{
			if (e.RetriesRemaining > 0
				|| !(e.Exception is ServiceHostCommunicationException communicationException)
				|| string.IsNullOrEmpty(SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.Value)
				|| !TryApplyLock())
			{
				return;
			}

			var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var email = new EmailDef
			{
				FromDisplayName = System.Environment.MachineName,
				Subject = Res.GetString(
					"18C49414-8BD2-4344-A9FA-804D33235449",
					"Nudge Failure - System: [{0}{1}] Source Host: [{2}]",
					productRegistrationKey.EnterpriseCode,
					productRegistrationKey.ServerCode,
					System.Environment.MachineName),
				Body = $@"
Process controller host cannot be reached from [{System.Environment.MachineName}].
Error message:
{communicationException.Message}

Process controller host list:
{string.Join(System.Environment.NewLine, serviceHostsCache.Value.ConfiguredServiceHosts.Select(client => client.HostName))}

You are receiving this email because your address is configured in registry {SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.GetLocationInEnglish()}.",
			};
			email.AddRecipientForSystemCommunication(new[] { SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationEmail.Value });

			using (FactorySaveAlerterOverride.TemporarilyOverride(this))
			{
				outgoingMailManager.Value.CreateAndSave(email);
			}
		}

		static bool TryApplyLock()
		{
			try
			{
				if (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequency == TimeSpan.Zero)
				{
					return true;
				}

				var semaphoreDbManager = new SemaphoreDbManager();
				var heartbeatInfo = ((IHeartbeatInfoFactory)new EnterpriseHeartbeatInfoFactory()).New();
				var uniqueId = Guid.NewGuid();
				semaphoreDbManager.CreateHeartbeatInDatabase(uniqueId,
					System.Environment.MachineName,
					heartbeatInfo.ProcessId,
					heartbeatInfo.UserPk,
					GlbStaffSchema.Constants.Prefix,
					(int)SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequency.TotalSeconds,
					"PRC",
					Globals.ClientIdentifier);
				semaphoreDbManager.CreateSemaphoreHandleInTransaction(uniqueId,
					$"ComErrNotifier:{System.Environment.MachineName}",
					"PRC",
					1);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public void Attach(INudgingController nudgingController)
		{
			nudgingController.NudgeFailedEvent += OnNudgeFailed;
		}

		readonly Lazy<IOutgoingMailManager> outgoingMailManager;
		readonly Lazy<IServiceHostsCache> serviceHostsCache;
	}
}
