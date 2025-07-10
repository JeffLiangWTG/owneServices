using System;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ExciseDutyProviderTest : Customs.Business.Testing.DataProviderTestCase<ExciseDutyProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ExciseDutyProvider(null));
		}

		public void TestCode()
		{
			AssertEquals("AB1", dataProvider.Code);
		}

		public void TestDegreePercentage()
		{
			AssertEquals(12.51m, dataProvider.DegreePercentage);
		}

		public void TestValue()
		{
			AssertEquals("ExciseValue rounds 2 decimal places", 20.25m, dataProvider.Value);
		}

		public void TestValue_Format()
		{
			tariff.BZ_TobaccoRetailPrice = 1.015m;
			AssertEquals("20.3", dataProvider.Value.ToString());
		}

		public void TestAmount()
		{
			var exciseDuty = Provider.Amount;
			AssertEquals("Cached", exciseDuty, Provider.Amount);
		}

		public void TestAmount_WrongType()
		{
			tariff.BZ_Type = "ABC";
			AssertNull(Provider.Amount);
		}

		protected override void SetUp()
		{
			tariff = Factory.New<CusLineTariffDetail>();
			tariff.BZ_Tariff = "AB1";
			tariff.BZ_PercentAlcohol = 12.505m;
			tariff.BZ_TobaccoRetailPrice = 1.0124m;
			tariff.BZ_Qty1 = 20;
			tariff.BZ_ParentTableCode = "JI";
			dataProvider = new ExciseDutyProvider(tariff);
		}
		ExciseDutyProvider dataProvider;
		CusLineTariffDetail tariff;

		protected override ExciseDutyProvider GetProvider() => dataProvider;
	}
}
