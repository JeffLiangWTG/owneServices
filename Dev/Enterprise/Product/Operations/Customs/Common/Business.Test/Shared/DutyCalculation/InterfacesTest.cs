using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class DutyResultTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestFlatRateAmountAndUQ()
		{
			DutyResult result = new DutyResult();
			result.FlatRateAmount = 10m;
			result.FlatRateUQ = "LA";
			NUnit.Framework.Assert.That(result.FlatRateAmount, Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "Flat rate amount");
			NUnit.Framework.Assert.That(result.FlatRateUQ, Is.EqualTo("LA").Using(CustomComparers.TypeComparison), "Flat rate UQ");
		}

		[ExpectNoExceptions]
		public void TestHasDuty()
		{
			DutyResult result = new DutyResult();
			NUnit.Framework.Assert.That(!result.HasDuty, Is.True, "No duty by default");
			result.Percent = 4;
			NUnit.Framework.Assert.That(result.HasDuty, Is.True, "Has duty");
			result.Amount = new Money(3, GlbCompany.CurrentCompany.Country.LocalCurrency);
			NUnit.Framework.Assert.That(result.HasDuty, Is.True, "Has duty");
			result.Percent = 0;
			NUnit.Framework.Assert.That(result.HasDuty, Is.True, "Has duty");
			result.Amount = new Money(0, GlbCompany.CurrentCompany.Country.LocalCurrency);
			NUnit.Framework.Assert.That(!result.HasDuty, Is.True, "No duty");
		}

		[ExpectNoExceptions]
		public void TestDutyRateDescription()
		{
			DutyResult result = new DutyResult();
			NUnit.Framework.Assert.That(result.DutyRateDescription, Is.EqualTo(ZString.Empty));

			result.Percent = 5m;
			NUnit.Framework.Assert.That(result.DutyRateDescription, Is.EqualTo("5.00%").Using(CustomComparers.TypeComparison));

			result.Percent = 0m;
			result.FlatRateAmount = 10m;
			result.FlatRateUQ = "KG";
			NUnit.Framework.Assert.That(result.DutyRateDescription, Is.EqualTo("10.00/KG").Using(CustomComparers.TypeComparison));

			result.Percent = 5m;
			NUnit.Framework.Assert.That(result.DutyRateDescription, Is.EqualTo("5.00%+10.00/KG").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUninitialisedAmount()
		{
			DutyResult result = new DutyResult();
			NUnit.Framework.Assert.That(result.Amount.Amount, Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAmount()
		{
			DutyResult result = new DutyResult();
			RefCurrency aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			const decimal MonetaryAmount = 123m;
			Money amount = new Money(MonetaryAmount, aUD);

			result.Amount = amount;
			NUnit.Framework.Assert.That(result.Amount, Is.EqualTo(amount));
		}

		[ExpectNoExceptions]
		public void TestPercent()
		{
			DutyResult result = new DutyResult();
			NUnit.Framework.Assert.That(result.Percent, Is.EqualTo(0m).Using(CustomComparers.TypeComparison));

			const decimal PercentageAmount = 5.2m;
			result.Percent = PercentageAmount;
			NUnit.Framework.Assert.That(result.Percent, Is.EqualTo(PercentageAmount).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCompareTo()
		{
			RefCurrency aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			DutyResult result1 = new DutyResult();
			result1.Amount = new Money(100, aUD);

			DutyResult result2 = new DutyResult();

			result2.Amount = new Money(101, aUD);
			NUnit.Framework.Assert.That(result1.CompareTo(result2), Is.EqualTo(-1));

			result2.Amount = new Money(100, aUD);
			NUnit.Framework.Assert.That(result1.CompareTo(result2), Is.EqualTo(0));

			result2.Amount = new Money(99, aUD);
			NUnit.Framework.Assert.That(result1.CompareTo(result2), Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestRoundDown()
		{
			RefCurrency aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			DutyResult result = new DutyResult();
			result.Amount = new Money(100.1234m, aUD);
			DutyResult resultAfterRounding = result.RoundDown(2);
			NUnit.Framework.Assert.That(resultAfterRounding.Amount.Amount, Is.EqualTo(100.12m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsValid()
		{
			RefCurrency aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			DutyResult result = new DutyResult();
			result.Amount = new Money(100.1234m, aUD, false);
			NUnit.Framework.Assert.That(!result.IsValid, Is.True);
			result.Amount = new Money(100.1234m, aUD, true);
			NUnit.Framework.Assert.That(result.IsValid, Is.True);
		}
	}
}
