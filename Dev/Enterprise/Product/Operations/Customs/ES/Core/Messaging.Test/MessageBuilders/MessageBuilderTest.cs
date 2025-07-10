using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestsSubclassesOf(typeof(MessageBuilder<>))]
	public abstract class MessageBuilderTest<T, TProvider> : TestCaseWithFactory
		where TProvider : IESEDIMessageCollectionProvider
		where T : MessageBuilder<TProvider>
	{
		public abstract void TestConstructor();

		public abstract void TestCreateEDIMessage();

		protected ZString ExpectedMessageSubType => DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
		protected abstract ZString ExpectedMessageType { get; }
		protected abstract void AssertUnsignedMessageText(ZString messageText);
		protected abstract void AssertSignedMessageText(ZString messageText);
		protected abstract T CreateMessageBuilder();
		protected abstract T CreateMessageBuilderWithNullProvider();
	}
}
