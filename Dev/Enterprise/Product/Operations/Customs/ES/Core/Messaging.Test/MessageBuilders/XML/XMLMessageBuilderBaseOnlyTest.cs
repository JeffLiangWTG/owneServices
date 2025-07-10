using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public class XMLMessageBuilderBaseOnlyTest : TestCase
	{
		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestTransactionId()
		{
			var mockProvider = Mock.Of<IESEDIMessageCollectionProvider>();
			var messageBuilder = new XMLMessageBuilderForTest(mockProvider, ZString.Empty, ZString.Empty);
			RandomGeneratorHelper.MockRandomGenerator(messageBuilder, 8765);
			AssertEquals("ES2001091613238765", messageBuilder.TransactionIdForTest);
		}

		public class XMLMessageBuilderForTest : XMLMessageBuilder<IESEDIMessageCollectionProvider, object>
		{
			public XMLMessageBuilderForTest(IESEDIMessageCollectionProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
			{
			}

			public string TransactionIdForTest => TransactionId;

			protected override object GenerateXMLMessage()
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
