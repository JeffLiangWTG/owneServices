using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Messaging.Testing;
using Moq;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.DE.Messaging.Testing.MessageBuilderTestHelper;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestTimeZone]
	abstract class AESMessageBuilderTest<T, K> : MessageBuilderTest<T, K>
		where T : MessageBuilder<K> where K : class
	{
		public void TestMessageSender()
		{
			var expectedXML = @"<MessageSender><identificationNumber>DEEOR001</identificationNumber><subsidiaryNumber>0001</subsidiaryNumber><authenticationNumber>1234567890000000000000000</authenticationNumber></MessageSender>";
			AssertXMLContainsUnformatted(expectedXML, messageBuilder.GetXMLMessage().AsString());
		}

		public void TestMessageRecipientID()
		{
			var expectedXML = @"<MessageRecipient><referenceNumber>DE000011</referenceNumber></MessageRecipient>";
			AssertXMLContainsUnformatted(expectedXML, messageBuilder.GetXMLMessage().AsString());
		}

		[TestDate(2020, 1, 9, 15, 13, 23)]
		[ExpectNoExceptions]
		public void TestPreparationDateTime()
		{
			var expectedXML = @"<preparationDateAndTime>2020-01-09T15:13:00</preparationDateAndTime>";
			NUnit.Framework.Assert.That(messageBuilder.GetXMLMessage().AsString(), Does.Contain(expectedXML));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 1, 9, 15, 13, 23, 111)]
		[ExpectNoExceptions]
		public void TestCompleteMessage()
		{
			NUnit.Framework.Assert.That(TestExtensionsAESVersion3_0.AESTestFilesDirectory + $@"\{GetCompleteMessageFileName()}.txt", CustomConstraints.ASCIIFileSameAsString(messageBuilder.GetXMLMessage().AsString()));
		}

		[ExpectNoExceptions]
		public void TestMessageVersion()
		{
			var expectedXML = $"<messageVersion>{GetExpectedMessageVersion()}</messageVersion>";
			NUnit.Framework.Assert.That(messageBuilder.GetXMLMessage().AsString(), Does.Contain(expectedXML));
		}

		protected abstract string GetExpectedMessageVersion();

		protected abstract string GetCompleteMessageFileName();

		protected static IPartyID GetDeclarantID() => MockPartyId("DEEOR002", "0002").Object;

		protected override void SetUp()
		{
			base.SetUp();
			messageHeader = new Mock<IAESMessageHeader>();
			messageHeader.Setup(x => x.MessageIdentification).Returns("<<SENDERS REFERENCE PLACE HOLDER>>");
			messageHeader.Setup(x => x.InterchangeSender).Returns(MockPartyId("DEEOR001", "0001").Object);
			messageHeader.Setup(x => x.InterchangeRecipientID).Returns("DE000011");
			messageHeader.Setup(x => x.AuthorizationNumber).Returns("1234567890000000000000000");
			messageHeader.Setup(x => x.PreparationDateAndTimeUtc).Returns(new UniversalDateAndTimeProvider(true));
		}
		protected Mock<IAESMessageHeader> messageHeader;
		protected MessageBuilder<K> messageBuilder;
	}
}
