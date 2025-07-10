using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class StandaloneCommercialInvoiceDataObjectReader : DataTransfer.Universal.StandaloneCommercialInvoiceDataObjectReader
	{
		public StandaloneCommercialInvoiceDataObjectReader(UniversalShipment shipmentDataObject, UniversalCustoms.CommercialInvoiceHeader invoiceHeaderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipmentDataObject, invoiceHeaderDataObject, logger, factory)
		{
		}

		protected override DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(BaseJobComInvoiceGroupHeader groupHeader, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader);
		}
	}
}
