using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	public class EDIReleaseImportMessageManagerForTesting : EDIReleaseImportMessageManager
	{
		public EDIReleaseImportMessageManagerForTesting(IEDIReleaseOGD importDeclarationEDIReleaseWrapper)
			: base(importDeclarationEDIReleaseWrapper, new TestMessageInstructionUserNotification())
		{
		}

		public IMessageBuilder GetMessageBuilder_Exposed(MessageSubTypes actionCode)
		{
			return GetMessageBuilder(actionCode);
		}

		public Enterprise.Messaging.Business.EDIMessage[] PopulateMessage_Exposed(MessageSubTypes actionCode)
		{
			return PopulateMessage(actionCode);
		}

		public bool CanSendThisMessage_Exposed(MessageSubTypes actionCode, out ZString messageText)
		{
			return CanSendThisMessage(actionCode, out messageText);
		}

		public ZString GetAdditionalWarningsMessage_Exposed(MessageSubTypes actionCode)
		{
			return GetAdditionalWarningsMessage(actionCode);
		}

		public ValidateForMessageType ValidateTypeForTesting
		{
			get { return GetValidationType(); }
		}

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			messageText = string.Empty;
			return OverrideCanSendThisMessage || base.CanSendThisMessage(actionCode, out messageText);
		}

		public void Call_OnMessageQueuedForSending(MessageSubTypes actionCode)
		{
			base.OnMessageQueuedForSending(actionCode);
		}

		public bool OverrideCanSendThisMessage { private get; set; }

		public TestMessageInstructionUserNotification Notification
		{
			get { return ((TestMessageInstructionUserNotification)notification); }
		}
	}
}
