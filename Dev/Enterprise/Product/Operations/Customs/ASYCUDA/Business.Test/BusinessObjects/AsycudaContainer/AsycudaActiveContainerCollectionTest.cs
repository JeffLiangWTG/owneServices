using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaActiveContainerCollection))]
	sealed class AsycudaActiveContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<AsycudaActiveContainerCollection>
	{
		public void TestIndexer()
		{
			var collection = (ICusInBondContainerCollection)GetCollectionToTest();
			var container = collection.AddNew();
			AssertSame(container, collection[0]);
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaActiveContainerCollection);

		protected override AsycudaActiveContainerCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return (AsycudaActiveContainerCollection)header.ContainersAsICusInBondContainerCollectionForSynching;
		}
	}
}
