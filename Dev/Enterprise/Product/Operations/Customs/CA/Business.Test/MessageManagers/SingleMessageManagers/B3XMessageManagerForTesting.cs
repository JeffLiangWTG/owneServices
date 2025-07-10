using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class B3XMessageManagerForTesting : B3XMessageManager
	{
		public B3XMessageManagerForTesting(IB3Header b3Header, bool withB3ScheduleSupport = false, bool defaultScheduleB3 = true)
			: base(b3Header, withB3ScheduleSupport ? new TestUserNotificationWithB3ScheduleSupport(defaultScheduleB3) : new TestMessageInstructionUserNotification())
		{
		}

		public B3XMessageManagerForTesting(IB3Header b3Header, IUserNotification notification)
			: base(b3Header, notification)
		{
		}

		public void SetNextAnswer(bool answer)
		{
			Notification.NextAnswer = answer;
		}

		public string LastMessage
		{
			get { return Notification.LastMessage; }
		}

		public IMessageBuilder GetMessageBuilder_Exposed(MessageSubTypes actionCode)
		{
			return GetMessageBuilder(actionCode);
		}

		public void PopulateMessage_Exposed(MessageSubTypes actionCode)
		{
			PopulateMessage(actionCode);
		}

		public bool CanSendThisMessage_Exposed(MessageSubTypes actionCode, out ZString messageText)
		{
			return CanSendThisMessage(actionCode, out messageText);
		}

		public ValidateForMessageType ValidateTypeForTesting
		{
			get { return GetValidationType(); }
		}

		public void Call_OnMessageQueuedForSending(MessageSubTypes actionCode)
		{
			base.OnMessageQueuedForSending(actionCode);
		}

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			messageText = string.Empty;
			return OverrideCanSendThisMessage || base.CanSendThisMessage(actionCode, out messageText);
		}

		protected override bool RunRationalityCheckAndAskForConfirmation(MessageSubTypes actionCode, bool runPreSaveValidation, out bool sendWithMessageErrors)
		{
			if (OverrideCanSendThisMessage)
			{
				sendWithMessageErrors = false;
				return true;
			}
			else
			{
				return base.RunRationalityCheckAndAskForConfirmation(actionCode, runPreSaveValidation, out sendWithMessageErrors);
			}
		}

		public bool OverrideCanSendThisMessage { private get; set; }

		public TestMessageInstructionUserNotification Notification
		{
			get { return ((TestMessageInstructionUserNotification)notification); }
		}

		public ZString LastNotification(MessageSubTypes actionCodeToSend)
		{
			ShowQueuedForSending(actionCodeToSend);
			return ((TestMessageInstructionUserNotification)notification).LastMessage;
		}
	}
}
