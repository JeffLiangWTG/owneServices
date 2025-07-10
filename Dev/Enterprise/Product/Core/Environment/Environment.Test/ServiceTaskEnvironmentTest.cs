using System;
using Enterprise.Environment.Semaphore;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Environment.Testing
{
	sealed class ServiceTaskEnvironmentTest : EnvTest
	{
		public void TestSemaphoreProviderIsOfTypeServiceTaskSemaphoreProvider()
		{
			using (var env = new ServiceTaskEnvironment())
			{
				AssertEquals(typeof(ServiceTaskSemaphoreProvider), ((IEnvironment)env).SemaphoreProvider.GetType());
			}
		}

		protected override void SetTestEnvironment()
		{
			var currentDepartment = Env.CurrentDepartment.PK;
			var currentBranch = Env.CurrentBranch.PK;
			new ServiceManagerEnvProvider(usePooledConnection: true).Enable();
			userContextSwitch = Env.SetTemporaryUserContext(User.SupportUserName, currentBranch, currentDepartment);
		}

		protected override void TearDown()
		{
			userContextSwitch?.Dispose();
			base.TearDown();
		}

		IDisposable userContextSwitch;
	}
}
