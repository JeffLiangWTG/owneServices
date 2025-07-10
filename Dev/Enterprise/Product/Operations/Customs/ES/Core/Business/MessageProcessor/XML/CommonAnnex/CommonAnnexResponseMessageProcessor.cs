using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.EnvioDeDocumentosV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.MessageSending.ESMessageSender;

namespace Enterprise.Customs.ES.Business
{
	public class CommonAnnexResponseMessageProcessor : XMLResponseMessageProcessor<EnvioDeDocumentosV1Sal, IMessagePrettyFormatter>
	{
		public CommonAnnexResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Common Annex Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameEnvioDeDocumentosV1Sal;

		protected sealed override ZString AcceptedResponseCode => ResponseMessageCodeList.AcceptedCode;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.T2lDocumentationPous };

		protected override ZBool IsAnnexMessage(EDIMessage message) => true;

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(EnvioDeDocumentosV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new CommonAnnexMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(EnvioDeDocumentosV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			return ProcessAcceptedDeclarationForAnnexAESAndT2LPOUS(message, entryHeader, GetMessageBuildersData);
		}

		protected override void ProcessRejectedDeclaration(EnvioDeDocumentosV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			ProcessRejectedDeclarationForAnnexAESAndT2LPOUS(message, entryHeader);
		}

		protected override List<MessageBuilderData> GetMessageBuildersData(CusEntryHeader entryHeader, CertificateObject certificateObject)
		{
			var builderManager = new ESMessageBuilderManager(DeclarationMessageTypeList.Codes.T2lDocumentationPous, DeclarationMessageSubTypeList.Codes.OriginalDeclaration, entryHeader, certificateObject);

			return ESMessageSender.GetCommonAnnexMessageBuilders(entryHeader, builderManager, entryHeader.ZG_RequestDispatch);
		}

		protected override EnvioDeDocumentosV1Sal DererializeMessage(EDIMessage message, string xsdSchemaEmbeddedResourceName)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				return ESXmlObjectSerializer.DeserializeWithoutValidation<EnvioDeDocumentosV1Sal>(xsdSchemaEmbeddedResourceName, bodyTextReader, false, false);
			}
		}

		const string XsdSchemaNameEnvioDeDocumentosV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.Incoming.EnvioDeDocumentosV1Sal.xsd";
	}
}
