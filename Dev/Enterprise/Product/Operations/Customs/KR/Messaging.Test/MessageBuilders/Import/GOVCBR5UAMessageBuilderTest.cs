using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5UAMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateDeclaration()
		{
			var importHeaderMock = new Mock<IImport5UAHeader>();
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("4062001070010U");
			importHeaderMock.Setup(m => m.SequenceNo).Returns(1);
			importHeaderMock.Setup(m => m.PenaltyType).Returns("A");
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			importHeaderMock.Setup(m => m.ExemptionProcessCode).Returns("B");
			importHeaderMock.Setup(m => m.AmendmentDeclarationDate).Returns(new ZDate("2014-01-01"));
			importHeaderMock.Setup(m => m.AmendmentVersionNo).Returns(1);
			importHeaderMock.Setup(m => m.PenaltyExemptionReasonsCode).Returns("B5");
			importHeaderMock.Setup(m => m.PenaltyExemptionReason).Returns("사유내용");
			importHeaderMock.Setup(m => m.PenaltyExemptionAmount).Returns(999999999m);

			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("상호");
			declarantMock.Setup(m => m.RepresentativeName).Returns("신청인명");
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			var result = new GOVCBR5UAMessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertEquals("01010", result.DeclarationOfficeId.Value);
			AssertEquals("4062001070010U", result.Id.Value);
			AssertEquals(999999999m, result.InvoiceAmount.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5UA", result.TypeCode.Value);
			AssertEquals("1", result.VersionId.Value);
			AssertEquals("A", result.SubTypeCode.Value);
			AssertEquals("사유내용", result.Reason.Value);
			AssertEquals("B5", result.ReasonCode.Value);
			AssertEquals("B", result.Amendment.ChangeReasonCode.Value);
			AssertEquals("20140101", result.Amendment.AmendmentDateTime);
			AssertEquals(1m, result.Amendment.Pointer.SequenceNumeric);
			AssertEquals("상호", result.Submitter.Name.Value);
			AssertEquals("신청인명", result.Submitter.Contact.Name.Value);

			importHeaderMock.VerifyAll();
			declarantMock.VerifyAll();
		}

		public void TestEmptyAndDecimalPlacesPenaltyExemption()
		{
			var importHeaderMock = new Mock<IImport5UAHeader>();
			var declarantMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			importHeaderMock.Setup(m => m.PenaltyExemptionReason).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.PenaltyExemptionAmount).Returns(99.1m);
			var result = new GOVCBR5UAMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.Reason);
			//AssertEquals(99m, result.InvoiceAmount.Value);
		}

		public void TestEmptyAmendmentDateTime()
		{
			var importHeaderMock = new Mock<IImport5UAHeader>();
			importHeaderMock.Setup(m => m.AmendmentDeclarationDate).Returns(ZDate.Invalid);
			var result = new GOVCBR5UAMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(ZString.Empty, result.Amendment.AmendmentDateTime);

			importHeaderMock.Setup(m => m.AmendmentDeclarationDate).Returns(new ZDate("2012-01-01"));
			result = new GOVCBR5UAMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("20120101", result.Amendment.AmendmentDateTime);
		}
	}
}
