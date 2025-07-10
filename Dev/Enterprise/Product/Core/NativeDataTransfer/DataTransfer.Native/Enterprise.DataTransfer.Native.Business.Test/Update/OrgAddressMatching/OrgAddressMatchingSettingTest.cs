using System.Linq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgAddressMatching
{
	public class OrgAddressMatchingSettingTest : TestCase
	{
		public void TestEnableList()
		{
			var setting = new OrgAddressMatchingSetting();
			AssertEquals("Enable List should be empty", 0, setting.EnableList.Count());
		}

		public void TestDisableList()
		{
			var setting = new OrgAddressMatchingSetting();
			AssertEquals("Disable List should be empty", 0, setting.DisableList.Count());
		}
	}
}
