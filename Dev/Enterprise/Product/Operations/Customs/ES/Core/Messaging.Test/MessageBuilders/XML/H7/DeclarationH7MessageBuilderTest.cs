using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AltaH7V1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(DeclarationH7MessageBuilder))]
	class DeclarationH7MessageBuilderTest : H7CommonMessageBuilderTest<DeclarationH7MessageBuilder, IDeclarationH7MessageDataProvider, AltaH7V1Ent>
	{
		public override void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When provider is null", () => CreateMessageBuilderWithNullProvider());
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public override void TestCreateEDIMessage()
		{
			var messageBuilder = CreateMessageBuilder();

			CombineAssertions(() =>
			{
				AssertEquals("messageBuilder.MessageType", ExpectedMessageType, messageBuilder.MessageType);
				AssertEquals("messageBuilder.MessageSubType", ExpectedMessageSubType, messageBuilder.MessageSubType);
				AssertEquals("messageBuilder.Provider", mockProvider.Object, messageBuilder.Provider);
				AssertUnsignedMessageText(messageBuilder.UnsignedMessageText);
				AssertSignedMessageText(messageBuilder.GetSignedMessageText());
			});
		}

		public override void TestMessageTextWhenIsTestIsFalse()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertContains("<messageRecipient>ES.AEAT</messageRecipient>", messageText);
			});
		}

		public void TestMessageTextWhenDeclarantSameAsImporter()
		{
			var mockDeclarant = SetUpDeclarant(true);
			mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains(@"<Declarant>
      <ContactPerson>
        <name>contact name</name>
        <eMailAddress>mail.declarant@mail.com</eMailAddress>
        <phoneNumber>987654321</phoneNumber>
      </ContactPerson>
      <identificationNumber>ESA78587268</identificationNumber>
    </Declarant>", messageText);
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.H7Declaration;

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.H7TestFilePath, "TestH7DeclarationMessage.txt");

		protected override DeclarationH7MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new DeclarationH7MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override DeclarationH7MessageBuilder CreateMessageBuilderWithNullProvider() => new DeclarationH7MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();

			mockHeader = SetUpHeader();
			mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);

			var mockLine1 = SetUpLine(1);
			var mockLine2 = SetUpLine(2);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1, mockLine2 });
		}

		Mock<IDeclarationH7Header> mockHeader;

		Mock<IDeclarationH7Header> SetUpHeader()
		{
			var mockHeader = new Mock<IDeclarationH7Header>();
			mockHeader.Setup(m => m.SupervisingCustomsOffice).Returns("ES009999");
			mockHeader.Setup(m => m.GrossWeight).Returns(200.200M);
			mockHeader.Setup(m => m.ReferenceNumberUCR).Returns("UCPReference");
			mockHeader.Setup(m => m.AdditionalProcedures).Returns(new ZString[] { "A01", "B02" });
			mockHeader.Setup(m => m.GoodsLocation).Returns("Location");

			var mockRepresentative = SetUpRepresentative();
			mockHeader.Setup(m => m.Representative).Returns(mockRepresentative);

			var mockDeclarant = SetUpDeclarant(false);
			mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant);

			var mockExporter = SetUpExporter();
			mockHeader.Setup(m => m.Exporter).Returns(mockExporter);

			var mockImporter = SetUpImporter();
			mockHeader.Setup(m => m.Importer).Returns(mockImporter);

			var supDoc1 = BuilderHelperTest.SetUpDocument("N380", "ES3600000001");
			var supDoc2 = BuilderHelperTest.SetUpDocument("1230", "ES3600000002");
			mockHeader.Setup(m => m.SupportingDocuments).Returns(new[] { supDoc1, supDoc2 });

			var addRef1 = BuilderHelperTest.SetUpDocument("ref1", "reference1");
			var addRef2 = BuilderHelperTest.SetUpDocument("ref2", "reference2");
			mockHeader.Setup(m => m.AdditionalReferences).Returns(new[] { addRef1, addRef2 });

			var transpDoc1 = BuilderHelperTest.SetUpDocument("5025", "transport1");
			var transpDoc2 = BuilderHelperTest.SetUpDocument("5025", "transport2");
			mockHeader.Setup(m => m.TransportDocuments).Returns(new[] { transpDoc1, transpDoc2 });

			var addInfo1 = SetUpAdditionalInfo("A1000", "Additional Info Description 1");
			var addInfo2 = SetUpAdditionalInfo("B2000", "Additional Info Description 2");
			mockHeader.Setup(m => m.AdditionalInformation).Returns(new[] { addInfo1, addInfo2 });

			var mockTransportCostToDestination = SetUpValueCost();
			mockHeader.Setup(m => m.TransportCostToDestination).Returns(mockTransportCostToDestination);

			var preDoc1 = BuilderHelperTest.SetUpDocument("380", "1872345");
			var preDoc2 = BuilderHelperTest.SetUpDocument("001", "ES3600000005");
			mockHeader.Setup(m => m.PreviousDocuments).Returns(new[] { preDoc1, preDoc2 });

			var addFiscalRef1 = SetUpAdditionalFiscalRef("A12345678", "EXP");
			var addFiscalRef2 = SetUpAdditionalFiscalRef("ES987654321A", "IMP");
			mockHeader.Setup(m => m.AdditionalFiscalRef).Returns(new[] { addFiscalRef1, addFiscalRef2 });

			return mockHeader;
		}

		IPartyContactProvider SetUpContactInformation(ZString name, ZString email, ZString phoneNumber)
		{
			var mockContactInformation = new Mock<IPartyContactProvider>();
			mockContactInformation.Setup(m => m.Name).Returns(name);
			mockContactInformation.Setup(m => m.Email).Returns(email);
			mockContactInformation.Setup(m => m.PhoneNumber).Returns(phoneNumber);
			return mockContactInformation.Object;
		}

		IH7Representative SetUpRepresentative()
		{
			var mockRepresentative = new Mock<IH7Representative>();
			mockRepresentative.Setup(m => m.Id).Returns("DE123456789A");
			mockRepresentative.Setup(m => m.Status).Returns(2);

			var contactInfo = SetUpContactInformation("representative name ªº", "mail.representative@mail.com", "123456789");
			mockRepresentative.Setup(m => m.ContactInfo).Returns(contactInfo);

			return mockRepresentative.Object;
		}

		IH7Declarant SetUpDeclarant(ZBool isImporter)
		{
			var mockDeclarant = new Mock<IH7Declarant>();
			mockDeclarant.Setup(m => m.Id).Returns("ESA78587268");
			mockDeclarant.Setup(m => m.Name).Returns("Declarant name");
			mockDeclarant.Setup(m => m.Address).Returns("declarant address complete");
			mockDeclarant.Setup(m => m.City).Returns("MADRID");
			mockDeclarant.Setup(m => m.PostCode).Returns("28010");
			mockDeclarant.Setup(m => m.Country).Returns("ES");
			mockDeclarant.Setup(m => m.IsImporter).Returns(isImporter);

			var contactInfo = SetUpContactInformation("contact name", "mail.declarant@mail.com", "987654321");
			mockDeclarant.Setup(m => m.ContactInfo).Returns(contactInfo);

			return mockDeclarant.Object;
		}

		IPartyProvider SetUpExporter()
		{
			var mockExporter = new Mock<IPartyProvider>();
			mockExporter.Setup(m => m.Id).Returns("12345678A");
			mockExporter.Setup(m => m.Name).Returns("NORON EHF");
			mockExporter.Setup(m => m.Address).Returns("SKUTUVOGUR 7");
			mockExporter.Setup(m => m.City).Returns("104 REYKJAVIK");
			mockExporter.Setup(m => m.PostCode).Returns("40025");
			mockExporter.Setup(m => m.Country).Returns("IS");

			return mockExporter.Object;
		}

		IH7Importer SetUpImporter()
		{
			var mockImporter = new Mock<IH7Importer>();
			mockImporter.Setup(m => m.Id).Returns("ESA12354678");
			mockImporter.Setup(m => m.Name).Returns("Importer name");
			mockImporter.Setup(m => m.Address).Returns("Importer address complete");
			mockImporter.Setup(m => m.City).Returns("BARCELONA");
			mockImporter.Setup(m => m.PostCode).Returns("10020");
			mockImporter.Setup(m => m.Country).Returns("ES");
			mockImporter.Setup(m => m.IsParticular).Returns(true);

			var contactInfo = SetUpContactInformation("Importer contact name", "mail.importer@mail.com", "321654987");
			mockImporter.Setup(m => m.ContactInfo).Returns(contactInfo);
			return mockImporter.Object;
		}

		IH7AdditionalInfo SetUpAdditionalInfo(ZString code, ZString description)
		{
			var mockAdditionalInfo = new Mock<IH7AdditionalInfo>();
			mockAdditionalInfo.Setup(m => m.Code).Returns(code);
			mockAdditionalInfo.Setup(m => m.Description).Returns(description);
			return mockAdditionalInfo.Object;
		}

		IH7Value SetUpValueCost()
		{
			var mockValueCost = new Mock<IH7Value>();
			mockValueCost.Setup(m => m.Amount).Returns(215.3M);
			mockValueCost.Setup(m => m.CurrencyCode).Returns("EUR");
			return mockValueCost.Object;
		}

		IH7AdditionalFiscalRef SetUpAdditionalFiscalRef(ZString id, ZString role)
		{
			var mockAdditionalFiscalRef = new Mock<IH7AdditionalFiscalRef>();
			mockAdditionalFiscalRef.Setup(m => m.Id).Returns(id);
			mockAdditionalFiscalRef.Setup(m => m.Role).Returns(role);
			return mockAdditionalFiscalRef.Object;
		}

		IDeclarationH7Line SetUpLine(ZInt lineNumber)
		{
			var mockLine = new Mock<IDeclarationH7Line>();
			mockLine.Setup(m => m.LineNumber).Returns(lineNumber);
			mockLine.Setup(m => m.GoodsDescription).Returns("goods description");
			mockLine.Setup(m => m.CommodityCode).Returns("73181100");
			mockLine.Setup(m => m.GrossWeight).Returns(500.66);
			mockLine.Setup(m => m.ComplementaryUnitsQty).Returns(66.88);
			mockLine.Setup(m => m.NumberOfPackages).Returns(450);

			var mockValue = SetUpValueCost();
			mockLine.Setup(m => m.Value).Returns(mockValue);

			return mockLine.Object;
		}
	}
}
