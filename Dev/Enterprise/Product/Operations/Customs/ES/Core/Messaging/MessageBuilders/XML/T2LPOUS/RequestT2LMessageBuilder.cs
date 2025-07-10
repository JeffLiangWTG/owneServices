using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01V1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
public class RequestT2LMessageBuilder : T2LPOUSCommonMessageBuilder<IRequestT2LMessageDataProvider, Iep01Type>
{
	public RequestT2LMessageBuilder(IRequestT2LMessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
	{
	}

	protected override Iep01Type GenerateXMLMessage() => GetPopulatedIEP01Type();

	Iep01Type GetPopulatedIEP01Type()
	{
		var iEP01Type = GetPopulatedTypeCommon<Iep01Type, MessageTd, PersonReqPresType, AddressType, ContactPersonInformationTypeEs>();
		GetPopulatedTypeCommonRequestAndReception<AuthorisationType
												, RepresentativeDataType
												, GoodsShipmentForT2Lt2LfTypeEs
												, PersonReqPresType
												, ContactPersonInformationTypeEs
												, TransportEquipmentType
												, AdditionalInformationType
												, DocumentType02Es
												, GoodsItemForT2Lt2LfType
												, CommodityCodeType
												, GoodsMeasureType
												, PackagingType>(iEP01Type, provider);

		if (iEP01Type != null)
		{
			iEP01Type.ProofOperationInformationForT2Lt2Lf = GetPopulatedProofInformation(provider.ProofOperationInformationForT2LT2LF);
		}
		return iEP01Type;
	}

	CentralProofInformationForT2Lt2LfType GetPopulatedProofInformation(IT2LPOUSRequestProofOperationInformationForT2LT2LF proofInformationProvider)
	{
		var proofInformation = default(CentralProofInformationForT2Lt2LfType);
		if (proofInformationProvider != null)
		{
			proofInformation = new CentralProofInformationForT2Lt2LfType()
			{
				Lrn = proofInformationProvider.LRN,
			};
			GetPopulatedProofInformationCommon<RequestedValidityOfTheProofType>(proofInformationProvider, proofInformation);
		}
		return proofInformation;
	}
}
