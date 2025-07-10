using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.MonthlyClosing.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class IAdditionDeductionEqualityComparerTest : TestCase
	{
		public void TestGetHashCode() => AssertEquals(0, additionDeductionEqualityComparer.GetHashCode(originalAdditionDeduction));

		public void TestEquals() => Assert(additionDeductionEqualityComparer.Equals(originalAdditionDeduction, additionDeductionMock.Object));

		public void TestEquals_Type()
		{
			additionDeductionMock.Setup(x => x.Type).Returns("Q");
			AssertEquals(expected: false, additionDeductionEqualityComparer.Equals(originalAdditionDeduction, additionDeductionMock.Object));
		}

		public void TestEquals_CurrencyRateIATA()
		{
			additionDeductionMock.Setup(x => x.CurrencyRateIATA).Returns(false);
			AssertEquals(expected: false, additionDeductionEqualityComparer.Equals(originalAdditionDeduction, additionDeductionMock.Object));
		}

		public void TestEquals_CurrencyRateDate()
		{
			additionDeductionMock.Setup(x => x.CurrencyRateDate).Returns(new DateTime(2023, 12, 8));
			AssertEquals(expected: false, additionDeductionEqualityComparer.Equals(originalAdditionDeduction, additionDeductionMock.Object));
		}

		public void TestEquals_Percentage()
		{
			additionDeductionMock.Setup(x => x.Percentage).Returns(1.23m);
			AssertEquals(expected: false, additionDeductionEqualityComparer.Equals(originalAdditionDeduction, additionDeductionMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			additionDeductionMock = CusReconBuildersTestHelper.AdditionDeductionMock;
			additionDeductionEqualityComparer = new IAdditionDeductionEqualityComparer();
		}
		IAdditionDeduction originalAdditionDeduction => CusReconBuildersTestHelper.AdditionDeductionMock.Object;
		Mock<IAdditionDeduction> additionDeductionMock;
		IAdditionDeductionEqualityComparer additionDeductionEqualityComparer;
	}
}

