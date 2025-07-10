using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	class WarehouseCustomsLineDetails : Customs.DataTransfer.Universal.WarehouseCustomsLineDetails
	{
		public WarehouseCustomsLineDetails(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetail fallbackDetail)
			: base(factory, invoiceLine, fallbackDetail)
		{
		}
	}
}
