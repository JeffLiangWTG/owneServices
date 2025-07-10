using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Messaging.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class IIDMessageManager : CAMessageManager
	{
		#region Constructor

		public IIDMessageManager(ICAEDIFACTMessageAttachee dataWrapper, IUserNotification notification)
			: base(dataWrapper, new IIDStatusCalculator(), notification)
		{
		}

		#endregion

		protected new IIDStatusCalculator StatusCalculator
		{
			get { return (IIDStatusCalculator)base.StatusCalculator; }
		}

		IIDMessageWrapper MessageWrapper => (IIDMessageWrapper)DataWrapper;

		#region override

		protected override bool PreCheck4CreditOKToSendChecking(JobDeclaration declaration) => declaration?.B3EntryHeader?.HasDiscrepancyInTotalDutyAndTaxes ?? true;

		protected override void OnMessageQueuedForSending(MessageSubTypes actionCode)
		{
			base.OnMessageQueuedForSending(actionCode);
			if (actionCode == MessageSubTypes.Create)
			{
				var messageWrapper = DataWrapper as IIDMessageWrapper;
				if (messageWrapper != null)
				{
					messageWrapper.PopulateEntrySubmittedDateIfRequired();
				}
			}

			var declaration = BusinessObject as JobDeclaration;
			if (declaration != null)
			{
				var entryHeader = declaration?.ReleaseEntryHeader;
				if (entryHeader != null)
				{
					entryHeader.CH_EntryStatus = string.Empty;
				}
			}
		}

		protected override bool DefineActionCodeIfUndefined(ref MessageSubTypes actionCode)
		{
			if (actionCode == MessageSubTypes.Undefined)
			{
				var messageWrapper = MessageWrapper;
				if (messageWrapper == null || !messageWrapper.IsMessageValidationPassed)
				{
					actionCode = MessageSubTypes.Create;
				}
				else
				{
					actionCode = MessageSubTypes.Change;
				}
			}
			return true;
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] PopulateMessage(MessageSubTypes actionCode)
		{
			var messages = new List<Enterprise.Messaging.Business.EDIMessage>();
			var builder = GetMessageBuilder(actionCode);
			foreach (var builderResult in builder.PopulateMessages().GetBuilderResults())
			{
				var message = builderResult.Message;
				DataWrapper.AddMessage(message);
				if (StatusCalculator != null)
				{
					DataWrapper.MessageStatus = StatusCalculator.GetMessageAwaitingStatus(message);
				}

				messages.Add(message);
			}
			return messages.ToArray();
		}

		protected override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CAACROSSMsgSend; }
		}

		protected override void ShowQueuedForSending(MessageSubTypes actionCodeToSend)
		{
			var message = Res.GetString("BF2E277E-8887-4FF3-9EFE-58202ACB7704", "IID {0} {1} has been generated.", GetActionCodeDescription(actionCodeToSend), MessageFriendlyName);
			var caption = Res.GetString("41042ED8-D391-4EF2-9915-4D1E8C1A6021", "Message generated");
			notification.ShowInformation(message, caption);
		}

		protected override string GetActionCodeDescription(MessageSubTypes actionCodeToSend)
		{
			var result = ZString.Empty;
			switch (actionCodeToSend)
			{
				case MessageSubTypes.Create:
					result = ZString.Format("Original");
					break;
				case MessageSubTypes.Change:
					result = ZString.Format("Change");
					break;
				case MessageSubTypes.Withdraw:
					result = ZString.Format("Withdraw");
					break;
				case MessageSubTypes.Amend:
					result = ZString.Format("Amendment");
					break;
				default:
					result = actionCodeToSend.ToString();
					break;
			}
			return result;
		}

		public override string MessageFriendlyName
		{
			get { return ResString.GetMultilingualString("0a2dd7da-38aa-46e6-afdf-d9074b64c6d8", "Message for {0}", DataWrapper.TopLevelBusinessObject.HumanReadableName); }
		}

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			var iidMessageSubType = GetIIDMessageSubTypeByMessageSubType(actionCode);
			return new IIDMessageBuilder(iidMessageSubType, DataWrapper as IIDMessageWrapper);
		}

		ZString GetIIDMessageSubTypeByMessageSubType(MessageSubTypes type)
		{
			switch (type)
			{
				case MessageSubTypes.Create:
					return IIDMessageSubTypeList.Codes.Original;
				case MessageSubTypes.Change:
					return IIDMessageSubTypeList.Codes.Change;
				case MessageSubTypes.Withdraw:
					return IIDMessageSubTypeList.Codes.Cancellation;
				case MessageSubTypes.Amend:
					return IIDMessageSubTypeList.Codes.Amendment;
				default:
					return IIDMessageSubTypeList.Codes.Undefined;
			}
		}

		protected override ZString GetAdditionalMessageErrors(MessageSubTypes actionCode)
		{
			var stringBuilder = new ZStringBuilder();
			stringBuilder.AppendIfNotEmpty(base.GetAdditionalMessageErrors(actionCode));
			if (IsWaitingForResponse)
			{
				stringBuilder.Append(Res.GetString("c78cc079-21e0-43e1-9d15-7ee552b70276", "Entry is currently waiting for a response from CBSA or has a message already scheduled to send."));
			}
			if (actionCode == MessageSubTypes.Amend)
			{
				var messageWrapper = MessageWrapper;
				if (messageWrapper != null && messageWrapper.AmendmentReason.IsEmpty)
				{
					stringBuilder.Append(Res.GetString("b0329761-a990-4464-8a76-a1aa3eba215c", "You are about to send an amendment message, please specify the amendment reason code under Misc Tab -> Amendment Reason."));
				}
			}

			return stringBuilder.ToStringWithDelimiterBetweenAppends("\r\n\r\n");
		}

		protected override bool NeedValidationForNotificationMessageInstruction(MessageSubTypes actionCode)
		{
			return actionCode != MessageSubTypes.Withdraw;
		}

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			var result = base.CanSendThisMessage(actionCode, out messageText);

			if (result)
			{
				var declaration = BusinessObject as JobDeclaration;
				result = IsCreditCheckOKToSend(declaration, out messageText);
			}

			if (result && messageText.IsEmpty)
			{
				if (actionCode == MessageSubTypes.Withdraw && MessageWrapper.HasAcceptedWithdrawMessage)
				{
					messageText = Res.GetString("AE6142F0-CC1B-4151-84F7-7F7F9D179CCA", "this has already been canceled.");
				}

				result = messageText.IsEmpty;
			}

			return result;
		}

		#endregion

	}
}
