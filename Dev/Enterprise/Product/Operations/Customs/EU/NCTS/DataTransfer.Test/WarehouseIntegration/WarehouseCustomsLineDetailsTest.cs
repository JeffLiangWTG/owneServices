using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.WarehouseIntegration.Testing
{
	sealed class WarehouseCustomsLineDetailsTest : TestCaseWithFactory
	{
		public void TestCountryOfDestination()
		{
			var shipment = new UniversalDataBuss.DataObjects.Universal.Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				GoodsDestination = "DE"
			};
			var lineDetails = new WarehouseCustomsLineDetails(Factory, new CommercialInvoiceLine(), new WarehouseCustomsFallbackDetailWithEntryInstruction(), shipment);
			AssertEquals("DE", lineDetails.CountryOfDestination);
		}
	}
}
