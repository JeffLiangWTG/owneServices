using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaResulSalidaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationExitResultAESResponseMessageProcessor : AESCommonInboxNotificationResponseMessageProcessor<ComunicaResulSalidaV1Sal>
	{
		public InboxNotificationExitResultAESResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Inbox Notification Export Exit Result Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameComunicaResulSalidaV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ExportExitResultCommunication };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaResulSalidaV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new InboxNotificationExitResultAESMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(ComunicaResulSalidaV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => ZString.Empty;

		const string XsdSchemaNameComunicaResulSalidaV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.ComunicaResulSalidaV1Sal.xsd";
	}
}
