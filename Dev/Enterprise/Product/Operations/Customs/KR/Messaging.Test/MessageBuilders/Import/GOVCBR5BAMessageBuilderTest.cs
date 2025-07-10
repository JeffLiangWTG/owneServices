using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5BAMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImport5BAHeader> importHeaderMock;

		protected override void SetUp()
		{
			base.SetUp();

			importHeaderMock = new Mock<IImport5BAHeader>();
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("02455874051");
			importHeaderMock.Setup(m => m.ImportDeclarationDate).Returns(new ZDate("2014-05-06"));
			importHeaderMock.Setup(m => m.TariffRateClassification).Returns("CUD");
			importHeaderMock.Setup(m => m.TariffRate).Returns(4.2m);

			var importLine1 = new Mock<IImport5BALine>();
			importLine1.Setup(m => m.EntryLineNo).Returns(1);
			importLine1.Setup(m => m.HSCode).Returns("0000000001");
			importLine1.Setup(m => m.HSDescription).Returns("품명");
			importLine1.Setup(m => m.InvoiceDescription).Returns("거래품명");

			var importLine2 = new Mock<IImport5BALine>();
			importLine2.Setup(m => m.EntryLineNo).Returns(2);
			importLine2.Setup(m => m.HSCode).Returns("0000000002");
			importLine2.Setup(m => m.HSDescription).Returns("품명2");
			importLine2.Setup(m => m.InvoiceDescription).Returns("거래품명2");

			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImport5BALine[] { importLine1.Object, importLine2.Object });

			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("신청인상호");
			declarantMock.Setup(m => m.RepresentativeName).Returns("성명");
			declarantMock.Setup(m => m.AddressLine1).Returns("기본주소");
			declarantMock.Setup(m => m.AddressLine2).Returns("상세주소");
			declarantMock.Setup(m => m.Postcode).Returns("46512");
			declarantMock.Setup(m => m.RoadNameCode).Returns("101010");
			declarantMock.Setup(m => m.BuildingNumber).Returns("10201");
			declarantMock.Setup(m => m.IsIndividual).Returns(ZBool.False);
			declarantMock.Setup(m => m.CountryCode).Returns("KR");

			declarantMock.Setup(m => m.ForeignCompanyID).Returns("1028142299");
			declarantMock.Setup(m => m.BusinessRegNo).Returns("0000000000");

			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
		}

		public void TestGenerateDeclaration()
		{
			var result = new GOVCBR5BAMessageBuilder(importHeaderMock.Object).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
			}

			AssertEquals("02455874051", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5BA", result.TypeCode.Value);

			AssertEquals(1m, result.Consignment[0].SequenceNumeric);
			AssertEquals("거래품명", result.Consignment[0].ConsignmentItem.Commodity.CargoDescription.Value);
			AssertEquals("품명", result.Consignment[0].ConsignmentItem.Commodity.Description.Value);
			AssertEquals("0000000002", result.Consignment[1].ConsignmentItem.Commodity.Classification.Id.Value);
			AssertEquals(4.2m, result.Consignment[1].ConsignmentItem.Commodity.DutyTaxFee.TaxRateNumeric);
			AssertEquals("CUD", result.Consignment[1].ConsignmentItem.Commodity.DutyTaxFee.TypeCode.Value);

			AssertEquals("20140506", result.PreviousDocument.IssueDateTime);

			AssertEquals("0000000000", result.Submitter.Id.Value);
			AssertEquals("신청인상호", result.Submitter.Name.Value);
			AssertEquals("101010", result.Submitter.Address.CountrySubDivisionId.Value);
			AssertEquals("상세주소", result.Submitter.Address.Line.Value);
			AssertEquals("46512", result.Submitter.Address.PostcodeId.Value);
			AssertEquals("10201", result.Submitter.Address.BuildingNumber.Value);
			AssertEquals("기본주소", result.Submitter.Address.Description.Value);
			AssertEquals("성명", result.Submitter.Contact.Name.Value);
		}

		public void TestNotCreateElementWhenNoMandatory()
		{
			var emptyMock = new Mock<IImport5BAHeader>();
			emptyMock.Setup(m => m.EntryLines).Returns(new IImport5BALine[] { new Mock<IImport5BALine>().Object });
			var result = new GOVCBR5BAMessageBuilder(emptyMock.Object).GenerateMessage();

			AssertEquals(ZString.Empty, result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5BA", result.TypeCode.Value);

			AssertEquals(ZDecimal.Zero, result.Consignment[0].SequenceNumeric);
			AssertEquals(ZString.Empty, result.Consignment[0].ConsignmentItem.Commodity.CargoDescription.Value);
			AssertEquals(ZString.Empty, result.Consignment[0].ConsignmentItem.Commodity.Description.Value);
			AssertEquals(ZString.Empty, result.Consignment[0].ConsignmentItem.Commodity.Classification.Id.Value);
			AssertEquals(ZDecimal.Zero, result.Consignment[0].ConsignmentItem.Commodity.DutyTaxFee.TaxRateNumeric);
			AssertEquals(ZString.Empty, result.Consignment[0].ConsignmentItem.Commodity.DutyTaxFee.TypeCode.Value);

			AssertEquals(ZString.Empty, result.PreviousDocument.IssueDateTime);

			AssertEquals(ZString.Empty, result.Submitter.Id.Value);
			AssertEquals(ZString.Empty, result.Submitter.Name.Value);
			AssertEquals(ZString.Empty, result.Submitter.Contact.Name.Value);
			AssertEquals(ZString.Empty, result.Submitter.Address.PostcodeId.Value);
			AssertEquals(ZString.Empty, result.Submitter.Address.Description.Value);
			AssertNull(result.Submitter.Address.CountrySubDivisionId);
			AssertNull(result.Submitter.Address.Line);
			AssertNull(result.Submitter.Address.BuildingNumber);
		}

		public void TestCheckDateValueIsInvalid()
		{
			importHeaderMock.Setup(m => m.ImportDeclarationDate).Returns(new ZDate());
			var result = new GOVCBR5BAMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(string.Empty, result.PreviousDocument.IssueDateTime);

			importHeaderMock.Setup(m => m.ImportDeclarationDate).Returns(ZDate.Invalid);
			result = new GOVCBR5BAMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(string.Empty, result.PreviousDocument.IssueDateTime);
		}
	}
}
