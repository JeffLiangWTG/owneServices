using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(TransportMeanCollection))]
	sealed class TransportMeanCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaulting()
		{
			var transportCollection = (TransportMeanCollection)GetCollectionToTest();
			var transportMean = transportCollection.AddNew();

			AssertEquals("ASY", transportMean.JW_ParentType);
			AssertEquals("ROA", transportMean.JW_TransportMode);
		}

		public void TestDefaultingLegOrderForNewElements()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "IL";
			var transportCollection = new TransportMeanCollection(header);

			var transport1 = transportCollection.AddNew();
			AssertEquals("First Transport Leg Order", (byte)1, transport1.JW_LegOrder);

			var transport2 = transportCollection.AddNew();
			AssertEquals("Second Transport Leg Order", (byte)2, transport2.JW_LegOrder);

			transportCollection.AddNew().JW_LegOrder = 9;
			var transportX = transportCollection.AddNew();
			AssertEquals("Last Transport Leg Order", (byte)10, transportX.JW_LegOrder);

			transport2.JW_LegOrder = 255;
			AssertEquals("When existing leg has reached the maximum leg order value (255), LegOrder for a new one", (byte)255, transportCollection.AddNew().JW_LegOrder);
		}

		public void TestAddNewType()
		{
			var transportCollection = (TransportMeanCollection)GetCollectionToTest();
			var transportMean = transportCollection.AddNew();
			AssertType<TransportMean>(transportMean);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => Factory.New<AsycudaManifestHeader>().TransportMeans;
	}
}
