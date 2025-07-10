using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Edifact.Auto;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestsSubclassesOf(typeof(EDIFACTMessageBuilder<IEDIFACTMessageDataProvider, SegmentGroup>))]
	public abstract class EDIFACTMessageBuilderTest<TMessageBuilder, TProvider, TSegmentGroup> : MessageBuilderTest<TMessageBuilder, TProvider>
		where TProvider : IEDIFACTMessageDataProvider
		where TSegmentGroup : SegmentGroup
		where TMessageBuilder : EDIFACTMessageBuilder<TProvider, TSegmentGroup>
	{
		protected abstract ZString DeclarantIdForUNBSegment { get; }

		protected sealed override void AssertUnsignedMessageText(ZString messageText) => AssertMessageText(messageText);
		protected sealed override void AssertSignedMessageText(ZString messageText) => AssertMessageText(messageText);

		void AssertMessageText(ZString messageText)
		{
			AssertStartsWith("UNB Segment", @"UNB+UNOA:1", messageText);

			var testFile = GetTestFile();
			AssertMultilineASCIIEquals(testFile, messageText.Replace("'", "'\n"));

			AssertEndsWith("UNZ Segment", @"UNZ+1+<<MSGNO PLACEHOLDER>>'", messageText);
		}

		protected abstract ZString GetTestFile();

		public abstract void TestUNBNotTest();

		protected string GetTestFileContents(string testFilePath, string fileName)
		{
			return TestFileReader.GetEmbeddedFileText(testFilePath, fileName);
		}

		protected virtual TestFileReader TestFileReader => testFileReader ?? (testFileReader = new TestFileReader(typeof(EDIFACTMessageBuilderTest<,,>)));
		TestFileReader testFileReader;
	}
}
