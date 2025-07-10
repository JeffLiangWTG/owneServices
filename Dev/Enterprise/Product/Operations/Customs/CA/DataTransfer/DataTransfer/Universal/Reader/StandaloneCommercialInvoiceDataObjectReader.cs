using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class StandaloneCommercialInvoiceDataObjectReader : Customs.DataTransfer.Universal.StandaloneCommercialInvoiceDataObjectReader<JobComInvoiceHeader, JobComInvoiceGroupHeader>
	{
		public StandaloneCommercialInvoiceDataObjectReader(UniversalShipment shipmentDataObject, UniversalCustoms.CommercialInvoiceHeader invoiceHeaderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipmentDataObject, invoiceHeaderDataObject, logger, factory)
		{
		}

		protected new UniversalDataObjectReaderHelper Helper
		{
			get { return (UniversalDataObjectReaderHelper)base.Helper; }
		}

		protected override Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper CreateNewUniversalDataObjectReaderHelper()
		{
			return new UniversalDataObjectReaderHelper(factory, dataObject.GetTargetCountryCode());
		}

		protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader);
		}
	}
}
