using Enterprise.Customs.DE.Business.MonthlyClosing.Testing;
using Enterprise.Customs.DE.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ISCIPEDLineEqualityComparerTest : TestCase
	{
		public void TestGetHashCode() => AssertEquals(0, lineEqualityComparer.GetHashCode(originalSCIPEDLine));

		public void TestEquals() => Assert(lineEqualityComparer.Equals(originalSCIPEDLine, scipedLineMock.Object));

		public void TestEquals_RequestedPreferentialTreatment()
		{
			scipedLineMock.Setup(x => x.RequestedPreferentialTreatment).Returns("B");
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalSCIPEDLine, scipedLineMock.Object));
		}

		public void TestEquals_InwardMovementAmount()
		{
			scipedLineMock.Setup(x => x.InwardMovementAmount).Returns(CusReconBuildersTestHelper.GetAmount("Y", 18219, "NAR"));
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalSCIPEDLine, scipedLineMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			scipedLineMock = CusReconBuildersTestHelper.SCIPEDLineMock;
			lineEqualityComparer = new ISCIPEDLineEqualityComparer();
		}
		ISCIPEDLineEqualityComparer lineEqualityComparer;
		Mock<ISCIPEDLine> scipedLineMock;
		ISCIPEDLine originalSCIPEDLine => CusReconBuildersTestHelper.SCIPEDLineMock.Object;
	}
}
