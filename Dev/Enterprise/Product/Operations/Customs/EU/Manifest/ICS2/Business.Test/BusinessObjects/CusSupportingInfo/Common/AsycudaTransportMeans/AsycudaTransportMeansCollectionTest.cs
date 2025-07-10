using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaTransportMeansCollection))]
	sealed class AsycudaTransportMeansCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMaximumCollectionCount()
		{
			AssertEquals("Maximum count for collection is 99", 99, Collection.MaxCount);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			return new AsycudaTransportMeansCollection(masterBill);
		}
	}
}
