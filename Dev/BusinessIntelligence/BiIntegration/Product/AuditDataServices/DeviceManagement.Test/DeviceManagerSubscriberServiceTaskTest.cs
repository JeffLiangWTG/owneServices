using System.Collections.Generic;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.DeviceManagement.Test
{
	[TestedType(typeof(DeviceManagementSubscriberServiceTask))]
	class DeviceManagerSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<DeviceManagementSubscriberServiceTask>
	{
		public void TestCanRunInAnyBranch()
		{
			var initialUserContext = Env.CurrentUserContext;
			var deviceManagementSubscriberServiceTask = new DeviceManagementSubscriberServiceTaskForTest();

			try
			{
				using (EnvProxy.Instance.TemporaryServiceTaskContext("DVM", canRunInAnyBranch: true))
				{
					deviceManagementSubscriberServiceTask.ServiceLogger = new LoggerForTest();
					deviceManagementSubscriberServiceTask.RunTask(CancellationToken.None);
				}
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
				AssertEquals("No Errors", 0, ErrorReporter.LastMessageReported.Length);
			}
		}

		public override DeviceManagementSubscriberServiceTask GenerateServiceTask()
		{
			return new DeviceManagementSubscriberServiceTask();
		}

		public override string ServiceTaskName()
		{
			return DeviceManagementSubscriberServiceTask.Description;
		}

		public void TestTaskWhenAuditServerIsNull()
		{
			using (BiServers.TemporarilySetAuditServerToNull())
			{
				AssertEquals("No Audit server set in the registry.", AuditSubscriberTask.IsAuditEnabled());
			}
		}

		protected override string[] GetExpectedHostedServiceRequirements()
		{
			return new[] { "IsEdiClient", "CheckCdcIsEnabled", "IsAuditEnabled", "CheckCdcShouldBeDisabled" };
		}

		public override void TestIsLoaded()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride(Clients.EDI)))
			{
				base.TestIsLoaded();
			}
		}

		protected override bool IsClientSpecific => true;

		class DeviceManagementSubscriberServiceTaskForTest : DeviceManagementSubscriberServiceTask
		{
			protected override IEnumerable<IAuditSubscriber> GetSubscribers()
			{
				_ = Env.CurrentBranch;
				return base.GetSubscribers();
			}
		}
	}
}
