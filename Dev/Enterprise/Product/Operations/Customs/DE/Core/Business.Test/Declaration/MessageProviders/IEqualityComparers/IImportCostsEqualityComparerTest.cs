using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.MonthlyClosing.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class IImportCostsEqualityComparerTest : TestCase
	{
		public void TestGetHashCode() => AssertEquals(0, importCostsEqualityComparer.GetHashCode(originalImportCosts));

		public void TestEquals() => Assert(importCostsEqualityComparer.Equals(originalImportCosts, importCostsMock.Object));

		public void TestEquals_CurrencyRateAgreedFlag()
		{
			importCostsMock.Setup(x => x.CurrencyRateAgreedFlag).Returns(false);
			AssertEquals(false, importCostsEqualityComparer.Equals(originalImportCosts, importCostsMock.Object));
		}

		public void TestEquals_CurrencyRate()
		{
			importCostsMock.Setup(x => x.CurrencyRate).Returns(3.21m);
			AssertEquals(false, importCostsEqualityComparer.Equals(originalImportCosts, importCostsMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			importCostsMock = CusReconBuildersTestHelper.ImportCostsMock;
			importCostsEqualityComparer = new IImportCostsEqualityComparer();
		}
		IImportCostsEqualityComparer importCostsEqualityComparer;
		Mock<IImportCosts> importCostsMock;
		IImportCosts originalImportCosts => CusReconBuildersTestHelper.ImportCostsMock.Object;
	}
}
