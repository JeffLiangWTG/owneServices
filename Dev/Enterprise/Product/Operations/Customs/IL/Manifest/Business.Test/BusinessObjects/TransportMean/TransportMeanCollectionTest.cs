using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(TransportMeanCollection))]
	sealed class TransportMeanCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestAddNewType()
		{
			var transportCollection = (TransportMeanCollection)GetCollectionToTest();
			var transportMean = transportCollection.AddNew();
			AssertType<TransportMean>(transportMean);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => Factory.New<AsycudaManifestHeader>().TransportMeans;
	}
}
