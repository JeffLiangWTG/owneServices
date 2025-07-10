using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class IMoneyEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, moneyEqualityComparer.GetHashCode(money));
		}

		public void TestEquals()
		{
			AssertEquals(true, moneyEqualityComparer.Equals(money, MoneyMock.Object));
		}

		public void TestEquals_CurrencyCode()
		{
			var modifiedCustomsValueMock = MoneyMock;
			modifiedCustomsValueMock.Setup(x => x.CurrencyCode).Returns("AUR");
			AssertEquals(false, moneyEqualityComparer.Equals(money, modifiedCustomsValueMock.Object));
		}

		public void TestEquals_Value()
		{
			var modifiedCustomsValueMock = MoneyMock;
			modifiedCustomsValueMock.Setup(x => x.Value).Returns(2.22M);
			AssertEquals(false, moneyEqualityComparer.Equals(money, modifiedCustomsValueMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			money = MoneyMock.Object;
			moneyEqualityComparer = new IMoneyEqualityComparer();
		}
		IMoney money;
		IMoneyEqualityComparer moneyEqualityComparer;

		Mock<IMoney> MoneyMock
		{
			get
			{
				var result = new Mock<IMoney>();
				result.Setup(x => x.CurrencyCode).Returns("EUR");
				result.Setup(x => x.Value).Returns(1.11M);
				return result;
			}
		}
	}
}
