using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.WarehouseIntegration
{
	public class WarehouseCustomsLineDetails : Customs.DataTransfer.Universal.WarehouseCustomsLineDetails
	{
		public WarehouseCustomsLineDetails(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, Customs.DataTransfer.Universal.WarehouseCustomsFallbackDetail fallbackDetail, UniversalDataBuss.DataObjects.Universal.Shipment shipment)
			: base(factory, invoiceLine, fallbackDetail, shipment)
		{
		}

		public override ZString? CountryOfDestination => shipment.GoodsDestination;
	}
}
