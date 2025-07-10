using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.ARAP.Invoicing.Testing
{
	public class InvoiceAmountRounderTest : TestCaseWithFactory
	{
		public void TestApplyCustomRounding()
		{
			#region Amount = 1081.23M;

			AssertRoundingAmountAndErrorMessage(0.02M, 1081.23M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.FiveMinor);
			AssertRoundingAmountAndErrorMessage(0.07M, 1081.23M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TenMinor);
			AssertRoundingAmountAndErrorMessage(0.17M, 1081.23M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TwentyMinor);
			AssertRoundingAmountAndErrorMessage(0.27M, 1081.23M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.FiftyMinor);
			AssertRoundingAmountAndErrorMessage(0.77M, 1081.23M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.OneMajor);

			AssertRoundingAmountAndErrorMessage(-0.03M, 1081.23M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.FiveMinor);
			AssertRoundingAmountAndErrorMessage(-0.03M, 1081.23M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.TenMinor);
			AssertRoundingAmountAndErrorMessage(-0.03M, 1081.23M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.TwentyMinor);
			AssertRoundingAmountAndErrorMessage(-0.23M, 1081.23M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.FiftyMinor);
			AssertRoundingAmountAndErrorMessage(-0.23M, 1081.23M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.OneMajor);

			AssertRoundingAmountAndErrorMessage(-0.23M, 1081.23M, InvoiceRoundingOption.RBM, InvoiceRoundCurrencyUnit.OneMajor);

			#endregion

			#region Amount = 1081.67M;

			AssertRoundingAmountAndErrorMessage(0.03M, 1081.67M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.FiveMinor);
			AssertRoundingAmountAndErrorMessage(0.03M, 1081.67M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TenMinor);
			AssertRoundingAmountAndErrorMessage(0.13M, 1081.67M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TwentyMinor);
			AssertRoundingAmountAndErrorMessage(0.33M, 1081.67M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.FiftyMinor);
			AssertRoundingAmountAndErrorMessage(0.33M, 1081.67M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.OneMajor);

			AssertRoundingAmountAndErrorMessage(-0.02M, 1081.67M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.FiveMinor);
			AssertRoundingAmountAndErrorMessage(-0.07M, 1081.67M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.TenMinor);
			AssertRoundingAmountAndErrorMessage(-0.07M, 1081.67M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.TwentyMinor);
			AssertRoundingAmountAndErrorMessage(-0.17M, 1081.67M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.FiftyMinor);
			AssertRoundingAmountAndErrorMessage(-0.67M, 1081.67M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.OneMajor);

			AssertRoundingAmountAndErrorMessage(0.33M, 1081.67M, InvoiceRoundingOption.RBM, InvoiceRoundCurrencyUnit.OneMajor);

			#endregion

			#region Amount = 1081.80M;

			AssertRoundingAmountAndErrorMessage(0M, 1081.80M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.FiveMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.80M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TenMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.80M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TwentyMinor);
			AssertRoundingAmountAndErrorMessage(0.20M, 1081.80M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.FiftyMinor);
			AssertRoundingAmountAndErrorMessage(0.20M, 1081.80M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.OneMajor);

			AssertRoundingAmountAndErrorMessage(0M, 1081.80M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.FiveMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.80M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.TenMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.80M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.TwentyMinor);
			AssertRoundingAmountAndErrorMessage(-0.30M, 1081.80M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.FiftyMinor);
			AssertRoundingAmountAndErrorMessage(-0.80M, 1081.80M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.OneMajor);

			AssertRoundingAmountAndErrorMessage(0.20M, 1081.80M, InvoiceRoundingOption.RBM, InvoiceRoundCurrencyUnit.OneMajor);

			#endregion

			#region Amount = 1081.00M;

			AssertRoundingAmountAndErrorMessage(0M, 1081.00M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.FiveMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.00M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TenMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.00M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TwentyMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.00M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.FiftyMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.00M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.OneMajor);

			AssertRoundingAmountAndErrorMessage(0M, 1081.00M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.FiveMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.00M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.TenMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.00M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.TwentyMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.00M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.FiftyMinor);
			AssertRoundingAmountAndErrorMessage(0M, 1081.00M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.OneMajor);

			AssertRoundingAmountAndErrorMessage(0M, 1081.00M, InvoiceRoundingOption.RBM, InvoiceRoundCurrencyUnit.OneMajor);

			#endregion

			#region Amount = 1081.234M;

			var expectedError = "amount 1081.234 has incorrect decimal places with currency AUD.";
			AssertRoundingAmountAndErrorMessage(0.02M, 1081.234M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.FiveMinor, expectedError);
			AssertRoundingAmountAndErrorMessage(0.07M, 1081.234M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TenMinor, expectedError);
			AssertRoundingAmountAndErrorMessage(0.17M, 1081.234M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TwentyMinor, expectedError);
			AssertRoundingAmountAndErrorMessage(0.27M, 1081.234M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.FiftyMinor, expectedError);
			AssertRoundingAmountAndErrorMessage(0.77M, 1081.234M, InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.OneMajor, expectedError);

			AssertRoundingAmountAndErrorMessage(-0.03M, 1081.234M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.FiveMinor, expectedError);
			AssertRoundingAmountAndErrorMessage(-0.03M, 1081.234M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.TenMinor, expectedError);
			AssertRoundingAmountAndErrorMessage(-0.03M, 1081.234M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.TwentyMinor, expectedError);
			AssertRoundingAmountAndErrorMessage(-0.23M, 1081.234M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.FiftyMinor, expectedError);
			AssertRoundingAmountAndErrorMessage(-0.23M, 1081.234M, InvoiceRoundingOption.ARD, InvoiceRoundCurrencyUnit.OneMajor, expectedError);

			AssertRoundingAmountAndErrorMessage(-0.23M, 1081.234M, InvoiceRoundingOption.RBM, InvoiceRoundCurrencyUnit.OneMajor, expectedError);

			#endregion
		}

		void AssertRoundingAmountAndErrorMessage(ZDecimal roundingAmount, ZDecimal amount, InvoiceRoundingOption roundingOption, InvoiceRoundCurrencyUnit roundCurrencyUnit, string expectedError = null)
		{
			var currency = creator.AUD.Code;
			string error = null;
			Action<string> reportError = (string errorMessage) => { error = errorMessage; };

			AssertEquals(roundingAmount, rounder.ApplyCustomRounding(amount, currency, roundingOption, roundCurrencyUnit, reportError));
			AssertEquals("error message: ", expectedError, error);
		}

		#region Implementation

		IInvoiceAmountRounder rounder;
		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();

			creator = new TestObjectCreator(Factory);
			rounder = new InvoiceAmountRounder();
		}

		#endregion
	}
}
