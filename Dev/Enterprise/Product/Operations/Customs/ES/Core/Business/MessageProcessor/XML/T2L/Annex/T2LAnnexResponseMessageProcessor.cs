using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LanexosV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class T2LAnnexResponseMessageProcessor : T2LCommonResponseMessageProcessor<T2LanexosV1Sal>
	{
		public T2LAnnexResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"T2L Annex Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameT2LanexosV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.T2lAnnex };
		protected override ZBool IsAnnexMessage(EDIMessage message) => true;

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(T2LanexosV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new T2LAnnexMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(T2LanexosV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			var messages = entryHeader.Messages.Cast<EDIMessage>();

			SetSentMessageStatusAsReceived(message, entryHeader.Factory);
			SetMessageStatusAsReceived(message);

			var isExpedition = messages.Any(x => x.EM_MessageType == Messaging.DeclarationMessageTypeList.Codes.T2lExpedition && x.EM_Status == EDIMessage.Status.Received);
			if (isExpedition)
			{
				SetCSVClearance(response, message, entryHeader);
				SetCircuit(response, entryHeader);
			}
			else // Supposing annex can only be sent with reception or expedition
			{
				if (IsLastAnnex(entryHeader))
				{
					entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
				}
			}

			if (ShouldChangeCHStatusForAnnexes(entryHeader))
			{
				SetCHStatusAsReceived(entryHeader);

				TriggerMessageSendingCommon(message, ((IESMessageInfoProvider)entryHeader).Broker.GS_Code, entryHeader.Declaration.CusAgent, ShouldTriggerAnnexes(entryHeader), entryHeader, SendAnnex);
			}

			return ZString.Empty;
		}

		ZBool IsLastAnnex(CusEntryHeader entryHeader)
		{
			var eDocPivotList = entryHeader.GetAllSendableEDocPivots();

			var hasAnnexesWithoutResponse = entryHeader.EDocPivotCollection
								.Cast<CusStorageDocPivot>()
								.Any(p => p.MessageStatus != EDIMessageStatusList.Codes.Received);

			return !eDocPivotList.Any() && !hasAnnexesWithoutResponse;
		}

		protected override void ProcessRejectedDeclaration(T2LanexosV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => ProcessRejectedDeclarationForAnnexes(message, entryHeader);

		const string XsdSchemaNameT2LanexosV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.T2L.Incoming.T2LanexosV1Sal.xsd";
	}
}
