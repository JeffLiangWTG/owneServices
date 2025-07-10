using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.MonthlyClosing.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class IImportSpecialCaseEqualityComparerTest : TestCase
	{
		public void TestGetHashCode() => AssertEquals(0, importSpecialCaseEqualityComparer.GetHashCode(originalImportSpecialCase));

		public void TestEquals() => Assert(importSpecialCaseEqualityComparer.Equals(originalImportSpecialCase, importSpecialCaseMock.Object));

		public void TestEquals_Group()
		{
			importSpecialCaseMock.Setup(x => x.Group).Returns("B");
			AssertEquals(expected: false, importSpecialCaseEqualityComparer.Equals(originalImportSpecialCase, importSpecialCaseMock.Object));
		}

		public void TestEquals_ApplicationType()
		{
			importSpecialCaseMock.Setup(x => x.ApplicationType).Returns("S");
			AssertEquals(expected: false, importSpecialCaseEqualityComparer.Equals(originalImportSpecialCase, importSpecialCaseMock.Object));
		}

		public void TestEquals_RateOrAmountOrFactor()
		{
			importSpecialCaseMock.Setup(x => x.RateOrAmountOrFactor).Returns(4.56m);
			AssertEquals(expected: false, importSpecialCaseEqualityComparer.Equals(originalImportSpecialCase, importSpecialCaseMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			importSpecialCaseMock = CusReconBuildersTestHelper.ImportSpecialCaseMock;
			importSpecialCaseEqualityComparer = new IImportSpecialCaseEqualityComparer();
		}
		IImportSpecialCaseEqualityComparer importSpecialCaseEqualityComparer;
		Mock<IImportSpecialCase> importSpecialCaseMock;
		readonly IImportSpecialCase originalImportSpecialCase = CusReconBuildersTestHelper.ImportSpecialCaseMock.Object;
	}
}
