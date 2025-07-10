using Enterprise.Customs.CH.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CH.DataTransfer;

public class DeclarationDataObjectReader : JobDeclarationDataObjectReader<JobDeclaration, Bill, CusContainer, JobComInvoiceGroupHeader>
{
	public DeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null) : base(declarationDataObject, logger, factory, shipment)
	{
	}

	protected override Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectReader CreateCustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, JobDeclaration declaration)
	{
		return new CustomsEntryInstructionDataObjectReader(entryInstructionDataObject, logger, Helper, factory, declaration);
	}

	protected override CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
	{
		return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
	}

	protected override CustomsContainerDataObjectReader<JobDeclaration, CusContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, JobDeclaration declaration, ILandedCostDataReader landedCostDataReader)
	{
		return new CustomsContainerDataObjectReader<JobDeclaration, CusContainer>(containerDataObject, logger, Helper, declaration, landedCostDataReader);
	}

	protected override AdditionalBillDataObjectReader<Bill> GetNewAdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, AdditionalBillDataProvider<Bill> additionalBillDataProvider, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
	{
		return new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, Helper, additionalBillDataProvider, primaryMasterBillDetail, primaryHouseBillDetail);
	}
}
