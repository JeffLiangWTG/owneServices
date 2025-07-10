using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCDOTC_v515.CCDOTCV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class AnnexNCTSResponseMessageProcessor : NCTS5CommonResponseMessageProcessor<Ccdotcv1Sal, AnnexNCTSMessagePrettyFormatter>
	{
		public AnnexNCTSResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"NCTS Annex Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCCDOTCV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes };

		protected override ZBool IsAnnexMessage(EDIMessage message) => true;

		protected override ZBool SetPhaseStatusTo015 => true;

		protected override AnnexNCTSMessagePrettyFormatter GetNewMessagePrettyFormatter(Ccdotcv1Sal response, EDIMessage message, NctsHeader nctsHeader) => new AnnexNCTSMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(Ccdotcv1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			if (!nctsHeader.IsDepartureMovement)
			{
				throw new InvalidOperationException(SetNctsMessageFailedLogDescription(nctsHeader, "Departure"));
			}

			SetSentMessageStatusAsReceived(message, nctsHeader.Factory);
			SetMessageStatusAsReceived(message);

			if (ShouldChangeMessageStatusForAnnexes(nctsHeader))
			{
				SetCHStatusAsReceived(nctsHeader, messageStatus: ZString.Empty);

				var shouldTriggerAnnexes = nctsHeader.RequestDispatch == Customs.Business.YesNoList.Codes.Yes;
				TriggerMessageSendingCommon(message, ((IESMessageInfoProvider)nctsHeader).Broker.GS_Code, nctsHeader.MovementHeader.CusAgent, shouldTriggerAnnexes, nctsHeader, SendAnnex, messageStatus: LogicalStatusList.Codes.Sent);
			}

			return ZString.Empty;
		}

		protected override void ProcessRejectedDeclaration(Ccdotcv1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			ProcessRejectedDeclarationForAnnexes(message, nctsHeader);
			nctsHeader.RequestDispatch = ZString.Empty;
		}

		List<ESEDIMessage> SendAnnex(NctsHeader nctsHeader, CertificateObject certificateObject)
		{
			var messages = new List<ESEDIMessage>();

			var builderManager = new ESNctsMessageBuilderManager(nctsHeader, certificateObject, DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes);
			var messageBuilders = ESNctsMessageSender.GetNCTSAnnexesMessageBuilders(nctsHeader, builderManager, nctsHeader.RequestDispatch);

			if (!messageBuilders.IsNullOrEmpty())
			{
				ESNctsMessageSender.Send(messageBuilders, messages);
			}

			return messages;
		}

		ZBool ShouldChangeMessageStatusForAnnexes(NctsHeader nctsHeader)
		{
			var messageStatus = nctsHeader.CommonMovementHeader.BM_MessageStatus;
			return !nctsHeader.HasAnnexesSentWithoutResponse() && messageStatus != EDIMessageStatusList.Codes.Failed && messageStatus != EDIMessageStatusList.Codes.Rejected;
		}

		const string XsdSchemaNameCCDOTCV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.CCDOTCV1Sal.xsd";
	}
}
