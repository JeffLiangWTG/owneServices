using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5GWMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImport5GWHeader> import5GWMock;

		protected override void SetUp()
		{
			import5GWMock = new Mock<IImport5GWHeader>();
		}

		public void TestGenerateDeclaration()
		{
			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("상호");
			declarantMock.Setup(m => m.RepresentativeName).Returns("성명");
			declarantMock.Setup(m => m.PhoneNumber).Returns("010-0000-0000");
			import5GWMock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			import5GWMock.Setup(m => m.ApplicationNumber).Returns("11598121000008U");
			import5GWMock.Setup(m => m.StartDateTime).Returns(new ZDateTime("2014-05-06 12:30:12"));
			import5GWMock.Setup(m => m.EndDateTime).Returns(new ZDateTime("2014-05-06 12:30:12"));
			import5GWMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			import5GWMock.Setup(m => m.DeclarationCustomsDivision).Returns("20");
			import5GWMock.Setup(m => m.ApplicationReason).Returns("임시개청/취소사유");

			var detailsMock1 = new Mock<IImport5GWEntry>();
			detailsMock1.Setup(m => m.ReferenceNumberType).Returns(ReferenceNumberTypeList.Codes.CMN);
			detailsMock1.Setup(m => m.ReferenceNumber).Returns("000000000000000");
			detailsMock1.Setup(m => m.HSDescription).Returns("품명");
			detailsMock1.Setup(m => m.TotalCustomsValueInUSD).Returns(21M);
			detailsMock1.Setup(m => m.TotalPackQty).Returns(99);
			detailsMock1.Setup(m => m.TotalGrossWeightInKG).Returns(99M);
			detailsMock1.Setup(m => m.BondedAreaCode).Returns("02010578");
			detailsMock1.Setup(m => m.PayerCompanyName).Returns("상호");

			var detailsMock2 = new Mock<IImport5GWEntry>();
			detailsMock2.Setup(m => m.ReferenceNumberType).Returns(ReferenceNumberTypeList.Codes.IMP);
			detailsMock2.Setup(m => m.ReferenceNumber).Returns("111111");
			detailsMock2.Setup(m => m.HSDescription).Returns("품명2");
			detailsMock2.Setup(m => m.TotalCustomsValueInUSD).Returns(22M);
			detailsMock2.Setup(m => m.TotalPackQty).Returns(100);
			detailsMock2.Setup(m => m.TotalGrossWeightInKG).Returns(100M);
			detailsMock2.Setup(m => m.BondedAreaCode).Returns("02010578");
			detailsMock2.Setup(m => m.PayerCompanyName).Returns("상호");
			import5GWMock.Setup(m => m.Entries).Returns(new IImport5GWEntry[] { detailsMock1.Object, detailsMock2.Object });

			var result = new GOVCBR5GWMessageBuilder(import5GWMock.Object).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
			}
			AssertEquals("11598121000008U", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("20140506123012", result.AdditionalInformation.BeginningDateTime);
			AssertEquals("20140506123012", result.AdditionalInformation.EndingDateTime);
			AssertEquals("01020", result.DeclarationOfficeId.Value);
			AssertEquals("임시개청/취소사유", result.Reason.Value);
			AssertEquals("000000000000000", result.GoodsShipment[0].AdditionalDocument.Id.Value);
			AssertEquals("XXC", result.GoodsShipment[0].AdditionalDocument.TypeCode.Value);
			AssertEquals("111111", result.GoodsShipment[1].AdditionalDocument.Id.Value);
			AssertEquals("ABT", result.GoodsShipment[1].AdditionalDocument.TypeCode.Value);
			AssertEquals("품명", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CargoDescription.Value);
			AssertEquals(22M, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);
			AssertEquals(99M, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CountQuantity.Value);
			AssertEquals(100M, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.SizeMeasure.Value);
			AssertEquals("02010578", result.GoodsShipment[0].Warehouse.Id.Value);
			AssertEquals("상호", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Payer.Name.Value);

			AssertEquals("상호", result.Submitter.Name.Value);
			AssertEquals("성명", result.Submitter.Contact.Name.Value);
			AssertEquals("010-0000-0000", result.Submitter.Communication.Id.Value);
			import5GWMock.VerifyAll();
			declarantMock.VerifyAll();
			detailsMock1.VerifyAll();
			detailsMock2.VerifyAll();
		}
		public void TestDateTimeValuesCheck()
		{
			var headerMock = new Mock<IImport5GWHeader>();
			headerMock.Setup(m => m.ApplicationNumber).Returns("");
			headerMock.Setup(m => m.DeclarationCustomsOffice).Returns("");
			headerMock.Setup(m => m.DeclarationCustomsDivision).Returns("");
			headerMock.Setup(m => m.ApplicationReason).Returns("");
			headerMock.Setup(m => m.StartDateTime).Returns(new ZDateTime());
			headerMock.Setup(m => m.EndDateTime).Returns(new ZDateTime());
			headerMock.Setup(m => m.Entries).Returns(new IImport5GWEntry[] { new Mock<IImport5GWEntry>().Object });
			headerMock.Setup(m => m.Declarant).Returns(new Mock<IOrganization>().Object);

			var result = new GOVCBR5GWMessageBuilder(headerMock.Object).GenerateMessage();
			AssertEquals("The Start Date is mandatory element.", string.Empty, result.AdditionalInformation.BeginningDateTime);
			AssertEquals("The End Date is conditional element.", null, result.AdditionalInformation.EndingDateTime);

			headerMock.Setup(m => m.StartDateTime).Returns(new ZDateTime(2014, 05, 06, 12, 30, 12));
			headerMock.Setup(m => m.EndDateTime).Returns(new ZDateTime(2014, 06, 06, 12, 30, 12));
			result = new GOVCBR5GWMessageBuilder(headerMock.Object).GenerateMessage();
			AssertEquals("20140506123012", result.AdditionalInformation.BeginningDateTime);
			AssertEquals("20140606123012", result.AdditionalInformation.EndingDateTime);
			headerMock.VerifyAll();
			import5GWMock.VerifyAll();
		}

		public void TestNotCreateElementWhenNoMandatory()
		{
			import5GWMock.Setup(m => m.Entries).Returns(new IImport5GWEntry[] { new Mock<IImport5GWEntry>().Object });
			import5GWMock.Setup(m => m.Declarant).Returns(new Mock<IOrganization>().Object);
			var result = new GOVCBR5GWMessageBuilder(import5GWMock.Object).GenerateMessage();

			AssertEquals(ZString.Empty, result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5GW", result.TypeCode.Value);

			AssertNull(result.Reason);

			AssertEquals(ZString.Empty, result.GoodsShipment[0].AdditionalDocument.Id.Value);
			AssertEquals(ZString.Empty, result.GoodsShipment[0].AdditionalDocument.TypeCode.Value);
			AssertEquals(ZString.Empty, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CargoDescription.Value);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CountQuantity);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.SizeMeasure);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee);
			AssertEquals(ZString.Empty, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Payer.Name.Value);
			AssertEquals(ZString.Empty, result.GoodsShipment[0].Warehouse.Id.Value);
		}
	}
}
