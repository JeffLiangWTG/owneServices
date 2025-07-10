using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ES.DataTransfer.Universal
{
	public class JobDeclarationDataObjectReader : EU.DataTransfer.Universal.JobDeclarationDataObjectReader
	{
		public JobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null)
			: base(declarationDataObject, logger, factory, forwardingShipment)
		{
		}

		protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<EU.Business.Declaration.JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(EU.Business.Declaration.JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
			=> new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
	}
}
