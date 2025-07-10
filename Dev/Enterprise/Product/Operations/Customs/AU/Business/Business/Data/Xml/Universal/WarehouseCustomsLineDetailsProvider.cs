using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using static Enterprise.Customs.AU.Declaration.Business.WarehouseCustomsLineDetails;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class WarehouseCustomsLineDetailsProvider : DataTransfer.Universal.WarehouseCustomsLineDetailsProvider
	{
		public WarehouseCustomsLineDetailsProvider(Shipment shipment)
			: base(shipment)
		{
		}

		protected override IWarehouseCustomsLineDetails GetNewLineDetail(CommercialInvoiceLine invoiceLine, CommercialInvoiceHeader invoice)
		{
			return new WarehouseCustomsLineDetails(Factory, invoiceLine, invoice, (DeclarationDetail)GetFallbackDetail(invoice));
		}

		protected override DataTransfer.Universal.WarehouseCustomsFallbackDetail GetNewDeclarationDetail()
		{
			var messageType = shipment.MessageType.GetCodeAsUpperCase();
			var declarationDetail = new DeclarationDetail()
			{
				IsExWarehouse = messageType == JobMessageTypeList.Codes.ExWarehouse,
				IsWarehousedByExternalAgent = messageType == JobMessageTypeList.Codes.WarehousedByExternalAgent,
				SupplierAddress = shipment.OrganizationAddressCollection.FindBestSupplierMatch()
			};
			return declarationDetail;
		}
	}
}
