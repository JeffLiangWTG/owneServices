using System.Xml;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	class EmcsInterchangeCustomsDataProviderTest : TestCaseWithFactory
	{
		public void TestInterchangeRecipientEORIBranch()
		{
			AssertEquals(string.Empty, customsDataProvider.InterchangeRecipientEORIBranch);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals(string.Empty, customsDataProvider.LocalReferenceNumber);
		}

		public void TestMessageSubType()
		{
			AssertEquals("EMB", customsDataProvider.MessageSubType);
		}

		public void TestMessageSubType_InvalidPath()
		{
			var customsDataProviderWithIncorrectInput = new EmcsInterchangeCustomsDataProvider(Factory, xmlDocument);
			AssertNull(customsDataProviderWithIncorrectInput.MessageSubType);
		}

		public void TestMessageType()
		{
			AssertEquals(EDIMessageTypeList.Codes.EMCS, customsDataProvider.MessageType);
		}

		public void TestMessageType_Unknown()
		{
			var customsDataProviderWithIncorrectInput = new EmcsInterchangeCustomsDataProvider(Factory, xmlDocument);
			AssertEquals("UNK", customsDataProviderWithIncorrectInput.MessageType);
		}

		protected override void SetUp()
		{
			base.MasterSetUp();
			xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(TestCustomsData);
			customsDataProvider = new EmcsInterchangeCustomsDataProvider(Factory, xmlDocument.FirstChild);
		}
		XmlDocument xmlDocument;
		EmcsInterchangeCustomsDataProvider customsDataProvider;

		const string TestCustomsData = @"<ED802B>
		<Header>
			<MessageSender>DE000050</MessageSender>
			<MessageRecipient>DE98000005100</MessageRecipient>
			<DateOfPreparation>2020-10-15</DateOfPreparation>
			<TimeOfPreparation>00:00:28</TimeOfPreparation>
			<InterchangeControlReference>0000043002</InterchangeControlReference>
			<MessageGroup>eMb</MessageGroup>
			<MessageIdentifier>0000043002</MessageIdentifier>
			<MessageVersion>B.1.1</MessageVersion>
		</Header>
	</ED802B>";
	}
}
