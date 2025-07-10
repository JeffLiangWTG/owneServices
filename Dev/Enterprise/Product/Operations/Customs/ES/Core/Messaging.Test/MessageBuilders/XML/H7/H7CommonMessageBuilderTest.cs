using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class H7CommonMessageBuilderTest<TMessageBuilder, TProvider, T> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
		where TProvider : class, IESEDIMessageCollectionProvider
		where TMessageBuilder : XMLMessageBuilder<TProvider, T>
	{
		public abstract void TestMessageTextWhenIsTestIsFalse();
		protected override sealed ZString GetSignedMessageTestFileContent() => GetTestFile();
		protected override sealed ZString GetUnsignedMessageTestFileContent() => GetTestFile();
		protected abstract ZString GetTestFile();
	}
}
