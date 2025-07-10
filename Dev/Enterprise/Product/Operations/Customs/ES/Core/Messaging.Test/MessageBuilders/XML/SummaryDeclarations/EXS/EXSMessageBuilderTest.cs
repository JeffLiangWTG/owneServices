using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE615V4Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(EXSMessageBuilder))]
	public class EXSMessageBuilderTest : SummaryDeclarationsCommonMessageBuilderTest<EXSMessageBuilder, IEXSMessageDataProvider, Cc615A>
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

		public void TestPopulateHeader()
		{
			mockProvider.Setup(m => m.Header).Returns((IEXSHeader)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestTransportDocument()
		{
			mockProvider.Setup(m => m.TransportDocument).Returns((IDocumentsCommon)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateConsignor()
		{
			mockProvider.Setup(m => m.Consignor).Returns((IPartyProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateConsignee()
		{
			mockProvider.Setup(m => m.Consignee).Returns((IPartyProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateAdditionalActors()
		{
			mockProvider.Setup(m => m.AdditionalActors).Returns((IReadOnlyCollection<IEXSAdditionalActor>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("ASCA1", messageText);
				AssertNotContains("RoleASCA11", messageText);
				AssertNotContains("TINASCA12", messageText);
			});
		}

		public void TestPopulateAdditionalActor()
		{
			mockProvider.Setup(m => m.AdditionalActors).Returns(new IEXSAdditionalActor[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("RoleASCA11", messageText);
				AssertNotContains("TINASCA12", messageText);
			});
		}

		public void TestIsTest()
		{
			CombineAssertions(() =>
			{
				mockProvider.Setup(m => m.IsTest).Returns(ZBool.True);
				var messageText = CreateMessageBuilder().GetSignedMessageText();
				AssertNotContains("TesIndMES18", messageText);

				mockProvider.Setup(m => m.IsTest).Returns(ZBool.False);
				messageText = CreateMessageBuilder().GetSignedMessageText();
				AssertNotContains("TesIndMES18", messageText);
			});
		}

		public void TestPopulateAdditionalInfoList()
		{
			mockProvider.Setup(m => m.AdditionalInfo).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("ADDINF1", messageText);
				AssertNotContains("CodeADDINF11", messageText);
				AssertNotContains("TextADDINF12", messageText);
			});
		}

		public void TestPopulateAdditionalInfo()
		{
			mockProvider.Setup(m => m.AdditionalInfo).Returns(new IDocumentsCommon[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("CodeADDINF11", messageText);
				AssertNotContains("TextADDINF12", messageText);
			});
		}

		public void TestPopulateGoodsItems()
		{
			mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IEXSLine>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLine()
		{
			mockProvider.Setup(m => m.Lines).Returns(new IEXSLine[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateCertificates()
		{
			mockLine1.Setup(m => m.Certificates).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateCertificate()
		{
			mockLine1.Setup(m => m.Certificates).Returns(new IDocumentsCommon[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulatePreviousDocument()
		{
			mockLine1.Setup(m => m.PreviousDocument).Returns((IEXSDocument)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("PREDOCGODITM1", messageText);
				AssertNotContains("DocTypPD11", messageText);
				AssertNotContains("DocRefPD12", messageText);
				AssertNotContains("DocGdsIteNumPD13", messageText);
			});
		}

		public void TestPopulateLineConsignor()
		{
			mockLine1.Setup(m => m.Consignor).Returns((IPartyProvider)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateCommodityCode()
		{
			mockLine1.Setup(m => m.CommodityCode).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("COMCODGODITM", messageText);
				AssertNotContains("ComNomCMD1", messageText);
			});
		}

		public void TestPopulateLineConsignee()
		{
			mockLine1.Setup(m => m.Consignee).Returns((IPartyProvider)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateContainers()
		{
			mockLine1.Setup(m => m.Containers).Returns((IReadOnlyCollection<ZString>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("CONNR2", messageText);
				AssertNotContains("ConNumNR21", messageText);
			});
		}

		public void TestPopulateContainer()
		{
			mockLine1.Setup(m => m.Containers).Returns((IReadOnlyCollection<ZString>)Enumerable.Empty<ZString>());
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("ConNumNR21", messageText);
		}

		public void TestPopulatePackages()
		{
			mockLine1.Setup(m => m.Packages).Returns((IReadOnlyCollection<IEXSPackage>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulatePackage()
		{
			mockLine1.Setup(m => m.Packages).Returns(new IEXSPackage[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLineAdditionalActors()
		{
			mockLine1.Setup(m => m.AdditionalActors).Returns((IReadOnlyCollection<IEXSAdditionalActor>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("ASCA2", messageText);
				AssertNotContains("RoleASCA21", messageText);
				AssertNotContains("TINASCA22", messageText);
			});
		}

		public void TestPopulateLineAdditionalActor()
		{
			mockLine1.Setup(m => m.AdditionalActors).Returns(new IEXSAdditionalActor[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("RoleASCA21", messageText);
				AssertNotContains("TINASCA22", messageText);
			});
		}

		public void TestPopulateLineAdditionalInfoList()
		{
			mockLine1.Setup(m => m.AdditionalInfo).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("ADDINF2", messageText);
				AssertNotContains("CodeADDINF21", messageText);
				AssertNotContains("TextADDINF22", messageText);
			});
		}

		public void TestPopulateLineAdditionalInfo()
		{
			mockLine1.Setup(m => m.AdditionalInfo).Returns(new IDocumentsCommon[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("CodeADDINF21", messageText);
				AssertNotContains("TextADDINF22", messageText);
			});
		}

		public void TestPopulateItineraryCountries()
		{
			mockProvider.Setup(m => m.ItineraryCountries).Returns((IReadOnlyCollection<ZString>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("ITI", messageText);
				AssertNotContains("CouOfRouCodITI1", messageText);
			});
		}

		public void TestPopulateCustomsOffice()
		{
			mockProvider.Setup(m => m.CustomsOffice).Returns(ZString.Empty);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("CUSOFFLON", messageText);
				AssertNotContains("RefNumCOL1", messageText);
			});
		}

		public void TestPopulateLodgingPerson()
		{
			mockProvider.Setup(m => m.LodgingPerson).Returns((IEXSPartyProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateRepresentative()
		{
			mockProvider.Setup(m => m.Representative).Returns((IEXSRepresentative)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateCarrier()
		{
			mockProvider.Setup(m => m.Carrier).Returns((IEXSContactPersonWithId)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateSeals()
		{
			mockProvider.Setup(m => m.Seals).Returns((IReadOnlyCollection<ZString>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("SEAID529", messageText);
				AssertNotContains("SeaIdSEAID530", messageText);
			});
		}

		public void TestPopulateSeal()
		{
			mockProvider.Setup(m => m.Seals).Returns((IReadOnlyCollection<ZString>)Enumerable.Empty<ZString>());
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("SeaIdSEAID530", messageText);
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ExitSummaryDeclaration;
		protected override ZString GetSignedMessageTestFileContent() => GetTestFile();
		protected override ZString GetUnsignedMessageTestFileContent() => GetTestFile();
		ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.EXSTestFilePath, "TestEXSMessage.txt");

		protected override EXSMessageBuilder CreateMessageBuilder() => new EXSMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType);
		protected override EXSMessageBuilder CreateMessageBuilderWithNullProvider() => new EXSMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider.Setup(m => m.SenderId).Returns("89890001K");

			mockProvider.Setup(m => m.ItineraryCountries).Returns(new ZString[] { "ES", "FR", "GB" });
			mockProvider.Setup(m => m.CustomsOffice).Returns("ES004611");
			mockProvider.Setup(m => m.Seals).Returns(new ZString[] { "00000001", "00000002", "00000003" });

			var mockHeader = SetUpHeader();
			mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);

			var mockTransportDocument = BuilderHelperTest.SetUpDocument("AAA", "Transport");
			mockProvider.Setup(m => m.TransportDocument).Returns(mockTransportDocument);

			var mockConsignor = BuilderHelperTest.SetUpParty("89890001K", "Lobo Lobate", "Lobera de Valdelacierva", "Por poco en Vinuelas", "19069", "ES");
			mockProvider.Setup(m => m.Consignor).Returns(mockConsignor);

			var mockConsignee = BuilderHelperTest.SetUpParty("89890001K", "Ave", "Acostadero eras de abajo", "Fuentelahigera de Albatages", "19024", "ES");
			mockProvider.Setup(m => m.Consignee).Returns(mockConsignee);

			var mockLodgingPerson = SetUpLodgingPerson("ESA78587268", "importer name ªº", "importer address complete", "MADRID", "12345", "ES", "mail@mail.com");
			mockProvider.Setup(m => m.LodgingPerson).Returns(mockLodgingPerson.Object);

			var mockAdditionalActor1 = SetUpAdditionalActor("role1", "id1");
			var mockAdditionalActor2 = SetUpAdditionalActor("role2", "id2");
			mockProvider.Setup(m => m.AdditionalActors).Returns(new IEXSAdditionalActor[] { mockAdditionalActor1.Object, mockAdditionalActor2.Object });

			var mockAdditionalInfo1 = SetUpAdditionalInfo("code1", "text1");
			var mockAdditionalInfo2 = SetUpAdditionalInfo("code2", "text2");
			mockProvider.Setup(m => m.AdditionalInfo).Returns(new IDocumentsCommon[] { mockAdditionalInfo1.Object, mockAdditionalInfo2.Object });

			var mockRepresentative = SetUpRepresentative();
			mockProvider.Setup(m => m.Representative).Returns(mockRepresentative.Object);

			var mockCarrier = SetUpCarrier();
			mockProvider.Setup(m => m.Carrier).Returns(mockCarrier.Object);

			var mockCertificate1 = BuilderHelperTest.SetUpDocument("N703", "ESAEOC1");
			var mockCertificate2 = BuilderHelperTest.SetUpDocument("N706", "ESAEOC2");

			var mockPackage1 = SetUpPackage(4, "BX", false);
			var mockPackage2 = SetUpPackage(0, "BX", false);
			var mockPackage3 = SetUpPackage(0, "VO", true);

			var mockPreviousDocument1 = SetUpDocument("N704", "ESAAAAA", "1");

			var mockAdditionalActor3 = SetUpAdditionalActor("role3", "id3");
			var mockAdditionalActor4 = SetUpAdditionalActor("role4", "id4");

			var mockAdditionalInfo3 = SetUpAdditionalInfo("code3", "text3");
			var mockAdditionalInfo4 = SetUpAdditionalInfo("code4", "text4");

			mockLine1 = SetUpLine(1, new IDocumentsCommon[] { mockCertificate1, mockCertificate2 }, new ZString[] { "Container1", "Container2" }, Enumerable.Empty<IEXSPackage>(), mockPreviousDocument1, Enumerable.Empty<IEXSAdditionalActor>(), new IDocumentsCommon[] { mockAdditionalInfo3.Object, mockAdditionalInfo4.Object });
			var mockLine2 = SetUpLine(2, Enumerable.Empty<IDocumentsCommon>(), Enumerable.Empty<ZString>(), new IEXSPackage[] { mockPackage1, mockPackage2, mockPackage3 }, null, new IEXSAdditionalActor[] { mockAdditionalActor3.Object, mockAdditionalActor4.Object }, Enumerable.Empty<IDocumentsCommon>());
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}

		protected Mock<IEXSLine> mockLine1;

		Mock<IEXSHeader> SetUpHeader()
		{
			var mockHeader = new Mock<IEXSHeader>();
			mockHeader.Setup(m => m.ReferenceNumber).Returns("LRN000000041");
			mockHeader.Setup(m => m.GoodsLocation).Returns("4611ZZZ999");
			mockHeader.Setup(m => m.TotalLinesNum).Returns(2);
			mockHeader.Setup(m => m.TotalPackagesQty).Returns(21);
			mockHeader.Setup(m => m.TotalGrossWeight).Returns(150.0M);
			mockHeader.Setup(m => m.DeclarationDate).Returns(new ZDateTime(2020, 03, 12));
			mockHeader.Setup(m => m.DeclarationPlace).Returns("Madrid");
			mockHeader.Setup(m => m.SpecificCircumstanceInd).Returns("A");
			mockHeader.Setup(m => m.NatSpecificCircumstanceInd).Returns("B");
			mockHeader.Setup(m => m.DocumentOperationIndicator).Returns("M");
			mockHeader.Setup(m => m.DocumentReferenceNumber).Returns("99989036AZM0000101");
			mockHeader.Setup(m => m.MethodOfPayment).Returns("A");
			return mockHeader;
		}

		Mock<IEXSPartyProvider> SetUpLodgingPerson(ZString id, ZString name, ZString address, ZString city, ZString postCode, ZString country, ZString email)
		{
			var addressInformation = new Mock<IEXSPartyProvider>();
			addressInformation.Setup(m => m.Id).Returns(id);
			addressInformation.Setup(m => m.Name).Returns(name);
			addressInformation.Setup(m => m.Address).Returns(address);
			addressInformation.Setup(m => m.City).Returns(city);
			addressInformation.Setup(m => m.PostCode).Returns(postCode);
			addressInformation.Setup(m => m.Country).Returns(country);
			addressInformation.Setup(m => m.EmailAddress).Returns(email);
			return addressInformation;
		}

		Mock<IEXSAdditionalActor> SetUpAdditionalActor(ZString role, ZString id)
		{
			var mockActor = new Mock<IEXSAdditionalActor>();
			mockActor.Setup(m => m.Role).Returns(role);
			mockActor.Setup(m => m.Id).Returns(id);
			return mockActor;
		}

		Mock<IDocumentsCommon> SetUpAdditionalInfo(ZString code, ZString text)
		{
			var mockActor = new Mock<IDocumentsCommon>();
			mockActor.Setup(m => m.Name).Returns(code);
			mockActor.Setup(m => m.Number).Returns(text);
			return mockActor;
		}

		Mock<IEXSRepresentative> SetUpRepresentative()
		{
			var mockRepresentative = new Mock<IEXSRepresentative>();
			mockRepresentative.Setup(m => m.Id).Returns("ES78945612");
			mockRepresentative.Setup(m => m.Name).Returns("representative");
			mockRepresentative.Setup(m => m.Phone).Returns("123456789");
			mockRepresentative.Setup(m => m.EmailAddress).Returns("representative@mail.com");
			mockRepresentative.Setup(m => m.DirectRepresentation).Returns("2");
			return mockRepresentative;
		}

		Mock<IEXSContactPersonWithId> SetUpCarrier()
		{
			var mockCarrier = new Mock<IEXSContactPersonWithId>();
			mockCarrier.Setup(m => m.Id).Returns("ES98765432");
			mockCarrier.Setup(m => m.Name).Returns("carrier");
			mockCarrier.Setup(m => m.Phone).Returns("789456123");
			mockCarrier.Setup(m => m.EmailAddress).Returns("carrier@mail.com");
			return mockCarrier;
		}

		IEXSPackage SetUpPackage(ZInt packagesQty, ZString packageType, ZBool isPackTypeBulk)
		{
			var mockPackage = new Mock<IEXSPackage>();
			mockPackage.Setup(m => m.PackageType).Returns(packageType);
			mockPackage.Setup(m => m.Marks).Returns("Cubo entero 001");
			mockPackage.Setup(m => m.PackagesQty).Returns(packagesQty);
			mockPackage.Setup(m => m.IsPackTypeBulk).Returns(isPackTypeBulk);
			return mockPackage.Object;
		}

		Mock<IEXSLine> SetUpLine(ZInt lineNumber, IEnumerable<IDocumentsCommon> certificates, IEnumerable<ZString> containers, IEnumerable<IEXSPackage> packages, IEXSDocument previoudDocs, IEnumerable<IEXSAdditionalActor> additionalActors, IEnumerable<IDocumentsCommon> additionalInfo)
		{
			var mockLine = new Mock<IEXSLine>();
			mockLine.Setup(m => m.LineNumber).Returns(lineNumber);
			mockLine.Setup(m => m.GoodsDescription).Returns("Huevos revueltos");
			mockLine.Setup(m => m.GrossWeight).Returns(4.0M);
			mockLine.Setup(m => m.MethodOfPayment).Returns("A");
			mockLine.Setup(m => m.UNDangerousCode).Returns("1");
			mockLine.Setup(m => m.ReferenceNumber).Returns("Reference");
			mockLine.Setup(m => m.Certificates).Returns((IReadOnlyCollection<IDocumentsCommon>)certificates);
			mockLine.Setup(m => m.PreviousDocument).Returns(previoudDocs);
			mockLine.Setup(m => m.CommodityCode).Returns("123456");
			mockLine.Setup(m => m.Containers).Returns((IReadOnlyCollection<ZString>)containers);
			mockLine.Setup(m => m.Packages).Returns((IReadOnlyCollection<IEXSPackage>)packages);
			mockLine.Setup(m => m.CusCode).Returns("CusCode");
			mockLine.Setup(m => m.AdditionalActors).Returns((IReadOnlyCollection<IEXSAdditionalActor>)additionalActors);
			mockLine.Setup(m => m.AdditionalInfo).Returns((IReadOnlyCollection<IDocumentsCommon>)additionalInfo);

			var mockConsignor = BuilderHelperTest.SetUpParty("89890001K", "Lobo Lobate", "Lobera de Valdelacierva", "Por poco en Vinuelas", "19069", "ES");
			mockLine.Setup(m => m.Consignor).Returns(mockConsignor);

			var mockConsignee = BuilderHelperTest.SetUpParty("89890001K", "Ave", "Acostadero eras de abajo", "Fuentelahigera de Albatages", "19024", "ES");
			mockLine.Setup(m => m.Consignee).Returns(mockConsignee);

			return mockLine;
		}

		IEXSDocument SetUpDocument(ZString name, ZString number, ZString lineNumber)
		{
			var mockDoc = new Mock<IEXSDocument>();
			mockDoc.Setup(m => m.Name).Returns(name);
			mockDoc.Setup(m => m.Number).Returns(number);
			mockDoc.Setup(m => m.LineNumber).Returns(lineNumber);
			return mockDoc.Object;
		}
	}
}
