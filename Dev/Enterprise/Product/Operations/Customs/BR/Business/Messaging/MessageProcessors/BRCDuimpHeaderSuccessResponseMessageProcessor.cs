using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BR.MessageDefinitions.Duimp;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business
{
	public class BRCDuimpHeaderSuccessResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCDuimpHeaderSuccessResponseMessageProcessor(LoggingInformation logger)
				: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("2CD75EF2-8F4D-4CEF-9FB5-380B45113DE0", "DUIMP Header Success Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CIH };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Success };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			var outgoingMessage = BRMessageHelper.GetOutgoingMessage(message);
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var outgoingMessageSubType = outgoingMessage?.EM_MessageSubType ?? string.Empty;
				if (outgoingMessageSubType == EDIMessageSubTypeList.Codes.CompleteConsult)
				{
					ProcessCompleteConsultResponse(message, entryHeader);
				}
				else if (outgoingMessageSubType == EDIMessageSubTypeList.Codes.Original || outgoingMessageSubType == EDIMessageSubTypeList.Codes.Update)
				{
					ProcessOriginalOrUpdateResponse(message, outgoingMessage, entryHeader);
				}
				else if (outgoingMessageSubType == ImportEntryActionCodeList.Codes.DIA || outgoingMessageSubType == ImportEntryActionCodeList.Codes.REG)
				{
					message.EM_Status = EDIMessageStatusList.Codes.Received;
					entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
				}
				else
				{
					message.EM_Status = EDIMessageStatusList.Codes.Discarded;
					Logger.LogError($"Message #{message.EM_MessageNum}: Request Message {outgoingMessage?.EM_MessageNum} is not original, update or complete consult.");
				}
			}
		}

		void ProcessCompleteConsultResponse(EDIMessage message, CusEntryHeader entryHeader)
		{
			var duimpHeader = BRMessageHelper.DeserializeObject<DuimpConsultaCover>(message.EM_MessageText);
			if (duimpHeader == null)
			{
				message.EM_Status = EDIMessageStatusList.Codes.Failed;
				Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed.");
			}
			else
			{
				message.EM_MessageInterpretation = new DuimpHeaderCompleteConsultMessagePrettyFormatter(entryHeader, duimpHeader).GetFormattedMessageText();
				entryHeader.CH_AdministrativeStatus = BRAdministrativeStatusList.MapToCWCode(duimpHeader.situacao?.situacaoLicenciamento);
				entryHeader.CH_CargoStatus = BRCargoStatusList.MapToCWCode(duimpHeader.situacao?.controleCarga);
				entryHeader.CH_RiskChannel = RiskChannelList.GetRiskChannelValue(duimpHeader.resultadoAnaliseRisco?.canalConsolidado);

				var accessKey = duimpHeader.identificacao?.chaveAcesso;
				if (accessKey != null)
				{
					entryHeader.EntryAccessKey = accessKey;
				}
			}
		}

		void ProcessOriginalOrUpdateResponse(EDIMessage incomingMessage, EDIMessage outgoingMessage, CusEntryHeader entryHeader)
		{
			var messageReturn = BRMessageHelper.DeserializeObject<RespostaApi>(incomingMessage.EM_MessageText);

			var number = messageReturn?.identificacao?.numero;
			if (number == null)
			{
				entryHeader.CH_Status = BRMessageStatusList.Codes.Failed;
				incomingMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
				Logger.LogError($"Message #{incomingMessage.EM_MessageNum}: Message deserialization was failed.");
			}
			else if (outgoingMessage.EM_MessageSubType == EDIMessageSubTypeList.Codes.Original)
			{
				var version = messageReturn?.identificacao?.versao;
				if (version != null)
				{
					entryHeader.CH_AuthorityVersion = version;
				}
				if (number.Length > 0)
				{
					entryHeader.MovementReferenceNumberSetter(number);
					entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
					entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
					new DuimpMessageManager(new DuimpMessageSendingObject(entryHeader)).GenerateMessages();
				}
				else
				{
					entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
				}
			}
			else if (outgoingMessage.EM_MessageSubType == EDIMessageSubTypeList.Codes.Update)
			{
				PostponeIfAnyLineMessageInQueue(incomingMessage, entryHeader, Logger);

				if (entryHeader.AllEntryLines.All(line => line.CL_CustomsPostedStatus.IsAccepted() || line.CL_CustomsPostedStatus.IsDeleted()))
				{
					entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
					entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
				}
				else
				{
					entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
				}
			}
		}

		internal static void PostponeIfAnyLineMessageInQueue(EDIMessage incomingMessage, CusEntryHeader entryHeader, LoggingInformation logger)
		{
			if (!BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(incomingMessage.Factory,
					BRMessageHelper.GetOutgoingMessagesSentAtTheSameTime(incomingMessage, MessageTypeList.Codes.CIL)))
			{
				var postponedMessage = $"Message #{incomingMessage.EM_MessageNum} postponed: Entry Header {entryHeader.CH_BGMReference}, has CIL message waiting response.";

				logger.LogWarning(postponedMessage);
				throw new MessageProcessLockException(postponedMessage);
			}
		}
	}
}
