using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCDOCC_v514.CCDOCCV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageSending.ESMessageSender;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class AnnexAESResponseMessageProcessor : AESCommonResponseMessageProcessor<Ccdoccv1Sal>
	{
		public AnnexAESResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Export Annex Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCCDOCCV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ExportAnnexes };

		protected override ZBool IsAnnexMessage(EDIMessage message) => true;

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(Ccdoccv1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new AnnexAESMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(Ccdoccv1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			return ProcessAcceptedDeclarationForAnnexAESAndT2LPOUS(message, entryHeader, GetMessageBuildersData);
		}

		protected override void ProcessRejectedDeclaration(Ccdoccv1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			ProcessRejectedDeclarationForAnnexAESAndT2LPOUS(message, entryHeader);
		}

		protected override List<MessageBuilderData> GetMessageBuildersData(CusEntryHeader entryHeader, CertificateObject certificateObject)
		{
			var builderManager = new ESMessageBuilderManager(DeclarationMessageTypeList.Codes.ExportAnnexes, DeclarationMessageSubTypeList.Codes.OriginalDeclaration, entryHeader, certificateObject);

			return ESMessageSender.GetAESAnnexesMessageBuilders(entryHeader, builderManager, entryHeader.ZG_RequestDispatch);
		}

		const string XsdSchemaNameCCDOCCV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.CCDOCCV1Sal.xsd";
	}
}
