using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBRDHRMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImportDHRHeader> fTAHeaderMock;

		protected override void SetUp()
		{
			base.SetUp();

			fTAHeaderMock = new Mock<IImportDHRHeader>();
			fTAHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("4062001070010I");
			fTAHeaderMock.Setup(m => m.LawCode).Returns("1");
			fTAHeaderMock.Setup(m => m.StatementNumber5WN).Returns("040112050000001");
			fTAHeaderMock.Setup(m => m.DepartureCountryCode).Returns("JP");
			fTAHeaderMock.Setup(m => m.DeparturePort).Returns("선적항명");
			fTAHeaderMock.Setup(m => m.TransshipmentYN).Returns("Y");
			fTAHeaderMock.Setup(m => m.TransshipmentCountryCode).Returns("JP");
			fTAHeaderMock.Setup(m => m.TransshipmentPort).Returns("환적항");
			fTAHeaderMock.Setup(m => m.DepartureDate).Returns(new ZDate("2015-11-30"));
			fTAHeaderMock.Setup(m => m.TransshipmentDate).Returns(new ZDate("2015-11-30"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 02, 02)]
		public void TestGenerateDeclaration_FullData()
		{
			var fTAHeaderlineMock1 = new Mock<IImportFTALine>();
			fTAHeaderlineMock1.Setup(m => m.EntryLineNo).Returns(001);
			fTAHeaderlineMock1.Setup(m => m.SequenceNo).Returns(1);
			fTAHeaderlineMock1.Setup(m => m.HSCode).Returns("000000");
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginProductType).Returns("A");
			fTAHeaderlineMock1.Setup(m => m.AdditionalInvoiceIssuedInThirdCountryYN).Returns("Y");
			fTAHeaderlineMock1.Setup(m => m.AdditionalInvoiceIssuingThirdCountryCode).Returns("JP");
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginExporterNumber).Returns("000000010");
			fTAHeaderlineMock1.Setup(m => m.CountryOfOrigin).Returns("JP");
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginIssueDate).Returns(new ZDate("2015-11-30"));
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginTotalNetWeight).Returns(123);
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginSplitOrder).Returns(3);
			fTAHeaderlineMock1.Setup(m => m.NetWeight).Returns(123);
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginNo).Returns("K01321065431");
			fTAHeaderlineMock1.Setup(m => m.CertifiticateOfOriginIssuingAgencyType).Returns("1");
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginAgencyName).Returns("기관명");
			fTAHeaderlineMock1.Setup(m => m.CountryOfOriginSupportingDocType).Returns("1");
			fTAHeaderlineMock1.Setup(m => m.CertifiticateOfOriginIssuerType).Returns("1");
			fTAHeaderlineMock1.Setup(m => m.DutyRateCode).Returns("FAS1");
			fTAHeaderlineMock1.Setup(m => m.TariffRate).Returns(3m);

			var fTAHeaderlineMock2 = new Mock<IImportFTALine>();
			fTAHeaderlineMock2.Setup(m => m.EntryLineNo).Returns(002);
			fTAHeaderlineMock2.Setup(m => m.SequenceNo).Returns(2);
			fTAHeaderlineMock2.Setup(m => m.HSCode).Returns("000000");
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginProductType).Returns("B");
			fTAHeaderlineMock2.Setup(m => m.AdditionalInvoiceIssuedInThirdCountryYN).Returns("Y");
			fTAHeaderlineMock2.Setup(m => m.AdditionalInvoiceIssuingThirdCountryCode).Returns("KR");
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginExporterNumber).Returns("000000010");
			fTAHeaderlineMock2.Setup(m => m.CountryOfOrigin).Returns("KR");
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginIssueDate).Returns(new ZDate("2015-11-30"));
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginTotalNetWeight).Returns(123);
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginSplitOrder).Returns(3);
			fTAHeaderlineMock2.Setup(m => m.NetWeight).Returns(123);
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginNo).Returns("K01321065431");
			fTAHeaderlineMock2.Setup(m => m.CertifiticateOfOriginIssuingAgencyType).Returns("2");
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginAgencyName).Returns("기관명");
			fTAHeaderlineMock2.Setup(m => m.CountryOfOriginSupportingDocType).Returns("2");
			fTAHeaderlineMock2.Setup(m => m.CertifiticateOfOriginIssuerType).Returns("2");
			fTAHeaderlineMock2.Setup(m => m.DutyRateCode).Returns("FAS1");
			fTAHeaderlineMock2.Setup(m => m.TariffRate).Returns(3m);

			fTAHeaderMock.Setup(m => m.EntryLines).Returns(new IImportFTALine[] { fTAHeaderlineMock1.Object, fTAHeaderlineMock2.Object });

			var invoiceLineMock1 = new Mock<IImportDHRInvoiceLine>();
			invoiceLineMock1.Setup(m => m.EntryLineNo).Returns(001);
			invoiceLineMock1.Setup(m => m.InvoiceLineNo).Returns(01);
			invoiceLineMock1.Setup(m => m.CertificateOfOriginNo).Returns("CNX1234567890");
			invoiceLineMock1.Setup(m => m.CertificateOfOriginSeqNo).Returns(01);
			invoiceLineMock1.Setup(m => m.CertificateOfOriginUsedQuantity).Returns(500m);
			invoiceLineMock1.Setup(m => m.CertificateOfOriginUsedUQ).Returns("PCS");

			var invoiceLineMock2 = new Mock<IImportDHRInvoiceLine>();
			invoiceLineMock2.Setup(m => m.EntryLineNo).Returns(002);
			invoiceLineMock2.Setup(m => m.InvoiceLineNo).Returns(02);
			invoiceLineMock2.Setup(m => m.CertificateOfOriginNo).Returns("CNX1234567890");
			invoiceLineMock2.Setup(m => m.CertificateOfOriginSeqNo).Returns(02);
			invoiceLineMock2.Setup(m => m.CertificateOfOriginUsedQuantity).Returns(500m);
			invoiceLineMock2.Setup(m => m.CertificateOfOriginUsedUQ).Returns("PCS");

			fTAHeaderMock.Setup(m => m.DHRInvoiceLines).Returns(new IImportDHRInvoiceLine[] { invoiceLineMock1.Object, invoiceLineMock2.Object });

			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.CompanyName).Returns("상호");
			supplierMock.Setup(m => m.RepresentativeName).Returns("수출자성명");
			supplierMock.Setup(m => m.AddressLine1).Returns("주소");
			supplierMock.Setup(m => m.FaxNumber).Returns("0425559999");
			supplierMock.Setup(m => m.PhoneNumber).Returns("0425559999");
			supplierMock.Setup(m => m.CountryCode).Returns("JP");
			fTAHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			var manufacturerMock = new Mock<IOrganization>();
			manufacturerMock.Setup(m => m.CompanyName).Returns("상호");
			manufacturerMock.Setup(m => m.RepresentativeName).Returns("생산자성명");
			manufacturerMock.Setup(m => m.AddressLine1).Returns("주소");
			manufacturerMock.Setup(m => m.FaxNumber).Returns("0421569874");
			manufacturerMock.Setup(m => m.PhoneNumber).Returns("0421569874");
			fTAHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturerMock.Object);

			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.CompanyName).Returns("상호");
			importerMock.Setup(m => m.RepresentativeName).Returns("수입자성명");
			importerMock.Setup(m => m.AddressLine1).Returns("기본주소");
			importerMock.Setup(m => m.AddressLine2).Returns("상세주소");
			importerMock.Setup(m => m.RoadNameCode).Returns("01002");
			importerMock.Setup(m => m.Postcode).Returns("45521");
			importerMock.Setup(m => m.BuildingNumber).Returns("11101");
			importerMock.Setup(m => m.FaxNumber).Returns("0425550011");
			importerMock.Setup(m => m.PhoneNumber).Returns("0425550011");
			importerMock.Setup(m => m.Email).Returns("aaa@bbb.com");
			importerMock.Setup(m => m.BusinessRegNo).Returns("4444444444");
			importerMock.Setup(m => m.UnipassIDForOrganization).Returns("6666666666");

			fTAHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);

			var result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBRDHR_Result.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();
					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("4062001070010I", result.Id.Value);
			AssertEquals("20200202", result.IssueDateTime);
			AssertEquals("GOVCBRDHR", result.TypeCode.Value);
			AssertEquals("1", result.AdditionalDocument.TypeCode.Value);
			AssertEquals("040112050000001", result.AdditionalDocument.Id.Value);

			AssertEquals("2", result.Consignment[1].SequenceId.Value);
			AssertEquals("CNX1234567890", result.Consignment[0].AdditionalDocument.Id.Value);
			AssertEquals("2", result.Consignment[1].AdditionalDocument.CriteriaConformanceId.Value);
			AssertEquals(500M, result.Consignment[0].ConsignmentItem.Commodity.CountQuantity.Value);
			AssertEquals("1", result.Consignment[0].ConsignmentItem.Commodity.IdentityQualifierCode.Value);

			AssertEquals("상호", result.Exporter.Name.Value);
			AssertEquals("JP", result.Exporter.Address.CountryCode.Value);
			AssertEquals("주소", result.Exporter.Address.Line.Value);
			AssertEquals("수출자성명", result.Exporter.Contact.Name.Value);
			AssertEquals("0425559999", result.Exporter.Communication[0].Id.Value);
			AssertEquals("0425559999", result.Exporter.Communication[1].Id.Value);

			AssertEquals(1m, result.GoodsShipment[0].SequenceNumeric);
			AssertEquals("JP", result.GoodsShipment[1].ExportationCountryCode.Value);
			AssertEquals(1m, result.GoodsShipment[0].AdditionalDocument.SequenceNumeric);
			AssertEquals("20151130", result.GoodsShipment[1].Consignment.BorderTransportMeans.DepartureDateTime);
			AssertEquals("20151130", result.GoodsShipment[0].Consignment.BorderTransportMeans.Itinerary.DepartureDateTime);
			AssertEquals("FAS1", result.GoodsShipment[1].Consignment.DutyTaxFee.DutyRegimeCode.Value);
			AssertEquals(3m, result.GoodsShipment[0].Consignment.DutyTaxFee.TaxRateNumeric);
			AssertEquals("선적항명", result.GoodsShipment[1].Consignment.LoadingLocation.Name.Value);
			AssertEquals("JP", result.GoodsShipment[1].Consignment.TranshipmentLocation.Id.Value);
			AssertEquals("환적항", result.GoodsShipment[0].Consignment.TranshipmentLocation.Name.Value);
			AssertEquals("JP", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalDocument.IssueLocationId.Value);
			AssertEquals("1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalInformation.StatementCode.Value);
			AssertEquals("1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalInformation.StatementTypeCode.Value);
			AssertEquals("20151130", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueDateTime);
			AssertEquals("K01321065431", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueId.Value);
			AssertEquals("기관명", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueAgencyName.Value);
			AssertEquals(3M, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.SplitVersionNumeric);
			AssertEquals("1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueAgencyTypeCode.Value);

			AssertEquals("000000", result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.Classification.Id.Value);
			AssertEquals(123M, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity.WeightMeasure.Value);
			AssertEquals(123M, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals("KR", result.GoodsShipment[1].GovernmentAgencyGoodsItem.Origin.CountryCode.Value);
			AssertEquals("A", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Origin.RuleCode.Value);

			AssertEquals("4444444444", result.Importer.Id[0].Value);
			AssertEquals("6666666666", result.Importer.Id[1].Value);
			AssertEquals("상호", result.Importer.Name.Value);
			AssertEquals("기본주소", result.Importer.Address.Description.Value);
			AssertEquals("상세주소", result.Importer.Address.Line.Value);
			AssertEquals("01002", result.Importer.Address.CountrySubDivisionId.Value);
			AssertEquals("11101", result.Importer.Address.BuildingNumber.Value);
			AssertEquals("45521", result.Importer.Address.PostcodeId.Value);
			AssertEquals("수입자성명", result.Importer.Contact.Name.Value);
			AssertEquals("0425550011", result.Importer.Communication[0].Id.Value);
			AssertEquals("0425550011", result.Importer.Communication[1].Id.Value);
			AssertEquals("aaa@bbb.com", result.Importer.Communication[2].Id.Value);

			AssertEquals("상호", result.Manufacturer.Name.Value);
			AssertEquals("주소", result.Manufacturer.Address.Line.Value);
			AssertEquals("생산자성명", result.Manufacturer.Contact.Name.Value);
			AssertEquals("0421569874", result.Manufacturer.Communication[0].Id.Value);
			AssertEquals("0421569874", result.Manufacturer.Communication[1].Id.Value);
			fTAHeaderMock.VerifyAll();
			fTAHeaderlineMock1.VerifyAll();
			fTAHeaderlineMock2.VerifyAll();
			invoiceLineMock1.VerifyAll();
			invoiceLineMock2.VerifyAll();
			supplierMock.VerifyAll();
			manufacturerMock.VerifyAll();
			importerMock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 02, 02)]
		public void TestGenerateDeclaration_WithTest_Empty()
		{
			fTAHeaderMock.Setup(m => m.LawCode).Returns("");
			fTAHeaderMock.Setup(m => m.StatementNumber5WN).Returns("");
			fTAHeaderMock.Setup(m => m.DepartureCountryCode).Returns("");
			fTAHeaderMock.Setup(m => m.DeparturePort).Returns("");
			fTAHeaderMock.Setup(m => m.TransshipmentYN).Returns("");
			fTAHeaderMock.Setup(m => m.TransshipmentCountryCode).Returns("");
			fTAHeaderMock.Setup(m => m.TransshipmentPort).Returns("");
			fTAHeaderMock.Setup(m => m.DepartureDate).Returns(new ZDate(""));
			fTAHeaderMock.Setup(m => m.TransshipmentDate).Returns(new ZDate(""));

			var fTAHeaderlineMock1 = new Mock<IImportFTALine>();
			fTAHeaderlineMock1.Setup(m => m.EntryLineNo).Returns(001);
			fTAHeaderlineMock1.Setup(m => m.SequenceNo).Returns(1);
			fTAHeaderlineMock1.Setup(m => m.HSCode).Returns("");
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginProductType).Returns("");
			fTAHeaderlineMock1.Setup(m => m.AdditionalInvoiceIssuedInThirdCountryYN).Returns("");
			fTAHeaderlineMock1.Setup(m => m.AdditionalInvoiceIssuingThirdCountryCode).Returns("");
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginExporterNumber).Returns("");
			fTAHeaderlineMock1.Setup(m => m.CountryOfOrigin).Returns("");
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginIssueDate).Returns(new ZDate(""));
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginTotalNetWeight).Returns(0);
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginSplitOrder).Returns(0);
			fTAHeaderlineMock1.Setup(m => m.NetWeight).Returns(0);
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginNo).Returns("K01321065431");
			fTAHeaderlineMock1.Setup(m => m.CertifiticateOfOriginIssuingAgencyType).Returns("");
			fTAHeaderlineMock1.Setup(m => m.CertificateOfOriginAgencyName).Returns("");
			fTAHeaderlineMock1.Setup(m => m.CountryOfOriginSupportingDocType).Returns("1");
			fTAHeaderlineMock1.Setup(m => m.CertifiticateOfOriginIssuerType).Returns("1");
			fTAHeaderlineMock1.Setup(m => m.DutyRateCode).Returns("FAS1");
			fTAHeaderlineMock1.Setup(m => m.TariffRate).Returns(3m);

			var fTAHeaderlineMock2 = new Mock<IImportFTALine>();
			fTAHeaderlineMock2.Setup(m => m.EntryLineNo).Returns(002);
			fTAHeaderlineMock2.Setup(m => m.SequenceNo).Returns(2);
			fTAHeaderlineMock2.Setup(m => m.HSCode).Returns("");
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginProductType).Returns("");
			fTAHeaderlineMock2.Setup(m => m.AdditionalInvoiceIssuedInThirdCountryYN).Returns("");
			fTAHeaderlineMock2.Setup(m => m.AdditionalInvoiceIssuingThirdCountryCode).Returns("");
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginExporterNumber).Returns("");
			fTAHeaderlineMock2.Setup(m => m.CountryOfOrigin).Returns("");
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginIssueDate).Returns(new ZDate(""));
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginTotalNetWeight).Returns(0);
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginSplitOrder).Returns(0);
			fTAHeaderlineMock2.Setup(m => m.NetWeight).Returns(0);
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginNo).Returns("K01321065431");
			fTAHeaderlineMock2.Setup(m => m.CertifiticateOfOriginIssuingAgencyType).Returns("");
			fTAHeaderlineMock2.Setup(m => m.CertificateOfOriginAgencyName).Returns("");
			fTAHeaderlineMock2.Setup(m => m.CountryOfOriginSupportingDocType).Returns("2");
			fTAHeaderlineMock2.Setup(m => m.CertifiticateOfOriginIssuerType).Returns("2");
			fTAHeaderlineMock2.Setup(m => m.DutyRateCode).Returns("FAS1");
			fTAHeaderlineMock2.Setup(m => m.TariffRate).Returns(3m);

			fTAHeaderMock.Setup(m => m.EntryLines).Returns(new IImportFTALine[] { fTAHeaderlineMock1.Object, fTAHeaderlineMock2.Object });

			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.CompanyName).Returns("상호");
			supplierMock.Setup(m => m.RepresentativeName).Returns("수출자성명");
			supplierMock.Setup(m => m.AddressLine1).Returns("주소");
			supplierMock.Setup(m => m.FaxNumber).Returns("");
			supplierMock.Setup(m => m.PhoneNumber).Returns("0425559999");
			supplierMock.Setup(m => m.CountryCode).Returns("JP");
			fTAHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			var manufacturerMock = new Mock<IOrganization>();
			manufacturerMock.Setup(m => m.CompanyName).Returns("상호");
			manufacturerMock.Setup(m => m.RepresentativeName).Returns("생산자성명");
			manufacturerMock.Setup(m => m.AddressLine1).Returns("주소");
			manufacturerMock.Setup(m => m.FaxNumber).Returns("");
			manufacturerMock.Setup(m => m.PhoneNumber).Returns("");
			fTAHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturerMock.Object);

			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.CompanyName).Returns("상호");
			importerMock.Setup(m => m.RepresentativeName).Returns("수입자성명");
			importerMock.Setup(m => m.AddressLine1).Returns("기본주소");
			importerMock.Setup(m => m.AddressLine2).Returns("");
			importerMock.Setup(m => m.RoadNameCode).Returns("");
			importerMock.Setup(m => m.Postcode).Returns("45521");
			importerMock.Setup(m => m.BuildingNumber).Returns("");
			importerMock.Setup(m => m.FaxNumber).Returns("0425550011");
			importerMock.Setup(m => m.PhoneNumber).Returns("0425550011");
			importerMock.Setup(m => m.Email).Returns("aaa@bbb.com");
			importerMock.Setup(m => m.BusinessRegNo).Returns("4444444444");
			importerMock.Setup(m => m.UnipassIDForOrganization).Returns("");

			fTAHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);

			var result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBRDHR_Result_Empty.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("4062001070010I", result.Id.Value);
			AssertEquals("20200202", result.IssueDateTime);
			AssertEquals("GOVCBRDHR", result.TypeCode.Value);

			AssertEquals("상호", result.Exporter.Name.Value);
			AssertEquals("JP", result.Exporter.Address.CountryCode.Value);
			AssertEquals("주소", result.Exporter.Address.Line.Value);
			AssertEquals("수출자성명", result.Exporter.Contact.Name.Value);
			AssertEquals("0425559999", result.Exporter.Communication[0].Id.Value);
			AssertEquals("", result.Exporter.Communication[1].Id.Value);

			AssertEquals(1m, result.GoodsShipment[0].SequenceNumeric);
			AssertEquals(1m, result.GoodsShipment[0].AdditionalDocument.SequenceNumeric);
			AssertEquals("FAS1", result.GoodsShipment[1].Consignment.DutyTaxFee.DutyRegimeCode.Value);
			AssertEquals(3m, result.GoodsShipment[0].Consignment.DutyTaxFee.TaxRateNumeric);
			AssertEquals("1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalInformation.StatementCode.Value);
			AssertEquals("1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalInformation.StatementTypeCode.Value);
			AssertEquals("K01321065431", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueId.Value);
			AssertEquals(null, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.SplitVersionNumeric);

			AssertEquals(1, result.Importer.Id.Count);
			AssertEquals("4444444444", result.Importer.Id[0].Value);
			AssertEquals("상호", result.Importer.Name.Value);
			AssertEquals("기본주소", result.Importer.Address.Description.Value);
			AssertEquals("45521", result.Importer.Address.PostcodeId.Value);
			AssertEquals("수입자성명", result.Importer.Contact.Name.Value);
			AssertEquals("0425550011", result.Importer.Communication[0].Id.Value);
			AssertEquals("0425550011", result.Importer.Communication[1].Id.Value);
			AssertEquals("aaa@bbb.com", result.Importer.Communication[2].Id.Value);

			AssertEquals("상호", result.Manufacturer.Name.Value);
			AssertEquals("주소", result.Manufacturer.Address.Line.Value);
			AssertEquals("생산자성명", result.Manufacturer.Contact.Name.Value);
			AssertNull(result.Manufacturer.Communication);
			fTAHeaderMock.VerifyAll();
			fTAHeaderlineMock1.VerifyAll();
			fTAHeaderlineMock2.VerifyAll();
			supplierMock.VerifyAll();
			manufacturerMock.VerifyAll();
			importerMock.VerifyAll();
		}

		public void TestEmptyAdditionalDocument()
		{
			fTAHeaderMock = new Mock<IImportDHRHeader>();
			var result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.AdditionalDocument);

			fTAHeaderMock.Setup(m => m.LawCode).Returns("4");
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.AdditionalDocument.TypeCode);
			AssertNull(result.AdditionalDocument.Id);

			fTAHeaderMock.Setup(m => m.LawCode).Returns(ZString.Empty);
			fTAHeaderMock.Setup(m => m.StatementNumber5WN).Returns("040112050000001");
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.AdditionalDocument.TypeCode);
			AssertNotNull(result.AdditionalDocument.Id);
		}

		public void TestEmptyConsignment()
		{
			var invoiceLine = new Mock<IImportDHRInvoiceLine>();
			fTAHeaderMock = new Mock<IImportDHRHeader>();
			var result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.Consignment);

			invoiceLine.Setup(m => m.CertificateOfOriginNo).Returns("CNX1234567890");
			fTAHeaderMock.Setup(m => m.DHRInvoiceLines).Returns(new[] { invoiceLine.Object });
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.Consignment[0].AdditionalDocument.Id);
			AssertNotNull(result.Consignment[0].SequenceId);
			AssertNull(result.Consignment[0].AdditionalDocument.CriteriaConformanceId);
			AssertNull(result.Consignment[0].ConsignmentItem);

			invoiceLine.Setup(m => m.CertificateOfOriginNo).Returns(ZString.Empty);
			invoiceLine.Setup(m => m.CertificateOfOriginSeqNo).Returns(1);
			fTAHeaderMock.Setup(m => m.DHRInvoiceLines).Returns(new[] { invoiceLine.Object });
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.Consignment[0].AdditionalDocument.Id);
			AssertNotNull(result.Consignment[0].AdditionalDocument.CriteriaConformanceId);

			invoiceLine.Setup(m => m.CertificateOfOriginSeqNo).Returns(0);
			invoiceLine.Setup(m => m.CertificateOfOriginUsedQuantity).Returns(1);
			fTAHeaderMock.Setup(m => m.DHRInvoiceLines).Returns(new[] { invoiceLine.Object });
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.Consignment[0].AdditionalDocument);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.CountQuantity);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.IdentityQualifierCode);

			invoiceLine.Setup(m => m.CertificateOfOriginUsedQuantity).Returns(0);
			invoiceLine.Setup(m => m.InvoiceLineNo).Returns(1);
			fTAHeaderMock.Setup(m => m.DHRInvoiceLines).Returns(new[] { invoiceLine.Object });
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.CountQuantity);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.IdentityQualifierCode);
		}

		public void TestEmptyGoodsShipment()
		{
			var entryLine = new Mock<IImportFTALine>();
			fTAHeaderMock = new Mock<IImportDHRHeader>();
			fTAHeaderMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			var result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].ExportationCountryCode);
			AssertNull(result.GoodsShipment[0].TransactionNatureCode);
			AssertNull(result.GoodsShipment[0].AdditionalInformation);
			AssertNull(result.GoodsShipment[0].Exporter);
			AssertNull(result.GoodsShipment[0].Invoice);
		}

		public void TestEmptyGoodsShipmentConsignment()
		{
			var entryLine = new Mock<IImportFTALine>();
			fTAHeaderMock = new Mock<IImportDHRHeader>();
			fTAHeaderMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			var result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].Consignment.BorderTransportMeans);
			AssertNull(result.GoodsShipment[0].Consignment.LoadingLocation);
			AssertNull(result.GoodsShipment[0].Consignment.TranshipmentLocation);

			fTAHeaderMock.Setup(m => m.DepartureDate).Returns(new ZDate("2023-01-01"));
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment[0].Consignment.BorderTransportMeans.DepartureDateTime);
			AssertNull(result.GoodsShipment[0].Consignment.BorderTransportMeans.Itinerary);

			fTAHeaderMock.Setup(m => m.DepartureDate).Returns(null);
			fTAHeaderMock.Setup(m => m.TransshipmentDate).Returns(new ZDate("2023-01-01"));
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].Consignment.BorderTransportMeans.DepartureDateTime);
			AssertNotNull(result.GoodsShipment[0].Consignment.BorderTransportMeans.Itinerary);

			fTAHeaderMock.Setup(m => m.TransshipmentCountryCode).Returns("KR");
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment[0].Consignment.TranshipmentLocation.Id);
			AssertNull(result.GoodsShipment[0].Consignment.TranshipmentLocation.Name);

			fTAHeaderMock.Setup(m => m.TransshipmentCountryCode).Returns(ZString.Empty);
			fTAHeaderMock.Setup(m => m.TransshipmentPort).Returns("환적항");
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].Consignment.TranshipmentLocation.Id);
			AssertNotNull(result.GoodsShipment[0].Consignment.TranshipmentLocation.Name);
		}

		public void TestEmptyGovernmentAgencyGoodsItem()
		{
			var supplier = new Mock<IOrganization>();
			var entryLine = new Mock<IImportFTALine>();
			entryLine.Setup(m => m.SequenceNo).Returns(1);
			var manufacturer = new Mock<IOrganization>();
			fTAHeaderMock = new Mock<IImportDHRHeader>();
			fTAHeaderMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			fTAHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturer.Object);
			fTAHeaderMock.Setup(m => m.Supplier).Returns(supplier.Object);
			var result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalDocument);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalInformation.StatementTypeCode);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueDateTime);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueAgencyName);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.SplitVersionNumeric);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Classification);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.GoodsMeasure);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Producer);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Origin);

			entryLine.Setup(m => m.CountryOfOrigin).Returns("IL");
			manufacturer.Setup(m => m.AddressLine1).Returns("생산지역주소");
			manufacturer.Setup(m => m.AddressLine2).Returns("주소 상세");

			fTAHeaderMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			fTAHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturer.Object);
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Origin.CountryCode);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Origin.RuleCode);
			AssertEquals("생산지역주소 주소 상세", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Producer.Address.Line.Value);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Producer.Address.PostcodeId);

			manufacturer.Setup(m => m.AddressLine1).Returns(ZString.Empty);
			manufacturer.Setup(m => m.Postcode).Returns("14123");
			fTAHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturer.Object);
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Producer.Address.Line);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Producer.Address.PostcodeId);

			entryLine.Setup(m => m.CountryOfOrigin).Returns(ZString.Empty);
			entryLine.Setup(m => m.CertificateOfOriginProductType).Returns("A");

			fTAHeaderMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Origin.CountryCode);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Origin.RuleCode);
		}

		public void TestEmptyImporter()
		{
			fTAHeaderMock = new Mock<IImportDHRHeader>();
			var result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.Importer.Address.CountrySubDivisionId);
			AssertNull(result.Importer.Address.Line);
		}

		public void TestEmptyManufacturer()
		{
			fTAHeaderMock = new Mock<IImportDHRHeader>();
			var result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNull(result.Manufacturer);

			var manufacturer = new Mock<IOrganization>();
			manufacturer.Setup(m => m.PhoneNumber).Returns("010-1234-1234");
			fTAHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturer.Object);
			result = new GOVCBRDHRMessageBuilder(fTAHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.Manufacturer.Communication[0].Id);
			AssertNotNull(result.Manufacturer.Communication[0].TypeId);
		}
	}
}
