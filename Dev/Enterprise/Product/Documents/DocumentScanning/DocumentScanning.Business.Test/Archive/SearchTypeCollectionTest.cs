using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(SearchTypeCollection))]
	public class SearchTypeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SearchTypeCollection>
	{
		protected override SearchTypeCollection GetCollectionToTest()
		{
			return new SearchTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SearchType(new SearchTypeTest.DummyAssemblyData(), new ArchiveEDocsManager(new DocumentFactoryProvider().GetFactory(Factory)));
		}

		public void TestAllowNew()
		{
			SearchTypeCollection searchTypeCollection = new SearchTypeCollection();
			AssertEquals("AllowNew", false, searchTypeCollection.AllowNew);
		}

		public void TestIsAtLeastOneSelected()
		{
			SearchTypeCollection searchTypeCollection = new SearchTypeCollection();

			SearchType searchType1 = new SearchType(new SearchTypeTest.DummyAssemblyData(), new ArchiveEDocsManager(new DocumentFactoryProvider().GetFactory(Factory)));
			SearchType searchType2 = new SearchType(new SearchTypeTest.DummyAssemblyData(), new ArchiveEDocsManager(new DocumentFactoryProvider().GetFactory(Factory)));

			AssertEquals("IsAtLeastOneSelected", false, searchTypeCollection.IsAtLeastOneSelected);

			searchType1.IsFilterOn = false;
			searchType2.IsFilterOn = false;
			searchTypeCollection.Add(searchType1);
			searchTypeCollection.Add(searchType2);
			AssertEquals("IsAtLeastOneSelected", false, searchTypeCollection.IsAtLeastOneSelected);

			searchType1.IsFilterOn = true;
			searchType2.IsFilterOn = false;
			AssertEquals("IsAtLeastOneSelected", true, searchTypeCollection.IsAtLeastOneSelected);

			searchType1.IsFilterOn = false;
			searchType2.IsFilterOn = true;
			AssertEquals("IsAtLeastOneSelected", true, searchTypeCollection.IsAtLeastOneSelected);

			searchType1.IsFilterOn = true;
			searchType2.IsFilterOn = true;
			AssertEquals("IsAtLeastOneSelected", true, searchTypeCollection.IsAtLeastOneSelected);
		}
	}
}
