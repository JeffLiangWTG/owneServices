//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using System.Collections.Generic;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Customs.CA.Business.MessageProcessors;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Edifact.D96A.Elements;
	using Enterprise.Environment;
	using Enterprise.Messaging.MessageBuilders;
	using Enterprise.Security;
	using MessageBuilders = Customs.Business.MessageManagers;

	public class RNSMessageManager : CAMessageManager
	{
		public RNSMessageManager(IRNSRequest dataWrapper, MessageBuilders.IUserNotification notification, bool isStatusQuery, IEnumerable<string> messageErrors = null)
			: base(dataWrapper, null, notification)
		{
			this.isStatusQuery = isStatusQuery;
			fShouldWaitUntilResponded = true;
			fShouldJobBeSavedBeforeSendingMessage = true;
			this.messageErrors = messageErrors;
		}
		readonly IEnumerable<string> messageErrors;

		#region Overrides of SingleMessageManager

		public override string MessageFriendlyName
		{
			get
			{
				var humanReadableName = DataWrapper.TopLevelBusinessObject.HumanReadableName;

				return string.Format("{0}{1}{2}",
					isStatusQuery ? RNSMessageTypes.Descriptions.StatusQuery : RNSMessageTypes.Descriptions.ArrivalCertification,
					humanReadableName.IsEmpty ? "" : Res.GetString("c3d5fe01-c13f-4180-8d1d-339f9551346d", " for "),
					humanReadableName);
			}
		}

		public override MessageSendingNotificationCollection GetNotificationsForSendingAnOriginal()
		{
			var result = base.GetNotificationsForSendingAnOriginal();
			if (messageErrors != null)
			{
				messageErrors.ForEach(x => result.AddWarning(x));
			}
			if (!CanSendThisMessage(MessageSubTypes.Create, out ZString errorText))
			{
				result.AddWarning(Res.GetString("cfb93632-9d9a-4813-ac94-d6e13b7551ce", "As {0}", errorText));
			}
			return result;
		}

		protected override bool ShouldWaitUntilResponded
		{
			get
			{
				return fShouldWaitUntilResponded;
			}
		}

		bool fShouldWaitUntilResponded;

		public void SetShouldWaitUntilResponded(bool value)
		{
			fShouldWaitUntilResponded = value;
		}

		protected override bool ShouldJobBeSavedBeforeSendingMessage
		{
			get { return fShouldJobBeSavedBeforeSendingMessage; }
		}

		public void SetShouldJobBeSavedBeforeSendingMessage(bool value)
		{
			fShouldJobBeSavedBeforeSendingMessage = value;
		}
		bool fShouldJobBeSavedBeforeSendingMessage;

		#endregion

		#region Overrides of CAMessageManager

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			base.CanSendThisMessage(actionCode, out messageText);

			if (messageText.IsEmpty)
			{
				if (DataWrapper.TransactionNumber.IsEmpty && DataWrapper.CargoControlNumber.IsEmpty)
				{
					messageText = Res.GetString("56d8a670-2cc7-4106-bab7-26918cbe7e88", "Cargo Control Number must be specified.");
				}

				if (!isStatusQuery && !DataWrapper.DateOfArrival.IsValid)
				{
					messageText = Res.GetString("806f5d9e-8ff6-47b0-9c55-38244aed9e70", "valid Arrival Date must be provided for {0}.", RNSMessageTypes.Descriptions.ArrivalCertification);
				}
			}
			return messageText.IsEmpty;
		}

		protected override SecurityCheckpoint SendWithMessageErrorsSecurityCheckpoint => Env.Security.CARNSSendWithMessageErrors;

		public override bool IsWaitingForResponse
		{
			get { return ImportLinkedObjectManager.IsAnyRNSRequestAwaitingReply(DataWrapper); }
		}

		protected override string AwaitingCustomsResponseMessage
		{
			get
			{
				var builder = new ZStringBuilder();
				if (!DataWrapper.TransactionNumber.IsEmpty)
				{
					builder.Append(Res.GetString("7d4d8ca0-28fc-4a56-9cb4-f874833235e2", "Transaction Number: {0}", DataWrapper.TransactionNumber));
				}

				if (!DataWrapper.CargoControlNumber.IsEmpty)
				{
					builder.Append(Res.GetString("2e4c081c-2b35-4959-b310-13cf57ef08ad", "CCN: {0}", DataWrapper.CargoControlNumber));
				}

				var message = Res.GetString("7f2eb771-c9dd-4179-a8a8-b99abf740fce", @"An RNS Request has already been sent and is awaiting a CBSA response ({0}).
Sending another one now may cause you, or the other party who sent the original request, to  not receive a response.
Please be aware that acknowledgement responses are sent to all relevant parties, so you should not need to resend this request.
If you do not receive a response within a reasonable period of time, then try resending the request at that time.


Are you sure that you want to resend to the CBSA?", builder.ToStringWithDelimiterBetweenAppends(", "));
				return message;
			}
		}

		protected override ZString GetAdditionalWarningsMessage(MessageSubTypes actionCode)
		{
			var builder = new ZStringBuilder();

			var warningMessage = base.GetAdditionalWarningsMessage(actionCode);
			if (!warningMessage.IsEmpty)
			{
				builder.Append(warningMessage);
			}

			GetValidationResult(builder);
			if (IsWaitingForResponse)
			{
				builder.Append(AwaitingCustomsResponseMessage);
			}
			return builder.ToStringWithDelimiterBetweenAppends("\r\n");
		}

		void GetValidationResult(ZStringBuilder builder)
		{
			if (!isStatusQuery)
			{
				var declaration = ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(DataWrapper.Factory, DataWrapper.TransactionNumber, JobMessageTypeList.Codes.Import)
					?? ImportLinkedObjectManager.GetCusJobDeclarationByCargoControlNumber(DataWrapper.Factory, DataWrapper.CargoControlNumber, JobMessageTypeList.Codes.Import);

				if (declaration != null)
				{
					var timeAtPort = declaration.TimeAtPortOfDischarge;
					var entryStatus = declaration.JE_EntryStatus;
					var transportMode = declaration.JE_TransportMode;
					var dateOfFirstArrival = declaration.JE_DateOfFirstArrival;

					if (entryStatus.IsEmpty || entryStatus == EntryStatusList.Codes.Cancelled || entryStatus == EntryStatusList.Codes.Error)
					{
						builder.Append(Res.GetString("4BB97A4B-8F92-4680-909D-D04F5387F30B", "An ACROSS release declaration appears to have not been lodged."));
					}
					if (!timeAtPort.IsValid)
					{
						builder.Append(Res.GetString("e21835a8-45ef-494e-9383-6484942d0d25",
								"Please check the Time Zone of Arrival Port or your Home Port, it should not be empty."));
					}
					else
					{
						if (transportMode == Core.Constants.TransportModes.Air && dateOfFirstArrival.IsValid && dateOfFirstArrival > timeAtPort.AddHours(20))
						{
							builder.Append(Res.GetString("267881BD-4425-4826-A3D4-33E13EC99C71", "Related ETA First Port of Arrival date cannot be farther than 20 hours in the future for an AIR shipment being arrived."));
						}
						else if (transportMode == Core.Constants.TransportModes.Sea && dateOfFirstArrival.IsValid && dateOfFirstArrival.Date > timeAtPort.Date)
						{
							builder.Append(Res.GetString("A299F0D4-ED27-40C1-BE9A-1FF0B7A8FEF3", "Related ETA First Port of Arrival date cannot be in the future for a Marine shipment being arrived."));
						}
						else if (transportMode == Core.Constants.TransportModes.Road && dateOfFirstArrival.IsValid && dateOfFirstArrival.Date > timeAtPort.Date)
						{
							builder.Append(Res.GetString("E0674956-091C-4838-A75F-16AA52E8436F", "Related ETA First Port of Arrival date cannot be in the future for a ROAD shipment being arrived."));
						}
					}
				}
			}
		}

		protected override bool ShowAwaitingCustomsResponse()
		{
			return notification.ShowConfirmation(AwaitingCustomsResponseMessage, WarningCaption, true);
		}

		public override bool CanSendWithdrawal
		{
			get { return false; }
		}

		protected override bool DefineActionCodeIfUndefined(ref MessageSubTypes actionCode)
		{
			return true; //Action code is not used by RNS Request
		}

		protected override void OnMessageQueuedForSending(MessageSubTypes actionCode)
		{
			//Is not supported by RNS Request
		}

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			var messageType = isStatusQuery ? DocumentMessageNameCodedList.PreviousCustomsDocumentMessage
								: DocumentMessageNameCodedList.ForwardersWarehouseReceipt;
			return new RNSRequestMessageBuilder(DataWrapper, messageType);
		}

		public new IRNSRequest DataWrapper
		{
			get { return (IRNSRequest)base.DataWrapper; }
		}

		protected override SecurityCheckpoint SecurityCheckpoint
		{
			get { return isStatusQuery ? Env.Security.CARNSEnqMsgSend : Env.Security.CARNSArrivalMsgSend; }
		}

		readonly bool isStatusQuery;

		protected override void UpdateMessageDetailsCore(Enterprise.Messaging.Business.EDIMessage message)
		{
			message.EM_ApplicationReference = DataWrapper.CargoControlNumber.Replace(" ", "");
			message.EM_MessageOwner = DataWrapper.TransactionNumber;
			message.EM_IsTestMessage = ShouldSendMessagesInTestMode;
		}

		protected override void ShowQueuedForSending(MessageSubTypes actionCodeToSend)
		{
			base.ShowQueuedForSending(actionCodeToSend);
			DataWrapper.RefreshMessagesForDisplay();
		}

		#endregion
	}
}
