using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5ACMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IExport5ACHeader> exportHeaderMock;

		protected override void SetUp()
		{
			base.SetUp();

			exportHeaderMock = new Mock<IExport5ACHeader>();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2014, 05, 06)]
		public void TestGenerateDeclaration()
		{
			exportHeaderMock.Setup(m => m.ApplicationNumber).Returns("11598121000008U");
			exportHeaderMock.Setup(m => m.StartDateTime).Returns(new ZDateTime("2014-05-06 12:30"));
			exportHeaderMock.Setup(m => m.EndDateTime).Returns(new ZDateTime("2014-05-09 12:30"));
			exportHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			exportHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("20");

			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("상호");
			declarantMock.Setup(m => m.RepresentativeName).Returns("성명");
			declarantMock.Setup(m => m.PhoneNumber).Returns("0426589012");

			exportHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			exportHeaderMock.Setup(m => m.ApplicationReason).Returns("임시개청사유");

			var exportEntryLine1 = new Mock<IExport5ACEntry>();
			exportEntryLine1.Setup(m => m.ReferenceNumber).Returns("0000000000");
			exportEntryLine1.Setup(m => m.TotalCustomsValueInUSD).Returns(1000);
			exportEntryLine1.Setup(m => m.TotalPackQty).Returns(99);
			exportEntryLine1.Setup(m => m.TotalGrossWeightInKG).Returns(500);
			exportEntryLine1.Setup(m => m.SupplierName).Returns("수출화주");

			var exportEntryLine2 = new Mock<IExport5ACEntry>();
			exportEntryLine2.Setup(m => m.ReferenceNumber).Returns("1111111111");
			exportEntryLine2.Setup(m => m.TotalCustomsValueInUSD).Returns(5000);
			exportEntryLine2.Setup(m => m.TotalPackQty).Returns(5);
			exportEntryLine2.Setup(m => m.TotalGrossWeightInKG).Returns(100);
			exportEntryLine2.Setup(m => m.SupplierName).Returns("수출화주2");

			exportHeaderMock.Setup(m => m.Entries).Returns(new IExport5ACEntry[] { exportEntryLine1.Object, exportEntryLine2.Object });

			var result = new GOVCBR5ACMessageBuilder(exportHeaderMock.Object).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ExportOutgoingTestFilePath, "GOVCBR5AC.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("11598121000008U", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5AC", result.TypeCode.Value);
			AssertEquals("201405061230", result.AdditionalInformation.BeginningDateTime);
			AssertEquals("201405091230", result.AdditionalInformation.EndingDateTime);
			AssertEquals("01020", result.DeclarationOfficeId.Value);

			AssertEquals("상호", result.Submitter.Name.Value);
			AssertEquals("성명", result.Submitter.Contact.Name.Value);
			AssertEquals("0426589012", result.Submitter.Communication.Id.Value);

			AssertEquals("임시개청사유", result.Reason.Value);

			AssertEquals("0000000000", result.GoodsShipment[0].AdditionalDocument.Id.Value);
			AssertEquals(1000m, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);
			AssertEquals(99m, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CountQuantity.Value);
			AssertEquals(500m, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.SizeMeasure.Value);
			AssertEquals("수출화주", result.GoodsShipment[0].Exporter.Name.Value);

			AssertEquals("1111111111", result.GoodsShipment[1].AdditionalDocument.Id.Value);
			AssertEquals(5000m, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);
			AssertEquals(5m, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.CountQuantity.Value);
			AssertEquals(100m, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.SizeMeasure.Value);
			AssertEquals("수출화주2", result.GoodsShipment[1].Exporter.Name.Value);

			exportHeaderMock.VerifyAll();
			declarantMock.VerifyAll();
			exportEntryLine1.VerifyAll();
			exportEntryLine2.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2014, 05, 06)]
		public void TestWithConditionalElements()
		{
			exportHeaderMock.Setup(m => m.ApplicationNumber).Returns("11598121000008U");
			exportHeaderMock.Setup(m => m.StartDateTime).Returns(new ZDateTime("2014-05-06 12:30"));
			exportHeaderMock.Setup(m => m.EndDateTime).Returns(ZDateTime.Empty);
			exportHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			exportHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("20");

			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("상호");
			declarantMock.Setup(m => m.RepresentativeName).Returns("성명");
			declarantMock.Setup(m => m.PhoneNumber).Returns("0426589012");

			exportHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			exportHeaderMock.Setup(m => m.ApplicationReason).Returns(ZString.Empty);

			var result = new GOVCBR5ACMessageBuilder(exportHeaderMock.Object).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ExportOutgoingTestFilePath, "GOVCBR5AC_Empty.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("11598121000008U", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5AC", result.TypeCode.Value);
			AssertEquals("201405061230", result.AdditionalInformation.BeginningDateTime);
			AssertEquals("01020", result.DeclarationOfficeId.Value);

			AssertEquals("상호", result.Submitter.Name.Value);
			AssertEquals("성명", result.Submitter.Contact.Name.Value);
			AssertEquals("0426589012", result.Submitter.Communication.Id.Value);
			exportHeaderMock.VerifyAll();
			declarantMock.VerifyAll();
		}
		public void TestDateTimeValuesCheck()
		{
			var headerMock = new Mock<IExport5ACHeader>();
			headerMock.Setup(m => m.ApplicationNumber).Returns("");
			headerMock.Setup(m => m.DeclarationCustomsOffice).Returns("");
			headerMock.Setup(m => m.DeclarationCustomsDivision).Returns("");
			headerMock.Setup(m => m.ApplicationReason).Returns("");
			headerMock.Setup(m => m.StartDateTime).Returns(new ZDateTime());
			headerMock.Setup(m => m.EndDateTime).Returns(new ZDateTime());

			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("상호");
			declarantMock.Setup(m => m.RepresentativeName).Returns("성명");
			declarantMock.Setup(m => m.PhoneNumber).Returns("0426589012");
			headerMock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			var result = new GOVCBR5ACMessageBuilder(headerMock.Object).GenerateMessage();
			AssertEquals("The Start Date is mandatory element.", string.Empty, result.AdditionalInformation.BeginningDateTime);
			AssertEquals("The End Date is conditional element.", null, result.AdditionalInformation.EndingDateTime);

			headerMock.Setup(m => m.StartDateTime).Returns(new ZDateTime(2014, 05, 06, 12, 30, 12));
			headerMock.Setup(m => m.EndDateTime).Returns(new ZDateTime(2014, 06, 06, 12, 30, 12));
			result = new GOVCBR5ACMessageBuilder(headerMock.Object).GenerateMessage();
			AssertEquals("201405061230", result.AdditionalInformation.BeginningDateTime);
			AssertEquals("201406061230", result.AdditionalInformation.EndingDateTime);
			headerMock.VerifyAll();
		}
	}
}
