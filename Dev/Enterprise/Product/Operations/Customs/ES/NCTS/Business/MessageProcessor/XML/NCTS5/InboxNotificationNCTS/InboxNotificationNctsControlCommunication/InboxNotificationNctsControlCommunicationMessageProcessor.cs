using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaControlesParV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class InboxNotificationNctsControlCommunicationMessageProcessor : NCTS5CommonInboxNotificationResponseMessageProcessor<ComunicaControlesParV1Sal, InboxNotificationNctsControlCommunicationMessagePrettyFormatter>
	{
		public InboxNotificationNctsControlCommunicationMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Inbox Notification NCTS Controls";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameComunicaControlesParV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsControls };

		protected override InboxNotificationNctsControlCommunicationMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaControlesParV1Sal response, EDIMessage message, NctsHeader nctsHeader) => new InboxNotificationNctsControlCommunicationMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(ComunicaControlesParV1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			if (!nctsHeader.IsDepartureMovement)
			{
				throw new InvalidOperationException(SetNctsMessageFailedLogDescription(nctsHeader, "Departure"));
			}

			SetDepartureCustomsControlStatus(nctsHeader);

			return ZString.Empty;
		}

		void SetDepartureCustomsControlStatus(NctsHeader nctsHeader)
		{
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
		}

		const string XsdSchemaNameComunicaControlesParV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.ComunicaControlesParV1Sal.xsd";
	}
}
