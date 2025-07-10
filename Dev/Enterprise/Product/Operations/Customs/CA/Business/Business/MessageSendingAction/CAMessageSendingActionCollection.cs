using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public enum MessageSendingMessageType
	{
		Original,
		Amendment,
		Deletion
	}

	public class CAMessageSendingActionCollection : NonPersistentBusinessObjectCollection<CAMessageSendingAction>
		, Customs.Business.IDeferredAmendmentSavingOptions
	{
		public CAMessageSendingActionCollection(JobDeclaration declaration, MessageSendingMessageType messageType)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			this.messageSendingMessageType = messageType;
			PopulateElements();

			if (Count == 1)
			{
				CAMessageSendingAction action = this[0];

				using (action.SuspendSettingHasChanges())
				{
					this[0].CA_SendMessage = true;
				}
			}
		}

		public readonly JobDeclaration declaration;
		public readonly MessageSendingMessageType messageSendingMessageType;

		public bool IsOriginal
		{
			get { return messageSendingMessageType == MessageSendingMessageType.Original; }
		}

		public bool IsAmendment
		{
			get { return messageSendingMessageType == MessageSendingMessageType.Amendment; }
		}

		public bool IsWithdrawal
		{
			get { return messageSendingMessageType == MessageSendingMessageType.Deletion; }
		}

		public CAMessageSendingAction FindFirstElement(MessageType messageType)
		{
			foreach (CAMessageSendingAction action in this)
			{
				if (action.messageType == messageType)
				{
					return action;
				}
			}
			return null;
		}

		public bool HasAtLeastOneToSendMessageFor
		{
			get
			{
				foreach (CAMessageSendingAction action in this)
				{
					if (action.CA_SendMessage)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool SaveWithoutSending
		{
			get
			{
				bool result = false;
				if (!IsCancelled)
				{
					foreach (CAMessageSendingAction action in this)
					{
						if (action.CA_SaveWithoutSending)
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		public bool SendMessage
		{
			get
			{
				bool result = false;
				if (!IsCancelled)
				{
					foreach (CAMessageSendingAction action in this)
					{
						if (action.CA_SendMessage)
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		public
#if DEBUG
 virtual
#endif
 ZBool IsCancelled
		{
			get { return fIsCancelled; }
			set { fIsCancelled = value; }
		}
		ZBool fIsCancelled;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public bool SendMessagesWithoutSaving(Customs.Business.ISendsMessagesToCustoms sender)
		{
			JobDeclarationMessageManager manager = new JobDeclarationMessageManager(declaration, this);

			bool result = false;

			if (messageSendingMessageType == MessageSendingMessageType.Original)
			{
				result = manager.SendOriginalMessages(sender).Any();
			}
			else if (messageSendingMessageType == MessageSendingMessageType.Amendment)
			{
				result = manager.AmendMessages(sender);
			}
			else if (messageSendingMessageType == MessageSendingMessageType.Deletion)
			{
				result = manager.WithdrawMessages(sender);
			}

			if (result)
			{
				UpdateEntrySubmittedDate();
			}

			return result;
		}

		public void RemoveActionsNotRequiringAmendment(Customs.Business.RequiredMessagesInformation detectionResult)
		{
			List<Customs.Business.SingleMessageManager> managersWithPendingAmendment = new List<Customs.Business.SingleMessageManager>(detectionResult.ManagersToSendMessagesFor);

			foreach (CAMessageSendingAction action in this.ToArray())
			{
				if (!managersWithPendingAmendment.Contains(action.MessageManager))
				{
					Remove(action);
				}
			}
		}

		#region Implementation
		void UpdateEntrySubmittedDate()
		{
			foreach (CAMessageSendingAction action in this)
			{
				if (action.CA_SendMessage && action.actions.IsOriginal)
				{
					action.entry.CH_EntrySubmittedDate = ZDateTime.Now;
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		internal Customs.Business.SingleMessageManager[] GetSingleMessageManagersSelected()
		{
			List<Customs.Business.SingleMessageManager> result = new List<Customs.Business.SingleMessageManager>();
			foreach (CAMessageSendingAction action in this)
			{
				if (action.CA_SendMessage)
				{
					result.Add(action.MessageManager);
				}
			}
			return result.ToArray();
		}

		internal Customs.Business.SingleMessageManager[] GetAllSingleMessageManagers()
		{
			List<Customs.Business.SingleMessageManager> result = new List<Customs.Business.SingleMessageManager>();
			foreach (CAMessageSendingAction action in this)
			{
				result.Add(action.MessageManager);
			}
			return result.ToArray();
		}

		protected virtual void PopulateElements()
		{
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				if (entry.IsDataLoadingModule)
				{
					Add(new CAMessageSendingAction(entry, MessageType.DataLoadingModule, this));
				}
				else
				{
					throw new InvalidOperationException("invalid entry.CH_MessageType" + " " + entry.CH_MessageType);
				}
			}
		}
		#endregion

		#region IDeferredAmendmentSavingOptions Members

		ZBool Customs.Business.IDeferredAmendmentSavingOptions.IsCancelled
		{
			get { return IsCancelled; }
			set { IsCancelled = value; }
		}

		public void ProcessWhenChangesAreSavedWithoutSending()
		{
			if (!IsCancelled)
			{
				foreach (CAMessageSendingAction action in this)
				{
					action.ProcessWhenSavedWithoutSending();
				}
			}
		}

		ZBool Customs.Business.IDeferredAmendmentSavingOptions.ShouldTakeReasonForSavingWithoutSendingSeparately
		{
			get { return ZBool.False; }
		}

		ZBool Customs.Business.IDeferredAmendmentSavingOptions.ShouldSaveWithoutSendingAmendment
		{
			get { return SaveWithoutSending; }
		}

		ZBool Customs.Business.IDeferredAmendmentSavingOptions.ShouldSendMessages
		{
			get { return SendMessage; }
		}

		ZBool Customs.Business.IDeferredAmendmentSavingOptions.SignificantAmendmentsHaveBeenMade
		{
			get { return ZBool.False; }
		}

#if DEBUG

		void Customs.Business.IDeferredAmendmentSavingOptions.SetSaveWithEntryChangesValueForTestingTo(ZBool value)
		{
			if (Count > 0)
			{
				this[0].CA_SaveWithoutSending = value;
			}
		}

		void Customs.Business.IDeferredAmendmentSavingOptions.SetSaveWithoutEntryChangesValueForTestingTo(ZBool value)
		{
			if (Count > 0)
			{
				this[0].CA_SaveWithoutSending = value;
			}
		}

		void Customs.Business.IDeferredAmendmentSavingOptions.SetSendAmendmentValueForTestingTo(ZBool value)
		{
			if (Count > 0)
			{
				this[0].CA_SendMessage = value;
			}
		}

#endif

		#endregion
	}
}
