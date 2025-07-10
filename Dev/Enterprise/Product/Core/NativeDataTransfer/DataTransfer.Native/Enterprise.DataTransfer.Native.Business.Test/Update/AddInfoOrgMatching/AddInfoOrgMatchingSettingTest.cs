using System.Linq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.AddInfoOrgMatching.Tests
{
	internal class AddInfoOrgMatchingSettingTest : TestCase
	{
		public void TestEnableList()
		{
			var setting = new AddInfoOrgMatchingSetting();
			AssertEquals(1, setting.EnableList.Count());
			AssertEquals("Product", setting.EnableList.ToList()[0]);
		}

		public void TestDisableList()
		{
			var setting = new AddInfoOrgMatchingSetting();
			AssertEquals("Disable List should be empty", 0, setting.DisableList.Count());
		}
	}
}
