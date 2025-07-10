using System.Xml;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	sealed class NctsInterchangeCustomsDataProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestInterchangeRecipientEORIBranch()
		{
			NUnit.Framework.Assert.That(customsDataProvider.InterchangeRecipientEORIBranch, Is.EqualTo("0057"));
		}

		[ExpectNoExceptions]
		public void TestInterchangeRecipientEORIBranch_InvalidPath()
		{
			NUnit.Framework.Assert.That(customsDataProviderWithIncorrectInput.InterchangeRecipientEORIBranch, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			NUnit.Framework.Assert.That(customsDataProvider.LocalReferenceNumber, Is.EqualTo("3_1_2_SCPRL1_91_NZ"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_InvalidPath()
		{
			NUnit.Framework.Assert.That(customsDataProviderWithIncorrectInput.LocalReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestMessageSubType()
		{
			NUnit.Framework.Assert.That(customsDataProvider.MessageSubType, Is.EqualTo(NctsMessageSubTypeList.Codes.StatusRequestMessage));
		}

		[ExpectNoExceptions]
		public void TestMessageSubType_InvalidPath()
		{
			NUnit.Framework.Assert.That(customsDataProviderWithIncorrectInput.MessageSubType, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestMessageType()
		{
			NUnit.Framework.Assert.That(customsDataProvider.MessageType, Is.EqualTo(EDIMessageTypeList.Codes.NCTS));
		}

		[ExpectNoExceptions]
		public void TestMessageType_Unknown()
		{
			NUnit.Framework.Assert.That(customsDataProviderWithIncorrectInput.MessageType, Is.EqualTo("UNK"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(TestCustomsData);
			customsDataProvider = new NctsInterchangeCustomsDataProvider(Factory, xmlDocument.FirstChild);
			customsDataProviderWithIncorrectInput = new NctsInterchangeCustomsDataProvider(Factory, xmlDocument);
		}
		NctsInterchangeCustomsDataProvider customsDataProvider;
		NctsInterchangeCustomsDataProvider customsDataProviderWithIncorrectInput;

		const string TestCustomsData = @"<DETQSC>
		<messageGroup>TRQ</messageGroup>
		<messageType>DETQSC</messageType>
		<MessageRecipient>
			<subsidiaryNumber>0057</subsidiaryNumber>
		</MessageRecipient>
		<TransitOperation>
			<LRN>3_1_2_SCPRL1_91_NZ</LRN>
		</TransitOperation>
	</DETQSC>";
	}
}
