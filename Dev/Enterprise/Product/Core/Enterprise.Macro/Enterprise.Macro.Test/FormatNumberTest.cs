using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Macro.Test
{
	class FormatNumberTest : TestCaseWithFactory
	{
		public void TestFormatNumberWithInvalidDecimalplaces()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Decimal = 12412351m;
			var expr = $"FormatNumber(\"100\")".With(Context).CreateExpression();
			using (var scope = new MacroScope(dummy.Z0_Decimal))
			{
				var result = expr.Evaluate(scope);
				AssertNull(result);
				var notifications = expr.Errors
					.Select(err => err.Message)
					.ToArray();
				Assert(notifications.Contains("Decimal places can not be greater than 99."));
			}
		}

		public void TestFormatNumberWithInvalidDecimalplacesFormat()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Decimal = 12412351m;
			var expr = $"FormatNumber(12.5)".With(Context).CreateExpression();
			using (var scope = new MacroScope(dummy.Z0_Decimal))
			{
				var result = expr.Evaluate(scope);
				AssertNull(result);
				var notifications = expr.Errors
					.Select(err => err.Message)
					.ToArray();
				Assert(notifications.Contains("Invalid Currency Code or Decimal Places format: 12.5"));
			}
		}

		public void TestFormatNumberWithoutDecimalplaces()
		{
			var result = $"FormatNumber(\"12412351.3456789\")".With(Context).CreateExpression().Evaluate();
			AssertNull(result);
		}

		public void TestFormatAmountWithNegativeDecimalPlaces()
		{
			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.Australia)))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				AssertEquals("12,412,351.345679", $"FormatNumber(\"12412351.3456789\", \"-1\")".With(Context).CreateExpression().Evaluate());

				GlbCompany.CurrentCompany.GC_IsReciprocal = false;
				AssertEquals("12,412,351.345679", $"FormatNumber(\"12412351.3456789\", \"-1\")".With(Context).CreateExpression().Evaluate());
			}
		}

		public void TestFormatAmountWithIncorrectParameters()
		{
			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.Australia)))
			{
				var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummy.Z0_Date = new ZDate(2025, 1, 13);
				var expr = $"FormatNumber(Z0_Date, \"2\")".With(Context).CreateExpression();
				using (var scope = new MacroScope(dummy))
				{
					var result = expr.Evaluate(scope);
					AssertNull(result);
					var notifications = expr.Errors
						.Select(err => err.Message)
						.ToArray();
					Assert(notifications.Contains("Invalid value format: 13-Jan-25 00:00:00"));
				}
			}
		}

		public void TestFormatNumberWithVariousAmountTypes()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Decimal = 12412351m;
			dummy.Z0_Long = 12412351;
			dummy.Z0_Number = 12412351;
			dummy.Z0_Short = 12412;
			dummy.Z0_Description = "12412351.3456789";

			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.Australia)))
			{
				using (var scope = new MacroScope(dummy))
				{
					AssertEquals("12,412,351.35", $"FormatNumber(Z0_Description, \"AU\")".With(Context).CreateExpression().Evaluate(scope));
					AssertEquals("12,412,351.00", $"FormatNumber(Z0_Decimal, \"-AU\")".With(Context).CreateExpression().Evaluate(scope));
					AssertEquals("12,412,351.00", $"FormatNumber(Z0_Long, \"AU\")".With(Context).CreateExpression().Evaluate(scope));
					AssertEquals("12,412,351.00", $"FormatNumber(Z0_Number, \"AU\")".With(Context).CreateExpression().Evaluate(scope));
					AssertEquals("12,412.00", $"FormatNumber(Z0_Short, \"AU\")".With(Context).CreateExpression().Evaluate(scope));
				}
				using (var scope = new MacroScope(dummy.Z0_Decimal))
				{
					AssertEquals("12,412,351.00", $"FormatNumber(\"AU\")".With(Context).CreateExpression().Evaluate(scope));
				}
				using (var scope = new MacroScope(dummy.Z0_Description))
				{
					AssertEquals("12,412,351.35", $"FormatNumber(\"AU\")".With(Context).CreateExpression().Evaluate(scope));
				}
				AssertEquals("12,412,351.35", $"FormatNumber(\"12412351.3456789\", \"AU\")".With(Context).CreateExpression().Evaluate());
				AssertEquals("12,412,351.35", $"FormatNumber(12412351.3456789, \"AU\")".With(Context).CreateExpression().Evaluate());
				AssertEquals("12,412,351.00", $"FormatNumber(12412351, \"AU\")".With(Context).CreateExpression().Evaluate());
			}
		}

		public void TestFormatNumberResult()
		{
			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.France)))
			{
				AssertEquals(replaceSpace("12 412 351,33"), $"FormatNumber(\"12412351.33\", \"USD\")".With(Context).CreateExpression().Evaluate());
				AssertEquals("load'o'crap", $"FormatNumber(\"load'o'crap\", \"USD\")".With(Context).CreateExpression().Evaluate());
				AssertEquals(replaceSpace("12 412 351,346"), $"FormatNumber(\"12412351.34567\", \"3\")".With(Context).CreateExpression().Evaluate());
			}

			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.HongKong)))
			{
				AssertEquals("0.00", $"FormatNumber(\"4.540197384717E-11\", \"HKD\")".With(Context).CreateExpression().Evaluate());
			}
			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.Australia)))
			{
				AssertEquals("1,234.56", $"FormatNumber(\"1234.56\", \"PLN\")".With(Context).CreateExpression().Evaluate());
				AssertEquals("1,234.56", $"FormatNumber(\"1234.56\", \"AU\")".With(Context).CreateExpression().Evaluate());
			}

			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.Poland)))
			{
				AssertEquals(replaceSpace("1 234,56"), $"FormatNumber(\"1234.56\", \"PLN\")".With(Context).CreateExpression().Evaluate());
				AssertEquals(replaceSpace("1 234,56"), $"FormatNumber(\"1234.56\", \"AU\")".With(Context).CreateExpression().Evaluate());
			}
		}

		public void TestFormatAmountShouldNotChangeCurrentNumberFormat()
		{
			var currentNumberFormat = Culture.CurrentCompanyCountryCulture.NumberFormat;

			AssertEquals("$", currentNumberFormat.CurrencySymbol);
			AssertEquals(2, currentNumberFormat.CurrencyDecimalDigits);
			var formatResult = $"FormatNumber(12412351.33, 1)".With(Context).CreateExpression().Evaluate();
			AssertEquals("12,412,351.3", formatResult);

			AssertEquals("$", currentNumberFormat.CurrencySymbol);
			AssertEquals(2, currentNumberFormat.CurrencyDecimalDigits);
		}

		string replaceSpace(string expectResult)
		{
			var noCurrencySymbolFormat = (NumberFormatInfo)Culture.CurrentCompanyCountryCulture.NumberFormat.Clone();
			noCurrencySymbolFormat.CurrencySymbol = "";

			var spaceMatch = 1234.56.ToString("C", noCurrencySymbolFormat).TrimEnd().FirstOrDefault(char.IsWhiteSpace);

			if (spaceMatch != default)
			{
				return expectResult.Replace(' ', spaceMatch);
			}
			return expectResult;
		}

		IMacroEvaluationContext Context
		{
			get
			{
				if (context == null)
				{
					context = new IMacroLibrary[]
					{
						new CargoWiseOneStandardLibrary()
					}
					.CreateContext();
				}

				return context;
			}
		}
		IMacroEvaluationContext context;
	}
}
