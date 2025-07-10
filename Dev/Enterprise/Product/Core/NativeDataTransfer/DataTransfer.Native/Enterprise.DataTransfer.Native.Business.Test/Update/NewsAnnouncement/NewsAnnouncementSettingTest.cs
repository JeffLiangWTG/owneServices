using System.Linq;
using Enterprise.DataTransfer.Native.Business.Update.NewsAnnouncement;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.Testing
{
	public class NewsAnnouncementSettingTest : TestCase
	{
		public void TestEnableList()
		{
			var setting = new NewsAnnouncementSetting();
			AssertContainsExactElementsInExactOrder(new[] { "NewsAnnouncement" }, setting.EnableList);
		}

		public void TestDisableList()
		{
			var setting = new NewsAnnouncementSetting();
			AssertEquals("Disable List should be empty", 0, setting.DisableList.Count());
		}
	}
}
