using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class CalculateFreightBizObjValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckInsuranceAmount()
		{
			bizObj.InsuranceAmount = ZDecimal.Zero;
			bizObj.Amount = ZDecimal.Zero;
			AssertHasError(bizObj.InsuranceAmountInfo, "The freight and insurance amounts cannot both be 0");
			bizObj.InsuranceAmount = 150;
			AssertNoError(bizObj.InsuranceAmountInfo, "The freight and insurance amounts cannot both be 0");
			bizObj.Amount = 150;
			bizObj.InsuranceAmount = ZDecimal.Zero;
			AssertNoError(bizObj.InsuranceAmountInfo, "The freight and insurance amounts cannot both be 0");
		}

		public void TestCheckInsuranceCurrency()
		{
			bizObj.InsuranceCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertNoErrorContaining(bizObj.InsuranceCurrencyInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(bizObj.InsuranceCurrencyInfo, ListValidation.InvalidCodeError);
			bizObj.InsuranceCurrency = ZString.Empty;
			AssertHasErrorContaining(bizObj.InsuranceCurrencyInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(bizObj.InsuranceCurrencyInfo, ListValidation.InvalidCodeError);
			AssertHasErrorContaining(bizObj.InsuranceCurrencyInfo, CalculateFreightBizObjValidation.InconsistentInsuranceCurrencies);
			bizObj.InsuranceAmount = 10m;
			bizObj.Validation.ValidateInsuranceCurrency();
			AssertNoErrorContaining(bizObj.InsuranceCurrencyInfo, CalculateFreightBizObjValidation.InconsistentInsuranceCurrencies);
			bizObj.InsuranceCurrency = "!@";
			AssertNoErrorContaining(bizObj.InsuranceCurrencyInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(bizObj.InsuranceCurrencyInfo, ListValidation.InvalidCodeError);
		}

		public void TestPercentageInEUBorder()
		{
			var message = CalculateFreightBizObjValidation.PercentageShouldBeBetween;
			bizObj.PercentageInEUBorder = -1;
			AssertHasError(bizObj.PercentageInEUBorderInfo, message);
			bizObj.PercentageInEUBorder = ZDecimal.Zero;
			AssertNoError(bizObj.PercentageInEUBorderInfo, message);
			bizObj.PercentageInEUBorder = 101;
			AssertHasError(bizObj.PercentageInEUBorderInfo, message);
			bizObj.PercentageInEUBorder = 100;
			AssertNoError(bizObj.PercentageInEUBorderInfo, message);
		}

		public void TestPercentageDomestic()
		{
			var message = CalculateFreightBizObjValidation.PercentageShouldBeBetween;
			bizObj.PercentageDomestic = -1;
			AssertHasError(bizObj.PercentageDomesticInfo, message);
			bizObj.PercentageDomestic = ZDecimal.Zero;
			AssertNoError(bizObj.PercentageDomesticInfo, message);
			bizObj.PercentageDomestic = 101;
			AssertHasError(bizObj.PercentageDomesticInfo, message);
			bizObj.PercentageDomestic = 100;
			AssertNoError(bizObj.PercentageDomesticInfo, message);
		}

		public void TestCheckTotalPercentage()
		{
			var message = CalculateFreightBizObjValidation.PercentageShouldBe100Totally;
			bizObj.Amount = 10000m;
			bizObj.Percentage = 10;
			bizObj.PercentageInEUBorder = 20;
			bizObj.PercentageDomestic = 80;
			bizObj.Validation.ValidateAll();
			AssertHasError(bizObj.PercentageInfo, message);
			AssertHasError(bizObj.PercentageInEUBorderInfo, message);
			AssertHasError(bizObj.PercentageDomesticInfo, message);

			bizObj.PercentageDomestic = 60;
			bizObj.Validation.ValidateAll();
			AssertHasError(bizObj.PercentageInfo, message);
			AssertHasError(bizObj.PercentageInEUBorderInfo, message);
			AssertHasError(bizObj.PercentageDomesticInfo, message);

			bizObj.PercentageDomestic = 70;
			bizObj.Validation.ValidateAll();
			AssertNoError(bizObj.PercentageInfo, message);
			AssertNoError(bizObj.PercentageInEUBorderInfo, message);
			AssertNoError(bizObj.PercentageDomesticInfo, message);
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
