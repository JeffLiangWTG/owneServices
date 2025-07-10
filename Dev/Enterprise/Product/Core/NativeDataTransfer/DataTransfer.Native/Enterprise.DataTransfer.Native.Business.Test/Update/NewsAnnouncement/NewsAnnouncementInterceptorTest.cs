using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Business.Update.NewsAnnouncement;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Registry.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.Testing
{
	public class NewsAnnouncementInterceptorTest : TestCaseWithFactory
	{
		public void TestValidateSection()
		{
			var setting = new NewsAnnouncementSetting();
			var services = new AncillaryImportServices();
			var interceptor = new NewsAnnouncementInterceptor(setting, services) { Function = DummyMethod };

			AssertSectionThrowsException(NewsSectionTypeList.Codes.BorderWise, interceptor, services);
			AssertSectionThrowsException(NewsSectionTypeList.Codes.ProductUpdates, interceptor, services);
			AssertSectionThrowsException(NewsSectionTypeList.Codes.TechnicalAdvisoryNotes, interceptor, services);
			AssertSectionThrowsException(NewsSectionTypeList.Codes.WiseLearningUpdates, interceptor, services);
			AssertSectionThrowsException(NewsSectionTypeList.Codes.WiseNews, interceptor, services);
			AssertSectionThrowsException("Invalid", interceptor, services);
			AssertSectionThrowsNoException(NewsSectionTypeList.Codes.ClientAnnouncements, interceptor, services);
			AssertSectionThrowsNoException(NewsSectionTypeList.Codes.ClientNews, interceptor, services);
			AssertSectionThrowsNoException(NewsSectionTypeList.Codes.ClientStaffNews, interceptor, services);
		}

		public void TestValidateSection_NotSet()
		{
			var setting = new NewsAnnouncementSetting();
			var services = new AncillaryImportServices();
			var interceptor = new NewsAnnouncementInterceptor(setting, services) { Function = DummyMethod };

			var newsAnnouncement = new Entity(TestUtil.FindEntityDefinition("NewsAnnouncement", "GlbReleaseNote"), services);
			var newsAnnouncementSet = new EntitySet("NewsAnnouncement") { Root = newsAnnouncement };
			AssertExceptionThrown(
				typeof(NativeXMLUserVisibleException),
				$"NewsAnnouncement cannot be imported as section has not been provided.",
				() => interceptor.Invoke(newsAnnouncementSet));
		}

		void AssertSectionThrowsException(string section, NewsAnnouncementInterceptor interceptor, AncillaryImportServices services)
		{
			var newsAnnouncement = new Entity(TestUtil.FindEntityDefinition("NewsAnnouncement", "GlbReleaseNote"), services);
			var newsAnnouncementSet = new EntitySet("NewsAnnouncement") { Root = newsAnnouncement };
			newsAnnouncement["Section"] = section;
			AssertExceptionThrown(
				typeof(NativeXMLUserVisibleException),
				$"NewsAnnouncement cannot be imported as {section} section is invalid.",
				() => interceptor.Invoke(newsAnnouncementSet));
		}

		void AssertSectionThrowsNoException(string section, NewsAnnouncementInterceptor interceptor, AncillaryImportServices services)
		{
			var newsAnnouncement = new Entity(TestUtil.FindEntityDefinition("NewsAnnouncement", "GlbReleaseNote"), services);
			var newsAnnouncementSet = new EntitySet("NewsAnnouncement") { Root = newsAnnouncement };
			newsAnnouncement["Section"] = section;
			AssertNoExceptionThrown(() => interceptor.Invoke(newsAnnouncementSet));
		}

		static void DummyMethod(IEntitySet entitySet)
		{
		}
	}
}
