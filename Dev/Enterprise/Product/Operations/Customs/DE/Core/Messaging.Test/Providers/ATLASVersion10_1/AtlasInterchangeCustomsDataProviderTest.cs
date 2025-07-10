using System.Xml;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	sealed class AtlasInterchangeCustomsDataProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestInterchangeRecipientEORIBranch_MetaData()
		{
			NUnit.Framework.Assert.That(customsDataProvider.InterchangeRecipientEORIBranch, Is.EqualTo("0057"));
		}

		[ExpectNoExceptions]
		public void TestInterchangeRecipientEORIBranch_NoMetaData()
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(TestCustomsData_NoMetaData);
			customsDataProvider = new AtlasInterchangeCustomsDataProvider(Factory, xmlDocument.FirstChild);
			NUnit.Framework.Assert.That(customsDataProvider.InterchangeRecipientEORIBranch, Is.EqualTo("0000"));
		}

		[ExpectNoExceptions]
		public void TestInterchangeRecipientEORIBranch_InvalidXml()
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
		public void TestMessageSubType_MetaData()
		{
			NUnit.Framework.Assert.That(customsDataProvider.MessageSubType, Is.EqualTo("SAN"));
		}

		[ExpectNoExceptions]
		public void TestMessageSubType_NoMetaData()
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(TestCustomsData_NoMetaData);
			customsDataProvider = new AtlasInterchangeCustomsDataProvider(Factory, xmlDocument.FirstChild);
			NUnit.Framework.Assert.That(customsDataProvider.MessageSubType, Is.EqualTo("SKM"));
		}

		[ExpectNoExceptions]
		public void TestMessageSubType_InvalidPath()
		{
			NUnit.Framework.Assert.That(customsDataProviderWithIncorrectInput.MessageSubType, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestMessageType_TemporaryStorage()
		{
			AssertMessageTypes(new TemporaryStorageMessageSubTypeList().GetAllCodes(), EDIMessageTypeList.Codes.TemporaryStorage);
		}

		[ExpectNoExceptions]
		public void TestMessageType_NCTS()
		{
			AssertMessageTypes(new NctsMessageSubTypeList().GetAllCodes(), EDIMessageTypeList.Codes.NCTS);
		}

		[ExpectNoExceptions]
		public void TestMessageType_Import()
		{
			AssertMessageTypes(new ImportMessageSubTypeList().GetAllCodes(), EDIMessageTypeList.Codes.Import);
		}

		[ExpectNoExceptions]
		public void TestMessageType_Import_MonthlyClosing()
		{
			AssertMessageTypes(new MonthlyClosingMessageSubTypeList().GetAllCodes(), EDIMessageTypeList.Codes.Import);
		}

		[ExpectNoExceptions]
		public void TestMessageType_Unknown()
		{
			NUnit.Framework.Assert.That(customsDataProviderWithIncorrectInput.MessageType, Is.EqualTo("UNK"));
		}

		protected override void SetUp()
		{
			MasterSetUp();
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(TestCustomsData);
			customsDataProvider = new AtlasInterchangeCustomsDataProvider(Factory, xmlDocument.FirstChild);
			customsDataProviderWithIncorrectInput = new AtlasInterchangeCustomsDataProvider(Factory, xmlDocument);
		}
		AtlasInterchangeCustomsDataProvider customsDataProvider;
		AtlasInterchangeCustomsDataProvider customsDataProviderWithIncorrectInput;

		[ExpectNoExceptions]
		void AssertMessageTypes(string[] messageGroupCodes, string expectedMessageType)
		{
			var xmlDocument = new XmlDocument();
			CombineAssertions(() =>
			{
				foreach (var code in messageGroupCodes)
				{
					var customsTestData = $@"
<GCRECF>
	<MetaData>
		<MessageGroup>{code}</MessageGroup>
	</MetaData>
</GCRECF>";

					xmlDocument.LoadXml(customsTestData);
					customsDataProvider = new AtlasInterchangeCustomsDataProvider(Factory, xmlDocument.FirstChild);
					NUnit.Framework.Assert.That(customsDataProvider.MessageType, Is.EqualTo(expectedMessageType), code);
				}
			});
		}

		const string TestCustomsData = @"
<GCRECF>
	<MetaData>
		<Preparation>
			<Date>2021-07-08</Date>
			<Time>12:05:00</Time>
		</Preparation>
		<InterchangeControlReference>0000000564364</InterchangeControlReference>
		<MessageReferenceNumber>1</MessageReferenceNumber>
		<MessageIdentifier>CUSREC58750000000564364080721120530</MessageIdentifier>
		<MessageGroup>SaN</MessageGroup>
		<MessageType>GCRECF</MessageType>
		<InterchangeSender>
			<Identification>
				<ReferenceNumber>DE005875</ReferenceNumber>
			</Identification>
		</InterchangeSender>
		<InterchangeRecipient>
			<Identification>
				<ReferenceNumber>DE8999120</ReferenceNumber>
				<SubsidiaryNumber>0057</SubsidiaryNumber>
			</Identification>
		</InterchangeRecipient>
	</MetaData>
	<Header>
		<MessageVersion>E.1.8</MessageVersion>
		<ReferencedMessageIdentifier>HYEWT1CM200000000000414</ReferencedMessageIdentifier>
		<ReferenceNumber>ATB150000630720215875</ReferenceNumber>
		<LRN>3_1_2_SCPRL1_91_NZ</LRN>
	</Header>
</GCRECF>";

		const string TestCustomsData_NoMetaData = @"
<DEIACA>
	<preparationDateAndTime>2024-06-06T15:49:00</preparationDateAndTime>
	<messageIdentification>9905071580</messageIdentification>
	<messageGroup>SKM</messageGroup>
	<messageType>DEIACA</messageType>
	<messageVersion>A.1.4</messageVersion>
	<MRN>24DE586600522173J8</MRN>
	<notificationDate>2024-06-06T15:48:00</notificationDate>
	<scheduledControlDate>2024-06-06T15:48:00</scheduledControlDate>
	<MessageSender>
		<referenceNumber>DE005875</referenceNumber>
		</MessageSender>
	<MessageRecipient>
		<identificationNumber>DE8999120</identificationNumber>
			<subsidiaryNumber>0000</subsidiaryNumber>
	</MessageRecipient>
	<customsOfficeOfControl>
			<referenceNumber>DE005875</referenceNumber>
	</customsOfficeOfControl>
	<transportDocument>
		<documentNumber>Ordnungsmerkmal des Transportdokumentes</documentNumber>
			<type>C625</type>
	</transportDocument>
	<activeBorderTransportMeans>
		<identificationNumber>WI-ZG123</identificationNumber>
		<typeOfIdentification>20</typeOfIdentification>
		<typeOfMeansOfTransport>1511</typeOfMeansOfTransport>
		<nationality>DE</nationality>
		<modeOfTransportAtTheBorder>3</modeOfTransportAtTheBorder>
		<conveyanceReferenceNumber>Nummer123</conveyanceReferenceNumber>
	</activeBorderTransportMeans>
	<control>
		<examinationPlace>
			<placeOfExamination>Ort, an dem die Kontrolle vorgenommen wird</placeOfExamination>
			<referenceNumber>Reference 123</referenceNumber>
		</examinationPlace>
		<controlSubject>
			<consignmentMasterLevel>
				<receptacle>
					<receptacleIdentificationNumber>1234</receptacleIdentificationNumber>
				</receptacle>
				<goodsItem>
					<goodsItemNumber>1</goodsItemNumber>
					<packaging>
						<shippingMarks>ADRS</shippingMarks>
						<typeOfPackages>CT</typeOfPackages>
					</packaging>
					<transportEquipment>
						<containerIdentificationNumber>Container1</containerIdentificationNumber>
					</transportEquipment>
				</goodsItem>
				<consignmentHouseLevel>
					<passiveBorderTransportMeans>
						<identificationNumber>Nummer12</identificationNumber>
						<typeOfIdentification>21</typeOfIdentification>
						<nationality>DE</nationality>
					</passiveBorderTransportMeans>
				</consignmentHouseLevel>
			</consignmentMasterLevel>
		</controlSubject>
		<typeOfControls>
			<type>10</type>
		</typeOfControls>
		<additionalInformation>
		  <text>addInfo</text>
		</additionalInformation>
		<temporaryStorageData>
			<MRN>24DE5866A0522173U6</MRN>
			<goodsItemNumber>1</goodsItemNumber>
			<identificationByKey>
				<type>ZZZ</type>
				<key>REF</key>
			</identificationByKey>
		</temporaryStorageData>
	</control>
	<declarant>
		<name>Declarant</name>
		<identificationNumber>DE8999120</identificationNumber>
	</declarant>
	<custodian>
		<identificationNumber>DE8999120</identificationNumber>
		<subsidiaryNumber>0000</subsidiaryNumber>
	</custodian>
</DEIACA>";
	}
}
