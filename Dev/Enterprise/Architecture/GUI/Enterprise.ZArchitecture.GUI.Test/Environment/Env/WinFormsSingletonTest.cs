using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	sealed class WinFormsSingletonTest : TestCase
	{
		public void TestWinFormsEnvIsSingleton()
		{
			BaseEnvironment firstInstance = Env.Instance;
			AssertNotNull(firstInstance);
			Assert(firstInstance is WinFormsEnvironment);
			BaseEnvironment secondInstance = Env.Instance;
			AssertNotNull(secondInstance);
			AssertEquals(firstInstance, secondInstance);
		}

		public void TestWinFormsEnvInstanceChangesWithDelegateChange()
		{
			var originalInstance = Env.Instance;
			AssertNotNull(originalInstance);
			var originalProvider = Env.GetCurrentProvider();
			var newProvider = new WinFormsEnvironmentProvider();

			try
			{
				newProvider.Enable();
				Assert(!Env.GetCurrentProvider().Equals(originalProvider));
				BaseEnvironment secondInstance = Env.Instance;
				Assert(!originalInstance.Equals(secondInstance));
			}
			finally
			{
				newProvider.Dispose();
				originalProvider.Enable();
			}
		}

		public void TestGetDbEnvironmentInstance()
		{
			AssertType(typeof(WinFormsEnvironment), Env.Instance);
			AssertType(typeof(WinFormsDbEnvironment), DbEnv.Instance);

			bool oldIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;
				((WinFormsEnvironmentProvider)Env.GetCurrentProvider()).Enable();
				AssertType(typeof(BaseDbEnvironment), DbEnv.Instance);
			}
			finally
			{
				Globals.IsUserInteractive = oldIsUserInteractive;
				((WinFormsEnvironmentProvider)Env.GetCurrentProvider()).Enable();
			}
		}
	}
}
