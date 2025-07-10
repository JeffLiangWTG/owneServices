using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDMultiMessageManager : Customs.Business.MultiMessageManager, Customs.Business.IMessageManager
	{
		public IMDMultiMessageManager(JobDeclaration jobDeclaration, CMRMessageTypes messageType)
			: base()
		{
			this.jobDeclaration = jobDeclaration;
			this.MessageType = messageType;
		}

		protected override IEnumerable<INotification> GetNewMessageErrorCollector()
		{
			return new ZNotificationCollectorMinusCPDecQuestionsAndDuplicates(jobDeclaration).GetMessageErrors();
		}

		protected override bool RunPreSaveValidationWhenSendingAMessage
		{
			get { return false; }
		}

		protected override BusinessObject TopLevelBusinessObjectForValidation
		{
			get
			{
				if (ConsolidatedDeclaration.GetConsolidatedDeclaration(jobDeclaration) is ConsolidatedDeclaration consolidatedDeclaration)
				{
					return consolidatedDeclaration;
				}
				else
				{
					return base.TopLevelBusinessObjectForValidation;
				}
			}
		}

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return jobDeclaration; }
		}

		internal class ZNotificationCollectorMinusCPDecQuestionsAndDuplicates : Customs.Business.CustomsNotificationCollector
		{
			public ZNotificationCollectorMinusCPDecQuestionsAndDuplicates(IBusiness topBizObj)
				: base(topBizObj, true, false, PropertyDescriptionType.HumanReadableName)
			{
			}

			protected override bool ShouldIncludeNotificationsFromObject(BusinessObject child)
			{
				return !(child is CMRCusEntryCPDec) && base.ShouldIncludeNotificationsFromObject(child);
			}

			protected override bool ShouldIncludeNotificationsFromInfo(ZPropertyInfo info)
			{
				return !info.Name.Equals(AUAddInfo.Schema.AddInfoLine) && base.ShouldIncludeNotificationsFromInfo(info);
			}
		}

		#region Public Properties
		public bool CanSendOriginalMessage
		{
			get
			{
				foreach (IMDMessageManager singleManager in AllMessageManagers)
				{
					if (singleManager.CanSendOriginal)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool CanSendWithdrawal
		{
			get
			{
				foreach (IMDMessageManager singleManager in AllMessageManagers)
				{
					if (singleManager.CanSendWithdrawal)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool CanSendAmendment
		{
			get
			{
				foreach (IMDMessageManager singleManager in AllMessageManagers)
				{
					if (singleManager.CanSendAmendment)
					{
						return true;
					}
				}
				return false;
			}
		}

		public new CMRAmendmentWithdrawalReason AmendmentWithdrawalReason
		{
			get { return (CMRAmendmentWithdrawalReason)base.AmendmentWithdrawalReason; }
			set { base.AmendmentWithdrawalReason = value; }
		}

		public EFTPaymentInformationCollection EFTPaymentInformations
		{
			get { return fEFTPaymentInformations; }
			set { fEFTPaymentInformations = value; }
		}
		EFTPaymentInformationCollection fEFTPaymentInformations;
		#endregion

		#region Public Methods

		public bool SendMessages(Customs.Business.ISendsMessagesToCustoms sender)
		{
			var result = false;

			switch (MessageType)
			{
				case CMRMessageTypes.Amendment:
					result = AmendMessages(sender);
					break;

				case CMRMessageTypes.LodgeWithoutPay:
				case CMRMessageTypes.LodgeWithPay:
				case CMRMessageTypes.PreLodge:
				case CMRMessageTypes.Payment:
					{
						var messages = SendOriginalMessages(sender);
						result = messages.Any() || jobDeclaration.IsQueuedEntryLodgement || jobDeclaration.IsQueuedEntryPayment;
					}
					break;

				case CMRMessageTypes.Withdrawal:
					result = WithdrawMessages(sender);
					break;
			}

			return result;
		}

		public bool SendWithdrawalMessages(CusEntryHeader[] entriesToSend)
		{
			IMDMessageManager[] managers = GetManagersFromEntryHeader(entriesToSend);
			return WithdrawMessages(jobDeclaration.MessageInitiator, managers);
		}

		public bool SendMessagesFromAmendmentMenu(CusEntryHeader[] entriesToSend)
		{
			IMDMessageManager[] managers = GetManagersFromEntryHeader(entriesToSend);
			return AmendMessages(jobDeclaration.MessageInitiator, managers);
		}

		public int SendQueuedMessage(JobDeclaration jobDeclaration, NotificationBuffer notificationBuffer, ZString queuedUserCode)
		{
			var manager = new IMDMessageManager(jobDeclaration.EntryHeader, this);
			var generatedMessages = manager.GenerateOriginalMessages(jobDeclaration.EntryHeader);
			var result = generatedMessages.Length;
			if (result > 0)
			{
				OnOneOrMoreOriginalsSent();
				jobDeclaration.LogCustomsCommencedIfNeeded();

				foreach (EDIMessage message in generatedMessages)
				{
					message.EM_SystemCreateUser = queuedUserCode;
				}
			}

			var messageNotifications = manager.GetNotificationsForSendingAnOriginal();
			foreach (var notification in messageNotifications)
			{
				if (notification.IsWarning)
				{
					notificationBuffer.AddWarning(notification.Message);
				}
				else if (notification.IsError)
				{
					notificationBuffer.AddError(notification.Message);
				}
			}

			return result;
		}

		#endregion

		#region Implementation

		public override bool ShouldSendMessagesInTestMode
		{
			get { return Env.Registry.CMRTestMode; }
		}

		protected override bool DeferredAmendmentTillAfterSaveSuccessfulCore
		{
			get { return jobDeclaration.IsWHSUniversalXMLActive; }
		}

		protected override string OriginalMessageTypeUsedInConfirmation
		{
			get
			{
				return MessageType == CMRMessageTypes.Payment ? "payment" : base.OriginalMessageTypeUsedInConfirmation;
			}
		}

		protected override void OnMessagesSent(Customs.Business.ISendsMessagesToCustoms sender)
		{
			base.OnMessagesSent(sender);

			if (!jobDeclaration.IsQueuedEntryLodgement)
			{
				jobDeclaration.LogCustomsCommencedIfNeeded();
			}
		}

		protected override void OnOneOrMoreOriginalsSent()
		{
			base.OnOneOrMoreOriginalsSent();
			if (MessageType == CMRMessageTypes.LodgeWithPay ||
				MessageType == CMRMessageTypes.LodgeWithoutPay ||
				MessageType == CMRMessageTypes.PreLodge)
			{
				#pragma warning disable IDE0001 // Prevent simplification to base class
				using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(jobDeclaration))
				#pragma warning restore IDE0001 // Prevent simplification to base class
				{
					jobDeclaration.JE_EntrySubmittedDate = ZDateTime.Now;
					SetSentWithMessageErrorsFlag();
				}
			}
		}

		protected override void OnOneOrMoreAmendmentsSent()
		{
			base.OnOneOrMoreAmendmentsSent();
			#pragma warning disable IDE0001 // Prevent simplification to base class
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(jobDeclaration))
			#pragma warning restore IDE0001 // Prevent simplification to base class
			{
				SetSentWithMessageErrorsFlag();
			}
		}

		void SetSentWithMessageErrorsFlag()
		{
			bool sentWithMessageErrors = jobDeclaration.HasMessageErrors;
			foreach (CusEntryHeader entryHeader in jobDeclaration.ActiveEntryHeaders)
			{
				if (entryHeader.Messages != null && entryHeader.Messages.LastOutgoingMessage != null)
				{
					entryHeader.Messages.LastOutgoingMessage.EM_SendWithMessageErrors = sentWithMessageErrors;
				}
			}
		}

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return true; }
		}

		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			ArrayList result = new ArrayList();

			foreach (CusEntryHeader entryHeader in jobDeclaration.ActiveEntryHeaders)
			{
				bool shouldAddAManagerForThisEntry = MessageType != CMRMessageTypes.Payment || ShouldGeneratePaymentMessageForThisEntryHeader(entryHeader);

				if (shouldAddAManagerForThisEntry)
				{
					IMDMessageManager singleManager = new IMDMessageManager(entryHeader, this);
					result.Add(singleManager);
				}
			}
			return (Customs.Business.SingleMessageManager[])result.ToArray(typeof(Customs.Business.SingleMessageManager));
		}

		internal bool ShouldGeneratePaymentMessageForThisEntryHeader(CusEntryHeader entryHeader)
		{
			bool result = false;
			if (MessageType == CMRMessageTypes.Payment && EFTPaymentInformations != null)
			{
				var eFTPayInfo = EFTPaymentInformations.GetElementByEntryHeader(entryHeader);
				if (eFTPayInfo != null && eFTPayInfo.HasAmountsToPay)
				{
					result = true;
				}
			}
			return result;
		}

		internal IMDMessageManager[] GetManagersFromEntryHeader(CusEntryHeader[] entries)
		{
			ArrayList iMDMessageManagers = new ArrayList();
			foreach (CusEntryHeader entryHeader in entries)
			{
				foreach (IMDMessageManager singleManager in AllMessageManagers)
				{
					if (singleManager.BusinessObject.PK == entryHeader.PK)
					{
						iMDMessageManagers.Add(singleManager);
						break;
					}
				}
			}

			if (entries.Length != iMDMessageManagers.Count)
			{
				ZStringBuilder errors = new ZStringBuilder();
				errors.Append("User selected");
				foreach (CusEntryHeader entryHeader in entries)
				{
					errors.Append("\r\nEntry 1:" + entryHeader.EntryNumber + " " + entryHeader.PK);
				}

				errors.Append("\r\nEntries in AllMessageManagers");
				foreach (IMDMessageManager singleManager in AllMessageManagers)
				{
					errors.Append("\r\n" + singleManager.BusinessObject.PK);
				}

				ErrorReporter.ReportOnce("Number of managers != number of entries when sending an amendment", errors.ToString());
			}

			return (IMDMessageManager[])iMDMessageManagers.ToArray(typeof(IMDMessageManager));
		}

		public readonly CMRMessageTypes MessageType;
		readonly JobDeclaration jobDeclaration;

		#endregion

		#region IMessageManager Members

		public Customs.Business.MessageSendingNotificationCollection CheckBusinessObjectLevelValidationIfRequired()
		{
			Customs.Business.MessageSendingNotificationCollection result;
			result = Validation.CheckBusinessObjectLevelValidation(ShouldSendMessagesInTestMode);
			if (!result.ContainsError())
			{
				var paymentDate = ZDateTime.Empty;
				foreach (CusEntryHeader entryHeader in jobDeclaration.ActiveEntryHeaders)
				{
					foreach (EDIMessage message in entryHeader.Messages)
					{
						if (message.EM_MessageType == CMRMessage.CMRMessageTypes.PAYREC)
						{
							var date = message.EM_SystemCreateTimeUtc;
							if (date < paymentDate || paymentDate == ZDateTime.Empty)
							{
								paymentDate = date;
							}
						}
					}
				}

				if (paymentDate != ZDateTime.Empty && paymentDate.AddYears(4) < ZDateTime.Today)
				{
					result.AddWarning(result.ContainsWarning() ? "\r\n" + WarningPaymentDate : WarningAndConfirmationWarningPaymentDate);
				}
			}
			return result;
		}

		public static string WarningPaymentDate
		{
			get { return Res.GetString("E08ABBE7-F76A-499F-B32A-3453A0374B46", "Date of duty payment is more than 4 years in the past."); }
		}

		public static string WarningAndConfirmationWarningPaymentDate
		{
			get { return Res.GetString("C01C5786-FD5E-40B8-B97D-EB064C4B805C", "Date of duty payment is more than 4 years in the past. Do you wish to continue?"); }
		}

		Customs.Business.MessageGenerationResultCollection Customs.Business.IMessageManager.SendAnyMessagesRequired(Customs.Business.RequiredMessagesInformation information)
		{
			AmendmentWithdrawalReason = (CMRAmendmentWithdrawalReason)information.AmendmentWithdrawalReason;
			return base.SendAnyMessagesRequired(information);
		}

		Customs.Business.IDeferredAmendmentSavingOptions Customs.Business.IMessageManager.GetDeferredAmendmentSavingOptions()
		{
			return GetDeferredAmendmentSavingOptionsCore();
		}

		protected virtual DeferredAmendmentSavingOptions GetDeferredAmendmentSavingOptionsCore()
		{
			return new DeferredAmendmentSavingOptions(jobDeclaration);
		}

		protected override void ProcessWhenChangesAreSavedWithoutSendingCore(Customs.Business.IDeferredAmendmentSavingOptions saveOptions, Customs.Business.RequiredMessagesInformation information)
		{
			(saveOptions as DeferredAmendmentSavingOptions)?.ProcessWhenChangesAreSavedWithoutSending(information.AmendmentWithdrawalReason.ReasonText);
		}

		#endregion
	}
}
