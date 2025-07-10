using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBRD72MessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImportD72Header> importD72Mock;

		protected override void SetUp()
		{
			importD72Mock = new Mock<IImportD72Header>();

			importD72Mock.Setup(m => m.ImportDeclarationNumber).Returns("999999999999");
			importD72Mock.Setup(m => m.SequenceNo).Returns(1);
			importD72Mock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			importD72Mock.Setup(m => m.DeclarationCustomsDivision).Returns("30");
			importD72Mock.Setup(m => m.DeclarantType).Returns("1");
			importD72Mock.Setup(m => m.BeforeReExportScheduledDate).Returns(new ZDate("2020-09-07"));
			importD72Mock.Setup(m => m.AfterReExportScheduledDate).Returns(new ZDate("2020-09-07"));
			importD72Mock.Setup(m => m.ReasonDescription).Returns("TEST");

			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("신청인상호");
			declarantMock.Setup(m => m.RepresentativeName).Returns("신청인대표자명");
			declarantMock.Setup(m => m.AddressLine1).Returns("신청인 주소1");
			declarantMock.Setup(m => m.AddressLine2).Returns("신청인 주소2");
			declarantMock.Setup(m => m.Postcode).Returns("12345");
			declarantMock.Setup(m => m.RoadNameCode).Returns("101010");
			declarantMock.Setup(m => m.BuildingNumber).Returns("020120");
			declarantMock.Setup(m => m.IsIndividual).Returns(ZBool.False);
			declarantMock.Setup(m => m.BusinessRegNo).Returns("사업자등록번호");
			importD72Mock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			var entryLine1Mock = new Mock<IImportD72Line>();
			entryLine1Mock.Setup(m => m.EntryLineNo).Returns(1);
			entryLine1Mock.Setup(m => m.DetailLineNo).Returns(1);
			entryLine1Mock.Setup(m => m.HSDescription).Returns("신고품명1");
			entryLine1Mock.Setup(m => m.ItemDescription).Returns("규격1");
			entryLine1Mock.Setup(m => m.QuantityUnit).Returns("CT");
			entryLine1Mock.Setup(m => m.Quantity).Returns(10m);
			entryLine1Mock.Setup(m => m.AmountCurrency).Returns("USD");
			entryLine1Mock.Setup(m => m.Amount).Returns(1000.5m);
			entryLine1Mock.Setup(m => m.Remark).Returns("비고내용");

			var entryLine2Mock = new Mock<IImportD72Line>();
			entryLine2Mock.Setup(m => m.EntryLineNo).Returns(2);
			entryLine2Mock.Setup(m => m.DetailLineNo).Returns(2);
			entryLine2Mock.Setup(m => m.HSDescription).Returns("신고품명2");
			entryLine2Mock.Setup(m => m.ItemDescription).Returns("규격2");
			entryLine2Mock.Setup(m => m.QuantityUnit).Returns("CT");
			entryLine2Mock.Setup(m => m.Quantity).Returns(100m);
			entryLine2Mock.Setup(m => m.AmountCurrency).Returns("USD");
			entryLine2Mock.Setup(m => m.Amount).Returns(10000.0m);
			entryLine2Mock.Setup(m => m.Remark).Returns("비고내용2");
			importD72Mock.Setup(m => m.Lines).Returns(new IImportD72Line[] { entryLine1Mock.Object, entryLine2Mock.Object });
		}

		public void TestGenerateDeclaration()
		{
			var result = new GOVCBRD72MessageBuilder(importD72Mock.Object).GenerateMessage();

			AssertEquals("010", result.DeclarationOfficeId.Value);
			AssertEquals("999999999999", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("30", result.SubsequentDeclarationOfficeId.Value);
			AssertEquals("GOVCBRD72", result.TypeCode.Value);
			AssertEquals("1", result.VersionId.Value);
			AssertEquals("TEST", result.Reason.Value);
			AssertEquals("20200907", result.AdditionalInformation.LimitDateTime);
			AssertEquals("20200907", result.PreviousDocument.IssueDateTime);
			AssertEquals(1m, result.GoodsShipment[0].GovernmentAgencyGoodsItem.SequenceNumeric);
			AssertEquals("비고내용", result.GoodsShipment[0].GovernmentAgencyGoodsItem.AdditionalInformation.Content.Value);
			AssertEquals("신고품명1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CargoDescription.Value);
			AssertEquals(10m, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CountQuantity.Value);
			AssertEquals("규격2", result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.Description.Value);
			AssertEquals("1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.IdentityQualifierCode.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Usd, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.ValueAmount.CurrencyId);
			AssertEquals(10000.0m, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.ValueAmount.Value);
			AssertEquals("사업자등록번호", result.Submitter.Id.Value);
			AssertEquals("04", result.Submitter.RoleCode.Value);
			AssertEquals("1", result.Submitter.TypeCode.Value);
			AssertEquals("신청인상호", result.Submitter.Name.Value);
			AssertEquals("020120", result.Submitter.Address.BuildingNumber.Value);
			AssertEquals("12345", result.Submitter.Address.PostcodeId.Value);
			AssertEquals("신청인 주소2", result.Submitter.Address.Line.Value);
			AssertEquals("신청인 주소1", result.Submitter.Address.Description.Value);
			AssertEquals("101010", result.Submitter.Address.CountrySubDivisionId.Value);
			AssertEquals("신청인대표자명", result.Submitter.Contact.Name.Value);
			importD72Mock.VerifyAll();
		}

		public void TestWithSubmitter()
		{
			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("신청인상호");
			declarantMock.Setup(m => m.RepresentativeName).Returns("신청인대표자명");
			declarantMock.Setup(m => m.AddressLine1).Returns("신청인 주소1");
			declarantMock.Setup(m => m.AddressLine2).Returns("신청인 주소2");
			declarantMock.Setup(m => m.Postcode).Returns("12345");
			declarantMock.Setup(m => m.RoadNameCode).Returns("101010");
			declarantMock.Setup(m => m.BuildingNumber).Returns("020120");
			declarantMock.Setup(m => m.IsIndividual).Returns(ZBool.True);
			declarantMock.Setup(m => m.PassportNo).Returns("FKR2222222");
			importD72Mock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			var result = new GOVCBRD72MessageBuilder(importD72Mock.Object).GenerateMessage();
			AssertEquals("FKR2222222", result.Submitter.Id.Value);
			AssertEquals("02", result.Submitter.RoleCode.Value);
			importD72Mock.VerifyAll();
			declarantMock.VerifyAll();
		}

		public void TestNoExceptionWithInvalidCurrencyID()
		{
			var entryLine1Mock = new Mock<IImportD72Line>();
			entryLine1Mock.Setup(m => m.EntryLineNo).Returns(1);
			entryLine1Mock.Setup(m => m.DetailLineNo).Returns(1);
			entryLine1Mock.Setup(m => m.HSDescription).Returns("신고품명1");
			entryLine1Mock.Setup(m => m.ItemDescription).Returns("규격1");
			entryLine1Mock.Setup(m => m.QuantityUnit).Returns("CT");
			entryLine1Mock.Setup(m => m.Quantity).Returns(10m);
			entryLine1Mock.Setup(m => m.AmountCurrency).Returns(";;;");
			entryLine1Mock.Setup(m => m.Amount).Returns(1000.5m);
			entryLine1Mock.Setup(m => m.Remark).Returns("비고내용");
			importD72Mock.Setup(m => m.Lines).Returns(new IImportD72Line[] { entryLine1Mock.Object });

			AssertNoExceptionThrown("No exception should be thrown when invalid currency is entered", () => new GOVCBRD72MessageBuilder(importD72Mock.Object).GenerateMessage());
			entryLine1Mock.VerifyAll();
			importD72Mock.VerifyAll();
		}

		public void TestEmptyElement()
		{
			var importD72Mock = new Mock<IImportD72Header>();
			importD72Mock.Setup(m => m.Lines).Returns(new IImportD72Line[] { new Mock<IImportD72Line>().Object });
			var result = new GOVCBRD72MessageBuilder(importD72Mock.Object).GenerateMessage();
			AssertNotNull(result.Id);
			AssertNotNull(result.VersionId);
			AssertNotNull(result.DeclarationOfficeId);
			AssertNull(result.SubsequentDeclarationOfficeId);
			AssertNotNull(result.IssueDateTime);
			AssertNotNull(result.Submitter.TypeCode);
			AssertNull(result.Submitter.Name);
			AssertNotNull(result.Submitter.Contact.Name);
			AssertNotNull(result.Submitter.RoleCode);
			AssertNotNull(result.Submitter.Id);
			AssertNull(result.Submitter.Address.PostcodeId);
			AssertNull(result.Submitter.Address.CountrySubDivisionId);
			AssertNull(result.Submitter.Address.BuildingNumber);
			AssertNotNull(result.Submitter.Address.Description);
			AssertNull(result.Submitter.Address.Line);
			AssertNotNull(result.PreviousDocument.IssueDateTime);
			AssertNotNull(result.AdditionalInformation.LimitDateTime);
			AssertNotNull(result.Reason);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.SequenceNumeric);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.IdentityQualifierCode);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CargoDescription);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Description);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CountQuantity.KcsUnitCode);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CountQuantity);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.ValueAmount);
			AssertNull("Not created AdditionalInformation.Content element.", result.GoodsShipment[0].GovernmentAgencyGoodsItem.AdditionalInformation);
		}

		public void TestEmptyDate()
		{
			var importD72Mock = new Mock<IImportD72Header>();
			importD72Mock.Setup(m => m.Lines).Returns(new IImportD72Line[] { new Mock<IImportD72Line>().Object });
			var result = new GOVCBRD72MessageBuilder(importD72Mock.Object).GenerateMessage();
			AssertEquals(string.Empty, result.PreviousDocument.IssueDateTime);
			AssertEquals(string.Empty, result.AdditionalInformation.LimitDateTime);

			importD72Mock.Setup(m => m.BeforeReExportScheduledDate).Returns(ZDate.Invalid);
			importD72Mock.Setup(m => m.AfterReExportScheduledDate).Returns(ZDate.Invalid);
			result = new GOVCBRD72MessageBuilder(importD72Mock.Object).GenerateMessage();
			AssertEquals(string.Empty, result.PreviousDocument.IssueDateTime);
			AssertEquals(string.Empty, result.AdditionalInformation.LimitDateTime);
		}
	}
}
