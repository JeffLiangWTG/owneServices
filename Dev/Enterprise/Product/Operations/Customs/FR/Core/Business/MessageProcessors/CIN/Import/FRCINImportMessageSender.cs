using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessagesWrappers.CIN;
using Enterprise.Customs.FR.Messaging.MessageBuilders.CIN;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.FR.Messaging.MessageBuilders.CIN.CINImportMessageBuilder;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRCINImportMessageSender
	{
		public FRCINImportMessageSender(CusTempStorageJobHeader jobHeader, CusTempStorageJobHeader previousJobHeader, MessageBuilderType messageType)
		{
			this.jobHeader = Argument.NotNull(jobHeader, "CusTempStorageJobHeader cannot be null");
			this.previousJobHeader = previousJobHeader;
			this.messageType = messageType;
		}

		public (bool canSend, string reason) CanSend()
		{
			var canSend = true;
			var reason = string.Empty;
			message = null;

			var cusTempStorageDec = jobHeader.CusTempStorageDec;
			if (cusTempStorageDec == null)
			{
				canSend = false;
				reason = SetReason(true, ValidationNoDeclaration);
			}
			else if (!cusTempStorageDec.CusTempStorageLines.Any())
			{
				canSend = false;
				reason = SetReason(true, ValidationNoLines);
			}
			else if (messageType == MessageBuilderType.Correction)
			{
				(canSend, reason) = CanSendCorrection();
			}

			return (canSend, reason);
		}

		(bool canSend, string reason) CanSendCorrection()
		{
			var canSend = false;
			var reason = string.Empty;

			if (jobHeader.CusTempStorageDec.STH_MessageStatus == CusTempStorageDec.DeclarationStatusForCorrectionMessage)
			{
				canSend = LinesHaveChanges();
			}

			return (canSend, reason);
		}

		bool LinesHaveChanges()
		{
			if (previousJobHeader != null)
			{
				var headerWrapper = new CINHeaderWrapper(jobHeader);
				var previousHeaderWrapper = (previousJobHeader == null ? null : new CINHeaderWrapper(previousJobHeader));
				var changedLines = CINWarehouseMovementCorMessageBuilder.GetChangedLines(headerWrapper, previousHeaderWrapper);

				return changedLines.Any();
			}
			else
			{
				return false;
			}
		}

		string SetReason(bool silentForCorrection, MultilingualString reason)
		{
			if (silentForCorrection && messageType == MessageBuilderType.Correction)
			{
				return string.Empty;
			}
			else
			{
				return reason;
			}
		}

		public (bool, string) Send()
		{
			try
			{
				if (message == null)
				{
					PrepareMessage();
				}

				message.Factory.Save();

				jobHeader.CusTempStorageDec.Messages.AddFromDatabase(message.PK);

				jobHeader.Factory.Save();

				message = null;

				return (true, FormattableString.Invariant($"{MessageSendSuccessful}"));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return (false, FormattableString.Invariant($"{MessageSendFailure}\n{ex.Message}"));
			}
		}

		public void PrepareMessage()
		{
			message = CreateEdiMessage();
			var messageId = message.EM_MessageNum;

			var headerWrapper = new CINHeaderWrapper(jobHeader);
			var previousHeaderWrapper = previousJobHeader == null ? null : new CINHeaderWrapper(previousJobHeader);
			var builder = Create(headerWrapper, previousHeaderWrapper, messageId, messageType);

			var content = builder.GetMessage();

			message.EM_MessageText = content;
		}

		public void CancelMessageOnFailure()
		{
			if (message != null)
			{
				message.Reload();
				if (message.EM_Status == EDIMessage.Status.Queued)
				{
					message.EM_Status = EDIMessage.Status.Cancelled;
					message.Factory.Save();
				}
			}
		}

		EDIMessage CreateEdiMessage()
		{
			var factory = jobHeader.CreateNewFactory();
			var msg = factory.New<EDIMessage>();

			msg.EM_ApplicationCode = FREDIMessage.ApplicationCodes.FRCustomsMessage;
			msg.EM_Status = EDIMessage.Status.Queued;
			msg.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			msg.EM_MessageType = MessageTypeList.Codes.CIN;
			msg.MessageNumberStrategy = new FRMessageNumberStrategy(jobHeader.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);

			factory.Save();

			return msg;
		}

		readonly CusTempStorageJobHeader jobHeader;
		readonly CusTempStorageJobHeader previousJobHeader;
		readonly MessageBuilderType messageType;

		public EDIMessage message;

		static MultilingualString MessageSendSuccessful => ResString.GetMultilingualString("FRCINMessageSender-SendToCustoms-Successful", "Message sent successfully");

		static MultilingualString MessageSendFailure => ResString.GetMultilingualString("FRCINMessageSender-SendToCustoms-Failure", "Failed to send message");

		static MultilingualString ValidationNoDeclaration => ResString.GetMultilingualString("FRCINValidation-NoDeclaration", "No Declaration has been created.");

		public static MultilingualString ValidationNoLines => ResString.GetMultilingualString("FRCINValidation-NoLines", "Declaration has no lines, message cannot be created.");
	}
}
