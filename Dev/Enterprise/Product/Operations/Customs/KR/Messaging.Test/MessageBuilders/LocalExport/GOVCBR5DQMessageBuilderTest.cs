using System.IO;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5DQMessageBuilderTest : TestCaseWithFactory
	{
		Mock<ILocalExportEntryHeader> localExportHeaderMock;

		protected override void SetUp()
		{
			base.SetUp();

			#region ILocalExportEntryHeader
			localExportHeaderMock = new Mock<ILocalExportEntryHeader>();
			localExportHeaderMock.Setup(m => m.DeclarationType).Returns(LocalExportTransactionNatureCodeList.Codes._07);
			localExportHeaderMock.Setup(m => m.DeclarationNumber).Returns("1083699012345");
			localExportHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			localExportHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			localExportHeaderMock.Setup(m => m.BondedAreaCode).Returns("반입장소");
			localExportHeaderMock.Setup(m => m.DeclarationDate).Returns(new ZDate("2013-01-01"));
			localExportHeaderMock.Setup(m => m.FlightNoOrVesselName).Returns("홍길동호");
			localExportHeaderMock.Setup(m => m.MRNNo).Returns("00000000000");
			localExportHeaderMock.Setup(m => m.CrewCount).Returns(15);
			localExportHeaderMock.Setup(m => m.ScheduledSailingDays).Returns(24);
			localExportHeaderMock.Setup(m => m.GoodsType).Returns("1");
			localExportHeaderMock.Setup(m => m.DrawbackApplicantType).Returns("1");
			localExportHeaderMock.Setup(m => m.VesselRadioCallSign).Returns("A01B2");
			localExportHeaderMock.Setup(m => m.TotalDeclarationAmount).Returns(10998m);
			localExportHeaderMock.Setup(m => m.TotalGrossWeight).Returns(900m);
			localExportHeaderMock.Setup(m => m.TotalPackages).Returns(100);
			#endregion

			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.BusinessRegNo).Returns("사업자등록번호");
			supplierMock.Setup(m => m.UnipassIDForOrganization).Returns("통관고유부호");
			localExportHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDeclaration_TestWithNotNull()
		{
			var exporterMock = new Mock<IOrganization>();
			exporterMock.Setup(m => m.BusinessRegNo).Returns("1234567890");
			localExportHeaderMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			var manufacturerMock = new Mock<IOrganization>();
			manufacturerMock.Setup(m => m.BusinessRegNo).Returns("사업자등록번호");
			manufacturerMock.Setup(m => m.UnipassIDForOrganization).Returns("통관고유부호");
			localExportHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturerMock.Object);

			#region Stevedores
			var borderTransportMeans1 = new Mock<ILocalExportStevedore>();
			borderTransportMeans1.Setup(m => m.SequenceNo).Returns(1);
			borderTransportMeans1.Setup(m => m.Birthday).Returns(new ZDate("1991-05-06"));
			borderTransportMeans1.Setup(m => m.FullName).Returns("홍길동");
			borderTransportMeans1.Setup(m => m.RoadNameCode).Returns("110001");
			borderTransportMeans1.Setup(m => m.BuildingNumber).Returns("121200");
			borderTransportMeans1.Setup(m => m.Postcode).Returns("43012");
			borderTransportMeans1.Setup(m => m.AddressLine1).Returns("기본주소");
			borderTransportMeans1.Setup(m => m.AddressLine2).Returns("상세주소");

			var borderTransportMeans2 = new Mock<ILocalExportStevedore>();
			borderTransportMeans2.Setup(m => m.SequenceNo).Returns(2);
			borderTransportMeans2.Setup(m => m.Birthday).Returns(new ZDate("1991-06-06"));
			borderTransportMeans2.Setup(m => m.FullName).Returns("Hong-Gil-Dong");
			borderTransportMeans2.Setup(m => m.RoadNameCode).Returns("110002");
			borderTransportMeans2.Setup(m => m.BuildingNumber).Returns("121201");
			borderTransportMeans2.Setup(m => m.Postcode).Returns("43013");
			borderTransportMeans2.Setup(m => m.AddressLine1).Returns("기본주소2");
			borderTransportMeans2.Setup(m => m.AddressLine2).Returns("상세주소2");

			localExportHeaderMock.Setup(m => m.Stevedores).Returns(new ILocalExportStevedore[] { borderTransportMeans1.Object, borderTransportMeans2.Object });
			#endregion

			#region EntryLines
			var entryLines1 = new Mock<ILocalExportEntryLine>();
			entryLines1.Setup(m => m.EntryLineNo).Returns(1);
			entryLines1.Setup(m => m.HSCode).Returns("1234567890");
			entryLines1.Setup(m => m.InvoiceDescription).Returns("STAINLESS STEEL");
			entryLines1.Setup(m => m.GoodsNo).Returns("물품식별번호");
			entryLines1.Setup(m => m.Quantity).Returns(9999m);
			entryLines1.Setup(m => m.QuantityUnit).Returns("KG");
			entryLines1.Setup(m => m.Packages).Returns(99);
			entryLines1.Setup(m => m.PackagesType).Returns("VL");
			entryLines1.Setup(m => m.NetWeight).Returns(9999m);
			entryLines1.Setup(m => m.FOBAmount).Returns(9999m);
			entryLines1.Setup(m => m.DocumentNo).Returns("L172770925459");
			entryLines1.Setup(m => m.DocumentType).Returns("1");
			entryLines1.Setup(m => m.InboundDate).Returns(new ZDate("2013-01-01"));
			entryLines1.Setup(m => m.PreviousTransactionReferenceNo).Returns("010151234567001999");
			entryLines1.Setup(m => m.PreviousTransactionReferenceNoType).Returns("01");
			entryLines1.Setup(m => m.MaterialCode).Returns(ZString.Empty);

			var entryLines2 = new Mock<ILocalExportEntryLine>();
			entryLines2.Setup(m => m.EntryLineNo).Returns(2);
			entryLines2.Setup(m => m.HSCode).Returns("1234567891");
			entryLines2.Setup(m => m.InvoiceDescription).Returns("STAINLESS STEEL2");
			entryLines2.Setup(m => m.GoodsNo).Returns("물품식별번호2");
			entryLines2.Setup(m => m.Quantity).Returns(999m);
			entryLines2.Setup(m => m.QuantityUnit).Returns("KG");
			entryLines2.Setup(m => m.Packages).Returns(9);
			entryLines2.Setup(m => m.PackagesType).Returns("VL");
			entryLines2.Setup(m => m.NetWeight).Returns(999m);
			entryLines2.Setup(m => m.FOBAmount).Returns(999m);
			entryLines2.Setup(m => m.DocumentNo).Returns("L172770925458");
			entryLines2.Setup(m => m.DocumentType).Returns("2");
			entryLines2.Setup(m => m.InboundDate).Returns(new ZDate("2013-01-02"));
			entryLines2.Setup(m => m.PreviousTransactionReferenceNo).Returns("010151234567001990");
			entryLines2.Setup(m => m.PreviousTransactionReferenceNoType).Returns("02");
			entryLines2.Setup(m => m.MaterialCode).Returns(ZString.Empty);

			localExportHeaderMock.Setup(m => m.EntryLines).Returns(new ILocalExportEntryLine[] { entryLines1.Object, entryLines2.Object });
			#endregion

			#region OtherTransportMeans
			var otherTransportMeans1 = new Mock<ILocalExportOtherTransportMeans>();
			otherTransportMeans1.Setup(m => m.SequenceNo).Returns(1);
			otherTransportMeans1.Setup(m => m.WorkingVesselName).Returns("작업선박명");
			otherTransportMeans1.Setup(m => m.WorkingVesselLloydsNumber).Returns("선박번호");
			otherTransportMeans1.Setup(m => m.TransportVehicleRegNo).Returns("운송차량번호");

			var otherTransportMeans2 = new Mock<ILocalExportOtherTransportMeans>();
			otherTransportMeans2.Setup(m => m.SequenceNo).Returns(2);
			otherTransportMeans2.Setup(m => m.WorkingVesselName).Returns("작업선박명2");
			otherTransportMeans2.Setup(m => m.WorkingVesselLloydsNumber).Returns("선박번호2");
			otherTransportMeans2.Setup(m => m.TransportVehicleRegNo).Returns("운송차량번호2");

			localExportHeaderMock.Setup(m => m.OtherTransportMeans).Returns(new ILocalExportOtherTransportMeans[] { otherTransportMeans1.Object, otherTransportMeans2.Object });
			#endregion

			#region Importer
			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.CompanyName).Returns("무한상사");
			importerMock.Setup(m => m.RepresentativeName).Returns("김대표");

			importerMock.Setup(m => m.AddressLine1).Returns("기본주소");
			importerMock.Setup(m => m.AddressLine2).Returns("상세주소");
			importerMock.Setup(m => m.Postcode).Returns("32012");
			importerMock.Setup(m => m.RoadNameCode).Returns("110001");
			importerMock.Setup(m => m.BuildingNumber).Returns("121200");
			importerMock.Setup(m => m.BusinessRegNo).Returns("1200020212");

			localExportHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);
			#endregion

			var result = new GOVCBR5DQMessageBuilder(localExportHeaderMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.LocalExportOutgoingTestFilePath, "GOVCBR5DQ_CUS.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}
			AssertEquals("01010", result.DeclarationOfficeId.Value);
			AssertEquals("9", result.FunctionCode.Value);
			AssertEquals(2m, result.GoodsItemQuantity.Value);
			AssertEquals("1083699012345", result.Id.Value);
			AssertEquals(10998m, result.InvoiceAmount.Value);
			AssertEquals(900m, result.TotalGrossMassMeasure.Value);
			AssertEquals(100m, result.TotalPackageQuantity.Value);
			AssertEquals("GOVCBR5DQ", result.TypeCode.Value);
			AssertEquals("07", result.TransactionNatureCode.Value);
			AssertEquals("1", result.AdditionalInformation.StatementCode.Value);
			AssertEquals("1234567890", result.Agent.Id.Value);
			AssertEquals(15m, result.BorderTransportMeans.CrewQuantity.Value);
			AssertEquals("A01B2", result.BorderTransportMeans.Id.Value);
			AssertEquals("홍길동호", result.BorderTransportMeans.Name.Value);
			AssertEquals(24m, result.BorderTransportMeans.VoyageDatesNumeric);
			AssertEquals(1m, result.BorderTransportMeans.PersonOnBoard[0].SequenceNumeric);
			AssertEquals("Hong-Gil-Dong", result.BorderTransportMeans.PersonOnBoard[1].GivenName.Value);
			AssertEquals("19910506", result.BorderTransportMeans.PersonOnBoard[0].BirthDateTime);
			AssertEquals("110001", result.BorderTransportMeans.PersonOnBoard[0].Address.CountrySubDivisionId.Value);
			AssertEquals("상세주소2", result.BorderTransportMeans.PersonOnBoard[1].Address.Line.Value);
			AssertEquals("43012", result.BorderTransportMeans.PersonOnBoard[0].Address.PostcodeId.Value);
			AssertEquals("121200", result.BorderTransportMeans.PersonOnBoard[0].Address.BuildingNumber.Value);
			AssertEquals("기본주소2", result.BorderTransportMeans.PersonOnBoard[1].Address.Description.Value);
			AssertEquals("1200020212", result.Consignment.Consignee.Id.Value);
			AssertEquals("무한상사", result.Consignment.Consignee.Name.Value);
			AssertEquals("110001", result.Consignment.Consignee.Address.CountrySubDivisionId.Value);
			AssertEquals("상세주소", result.Consignment.Consignee.Address.Line.Value);
			AssertEquals("32012", result.Consignment.Consignee.Address.PostcodeId.Value);
			AssertEquals("121200", result.Consignment.Consignee.Address.BuildingNumber.Value);
			AssertEquals("기본주소", result.Consignment.Consignee.Address.Description.Value);
			AssertEquals("김대표", result.Consignment.Consignee.Contact.RepresentativeName.Value);
			AssertEquals(1m, result.Consignment.ConsignmentItem[0].SequenceNumeric);
			AssertEquals("1", result.Consignment.ConsignmentItem[0].AdditionalDocument.TypeCode.Value);
			AssertEquals("L172770925459", result.Consignment.ConsignmentItem[0].AdditionalDocument.Id.Value);
			AssertEquals("STAINLESS STEEL2", result.Consignment.ConsignmentItem[1].Commodity.CargoDescription.Value);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].Commodity.CountQuantity.Value);
			AssertEquals("KG", result.Consignment.ConsignmentItem[0].Commodity.CountQuantity.KcsUnitCode);
			AssertEquals("물품식별번호2", result.Consignment.ConsignmentItem[1].Commodity.Id.Value);
			AssertEquals("20130101", result.Consignment.ConsignmentItem[0].Commodity.JurisdictionDateTime);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].Commodity.ValueAmount.Value);
			AssertEquals("010151234567001990", result.Consignment.ConsignmentItem[1].Commodity.AdditionalDocument.Id.Value);
			AssertEquals("01", result.Consignment.ConsignmentItem[0].Commodity.AdditionalDocument.TypeCode.Value);
			AssertEquals("1234567890", result.Consignment.ConsignmentItem[0].Commodity.Classification.Id.Value);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals(99m, result.Consignment.ConsignmentItem[0].Packaging.QuantityQuantity.Value);
			AssertEquals("VL", result.Consignment.ConsignmentItem[0].Packaging.TypeCode.Value);
			AssertNull(result.Consignment.ConsignmentItem[0].PreviousDocument);
			AssertEquals("20130101", result.Consignment.Warehouse.ArrivalDateTime);
			AssertEquals("반입장소", result.Consignment.Warehouse.Name.Value);
			AssertEquals(1m, result.GoodsShipment[0].SequenceNumeric);
			AssertEquals("운송차량번호", result.GoodsShipment[0].Consignment.BorderTransportMeans.Id[0].Value);
			AssertEquals("선박번호2", result.GoodsShipment[1].Consignment.BorderTransportMeans.Id[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Mlt, result.GoodsShipment[0].Consignment.BorderTransportMeans.Id[0].SchemeAgencyId);
			AssertEquals(AgencyIdentificationCodeContentType.Item54, result.GoodsShipment[0].Consignment.BorderTransportMeans.Id[1].SchemeAgencyId);
			AssertEquals("작업선박명2", result.GoodsShipment[1].Consignment.BorderTransportMeans.Name.Value);

			AssertEquals("통관고유부호", result.Manufacturer[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Manufacturer[0].SchemeAgencyId);
			AssertEquals("사업자등록번호", result.Manufacturer[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Ktx, result.Manufacturer[1].SchemeAgencyId);

			AssertEquals("00000000000", result.PreviousDocument.Id.Value);
			AssertEquals("통관고유부호", result.Submitter.Id[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Submitter.Id[0].SchemeAgencyId);
			AssertEquals("사업자등록번호", result.Submitter.Id[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Ktx, result.Submitter.Id[1].SchemeAgencyId);
			AssertEquals("1", result.Submitter.RoleCode.Value);

			localExportHeaderMock.VerifyAll();
			exporterMock.VerifyAll();
			manufacturerMock.VerifyAll();
			borderTransportMeans1.VerifyAll();
			borderTransportMeans2.VerifyAll();
			entryLines1.VerifyAll();
			entryLines2.VerifyAll();
			otherTransportMeans1.VerifyAll();
			otherTransportMeans2.VerifyAll();
			importerMock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDeclaration_TestWithNull()
		{
			localExportHeaderMock.Setup(m => m.VesselRadioCallSign).Returns(ZString.Empty);
			localExportHeaderMock.Setup(m => m.CrewCount).Returns(ZInt.Zero);
			localExportHeaderMock.Setup(m => m.ScheduledSailingDays).Returns(ZInt.Zero);
			localExportHeaderMock.Setup(m => m.BondedAreaCode).Returns(ZString.Empty);
			localExportHeaderMock.Setup(m => m.DeclarationDate).Returns(ZDate.Empty);

			#region Stevedores
			var borderTransportMeans1 = new Mock<ILocalExportStevedore>();
			borderTransportMeans1.Setup(m => m.SequenceNo).Returns(12);
			borderTransportMeans1.Setup(m => m.Birthday).Returns(new ZDate("1991-05-06"));
			borderTransportMeans1.Setup(m => m.FullName).Returns("홍길동");
			borderTransportMeans1.Setup(m => m.RoadNameCode).Returns(ZString.Empty);
			borderTransportMeans1.Setup(m => m.BuildingNumber).Returns(ZString.Empty);
			borderTransportMeans1.Setup(m => m.Postcode).Returns(ZString.Empty);
			borderTransportMeans1.Setup(m => m.AddressLine1).Returns("기본주소");
			borderTransportMeans1.Setup(m => m.AddressLine2).Returns(ZString.Empty);

			var borderTransportMeans2 = new Mock<ILocalExportStevedore>();
			borderTransportMeans2.Setup(m => m.SequenceNo).Returns(13);
			borderTransportMeans2.Setup(m => m.Birthday).Returns(new ZDate("1991-06-06"));
			borderTransportMeans2.Setup(m => m.FullName).Returns("Hong-Gil-Dong");
			borderTransportMeans2.Setup(m => m.RoadNameCode).Returns(ZString.Empty);
			borderTransportMeans2.Setup(m => m.BuildingNumber).Returns(ZString.Empty);
			borderTransportMeans2.Setup(m => m.Postcode).Returns(ZString.Empty);
			borderTransportMeans2.Setup(m => m.AddressLine1).Returns("기본주소2");
			borderTransportMeans2.Setup(m => m.AddressLine2).Returns(ZString.Empty);

			localExportHeaderMock.Setup(m => m.Stevedores).Returns(new ILocalExportStevedore[] { borderTransportMeans1.Object, borderTransportMeans2.Object });
			#endregion

			#region EntryLines
			var entryLines1 = new Mock<ILocalExportEntryLine>();
			entryLines1.Setup(m => m.EntryLineNo).Returns(123);
			entryLines1.Setup(m => m.HSCode).Returns("1234567890");
			entryLines1.Setup(m => m.InvoiceDescription).Returns("STAINLESS STEEL");
			entryLines1.Setup(m => m.GoodsNo).Returns(ZString.Empty);
			entryLines1.Setup(m => m.Quantity).Returns(9999m);
			entryLines1.Setup(m => m.QuantityUnit).Returns("KG");
			entryLines1.Setup(m => m.Packages).Returns(99);
			entryLines1.Setup(m => m.PackagesType).Returns("VL");
			entryLines1.Setup(m => m.NetWeight).Returns(9999m);
			entryLines1.Setup(m => m.FOBAmount).Returns(9999m);
			entryLines1.Setup(m => m.DocumentNo).Returns("L172770925459");
			entryLines1.Setup(m => m.DocumentType).Returns("01");
			entryLines1.Setup(m => m.InboundDate).Returns(ZDate.Empty);
			entryLines1.Setup(m => m.PreviousTransactionReferenceNo).Returns(ZString.Empty);
			entryLines1.Setup(m => m.PreviousTransactionReferenceNoType).Returns(ZString.Empty);
			entryLines1.Setup(m => m.MaterialCode).Returns(ZString.Empty);

			var entryLines2 = new Mock<ILocalExportEntryLine>();
			entryLines2.Setup(m => m.EntryLineNo).Returns(1234);
			entryLines2.Setup(m => m.HSCode).Returns("1234567891");
			entryLines2.Setup(m => m.InvoiceDescription).Returns("STAINLESS STEEL2");
			entryLines2.Setup(m => m.GoodsNo).Returns(ZString.Empty);
			entryLines2.Setup(m => m.Quantity).Returns(999m);
			entryLines2.Setup(m => m.QuantityUnit).Returns("KG");
			entryLines2.Setup(m => m.Packages).Returns(9);
			entryLines2.Setup(m => m.PackagesType).Returns("VL");
			entryLines2.Setup(m => m.NetWeight).Returns(999m);
			entryLines2.Setup(m => m.FOBAmount).Returns(999m);
			entryLines2.Setup(m => m.DocumentNo).Returns("L172770925458");
			entryLines2.Setup(m => m.DocumentType).Returns("02");
			entryLines2.Setup(m => m.InboundDate).Returns(ZDate.Empty);
			entryLines2.Setup(m => m.PreviousTransactionReferenceNo).Returns(ZString.Empty);
			entryLines2.Setup(m => m.PreviousTransactionReferenceNoType).Returns(ZString.Empty);
			entryLines2.Setup(m => m.MaterialCode).Returns(ZString.Empty);

			localExportHeaderMock.Setup(m => m.EntryLines).Returns(new ILocalExportEntryLine[] { entryLines1.Object, entryLines2.Object });
			#endregion

			#region OtherTransportMeans
			var otherTransportMeans1 = new Mock<ILocalExportOtherTransportMeans>();
			otherTransportMeans1.Setup(m => m.SequenceNo).Returns(1);
			otherTransportMeans1.Setup(m => m.WorkingVesselName).Returns(ZString.Empty);
			otherTransportMeans1.Setup(m => m.WorkingVesselLloydsNumber).Returns(ZString.Empty);
			otherTransportMeans1.Setup(m => m.TransportVehicleRegNo).Returns(ZString.Empty);

			var otherTransportMeans2 = new Mock<ILocalExportOtherTransportMeans>();
			otherTransportMeans2.Setup(m => m.SequenceNo).Returns(2);
			otherTransportMeans2.Setup(m => m.WorkingVesselName).Returns(ZString.Empty);
			otherTransportMeans2.Setup(m => m.WorkingVesselLloydsNumber).Returns(ZString.Empty);
			otherTransportMeans2.Setup(m => m.TransportVehicleRegNo).Returns(ZString.Empty);

			localExportHeaderMock.Setup(m => m.OtherTransportMeans).Returns(new ILocalExportOtherTransportMeans[] { otherTransportMeans1.Object, otherTransportMeans2.Object });
			#endregion

			#region Importer
			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.CompanyName).Returns("무한상사");
			importerMock.Setup(m => m.RepresentativeName).Returns("김대표");

			importerMock.Setup(m => m.AddressLine1).Returns(ZString.Empty);
			importerMock.Setup(m => m.AddressLine2).Returns(ZString.Empty);
			importerMock.Setup(m => m.Postcode).Returns(ZString.Empty);
			importerMock.Setup(m => m.RoadNameCode).Returns(ZString.Empty);
			importerMock.Setup(m => m.BuildingNumber).Returns(ZString.Empty);
			importerMock.Setup(m => m.BusinessRegNo).Returns("1200020212");

			localExportHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);
			#endregion

			var result = new GOVCBR5DQMessageBuilder(localExportHeaderMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.LocalExportOutgoingTestFilePath, "GOVCBR5DQ_Empty.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}
			AssertEquals("01010", result.DeclarationOfficeId.Value);
			AssertEquals("9", result.FunctionCode.Value);
			AssertEquals(2m, result.GoodsItemQuantity.Value);
			AssertEquals("1083699012345", result.Id.Value);
			AssertEquals(10998m, result.InvoiceAmount.Value);
			AssertEquals(900m, result.TotalGrossMassMeasure.Value);
			AssertEquals(100m, result.TotalPackageQuantity.Value);
			AssertEquals("GOVCBR5DQ", result.TypeCode.Value);
			AssertEquals("07", result.TransactionNatureCode.Value);
			AssertEquals("1", result.AdditionalInformation.StatementCode.Value);
			AssertNull(result.BorderTransportMeans.CrewQuantity);
			AssertNull(result.BorderTransportMeans.Id);
			AssertEquals("홍길동호", result.BorderTransportMeans.Name.Value);
			AssertNull(result.BorderTransportMeans.VoyageDatesNumeric);
			AssertEquals(12m, result.BorderTransportMeans.PersonOnBoard[0].SequenceNumeric);
			AssertEquals("Hong-Gil-Dong", result.BorderTransportMeans.PersonOnBoard[1].GivenName.Value);
			AssertEquals("19910506", result.BorderTransportMeans.PersonOnBoard[0].BirthDateTime);
			AssertNull(result.BorderTransportMeans.PersonOnBoard[0].Address.CountrySubDivisionId);
			AssertNull(result.BorderTransportMeans.PersonOnBoard[0].Address.Line);
			AssertNull(result.BorderTransportMeans.PersonOnBoard[0].Address.PostcodeId);
			AssertNull(result.BorderTransportMeans.PersonOnBoard[0].Address.BuildingNumber);
			AssertEquals("기본주소", result.BorderTransportMeans.PersonOnBoard[0].Address.Description.Value);
			AssertEquals("1200020212", result.Consignment.Consignee.Id.Value);
			AssertEquals("무한상사", result.Consignment.Consignee.Name.Value);
			AssertNull(result.Consignment.Consignee.Address);
			AssertEquals("김대표", result.Consignment.Consignee.Contact.RepresentativeName.Value);
			AssertEquals(123m, result.Consignment.ConsignmentItem[0].SequenceNumeric);
			AssertEquals("01", result.Consignment.ConsignmentItem[0].AdditionalDocument.TypeCode.Value);
			AssertEquals("L172770925458", result.Consignment.ConsignmentItem[1].AdditionalDocument.Id.Value);
			AssertEquals("STAINLESS STEEL2", result.Consignment.ConsignmentItem[1].Commodity.CargoDescription.Value);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].Commodity.CountQuantity.Value);
			AssertEquals("KG", result.Consignment.ConsignmentItem[0].Commodity.CountQuantity.KcsUnitCode);
			AssertNull(result.Consignment.ConsignmentItem[0].Commodity.Id);
			AssertNull(result.Consignment.ConsignmentItem[0].Commodity.JurisdictionDateTime);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].Commodity.ValueAmount.Value);
			AssertNull(result.Consignment.ConsignmentItem[0].Commodity.AdditionalDocument);
			AssertEquals("1234567891", result.Consignment.ConsignmentItem[1].Commodity.Classification.Id.Value);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals(99m, result.Consignment.ConsignmentItem[0].Packaging.QuantityQuantity.Value);
			AssertEquals("VL", result.Consignment.ConsignmentItem[0].Packaging.TypeCode.Value);
			AssertNull(result.Consignment.ConsignmentItem[0].PreviousDocument);
			AssertNull(result.Consignment.Warehouse);
			AssertEquals(1m, result.GoodsShipment[0].SequenceNumeric);
			AssertNull(result.GoodsShipment[0].Consignment);
			AssertNull(result.Agent);

			AssertEquals("00000000000", result.PreviousDocument.Id.Value);
			AssertEquals("통관고유부호", result.Submitter.Id[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Submitter.Id[0].SchemeAgencyId);
			AssertEquals("사업자등록번호", result.Submitter.Id[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Ktx, result.Submitter.Id[1].SchemeAgencyId);
			AssertEquals("1", result.Submitter.RoleCode.Value);

			localExportHeaderMock.VerifyAll();
			borderTransportMeans1.VerifyAll();
			borderTransportMeans2.VerifyAll();
			entryLines1.VerifyAll();
			entryLines2.VerifyAll();
			otherTransportMeans1.VerifyAll();
			otherTransportMeans2.VerifyAll();
			importerMock.VerifyAll();
		}

		public void TestArrivalDateTimeAndJurisdictionDateTime()
		{
			var localExportHeaderMock = new Mock<ILocalExportEntryHeader>();
			var entryLines = new Mock<ILocalExportEntryLine>();
			entryLines.Setup(m => m.InboundDate).Returns(ZDate.Empty);
			localExportHeaderMock.Setup(m => m.EntryLines).Returns(new ILocalExportEntryLine[] { entryLines.Object });
			var result = new GOVCBR5DQMessageBuilder(localExportHeaderMock.Object).GenerateMessage();
			AssertNull(result.Consignment.Warehouse);
			AssertNull(result.Consignment.ConsignmentItem[0].Commodity.JurisdictionDateTime);

			localExportHeaderMock.Setup(m => m.DeclarationDate).Returns(new ZDate("2022-11-01"));
			entryLines.Setup(m => m.InboundDate).Returns(new ZDate("2022-11-02"));

			result = new GOVCBR5DQMessageBuilder(localExportHeaderMock.Object).GenerateMessage();
			AssertEquals("20221101", result.Consignment.Warehouse.ArrivalDateTime);
			AssertEquals("20221102", result.Consignment.ConsignmentItem[0].Commodity.JurisdictionDateTime);
		}

		public void TestExporterRegistrationNo()
		{
			var result = new GOVCBR5DQMessageBuilder(localExportHeaderMock.Object).GenerateMessage();
			AssertNull(result.Agent);

			var exporterMock = new Mock<IOrganization>();
			localExportHeaderMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			result = new GOVCBR5DQMessageBuilder(localExportHeaderMock.Object).GenerateMessage();
			AssertNull(result.Agent);

			exporterMock.Setup(m => m.BusinessRegNo).Returns("1234567890");
			localExportHeaderMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			result = new GOVCBR5DQMessageBuilder(localExportHeaderMock.Object).GenerateMessage();
			AssertEquals("1234567890", result.Agent.Id.Value);
		}
	}
}
