using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCDOTC_v515.CCDOTCV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class AnnexNCTSMessageBuilder : NCTSCommonMessageBuilder<IAnnexNCTSMessageDataProvider, Ccdotcv1Ent>
	{
		public AnnexNCTSMessageBuilder(IAnnexNCTSMessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CCDOTC";

		protected override Ccdotcv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Ccdotcv1Ent>();
			if (declaration != null)
			{
				declaration.Ccdotc = GetPopulatedCCDOTCType();
			}
			return declaration;
		}

		CcdotcType GetPopulatedCCDOTCType()
		{
			var cCDOTCv515 = GetPopulatedMessage<CcdotcType>();
			if (cCDOTCv515 != null)
			{
				cCDOTCv515.TransitOperation = GetPopulatedCommonTransitOperationMRN<TransitOperationTypeDot>(provider.TransitOperation);
				cCDOTCv515.SolicitudDespacho = GetPopulatedDispatchRequest();
				cCDOTCv515.DocumentoDigitalizado = provider.Documents.ConvertToCollection(GetPopulatedAnnexesCommon<DocumentoDigitalizadoTypeDot>);
			}
			return cCDOTCv515;
		}

		SolicitudDespachoTypeDot GetPopulatedDispatchRequest()
		{
			var dispatchRequestCode = provider.DispatchRequestCode;
			return dispatchRequestCode.IsEmpty ? null : new SolicitudDespachoTypeDot { FinAnexos = dispatchRequestCode };
		}
	}
}
