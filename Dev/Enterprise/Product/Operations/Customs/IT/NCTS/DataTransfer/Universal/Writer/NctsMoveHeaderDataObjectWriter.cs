using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.NCTS.DataTransfer;

public class NctsMoveHeaderDataObjectWriter : EU.NCTS.DataTransfer.Phase4.NctsMoveHeaderDataObjectWriter
{
	public NctsMoveHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, Shipment headerData) : base(manager, helper, headerData)
	{
	}

	protected override EU.NCTS.DataTransfer.Phase4.DepartureGoodsItemDataObjectWriter GetNewDepartureGoodsItemDataObjectWriter(IDataWritingManager writeManager, UniversalDataObjectWriterHelper helper, Shipment headerData, CommercialInvoiceHeader commercialInvoiceHeaderData)
	{
		return new DepartureGoodsItemDataObjectWriter(writeManager, helper, headerData, commercialInvoiceHeaderData);
	}

	protected override Shipment PopulateDataObject(EU.NCTS.Business.NctsCommonMovementHeader moveHeaderBO)
	{
		var shipment = base.PopulateDataObject(moveHeaderBO);
		if (moveHeaderBO is NctsDepartureMovementHeader departureMovementHeader)
		{
			PopulateElectronicFolderCustomsReference(departureMovementHeader, shipment);
			PopulateDefermentAccountNumber(departureMovementHeader, shipment);
		}
		return shipment;
	}

	void PopulateDefermentAccountNumber(NctsDepartureMovementHeader departureMovementHeader, Shipment shipment)
	{
		shipment.DefermentAccountNumber = departureMovementHeader.DefermentAccountNumber;
	}

	void PopulateElectronicFolderCustomsReference(NctsDepartureMovementHeader departureMovementHeader, Shipment shipment)
	{
		var customsReference = new CustomsReference()
		{
			Type = new CodeDescriptionPair() { Code = CustomsReferenceList.Codes.ElectronicDocuments, Description = CustomsReferenceList.Descriptions.ElectronicDocuments },
			Reference = departureMovementHeader.UseElectronicFolder.ToString(),
		};
		shipment.CustomsReferenceCollection.Add(customsReference);
	}
}
