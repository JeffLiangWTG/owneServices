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
	[TestedType(typeof(DUAExportMessageBuilder))]
	class DUAExportMessageBuilderTest : EDIFACTMessageBuilderTest<DUAExportMessageBuilder, IDUAExportMessageDataProvider, CUSDECMessage>
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

		public void TestPopulateCSTSegment_NoCustomsProcedureCategory3()
		{
			mockProvider.Setup(m => m.CustomsProcedureCategory3).Returns(ZString.Empty);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST++EX:104:141+A:105:141++34:112:141+2801:113:148'", messageText);
		}

		public void TestAddNewLOCSegment_NoWarehouse()
		{
			mockProvider.Setup(m => m.Warehouse).Returns(ZString.Empty);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("LOC+18", messageText);
		}

		public void TestAddNewRFFInSG1Group_NoReferenceNumber()
		{
			mockProvider.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("RFF+ABJ", messageText);
		}

		public void TestAddNewRFFInSG1Group_NoSpecificCircumstancesIndicator()
		{
			mockProvider.Setup(m => m.SpecificCircumstancesIndicator).Returns(ZString.Empty);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("RFF+AJK", messageText);
		}

		public void TestPopulateSG4Groups_NoBorderTransportMode()
		{
			mockProvider.Setup(m => m.BorderTransportMode).Returns((ITransportMediumInfoCommon)null);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("TDT+11++4'TPL+:::M-1234-GD:ES'", messageText);
		}

		public void TestAddNewSG4Group_NoTransportModeOrName()
		{
			mockBorderTransportMode.Setup(m => m.TransportMode).Returns(ZString.Empty);
			mockBorderTransportMode.Setup(m => m.TransportId).Returns(ZString.Empty);
			mockProvider.Setup(m => m.BorderTransportMode).Returns(mockBorderTransportMode.Object);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("TDT+11++4'TPL+:::M-1234-GD:ES'", messageText);
		}

		public void TestAddNewSG4Group_NoTransportMode()
		{
			mockBorderTransportMode.Setup(m => m.TransportMode).Returns(ZString.Empty);
			mockProvider.Setup(m => m.BorderTransportMode).Returns(mockBorderTransportMode.Object);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("TDT+11++:M-1234-GD'", messageText);
		}

		public void TestAddNewSG4Group_NoTransportName()
		{
			mockBorderTransportMode.Setup(m => m.TransportId).Returns(ZString.Empty);
			mockProvider.Setup(m => m.BorderTransportMode).Returns(mockBorderTransportMode.Object);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("TDT+11++4'TPL+::::ES'", messageText);
		}

		public void TestAddNewNADWithAddressInSG6Group_NoAddressDetails()
		{
			mockProvider.Setup(m => m.Exporter).Returns((IDUAExportPartyProvider)null);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("NAD+EX", messageText);
		}

		public void TestAddNewNADWithAddressInSG6Group_NoOrganizationCodeQualifier()
		{
			mockExporter.Setup(m => m.OrganizationCodeQualifier).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Exporter).Returns(mockExporter.Object);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("NAD+EX+A01010101::148++EXPORTADORA ESPAÑOLA S.A.+BASOA 6+VITORIA++01012+ES'", messageText);
		}

		public void TestAddNewNADWithAddressInSG6Group_NoId()
		{
			mockExporter.Setup(m => m.Id).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Exporter).Returns(mockExporter.Object);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("NAD+EX+:P++EXPORTADORA ESPAÑOLA S.A.+BASOA 6+VITORIA++01012+ES'", messageText);
		}

		public void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory1()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory1).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1++10.00:117:141+A51801:117:148+A518:117:148'", messageText);
		}

		public void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory2()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory2).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1+020120309991103400:122:148++A51801:117:148+A518:117:148'", messageText);
		}

		public void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory3()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory3).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1+020120309991103400:122:148+10.00:117:141++A518:117:148'", messageText);
		}

		public void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory4()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory4).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1+020120309991103400:122:148+10.00:117:141+A51801:117:148'", messageText);
		}

		public void TestAddFTXSegmentForAdditionalInfomation_NoSpecialConditions()
		{
			mockLine1.Setup(m => m.SpecialConditions).Returns((IDUAExportSpecialConditions)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("FTX+REG+++30300:30301:30302:30303:TEXTO DE INDICACIONES ESPECIAL'", messageText);
		}

		public void TestAddFTXSegmentForAdditionalInfomation_NoConditionCode1()
		{
			var mockSpecialConditions = SetUpSpecialConditions();
			mockSpecialConditions.Setup(m => m.Code1).Returns(ZString.Empty);
			mockLine1.Setup(m => m.SpecialConditions).Returns(mockSpecialConditions.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("FTX+REG+++:30301:30302:30303:TEXTO DE INDICACIONES ESPECIAL'", messageText);
		}

		public void TestAddFTXSegmentForAdditionalInfomation_NoConditionCode2()
		{
			var mockSpecialConditions = SetUpSpecialConditions();
			mockSpecialConditions.Setup(m => m.Code2).Returns(ZString.Empty);
			mockLine1.Setup(m => m.SpecialConditions).Returns(mockSpecialConditions.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("FTX+REG+++30300::30302:30303:TEXTO DE INDICACIONES ESPECIAL'", messageText);
		}

		public void TestAddFTXSegmentForAdditionalInfomation_NoConditionCode3()
		{
			var mockSpecialConditions = SetUpSpecialConditions();
			mockSpecialConditions.Setup(m => m.Code3).Returns(ZString.Empty);
			mockLine1.Setup(m => m.SpecialConditions).Returns(mockSpecialConditions.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("FTX+REG+++30300:30301::30303:TEXTO DE INDICACIONES ESPECIAL'", messageText);
		}

		public void TestAddFTXSegmentForAdditionalInfomation_NoConditionCode4()
		{
			var mockSpecialConditions = SetUpSpecialConditions();
			mockSpecialConditions.Setup(m => m.Code4).Returns(ZString.Empty);
			mockLine1.Setup(m => m.SpecialConditions).Returns(mockSpecialConditions.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("FTX+REG+++30300:30301:30302::TEXTO DE INDICACIONES ESPECIAL'", messageText);
		}

		public void TestAddFTXSegmentForAdditionalInfomation_NoConditionText()
		{
			var mockSpecialConditions = SetUpSpecialConditions();
			mockSpecialConditions.Setup(m => m.Text).Returns(ZString.Empty);
			mockLine1.Setup(m => m.SpecialConditions).Returns(mockSpecialConditions.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("FTX+REG+++30300:30301:30302:30303'", messageText);
		}

		public void TestAddNewSG31Group_NoInternalPackages()
		{
			mockLine1.Setup(m => m.InternalPackages).Returns((IInternalPackagesInfoCommon)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PAC++1'PCI++MARCABULTIMARCABULTIMARCABULTIAASDN:T:50+BX'", messageText);
		}

		public void TestAddNewSG31Group_NoPackagesInInternalPackages()
		{
			var mockInternalPackages = new Mock<IInternalPackagesInfoCommon>();
			mockInternalPackages.Setup(m => m.Packages).Returns((IReadOnlyCollection<IInternalPackageIdentificationCommon>)Enumerable.Empty<IInternalPackageIdentificationCommon>());
			mockLine1.Setup(m => m.InternalPackages).Returns(mockInternalPackages.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PAC++1'PCI++MARCABULTIMARCABULTIMARCABULTIAASDN:T:50+BX'", messageText);
		}

		public void TestAddNewSG30Group_NoDocumentReferenceNumber()
		{
			mockLine1.Setup(m => m.DocumentReferenceNumber).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("RFF+AAE:DUA08ES00280112345679001:Z'", messageText);
		}

		public void TestAddNewSG37Groups_NoQuantity()
		{
			var dateOfIssue = new ZDateTime(2019, 08, 15);
			var mockDocument = SetUpDocument(dateOfIssue, ZDateTime.Empty, ZDecimal.Zero);
			mockLine1.Setup(m => m.Documents).Returns(new[] { mockDocument.Object });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("DOC+:::X001+ES3600000001'DTM+137:190815:101'", messageText);
		}

		public void TestAddNewSG37Groups_NoQuantityUnit()
		{
			var dateOfIssue = new ZDateTime(2019, 08, 15);
			var mockDocument = SetUpDocument(dateOfIssue, ZDateTime.Empty, 100M);
			mockDocument.Setup(m => m.QtyUnit).Returns(ZString.Empty);
			mockLine1.Setup(m => m.Documents).Returns(new[] { mockDocument.Object });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("DOC+:::X001+ES3600000001'DTM+137:190815:101'", messageText);
		}

		public void TestAddNewSG37Groups_QuantityWithoutDecimals()
		{
			var dateOfIssue = new ZDateTime(2019, 08, 15);
			var mockDocument = SetUpDocument(dateOfIssue, ZDateTime.Empty, 100M);
			mockLine1.Setup(m => m.Documents).Returns(new[] { mockDocument.Object });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("DOC+:::X001+ES3600000001::KN000000000100000'DTM+137:190815:101'", messageText);
		}

		[TestDate(2020, 3, 9, 16, 13, 23, 456)]
		public override void TestUNBNotTest()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns("1210244B");
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("UNB+UNOA:1+1210244B:ZZ+AEATADUE:ZZ+200309:1613+<<MSGNO PLACEHOLDER>>++&EE'", messageText);
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.Export;
		protected override ZString DeclarantIdForUNBSegment => "1210244B";

		protected override ZString GetTestFile() => GetTestFileContents(ExportTestFileConstants.TestFilePath, "TestDUAExportMessage.txt");

		protected override DUAExportMessageBuilder CreateMessageBuilder() => new DUAExportMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType);
		protected override DUAExportMessageBuilder CreateMessageBuilderWithNullProvider() => new DUAExportMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected Mock<IDUAExportMessageDataProvider> mockProvider;

		protected void SetUpMockProvider()
		{
			mockProvider = new Mock<IDUAExportMessageDataProvider>();

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
			mockProvider.Setup(m => m.MessageType).Returns("9");
			mockProvider.Setup(m => m.CustomsProcedureCategory1).Returns("EX");
			mockProvider.Setup(m => m.CustomsProcedureCategory2).Returns("A");
			mockProvider.Setup(m => m.CustomsProcedureCategory3).Returns("T2L");
			mockProvider.Setup(m => m.CustomsProcedureCategory4).Returns("34");
			mockProvider.Setup(m => m.CustomsProcedureCategory5).Returns("2801");
			mockProvider.Setup(m => m.CountryOfExport).Returns("ES");
			mockProvider.Setup(m => m.CountryOfDestination).Returns("IS");
			mockProvider.Setup(m => m.CustomsOfficeofExitCountryCode).Returns("ES");
			mockProvider.Setup(m => m.CustomsOfficeofExit).Returns("000801");
			mockProvider.Setup(m => m.LocationOfGoodsExamCustomsOffice).Returns("0811");
			mockProvider.Setup(m => m.LocationOfGoodsExam).Returns("BCN010");
			mockProvider.Setup(m => m.Warehouse).Returns("ESXA28000021");
			mockProvider.Setup(m => m.DateOfRecap).Returns(new ZDateTime(2019, 08, 15));
			mockProvider.Setup(m => m.GoodsInContainerIndicator).Returns(ZBool.True);
			mockProvider.Setup(m => m.RMTIndicator).Returns(ZBool.True);
			mockProvider.Setup(m => m.CountryCodes).Returns(new ZString[] { "ES", "FR", "IS" });
			mockProvider.Setup(m => m.SealCodes).Returns(new ZString[] { "SEAL1", "SEAL2" });
			mockProvider.Setup(m => m.TextFunctionCode).Returns("A");
			mockProvider.Setup(m => m.ReferenceNumber).Returns("REFNUM");
			mockProvider.Setup(m => m.SpecificCircumstancesIndicator).Returns("CODE");
			mockProvider.Setup(m => m.InternalTransportMode).Returns("3");
			mockProvider.Setup(m => m.TransportModeName).Returns("M-9999-ZZ");
			mockProvider.Setup(m => m.TermsOfDeliveryCode).Returns("CIF");
			mockProvider.Setup(m => m.DeliveryLocation).Returns("BARCELONA");
			mockProvider.Setup(m => m.LocationId).Returns("3");
			mockProvider.Setup(m => m.TotalAmount).Returns(14987);
			mockProvider.Setup(m => m.TotalAmountCurrencyCode).Returns("USD");
			mockProvider.Setup(m => m.IsDeclarationInEuros).Returns(ZBool.True);
			mockProvider.Setup(m => m.TotalNumberOfGoods).Returns(15);
			mockProvider.Setup(m => m.TotalNumberOfPackageElements).Returns(1586);

			mockBorderTransportMode = BuilderHelperTest.SetUpTransportMediumInfo("4", "M-1234-GD", "ES");
			mockProvider.Setup(m => m.BorderTransportMode).Returns(mockBorderTransportMode.Object);

			mockExporter = SetUpDUAExportParty("A01010101", "EXPORTADORA ESPAÑOLA S.A.", "BASOA 6", "VITORIA", "01012", "ES", "P");
			mockProvider.Setup(m => m.Exporter).Returns(mockExporter.Object);

			var mockReceiver = SetUpDUAExportParty(ZString.Empty, "NORON EHF", "SKUTUVOGUR 7", "104 REYKJAVIK", "40025", "IS", ZString.Empty);
			mockProvider.Setup(m => m.Receiver).Returns(mockReceiver.Object);

			var mockDeclarant = BuilderHelperTest.SetUpExportDeclarantPartyId("O", "MIDIRECCION.CORREO.EN.CASTILLAYLEON@MIXMAIL.COM");
			mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant);

			var mockExternalPackages = BuilderHelperTest.SetUpExternalPackages(2, new ZString[] { "TCKU2126154", "TCKU2126133", "AA", "BB", "CC", "DD", "EE", "FF", "GG", "HH" });
			var mockSpecialConditions = SetUpSpecialConditions();
			var mockInternalPackages = BuilderHelperTest.SetUpInternalPackages();
			var mockVehiclePackages = BuilderHelperTest.SetUpVehicle("VS8ZAZB7861ZB6913", "RENAULT", "LAGUNA 2007");
			var document1 = SetUpDocument(new ZDateTime(2019, 08, 15), ZDateTime.Empty, 1324.85M);
			var document2 = SetUpDocument(ZDateTime.Empty, new ZDateTime(2020, 08, 15), 100M);

			mockLine1 = SetUpLine(1, null, null, new[] { mockInternalPackages, mockInternalPackages }, new[] { mockVehiclePackages, mockVehiclePackages }, new[] { document1.Object, document2.Object });
			var mockLine2 = SetUpLine(2, mockExternalPackages, mockSpecialConditions.Object, Enumerable.Empty<IInternalPackageIdentificationCommon>(), Enumerable.Empty<IVehicleCommon>(), Enumerable.Empty<IDUAExportDocuments>());

			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}

		protected Mock<ITransportMediumInfoCommon> mockBorderTransportMode;
		protected Mock<IDUAExportPartyProvider> mockExporter;
		protected Mock<IDUAExportLine> mockLine1;

		Mock<IDUAExportPartyProvider> SetUpDUAExportParty(ZString id, ZString name, ZString address, ZString city, ZString postCode, ZString country, ZString prganizationCode)
		{
			var addressInformation = new Mock<IDUAExportPartyProvider>();
			addressInformation.Setup(m => m.Id).Returns(id);
			addressInformation.Setup(m => m.Name).Returns(name);
			addressInformation.Setup(m => m.Address).Returns(address);
			addressInformation.Setup(m => m.City).Returns(city);
			addressInformation.Setup(m => m.PostCode).Returns(postCode);
			addressInformation.Setup(m => m.Country).Returns(country);
			addressInformation.Setup(m => m.OrganizationCodeQualifier).Returns(prganizationCode);
			return addressInformation;
		}

		Mock<IDUAExportLine> SetUpLine(ZInt goodsItemNumber, IExternalPackagesInfoCommon externalPackage, IDUAExportSpecialConditions specialConditions, IEnumerable<IInternalPackageIdentificationCommon> internalPackages, IEnumerable<IVehicleCommon> vehiclePackages, IEnumerable<IDUAExportDocuments> documents)
		{
			var mockLine = new Mock<IDUAExportLine>();
			mockLine.Setup(m => m.GoodsItemNumber).Returns(goodsItemNumber);
			mockLine.Setup(m => m.GoodsCustomsProcedureCategory1).Returns("020120309991103400");
			mockLine.Setup(m => m.GoodsCustomsProcedureCategory2).Returns("10.00");
			mockLine.Setup(m => m.GoodsCustomsProcedureCategory3).Returns("A51801");
			mockLine.Setup(m => m.GoodsCustomsProcedureCategory4).Returns("A518");
			mockLine.Setup(m => m.GoodsDescription).Returns("CALZADO DE PIEL DE COCODRILO PARA SEÑORAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
			mockLine.Setup(m => m.CountryOfOrigin).Returns("ES");
			mockLine.Setup(m => m.StateOfOrigin).Returns("28");
			mockLine.Setup(m => m.GrossWeightInKG).Returns(0.100);
			mockLine.Setup(m => m.NetWeightInKG).Returns(123);
			mockLine.Setup(m => m.SupplementaryUnitsNumber).Returns(12);
			mockLine.Setup(m => m.SupplementaryUnitsQualifier).Returns("KL");
			mockLine.Setup(m => m.OtherUnitsNumber).Returns(2500);
			mockLine.Setup(m => m.OtherUnitsQualifier).Returns("UN");
			mockLine.Setup(m => m.DangerousGoodsCode).Returns("4134");
			mockLine.Setup(m => m.TotalGoodValueInEuros).Returns(8527.45);
			mockLine.Setup(m => m.DocumentReferenceNumber).Returns("DUA08ES00280112345679001");
			mockLine.Setup(m => m.DocumentTypeCode).Returns("Z");

			mockLine.Setup(m => m.ExternalPackages).Returns(externalPackage);
			mockLine.Setup(m => m.SpecialConditions).Returns(specialConditions);

			var mockInternalPackages = new Mock<IInternalPackagesInfoCommon>();
			mockInternalPackages.Setup(m => m.Packages).Returns((IReadOnlyCollection<IInternalPackageIdentificationCommon>)internalPackages);
			mockLine.Setup(m => m.InternalPackages).Returns(mockInternalPackages.Object);

			var mockVehiclePackages = new Mock<IVehiclePackagesInfoCommon>();
			mockVehiclePackages.Setup(m => m.Packages).Returns((IReadOnlyCollection<IVehicleCommon>)vehiclePackages);
			mockLine.Setup(m => m.VehiclePackages).Returns(mockVehiclePackages.Object);

			mockLine.Setup(m => m.Documents).Returns((IReadOnlyCollection<IDUAExportDocuments>)documents);
			return mockLine;
		}

		Mock<IDUAExportSpecialConditions> SetUpSpecialConditions()
		{
			var mockSpecialConditions = new Mock<IDUAExportSpecialConditions>();
			mockSpecialConditions.Setup(m => m.Code1).Returns("30300");
			mockSpecialConditions.Setup(m => m.Code2).Returns("30301");
			mockSpecialConditions.Setup(m => m.Code3).Returns("30302");
			mockSpecialConditions.Setup(m => m.Code4).Returns("30303");
			mockSpecialConditions.Setup(m => m.Text).Returns("TEXTO DE INDICACIONES ESPECIAL");
			return mockSpecialConditions;
		}

		Mock<IDUAExportDocuments> SetUpDocument(ZDateTime dateOfIssue, ZDateTime dateOfExpiry, ZDecimal quantity)
		{
			var mockDocument = new Mock<IDUAExportDocuments>();
			mockDocument.Setup(m => m.Name).Returns("X001");
			mockDocument.Setup(m => m.Number).Returns("ES3600000001");
			mockDocument.Setup(m => m.Quantity).Returns(quantity);
			mockDocument.Setup(m => m.QtyUnit).Returns("KN");
			mockDocument.Setup(m => m.DateOfIssue).Returns(dateOfIssue);
			mockDocument.Setup(m => m.DateOfExpiry).Returns(dateOfExpiry);
			return mockDocument;
		}
	}
}
