using System.Xml;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	class AesInterchangeCustomsDataProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestInterchangeRecipientEORIBranch()
		{
			NUnit.Framework.Assert.That(customsDataProvider.InterchangeRecipientEORIBranch, Is.EqualTo("0001"));
		}

		[ExpectNoExceptions]
		public void TestInterchangeRecipientEORIBranch_InvalidXml()
		{
			NUnit.Framework.Assert.That(customsDataProviderWithIncorrectInput.InterchangeRecipientEORIBranch, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			NUnit.Framework.Assert.That(customsDataProvider.LocalReferenceNumber, Is.EqualTo("LOCALREFERENCENUMBER"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_InvalidPath()
		{
			NUnit.Framework.Assert.That(customsDataProviderWithIncorrectInput.LocalReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestMessageSubType()
		{
			NUnit.Framework.Assert.That(customsDataProvider.MessageSubType, Is.EqualTo("EXP"));
		}

		[ExpectNoExceptions]
		public void TestMessageSubType_InvalidPath()
		{
			NUnit.Framework.Assert.That(customsDataProviderWithIncorrectInput.MessageSubType, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestMessageType()
		{
			NUnit.Framework.Assert.That(customsDataProvider.MessageType, Is.EqualTo(EDIMessageTypeList.Codes.AES));
		}

		[ExpectNoExceptions]
		public void TestMessageType_Unknown()
		{
			NUnit.Framework.Assert.That(customsDataProviderWithIncorrectInput.MessageType, Is.EqualTo("UNK"));
		}

		protected override void SetUp()
		{
			base.MasterSetUp();
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(TestCustomsData);
			customsDataProvider = new AesInterchangeCustomsDataProvider(Factory, xmlDocument.FirstChild);
			customsDataProviderWithIncorrectInput = new AesInterchangeCustomsDataProvider(Factory, xmlDocument);
		}
		AesInterchangeCustomsDataProvider customsDataProvider;
		AesInterchangeCustomsDataProvider customsDataProviderWithIncorrectInput;

		const string TestCustomsData = @"<DEXPRF>
	<preparationDateAndTime>2021-07-06T15:48:00</preparationDateAndTime>
	<messageIdentification>3000000012</messageIdentification>
	<messageGroup>EXP</messageGroup>
	<messageType>DEXPRF</messageType>
	<messageVersion>F.1.2</messageVersion>
	<correlationIdentifier>HYEZNTCMT00000000000672</correlationIdentifier>
	<MessageSender>
		<referenceNumber>DE005866</referenceNumber>
	</MessageSender>
	<MessageRecipient>
		<identificationNumber>DE9000348</identificationNumber>
		<subsidiaryNumber>0001</subsidiaryNumber>
	</MessageRecipient>
	<ExportOperation>
		<LRN>LOCALREFERENCENUMBER</LRN>
		<declarationType>AA</declarationType>
		<additionalDeclarationType>A</additionalDeclarationType>
		<exportDeclarationType>20000000</exportDeclarationType>
		<partyConstellation>1000</partyConstellation>
		<declarationRecordationDateAndTime>2000-01-01T00:00:00</declarationRecordationDateAndTime>
		<declarationAcceptanceDateAndTime>2000-01-01T00:00:00</declarationAcceptanceDateAndTime>
		<releaseDateAndTime>2000-01-01T00:00:00</releaseDateAndTime>
		<decisiveDate>2000-01-01</decisiveDate>
		<exitDate>2000-01-01</exitDate>
		<security>0</security>
		<specificCircumstanceIndicator>A00</specificCircumstanceIndicator>
		<totalAmountInvoiced>0</totalAmountInvoiced>
		<invoiceCurrency>AAA</invoiceCurrency>
	</ExportOperation>
</DEXPRF>";
	}
}
