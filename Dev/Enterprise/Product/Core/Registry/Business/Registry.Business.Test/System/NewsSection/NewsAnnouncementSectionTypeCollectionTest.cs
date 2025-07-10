using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NewsAnnouncementSectionTypeCollection))]
	sealed class NewsAnnouncementSectionTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<NewsAnnouncementSectionTypeCollection>
	{
		public void TestAllowNew()
		{
			var collection = new NewsAnnouncementSectionTypeCollection();
			AssertEquals(true, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = new NewsAnnouncementSectionTypeCollection();
			AssertEquals(true, collection.AllowRemove);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override NewsAnnouncementSectionTypeCollection GetCollectionToTest()
		{
			return new NewsAnnouncementSectionTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NewsAnnouncementSectionType();
		}
	}
}
