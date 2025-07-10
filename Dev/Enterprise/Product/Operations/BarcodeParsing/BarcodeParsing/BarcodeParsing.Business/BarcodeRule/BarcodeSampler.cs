using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine;

namespace Enterprise.BarcodeParsing.Business
{
	public static class BarcodeSampler
	{
		#region GetSampleBarcode

		public static ZString GetSampleBarcode(BarcodeRule rule)
		{
			var result = new StringBuilder();
			if (rule.Components.Any())
			{
				var componentWithMultiComponentDelim = rule.Components.Cast<IBarcodeRuleComponent>().FirstOrDefault(i => i.IsDelimiterMultiComponent);
				var multiComponentDelim = componentWithMultiComponentDelim?.Delimiter.ToString() ?? "";

				var ruleSampleBuilder = new StringBuilder();
				foreach (var component in rule.Components.OrderBy(c => c.BRC_Sequence))
				{
					var sample = GetSampleBarcodeForComponent(component);
					if (component.IsDelimiterMultiComponent || ruleSampleBuilder.Length > 0)
					{
						ruleSampleBuilder.Append(sample);
					}
					else
					{
						int nextSequence = component.BRC_Sequence + 1;
						var nextComponentIsMultiDelimComponent = rule.Components.FirstOrDefault(c => c.BRC_Sequence == nextSequence && c.IsDelimiterMultiComponent) != null;
						if (nextComponentIsMultiDelimComponent)
						{
							result.Append(TrimTerminatorIfNeeded(rule, sample));
							result.Append(multiComponentDelim);
						}
						else
						{
							result.Append(sample);
						}
					}
				}
				var ruleSampleAsString = ruleSampleBuilder.ToString();
				result.Append(TrimTerminatorIfNeeded(rule, ruleSampleAsString));
				result.Append(multiComponentDelim);
				result.Append(ruleSampleAsString);
			}

			var resultAsString = result.ToString().Replace(BarcodeRule.GS1Terminator, BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator);
			return rule.IsPartialRule ? string.Format(CultureInfo.InvariantCulture, "...{0}...", resultAsString) : TrimTerminatorIfNeeded(rule, resultAsString);
		}

		#region TrimTerminatorIfNeeded

		static string TrimTerminatorIfNeeded(IBarcodeRule rule, ZString sample)
		{
			var terminatorStartPos = sample.Length - rule.Terminator.Length;
			return sample.EndsWith(rule.Terminator, StringComparison.Ordinal) ? sample.Left(terminatorStartPos) : sample;
		}

		#endregion

		#region GetSampleBarcodeForComponent

		static string GetSampleBarcodeForComponent(BarcodeRuleComponent component)
		{
			var result = "";

			if (!component.BRC_Format.IsEmpty && !component.IsDateFormat)
			{
				result = GetSampleForAlphaNum(component);
			}
			else
			{
				result = GetSampleForDate(component);
			}

			if (!string.IsNullOrEmpty(result))
			{
				if (!component.IsFixedLength())
				{
					result += ((IBarcodeRule)component.Rule).Terminator;
				}

				if (!component.BRC_Delimiter.IsEmpty && !component.IsDelimiterMultiComponent)
				{
					result = TrimTerminatorIfNeeded(component.Rule, result) + (char)(int)component.BRC_Delimiter + result;
				}

				result = component.BRC_ApplicationID + result;
			}

			return result;
		}

		#endregion

		#region GetSampleForAlphaNum

		static string GetSampleForAlphaNum(BarcodeRuleComponent component)
		{
			const int idealSize = 5;

			// we are trying to make sample length close to 5
			int sampleSize;
			if (component.BRC_MaxLength < idealSize && component.BRC_MaxLength > 0)
			{
				sampleSize = component.BRC_MaxLength;
			}
			else if (component.BRC_MinLength > idealSize)
			{
				sampleSize = component.BRC_MinLength;
			}
			else
			{
				sampleSize = idealSize;
			}

			var allowedChars = GetAllowedCharsForComponentSample(component);
			var sample = BuildSampleFromAllowedChars(sampleSize, allowedChars);

			switch (component.BRC_Format)
			{
				case GS1DataFormatTypes.Codes.Numeric:
					SpecialFormattingForNumeric(component, sample);
					break;
				case GS1DataFormatTypes.Codes.AlphaNumericWithSymbols:
					SpecialFormattingForNumericWithSymbols(component, sample);
					break;
			}

			return sample.ToString();
		}

		static char[] GetAllowedCharsForComponentSample(BarcodeRuleComponent component)
		{
			char[] allowedChars;

			if (component.IsIgnored())
			{
				if (component.BRC_Format == GS1DataFormatTypes.Codes.Numeric || component.BRC_Format == GS1DataFormatTypes.Codes.Digit)
				{
					allowedChars = new[] { GetValidChar('0', component) };
				}
				else
				{
					allowedChars = new[] { GetValidChar('x', component) };
				}
			}
			else
			{
				allowedChars = GetNonDateFormatAllowedChars(component);
			}

			return allowedChars;
		}

		static char[] GetNonDateFormatAllowedChars(BarcodeRuleComponent component)
		{
			switch (component.BRC_Format)
			{
				case GS1DataFormatTypes.Codes.Alpha:
					return new[] { GetValidChar('A', component) };
				case GS1DataFormatTypes.Codes.AlphaWithSpace:
					return IsValidChar(' ', component)
						? new[] { GetValidChar('A', component), ' ' }
						: new[] { GetValidChar('A', component) };
				case GS1DataFormatTypes.Codes.AlphaNumeric:
					return new[] { GetValidChar('A', component), GetValidChar('6', component) };
				case GS1DataFormatTypes.Codes.AlphaNumericWithSpace:
					return IsValidChar(' ', component)
						? new[] { GetValidChar('A', component), GetValidChar('6', component), ' ' }
						: new[] { GetValidChar('A', component), GetValidChar('6', component) };
				case GS1DataFormatTypes.Codes.AlphaNumericWithSymbols:
					return new[] { GetValidChar('A', component), GetValidChar('6', component), GetValidChar('*', component) };
				case GS1DataFormatTypes.Codes.Numeric:
				case GS1DataFormatTypes.Codes.Digit:
					return new[] { GetValidChar('7', component) };
				default:
					return null;
			}
		}

		static void SpecialFormattingForNumericWithSymbols(BarcodeRuleComponent component, StringBuilder sample)
		{
			if (sample.Length < 3 && sample.Length > 0)
			{
				sample[sample.Length - 1] = GetValidChar('*', component);
			}
		}

		static void SpecialFormattingForNumeric(BarcodeRuleComponent component, StringBuilder sample)
		{
			if (sample.Length >= 3 && IsValidChar(',', component))
			{
				sample[1] = ',';
			}
			if (sample.Length >= 7 && IsValidChar('.', component))
			{
				sample[5] = '.';
			}
		}

		static bool IsValidChar(char characterToCheck, BarcodeRuleComponent component) => GetValidChar(characterToCheck, component) == characterToCheck;

		/// <summary>
		/// This method ensures that sampleChar is not a delimeter, not terminator and not AppID in partial rule
		/// </summary>
		static char GetValidChar(char sampleChar, BarcodeRuleComponent component)
		{
			var delimiter = GetDelimiterForComponent(component);
			var result = delimiter != sampleChar ? sampleChar : (char)(sampleChar + 1);
			ZString terminator = ((IBarcodeRule)component.Rule).Terminator;
			if (terminator.Length == 1 && terminator[0] == result)
			{
				result = (char)(result + 1);

				if (result == delimiter)
				{
					result = GetValidChar(result, component);
				}
			}

			if (component.Rule.IsPartialRule && result.ToString() == component.BRC_ApplicationID)
			{
				result = (char)(result + 1);
				result = GetValidChar(result, component);
			}

			return result;
		}

		static char GetDelimiterForComponent(BarcodeRuleComponent component)
		{
			var barcodeRule = component.Rule;
			return barcodeRule.BRU_IsDelimiterMultiComponent && barcodeRule.Components.Any(c => c.HasDelimiter())
				? barcodeRule.Components.Cast<IBarcodeRuleComponent>().First(c => c.HasDelimiter()).Delimiter
				: ((IBarcodeRuleComponent)component).Delimiter;
		}

		static StringBuilder BuildSampleFromAllowedChars(int length, char[] allowedChars)
		{
			var result = new StringBuilder();
			if (allowedChars != null && allowedChars.Length > 0)
			{
				var queue = new Queue<char>(allowedChars);
				for (var i = 0; i < length; i++)
				{
					var nextChar = queue.Dequeue();
					if (queue.Count == 0)
					{
						queue = new Queue<char>(allowedChars);
					}

					bool isLastElement = i == length - 1;
					if (nextChar == ' ' && isLastElement)
					{
						nextChar = queue.Dequeue();
					}

					result.Append(nextChar);
				}
			}
			return result;
		}

		#endregion

		#region GetSampleForDate

		static string GetSampleForDate(BarcodeRuleComponent component)
		{
			var result = "";
			var dateToUse = component.IsIgnored() ? new ZDate(2000, 1, 1) : ZDate.Today;
			switch (component.BRC_Format)
			{
				case OtherDataFormatTypes.Codes.DDMMYY:
					result = dateToUse.ToString("ddMMyy", CultureInfo.InvariantCulture);
					break;
				case OtherDataFormatTypes.Codes.DDMMYYYY:
					result = dateToUse.ToString("ddMMyyyy", CultureInfo.InvariantCulture);
					break;
				case OtherDataFormatTypes.Codes.MMDDYY:
					result = dateToUse.ToString("MMddyy", CultureInfo.InvariantCulture);
					break;
				case OtherDataFormatTypes.Codes.MMDDYYYY:
					result = dateToUse.ToString("MMddyyyy", CultureInfo.InvariantCulture);
					break;
				case GS1DataFormatTypes.Codes.YYMMDD:
					result = dateToUse.ToString("yyMMdd", CultureInfo.InvariantCulture);
					break;
			}

			return result;
		}

		#endregion

		#endregion
	}
}
