using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class IDataObjectExtensionsTest : TestCase
	{
		public void TestAdditionalSetup()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				.AdditionalSetup(x => x.GoodsDescription = "HELLO WORLD")
				.AdditionalSetup(x =>
					x.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()));
			AssertEquals("GoodsDescription", "HELLO WORLD", shipment.GoodsDescription);
			AssertNotNull("AdditionalReferenceCollection.Count", shipment.AdditionalReferenceCollection);
		}
	}
}
