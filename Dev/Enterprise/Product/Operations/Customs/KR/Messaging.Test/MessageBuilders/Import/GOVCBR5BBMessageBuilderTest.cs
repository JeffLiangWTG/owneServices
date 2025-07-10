using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5BBMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImport5BBHeader> importHeaderMock;
		Mock<IAmendmentDetails> amendmentMock;

		protected override void SetUp()
		{
			base.SetUp();

			importHeaderMock = new Mock<IImport5BBHeader>();
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("1234520001542X");
			importHeaderMock.Setup(m => m.TariffRateClassification).Returns("CUD");
			importHeaderMock.Setup(m => m.TariffRate).Returns(4.2m);

			amendmentMock = new Mock<IAmendmentDetails>();
			amendmentMock.Setup(m => m.AmendmentType).Returns("A");
			amendmentMock.Setup(m => m.AmendmentVersionNo).Returns(1);
			amendmentMock.Setup(m => m.AmendReasonDescription).Returns("정정사유");

			var importLineMock1 = new Mock<IImport5BBLine>();
			importLineMock1.Setup(m => m.EntryLineNo).Returns(1);
			importLineMock1.Setup(m => m.HSCode).Returns("0000000001");
			importLineMock1.Setup(m => m.HSDescription).Returns("품명");
			importLineMock1.Setup(m => m.InvoiceDescription).Returns("거래품명");
			importLineMock1.Setup(m => m.AmendDataItemID).Returns("1");
			importLineMock1.Setup(m => m.BeforeDescription).Returns("A-1");
			importLineMock1.Setup(m => m.AfterDescription).Returns("B-1");

			var importLineMock2 = new Mock<IImport5BBLine>();
			importLineMock2.Setup(m => m.EntryLineNo).Returns(2);
			importLineMock2.Setup(m => m.HSCode).Returns("0000000002");
			importLineMock2.Setup(m => m.HSDescription).Returns("품명2");
			importLineMock2.Setup(m => m.InvoiceDescription).Returns("거래품명2");
			importLineMock2.Setup(m => m.AmendDataItemID).Returns("2");
			importLineMock2.Setup(m => m.BeforeDescription).Returns("C-1");
			importLineMock2.Setup(m => m.AfterDescription).Returns("D-1");
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImport5BBLine[] { importLineMock1.Object, importLineMock2.Object });

			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("신청인상호");
			declarantMock.Setup(m => m.IsIndividual).Returns(ZBool.False);
			declarantMock.Setup(m => m.CountryCode).Returns("KR");
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2014, 01, 01)]
		public void TestGenerateDeclaration()
		{
			var result = new GOVCBR5BBMessageBuilder(importHeaderMock.Object, amendmentMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR5BB_0.xml"));
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
			AssertEquals("1234520001542X", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5BB", result.TypeCode.Value);
			AssertEquals("1", result.VersionId.Value);
			AssertEquals("A", result.TransactionNatureCode.Value);
			AssertEquals("정정사유", result.Reason.Value);
			AssertEquals(1m, result.Consignment[0].SequenceNumeric);
			AssertEquals("거래품명", result.Consignment[0].ConsignmentItem[0].Commodity.CargoDescription.Value);
			AssertEquals("품명", result.Consignment[0].ConsignmentItem[0].Commodity.Description.Value);
			AssertEquals("0000000001", result.Consignment[0].ConsignmentItem[0].Commodity.Classification.Id.Value);
			AssertEquals(4.2m, result.Consignment[0].ConsignmentItem[0].Commodity.DutyTaxFee.TaxRateNumeric);
			AssertEquals("CUD", result.Consignment[0].ConsignmentItem[0].Commodity.DutyTaxFee.TypeCode.Value);
			AssertEquals(2m, result.Consignment[1].SequenceNumeric);
			AssertEquals("거래품명2", result.Consignment[1].ConsignmentItem[0].Commodity.CargoDescription.Value);
			AssertEquals("품명2", result.Consignment[1].ConsignmentItem[0].Commodity.Description.Value);
			AssertEquals("0000000002", result.Consignment[1].ConsignmentItem[0].Commodity.Classification.Id.Value);
			AssertEquals("신청인상호", result.Submitter.Name.Value);
		}

		public void TestNotCreateElementWhenNoMandatoryAmendmentTypeAdd()
		{
			var emptyMock = new Mock<IImport5BBHeader>();
			emptyMock.Setup(m => m.EntryLines).Returns(new IImport5BBLine[] { new Mock<IImport5BBLine>().Object });
			var emptyAmendmentMock = new Mock<IAmendmentDetails>();
			emptyAmendmentMock.Setup(m => m.AmendmentType).Returns("A");
			var result = new GOVCBR5BBMessageBuilder(emptyMock.Object, emptyAmendmentMock.Object).GenerateMessage();

			AssertEquals(ZString.Empty, result.DeclarationOfficeId.Value);
			AssertEquals(ZString.Empty, result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5BB", result.TypeCode.Value);
			AssertEquals("0", result.VersionId.Value);
			AssertEquals("A", result.TransactionNatureCode.Value);
			AssertEquals(ZString.Empty, result.Reason.Value);

			AssertEquals(ZDecimal.Zero, result.Consignment[0].SequenceNumeric);
			AssertEquals(ZString.Empty, result.Consignment[0].ConsignmentItem[0].Commodity.CargoDescription.Value);
			AssertEquals(ZString.Empty, result.Consignment[0].ConsignmentItem[0].Commodity.Description.Value);
			AssertEquals(ZString.Empty, result.Consignment[0].ConsignmentItem[0].Commodity.Classification.Id.Value);
			AssertEquals(ZDecimal.Zero, result.Consignment[0].ConsignmentItem[0].Commodity.DutyTaxFee.TaxRateNumeric);
			AssertEquals(ZString.Empty, result.Consignment[0].ConsignmentItem[0].Commodity.DutyTaxFee.TypeCode.Value);
			AssertEquals(0, result.Consignment[0].Amendment.Count);

			AssertEquals(ZString.Empty, result.Submitter.Name.Value);
		}

		public void TestNotCreateElementWhenNoMandatoryAmendmentTypeUpdate()
		{
			var emptyMock = new Mock<IImport5BBHeader>();
			emptyMock.Setup(m => m.EntryLines).Returns(new IImport5BBLine[] { new Mock<IImport5BBLine>().Object });
			var emptyAmendmentMock = new Mock<IAmendmentDetails>();
			emptyAmendmentMock.Setup(m => m.AmendmentType).Returns("U");
			var result = new GOVCBR5BBMessageBuilder(emptyMock.Object, emptyAmendmentMock.Object).GenerateMessage();

			AssertEquals(ZString.Empty, result.DeclarationOfficeId.Value);
			AssertEquals(ZString.Empty, result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5BB", result.TypeCode.Value);
			AssertEquals("0", result.VersionId.Value);
			AssertEquals("U", result.TransactionNatureCode.Value);
			AssertEquals(ZString.Empty, result.Reason.Value);

			AssertEquals(ZDecimal.Zero, result.Consignment[0].SequenceNumeric);
			AssertEquals(0, result.Consignment[0].ConsignmentItem.Count);
			AssertEquals(ZString.Empty, result.Consignment[0].Amendment[0].StatementDescription.Value);
			AssertEquals(ZString.Empty, result.Consignment[0].Amendment[0].AdjustmentDescription.Value);
			AssertEquals(ZString.Empty, result.Consignment[0].Amendment[0].Pointer.TagId.Value);

			AssertEquals(ZString.Empty, result.Submitter.Name.Value);
		}

		public void TestNotCreateElementWhenNoMandatoryAmendmentTypeNull()
		{
			var emptyMock = new Mock<IImport5BBHeader>();
			var emptyAmendmentMock = new Mock<IAmendmentDetails>();
			var result = new GOVCBR5BBMessageBuilder(emptyMock.Object, emptyAmendmentMock.Object).GenerateMessage();

			AssertEquals(ZString.Empty, result.DeclarationOfficeId.Value);
			AssertEquals(ZString.Empty, result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5BB", result.TypeCode.Value);
			AssertEquals("0", result.VersionId.Value);
			AssertEquals(ZString.Empty, result.TransactionNatureCode.Value);
			AssertEquals(ZString.Empty, result.Reason.Value);

			AssertEquals(ZDecimal.Zero, result.Consignment[0].SequenceNumeric);
			AssertEquals(0, result.Consignment[0].ConsignmentItem.Count);
			AssertEquals(0, result.Consignment[0].Amendment.Count);

			AssertEquals(ZString.Empty, result.Submitter.Name.Value);
		}
	}
}
