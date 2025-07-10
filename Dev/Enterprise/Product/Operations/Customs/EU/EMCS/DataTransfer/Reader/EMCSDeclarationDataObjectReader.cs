using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.EMCS.DataTransfer
{
	public class EMCSDeclarationDataObjectReader : JobDeclarationDataObjectReader
	{
		public EMCSDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null)
			: base(declarationDataObject, logger, factory, forwardingShipment)
		{
		}

		protected override BaseJobDeclaration GetNewBusinessObjectCore()
		{
			return factory.New<EMCSJobDeclaration>();
		}

		protected override AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForDeclaration(BaseJobDeclaration declaration)
		{
			return new EMCSDeclarationAddInfoDataObjectReader(logger, Helper);
		}

		protected override CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, BaseJobDeclaration declaration, ILandedCostDataReader landedCostDataReader)
		{
			return new EMCSCustomsContainerDataObjectReader(containerDataObject, logger, Helper, (EMCSJobDeclaration)declaration, landedCostDataReader);
		}

		protected override CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(BaseJobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new EMCSCommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, (EMCSJobComInvoiceGroupHeader)groupHeader, dataObject, landedCostDataReader);
		}
	}
}
