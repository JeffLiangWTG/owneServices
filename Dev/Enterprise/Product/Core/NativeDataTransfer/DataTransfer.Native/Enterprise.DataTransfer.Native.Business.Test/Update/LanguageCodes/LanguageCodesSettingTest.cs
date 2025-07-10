using System.Linq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.LanguageCodes
{
	public class LanguageCodesSettingTest : TestCase
	{
		public void TestEnableList()
		{
			var setting = new LanguageCodesSetting();
			AssertEquals(1, setting.EnableList.Count());
			AssertEquals("Organization", setting.EnableList.ToList()[0]);
		}

		public void TestDisableList()
		{
			var setting = new LanguageCodesSetting();
			AssertEquals("Disable List should be empty", 0, setting.DisableList.Count());
		}
	}
}
