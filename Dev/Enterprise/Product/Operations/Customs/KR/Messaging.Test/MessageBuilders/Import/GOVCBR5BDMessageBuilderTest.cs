using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5BDMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImport5BDHeader> importHeaderMock;

		protected override void SetUp()
		{
			base.SetUp();

			importHeaderMock = new Mock<IImport5BDHeader>();
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("9999999999");
			importHeaderMock.Setup(m => m.RequestReason).Returns("신청사유");
			importHeaderMock.Setup(m => m.SecurityType).Returns("01");
			importHeaderMock.Setup(m => m.SecurityStartDate).Returns(new ZDate("2014-01-01"));
			importHeaderMock.Setup(m => m.SecurityEndDate).Returns(new ZDate("2014-01-01"));
			importHeaderMock.Setup(m => m.SecurityAmount).Returns(9999999999m);
			importHeaderMock.Setup(m => m.OtherSecurityType).Returns("종류");
			importHeaderMock.Setup(m => m.ReasonForEarlyRemoval).Returns("01");
		}

		public void TestGenerateDeclaration()
		{
			var result = new GOVCBR5BDMessageBuilder(importHeaderMock.Object).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				Assert("XmlDeclaration should be omitted. It will be inserted into another xml by eHub", !serialisedXml.StartsWith("<?xml version="));
			}

			AssertEquals("9999999999", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("신청사유", result.Reason.Value);
			AssertEquals("01", result.ObligationGuarantee.SecurityDetailsCode.Value);
			AssertEquals("20140101", result.ObligationGuarantee.SecurityEffectiveDateTime);
			AssertEquals("20140101", result.ObligationGuarantee.LpcoExpirationDateTime);
			AssertEquals(9999999999m, result.ObligationGuarantee.SecurityAmount.Value);
			AssertEquals("종류", result.AdditionalInformation.Content.Value);
			AssertEquals("01", result.ReasonCode.Value);
		}

		public void TestNotCreateElementWhenNoMandatory()
		{
			var emptyMock = new Mock<IImport5BDHeader>();
			var result = new GOVCBR5BDMessageBuilder(emptyMock.Object).GenerateMessage();

			AssertEquals(ZString.Empty, result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals(ZString.Empty, result.Reason.Value);
			AssertEquals(ZString.Empty, result.ObligationGuarantee.SecurityDetailsCode.Value);
			AssertEquals(ZString.Empty, result.ObligationGuarantee.SecurityEffectiveDateTime);
			AssertEquals(ZString.Empty, result.ObligationGuarantee.LpcoExpirationDateTime);
			AssertEquals(ZDecimal.Zero, result.ObligationGuarantee.SecurityAmount.Value);
			AssertNull(result.AdditionalInformation);
			AssertEquals(ZString.Empty, result.ReasonCode.Value);

			emptyMock.Setup(m => m.SecurityStartDate).Returns(ZDate.Invalid);
			emptyMock.Setup(m => m.SecurityEndDate).Returns(ZDate.Invalid);
			result = new GOVCBR5BDMessageBuilder(emptyMock.Object).GenerateMessage();
			AssertEquals(ZString.Empty, result.ObligationGuarantee.SecurityEffectiveDateTime);
			AssertEquals(ZString.Empty, result.ObligationGuarantee.LpcoExpirationDateTime);
		}
	}
}
