using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.AutoRefresh.Testing
{
	[TestedType(typeof(AutoRefreshManager))]
	sealed class AutoRefreshManagerTest : RegistryItemSetTestCase<AutoRefreshManager>
	{
		#region TestInstance

		public void TestInstance()
		{
			AssertNotNull(AutoRefreshManager.Instance);
			AssertEquals(AutoRefreshManager.Instance, AutoRefreshManager.Instance);
		}

		#endregion

		#region TestGetTimeoutDescription

		public void TestGetTimeoutDescription()
		{
			AssertEquals("", AutoRefreshManager.Instance.GetTimeoutDescription(0));
			AssertEquals("1 Minute", AutoRefreshManager.Instance.GetTimeoutDescription(1));
			AssertEquals("7 Minutes", AutoRefreshManager.Instance.GetTimeoutDescription(7));
			AssertEquals("30 Minutes", AutoRefreshManager.Instance.GetTimeoutDescription(30));
			AssertEquals("Hour", AutoRefreshManager.Instance.GetTimeoutDescription(60));
			AssertEquals("Hour and 1 Minute", AutoRefreshManager.Instance.GetTimeoutDescription(61));
			AssertEquals("Hour and 20 Minutes", AutoRefreshManager.Instance.GetTimeoutDescription(80));
			AssertEquals("2 Hours", AutoRefreshManager.Instance.GetTimeoutDescription(120));
			AssertEquals("2 Hours and 1 Minute", AutoRefreshManager.Instance.GetTimeoutDescription(121));
			AssertEquals("2 Hours and 45 Minutes", AutoRefreshManager.Instance.GetTimeoutDescription(165));
		}

		#endregion

		#region TestIsAutoRefreshEnabled

		public void TestIsAutoRefreshEnabled()
		{
			AssertEquals(false, AutoRefreshManager.Instance.IsAutoRefreshEnabled(DummyModuleIDs.Dummy));

			AutoRefreshManager.Instance.SetAutoRefreshTimeOut(DummyModuleIDs.Dummy, true, 5);
			AssertEquals(true, AutoRefreshManager.Instance.IsAutoRefreshEnabled(DummyModuleIDs.Dummy));

			EnvProxy.Instance.Security.AutoRefreshModuleGrids.IsAllowed = false;
			AssertEquals(false, AutoRefreshManager.Instance.IsAutoRefreshEnabled(DummyModuleIDs.Dummy));
		}

		#endregion

		#region TestGetAndSetAutoRefreshTimeOut

		public void TestSetAutoRefreshTimeOut()
		{
			AssertEquals("Default value", false, AutoRefreshManager.Instance.IsAutoRefreshEnabled(DummyModuleIDs.Dummy));
			AssertEquals("Default value", (byte)5, AutoRefreshManager.Instance.GetAutoRefreshTimeOut(DummyModuleIDs.Dummy));

			AutoRefreshManager.Instance.SetAutoRefreshTimeOut(DummyModuleIDs.Dummy, true, 15);
			AssertEquals(true, AutoRefreshManager.Instance.IsAutoRefreshEnabled(DummyModuleIDs.Dummy));
			AssertEquals((byte)15, AutoRefreshManager.Instance.GetAutoRefreshTimeOut(DummyModuleIDs.Dummy));
		}

		#endregion

		#region TestItemsAreAddedProperly

		public override void TestItemsAreAddedProperly()
		{
			base.TestItemsAreAddedProperly();
			Assert("This class has no RegistryItem properties to test, but calls base in case some are added later.", true);
		}

		#endregion
	}
}
