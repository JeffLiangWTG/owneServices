using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CalculateFreightBizObjValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPercentage()
		{
			bizObj.Percentage = -1;
			AssertHasError(bizObj.PercentageInfo, CalculateFreightBizObjValidation.PercentageShouldBeBetween);
			bizObj.Percentage = ZDecimal.Zero;
			AssertNoError(bizObj.PercentageInfo, CalculateFreightBizObjValidation.PercentageShouldBeBetween);
			bizObj.Percentage = 101;
			AssertHasError(bizObj.PercentageInfo, CalculateFreightBizObjValidation.PercentageShouldBeBetween);
			bizObj.Percentage = 100;
			AssertNoError(bizObj.PercentageInfo, CalculateFreightBizObjValidation.PercentageShouldBeBetween);
		}

		public void TestCheckCurrency()
		{
			bizObj.Currency = Core.Constants.CurrencyCodes.Australia;
			AssertNoErrorContaining(bizObj.CurrencyInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(bizObj.CurrencyInfo, ListValidation.InvalidCodeError);
			bizObj.Currency = ZString.Empty;
			AssertHasErrorContaining(bizObj.CurrencyInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(bizObj.CurrencyInfo, ListValidation.InvalidCodeError);
			AssertHasErrorContaining(bizObj.CurrencyInfo, CalculateFreightBizObjValidation.InconsistentCurrencies);
			bizObj.Amount = 10m;
			bizObj.Validation.ValidateCurrency();
			AssertNoErrorContaining(bizObj.CurrencyInfo, CalculateFreightBizObjValidation.InconsistentCurrencies);
			bizObj.Currency = "!@";
			AssertNoErrorContaining(bizObj.CurrencyInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(bizObj.CurrencyInfo, ListValidation.InvalidCodeError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		CalculateFreightBizObj bizObj;
	}
}
