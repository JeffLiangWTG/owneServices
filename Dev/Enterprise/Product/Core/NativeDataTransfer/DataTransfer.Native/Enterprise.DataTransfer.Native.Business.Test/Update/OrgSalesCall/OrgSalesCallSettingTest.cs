using System.Linq;
using Enterprise.DataTransfer.Native.Business.Update.OrgSalesCall;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.Testing
{
	class OrgSalesCallSettingTest : TestCase
	{
		public void TestEnableList()
		{
			var setting = new OrgSalesCallSetting();

			AssertEquals(2, setting.EnableList.Count());
			var enableList = setting.EnableList.ToList();
			AssertEquals("Organization", enableList[0]);
			AssertEquals("Communication", enableList[1]);
		}

		public void TestDisableList()
		{
			var setting = new OrgSalesCallSetting();
			AssertEquals("Disable List should be empty", 0, setting.DisableList.Count());
		}
	}
}
