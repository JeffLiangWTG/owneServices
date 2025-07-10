using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.WarehouseIntegration
{
	public class WarehouseCustomsLineDetailsProvider : Customs.DataTransfer.Universal.WarehouseCustomsLineDetailsProvider
	{
		public WarehouseCustomsLineDetailsProvider(Shipment shipment) : base(shipment)
		{
		}

		protected override IWarehouseCustomsLineDetails GetNewLineDetail(CommercialInvoiceLine invoiceLine, CommercialInvoiceHeader invoice)
			=> new WarehouseCustomsLineDetails(Factory, invoiceLine, GetFallbackDetail(invoice), shipment);
	}
}
