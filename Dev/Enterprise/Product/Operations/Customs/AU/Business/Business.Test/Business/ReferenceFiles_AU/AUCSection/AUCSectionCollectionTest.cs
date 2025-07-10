using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUCSectionCollection))]
	sealed class AUCSectionCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexerGetter()
		{
			collection.AddNew();
			Assert("Indexer should not return null", collection[0] != null);
		}

		public void TestNew()
		{
			AssertEquals("Type of added item should be AUCSection", typeof(AUCSection), collection.AddNew().GetType());
		}

		public void TestGetNearestNodeForCode_Import()
		{
			var collection = new AUCSectionCollection(EntryType.Import);
			var result = collection.GetNearestNodeForCode("0706.10.00 15");
			AssertType<AUCClass>(result);
			result = collection.GetNearestNodeForCode("0706.10.00 1522");
			AssertNull(result);
		}

		public void TestGetNearestNodeForCode_Export()
		{
			var collection = new AUCSectionCollection(EntryType.Export);
			var result = collection.GetNearestNodeForCode("0203.22.00");
			AssertType<AUCAHECC>(result);
			result = collection.GetNearestNodeForCode("0203.22.01 01");
			AssertNull(result);
		}

		[ExpectException(typeof(Exception))]
		public void TestIFindBoxListProviderList()
		{
			_ = ((IFindBoxListProvider)collection.AddNew()).List;
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AUCSectionCollection(Factory);

		AUCSectionCollection collection;
		protected override void SetUp()
		{
			base.SetUp();
			collection = new AUCSectionCollection(Factory);
		}
	}
}
