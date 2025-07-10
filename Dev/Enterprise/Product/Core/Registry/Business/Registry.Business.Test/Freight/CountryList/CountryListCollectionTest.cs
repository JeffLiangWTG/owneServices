using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CountryListCollection))]
	sealed class CountryListCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CountryListCollection>
	{
		public void TestLoad()
		{
			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			Guid[] guid = { guid1, guid2 };

			AssertEquals("Collection.Count", 0, Collection.Count);

			Collection.Load(guid);
			AssertEquals("Collection.Count", 2, Collection.Count);

			AssertEquals("Collection[0].CountryPK", guid1, Collection[0].CountryPK);
			AssertEquals("Collection[1].CountryPK", guid2, Collection[1].CountryPK);
		}

		public void TestCountryPKs()
		{
			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			CountryListElement element1 = Collection.AddNew();
			CountryListElement element2 = Collection.AddNew();
			element1.CountryPK = guid1;
			element2.CountryPK = guid2;
			AssertEquals("Collection.CountryPKs.Length should be 2", 2, Collection.CountryPKs.Length);
			AssertEquals("Collection.CountryPKs[0] should be guid1", guid1, Collection.CountryPKs[0]);
			AssertEquals("Collection.CountryPKs[1] should be guid2", guid2, Collection.CountryPKs[1]);
		}

		public void TestInvalidCountryPKs()
		{
			ZGuid guid1 = ZGuid.Empty;
			ZGuid guid2 = ZGuid.Invalid;
			CountryListElement element1 = Collection.AddNew();
			CountryListElement element2 = Collection.AddNew();
			element1.CountryPK = guid1;
			element2.CountryPK = guid2;
			AssertEquals("Collection.CountryPKs.Length should be 2", 2, Collection.CountryPKs.Length);
			AssertEquals("Collection.CountryPKs[0] should be guid1", Guid.Empty, Collection.CountryPKs[0]);
			AssertEquals("Collection.CountryPKs[1] should be guid2", Guid.Empty, Collection.CountryPKs[1]);
		}

		public void TestCountryCollection()
		{
			AssertEquals("CountryCollection.GetType()", ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefCountryCollection>(), Collection.CountryCollection.GetType());
		}

		protected override CountryListCollection GetCollectionToTest()
		{
			return new CountryListCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CountryListElement(Collection);
		}
	}
}
