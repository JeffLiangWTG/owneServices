using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDH2V1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(DeclarationDVDMessageBuilder))]
class DeclarationDVDMessageBuilderTest : DVDCommonMessageBuilderTest<DeclarationDVDMessageBuilder, IDeclarationDVDMessageDataProvider, Dvdh2V1Ent>
{
	#region Tests
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
		AssertNotContains("IndicadorTest", messageText);
	}

	public void TestPopulateHeader()
	{
		mockProvider.Setup(m => m.Header).Returns((IDeclarationDVDHeader)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateAuthorisations()
	{
		mockHeader.Setup(m => m.Authorisations).Returns((IReadOnlyCollection<IDeclarationDVDAuthorisation>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateAuthorisation()
	{
		mockHeader.Setup(m => m.Authorisations).Returns(new IDeclarationDVDAuthorisation[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateAdditionalSupplyActors()
	{
		mockHeader.Setup(m => m.AdditionalSupplyActors).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateAdditionalSupplyActor()
	{
		mockHeader.Setup(m => m.AdditionalSupplyActors).Returns(new IAdditionalSupplyChainActorCommon[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateSupportingDocuments()
	{
		mockHeader.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IDeclarationDVDSupportingDocument>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateSupportingDocument()
	{
		mockHeader.Setup(m => m.SupportingDocuments).Returns(new IDeclarationDVDSupportingDocument[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLocationOfGoods()
	{
		mockHeader.Setup(m => m.LocationOfGoods).Returns((IDeclarationDVDLocationOfGoods)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateWarehouse()
	{
		mockHeader.Setup(m => m.Warehouse).Returns((IWarehouseCommon)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulatePreviousDocuments()
	{
		mockHeader.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IDeclarationDVDPreviousDocument>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulatePreviousDocument()
	{
		mockHeader.Setup(m => m.PreviousDocuments).Returns(new IDeclarationDVDPreviousDocument[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGuarantees()
	{
		mockHeader.Setup(m => m.Guarantees).Returns((IReadOnlyCollection<IDeclarationDVDGuarantee>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGuarantee()
	{
		mockHeader.Setup(m => m.Guarantees).Returns(new IDeclarationDVDGuarantee[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLines()
	{
		mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IDeclarationDVDLine>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLine()
	{
		mockProvider.Setup(m => m.Lines).Returns(new IDeclarationDVDLine[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineSupportingDocuments()
	{
		mockLine1.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IDeclarationDVDSupportingDocumentForLine>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineSupportingDocument()
	{
		mockLine1.Setup(m => m.SupportingDocuments).Returns(new IDeclarationDVDSupportingDocumentForLine[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineAdditionalInfos()
	{
		mockLine1.Setup(m => m.AdditionalInfos).Returns((IReadOnlyCollection<IDeclarationDVDAdditionalInfo>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineAdditionalInfo()
	{
		mockLine1.Setup(m => m.AdditionalInfos).Returns(new IDeclarationDVDAdditionalInfo[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineAdditionalSupplyActors()
	{
		mockLine1.Setup(m => m.AdditionalSupplyActors).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineAdditionalSupplyActor()
	{
		mockLine1.Setup(m => m.AdditionalSupplyActors).Returns(new IAdditionalSupplyChainActorCommon[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineTariffAdditionalCodes()
	{
		mockLine1.Setup(m => m.TariffAdditionalCodes).Returns((IReadOnlyCollection<ZString>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineTariffAdditionalCode()
	{
		mockLine1.Setup(m => m.TariffAdditionalCodes).Returns(new ZString[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineNationalAdditionalCodes()
	{
		mockLine1.Setup(m => m.NationalAdditionalCodes).Returns((IReadOnlyCollection<ZString>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineNationalAdditionalCode()
	{
		mockLine1.Setup(m => m.NationalAdditionalCodes).Returns(new ZString[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineTaxes()
	{
		mockLine1.Setup(m => m.Taxes).Returns((IReadOnlyCollection<IDeclarationDVDTax>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineTax()
	{
		mockLine1.Setup(m => m.Taxes).Returns(new IDeclarationDVDTax[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineAdditionalProcedures()
	{
		mockLine1.Setup(m => m.AdditionalProcedures).Returns((IReadOnlyCollection<IDeclarationDVDEUAndNationalCodes>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineAdditionalProcedure()
	{
		mockLine1.Setup(m => m.AdditionalProcedures).Returns(new IDeclarationDVDEUAndNationalCodes[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLinePackages()
	{
		mockLine1.Setup(m => m.Packages).Returns((IReadOnlyCollection<IDVDCommonPackage>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLinePackage()
	{
		mockLine1.Setup(m => m.Packages).Returns(new IDVDCommonPackage[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineVehicles()
	{
		mockLine1.Setup(m => m.Vehicles).Returns((IReadOnlyCollection<IDeclarationDVDVehicle>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLineVehicle()
	{
		mockLine1.Setup(m => m.Vehicles).Returns(new IDeclarationDVDVehicle[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLinePreviousDocuments()
	{
		mockLine1.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IDeclarationDVDPreviousDocument>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLinePreviousDocument()
	{
		mockLine1.Setup(m => m.PreviousDocuments).Returns(new IDeclarationDVDPreviousDocument[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.DvdH2;

	protected override DeclarationDVDMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new DeclarationDVDMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override DeclarationDVDMessageBuilder CreateMessageBuilderWithNullProvider() => new DeclarationDVDMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.DVDTestFilePath, "TestDeclarationDVD.txt");

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider.Setup(m => m.MRN).Returns("20ES00999830001277");

		mockHeader = SetUpHeader();
		mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);

		var mockSupDoc1 = SetUpSupportingDocumentForLine("A123", "1234", "12345678A", new DateTime(2022, 7, 21), "NAR", 11.5m, "EUR", 50.6m);
		var mockSupDoc2 = SetUpSupportingDocumentForLine("B123", "4567", "A87654321", new DateTime(2022, 10, 6), "KGM", 20.4m, ZString.Empty, 0m);
		var mockSupDoc3 = SetUpSupportingDocumentForLine("C123", "4567", "A87654321", new DateTime(2022, 10, 6), ZString.Empty, 0m, "DOL", 56.7m);
		var supportingDocuments = new IDeclarationDVDSupportingDocumentForLine[] { mockSupDoc1.Object, mockSupDoc2.Object, mockSupDoc3.Object };

		var mockAddInfo1 = SetUpAdditionalInfo("A123", "1234", "description1");
		var mockAddInfo2 = SetUpAdditionalInfo("B123", "4567", "description2");
		var additionalInfos = new IDeclarationDVDAdditionalInfo[] { mockAddInfo1.Object, mockAddInfo2.Object };

		var mockAdditionalSupplyActor1 = SetUpSupplyActor("WH", "12345678A");
		var mockAdditionalSupplyActor2 = SetUpSupplyActor("MF", "A87654321");
		var additionalSupplyActors = new IAdditionalSupplyChainActorCommon[] { mockAdditionalSupplyActor1.Object, mockAdditionalSupplyActor2.Object };

		var mockTax1 = SetUpTax("NAR", 10.2m, 0m);
		var mockTax2 = SetUpTax("HL", 30.4m, 50.7m);
		var mockTax3 = SetUpTax(ZString.Empty, 0m, 60.7m);
		var taxes = new IDeclarationDVDTax[] { mockTax1.Object, mockTax2.Object, mockTax3.Object };

		var mockAddProcedure1 = SetUpAdditionalProcedure("123", "124");
		var mockAddProcedure2 = SetUpAdditionalProcedure("125", "126");
		var additionalProcedures = new IDeclarationDVDEUAndNationalCodes[] { mockAddProcedure1.Object, mockAddProcedure2.Object };

		var mockPackage1 = SetUpPackage("VO", "MARCA1", 0);
		var mockPackage2 = SetUpPackage("CR", "MARCA2", 20);
		var packages = new IDVDCommonPackage[] { mockPackage1.Object, mockPackage2.Object };

		var mockVehicle1 = SetUpVehicle("FR", "VS8ZAZB7861ZB6913", "RENAULT", "LAGUNA 2007");
		var mockVehicle2 = SetUpVehicle("FR", "XX", "Ford", "Focus");
		var vehicles = new IDeclarationDVDVehicle[] { mockVehicle1.Object, mockVehicle2.Object };

		var mockPrevDoc1 = SetUpPreviousDocument("A123", "12345678A", "1", "NAR", 2.7m);
		var mockPrevDoc2 = SetUpPreviousDocument("B123", "A87654321", "2", "KGM", 5.8m);
		var mockPrevDoc3 = SetUpPreviousDocument("C123", "A87654321", "3", ZString.Empty, 5.8m);
		var previousDocuments = new IDeclarationDVDPreviousDocument[] { mockPrevDoc1.Object, mockPrevDoc2.Object, mockPrevDoc3.Object };

		mockLine1 = SetUpLine("1", supportingDocuments, Enumerable.Empty<IDeclarationDVDAdditionalInfo>(), additionalSupplyActors, Enumerable.Empty<ZString>(), new ZString[] { "987", "654" },
			Enumerable.Empty<IDeclarationDVDTax>(), new ZString[] { "CONT1", "CONT2" }, Enumerable.Empty<IDeclarationDVDEUAndNationalCodes>(), packages, Enumerable.Empty<IDeclarationDVDVehicle>(), previousDocuments);
		var mockLine2 = SetUpLine("2", Enumerable.Empty<IDeclarationDVDSupportingDocumentForLine>(), additionalInfos, Enumerable.Empty<IAdditionalSupplyChainActorCommon>(),
			new ZString[] { "123", "456" }, Enumerable.Empty<ZString>(), taxes, Enumerable.Empty<ZString>(), additionalProcedures,
			Enumerable.Empty<IDVDCommonPackage>(), vehicles, Enumerable.Empty<IDeclarationDVDPreviousDocument>());
		mockProvider.Setup(m => m.Lines).Returns(new IDeclarationDVDLine[] { mockLine1.Object, mockLine2.Object });
	}
	Mock<IDeclarationDVDHeader> mockHeader;
	Mock<IDeclarationDVDLine> mockLine1;

	#region Structures SetUp

	Mock<IDeclarationDVDHeader> SetUpHeader()
	{
		var mockHeader = new Mock<IDeclarationDVDHeader>();

		mockHeader.Setup(m => m.CustomsOffice).Returns("ES009999");
		mockHeader.Setup(m => m.LRN).Returns("PRLSVNE000006");
		mockHeader.Setup(m => m.DeclarationType).Returns("IM");
		mockHeader.Setup(m => m.DeclarationSubType).Returns("A");
		mockHeader.Setup(m => m.TotalGrossMass).Returns(1.2m);
		mockHeader.Setup(m => m.ExporterId).Returns("12345678A");
		mockHeader.Setup(m => m.ConsigneeId).Returns("98765432B");
		mockHeader.Setup(m => m.DeclarantUECode).Returns("00400");
		mockHeader.Setup(m => m.DeclarantId).Returns("ES12345678A");
		mockHeader.Setup(m => m.DeclarationEmail).Returns("mail.mail@mail.com");
		mockHeader.Setup(m => m.DeclarationOtherEmail).Returns("other.mail@mail.com");
		mockHeader.Setup(m => m.RepresentativeId).Returns("42753869C");
		mockHeader.Setup(m => m.RepresentativeType).Returns("2");
		mockHeader.Setup(m => m.TransportCode).Returns("1");
		mockHeader.Setup(m => m.IsContainerised).Returns(true);
		mockHeader.Setup(m => m.CountryOfDestination).Returns("FR");
		mockHeader.Setup(m => m.CountryOfExport).Returns("ES");
		mockHeader.Setup(m => m.UCRReferenceNumber).Returns("UCR");
		mockHeader.Setup(m => m.CustomOfficeOfPresentation).Returns("FR009999");

		var mockAuthorisation1 = SetUpAuthorisation("BTI", "A12345678");
		var mockAuthorisation2 = SetUpAuthorisation("AEOS", "A87654321");
		mockHeader.Setup(m => m.Authorisations).Returns(new IDeclarationDVDAuthorisation[] { mockAuthorisation1.Object, mockAuthorisation2.Object });

		var mockAdditionalSupplyActor1 = SetUpSupplyActor("CS", "12345678A");
		var mockAdditionalSupplyActor2 = SetUpSupplyActor("MF", "A87654321");
		mockHeader.Setup(m => m.AdditionalSupplyActors).Returns(new IAdditionalSupplyChainActorCommon[] { mockAdditionalSupplyActor1.Object, mockAdditionalSupplyActor2.Object });

		var mockSupDoc1 = SetUpSupportingDocument("A123", "1234", "12345678A", new DateTime(2022, 7, 21));
		var mockSupDoc2 = SetUpSupportingDocument("B123", "4567", "A87654321", new DateTime(2022, 10, 6));
		mockHeader.Setup(m => m.SupportingDocuments).Returns(new IDeclarationDVDSupportingDocument[] { mockSupDoc1.Object, mockSupDoc2.Object });

		var mockLocation = new Mock<IDeclarationDVDLocationOfGoods>();
		mockLocation.Setup(m => m.LocationCountry).Returns("ES");
		mockLocation.Setup(m => m.LocationType).Returns("B");
		mockLocation.Setup(m => m.LocationQualifier).Returns("Y");
		mockLocation.Setup(m => m.LocationId).Returns("010101GENE");
		mockLocation.Setup(m => m.LocationAdditionalId).Returns("1234");
		mockLocation.Setup(m => m.LocationAddress).Returns("Street");
		mockLocation.Setup(m => m.LocationCity).Returns("Madrid");
		mockLocation.Setup(m => m.LocationPostCode).Returns("28003");
		mockHeader.Setup(m => m.LocationOfGoods).Returns(mockLocation.Object);

		var mockWarehouse = BuilderHelperTest.SetUpWarehouseCommon();
		mockHeader.Setup(m => m.Warehouse).Returns(mockWarehouse);

		var mockPrevDoc1 = SetUpPreviousDocument("A123", "12345678A", "1", "NAR", 2.7m);
		var mockPrevDoc2 = SetUpPreviousDocument("B123", "A87654321", "2", "KGM", 5.8m);
		var mockPrevDoc3 = SetUpPreviousDocument("C123", "A87654321", "3", ZString.Empty, 5.8m);
		mockHeader.Setup(m => m.PreviousDocuments).Returns(new IDeclarationDVDPreviousDocument[] { mockPrevDoc1.Object, mockPrevDoc2.Object, mockPrevDoc3.Object });

		var mockGuarantee1 = SetUpGuarantee("ref1", "ref2", "AAAA", "EUR", 2.6m, "ES009999");
		var mockGuarantee2 = SetUpGuarantee("ref3", "ref4", "BBBB", "DOL", 8.3m, "ES009998");
		var mockGuarantee3 = SetUpGuarantee("ref3", "ref4", ZString.Empty, ZString.Empty, ZDecimal.Zero, "ES009998");
		mockHeader.Setup(m => m.Guarantees).Returns(new IDeclarationDVDGuarantee[] { mockGuarantee1.Object, mockGuarantee2.Object, mockGuarantee3.Object });

		return mockHeader;
	}

	Mock<IDeclarationDVDLine> SetUpLine(ZString lineNum, IEnumerable<IDeclarationDVDSupportingDocumentForLine> supDocs, IEnumerable<IDeclarationDVDAdditionalInfo> addInfos, IEnumerable<IAdditionalSupplyChainActorCommon> addSupplyChainActors,
												IEnumerable<ZString> tariffAddCodes, IEnumerable<ZString> nationalAddCodes, IEnumerable<IDeclarationDVDTax> taxes, IEnumerable<ZString> containers, IEnumerable<IDeclarationDVDEUAndNationalCodes> addProcedures,
												IEnumerable<IDVDCommonPackage> packages, IEnumerable<IDeclarationDVDVehicle> vehicles, IEnumerable<IDeclarationDVDPreviousDocument> prevDocs)
	{
		var mockLine = new Mock<IDeclarationDVDLine>();

		mockLine.Setup(m => m.LineNumber).Returns(lineNum);
		mockLine.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IDeclarationDVDSupportingDocumentForLine>)supDocs);
		mockLine.Setup(m => m.AdditionalInfos).Returns((IReadOnlyCollection<IDeclarationDVDAdditionalInfo>)addInfos);
		mockLine.Setup(m => m.AdditionalSupplyActors).Returns((IReadOnlyCollection<IAdditionalSupplyChainActorCommon>)addSupplyChainActors);
		mockLine.Setup(m => m.Description).Returns("description");
		mockLine.Setup(m => m.CusCode).Returns("0018896-5");
		mockLine.Setup(m => m.TariffCode).Returns("640192");
		mockLine.Setup(m => m.TariffCodeCombined).Returns("10");
		mockLine.Setup(m => m.TariffAdditionalCodes).Returns((IReadOnlyCollection<ZString>)tariffAddCodes);
		mockLine.Setup(m => m.NationalAdditionalCodes).Returns((IReadOnlyCollection<ZString>)nationalAddCodes);
		mockLine.Setup(m => m.PreferenceCode).Returns("1");
		mockLine.Setup(m => m.ReductionCode).Returns("00");
		mockLine.Setup(m => m.Taxes).Returns((IReadOnlyCollection<IDeclarationDVDTax>)taxes);
		mockLine.Setup(m => m.NetMass).Returns(11.22m);
		mockLine.Setup(m => m.GrossMass).Returns(66.88m);
		mockLine.Setup(m => m.SupplementaryUnitsQty).Returns(77.33m);
		mockLine.Setup(m => m.Containers).Returns((IReadOnlyCollection<ZString>)containers);
		mockLine.Setup(m => m.CountryOfDestination).Returns("FR");
		mockLine.Setup(m => m.CountryOfExport).Returns("ES");
		mockLine.Setup(m => m.RequestedCPC).Returns("40");
		mockLine.Setup(m => m.PreviousCPC).Returns("01");
		mockLine.Setup(m => m.AdditionalProcedures).Returns((IReadOnlyCollection<IDeclarationDVDEUAndNationalCodes>)addProcedures);
		mockLine.Setup(m => m.CountryOfOrigin).Returns("IT");
		mockLine.Setup(m => m.Packages).Returns((IReadOnlyCollection<IDVDCommonPackage>)packages);
		mockLine.Setup(m => m.Vehicles).Returns((IReadOnlyCollection<IDeclarationDVDVehicle>)vehicles);
		mockLine.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IDeclarationDVDPreviousDocument>)prevDocs);
		mockLine.Setup(m => m.UCRReferenceNumber).Returns("UCR");

		return mockLine;
	}

	Mock<IDeclarationDVDAuthorisation> SetUpAuthorisation(ZString type, ZString ownerId)
	{
		var mockSupplyActor = new Mock<IDeclarationDVDAuthorisation>();
		mockSupplyActor.Setup(m => m.Type).Returns(type);
		mockSupplyActor.Setup(m => m.OwnerId).Returns(ownerId);
		return mockSupplyActor;
	}

	Mock<IAdditionalSupplyChainActorCommon> SetUpSupplyActor(ZString role, ZString id)
	{
		var mockSupplyActor = new Mock<IAdditionalSupplyChainActorCommon>();
		mockSupplyActor.Setup(m => m.Role).Returns(role);
		mockSupplyActor.Setup(m => m.Id).Returns(id);
		return mockSupplyActor;
	}

	Mock<IDeclarationDVDSupportingDocument> SetUpSupportingDocument(ZString euCode, ZString nationalCode, ZString number, ZDateTime documentDate)
	{
		var mockDocument = new Mock<IDeclarationDVDSupportingDocument>();
		mockDocument.Setup(m => m.EUCode).Returns(euCode);
		mockDocument.Setup(m => m.NationalCode).Returns(nationalCode);
		mockDocument.Setup(m => m.Number).Returns(number);
		mockDocument.Setup(m => m.DocumentDate).Returns(documentDate);
		return mockDocument;
	}

	Mock<IDeclarationDVDSupportingDocumentForLine> SetUpSupportingDocumentForLine(ZString euCode, ZString nationalCode, ZString number, ZDateTime documentDate, ZString unitOfMeasure, ZDecimal quantity, ZString currency, ZDecimal amount)
	{
		var mockDocument = new Mock<IDeclarationDVDSupportingDocumentForLine>();
		mockDocument.Setup(m => m.EUCode).Returns(euCode);
		mockDocument.Setup(m => m.NationalCode).Returns(nationalCode);
		mockDocument.Setup(m => m.Number).Returns(number);
		mockDocument.Setup(m => m.DocumentDate).Returns(documentDate);
		mockDocument.Setup(m => m.UnitOfMeasure).Returns(unitOfMeasure);
		mockDocument.Setup(m => m.Quantity).Returns(quantity);
		mockDocument.Setup(m => m.Currency).Returns(currency);
		mockDocument.Setup(m => m.Amount).Returns(amount);
		return mockDocument;
	}

	Mock<IDeclarationDVDPreviousDocument> SetUpPreviousDocument(ZString name, ZString number, ZString lineNumber, ZString unitOfMeasure, ZDecimal quantity)
	{
		var mockDocument = new Mock<IDeclarationDVDPreviousDocument>();
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		mockDocument.Setup(m => m.LineNumber).Returns(lineNumber);
		mockDocument.Setup(m => m.UnitOfMeasure).Returns(unitOfMeasure);
		mockDocument.Setup(m => m.Quantity).Returns(quantity);
		return mockDocument;
	}

	Mock<IDeclarationDVDGuarantee> SetUpGuarantee(ZString grnRef, ZString noGrnRef, ZString accessCode, ZString currency, ZDecimal amount, ZString office)
	{
		var mockDocument = new Mock<IDeclarationDVDGuarantee>();
		mockDocument.Setup(m => m.GRNReference).Returns(grnRef);
		mockDocument.Setup(m => m.NoGRNReference).Returns(noGrnRef);
		mockDocument.Setup(m => m.AccessCode).Returns(accessCode);
		mockDocument.Setup(m => m.Currency).Returns(currency);
		mockDocument.Setup(m => m.Amount).Returns(amount);
		mockDocument.Setup(m => m.Office).Returns(office);
		return mockDocument;
	}

	Mock<IDeclarationDVDAdditionalInfo> SetUpAdditionalInfo(ZString euCode, ZString nationalCode, ZString description)
	{
		var mockAdditionalInfo = new Mock<IDeclarationDVDAdditionalInfo>();
		mockAdditionalInfo.Setup(m => m.EUCode).Returns(euCode);
		mockAdditionalInfo.Setup(m => m.NationalCode).Returns(nationalCode);
		mockAdditionalInfo.Setup(m => m.Description).Returns(description);
		return mockAdditionalInfo;
	}

	Mock<IDeclarationDVDTax> SetUpTax(ZString baseUnit, ZDecimal baseQuantity, ZDecimal baseAmount)
	{
		var mockTax = new Mock<IDeclarationDVDTax>();
		mockTax.Setup(m => m.BaseUnit).Returns(baseUnit);
		mockTax.Setup(m => m.BaseQuantity).Returns(baseQuantity);
		mockTax.Setup(m => m.BaseAmount).Returns(baseAmount);
		return mockTax;
	}

	Mock<IDeclarationDVDEUAndNationalCodes> SetUpAdditionalProcedure(ZString euCode, ZString nationalCode)
	{
		var mockAdditionalProcedure = new Mock<IDeclarationDVDEUAndNationalCodes>();
		mockAdditionalProcedure.Setup(m => m.EUCode).Returns(euCode);
		mockAdditionalProcedure.Setup(m => m.NationalCode).Returns(nationalCode);
		return mockAdditionalProcedure;
	}

	Mock<IDeclarationDVDVehicle> SetUpVehicle(ZString type, ZString chassis, ZString brand, ZString model)
	{
		var mockVehicle = new Mock<IDeclarationDVDVehicle>();
		mockVehicle.Setup(m => m.Chassis).Returns(chassis);
		mockVehicle.Setup(m => m.Brand).Returns(brand);
		mockVehicle.Setup(m => m.Model).Returns(model);
		mockVehicle.Setup(m => m.Type).Returns(type);
		return mockVehicle;
	}
	#endregion
}
