using System.Linq;
using Enterprise.DataTransfer.Native.Business.Update.OrgOpportunity;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.Testing
{
	public class OrgOpportunitySettingTest : TestCase
	{
		public void TestEnableList()
		{
			var setting = new OrgOpportunitySetting();

			AssertEquals(2, setting.EnableList.Count());
			var enableList = setting.EnableList.ToList();
			AssertEquals("Organization", enableList[0]);
			AssertEquals("Opportunity", enableList[1]);
		}

		public void TestDisableList()
		{
			var setting = new OrgOpportunitySetting();
			AssertEquals("Disable List should be empty", 0, setting.DisableList.Count());
		}
	}
}
