using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaInvaliTranV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class InboxNotificationNctsInvalidationTransitMessageProcessor : NCTS5CommonInboxNotificationResponseMessageProcessor<ComunicaInvaliTranV1Sal, InboxNotificationNctsInvalidationTransitMessagePrettyFormatter>
	{
		public InboxNotificationNctsInvalidationTransitMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Inbox Notification NCTS Invalidation Comunication";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameComunicaInvaliTranV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation };

		protected override InboxNotificationNctsInvalidationTransitMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaInvaliTranV1Sal response, EDIMessage message, NctsHeader nctsHeader) => new InboxNotificationNctsInvalidationTransitMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(ComunicaInvaliTranV1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			if (!nctsHeader.IsDepartureMovement)
			{
				throw new InvalidOperationException(SetNctsMessageFailedLogDescription(nctsHeader, "Departure"));
			}

			var correctResponseData = response.DatosComunicacion;
			if (correctResponseData != null)
			{
				nctsHeader.MovementHeader.BM_CustomsStatus = correctResponseData.Estado switch
				{
					Ncts5TransitStatusList.Codes.InvalidatedByGuarantee => ESNCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid,
					_ => ESNCTS5DepartureCustomsStatusList.Codes.Invalidated,
				};
			}

			return ZString.Empty;
		}

		const string XsdSchemaNameComunicaInvaliTranV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.ComunicaInvaliTranV1Sal.xsd";
	}
}
