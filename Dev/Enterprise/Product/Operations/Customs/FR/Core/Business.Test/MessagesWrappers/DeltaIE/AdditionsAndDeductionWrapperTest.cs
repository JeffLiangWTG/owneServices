using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class AdditionsAndDeductionWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionsAndDeductionWrapper>
	{
		protected override AdditionsAndDeductionWrapper GetProvider()
		{
			return AdditionsAndDeductionWrapper.New(CustomsChargeTypeList.Codes.OtherCharges, 12m, "USD");
		}

		public void TestCurrency()
		{
			AssertEquals("USD", Provider.Currency);
		}

		public void TestAmount()
		{
			AssertEquals(12d, Provider.Amount);
		}

		public void TestCode()
		{
			AssertEquals(CustomsChargeTypeList.Codes.OtherCharges, Provider.Code);
		}
	}
}
