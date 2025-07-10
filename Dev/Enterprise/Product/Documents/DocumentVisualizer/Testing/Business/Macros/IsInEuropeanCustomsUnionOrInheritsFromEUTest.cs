using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	class IsInEuropeanCustomsUnionOrInheritsFromEUTest : TestCaseWithFactory
	{
		public void TestIsInEuropeanCustomsUnionOrInheritsFromEU()
		{
			AssertCountryCode(null, false);
			AssertCountryCode(string.Empty, false);
			AssertCountryCode("F", false);
			AssertCountryCode("CA", false);
			AssertCountryCode("NA", false);

			AssertCountryCode("GB", true);
			AssertCountryCode("TR", true);
			AssertCountryCode("IT", true);
			AssertCountryCode("DE", true);

			foreach (var country in Enterprise.Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				AssertCountryCode(country, true);
			}
		}

		void AssertCountryCode(string countryCode, bool expected)
		{
			var res = Expression.Evaluate(countryCode);

			AssertEquals("expected no errors", string.Empty, Expression.ToFormatString());
			AssertEquals($"Expected value for: {countryCode}", res, expected);
		}

		IMacroExpression Expression
		{
			get
			{
				if (expression == null)
				{
					const string macro = "IsInEuropeanCustomsUnionOrInheritsFromEU()";
					expression = macro.With<FilterLibrary>().CreateExpression();
				}

				return expression;
			}
		}

		IMacroExpression expression;
	}
}

