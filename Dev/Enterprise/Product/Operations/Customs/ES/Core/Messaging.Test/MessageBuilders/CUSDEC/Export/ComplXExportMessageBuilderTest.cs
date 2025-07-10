using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ComplXExportMessageBuilder))]
	public class ComplXExportMessageBuilderTest : EDIFACTMessageBuilderTest<ComplXExportMessageBuilder, IComplXExportMessageDataProvider, CUSDECMessage>
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

		public void TestDateOfIssue()
		{
			var document = SetUpDocument(new ZDateTime(2020, 7, 16), ZDateTime.Empty);
			mockLine1.Setup(m => m.Documents).Returns(new[] { document });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("DTM+137:200716:101'", messageText);
		}

		[TestDate(2020, 3, 9, 16, 13, 23, 456)]
		public override void TestUNBNotTest()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns("1210244B");
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("UNB+UNOA:1+1210244B:ZZ+AEATADUE:ZZ+200309:1613+<<MSGNO PLACEHOLDER>>++&EE'", messageText);
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.TypeXExport;
		protected override ZString DeclarantIdForUNBSegment => "1210244B";

		protected override ZString GetTestFile() => GetTestFileContents(ExportTestFileConstants.TestFilePath, "TestComplXExportMessage.txt");

		protected override ComplXExportMessageBuilder CreateMessageBuilder() => new ComplXExportMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType);
		protected override ComplXExportMessageBuilder CreateMessageBuilderWithNullProvider() => new ComplXExportMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected Mock<IComplXExportMessageDataProvider> mockProvider;

		protected void SetUpMockProvider()
		{
			mockProvider = new Mock<IComplXExportMessageDataProvider>();

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
			mockProvider.Setup(m => m.LocalReferenceNumber).Returns("1234123444");
			mockProvider.Setup(m => m.MessageType).Returns("36");
			mockProvider.Setup(m => m.CustomsProcedureCategory5).Returns("2801");
			mockProvider.Setup(m => m.TermsOfDeliveryCode).Returns("CIF");
			mockProvider.Setup(m => m.DeliveryLocation).Returns(ZString.Empty);
			mockProvider.Setup(m => m.TotalAmount).Returns(14987);
			mockProvider.Setup(m => m.TotalAmountCurrencyCode).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			mockProvider.Setup(m => m.TotalNumberOfGoods).Returns(15);

			var mockDeclarant = BuilderHelperTest.SetUpExportDeclarantPartyId(ZString.Empty, "MIDIRECCION.CORREO.EN.CASTILLAYLEON@MIXMAIL.COM");
			mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant);

			var dateOfExpiry = new ZDateTime(2019, 08, 15);
			var document1 = SetUpDocument(ZDateTime.Empty, dateOfExpiry);
			var document2 = SetUpDocument(ZDateTime.Empty, dateOfExpiry);

			mockLine1 = SetUpLine(1, new[] { document1, document2 });
			var mockLine2 = SetUpLine(2, Enumerable.Empty<IExportDocumentCommon>());

			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}
		Mock<IComplXExportLine> mockLine1;

		Mock<IComplXExportLine> SetUpLine(ZInt goodsItemNumber, IEnumerable<IExportDocumentCommon> documents)
		{
			var mockLine = new Mock<IComplXExportLine>();
			mockLine.Setup(m => m.GrossWeightInKG).Returns(0.100);
			mockLine.Setup(m => m.NetWeightInKG).Returns(123);
			mockLine.Setup(m => m.OtherUnitsNumber).Returns(2500);
			mockLine.Setup(m => m.OtherUnitsQualifier).Returns("UN");
			mockLine.Setup(m => m.TotalGoodValueInEuros).Returns(8527.45);
			mockLine.Setup(m => m.GoodsItemNumber).Returns(goodsItemNumber);
			mockLine.Setup(m => m.Documents).Returns((IReadOnlyCollection<IExportDocumentCommon>)documents);
			return mockLine;
		}

		IExportDocumentCommon SetUpDocument(ZDateTime dateOfIssue, ZDateTime dateOfExpiry)
		{
			var mockDocument = new Mock<IExportDocumentCommon>();
			mockDocument.Setup(m => m.Name).Returns("X001");
			mockDocument.Setup(m => m.Number).Returns("ES3600000001");
			mockDocument.Setup(m => m.DateOfIssue).Returns(dateOfIssue);
			mockDocument.Setup(m => m.DateOfExpiry).Returns(dateOfExpiry);
			return mockDocument.Object;
		}
	}
}
