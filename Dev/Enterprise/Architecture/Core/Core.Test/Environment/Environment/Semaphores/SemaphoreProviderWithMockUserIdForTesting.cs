using System;
using Enterprise.Semaphores.Common;
using Enterprise.Semaphores.Common.Testing;

namespace Enterprise.Core.Environment.Semaphores.Testing
{
	public sealed class SemaphoreProviderWithMockUserIdForTesting : SemaphoreProviderForTesting, IDisposable
	{
		public SemaphoreProviderWithMockUserIdForTesting(Guid userPk)
			: base()
		{
			heartbeatInfoFactoryWithMockUserId = new HeartbeatInfoFactoryWithMockUserForTesting(userPk);
		}

		protected override IHeartbeatInfoFactory HeartbeatSessionInfoFactory
		{
			get { return heartbeatInfoFactoryWithMockUserId; }
		}

		readonly IHeartbeatInfoFactory heartbeatInfoFactoryWithMockUserId;

		#region IDisposable Members

		public void Dispose()
		{
			DisposeInternalHeartbeat();
		}

		#endregion

		#region HeartbeatInfoFactoryWithMockUserForTesting

		class HeartbeatInfoFactoryWithMockUserForTesting : IHeartbeatInfoFactory
		{
			public HeartbeatInfoFactoryWithMockUserForTesting(Guid userPk)
			{
				this.userPk = userPk;
			}

			#region IHeartbeatInfoFactory Members

			public IHeartbeatInfo New()
			{
				return new HeartbeatInfo(Guid.NewGuid(), "HeartbeatInfoFactoryWithMockUserForTesting_TestHost", userPk, 0, HeartbeatTypes.Enterprise, null);
			}

			#endregion

			readonly Guid userPk;
		}

		#endregion
	}
}
