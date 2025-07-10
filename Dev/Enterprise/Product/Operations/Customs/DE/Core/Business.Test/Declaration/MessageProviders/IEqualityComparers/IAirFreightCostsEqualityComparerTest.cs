using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.MonthlyClosing.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class IAirFreightCostsEqualityComparerTest : TestCase
	{
		public void TestGetHashCode() => AssertEquals(0, airFreightCostsEqualityComparer.GetHashCode(originalAirFreightCosts));

		public void TestEquals() => Assert(airFreightCostsEqualityComparer.Equals(originalAirFreightCosts, airFreightCostsMock.Object));

		public void TestEquals_CurrencyRateIATA()
		{
			airFreightCostsMock.Setup(x => x.CurrencyRateIATA).Returns(false);
			AssertEquals(expected: false, airFreightCostsEqualityComparer.Equals(originalAirFreightCosts, airFreightCostsMock.Object));
		}

		public void TestEquals_CurrencyRateDate()
		{
			airFreightCostsMock.Setup(x => x.CurrencyRateDate).Returns(new DateTime(2023, 12, 8));
			AssertEquals(expected: false, airFreightCostsEqualityComparer.Equals(originalAirFreightCosts, airFreightCostsMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			airFreightCostsMock = CusReconBuildersTestHelper.AirFreightCostsMock;
			airFreightCostsEqualityComparer = new IAirFreightCostsEqualityComparer();
		}
		IAirFreightCostsEqualityComparer airFreightCostsEqualityComparer;
		Mock<IAirFreightCosts> airFreightCostsMock;
		IAirFreightCosts originalAirFreightCosts => CusReconBuildersTestHelper.AirFreightCostsMock.Object;
	}
}
