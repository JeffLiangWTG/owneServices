using System.Linq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgCountryData.Testing
{
	public class OrgCountryDataSettingTest : TestCase
	{
		public void TestEnableList()
		{
			var setting = new OrgCountryDataSetting();

			Assert("Should enable Order only", true);
			AssertEquals(1, setting.EnableList.Count());
			AssertEquals("Organization", setting.EnableList.ToList()[0]);
		}

		public void TestDisableList()
		{
			var setting = new OrgCountryDataSetting();
			AssertEquals("Disable List should be empty", 0, setting.DisableList.Count());
		}
	}
}
