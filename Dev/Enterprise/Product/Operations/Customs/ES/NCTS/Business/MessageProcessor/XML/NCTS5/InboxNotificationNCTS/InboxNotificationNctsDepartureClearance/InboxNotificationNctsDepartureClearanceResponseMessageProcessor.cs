using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaLevanteParV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class InboxNotificationNctsDepartureClearanceResponseMessageProcessor : NCTS5CommonInboxNotificationResponseMessageProcessor<ComunicaLevanteParV1Sal, InboxNotificationNctsDepartureClearanceMessagePrettyFormatter>
	{
		public InboxNotificationNctsDepartureClearanceResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Inbox notification NCTS Departure clearance";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameComunicaLevanteParV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance };

		protected override InboxNotificationNctsDepartureClearanceMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaLevanteParV1Sal response, EDIMessage message, NctsHeader nctsHeader) => new InboxNotificationNctsDepartureClearanceMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(ComunicaLevanteParV1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			if (!nctsHeader.IsDepartureMovement)
			{
				throw new InvalidOperationException(SetNctsMessageFailedLogDescription(nctsHeader, "Departure"));
			}

			SetDepartureClearanceStatus(nctsHeader);
			SetEntryNumbers(message, nctsHeader, response.DatosComunicacion?.Mrn, response.DatosComunicacion?.CircuitoExpedicion, response.DatosComunicacion?.FechaLevante, response.DatosComunicacion?.CsVdeDat, ZString.Empty, response.DatosComunicacion?.FechaLevante, response.DatosComunicacion?.FechaLimiteLlegada, ZString.Empty, true);

			return ZString.Empty;
		}

		void SetDepartureClearanceStatus(NctsHeader nctsHeader)
		{
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
		}

		const string XsdSchemaNameComunicaLevanteParV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.ComunicaLevanteParV1Sal.xsd";
	}
}
