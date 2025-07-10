using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public static class MessageSendingObjectExtensionMethods
	{
		public static void CheckShouldSendCore(this ZPropertyInfo shouldSendInfo, CusEntryHeader header, ZString messageType, Func<CusEntryNumber, bool> additionalEntryNumFilter = null)
		{
			var messageStatus = ZString.Empty;
			CusEntryNumber entryNumberOfMessageType = null;

			if (ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(messageType))
			{
				var entryType = ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(messageType) ? messageType : (ZString)ElectronicDocumentTypeList.GetOriginalType(messageType);

				entryNumberOfMessageType = GetCusEntryNumber(header, entryType, additionalEntryNumFilter ?? (x => true));
				messageStatus = entryNumberOfMessageType?.CE_EntryStatus ?? ZString.Empty;
			}
			else
			{
				messageStatus = header.CH_Status;
			}

			if (header.CH_Status == CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms)
			{
				shouldSendInfo.AddError(string.Format(cancellationApprovedByCustomsMessage, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationApprovedByCustoms(header.CH_MessageType)));
			}
			else if (header.CH_Status == CustomsMessageStatusTypeList.Codes.CancellationByCustoms)
			{
				shouldSendInfo.AddError(string.Format(cancellationByCustomsMessage, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(header.CH_MessageType)));
			}
			else if (CustomsMessageStatusTypeList.IsWaitingForResponse(messageStatus))
			{
				shouldSendInfo.AddError(waitingForResponse);
			}
			else
			{
				if (ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(messageType))
				{
					if (!CustomsMessageStatusTypeList.IsOriginalMessageAllowed(messageStatus))
					{
						shouldSendInfo.AddError(Res.GetString("809D067F-9A67-4C42-AAF5-CA46930E64C7", "You cannot send this message. Its status indicates Customs has already accepted a message of this type."));
					}
					else if (ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(messageType) && !ElectronicDocumentTypeList.CanSendBeforeDeclarationIsAccepted(messageType))
					{
						if (!CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(header.CH_Status))
						{
							if (CustomsMessageStatusTypeList.IsWaitingForResponse(header.CH_Status))
							{
								shouldSendInfo.AddError(waitingForResponse);
							}
							else
							{
								var mainOriginalMessageType = ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(messageType);
								shouldSendInfo.AddError(Res.GetString("B7017AA5-1D5C-4DDA-BF79-736BE275A3C8", "You cannot send this message. The status of '{0}' indicates Customs has never accepted an original message of '{1}'. Please send a message of '{1}' before trying to send this message.", messageType, mainOriginalMessageType));
							}
						}
					}
				}
				else if (ElectronicDocumentTypeList.IsAmendment(messageType) || ElectronicDocumentTypeList.IsCancellation(messageType))
				{
					if (!CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(messageStatus))
					{
						shouldSendInfo.AddError(Res.GetString("1FAA04A9-05F9-4513-8819-E53B131A1423", "You cannot send this message now. Its status indicates Customs has never accepted an original message of this type."));
					}
				}
				ValidateOnMandatoryCustomsReviewMessage(shouldSendInfo, header, messageType, messageStatus);
			}
		}

		public static void CheckShouldSendWithMessageErrors(this ZPropertyInfo shouldSendInfo, ZString entityTypeName)
		{
			if (shouldSendInfo.GetErrors().Count() == 0)
			{
				if (Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed)
				{
					shouldSendInfo.AddWarning(Res.GetString("46E40E4D-B029-4198-817C-A3BB939D3367", "The selected {0} has message errors. Please review them. However you will be able to send a message as you have the security right to send with message errors.", entityTypeName));
				}
				else
				{
					shouldSendInfo.AddError(Res.GetString("2B0A2C2E-E270-4A0D-83C6-42E7ACF5BF6B", "The selected {0} has message errors. Unless you fix them, you will not be able to send a message as you don't have the security right to send with message errors." , entityTypeName));
				}
			}
		}

		static CusEntryNumber GetCusEntryNumber(CusEntryHeader header, string entryType, Func<CusEntryNumber, bool> additionalEntryNumFilter)
		{
			return header.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == entryType && additionalEntryNumFilter(x));
		}

		static void ValidateOnMandatoryCustomsReviewMessage(ZPropertyInfo info, CusEntryHeader header, string messageType, string messageStatus)
		{
			if (CustomsMessageStatusTypeList.IsMessageAccepted(messageStatus) && !HasCustomsDoneMandatoryReviewFor(header, messageType))
			{
				var reviewMessageType = ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(messageType);
				info.AddError(string.Format(waitForCustomsApprovalMessage, reviewMessageType, messageType));
			}

			var mainMessageStatusReqCustomsReview = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(messageType);
			if (mainMessageStatusReqCustomsReview.Contains(header.CH_Status.ToString()))
			{
				var messageTypes = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(messageType);
				var lastOutgoingMessage = header.Messages.Cast<EDIMessage>().Where(x => messageTypes.Contains(x.EM_MessageType.ToString())).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
				if (lastOutgoingMessage != null)
				{
					EDIMessage reviewMessage;
					if (!HasCustomsDoneMandatoryReviewFor(header, lastOutgoingMessage, out reviewMessage))
					{
						var reviewMessageType = ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(lastOutgoingMessage.EM_MessageType);
						info.AddError(string.Format(waitForCustomsApprovalMessage, reviewMessageType, lastOutgoingMessage.EM_MessageType));
					}
				}
			}
		}

		/// <summary>
		/// If a Customs review message is expected for the passed-in message type, then it checks if the review message has arrived.
		/// Otherwise, it returns 'true' meaning acceptance is deemed as passing the review. 
		/// </summary>
		static bool HasCustomsDoneMandatoryReviewFor(CusEntryHeader entry, string outgoingMessageType)
		{
			var outgoingMessage = entry.Messages.Cast<EDIMessage>().Where(x => x.IsTransmitMessage && x.EM_MessageType == outgoingMessageType).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			return HasCustomsDoneMandatoryReviewFor(entry, outgoingMessage, out EDIMessage reviewMessage);
		}

		static bool HasCustomsDoneMandatoryReviewFor(CusEntryHeader entry, EDIMessage outgoingMessage, out EDIMessage reviewMessage)
		{
			var result = true;
			reviewMessage = null;
			if (outgoingMessage != null)
			{
				var reviewMessageType = ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(outgoingMessage.EM_MessageType);
				if (!string.IsNullOrEmpty(reviewMessageType))
				{
					reviewMessage = GetCustomsReviewMessageFor(entry, reviewMessageType, outgoingMessage.EM_MessageNum);
					result = reviewMessage != null;
				}
			}
			return result;
		}

		static EDIMessage GetCustomsReviewMessageFor(CusEntryHeader entry, string reviewMessageType, string messageNum)
		{
			return entry.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == reviewMessageType && x.EM_ApplicationReference == messageNum);
		}

		static MultilingualString waitingForResponse => ResString.GetMultilingualString("7C872B00-46F4-4897-B691-A9F76BB089FF", "You cannot send this message. Its status indicates the last electronic document is still waiting for a response.");
		static MultilingualString waitForCustomsApprovalMessage => ResString.GetMultilingualString("0BE3691D-4591-4BF3-92CB-CC032701639E", "Customs' review message of type '{0}' has not arrived yet. Please wait until it arrives as a response to the last electronic document, '{1}'");
		static MultilingualString cancellationApprovedByCustomsMessage => ResString.GetMultilingualString("438957FE-E26C-43B3-B254-B018411B774B", "You cannot send this message. Its status indicates the cancellation of a declaration has been accepted by Customs.Please refer to the recent {0} message");
		static MultilingualString cancellationByCustomsMessage => ResString.GetMultilingualString("D2D18458-9E9E-4E81-89ED-8B55CDD5E593", "This entry has been declined by Customs. Please refer to the recent {0} message. You can no longer send further messages on this entry.");
	}
}
