using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business
{
	public abstract class AESCommonResponseMessageProcessor<TResponse> : XMLResponseMessageProcessor<TResponse, IMessagePrettyFormatter>
		where TResponse : class, ICommonServiceSegment, IResponseCode, IMRNField, ICommonErrors
	{
		protected AESCommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}
		protected const string IndirectFlagCode = "I";
		protected const string ResponseCodeL = "L";
		protected const string ResponseCodeB = "B";
		protected const string ResponseCodeC = "C";

		protected sealed override ZString AcceptedResponseCode => AESAndNCTS5ResponseTypeCodeList.Codes.AcceptedMessage;

		protected void SetMovementReferenceNumberAndCircuitCan(CusEntryHeader entryHeader, ZDateTime admisionDate, ZString circuitAEAT, ZString circuitoATC)
		{
			SetMovementReferenceNumber(entryHeader, admisionDate, GetCircuitCodeFromText(circuitAEAT));
			if (!circuitoATC.IsEmpty)
			{
				entryHeader.SetCircuitCan(GetCircuitCodeFromText(circuitoATC));
			}
		}

		protected void ProcessDocuments(CusEntryHeader entryHeader)
		{
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.ProcessExportEntryLineSupportingDocuments();
				entryLine.ProcessExportEntryLinePreviousDocuments();
				entryLine.ProcessExportEntryLineAdditionalInfos();
			}
		}

		protected void TriggerInboxRequestsAES(EDIMessage message, CusEntryHeader entryHeader)
		{
			var messageTypesList = entryHeader.GetInboxRequestMessageTypesForAES();

			TriggerInboxRequest(entryHeader, message, messageTypesList);
		}

		protected sealed override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new ExportAESDocumentRequest(businessObject, certName);

		protected sealed override TResponse DererializeMessage(EDIMessage message, string xsdSchemaEmbeddedResourceName)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				return ESXmlObjectSerializer.DeserializeWithoutValidation<TResponse>(xsdSchemaEmbeddedResourceName, bodyTextReader, isAES: true, isNCTS: false);
			}
		}
	}
}
