using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class WebModuleIDTest : TestCase
	{
		public void TestIDs()
		{
			AssertNotNull(WebModuleId.OrgCarrierTracking);
		}

		public void TestInitialiseWebModuleID()
		{
			WebModuleID newID = new WebModuleID(WebModuleId.DummyWeb, "Test", "Test");
			AssertEquals(nameof(WebModuleId.DummyWeb), newID.ToString());
		}
	}
}
