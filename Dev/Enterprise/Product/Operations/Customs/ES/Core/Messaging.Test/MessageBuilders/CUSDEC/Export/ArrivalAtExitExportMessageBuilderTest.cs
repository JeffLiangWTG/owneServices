using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ArrivalAtExitExportMessageBuilder))]
	class ArrivalAtExitExportMessageBuilderTest : EDIFACTMessageBuilderTest<ArrivalAtExitExportMessageBuilder, IArrivalAtExitExportMessageDataProvider, CUSDECMessage>
	{
		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ArrivalAtExit;
		protected override ZString DeclarantIdForUNBSegment => "1210244B";

		protected override ZString GetTestFile() => GetTestFileContents(ExportTestFileConstants.TestFilePath, "TestArrivalAtExitExportMessage.txt");

		protected override ArrivalAtExitExportMessageBuilder CreateMessageBuilder() => new ArrivalAtExitExportMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType);
		protected override ArrivalAtExitExportMessageBuilder CreateMessageBuilderWithNullProvider() => new ArrivalAtExitExportMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

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

		[TestDate(2020, 3, 9, 16, 13, 23, 456)]
		public override void TestUNBNotTest()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns("1210244B");
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("UNB+UNOA:1+1210244B:ZZ+AEATADUE:ZZ+200309:1613+<<MSGNO PLACEHOLDER>>++&EE'", messageText);
		}

		protected Mock<IArrivalAtExitExportMessageDataProvider> mockProvider;

		protected void SetUpMockProvider()
		{
			mockProvider = new Mock<IArrivalAtExitExportMessageDataProvider>();

			mockProvider.Setup(m => m.Factory).Returns(Factory);
			mockProvider.Setup(m => m.IsTest).Returns(ZBool.True);
			mockProvider.Setup(m => m.Messages).Returns(new EDIMessageCollection(Factory.New(typeof(DummyBusinessObject)), Factory));
			mockProvider.Setup(m => m.BrokerCode).Returns("AZ");
			mockProvider.Setup(m => m.CertificateName).Returns("CertName");
			mockProvider.Setup(m => m.CertificateThumbPrint).Returns("CertThumbPrint");
			mockProvider.Setup(m => m.CertificateBytes).Returns(BuilderHelperTest.GetCertificateBytes());
			mockProvider.Setup(m => m.DecryptedCertificatePassphrase).Returns(BuilderHelperTest.CertificatePassword);
			mockProvider.Setup(m => m.BusinessObjectReference).Returns("Reference");

			var certificate = Factory.New<MasterFiles.Business.GlbExternalPassword>();
			mockProvider.Setup(m => m.CertificatePK).Returns(certificate.PK);
		}

		protected override void SetUp()
		{
			SetUpMockProvider();
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns(DeclarantIdForUNBSegment);
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns("1210244B");
			mockProvider.Setup(m => m.LocalReferenceNumber).Returns("1234123444");
			mockProvider.Setup(m => m.CustomsProcedureCategory5).Returns("11ES00280110000101");
			mockProvider.Setup(m => m.CustomsOfficeofExitCountryCode).Returns("ES");
			mockProvider.Setup(m => m.CustomsOfficeofExit).Returns("000801");
			mockProvider.Setup(m => m.LocationOfGoodsExamCustomsOffice).Returns("0811");
			mockProvider.Setup(m => m.LocationOfGoodsExam).Returns("BCN010");
			mockProvider.Setup(m => m.DateOfArrival).Returns(new ZDateTime(2019, 08, 15));

			var mockDeclarant = SetUpDeclarant();
			mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant.Object);
		}

		Mock<IExportDeclarantPartyIdProvider> SetUpDeclarant()
		{
			var mockDeclarant = new Mock<IExportDeclarantPartyIdProvider>();
			mockDeclarant.Setup(m => m.PartyQualifier).Returns("1");
			mockDeclarant.Setup(m => m.Id).Returns("1210244B");
			mockDeclarant.Setup(m => m.Name).Returns("GUTIERREZ S.A.");
			mockDeclarant.Setup(m => m.NameCode).Returns(ZString.Empty);
			mockDeclarant.Setup(m => m.EmailAddress).Returns("MIDIRECCION.CORREO.EN.CASTILLAYLEON@MIXMAIL.COM");
			return mockDeclarant;
		}
	}
}
