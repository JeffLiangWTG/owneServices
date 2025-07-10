using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5BFMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImport5BFHeader> importHeaderMock;

		protected override void SetUp()
		{
			base.SetUp();

			importHeaderMock = new Mock<IImport5BFHeader>();
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("010201400000015");
			importHeaderMock.Setup(m => m.ApplicationReason).Returns("신청사유");
		}

		public void TestGenerateDeclaration()
		{
			var result = new GOVCBR5BFMessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertEquals("010201400000015", result.Id.Value);
			AssertEquals("신청사유", result.Reason.Value);
			AssertEquals("GOVCBR5BF", result.TypeCode.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
		}
	}
}
