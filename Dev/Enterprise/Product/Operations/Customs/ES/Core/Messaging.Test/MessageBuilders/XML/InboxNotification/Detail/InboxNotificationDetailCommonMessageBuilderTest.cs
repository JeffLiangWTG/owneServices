using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class InboxNotificationDetailCommonMessageBuilderTest<TMessageBuilder, T> : XMLMessageBuilderTest<TMessageBuilder, IInboxNotificationDetailCommonMessageDataProvider, T>
		where TMessageBuilder : InboxNotificationDetailCommonMessageBuilder<IInboxNotificationDetailCommonMessageDataProvider, T>
	{
		protected sealed override ZString ExpectedMessageType => ZString.Empty;

		protected sealed override ZString GetSignedMessageTestFileContent() => GetTestFile();
		protected sealed override ZString GetUnsignedMessageTestFileContent() => GetTestFile();
		protected abstract ZString GetTestFile();
	}
}
