using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.MonthlyClosing.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ICFCPEDLineEqualityComparerTest : TestCase
	{
		public void TestGetHashCode() => AssertEquals(0, lineEqualityComparer.GetHashCode(originalCFCPEDLine));

		public void TestEquals() => Assert(lineEqualityComparer.Equals(originalCFCPEDLine, cfcpedLineMock.Object));

		public void TestEquals_CessionManagementFlag()
		{
			cfcpedLineMock.Setup(x => x.CessionManagementFlag).Returns("B");
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalCFCPEDLine, cfcpedLineMock.Object));
		}

		public void TestEquals_PreferentialOriginCountry()
		{
			cfcpedLineMock.Setup(x => x.PreferentialOriginCountry).Returns("FR");
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalCFCPEDLine, cfcpedLineMock.Object));
		}

		public void TestEquals_TobaccoRevenueStampNumber()
		{
			cfcpedLineMock.Setup(x => x.TobaccoRevenueStampNumber).Returns("5678");
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalCFCPEDLine, cfcpedLineMock.Object));
		}

		public void TestEquals_AssessmentOutwardProcessingFee()
		{
			cfcpedLineMock.Setup(x => x.AssessmentOutwardProcessingFee).Returns(4.56m);
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalCFCPEDLine, cfcpedLineMock.Object));
		}

		public void TestEquals_AssessmentTaxCosts()
		{
			cfcpedLineMock.Setup(x => x.AssessmentTaxCosts).Returns(6.54m);
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalCFCPEDLine, cfcpedLineMock.Object));
		}

		public void TestEquals_PreferentialTreatment()
		{
			var linePreferentialTreatmentMock = ILinePreferentialTreatmentEqualityComparerTest.PreferentialTreatmentMock;
			linePreferentialTreatmentMock.Setup(x => x.RequestedPreferentialTreatment).Returns("Y");
			cfcpedLineMock.Setup(x => x.PreferentialTreatment).Returns(linePreferentialTreatmentMock.Object);
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalCFCPEDLine, cfcpedLineMock.Object));
		}

		public void TestEquals_SpecialCase()
		{
			var importSpecialCaseMock = CusReconBuildersTestHelper.ImportSpecialCaseMock;
			importSpecialCaseMock.Setup(x => x.Group).Returns("B");
			cfcpedLineMock.Setup(x => x.SpecialCase).Returns(new[] { CusReconBuildersTestHelper.ImportSpecialCaseMock.Object, importSpecialCaseMock.Object });
			AssertEquals(expected: false, lineEqualityComparer.Equals(originalCFCPEDLine, cfcpedLineMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			cfcpedLineMock = CusReconBuildersTestHelper.CFCPEDLineMock;
			lineEqualityComparer = new ICFCPEDLineEqualityComparer();
		}
		ICFCPEDLineEqualityComparer lineEqualityComparer;
		Mock<ICFCPEDLine> cfcpedLineMock;
		ICFCPEDLine originalCFCPEDLine => CusReconBuildersTestHelper.CFCPEDLineMock.Object;
	}
}
