using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaDisconformeParV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class InboxNotifNonConformityResponseMessageProcessor : NCTS5CommonInboxNotificationResponseMessageProcessor<ComunicaDisconformeParV1Sal, InboxNotifNonConformityResponseMessagePrettyFormatter>
	{
		public InboxNotifNonConformityResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Inbox Notification NCTS Non-conformity Communication";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameDisconformeParTranV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture };

		protected override InboxNotifNonConformityResponseMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaDisconformeParV1Sal response, EDIMessage message, NctsHeader nctsHeader) => new InboxNotifNonConformityResponseMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(ComunicaDisconformeParV1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			if (!nctsHeader.IsDepartureMovement)
			{
				throw new InvalidOperationException(SetNctsMessageFailedLogDescription(nctsHeader, "Departure"));
			}

			var correctResponseData = response.DatosComunicacion;
			if (correctResponseData != null)
			{
				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;
			}

			return ZString.Empty;
		}

		const string XsdSchemaNameDisconformeParTranV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.ComunicaDisconformeParV1Sal.xsd";
	}
}
