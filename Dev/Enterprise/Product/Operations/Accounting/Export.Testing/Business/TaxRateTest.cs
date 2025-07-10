using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Export.Business.Testing
{
	class TaxRateTest : TestCaseWithFactory
	{
		#region TestGetExtraTaxAmountFromTaxAmount

		public void TestGetExtraTaxAmountFromTaxAmount_Italy()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				SetupAndAssertExtraTaxAmount();
			}
		}

		public void TestGetExtraTaxAmountFromTaxAmount_CostaRica()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.CostaRica))
			{
				SetupAndAssertExtraTaxAmount();
			}
		}

		void SetupAndAssertExtraTaxAmount()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("IVASPV", "SPLIT PAYMENT", AccTaxRate.Types.Rated, 10, AccTaxRate.ExtraTypes.VATRemittedByCustomer, 0, 1);

			var result = TaxRate.GetExtraTaxAmountFromTaxAmount(0M, taxRate.GetRate_ForTestOnly(), taxRate.AT_Type, taxRate.GetExtraRate_ForTestOnly(), taxRate.AT_ExtraTaxRateType, GlbCompany.CurrentCompany.Country.RN_Code, 100M);

			AssertEquals(-10M, result);
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
