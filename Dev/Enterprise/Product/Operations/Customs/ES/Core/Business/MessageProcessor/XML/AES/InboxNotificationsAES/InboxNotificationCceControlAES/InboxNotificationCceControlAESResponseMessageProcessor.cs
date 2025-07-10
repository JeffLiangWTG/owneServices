using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaControlesCCEV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationCceControlAESResponseMessageProcessor : AESCommonInboxNotificationResponseMessageProcessor<ComunicaControlesCcev1Sal>
	{
		public InboxNotificationCceControlAESResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Inbox Notification Export CCE Control Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameComunicaControlesCCEV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ExportCceControlCommunication };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaControlesCcev1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new InboxNotificationCceControlAESMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(ComunicaControlesCcev1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.ControlsAtEuOffice;

			return ZString.Empty;
		}

		const string XsdSchemaNameComunicaControlesCCEV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.ComunicaControlesCCEV1Sal.xsd";
	}
}
