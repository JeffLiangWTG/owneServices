using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.FR.DataTransfer.Universal
{
	public class WarehouseCustomsLineDetailsProvider : EU.DataTransfer.Universal.WarehouseCustomsLineDetailsProvider
	{
		public WarehouseCustomsLineDetailsProvider(Shipment shipment) : base(shipment)
		{
		}

		protected override WarehouseCustomsLineDetailsWithEntryInstruction GetNewLineDetail(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail)
		{
			return new WarehouseCustomsLineDetails(factory, invoiceLine, fallbackDetail, shipment);
		}
	}
}
