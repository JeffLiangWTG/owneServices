using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business
{
	public abstract class AESCommonInboxNotificationResponseMessageProcessor<TResponse> : XMLResponseMessageProcessor<TResponse, IMessagePrettyFormatter>
		where TResponse : class, ICommonServiceSegment, IResponseCode, IMRNField
	{
		protected AESCommonInboxNotificationResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected sealed override ZString AcceptedResponseCode => ZString.Empty;
		protected sealed override ZBool IsOnlyAcceptedDeclaration => true;
		protected override ZBool IsInboxDeclaration => true;

		protected sealed override CusEntryHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
		{
			return MessageProcessorHelper.GetRelevantBusinessObjectFromMRNCode<TResponse>(message, XsdSchemaEmbeddedResourceName, sentBusinessObjects, serializeWithValidation: false, isAES: true);
		}

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
