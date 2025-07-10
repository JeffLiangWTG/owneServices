using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class AmountComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, comparer.GetHashCode(originalAmount));
		}

		public void TestEquals()
		{
			AssertEquals(true, comparer.Equals(originalAmount, CreateAmount("AAA", "BBB", 1.12M)));
		}

		public void TestEquals_NullString()
		{
			var amount = CreateAmount(null, null, 1.12M);
			AssertNoExceptionThrown(() => comparer.Equals(amount, originalAmount));
		}

		public void TestEquals_Qualifier()
		{
			var amount = CreateAmount("CCC", "BBB", 1.12M);
			AssertEquals(false, comparer.Equals(originalAmount, amount));
		}

		public void TestEquals_MeasurementUnit()
		{
			var amount = CreateAmount("AAA", "CCC", 1.12M);
			AssertEquals(false, comparer.Equals(originalAmount, amount));
		}

		public void TestEquals_Quantity()
		{
			var amount = CreateAmount("AAA", "BBB", 1.34M);
			AssertEquals(false, comparer.Equals(originalAmount, amount));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalAmount = CreateAmount("AAA", "BBB", 1.12M);
			comparer = new AmountComparer();
		}
		Amount originalAmount;
		AmountComparer comparer;

		Amount CreateAmount(string qualifier, string measurementUnit, decimal quantity)
		{
			return new Amount
			{
				Qualifier = qualifier,
				MeasurementUnit = measurementUnit,
				Quantity = quantity
			};
		}
	}
}
