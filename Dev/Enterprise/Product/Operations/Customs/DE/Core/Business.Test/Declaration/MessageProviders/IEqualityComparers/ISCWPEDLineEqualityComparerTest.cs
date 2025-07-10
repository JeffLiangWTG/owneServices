using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.MonthlyClosing.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ISCWPEDLineEqualityComparerTest : TestCase
	{
		public void TestGetHashCode() => AssertEquals(0, lineEqualityComparer.GetHashCode(originalSCWPEDLine));

		public void TestEquals() => Assert(lineEqualityComparer.Equals(originalSCWPEDLine, scwpedLineMock.Object));

		public void TestEquals_RequestedPreferentialTreatment()
		{
			scwpedLineMock.Setup(x => x.RequestedPreferentialTreatment).Returns("210");
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalSCWPEDLine, scwpedLineMock.Object));
		}

		public void TestEquals_InwardMovementAmount()
		{
			scwpedLineMock.Setup(x => x.InwardMovementAmount).Returns(CusReconBuildersTestHelper.GetAmount("Y", 18219, "NAR"));
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalSCWPEDLine, scwpedLineMock.Object));
		}

		public void TestEquals_ForeignTradeImportEarlyClearanceFlag()
		{
			scwpedLineMock.Setup(x => x.ForeignTradeImportEarlyClearanceFlag).Returns("N");
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalSCWPEDLine, scwpedLineMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			scwpedLineMock = CusReconBuildersTestHelper.SCWPEDLineMock;
			lineEqualityComparer = new ISCWPEDLineEqualityComparer();
		}
		ISCWPEDLineEqualityComparer lineEqualityComparer;
		Mock<ISCWPEDLine> scwpedLineMock;
		ISCWPEDLine originalSCWPEDLine => CusReconBuildersTestHelper.SCWPEDLineMock.Object;
	}
}
