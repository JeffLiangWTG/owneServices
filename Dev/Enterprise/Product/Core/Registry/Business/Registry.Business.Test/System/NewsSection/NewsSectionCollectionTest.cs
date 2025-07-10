using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NewsSectionCollection))]
	sealed class NewsSectionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<NewsSectionCollection>
	{
		public void TestAllowNew()
		{
			var collection = new NewsSectionCollection();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = new NewsSectionCollection();
			AssertEquals(false, collection.AllowRemove);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override NewsSectionCollection GetCollectionToTest()
		{
			return new NewsSectionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NewsSection();
		}
	}
}
