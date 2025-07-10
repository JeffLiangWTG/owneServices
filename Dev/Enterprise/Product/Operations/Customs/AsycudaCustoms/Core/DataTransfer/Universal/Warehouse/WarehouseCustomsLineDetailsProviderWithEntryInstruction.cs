using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.AsycudaCustoms.DataTransfer.Universal
{
	public class WarehouseCustomsLineDetailsProviderWithEntryInstruction : WarehouseCustomsLineDetailsProviderWithEntryInstruction<JobDeclaration, JobComInvoiceHeader>
	{
		public WarehouseCustomsLineDetailsProviderWithEntryInstruction(Shipment shipment)
			: base(shipment)
		{
		}

		protected override CargoWise.Schema.ITableSchema GetDeclarationAddInfoSchema() => null;

		protected override CargoWise.Schema.ITableSchema GetInvoiceAddInfoSchema() => null;

		protected override Customs.DataTransfer.Universal.WarehouseCustomsLineDetailsWithEntryInstruction GetNewLineDetail(CargoWise.EntityFramework.BusinessObjectFactory factory, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail)
		{
			return new WarehouseCustomsLineDetailsWithEntryInstruction(factory, invoiceLine, fallbackDetail);
		}
	}
}
