using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBRD87MessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImportD87Header> importHeaderMock;

		protected override void SetUp()
		{
			base.SetUp();

			importHeaderMock = new Mock<IImportD87Header>();
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("001");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			importHeaderMock.Setup(m => m.EffectiveToDate).Returns(new ZDate("2013-12-12"));
			importHeaderMock.Setup(m => m.CarnetCertificateNumber).Returns("US899916191");
			importHeaderMock.Setup(m => m.TotalInvoiceAmount).Returns(new ZDecimal("999"));
			importHeaderMock.Setup(m => m.InvoiceCurrency).Returns("USD");
			importHeaderMock.Setup(m => m.TotalGrossWeight).Returns(999m);
			importHeaderMock.Setup(m => m.TotalGrossWeighUnit).Returns("KG");
			importHeaderMock.Setup(m => m.TotalQty).Returns(999.99m);
			importHeaderMock.Setup(m => m.UnipassDeclarantID).Returns("12345");
			importHeaderMock.Setup(m => m.RepresentativeProductName).Returns("VISAGE COSMETIC SURGERY SYSTEM");
			importHeaderMock.Setup(m => m.CarnetUseCode).Returns("A");
			importHeaderMock.Setup(m => m.TotalPackQty).Returns(new ZDecimal("999"));
			importHeaderMock.Setup(m => m.PackType).Returns("CT");
			importHeaderMock.Setup(m => m.BondedAreaCode).Returns("A1234567");
			importHeaderMock.Setup(m => m.HouseBillSplitDeclarationIndicator).Returns(new ZBool("true"));
			importHeaderMock.Setup(m => m.CargoManagementNo).Returns("000000000000000");

			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.CompanyName).Returns("명의인명");
			importHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.CompanyName).Returns("수입자 상호");
			importerMock.Setup(m => m.AddressLine1).Returns("수입자주소");
			importerMock.Setup(m => m.AddressLine2).Returns("");
			importerMock.Setup(m => m.PhoneNumber).Returns("02-541-1834");
			importHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);
		}

		[TestDate(2013, 12, 12)]
		public void TestGenerateDeclaration()
		{
			var result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLContains(@"<InvoiceAmount currencyID=""USD"">999</InvoiceAmount>", serialisedXml);
				AssertXMLContains(@"<TotalGrossMassMeasure kcsUnitCode=""KG"">999</TotalGrossMassMeasure>", serialisedXml);
			}

			AssertEquals("00110", result.DeclarationOfficeId.Value);
			AssertEquals("20131212", result.ExpirationDateTime);
			AssertEquals("US899916191", result.Id.Value);
			AssertEquals(999m, result.InvoiceAmount.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Usd, result.InvoiceAmount.CurrencyId);
			AssertEquals(999m, result.TotalGrossMassMeasure.Value);
			AssertEquals("KG", result.TotalGrossMassMeasure.KcsUnitCode);
			AssertEquals(999.99m, result.TotalPackageQuantity.Value);
			AssertEquals("명의인명", result.Agent.Name.Value);
			AssertEquals("12345", result.Agent.Id.Value);
			AssertEquals("VISAGE COSMETIC SURGERY SYSTEM", result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.CargoDescription.Value);
			AssertEquals("A", result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.IntendedUseCode.Value);
			AssertEquals(999m, result.GoodsShipment.GovernmentAgencyGoodsItem.Packaging.QuantityQuantity.Value);
			AssertEquals("CT", result.GoodsShipment.GovernmentAgencyGoodsItem.Packaging.TypeCode.Value);
			AssertEquals("A1234567", result.GoodsShipment.Warehouse.Id.Value);
			AssertEquals("수입자 상호", result.Importer.Name.Value);
			AssertEquals("수입자주소", result.Importer.Address.Line.Value);
			AssertEquals("02-541-1834", result.Importer.Communication.Id.Value);
			AssertEquals(true, result.TransportContractDocument.SplitDeclarationIndicator);
			AssertEquals("000000000000000", result.Ucr.CustomsAssignedReferenceId.Value);
			importHeaderMock.VerifyAll();
		}

		public void TestNoExceptionWithInvalidCurrencyID()
		{
			importHeaderMock.Setup(m => m.InvoiceCurrency).Returns(";;;");
			AssertNoExceptionThrown("No exception should be thrown when invalid currency is entered", () => new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage());
			importHeaderMock.VerifyAll();
		}

		public void TestEmptyGoodsShipment()
		{
			importHeaderMock = new Mock<IImportD87Header>();
			var result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.Warehouse);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Packaging);

			importHeaderMock.Setup(m => m.TotalPackQty).Returns(1);
			result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Packaging.QuantityQuantity);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Packaging.TypeCode);

			importHeaderMock.Setup(m => m.TotalPackQty).Returns(0);
			importHeaderMock.Setup(m => m.PackType).Returns("CT");
			result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Packaging.QuantityQuantity);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Packaging.TypeCode);
		}

		public void TestEmptyImporter()
		{
			importHeaderMock = new Mock<IImportD87Header>();
			var result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertNull(result.Importer);

			var importer = new Mock<IOrganization>();
			importer.Setup(m => m.CompanyName).Returns("company");
			importHeaderMock.Setup(m => m.Importer).Returns(importer.Object);
			result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.Importer.Name);
			AssertNull(result.Importer.Address);
			AssertNull(result.Importer.Communication);

			importer.Setup(m => m.CompanyName).Returns(ZString.Empty);
			importer.Setup(m => m.AddressLine1).Returns("기본주소");
			importer.Setup(m => m.PhoneNumber).Returns("010-1234-5678");
			importHeaderMock.Setup(m => m.Importer).Returns(importer.Object);
			result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertNull(result.Importer.Name);
			AssertNotNull(result.Importer.Address.Line);
			AssertNotNull(result.Importer.Communication.Id);
		}

		public void TestEmptyTotalGrossMassMeasure()
		{
			importHeaderMock = new Mock<IImportD87Header>();
			var result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.TotalGrossMassMeasure);

			importHeaderMock.Setup(m => m.TotalGrossWeight).Returns(100m);
			result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.TotalGrossMassMeasure.Value);
		}

		public void TestEmptyExpirationDate()
		{
			importHeaderMock = new Mock<IImportD87Header>();
			var result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(string.Empty, result.ExpirationDateTime);

			importHeaderMock.Setup(m => m.EffectiveToDate).Returns(ZDate.Invalid);
			result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(string.Empty, result.ExpirationDateTime);

			importHeaderMock.Setup(m => m.EffectiveToDate).Returns(ZDate.Empty);
			result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(string.Empty, result.ExpirationDateTime);

			importHeaderMock.Setup(m => m.EffectiveToDate).Returns(ZDate.Today);
			result = new GOVCBRD87MessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.ExpirationDateTime);
		}
	}
}
