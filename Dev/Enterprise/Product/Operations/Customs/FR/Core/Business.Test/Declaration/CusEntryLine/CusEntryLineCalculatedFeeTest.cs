using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineCalculatedFee))]
	sealed class CusEntryLineCalculatedFeeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var calculatedFee = new CusEntryLineCalculatedFee(Factory, "AK", new Money(10m, currency));
			CombineAssertions(() =>
			{
				AssertEquals("Transport", calculatedFee.Category);
				AssertEquals("AK", calculatedFee.ChargeType);
				AssertEquals(10m, calculatedFee.Amount);
				AssertEquals("USD", calculatedFee.Currency);
				AssertEquals(true, calculatedFee.IsLineLevel);
				AssertEquals(new Money(10m, currency), calculatedFee.Money);
			});

			currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");
			calculatedFee = new CusEntryLineCalculatedFee(Factory, "CZ", new Money(20m, currency), false);
			CombineAssertions(() =>
			{
				AssertEquals("Other Fees", calculatedFee.Category);
				AssertEquals("CZ", calculatedFee.ChargeType);
				AssertEquals(20m, calculatedFee.Amount);
				AssertEquals("EUR", calculatedFee.Currency);
				AssertEquals(false, calculatedFee.IsLineLevel);
				AssertEquals(new Money(20m, currency), calculatedFee.Money);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CusEntryLineCalculatedFee(Factory, ZString.Empty, Money.Empty);
		}
	}
}
