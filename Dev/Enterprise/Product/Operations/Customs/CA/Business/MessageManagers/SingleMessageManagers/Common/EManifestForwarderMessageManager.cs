//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Business.MessageManagers;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Environment;

	public abstract class CAEManifestForwarderMessageManager : CAResetToOriginalMessageManager
	{
		protected CAEManifestForwarderMessageManager(ICAEDIFACTMessageAttachee dataWrapper, EDIFACTMessageStatusCalculator statusCalculator, IUserNotification notification)
			: base(dataWrapper, statusCalculator, notification)
		{
		}

		public virtual void RefreshDetails()
		{ }

		protected override void ResetToOriginalCore()
		{
			DataWrapper.JobStatus = ZString.Empty;
			DataWrapper.MessageStatus = ZString.Empty;
			foreach (EDIMessage message in DataWrapper.Messages)
			{
				message.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Discarded;
			}
		}

		protected override bool ShouldWaitUntilResponded
		{
			get { return false; }
		}

		protected virtual bool AllowSendingMessageWhenAwaitingReply
		{
			get { return true; }
		}

		protected virtual bool ShouldSendWithAmendment
		{
			get { return false; }
		}

		public override MessageSendingNotificationCollection GetNotificationsForSendingAnOriginal()
		{
			var result = base.GetNotificationsForSendingAnOriginal();
			ZString errorText;
			if (!CanSendThisMessage(MessageSubTypes.Create, out errorText))
			{
				result.AddWarning(Res.GetString("A5823253-5F0E-4137-B71F-9BD6F0E60F86", "As {0}", errorText));
			}
			else
			{
				GetNotificationForSendingMessageWhenAwaitingReply(result);
			}
			return result;
		}

		public override MessageSendingNotificationCollection GetNotificationsForSendingAWithdrawal()
		{
			var result = base.GetNotificationsForSendingAWithdrawal();
			ZString errorText;
			if (!CanSendThisMessage(MessageSubTypes.Withdraw, out errorText))
			{
				result.AddWarning(Res.GetString("33E53344-C2BB-4953-B0C7-887A82F06A22", "As {0}", errorText));
			}
			else
			{
				GetNotificationForSendingMessageWhenAwaitingReply(result);
			}
			return result;
		}

		void GetNotificationForSendingMessageWhenAwaitingReply(MessageSendingNotificationCollection notifications)
		{
			if (StatusCalculator.IsAwaitingReply(DataWrapper.MessageStatus))
			{
				if (AllowSendingMessageWhenAwaitingReply)
				{
					notifications.AddWarning(Res.GetString("0F7CBA4C-B3D0-465A-BAEF-DCE4EFDA86DB", "There are messages waiting for responses, are you sure you wish to re-send now?"));
				}
				else
				{
					notifications.AddError(Res.GetString("FF3188B4-2B68-444A-8B17-1BD50C76773B", "There are messages waiting for responses, please do not send it again."));
				}
			}
		}

		protected override MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			var result = base.GetCommonNotificationsForSending();
			var additionalWarningsMessage = base.GetAdditionalWarningsMessage(MessageSubTypes.Undefined);
			if (!additionalWarningsMessage.IsEmpty)
			{
				result.AddWarning(additionalWarningsMessage);
			}
			return result;
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			var actionCode = MessageSubTypes.Undefined;
			DefineActionCodeIfUndefined(ref actionCode);
			return PopulateMessage(actionCode);
		}

		protected override bool DefineActionCodeIfUndefined(ref MessageSubTypes actionCode)
		{
			var result = base.DefineActionCodeIfUndefined(ref actionCode);
			if (result)
			{
				if (DefineActionCodeEvent != null)
				{
					DefineActionCodeEvent(ref actionCode);
				}
				else if ((actionCode == MessageSubTypes.Change && ((IACIForwarderMessageProvider)DataWrapper).IsPostArrival) || ShouldSendWithAmendment)
				{
					actionCode = MessageSubTypes.Request;
				}
			}
			return result;
		}
		public event DefineActionCodeDelegate DefineActionCodeEvent;
		public delegate void DefineActionCodeDelegate(ref MessageSubTypes actionCode);

		protected override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ConsolCAeManifestHouseBill; }
		}

		protected override Security.SecurityCheckpoint SendWithMessageErrorsSecurityCheckpoint
		{
			get { return Env.Security.ConsolCAeManifestSendWithMessageErrors; }
		}
	}
}
