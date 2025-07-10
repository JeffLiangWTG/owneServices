using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	abstract class MathOperationProvider : ValueProvider
	{
		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var valueAsString = match.Groups["operands"].Value;

			var operandRegex = new Regex(@"(,|^)\s*""(?<value>[^""]*)""\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
			var matches = operandRegex.Matches(valueAsString);

			decimal? result = null;
			int matchedCharacters = 0;

			try
			{
				foreach (Match operand in matches)
				{
					var value = operand.Groups["value"].Value;
					matchedCharacters += operand.Length;

					if (decimal.TryParse(value, NumberStyles.Number | NumberStyles.AllowExponent, NumberFormatInfo.CurrentInfo, out var number))
					{
						result = result == null ? number : Operation(result.Value, number) / UnitaryDecimalForPrecisionNormalisation;
					}
					else
					{
						ReportInvalidParams(report, macro);
						return string.Empty;
					}
				}
			}
			catch (OverflowException ex)
			{
				ReportMacroError(report, ex.Message);
				return string.Empty;
			}

			if (matches.Count == 0 || matchedCharacters != valueAsString.Length)
			{
				ReportInvalidParams(report, macro);
				return null;
			}

			return result.ToString();
		}

		protected void ReportInvalidParams(Report report, string macro)
		{
			ReportMacroError(report, Res.GetString("B87AEE9A-4EDE-4F18-8451-BD3A3E583146", "Invalid parameters provided: {0}, expected format: <{1}(\"value1\",\"value2\",...)>.", macro, OperationName));
		}

		protected abstract Func<decimal, decimal, decimal> Operation { get; }
		public abstract string OperationName { get; }
		const decimal UnitaryDecimalForPrecisionNormalisation = 1.000000000000000000000000000000m; //Max decimal precision is 29 digits
	}
}
