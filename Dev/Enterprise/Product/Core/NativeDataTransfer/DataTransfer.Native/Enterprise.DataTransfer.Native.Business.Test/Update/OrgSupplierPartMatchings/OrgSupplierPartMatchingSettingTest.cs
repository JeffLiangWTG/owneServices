using System.Linq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgSupplierPartMatchings
{
	public class OrgSupplierPartMatchingSettingTest : TestCase
	{
		public void TestEnableList()
		{
			var setting = new OrgSupplierPartMatchingSetting();

			Assert("Should enable Order only", true);
			AssertEquals(1, setting.EnableList.Count());
			AssertEquals("Order", setting.EnableList.ToList()[0]);
		}

		public void TestDisableList()
		{
			var setting = new OrgSupplierPartMatchingSetting();
			AssertEquals("Disable List should be empty", 0, setting.DisableList.Count());
		}
	}
}
