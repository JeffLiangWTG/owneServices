using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01INDV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
public class ReceptionT2LMessageBuilder : T2LPOUSCommonMessageBuilder<IReceptionT2LMessageDataProvider, Iep01IndType>
{
	public ReceptionT2LMessageBuilder(IReceptionT2LMessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
	{
	}

	protected override Iep01IndType GenerateXMLMessage() => GetPopulatedIEP01INDType();

	Iep01IndType GetPopulatedIEP01INDType()
	{
		var iEP01INDType = GetPopulatedTypeCommon<Iep01IndType, MessageTd, PersonReqPresType, AddressType, ContactPersonInformationTypeEs>();
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
												, PackagingType>(iEP01INDType, provider);

		if (iEP01INDType != null)
		{
			iEP01INDType.ProofOperationInformationForT2Lt2Lf = GetPopulatedProofInformation(provider.ProofOperationInformationForT2LT2LF);
			iEP01INDType.TipoAltaIndirecta = provider.TipoAltaIndirecta;
			iEP01INDType.EnvioDocumentos = GetPopulatedEnvioDocumento(provider.EnvioDocumentos);
		}

		return iEP01INDType;
	}

	CentralProofInformationForT2Lt2LfType GetPopulatedProofInformation(IT2LPOUSReceptionProofOperationInformationForT2LT2LF proofInformationProvider)
	{
		var proofInformation = default(CentralProofInformationForT2Lt2LfType);
		if (proofInformationProvider != null)
		{
			proofInformation = new CentralProofInformationForT2Lt2LfType()
			{
				CodigoReferencia = proofInformationProvider.CodigoReferencia,
			};
			GetPopulatedProofInformationCommon<RequestedValidityOfTheProofType>(proofInformationProvider, proofInformation);
		}
		return proofInformation;
	}

	Collection<DocumentoType> GetPopulatedEnvioDocumento(IReadOnlyCollection<IAnnexDocCommon> documentoProvider)
	{
		var documents = new Collection<DocumentoType>();
		if (documentoProvider != null)
		{
			foreach (var doc in documentoProvider)
			{
				documents.Add(new DocumentoType()
				{
					Descripcion = doc.Description,
					ReferenciaDelDocumento = doc.ReferenceNumber,
					DocumentoAnexo = doc.Image,
					ExtensionDelDocumento = doc.Extension
				});
			}
		}
		return documents;
	}
}
