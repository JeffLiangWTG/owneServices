//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using System;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Business.MessageManagers;
	using Enterprise.Customs.CA.Business.BatchProcessor;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Environment;
	using Enterprise.Security;

	public abstract class CAMessageManager : EDIFACTMessageManager
	{
		protected CAMessageManager(ICAEDIFACTMessageAttachee dataWrapper, EDIFACTMessageStatusCalculator statusCalculator, IUserNotification notification)
			: base(dataWrapper, statusCalculator, notification)
		{
		}

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			var result = base.CanSendThisMessage(actionCode, out messageText);
			if (result && SecurityCheckpoint != null && !SecurityCheckpoint.IsAllowed)
			{
				messageText = Env.Security.GetErrorMessageForNotAllowed(SecurityCheckpoint);
				result = false;
			}
			if (result)
			{
				messageText = BatchProcessorUtilities.BasicRegistryChecksForMessageSending(ShouldSendMessagesInTestMode, ((ICAEDIFACTMessageAttachee)DataWrapper).IsCancelled);
			}
			return string.IsNullOrEmpty(messageText);
		}

		protected bool IsCreditCheckOKToSend(JobDeclaration declaration, out ZString messageText)
		{
			var result = true;
			messageText = ZString.Empty;

			if (declaration != null && PreCheck4CreditOKToSendChecking(declaration))
			{
				var helper = new MessageManagerCreditCheckWithSecurityHelper(declaration);
				result = helper.IsCreditCheckOKToSend;
				if (!result)
				{
					ShouldSuppressErrorPrompt = helper.IsCreditCheckDoneOutsideCW1;
					messageText = helper.ReasonForNotAllowed;
				}
			}

			return result;
		}

		protected virtual bool PreCheck4CreditOKToSendChecking(JobDeclaration declaration) => declaration.JE_EntryStatus.IsEmpty || declaration.JE_EntryStatus == EntryStatusList.Codes.Cancelled;

		#region Events

		public event EventHandler MessageSending;
		public event EventHandler Validating;
		public event EventHandler SavingToDatabase;

		protected override void OnMessageSending()
		{
			MessageSending?.Invoke(this, EventArgs.Empty);
			base.OnMessageSending();
		}

		protected override void OnSavingToDatabase()
		{
			SavingToDatabase?.Invoke(this, EventArgs.Empty);
			base.OnSavingToDatabase();
		}

		protected override void OnValidationIsBeingRun()
		{
			Validating?.Invoke(this, EventArgs.Empty);
			var declaration = BusinessObject as JobDeclaration;
			if (declaration != null)
			{
				declaration.DeclarationValidator.OverrideValidationType = GetValidationType();
			}
		}

		protected override void OnValidationCompleted()
		{
			var declaration = BusinessObject as JobDeclaration;
			if (declaration != null)
			{
				declaration.DeclarationValidator.OverrideValidationType = ValidateForMessageType.Default;
			}
		}

		#endregion

		protected override void ShowMessageNotSent(MessageSubTypes actionCodeToSend, string messageText)
		{
			if (!ShouldSuppressErrorPrompt)
			{
				base.ShowMessageNotSent(actionCodeToSend, messageText);
			}
		}

		protected sealed override bool ShouldSendMessagesInTestMode => !Env.Instance.IsProductionSystem;

		protected virtual ValidateForMessageType GetValidationType()
		{
			return ValidateForMessageType.Default;
		}

		protected abstract SecurityCheckpoint SecurityCheckpoint { get; }

		#region Rationality Check

		ZString ValidationErrorsMessage(MessageSendingNotificationCollection businessObjectNotificationCollection)
		{
			var result = string.Empty;
			if (businessObjectNotificationCollection != null)
			{
				result = businessObjectNotificationCollection.ErrorNotificationsAsString() +
					businessObjectNotificationCollection.WarningNotificationsAsString();
			}
			return result;
		}

		protected virtual ZString GetAdditionalMessageErrors(MessageSubTypes actionCode)
		{
			return ZString.Empty;
		}

		protected virtual ZString GetAdditionalWarningsMessage(MessageSubTypes actionCode)
		{
			var stringBuilder = new ZStringBuilder();

			if (ShouldAddMQWarningMessage)
			{
				stringBuilder.Append(MQWarningMessage);
			}

			return stringBuilder.ToStringWithDelimiterBetweenAppends("\r\n");
		}

		protected virtual bool NeedValidationForNotificationMessageInstruction(MessageSubTypes actionCode)
		{
			return true;
		}
		#endregion

		protected override bool RunRationalityCheckAndAskForConfirmation(MessageSubTypes actionCode, bool runPreSaveValidation, out bool sendWithMessageErrors)
		{
			var result = false;
			var notificationWithMessageInstruction = notification as IMessageInstructionUserNotification;

			if (!NeedValidationForNotificationMessageInstruction(actionCode))
			{
				result = true;
				sendWithMessageErrors = true;
			}
			else if (notificationWithMessageInstruction != null)
			{
				var notifications = ValidateBusinessObject();
				if (notifications.ContainsError())
				{
					notification.ShowWarning(notifications.NotificationsAsString(), WarningCaption);
					sendWithMessageErrors = false;
				}
				else
				{
					var additionalMessageErrors = GetAdditionalMessageErrors(actionCode);
					var validationErrorMesssage = runPreSaveValidation
						? additionalMessageErrors.IsEmpty ? ValidationErrorsMessage(notifications) : new ZString(additionalMessageErrors + "\r\n\r\n" + ValidationErrorsMessage(notifications))
						: ZString.Empty;
					var additionalWarningsMessage = GetAdditionalWarningsMessage(actionCode);
					var messageInstruction = new MessageInstruction(BusinessObject.Factory, IsWaitingForResponse, validationErrorMesssage, additionalWarningsMessage, BusinessObject, ShowJobReadyForPosting, SendWithMessageErrorsSecurityCheckpoint.IsAllowed);
					result = notificationWithMessageInstruction.ShowMessageInstructionForm(messageInstruction);
					sendWithMessageErrors = !validationErrorMesssage.IsEmpty && result;
				}
			}
			else
			{
				result = base.RunRationalityCheckAndAskForConfirmation(actionCode, runPreSaveValidation, out sendWithMessageErrors);
			}

			return result;
		}

		protected override string AwaitingCustomsResponseMessage
		{
			get { return Res.GetString("af61b526-477a-4d91-8d68-44dc0d01056b", "This job is waiting for a CBSA response.\r\nAre you sure that you want to resend to the CBSA?"); }
		}

		protected override void OnMessageQueuedForSending(MessageSubTypes actionCode)
		{
			base.OnMessageQueuedForSending(actionCode);

			if (ShouldSetEntrySubmittedDate(actionCode))
			{
				var declaration = BusinessObject as JobDeclaration;
				if (declaration != null)
				{ declaration.JE_EntrySubmittedDate = ZDateTime.Now; }
			}
		}

		protected virtual bool ShouldSetEntrySubmittedDate(MessageSubTypes actionCode)
		{
			return false;
		}

		public bool ShowJobReadyForPosting { get { return ShowJobReadyForPostingCore(); } }

		protected virtual bool ShowJobReadyForPostingCore()
		{
			return false;
		}

		protected bool ShouldAddMQWarningMessage
		{
			get
			{
				var testMode = ShouldSendMessagesInTestMode;
				return Business.UniversalReferenceConstants.IsCAMQWAR && BatchProcessorUtilities.CBSAClientID(testMode).StartsWith("INET");
			}
		}

		bool ShouldSuppressErrorPrompt { get; set; }

		#region Reset Declaration

		public void ResetDeclaration()
		{
			var declaration = BusinessObject as JobDeclaration;
			if (declaration != null)
			{
				if (Env.Security.CustomsResetToOriginal.IsAllowed)
				{
					if (ShowConfirmationForReset() && (DataWrapper.JobStatus == ZString.Empty || ShowConfirmationForResetAfterLodged()))
					{
						declaration.ThrowAwayMerge();
						declaration.SetDeclarationException();
						declaration.Factory.Save();
						declaration.MarkApportionmentDirty();
						declaration.HasChanges = true;
					}
				}
				else
				{
					Env.Security.ShowError(Env.Security.CustomsResetToOriginal);
				}
			}
		}

		static string ResetDeclarationCaption
		{
			get { return Res.GetString("1c6e561a-ce6d-4eab-b6b9-64cd8ea89a8e", "Reset Declaration?"); }
		}

		internal static string MQWarningMessage
		{
			get { return Res.GetString("6C361B9E-4791-4CAA-8FFA-31574F1AE8DD", "Your system is still sending Customs messages via the old CIG connection. Support for this will be removed in the near future. All communications will be sent via the more robust MQ connection. Please see the Update Note entitled \"MQ Communications\" for more details."); }
		}

		bool ShowConfirmationForReset()
		{
			var message = Res.GetString("e9ec1ebe-9239-449c-8588-8522723485a4", @"Resetting a declaration will result in Messages sent to Customs being discarded.
Are you certain you want to continue?");
			return notification.ShowConfirmation(message, ResetDeclarationCaption);
		}

		bool ShowConfirmationForResetAfterLodged()
		{
			var message = Res.GetString("45fbb1b0-feea-483c-a586-c7c29f2fc2e0", @"WARNING: This Declaration has already been accepted. There is no reason to use the Reset Declaration option for an accepted declaration. Failure to manage this properly may result in a duplication of lodged declarations.
Are you certain you want to continue?");
			return notification.ShowConfirmation(message, ResetDeclarationCaption);
		}

		#endregion
	}
}
