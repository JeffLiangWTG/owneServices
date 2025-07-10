//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Business.MessageManagers;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Customs.CA.Business.MessageManagers;

	public delegate bool ShowMessageSendingActionFormDelegate(CAMessageSendingActionCollection actions);

	public class DataLoadingModuleMessageManager : FormalEntrySingleMessageManager
	{
		public DataLoadingModuleMessageManager(CusEntryHeader entry, CAMessageSendingAction action)
			: base(entry)
		{
			CanSendDeclarationChecker.EntryNotNullAndAttachedToDeclaration(entry);
			Argument.NotNull(action, "action");
			this.action = action;
		}

		#region SendDLMMessages

		public static void SendDLMMessages(JobDeclaration declaration, IUserNotification notification, ShowMessageSendingActionFormDelegate showMessageSendingActionForm)
		{
			if (CanSendDLMMessage(declaration, notification))
			{
				var messageSendingActions = new CAMessageSendingActionCollection(declaration, MessageSendingMessageType.Original);
				if (messageSendingActions.Count == 0)
				{
					var message = Res.GetString("b4fb3d6f-3383-412a-a422-fc0e32eb7e6c", "There are no entries to send Original messages for");
					var caption = Res.GetString("861426dd-92a8-4ff1-9843-a23a23fb3061", "Send Messages");
					notification.ShowInformation(message, caption);
				}
				else if (showMessageSendingActionForm(messageSendingActions))
				{
					var notifications = MessageSendingValidation.New(declaration, null).CheckBusinessObjectLevelValidation();

					if (CAMessageManager.ShouldContinueWithNotifications(notifications, notification)
						&& messageSendingActions.SendMessagesWithoutSaving(declaration.MessageInitiator))
					{
						try
						{
							declaration.Factory.Save();
						}
						catch (ZSaveException e)
						{
							ZExceptionReporting.HandleSaveException(e);
						}
					}
				}
			}
		}

		static bool CanSendDLMMessage(JobDeclaration declaration, IUserNotification notification)
		{
			bool result = declaration != null;
			if (result && (declaration.ActiveEntryHeaders.Count == 0 || declaration.MergeManager.RequiresMerge))
			{
				result = declaration.DoMerge();
			}
			if (result)
			{
				declaration.LoadChildEditableObjects();
				declaration.RunPreSaveValidation();

				var caption = Res.GetString("8af0931f-3cd1-4c31-9684-482bb38e0f61", "Cannot Send Message");
				if (declaration.HasErrors)
				{
					var message = Res.GetString("b52040b0-b5dd-4763-8dbb-704a0e747144", "The message cannot be sent. Please fix all errors on the form before trying to send a message");
					notification.ShowError(message, caption);
					result = false;
				}
				if (declaration.CustomsEntryHeaders.Count > 0 && declaration.CustomsEntryHeaders[0].IsG7ExportDeclaration)
				{
					var message = Res.GetString("17fc3cd0-4237-4f05-bf0a-165771bb3605", "This declaration has been formatted for a G7 Export declaration, you can not go back to DLM.");
					notification.ShowError(message, caption);
					result = false;
				}
			}
			return result;
		}

		#endregion

		#region Overriden

		protected override bool IncludeBusinessLayerNotificationsInMessageSendingNotifications
		{
			get { return false; }
		}

		protected override bool ShouldWaitUntilResponded
		{
			get { return false; }
		}

		public override bool HasActiveMessages
		{
			get { return EntryHeader.HasBeenLodgedAtCustoms; }
		}

		public override bool CanSendOriginal
		{
			get { return true; }
		}

		public override bool CanSendWithdrawal
		{
			get { return false; }
		}

		public override string MessageFriendlyName
		{
			get { return EntryHeader.CH_BGMReference; }
		}

		public override bool IsWaitingForResponse
		{
			get { return false; }
		}

		#endregion

		#region Sending Messages

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			return GenerateOriginalMessages((CusEntryHeader)bizo, false);
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateMessagesForAmendmentDetection(BusinessObject bizo)
		{
			return GenerateOriginalMessages((CusEntryHeader)bizo, true);
		}

		internal DataLoadingModuleMessageBuilder GetMessageBuilder(CusEntryHeader entry, MessageSendingMessageType sendingType)
		{
			if (sendingType == MessageSendingMessageType.Deletion)
			{
				throw new NotSupportedException(NotSupportingWithdrawalExceptionMessage);
			}
			return new DataLoadingModuleMessageBuilder(entry);
		}

		EDIMessage[] GenerateOriginalMessages(CusEntryHeader entry, bool isForAmendmentDetection)
		{
			List<EDIMessage> result = new List<EDIMessage>();
			DataLoadingModuleMessageBuilder builder = GetMessageBuilder(entry, MessageSendingMessageType.Original);
			if (builder != null)
			{
				EDIMessage message = builder.PopulateMessage();
				if (message != null)
				{
					result.Add(message);
					if (!isForAmendmentDetection)
					{
						entry.Messages.Add(message);
						SetSentStatusIfRequired(entry, message);
					}
				}
			}

			return result.ToArray();
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			List<EDIMessage> result = new List<EDIMessage>();

			if (action.CA_SendMessage)
			{
				CusEntryHeader entry = (CusEntryHeader)bizo;
				DataLoadingModuleMessageBuilder builder = GetMessageBuilder(entry, MessageSendingMessageType.Amendment);

				EDIMessage message = builder.PopulateMessage();

				if (message != null)
				{
					result.Add(message);
					entry.Messages.Add(message);
					SetSentStatusIfRequired(entry, message);
				}
			}
			return result.ToArray();
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			throw new NotSupportedException(NotSupportingWithdrawalExceptionMessage);
		}

		void SetSentStatusIfRequired(IMessageAttachee entry, EDIMessage lastMessage)
		{
			if (lastMessage.IsTransmitMessage && lastMessage.EM_MessageSubType == MessageTypeList.Codes.DataLoadingModule)
			{
				entry.MessageStatus = MessageStatusList.Codes.Sent;
			}
		}

		readonly CAMessageSendingAction action;
		const string NotSupportingWithdrawalExceptionMessage = "Data Loading Module is currently not supporting Withdrawal";

		#endregion
	}
}
