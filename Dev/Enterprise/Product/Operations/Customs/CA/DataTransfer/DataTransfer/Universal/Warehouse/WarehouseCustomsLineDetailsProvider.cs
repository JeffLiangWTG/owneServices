using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	class WarehouseCustomsLineDetailsProvider : WarehouseCustomsLineDetailsProvider<JobDeclaration, JobComInvoiceHeader>
	{
		public WarehouseCustomsLineDetailsProvider(Shipment shipment)
			: base(shipment)
		{
		}

		protected override IWarehouseCustomsLineDetails GetNewLineDetail(CommercialInvoiceLine invoiceLine, CommercialInvoiceHeader invoice)
		{
			return new WarehouseCustomsLineDetails(Factory, invoiceLine, GetFallbackDetail(invoice));
		}

		protected override ITableSchema GetDeclarationAddInfoSchema() => CAAddInfoSchema.Instance;

		protected override ITableSchema GetInvoiceAddInfoSchema() => CAAddInfoSchema.Instance;

		protected override IEnumerable<string> GetInvoiceLineAddInfosApplicableForInwardWarehousing()
		{
			return Factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing();
		}

		protected override WarehouseCustomsFallbackDetail GetNewDeclarationDetail()
		{
			var messageSubType = shipment.MessageSubType.Code.GetValueOrDefault();
			return new WarehouseCustomsFallbackDetail()
			{
				IsExWarehouse = B3EntryTypeList.IsExWarehouseEntryType(messageSubType) || CADEntryTypeList.IsExWarehouseEntryType(messageSubType),
				SupplierAddress = shipment.OrganizationAddressCollection.FindBestSupplierMatch(),
				InvoiceLineAddInfosApplicableForInwardWarehousing = InvoiceLineAddInfosApplicableForInwardWarehousing
			};
		}
	}
}
