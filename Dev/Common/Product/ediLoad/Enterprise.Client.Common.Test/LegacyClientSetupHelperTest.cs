using NUnit.Framework;

namespace Enterprise.Client.Common.Testing
{
	class LegacyClientSetupHelperTest : TestCase
	{
		public void TestGetInstanceDescription()
		{
			AssertEquals("InstanceDescription", "ediEnterprise Client", LegacyClientSetupHelper.GetInstanceDescription("ediEnterprise"));
			AssertEquals("InstanceDescription", "ediEnterprise Client (moo)", LegacyClientSetupHelper.GetInstanceDescription("moo"));
		}

		public void TestGetInstanceName()
		{
			AssertEquals("InstanceName", "x", LegacyClientSetupHelper.GetInstanceName("x", "y"));
			AssertEquals("InstanceName", "y", LegacyClientSetupHelper.GetInstanceName(string.Empty, "y"));
			AssertEquals("InstanceName", "ediEnterprise", LegacyClientSetupHelper.GetInstanceName(string.Empty, string.Empty));
		}

		public void TestIsInstanceSubKey()
		{
			AssertEquals(false, LegacyClientSetupHelper.IsInstanceSubKey(null));
			AssertEquals(false, LegacyClientSetupHelper.IsInstanceSubKey("Ins.3"));
			AssertEquals(false, LegacyClientSetupHelper.IsInstanceSubKey("Instance."));
			AssertEquals(false, LegacyClientSetupHelper.IsInstanceSubKey("Instance.a"));
			AssertEquals(true, LegacyClientSetupHelper.IsInstanceSubKey("Instance.1"));
			AssertEquals(true, LegacyClientSetupHelper.IsInstanceSubKey("Instance.20"));
		}
	}
}