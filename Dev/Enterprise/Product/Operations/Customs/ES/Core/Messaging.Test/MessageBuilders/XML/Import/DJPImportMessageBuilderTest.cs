using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.DocumentosSimplifiV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(DJPImportMessageBuilder))]
	class DJPImportMessageBuilderTest : ImportAbstractMessageBuilderTest<DJPImportMessageBuilder, IDJPImportMessageDataProvider, DocumentosSimplifiV1Ent>
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

		public override void TestPopulateSegmentosDeServicioIsTestFalse()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("Test=", messageText);
		}

		public void TestPopulateGlobalDocuments()
		{
			mockProvider.Setup(m => m.Documents).Returns((IReadOnlyCollection<IDJPDocument>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateGlobalDocument()
		{
			mockProvider.Setup(m => m.Documents).Returns(new IDJPDocument[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateDeclarationDocuments()
		{
			mockDeclaration1.Setup(m => m.Documents).Returns((IReadOnlyCollection<IDJPDocument>)null);
			mockProvider.Setup(m => m.Declarations).Returns(new[] { mockDeclaration1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateDeclarationDocument()
		{
			mockDeclaration1.Setup(m => m.Documents).Returns(new IDJPDocument[] { null });
			mockProvider.Setup(m => m.Declarations).Returns(new[] { mockDeclaration1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulatePartidas()
		{
			mockDeclaration1.Setup(m => m.Lines).Returns((IReadOnlyCollection<IDJPLine>)null);
			mockProvider.Setup(m => m.Declarations).Returns(new[] { mockDeclaration1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLine()
		{
			mockDeclaration1.Setup(m => m.Lines).Returns(new IDJPLine[] { null });
			mockProvider.Setup(m => m.Declarations).Returns(new[] { mockDeclaration1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.PendingSupportingDocuments;

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.ImportTestFilePath, "TestDJPImportMessage.txt");

		protected override DJPImportMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new DJPImportMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override DJPImportMessageBuilder CreateMessageBuilderWithNullProvider() => new DJPImportMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();

			var mockGlobalDoc1 = SetUpDocument("1407", "ref", new ZDateTime(2020, 03, 12), ZString.Empty);
			var mockGlobalDoc2 = SetUpDocument("1234", "ref2", ZDateTime.Empty, "A");

			mockProvider.Setup(m => m.Documents).Returns(new[] { mockGlobalDoc1, mockGlobalDoc2 });

			var mockLineDoc1 = SetUpDocument("1001", "refLine1", new ZDateTime(2020, 07, 09), ZString.Empty);
			var mockLineDoc2 = SetUpDocument("1002", "refLine2", ZDateTime.Empty, "A");
			var mockLineDoc3 = SetUpDocument("1003", "refLine3", new ZDateTime(2020, 12, 25), "N");
			var mockLineDoc4 = SetUpDocument("1004", "refLine4", ZDateTime.Empty, "R");
			var mockLineDoc5 = SetUpDocument("1005", "refLine5", new ZDateTime(2020, 07, 09), ZString.Empty);
			var mockLineDoc6 = SetUpDocument("1006", "refLine6", ZDateTime.Empty, "A");
			var mockLineDoc7 = SetUpDocument("1007", "refLine7", new ZDateTime(2020, 12, 25), "N");
			var mockLineDoc8 = SetUpDocument("1008", "refLine8", ZDateTime.Empty, "R");

			mockLine1 = SetUpLine(1, new IDJPDocument[] { mockLineDoc1 });
			var mockLine2 = SetUpLine(2, new IDJPDocument[] { mockLineDoc2, mockLineDoc3, mockLineDoc4 });
			var mockLine3 = SetUpLine(1, new IDJPDocument[] { mockLineDoc5, mockLineDoc6 });
			var mockLine4 = SetUpLine(2, new IDJPDocument[] { mockLineDoc7, mockLineDoc8 });

			var mockDeclDoc1 = SetUpDocument("5001", "refDecl1", new ZDateTime(2021, 04, 12), ZString.Empty);
			var mockDeclDoc2 = SetUpDocument("5002", "refDecl2", ZDateTime.Empty, "A");
			var mockDeclDoc3 = SetUpDocument("5003", "refDecl3", new ZDateTime(2021, 03, 31), "N");
			var mockDeclDoc4 = SetUpDocument("5004", "refDecl4", ZDateTime.Empty, "R");

			mockDeclaration1 = SetUpDeclaration("99989036AZM0000101", new IDJPDocument[] { mockDeclDoc1 }, new IDJPLine[] { mockLine1.Object, mockLine2.Object });
			var mockDeclaration2 = SetUpDeclaration("99989036AZM0000102", new IDJPDocument[] { mockDeclDoc2, mockDeclDoc3, mockDeclDoc4 }, new IDJPLine[] { mockLine3.Object, mockLine4.Object });

			mockProvider.Setup(m => m.Declarations).Returns(new[] { mockDeclaration1.Object, mockDeclaration2.Object });
		}

		Mock<IDJPDeclaration> mockDeclaration1;
		Mock<IDJPLine> mockLine1;

		IDJPDocument SetUpDocument(ZString docType, ZString docRef, ZDateTime docDate, ZString docIndicator)
		{
			var mockDocument = new Mock<IDJPDocument>();
			mockDocument.Setup(m => m.Name).Returns(docType);
			mockDocument.Setup(m => m.Number).Returns(docRef);
			mockDocument.Setup(m => m.Date).Returns(docDate);
			mockDocument.Setup(m => m.Indicator).Returns(docIndicator);
			return mockDocument.Object;
		}

		Mock<IDJPDeclaration> SetUpDeclaration(ZString mrn, IEnumerable<IDJPDocument> documents, IEnumerable<IDJPLine> lines)
		{
			var mockDeclaration = new Mock<IDJPDeclaration>();
			mockDeclaration.Setup(m => m.MRN).Returns(mrn);
			mockDeclaration.Setup(m => m.Documents).Returns((IReadOnlyCollection<IDJPDocument>)documents);
			mockDeclaration.Setup(m => m.Lines).Returns((IReadOnlyCollection<IDJPLine>)lines);
			return mockDeclaration;
		}

		Mock<IDJPLine> SetUpLine(ZInt lineNumber, IEnumerable<IDJPDocument> documents)
		{
			var mockLine = new Mock<IDJPLine>();
			mockLine.Setup(m => m.LineNumber).Returns(lineNumber);
			mockLine.Setup(m => m.Documents).Returns((IReadOnlyCollection<IDJPDocument>)documents);
			return mockLine;
		}
	}
}
