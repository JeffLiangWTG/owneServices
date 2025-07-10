using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.MonthlyClosing.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class IImportLineCustomsValueEqualityComparerTest : TestCase
	{
		public void TestGetHashCode() => AssertEquals(0, importLineCustomsValueEqualityComparer.GetHashCode(originalImportLineCustomsValue));

		public void TestEquals() => Assert(importLineCustomsValueEqualityComparer.Equals(originalImportLineCustomsValue, importLineCustomsValueMock.Object));

		public void TestEquals_CustomsValueDepartureAirport()
		{
			importLineCustomsValueMock.Setup(x => x.CustomsValueDepartureAirport).Returns("BER");
			AssertEquals(false, importLineCustomsValueEqualityComparer.Equals(originalImportLineCustomsValue, importLineCustomsValueMock.Object));
		}

		public void TestEquals_CustomsValueDestinationPlace()
		{
			importLineCustomsValueMock.Setup(x => x.CustomsValueDestinationPlace).Returns("Frankfurt");
			AssertEquals(false, importLineCustomsValueEqualityComparer.Equals(originalImportLineCustomsValue, importLineCustomsValueMock.Object));
		}

		public void TestEquals_CustomsValueAdditionDeductionDescription()
		{
			importLineCustomsValueMock.Setup(x => x.CustomsValueAdditionDeductionDescription).Returns("Description2");
			AssertEquals(false, importLineCustomsValueEqualityComparer.Equals(originalImportLineCustomsValue, importLineCustomsValueMock.Object));
		}

		public void TestEquals_CustomsValueNetPrice()
		{
			var netPriceMock = CusReconBuildersTestHelper.ImportCostsMock;
			netPriceMock.Setup(x => x.CurrencyRate).Returns(1.1m);
			importLineCustomsValueMock.Setup(x => x.CustomsValueNetPrice).Returns(netPriceMock.Object);
			AssertEquals(false, importLineCustomsValueEqualityComparer.Equals(originalImportLineCustomsValue, importLineCustomsValueMock.Object));
		}

		public void TestEquals_CustomsValueIndirectPayment()
		{
			var indirectPaymentMock = CusReconBuildersTestHelper.ImportCostsMock;
			indirectPaymentMock.Setup(x => x.CurrencyRate).Returns(1.1m);
			importLineCustomsValueMock.Setup(x => x.CustomsValueIndirectPayment).Returns(indirectPaymentMock.Object);
			AssertEquals(false, importLineCustomsValueEqualityComparer.Equals(originalImportLineCustomsValue, importLineCustomsValueMock.Object));
		}

		public void TestEquals_CustomsValueAirFreightCosts()
		{
			var airFreightCostsMock = CusReconBuildersTestHelper.AirFreightCostsMock;
			airFreightCostsMock.Setup(x => x.CurrencyRateIATA).Returns(false);
			importLineCustomsValueMock.Setup(x => x.CustomsValueAirFreightCosts).Returns(airFreightCostsMock.Object);
			AssertEquals(false, importLineCustomsValueEqualityComparer.Equals(originalImportLineCustomsValue, importLineCustomsValueMock.Object));
		}

		public void TestEquals_CustomsValueAdditionDeduction()
		{
			var additionDeductionMock = CusReconBuildersTestHelper.AdditionDeductionMock;
			additionDeductionMock.Setup(x => x.Type).Returns("C");
			importLineCustomsValueMock.Setup(x => x.CustomsValueAdditionDeduction).Returns(new[] { additionDeductionMock.Object, CusReconBuildersTestHelper.AdditionDeductionMock.Object });
			AssertEquals(false, importLineCustomsValueEqualityComparer.Equals(originalImportLineCustomsValue, importLineCustomsValueMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			importLineCustomsValueMock = CusReconBuildersTestHelper.ImportLineCustomsValueMock;
			importLineCustomsValueEqualityComparer = new IImportLineCustomsValueEqualityComparer();
		}
		IImportLineCustomsValueEqualityComparer importLineCustomsValueEqualityComparer;
		Mock<IImportLineCustomsValue> importLineCustomsValueMock;
		readonly IImportLineCustomsValue originalImportLineCustomsValue = CusReconBuildersTestHelper.ImportLineCustomsValueMock.Object;
	}
}
