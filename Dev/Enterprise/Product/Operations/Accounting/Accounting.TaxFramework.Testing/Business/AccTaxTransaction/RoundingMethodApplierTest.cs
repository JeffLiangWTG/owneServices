using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	public class RoundingMethodApplierTest : TestCaseWithFactory
	{
		public void TestApplyRoundingMethodArguments()
		{
			IRoundingMethodApplier roundingMethodApplier = new RoundingMethodApplier();
			ZString roundingMethod;
			ZDecimal amount = 150.39m;
			var isAmountInForeignCurrency = (ZBool)false;
			var amountCurrencyDecimalPlaces = (ZInt)(2);

			roundingMethod = ZString.Empty;
			AssertExceptionThrown<ArgumentException>("Invalid roundingMethod", "Invalid argument, roundingMethod argument is empty.",
							() => roundingMethodApplier.ApplyRounding(roundingMethod, amount, isAmountInForeignCurrency, amountCurrencyDecimalPlaces));

			roundingMethod = "BLA";
			AssertExceptionThrown<ArgumentException>("Invalid roundingMethod", "Invalid argument, \"BLA\" is not a valid rounding method",
				() => roundingMethodApplier.ApplyRounding(roundingMethod, amount, isAmountInForeignCurrency, amountCurrencyDecimalPlaces));

			roundingMethod = TaxAmountRoundingMethods.Standard.Code;
			amount = roundingMethodApplier.ApplyRounding(roundingMethod, amount, isAmountInForeignCurrency, amountCurrencyDecimalPlaces);
			AssertEquals("Amount already has correct number of decimal places.", 150.39m, amount);

			amountCurrencyDecimalPlaces = -1;
			AssertExceptionThrown<ArgumentException>("Invalid amountCurrencyDecimalPlaces", "Invalid argument, amountCurrencyDecimalPlaces must be >= 0",
				() => roundingMethodApplier.ApplyRounding(roundingMethod, amount, isAmountInForeignCurrency, amountCurrencyDecimalPlaces));
			amountCurrencyDecimalPlaces = 3;

			amount = (ZDecimal)int.MaxValue + 1;
			amount = roundingMethodApplier.ApplyRounding(roundingMethod, amount, isAmountInForeignCurrency, amountCurrencyDecimalPlaces);
			AssertEquals((ZDecimal)int.MaxValue + 1, amount);
		}

		public void TestApplyRoundingMethodStandardMethod()
		{
			IRoundingMethodApplier roundingMethodApplier = new RoundingMethodApplier();

			foreach (var isAmountInForeignCurrency in new ZBool[] { true, false })
			{
				ZInt[] signs = new ZInt[] { 1, -1 };

				foreach (var sign in signs)
				{
					ZDecimal taxAmount = 356.9184m * sign;
					ZString roundingMethod = TaxAmountRoundingMethods.Standard.Code;

					var amountCurrencyDecimalPlaces = (ZInt)3;
					taxAmount = roundingMethodApplier.ApplyRounding(roundingMethod, taxAmount, isAmountInForeignCurrency, amountCurrencyDecimalPlaces);
					AssertEquals(356.918m * sign, taxAmount);

					amountCurrencyDecimalPlaces = 2;
					taxAmount = roundingMethodApplier.ApplyRounding(roundingMethod, taxAmount, isAmountInForeignCurrency, amountCurrencyDecimalPlaces);
					AssertEquals(356.92m * sign, taxAmount);

					amountCurrencyDecimalPlaces = 0;
					taxAmount = roundingMethodApplier.ApplyRounding(roundingMethod, taxAmount, isAmountInForeignCurrency, amountCurrencyDecimalPlaces);
					AssertEquals(357m * sign, taxAmount);
				}
			}
		}

		public void TestApplyRoundingMethodRoundDownMethod()
		{
			IRoundingMethodApplier roundingMethodApplier = new RoundingMethodApplier();

			foreach (var isAmountInForeignCurrency in new ZBool[] { true, false })
			{
				ZInt[] signs = new ZInt[] { 1, -1 };

				foreach (var sign in signs)
				{
					ZDecimal taxAmount = 356.919 * sign;
					var roundingMethod = TaxAmountRoundingMethods.RoundDownToMinorUnit.Code;

					var amountCurrencyDecimalPlaces = (ZInt)2;
					taxAmount = roundingMethodApplier.ApplyRounding(roundingMethod, taxAmount, isAmountInForeignCurrency, amountCurrencyDecimalPlaces);
					if (isAmountInForeignCurrency)
					{
						AssertEquals(356.92m * sign, taxAmount);
					}
					else
					{
						AssertEquals(356.91m * sign, taxAmount);
					}

					amountCurrencyDecimalPlaces = 0;
					taxAmount = roundingMethodApplier.ApplyRounding(roundingMethod, taxAmount, isAmountInForeignCurrency, amountCurrencyDecimalPlaces);
					if (isAmountInForeignCurrency)
					{
						AssertEquals(357m * sign, taxAmount);
					}
					else
					{
						AssertEquals(356m * sign, taxAmount);
					}
				}
			}
		}
	}
}
