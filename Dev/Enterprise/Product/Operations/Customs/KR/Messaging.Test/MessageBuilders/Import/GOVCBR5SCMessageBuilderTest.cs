using System.IO;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5SCMessageBuilderTest : TestCaseWithFactory
	{
		public void TestDepartureDateTime_GoodsItem_BorderTransportMeans()
		{
			SetupMocksV1();
			headerMock.Setup(x => x.DepartureDate).Returns(ZDate.Empty);
			var result = messageBuilder.GenerateMessage();
			AssertNull(result.GoodsShipment[0].Consignment.BorderTransportMeans.DepartureDateTime);
			headerMock.VerifyAll();
			importerMock.VerifyAll();
			line1Mock.VerifyAll();
		}

		public void TestTransshipmentDate_GoodsItem_BorderTransportMeans()
		{
			SetupMocksV1();
			headerMock.Setup(x => x.TransshipmentDate).Returns(ZDate.Empty);
			var result = messageBuilder.GenerateMessage();
			AssertNull(result.GoodsShipment[0].Consignment.BorderTransportMeans.Itinerary);
			headerMock.VerifyAll();
			importerMock.VerifyAll();
			line1Mock.VerifyAll();
		}

		public void TestCertificateOfOriginSplitOrder_Zero()
		{
			SetupMocksV1();
			line1Mock.Setup(x => x.CertificateOfOriginSplitOrder).Returns(ZInt.Zero);
			var result = messageBuilder.GenerateMessage();
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.SplitVersionNumeric);
			headerMock.VerifyAll();
			importerMock.VerifyAll();
			line1Mock.VerifyAll();
		}

		public void TestSupplier_Null()
		{
			SetupMocksV1();
			headerMock.Setup(x => x.Supplier).Returns((IOrganization)null);
			var result = messageBuilder.GenerateMessage();
			AssertNotNull(result.Exporter);
			headerMock.VerifyAll();
			importerMock.VerifyAll();
			line1Mock.VerifyAll();
		}

		public void TestImporter_Null()
		{
			SetupMocks();
			headerMock.Setup(x => x.Importer).Returns((IOrganization)null);
			var result = messageBuilder.GenerateMessage();
			AssertNotNull(result.Importer);
			headerMock.VerifyAll();
			line1Mock.VerifyAll();
		}

		public void TestImporter_EmptyIDNumbers()
		{
			SetupMocksV3();
			var result = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				Assert("KTX Empty", !result.Importer.Id.Any(x => x.SchemeAgencyId == AgencyIdentificationCodeContentType.Ktx));
				Assert("Item380 Empty", !result.Importer.Id.Any(x => x.SchemeAgencyId == AgencyIdentificationCodeContentType.Item380));
				AssertEquals("Role Code Empty", string.Empty, result.Importer.RoleCode.Value);
			});
			headerMock.VerifyAll();
			importerMock.VerifyAll();
			line1Mock.VerifyAll();
		}

		public void TestImporter_BusinessIDNumbers()
		{
			SetupMocksV1();
			importerMock.Setup(x => x.IsIndividual).Returns(false);
			var result = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				AssertEquals("KTX Number", "사업자등록번호", result.Importer.Id.Single(x => x.SchemeAgencyId == AgencyIdentificationCodeContentType.Ktx).Value);
				AssertEquals("Item380 Number", "통관고유부호", result.Importer.Id.Single(x => x.SchemeAgencyId == AgencyIdentificationCodeContentType.Item380).Value);
				AssertEquals("Role Code", "04", result.Importer.RoleCode.Value);
			});
			headerMock.VerifyAll();
			importerMock.VerifyAll();
			line1Mock.VerifyAll();
		}

		public void TestManufacturer_Null()
		{
			SetupMocksV1();
			headerMock.Setup(x => x.Manufacturer).Returns((IOrganization)null);
			var result = messageBuilder.GenerateMessage();
			AssertNull(result.Manufacturer);
			headerMock.VerifyAll();
			importerMock.VerifyAll();
			line1Mock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 07, 29)]
		public void TestGenerateMessage()
		{
			SetupMocksV2();

			var manufacturer = new Mock<IOrganization>();
			manufacturer.Setup(m => m.AddressLine1).Returns("주소");
			manufacturer.Setup(m => m.AddressLine2).Returns("상세주소");
			manufacturer.Setup(m => m.Postcode).Returns("11111");
			line1Mock.Setup(m => m.Manufacturer).Returns(manufacturer.Object);

			var line2Mock = CreateLineMock(2, 1, "000001", "B", "US", "CERTEXP2", "MX", "AU", new ZDate(2014, 04, 13), 874.36m, 1, 489.23m, "CO123457020", "3", "기관명", "2", "3", "BBB", 5.2m);
			headerMock.Setup(x => x.EntryLines).Returns(new IImportFTALine[] { line1Mock.Object, line2Mock.Object });

			var result = messageBuilder.GenerateMessage();
			using (var dataProviderStream = KRXmlObjectSerializer.Serialize(result))
			{
				var expectedXml = File.ReadAllText(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR5SC_Result_D1.xml"));
				var actualXml = dataProviderStream.WriteToString();
				var xmlDiffResult = XmlDifferences.Compare(expectedXml, actualXml);
				AssertEquals(xmlDiffResult.ToString(), true, xmlDiffResult.AreEquals);
			}
			headerMock.VerifyAll();
			importerMock.VerifyAll();
			line1Mock.VerifyAll();
		}

		public void TestEmptyExporter()
		{
			var headerMock = new Mock<IImportFTAHeader>();
			var result = new GOVCBR5SCMessageBuilder(headerMock.Object).GenerateMessage();

			AssertNotNull(result.Exporter.Name);
			AssertNotNull(result.Exporter.Address.CountryCode);
			AssertNotNull(result.Exporter.Address.Line);
			AssertNotNull(result.Exporter.Contact.Name);
			AssertEquals(0, result.Exporter.Communication.Count);

			var supplier = new Mock<IOrganization>();
			supplier.Setup(m => m.PhoneNumber).Returns("01012345678");
			supplier.Setup(m => m.FaxNumber).Returns("00011112222");
			supplier.Setup(m => m.CompanyName).Returns("company");
			supplier.Setup(m => m.CountryCode).Returns("123");
			supplier.Setup(m => m.AddressLine1).Returns("기본주소");
			supplier.Setup(m => m.RepresentativeName).Returns("홍길동");

			headerMock.Setup(m => m.Supplier).Returns(supplier.Object);
			result = new GOVCBR5SCMessageBuilder(headerMock.Object).GenerateMessage();

			AssertEquals("company", result.Exporter.Name.Value);
			AssertEquals("123", result.Exporter.Address.CountryCode.Value);
			AssertEquals("기본주소", result.Exporter.Address.Line.Value);
			AssertEquals("홍길동", result.Exporter.Contact.Name.Value);
			AssertEquals(2, result.Exporter.Communication.Count);
			AssertEquals("01012345678", result.Exporter.Communication[0].Id.Value);
			AssertEquals("00011112222", result.Exporter.Communication[1].Id.Value);
		}

		public void TestGoodsShipment()
		{
			var headerMock = new Mock<IImportFTAHeader>();
			var entryLine = new Mock<IImportFTALine>();
			headerMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			var result = new GOVCBR5SCMessageBuilder(headerMock.Object).GenerateMessage();

			AssertEquals(0m, result.GoodsShipment[0].SequenceNumeric);
			AssertEquals(0m, result.GoodsShipment[0].AdditionalDocument.SequenceNumeric);
			AssertEquals(ZString.Empty, result.GoodsShipment[0].Consignment.DutyTaxFee.DutyRegimeCode.Value);
			AssertEquals(0m, result.GoodsShipment[0].Consignment.DutyTaxFee.TaxRateNumeric);
			AssertEquals(ZString.Empty, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalInformation.StatementCode.Value);
			AssertEquals(ZString.Empty, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueId.Value);

			AssertNull(result.GoodsShipment[0].ExportationCountryCode);
			AssertNull(result.GoodsShipment[0].TransactionNatureCode);
			AssertNull(result.GoodsShipment[0].AdditionalInformation);
			AssertNull(result.GoodsShipment[0].Exporter);
			AssertNull(result.GoodsShipment[0].Invoice);
			AssertNull(result.GoodsShipment[0].Consignment.BorderTransportMeans);
			AssertNull(result.GoodsShipment[0].Consignment.LoadingLocation);
			AssertNull(result.GoodsShipment[0].Consignment.TranshipmentLocation);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Origin);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalDocument);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Classification);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.GoodsMeasure);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Producer);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalInformation.StatementTypeCode);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueDateTime);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueAgencyName);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.SplitVersionNumeric);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueAgencyTypeCode);

			headerMock.Setup(m => m.DepartureCountryCode).Returns("82");
			headerMock.Setup(m => m.TransshipmentYN).Returns("Y");
			headerMock.Setup(m => m.DepartureDate).Returns(new ZDate("2023-01-01"));
			headerMock.Setup(m => m.DeparturePort).Returns("DeparturePort");
			headerMock.Setup(m => m.TransshipmentDate).Returns(new ZDate("2023-01-02"));
			headerMock.Setup(m => m.TransshipmentCountryCode).Returns("CA");
			headerMock.Setup(m => m.TransshipmentPort).Returns("환적항명");

			entryLine.Setup(m => m.SequenceNo).Returns(1);
			entryLine.Setup(m => m.EntryLineNo).Returns(1);
			entryLine.Setup(m => m.DutyRateCode).Returns("DutyRateCode");
			entryLine.Setup(m => m.TariffRate).Returns(0.5);
			entryLine.Setup(m => m.CountryOfOriginSupportingDocType).Returns("1");
			entryLine.Setup(m => m.CertifiticateOfOriginIssuerType).Returns("1");
			entryLine.Setup(m => m.AdditionalInvoiceIssuedInThirdCountryYN).Returns("Y");
			entryLine.Setup(m => m.CertificateOfOriginExporterNumber).Returns("000000010");
			entryLine.Setup(m => m.AssociatedCOOIssuingCountryCode).Returns("MX");
			entryLine.Setup(m => m.CertificateOfOriginIssueDate).Returns(new ZDate("2023-01-03"));
			entryLine.Setup(m => m.CertificateOfOriginNo).Returns("K01321065431");
			entryLine.Setup(m => m.CertificateOfOriginAgencyName).Returns("기관명");
			entryLine.Setup(m => m.CertifiticateOfOriginIssuingAgencyType).Returns("1");
			entryLine.Setup(m => m.CertificateOfOriginSplitOrder).Returns(1);
			entryLine.Setup(m => m.HSCode).Returns("000000");
			entryLine.Setup(m => m.NetWeight).Returns(1000);
			entryLine.Setup(m => m.CertificateOfOriginTotalNetWeight).Returns(1000);
			entryLine.Setup(m => m.CountryOfOrigin).Returns("IL");
			entryLine.Setup(m => m.CertificateOfOriginProductType).Returns("A");
			entryLine.Setup(m => m.AdditionalInvoiceIssuingThirdCountryCode).Returns("JP");

			var manufacturer = new Mock<IOrganization>();
			manufacturer.Setup(m => m.AddressLine1).Returns("생산지역주소");
			manufacturer.Setup(m => m.Postcode).Returns("14123");

			headerMock.Setup(m => m.Manufacturer).Returns(manufacturer.Object);
			headerMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			result = new GOVCBR5SCMessageBuilder(headerMock.Object).GenerateMessage();

			AssertEquals(1m, result.GoodsShipment[0].SequenceNumeric);
			AssertEquals(1m, result.GoodsShipment[0].AdditionalDocument.SequenceNumeric);
			AssertEquals("DutyRateCode", result.GoodsShipment[0].Consignment.DutyTaxFee.DutyRegimeCode.Value);
			AssertEquals(0.5m, result.GoodsShipment[0].Consignment.DutyTaxFee.TaxRateNumeric);
			AssertEquals("1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalInformation.StatementCode.Value);
			AssertEquals("K01321065431", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueId.Value);

			AssertEquals("82", result.GoodsShipment[0].ExportationCountryCode.Value);
			AssertEquals("Y", result.GoodsShipment[0].TransactionNatureCode.Value);
			AssertEquals("Y", result.GoodsShipment[0].AdditionalInformation.StatementCode.Value);
			AssertEquals("000000010", result.GoodsShipment[0].Exporter.Id.Value);
			AssertEquals("JP", result.GoodsShipment[0].Invoice.Submitter.Address.CountryCode.Value);
			AssertEquals("20230101", result.GoodsShipment[0].Consignment.BorderTransportMeans.DepartureDateTime);
			AssertEquals("20230102", result.GoodsShipment[0].Consignment.BorderTransportMeans.Itinerary.DepartureDateTime);
			AssertEquals("DeparturePort", result.GoodsShipment[0].Consignment.LoadingLocation.Name.Value);
			AssertEquals("CA", result.GoodsShipment[0].Consignment.TranshipmentLocation.Id.Value);
			AssertEquals("환적항명", result.GoodsShipment[0].Consignment.TranshipmentLocation.Name.Value);
			AssertEquals("IL", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Origin.CountryCode.Value);
			AssertEquals("A", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Origin.RuleCode.Value);
			AssertEquals("MX", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalDocument.IssueLocationId.Value);
			AssertEquals("000000", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Classification.Id.Value);
			AssertEquals(1000m, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity.WeightMeasure.Value);
			AssertEquals(1000m, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals("1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.AdditionalInformation.StatementTypeCode.Value);
			AssertEquals("20230103", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueDateTime);
			AssertEquals("기관명", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueAgencyName.Value);
			AssertEquals(1m, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.SplitVersionNumeric.Value);
			AssertEquals("1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CertificateOfOrigin.IssueAgencyTypeCode.Value);
		}

		public void TestEmptyImporter()
		{
			var headerMock = new Mock<IImportFTAHeader>();
			var result = new GOVCBR5SCMessageBuilder(headerMock.Object).GenerateMessage();

			AssertEquals(0, result.Importer.Id.Count);
			AssertNotNull(result.Importer.Name);
			AssertNotNull(result.Importer.RoleCode);
			AssertNotNull(result.Importer.Address.PostcodeId);
			AssertNotNull(result.Importer.Address.Description);
			AssertNotNull(result.Importer.Contact.Name);
			AssertEquals(3, result.Importer.Communication.Count);

			AssertNull(result.Importer.Address.Line);
			AssertNull(result.Importer.Address.BuildingNumber);
			AssertNull(result.Importer.Address.CountrySubDivisionId);

			var importer = new Mock<IOrganization>();
			importer.Setup(m => m.PhoneNumber).Returns("01012345678");
			importer.Setup(m => m.FaxNumber).Returns("00011112222");
			importer.Setup(m => m.CompanyName).Returns("company");
			importer.Setup(m => m.Postcode).Returns("123");
			importer.Setup(m => m.AddressLine1).Returns("기본주소");
			importer.Setup(m => m.AddressLine2).Returns("상세주소");
			importer.Setup(m => m.BuildingNumber).Returns("건물번호");
			importer.Setup(m => m.RoadNameCode).Returns("도로명주소");
			importer.Setup(m => m.RepresentativeName).Returns("홍길동");
			importer.Setup(m => m.UnipassIDForIndividual).Returns("1");
			headerMock.Setup(m => m.Importer).Returns(importer.Object);
			result = new GOVCBR5SCMessageBuilder(headerMock.Object).GenerateMessage();

			AssertEquals(1, result.Importer.Id.Count);
			AssertEquals("company", result.Importer.Name.Value);
			AssertEquals("", result.Importer.RoleCode.Value);
			AssertEquals("123", result.Importer.Address.PostcodeId.Value);
			AssertEquals("기본주소", result.Importer.Address.Description.Value);
			AssertEquals("홍길동", result.Importer.Contact.Name.Value);
			AssertEquals(3, result.Importer.Communication.Count);

			AssertEquals("상세주소", result.Importer.Address.Line.Value);
			AssertEquals("건물번호", result.Importer.Address.BuildingNumber.Value);
			AssertEquals("도로명주소", result.Importer.Address.CountrySubDivisionId.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();

			headerMock = new Mock<IImportFTAHeader>();
			headerMock.Setup(m => m.ImportDeclarationNumber).Returns("1364021237195M");
			headerMock.Setup(m => m.DepartureDate).Returns(new ZDate(2014, 5, 6));
			headerMock.Setup(m => m.DepartureCountryCode).Returns("KR");
			headerMock.Setup(m => m.DeparturePort).Returns("선적항명");
			headerMock.Setup(m => m.TransshipmentYN).Returns("Y");
			headerMock.Setup(m => m.TransshipmentDate).Returns(new ZDate(2014, 5, 8));
			headerMock.Setup(m => m.TransshipmentCountryCode).Returns("CA");
			headerMock.Setup(m => m.TransshipmentPort).Returns("환적항명");
		}

		void SetupMocksV1()
		{
			importerMock = CreateImporterV1();
			headerMock.Setup(m => m.Importer).Returns(importerMock.Object);
			SetupMocks();
		}

		void SetupMocksV2()
		{
			importerMock = CreateImporterV2();
			headerMock.Setup(m => m.Importer).Returns(importerMock.Object);
			SetupMocks();
		}

		void SetupMocksV3()
		{
			importerMock = CreateImporter();
			headerMock.Setup(m => m.Importer).Returns(importerMock.Object);
			SetupMocks();
		}

		void SetupMocks()
		{
			var supplierMock = CreateOrgansiation("성명", "상호", "주소", "0421569874", "0421569874");
			supplierMock.Setup(m => m.CountryCode).Returns("KR");
			headerMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			var manufacturerMock = CreateOrgansiation("생산자성명", "상호", "주소", "0421569874", "0421569874");
			headerMock.Setup(m => m.Manufacturer).Returns(manufacturerMock.Object);

			line1Mock = CreateLineMock(1, 1, "000000", "A", "CA", "CERTEXP1", "US", "TH", new ZDate(2014, 04, 12), 999.99m, 3, 99999.87m, "CO123457000", "1", "기관명", "1", "1", "AAA", 10.5m);
			headerMock.Setup(m => m.EntryLines).Returns(new IImportFTALine[] { line1Mock.Object });

			headerMock.Setup(m => m.LawCode).Returns("3");
			headerMock.Setup(m => m.StatementNumber5WN).Returns("123456789012345");

			messageBuilder = new GOVCBR5SCMessageBuilder(headerMock.Object);
		}

		Mock<IImportFTAHeader> headerMock;
		Mock<IOrganization> importerMock;
		Mock<IImportFTALine> line1Mock;
		GOVCBR5SCMessageBuilder messageBuilder;

		Mock<IImportFTALine> CreateLineMock(int lineNo, int sequenceNo, string hsCode, string certificateProductType, string additionalInvoiceIssuingThirdCountryCode, string certificateExporterNo, string associatedCOOIssuingCountryCode,
			string countryOfOrigin, ZDate certificateIssueDate, decimal certificateTotalNetWeight, int certificateSplitOrder, decimal netweight, string certificateNo, string certifiticateIssuingAgencyType, string certificateAgencyName,
			string countryOfOriginSupportingDocType, string certifiticateIssuerType, string dutyRateCode, decimal tariffRate)
		{
			var lineMock = new Mock<IImportFTALine>();
			lineMock.Setup(m => m.EntryLineNo).Returns(lineNo);
			lineMock.Setup(m => m.SequenceNo).Returns(sequenceNo);
			lineMock.Setup(m => m.HSCode).Returns(hsCode);
			lineMock.Setup(m => m.CertificateOfOriginProductType).Returns(certificateProductType);
			lineMock.Setup(m => m.AdditionalInvoiceIssuedInThirdCountryYN).Returns(string.IsNullOrEmpty(additionalInvoiceIssuingThirdCountryCode) ? YesNo.No : YesNo.Yes);
			lineMock.Setup(m => m.AdditionalInvoiceIssuingThirdCountryCode).Returns(additionalInvoiceIssuingThirdCountryCode);
			lineMock.Setup(m => m.CertificateOfOriginExporterNumber).Returns(certificateExporterNo);
			lineMock.Setup(m => m.AssociatedCOOIssuingCountryCode).Returns(associatedCOOIssuingCountryCode);
			lineMock.Setup(m => m.CountryOfOrigin).Returns(countryOfOrigin);
			lineMock.Setup(m => m.CertificateOfOriginIssueDate).Returns(certificateIssueDate);
			lineMock.Setup(m => m.CertificateOfOriginTotalNetWeight).Returns(certificateTotalNetWeight);
			lineMock.Setup(m => m.CertificateOfOriginSplitOrder).Returns(certificateSplitOrder);
			lineMock.Setup(m => m.NetWeight).Returns(netweight);
			lineMock.Setup(m => m.CertificateOfOriginNo).Returns(certificateNo);
			lineMock.Setup(m => m.CertifiticateOfOriginIssuingAgencyType).Returns(certifiticateIssuingAgencyType);
			lineMock.Setup(m => m.CertificateOfOriginAgencyName).Returns(certificateAgencyName);
			lineMock.Setup(m => m.CountryOfOriginSupportingDocType).Returns(countryOfOriginSupportingDocType);
			lineMock.Setup(m => m.CertifiticateOfOriginIssuerType).Returns(certifiticateIssuerType);
			lineMock.Setup(m => m.DutyRateCode).Returns(dutyRateCode);
			lineMock.Setup(m => m.TariffRate).Returns(tariffRate);
			return lineMock;
		}

		Mock<IOrganization> CreateOrgansiation(string representativeName, string companyName, string addressLine1, string phoneNo, string faxNo)
		{
			var organizationMock = new Mock<IOrganization>();
			organizationMock.Setup(m => m.RepresentativeName).Returns(representativeName);
			organizationMock.Setup(m => m.CompanyName).Returns(companyName);
			organizationMock.Setup(m => m.AddressLine1).Returns(addressLine1);
			organizationMock.Setup(m => m.PhoneNumber).Returns(phoneNo);
			organizationMock.Setup(m => m.FaxNumber).Returns(faxNo);
			return organizationMock;
		}

		Mock<IOrganization> CreateImporterV1()
		{
			var importerMock = CreateImporter();
			importerMock.Setup(m => m.BusinessRegNo).Returns("사업자등록번호");
			importerMock.Setup(m => m.UnipassIDForIndividual).Returns("통관고유부호");
			return importerMock;
		}

		Mock<IOrganization> CreateImporterV2()
		{
			var importerMock = CreateImporter();
			importerMock.Setup(m => m.RoadNameCode).Returns("01006");
			importerMock.Setup(m => m.IsIndividual).Returns(true);
			importerMock.Setup(m => m.KoreanRegNoForResident).Returns("주민등록번호");
			importerMock.Setup(m => m.UnipassIDForIndividual).Returns("통관고유부호");
			return importerMock;
		}

		Mock<IOrganization> CreateImporter()
		{
			var importerMock = CreateOrgansiation("성명", "상호", "기본주소", "0421569874", "0421569874");
			importerMock.Setup(m => m.AddressLine2).Returns("상세주소");
			importerMock.Setup(m => m.Postcode).Returns("45521");
			importerMock.Setup(m => m.BuildingNumber).Returns("11101");
			importerMock.Setup(m => m.Email).Returns("aaaa@aaa.com");
			return importerMock;
		}
	}
}
