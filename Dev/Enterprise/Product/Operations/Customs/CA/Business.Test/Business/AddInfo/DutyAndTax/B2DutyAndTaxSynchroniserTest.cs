using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class B2DutyAndTaxSynchroniserTest : TestCaseWithFactory
	{
		public void TestB2DutyAndTaxSynchroniser()
		{
			var source = Factory.NewWithValidTestData<DutyAndTax>();
			var destination = Factory.NewWithValidTestData<DutyAndTax>();
			var synchroniser = new B2DutyAndTaxSynchroniser(destination, source);
			synchroniser.SetEnabled(true, false);

			source.C1_TaxType = "DTY";
			AssertEquals("DTY", destination.C1_TaxType);

			source.C1_Code = "001";
			AssertEquals("001", destination.C1_Code);

			source.C1_ExemptCode = "48";
			AssertEquals("48", destination.C1_ExemptCode);

			source.C1_RateType = "X";
			AssertEquals("X", destination.C1_RateType);

			source.C1_Rate = 5m;
			AssertEquals(5m, destination.C1_Rate);

			source.C1_UnitOfMeasure = "BG";
			AssertEquals("BG", destination.C1_UnitOfMeasure);

			source.C1_Amount = 100m;
			AssertEquals(100m, destination.C1_Amount);

			source.C1_PreviousTranLine = 1;
			AssertEquals(1, destination.C1_PreviousTranLine);

			source.C1_PreviousTranNumber = "123";
			AssertEquals("123", destination.C1_PreviousTranNumber);

			source.C1_NormalValuePerUnit = 1m;
			AssertEquals(1m, destination.C1_NormalValuePerUnit);

			source.C1_NormalValueCurrency = "USD";
			AssertEquals("USD", destination.C1_NormalValueCurrency);

			source.C1_ForeignRate = 6m;
			AssertEquals(6m, destination.C1_ForeignRate);

			source.C1_ForeignCurrency = "CAD";
			AssertEquals("CAD", destination.C1_ForeignCurrency);

			source.C1_Override = true;
			AssertEquals(true, destination.C1_Override);

			destination.C1_TaxType = "GST";
			source.C1_TaxType = "SIM";
			AssertEquals("If the targer and source are different, it will not be synchronized", "GST", destination.C1_TaxType);

			destination.C1_TaxType = "SIM";
			source.C1_TaxType = "DTY";
			AssertEquals("If the targer and source are same, it will be synchronized", "DTY", destination.C1_TaxType);
		}
	}
}
