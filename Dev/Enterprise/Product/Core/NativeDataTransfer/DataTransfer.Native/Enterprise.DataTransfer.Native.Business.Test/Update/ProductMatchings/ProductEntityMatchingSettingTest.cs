using System.Linq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching
{
	public class ProductEntityMatchingSettingTest : TestCase
	{
		public void TestEnableList()
		{
			var setting = new ProductEntityMatchingSetting();

			Assert("Should enable Order only", true);
			AssertEquals(1, setting.EnableList.Count());
			AssertEquals("Product", setting.EnableList.ToList()[0]);
		}

		public void TestDisableList()
		{
			var setting = new ProductEntityMatchingSetting();
			AssertEquals("Disable List should be empty", 0, setting.DisableList.Count());
		}
	}
}
