using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class BRJobDeclarationDataObjectReader : JobDeclarationDataObjectReader
	{
		public BRJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null) : base(declarationDataObject, logger, factory, forwardingShipment)
		{
		}

		protected override CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(BaseJobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new BRInvoiceHeaderDataObjectReader(groupHeader, invoiceData, logger, Helper, dataObject, landedCostDataReader);
		}

		protected override CustomsEntryInstructionDataObjectReader CreateCustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, BaseJobDeclaration declaration)
		{
			return new BREntryInstructionDataObjectReader(entryInstructionDataObject, logger, Helper, factory, declaration);
		}
	}
}
