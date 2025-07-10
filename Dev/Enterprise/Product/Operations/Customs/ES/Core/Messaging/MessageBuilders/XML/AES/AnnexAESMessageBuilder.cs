using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCDOCC_v514.CCDOCCV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class AnnexAESMessageBuilder : AESCommonMessageBuilder<IAnnexAESMessageDataProvider, Ccdoccv1Ent>
	{
		public AnnexAESMessageBuilder(IAnnexAESMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CCDOCC";

		protected override Ccdoccv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Ccdoccv1Ent>();
			if (declaration != null)
			{
				declaration.Ccdocc = GetPopulatedCCDOCC();
			}
			return declaration;
		}

		CcdoccType GetPopulatedCCDOCC()
		{
			var cCDOCC = GetPopulatedMessage<CcdoccType>();
			if (cCDOCC != null)
			{
				cCDOCC.ExportOperation = GetPopulatedExportOperation();
				cCDOCC.SolicitudDespacho = GetPopulatedDispatchRequest();
				cCDOCC.DocumentoDigitalizado = provider.Documents.ConvertToCollection(GetPopulatedDocuments);
			}
			return cCDOCC;
		}

		ExportOperationTypeDoc GetPopulatedExportOperation()
		{
			var exportOperation = provider.ExportOperation;
			return exportOperation == null ? null : new ExportOperationTypeDoc { Mrn = exportOperation.MRN };
		}

		SolicitudDespachoTypeDoc GetPopulatedDispatchRequest()
		{
			var dispatchRequestCode = provider.DispatchRequestCode;
			return dispatchRequestCode.IsEmpty ? null : new SolicitudDespachoTypeDoc { FinAnexos = dispatchRequestCode };
		}

		DocumentoDigitalizadoTypeDoc GetPopulatedDocuments(IAnnexDocCommon providerDocument)
		{
			var document = default(DocumentoDigitalizadoTypeDoc);
			if (providerDocument != null)
			{
				document = new DocumentoDigitalizadoTypeDoc()
				{
					Descripcion = providerDocument.Description,
					NumeroReferencia = providerDocument.ReferenceNumber,
					Contenido = providerDocument.Image,
					Extension = providerDocument.Extension
				};
			}

			return document;
		}
	}
}
