using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaDisconformeSalidaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALInboxNotificationExitNonConformityAESResponseMessageProcessor : EALAESCommonInboxNotificationResponseMessageProcessor<ComunicaDisconformeSalidaV1Sal, EALInboxNotificationExitNonConformityAESMessagePrettyFormatter>
	{
		public EALInboxNotificationExitNonConformityAESResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore =>
			(NoResString)"Inbox Notification Export Exit Non-Conformity Declaration Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification };

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameComunicaDisconformeSalidaV1Sal;

		protected override EALInboxNotificationExitNonConformityAESMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaDisconformeSalidaV1Sal response) => new EALInboxNotificationExitNonConformityAESMessagePrettyFormatter(response);

		protected override void ProcessAcceptedDeclaration(ComunicaDisconformeSalidaV1Sal response, EDIMessage message, CusExitReport report)
		{
			report.CER_Status = AESEntryStatusList.Codes.Refused;
		}

		const string XsdSchemaNameComunicaDisconformeSalidaV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.ComunicaDisconformeSalidaV1Sal.xsd";
	}
}
