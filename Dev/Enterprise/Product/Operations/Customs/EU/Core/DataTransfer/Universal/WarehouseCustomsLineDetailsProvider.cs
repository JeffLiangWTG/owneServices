using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Schema;
using BaseWarehouseCustomsFallbackDetailWithEntryInstruction = Enterprise.Customs.DataTransfer.Universal.WarehouseCustomsFallbackDetailWithEntryInstruction;
using BaseWarehouseCustomsLineDetailsProviderWithEntryInstruction = Enterprise.Customs.DataTransfer.Universal.WarehouseCustomsLineDetailsProviderWithEntryInstruction<Enterprise.Customs.EU.Business.Declaration.JobDeclaration, Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader>;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class WarehouseCustomsLineDetailsProvider : BaseWarehouseCustomsLineDetailsProviderWithEntryInstruction
	{
		public WarehouseCustomsLineDetailsProvider(Shipment shipment)
			: base(shipment)
		{
		}

		protected override WarehouseCustomsFallbackDetail GetFallbackDetail(CommercialInvoiceHeader invoice)
		{
			var fallbackDetails = WarehouseCustomsFallbackDetailWithEntryInstruction.CloneFrom((BaseWarehouseCustomsFallbackDetailWithEntryInstruction)base.GetFallbackDetail(invoice));
			fallbackDetails.CountryCode = shipment.GetSourceCountryCode();
			fallbackDetails.LinePriceCurrency = invoice.InvoiceCurrency?.Code?.ToString();
			return fallbackDetails;
		}

		protected override WarehouseCustomsLineDetailsWithEntryInstruction GetNewLineDetail(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, BaseWarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail)
		{
			return new WarehouseCustomsLineDetails(factory, invoiceLine, (WarehouseCustomsFallbackDetailWithEntryInstruction)fallbackDetail, shipment);
		}

		protected override ITableSchema GetDeclarationAddInfoSchema() => EUAddInfoSchema.Instance;

		protected override ITableSchema GetInvoiceAddInfoSchema() => EUAddInfoSchema.Instance;
	}
}
