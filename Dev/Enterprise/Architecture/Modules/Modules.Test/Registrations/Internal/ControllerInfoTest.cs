using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ControllerInfoTest : TestCase
	{
		public void TestIsClientOverride()
		{
			AssertEquals("Non-client override", false, new ControllerInfo(ControllerIDs.GlbBranch, "", "").IsClientOverride);
			ClientOverrideControllerInfo info = new ClientOverrideControllerInfo(new ClientOverrideControllerID(ControllerIDs.GlbBranch), "", "");
			AssertEquals("Client override", true, info.IsClientOverride);
		}

		public void TestConstructors()
		{
			ClientOverrideControllerID controllerID = new ClientOverrideControllerID(ControllerIDs.GlbBranch);
			ClientOverrideControllerInfo info = new ClientOverrideControllerInfo(controllerID, "", "");
			AssertEquals(false, info.CanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist);

			info = new ClientOverrideControllerInfo(controllerID, "", "", "AU");
			AssertEquals(false, info.CanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist);

			info = new ClientOverrideControllerInfo(controllerID, "", "", true);
			AssertEquals(true, info.CanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist);

			info = new ClientOverrideControllerInfo(controllerID, "", "", false);
			AssertEquals(false, info.CanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist);
		}
	}
}
