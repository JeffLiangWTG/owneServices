using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5TMMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateDeclarationWhenIsIndividualIsFalse()
		{
			var import5TMMock = new Mock<IImport5TMHeader>();
			import5TMMock.Setup(m => m.ImportDeclarationNumber).Returns("1234520100523X");

			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.IsIndividual).Returns(ZBool.False);
			payerMock.Setup(m => m.CorporationCode).Returns("499999999");
			import5TMMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var entryLine1Mock = new Mock<IImport5TMLine>();
			entryLine1Mock.Setup(m => m.EntryLineNo).Returns(1);
			entryLine1Mock.Setup(m => m.HSDescription).Returns("품명규격1");
			entryLine1Mock.Setup(m => m.HSCode).Returns("HS_CODE1");
			entryLine1Mock.Setup(m => m.ValueForVAT).Returns(10.9);
			entryLine1Mock.Setup(m => m.VAT).Returns(10.1);
			entryLine1Mock.Setup(m => m.NetWeightKG).Returns(10.8);

			var entryLine2Mock = new Mock<IImport5TMLine>();
			entryLine2Mock.Setup(m => m.EntryLineNo).Returns(2);
			entryLine2Mock.Setup(m => m.HSDescription).Returns("품명규격2");
			entryLine2Mock.Setup(m => m.HSCode).Returns("HS_CODE2");
			entryLine2Mock.Setup(m => m.ValueForVAT).Returns(20.9);
			entryLine2Mock.Setup(m => m.VAT).Returns(11.1);
			entryLine2Mock.Setup(m => m.NetWeightKG).Returns(12.8);

			import5TMMock.Setup(m => m.EntryLines).Returns(new IImport5TMLine[] { entryLine1Mock.Object, entryLine2Mock.Object });

			var result = new GOVCBR5TMMessageBuilder(import5TMMock.Object).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
			}
			AssertEquals("1234520100523X", result.Id.Value);
			AssertEquals(ZDate.Today.ToString(DateFormatType.Date), result.IssueDateTime);
			AssertEquals("GOVCBR5TM", result.TypeCode.Value);
			AssertEquals(1M, result.GoodsShipment[0].SequenceNumeric);
			AssertEquals("품명규격2", result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.CargoDescription.Value);
			AssertEquals("HS_CODE1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Classification.Id.Value);
			AssertEquals(20.9M, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);
			AssertEquals(10.1M, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.Payment.TaxAssessedAmount.Value);
			AssertEquals(12.8M, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.GoodsMeasure.NetNetWeightMeasure.Value);

			AssertEquals("499999999", result.Payer.Id.Value);

			import5TMMock.VerifyAll();
			payerMock.VerifyAll();
			entryLine1Mock.VerifyAll();
			entryLine2Mock.VerifyAll();
		}

		public void TestGenerateDeclarationWhenIsIndividualIsTrue()
		{
			var import5TMMock = new Mock<IImport5TMHeader>();
			import5TMMock.Setup(m => m.ImportDeclarationNumber).Returns("1234520100523X");

			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.IsIndividual).Returns(ZBool.True);
			payerMock.Setup(m => m.KoreanRegNoForResident).Returns("199999999");
			import5TMMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var entryLine1Mock = new Mock<IImport5TMLine>();
			entryLine1Mock.Setup(m => m.EntryLineNo).Returns(1);
			entryLine1Mock.Setup(m => m.HSDescription).Returns("품명규격1");
			entryLine1Mock.Setup(m => m.HSCode).Returns("HS_CODE1");
			entryLine1Mock.Setup(m => m.ValueForVAT).Returns(10.9);
			entryLine1Mock.Setup(m => m.VAT).Returns(10.1);
			entryLine1Mock.Setup(m => m.NetWeightKG).Returns(10.8);

			var entryLine2Mock = new Mock<IImport5TMLine>();
			entryLine2Mock.Setup(m => m.EntryLineNo).Returns(2);
			entryLine2Mock.Setup(m => m.HSDescription).Returns("품명규격2");
			entryLine2Mock.Setup(m => m.HSCode).Returns("HS_CODE2");
			entryLine2Mock.Setup(m => m.ValueForVAT).Returns(20.9);
			entryLine2Mock.Setup(m => m.VAT).Returns(11.1);
			entryLine2Mock.Setup(m => m.NetWeightKG).Returns(12.8);

			import5TMMock.Setup(m => m.EntryLines).Returns(new IImport5TMLine[] { entryLine1Mock.Object, entryLine2Mock.Object });

			var result = new GOVCBR5TMMessageBuilder(import5TMMock.Object).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
			}
			AssertEquals("1234520100523X", result.Id.Value);
			AssertEquals(ZDate.Today.ToString(DateFormatType.Date), result.IssueDateTime);
			AssertEquals("GOVCBR5TM", result.TypeCode.Value);
			AssertEquals(1M, result.GoodsShipment[0].SequenceNumeric);
			AssertEquals("품명규격2", result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.CargoDescription.Value);
			AssertEquals("HS_CODE1", result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Classification.Id.Value);
			AssertEquals(20.9M, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);
			AssertEquals(10.1M, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.Payment.TaxAssessedAmount.Value);
			AssertEquals(12.8M, result.GoodsShipment[1].GovernmentAgencyGoodsItem.Commodity.GoodsMeasure.NetNetWeightMeasure.Value);

			AssertEquals("199999999", result.Payer.Id.Value);

			import5TMMock.VerifyAll();
			payerMock.VerifyAll();
			entryLine1Mock.VerifyAll();
			entryLine2Mock.VerifyAll();
		}

		public void TestEmptryElement()
		{
			var import5TMMock = new Mock<IImport5TMHeader>();
			var entryLineMock = new Mock<IImport5TMLine>();
			import5TMMock.Setup(m => m.EntryLines).Returns(new IImport5TMLine[] { entryLineMock.Object });

			var result = new GOVCBR5TMMessageBuilder(import5TMMock.Object).GenerateMessage();
			AssertEquals(ZString.Empty, result.Id.Value);
			AssertEquals("GOVCBR5TM", result.TypeCode.Value);
			AssertEquals(ZDate.Today.ToString(DateFormatType.Date), result.IssueDateTime);
			AssertEquals(null, result.Payer);

			AssertEquals(ZDecimal.Zero, result.GoodsShipment[0].SequenceNumeric);
			AssertEquals(ZString.Empty, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.CargoDescription.Value);
			AssertEquals(ZString.Empty, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Classification.Id.Value);
			AssertEquals(ZDecimal.Zero, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);
			AssertEquals(ZDecimal.Zero, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.Payment.TaxAssessedAmount.Value);
			AssertEquals(ZDecimal.Zero, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals(DefaultWeightUnit, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.GoodsMeasure.NetNetWeightMeasure.KcsUnitCode);

			var payerMock = new Mock<IOrganization>();
			import5TMMock.Setup(m => m.Payer).Returns(payerMock.Object);
			result = new GOVCBR5TMMessageBuilder(import5TMMock.Object).GenerateMessage();
			AssertEquals(null, result.Payer);
		}
	}
}
