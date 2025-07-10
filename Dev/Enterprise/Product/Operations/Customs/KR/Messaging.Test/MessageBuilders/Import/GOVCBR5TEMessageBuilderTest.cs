using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5TEMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IOrganization> PayerSetting(bool isIndividual)
		{
			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(x => x.IsIndividual).Returns(isIndividual);
			if (isIndividual)
			{
				payerMock.Setup(x => x.KoreanRegNoForResident).Returns("주민등록번호");
			}
			else
			{
				payerMock.Setup(x => x.BusinessRegNo).Returns("사업자등록번호");
			}
			return payerMock;
		}

		public void TestGenerateDeclarationWhenPayerIsBusiness()
		{
			var import5TEMock = new Mock<IImport5TEHeader>();
			import5TEMock.Setup(m => m.ImportDeclarationNumber).Returns("99999999999999");
			import5TEMock.Setup(m => m.SequenceNo).Returns(1);
			import5TEMock.Setup(m => m.ApplicationReason).Returns("신청사유");
			import5TEMock.Setup(m => m.BrokerID).Returns("신고인부호");

			var payerMock = PayerSetting(false);
			import5TEMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var result = new GOVCBR5TEMessageBuilder(import5TEMock.Object).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
			}
			AssertEquals("99999999999999", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5TE", result.TypeCode.Value);
			AssertEquals("1", result.VersionId.Value);
			AssertEquals("신청사유", result.Reason.Value);
			AssertEquals("신고인부호", result.Submitter.Id.Value);
			AssertEquals("사업자등록번호", result.Payer.Id.Value);
			import5TEMock.VerifyAll();
		}

		public void TestGenerateDeclarationWhenPayerIsNotBusiness()
		{
			var import5TEMock = new Mock<IImport5TEHeader>();
			import5TEMock.Setup(m => m.ImportDeclarationNumber).Returns("99999999999999");
			import5TEMock.Setup(m => m.SequenceNo).Returns(1);
			import5TEMock.Setup(m => m.ApplicationReason).Returns("신청사유");
			import5TEMock.Setup(m => m.BrokerID).Returns("신고인부호");

			var payerMock = PayerSetting(true);
			import5TEMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var result = new GOVCBR5TEMessageBuilder(import5TEMock.Object).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
			}
			AssertEquals("99999999999999", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5TE", result.TypeCode.Value);
			AssertEquals("1", result.VersionId.Value);
			AssertEquals("신청사유", result.Reason.Value);
			AssertEquals("신고인부호", result.Submitter.Id.Value);
			AssertEquals("주민등록번호", result.Payer.Id.Value);
			import5TEMock.VerifyAll();
		}
	}
}
