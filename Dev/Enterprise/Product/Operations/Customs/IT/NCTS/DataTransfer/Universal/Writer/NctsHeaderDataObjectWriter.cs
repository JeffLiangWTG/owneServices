using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.NCTS.DataTransfer;

public class NctsHeaderDataObjectWriter : EU.NCTS.DataTransfer.Phase4.NctsHeaderDataObjectWriter
{
	public NctsHeaderDataObjectWriter(IDataWritingManager manager) : base(manager)
	{
	}

	protected override void PopulateCountryData(EU.NCTS.Business.NctsHeader baseHeaderBO, Shipment headerData)
	{
		base.PopulateCountryData(baseHeaderBO, headerData);
		var headerBO = (NctsHeader)baseHeaderBO;
		headerData.DeclarantType = ListHelper.GetWithDescription<CodeDescriptionPair>(headerBO.RepresentationType, headerBO.Lookups.RepresentationTypeList);
		headerData.AddOrgAddress(writeManager, headerBO.DeclarantAddress, AddressTypes.Declarant);
		PopulateAuthorizationNumberCustomsReference(headerBO, headerData);
	}

	protected override EU.NCTS.DataTransfer.Phase4.NctsMoveHeaderDataObjectWriter GetNewMoveHeaderDataObjectWriter(Shipment headerData) => new NctsMoveHeaderDataObjectWriter(writeManager, helper, headerData);

	void PopulateAuthorizationNumberCustomsReference(NctsHeader baseHeaderBO, Shipment shipment)
	{
		var customsReference = new CustomsReference()
		{
			Type = new CodeDescriptionPair() { Code = CustomsReferenceList.Codes.AuthorizationNumber, Description = CustomsReferenceList.Descriptions.AuthorizationNumber },
			Reference = baseHeaderBO.Authorization,
		};
		shipment.CustomsReferenceCollection.Add(customsReference);
	}

	protected override CustomsProfileType CustomsProfileIdentifierType => CustomsProfileType.Node;
}
