using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestsSubclassesOf(typeof(EXDOCRefCodeCollection))]
	abstract class EXDOCRefCodeCollectionTest<T> : BusinessObjectCollectionTestCase where T : EXDOCRefCodeCollection
	{
		public void TestAdditionalFilter()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.CompleteFilter.IsNoResultQuery);

			var collection2 = GetCollectionWithNullTypeProviderToTest();
			AssertEquals(true, collection2.CompleteFilter.IsNoResultQuery);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => (T)Activator.CreateInstance(typeof(T), new object[] { Factory.New<QuarantineExDocHeader>() });

		protected virtual BusinessObjectCollection GetCollectionWithNullTypeProviderToTest() => (T)Activator.CreateInstance(typeof(T), new object[] { null });
	}
}
