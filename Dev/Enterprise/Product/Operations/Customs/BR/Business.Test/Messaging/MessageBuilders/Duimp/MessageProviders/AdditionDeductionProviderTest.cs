using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class AdditionDeductionProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(AdditionDeductionProvider.New(ZString.Empty, ZString.Empty, 0));
			AssertType<AdditionDeductionProvider>(AdditionDeductionProvider.New(ZString.Empty, ZString.Empty, 1));
		}

		public void TestProperties()
		{
			var dataProvider = AdditionDeductionProvider.New(ImportCustomsChargeTypeList.Codes.PackingCosts, ZString.Empty, 1001.1);
			CombineAssertions(() =>
			{
				AssertEquals("Type", "ACRESCIMO", dataProvider.Type);
				AssertNotNull("CurrencyCode", dataProvider.CurrencyCode);
				AssertEquals("ChargeCode", 3, dataProvider.ChargeCode);
				AssertEquals("Amount", 1001.1d, dataProvider.Amount);
			});

			dataProvider = AdditionDeductionProvider.New(ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry, ZString.Empty, 1001.1);
			CombineAssertions(() =>
			{
				AssertEquals("Type", "DEDUCAO", dataProvider.Type);
				AssertNotNull("CurrencyCode", dataProvider.CurrencyCode);
				AssertEquals("ChargeCode", 1, dataProvider.ChargeCode);
				AssertEquals("Amount", 1001.1d, dataProvider.Amount);
			});
		}
	}
}
