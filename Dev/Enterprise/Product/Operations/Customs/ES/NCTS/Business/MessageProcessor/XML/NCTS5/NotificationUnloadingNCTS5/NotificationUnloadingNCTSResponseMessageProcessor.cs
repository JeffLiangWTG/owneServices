using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC044C_v515.CC044CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NotificationUnloadingNCTSResponseMessageProcessor : NCTS5CommonResponseMessageProcessor<Cc044Cv1Sal, NotificationUnloadingNCTSMessagePrettyFormatter>
	{
		public NotificationUnloadingNCTSResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"NCTS Notification Unloading Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC044CV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods };

		protected override NotificationUnloadingNCTSMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc044Cv1Sal response, EDIMessage message, NctsHeader nctsHeader) => new NotificationUnloadingNCTSMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(Cc044Cv1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			if (!nctsHeader.IsArrivalMovement)
			{
				throw new InvalidOperationException(SetNctsMessageFailedLogDescription(nctsHeader, "Arrival"));
			}

			SetArrivalStatusCommon(nctsHeader, response.DatosRespuestaCorrecta?.Estado, ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease);

			if (nctsHeader.ArrivalMovementHeader.BM_CustomsStatus == ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease)
			{
				SetReleaseDate(response.PreparationDateAndTime, nctsHeader);
			}

			return ZString.Empty;
		}

		protected override void ProcessRejectedDeclaration(Cc044Cv1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Arrival;
		}

		protected override void SetExtraCHStatus(NctsHeader businessObject, ZString messageStatus)
		{
			if(messageStatus == EDIMessageStatusList.Codes.Failed && businessObject.ArrivalMovementHeader != null)
			{
				businessObject.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Arrival;
			}
		}

		const string XsdSchemaNameCC044CV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.CC044CV1Sal.xsd";
	}
}
