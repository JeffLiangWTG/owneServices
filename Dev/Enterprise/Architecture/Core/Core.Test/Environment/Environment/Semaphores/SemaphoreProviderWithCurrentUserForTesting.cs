using Enterprise.Semaphores.Common;
using Enterprise.Semaphores.Common.Testing;

namespace Enterprise.Core.Environment.Semaphores.Testing
{
	public sealed class SemaphoreProviderWithCurrentUserForTesting : SemaphoreProviderForTesting
	{
		public string RemoteClientName
		{
			get { return heartbeatInfoFactoryWithCurrentUser.remoteClientName; }
			set { heartbeatInfoFactoryWithCurrentUser.remoteClientName = value; }
		}

		protected override IHeartbeatInfoFactory HeartbeatSessionInfoFactory
		{
			get { return heartbeatInfoFactoryWithCurrentUser; }
		}

		readonly EnterpriseHeartbeatInfoFactory heartbeatInfoFactoryWithCurrentUser = new EnterpriseHeartbeatInfoFactory();
	}
}
