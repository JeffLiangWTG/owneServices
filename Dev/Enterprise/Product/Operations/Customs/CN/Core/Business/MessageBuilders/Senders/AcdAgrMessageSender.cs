using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CN.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	public class AcdAgrMessageSender
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "Baseline")]
		public AcdAgrMessageSender(IUserNotification notification = null)
		{
			if (notification != null)
			{
				ShowEnvironmentErrorMessage = (msg) => notification.ShowError(msg);
				ShowValidationErrorMessage = (msg, entry) => notification.ShowError(msg);
				AskUserToContinueIfAcdaNumberAlreadyExists = (msg, entry) => AskUserToContinueWithSend(notification, msg);
				AskUserToContinueIfSendWithMessageErrors = (msg, entry) => AskUserToContinueWithSend(notification, msg);
			}
		}

		public bool SendAndSave(CusEntryHeader entryHeader)
		{
			var result = false;

			var messages = Send(new[] { entryHeader });
			if (messages.Length > 0)
			{
				try
				{
					entryHeader.Factory.Save();
					result = true;
				}
				catch (ZSaveException ex)
				{
					messages.ForEach(m => m.Delete());
					entryHeader.Logs.LogsNotInDB.DeleteAll();
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
			return result;
		}

		public EDIMessage[] Send(IEnumerable<CusEntryHeader> entryHeaders)
		{
			var messages = new List<EDIMessage>();

			if (entryHeaders.Any())
			{
				var result = CNSWClientSettingChecker.CheckForAcdAgrMessageSending(entryHeaders.First().Declaration);
				if (result.IsEmpty)
				{
					foreach (var entryHeader in entryHeaders)
					{
						var messageDataProvider = new AcdAgrMessageDataProvider(entryHeader);
						if (CanSendMessage(messageDataProvider))
						{
							var message = entryHeader.Factory.New<CNEDIMessage>();
							message.EM_MessageType = EDIMessageTypeList.Codes.ACD;
							message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
							message.EM_ApplicationReference = entryHeader.LocalReferenceNumber;
							message.EM_Status = EDIMessage.Status.Queued;
							message.EM_MessageText =
								(new AcdAgrMessageBuilder(messageDataProvider) as IXmlMessageBuilder)
								.GenerateXmlMessage().GetSerializedString();
							message.EM_SendWithMessageErrors = messageDataProvider.MissingMandatoryFields.Any();
							entryHeader.Messages.Add(message);
							entryHeader.Logs.AddNew(AutoEvents.MessageSent, $"Send Agreement of Customs Declaration Agent Message;{entryHeader.CH_BGMReference}");
							messages.Add(message);
							OnMessageCreated?.Invoke(entryHeader, message);
						}
					}
				}
				else
				{
					ShowEnvironmentErrorMessage?.Invoke(result);
				}
			}
			return messages.ToArray();
		}

		bool CanSendMessage(AcdAgrMessageDataProvider messageDataProvider)
		{
			var entryHeader = messageDataProvider.EntryHeader;

			var continueWithSend = entryHeader.ACDANumber.IsEmpty || (AskUserToContinueIfAcdaNumberAlreadyExists?.Invoke(AcdaDocumentNumberAlreadyExistsMessage, entryHeader) ?? true);
			if (continueWithSend)
			{
				var missingFields = messageDataProvider.MissingMandatoryFields;
				if (missingFields.Any())
				{
					var missingFieldsMessage = new ZStringBuilder(missingFields.Select(x => $"  {x}")).ToStringWithNewLineBetweenAppends();

					if (Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed)
					{
						continueWithSend = AskUserToContinueIfSendWithMessageErrors?.Invoke(GetMandatoryFieldsNotFilledWarning(missingFieldsMessage), entryHeader) ?? true;
					}
					else
					{
						ShowValidationErrorMessage?.Invoke(GetMandatoryFieldsNotFilledMessage(missingFieldsMessage), entryHeader);
						continueWithSend = false;
					}
				}
			}

			return continueWithSend;
		}

		bool AskUserToContinueWithSend(IUserNotification notification, string message)
		{
			return notification?.Show(message, Res.GetString("47092F5D-02A3-437B-B300-08E4327711F9", "Continue with Send?"), ZMessageBoxButtons.YesNo, ZDialogResult.Yes) == ZDialogResult.Yes;
		}

		public Action<string> ShowEnvironmentErrorMessage { get; set; }
		public Action<string, CusEntryHeader> ShowValidationErrorMessage { get; set; }
		public Func<string, CusEntryHeader, bool> AskUserToContinueIfAcdaNumberAlreadyExists { get; set; }
		public Func<string, CusEntryHeader, bool> AskUserToContinueIfSendWithMessageErrors { get; set; }
		public Action<CusEntryHeader, EDIMessage> OnMessageCreated { get; set; }

		static string AcdaDocumentNumberAlreadyExistsMessage => Res.GetString("5B252390-4E3E-4C46-80F3-B03B21E4B487", "{0} number already exists on the linked entry instruction. Do you wish to continue?", CSDDocTypeList.Descriptions._10000001);

		static string GetMandatoryFieldsNotFilledWarning(string missingFields) => string.Concat(Res.GetString("7A164116-818C-41B0-B883-E5325523C2D9", "It is likely that your message will be rejected by Customs, as the following fields are not filled. Do you want to send the message despite these message errors?"), System.Environment.NewLine, missingFields);

		static string GetMandatoryFieldsNotFilledMessage(string missingFields) => string.Concat(Res.GetString("CAA518FC-A2F0-4F58-8D2B-21425ABBF396", "Please fill the following fields before sending the message."), System.Environment.NewLine, missingFields);
	}
}
