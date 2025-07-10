using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class IIDMessageManagerForTesting : IIDMessageManager
	{
		public IIDMessageManagerForTesting(IIDMessageWrapper dataWrapper, IUserNotification notification)
			: base(dataWrapper, notification)
		{
		}

		public void Call_OnMessageQueuedForSending(MessageSubTypes actionCode)
		{
			base.OnMessageQueuedForSending(actionCode);
		}

		public MessageSubTypes DefineActionCodeIfUndefined()
		{
			var result = MessageSubTypes.Undefined;
			base.DefineActionCodeIfUndefined(ref result);
			return result;
		}

		public new Enterprise.Messaging.Business.EDIMessage[] PopulateMessage(MessageSubTypes actionCode)
		{
			return base.PopulateMessage(actionCode);
		}

		public new IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return base.GetMessageBuilder(actionCode);
		}

		public ZString CanSendThisMessage() => CanSendThisMessage(MessageSubTypes.Create);

		public ZString CanSendThisMessage(MessageSubTypes actionCode)
		{
			var result = ZString.Empty;
			base.CanSendThisMessage(actionCode, out result);
			return result;
		}

		public new ZString GetAdditionalMessageErrors(MessageSubTypes actionCode)
		{
			return base.GetAdditionalMessageErrors(actionCode);
		}

		public new ZString GetAdditionalWarningsMessage(MessageSubTypes actionCode)
		{
			return base.GetAdditionalWarningsMessage(actionCode);
		}

		public new bool NeedValidationForNotificationMessageInstruction(MessageSubTypes actionCode)
		{
			return base.NeedValidationForNotificationMessageInstruction(actionCode);
		}

		public ZString LastNotification(MessageSubTypes actionCodeToSend)
		{
			ShowQueuedForSending(actionCodeToSend);
			return ((TestUserNotification)notification).LastMessage;
		}

		public bool PreCheck4CreditOKToSendChecking_Exposed(JobDeclaration declaration)
		{
			return PreCheck4CreditOKToSendChecking(declaration);
		}

		protected override bool ShouldJobBeSavedBeforeSendingMessage
		{
			get { return ShouldJobBeSavedBeforeSendingMessage_Exposed; }
		}

		public bool ShouldJobBeSavedBeforeSendingMessage_Exposed
		{
			get; set;
		} = true;
	}
}
