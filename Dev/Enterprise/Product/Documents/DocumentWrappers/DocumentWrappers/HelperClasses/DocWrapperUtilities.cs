using System;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	static class DocWrapperUtilities
	{
		internal static bool MultiLineStringAlreadyContainThisFullLine(ZString resultSoFar, ZString stringToCheckFor)
		{
			bool result = true;

			foreach (string input in stringToCheckFor.Split('\n'))
			{
				if (!string.IsNullOrEmpty(input))
				{
					if (!ContainsLine(resultSoFar, input))
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		static bool ContainsLine(string resultSoFar, string stringToCheckFor)
		{
			string newLineNotCarriageReturnNewLine = ('\n').ToString();

			foreach (string line in Regex.Split(resultSoFar, newLineNotCarriageReturnNewLine, RegexOptions.IgnoreCase))
			{
				if (!string.IsNullOrWhiteSpace(line) && line.Trim() == stringToCheckFor.Trim())
				{
					return true;
				}
			}
			return false;
		}

		#region Multilingual

		internal static string GetMultilingualOrDefaultDescription(string description, AccChargeCode chargeCode)
		{
			return GetMultilingualDescription(description, chargeCode)
				?? description;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Manage the multilingual manually")]
		internal static string GetMultilingualDescription(string description, AccChargeCode chargeCode)
		{
			string multilingualDescription = null;

			if (!AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.Value
				&& chargeCode != null
				&& !string.IsNullOrEmpty(description))
			{
				var chargeCodeDescription = string.Empty;
				if (description.StartsWith(chargeCode.AC_Desc, StringComparison.OrdinalIgnoreCase))
				{
					chargeCodeDescription = chargeCode.AC_Desc;
				}
				else if (description.StartsWith(chargeCode.AC_LocalLanguageDescription, StringComparison.OrdinalIgnoreCase))
				{
					chargeCodeDescription = chargeCode.AC_LocalLanguageDescription;
				}

				if (!string.IsNullOrEmpty(chargeCodeDescription))
				{
					multilingualDescription = chargeCode.AC_DescMultilingual;
					if (description != chargeCodeDescription)
					{
						multilingualDescription += description.Substring(chargeCodeDescription.Length);
					}
				}
			}

			return multilingualDescription;
		}

		#endregion

		public static string ToStringRounded(this ZDecimal value, int numberOfDecimalPlaces) => value.Round(numberOfDecimalPlaces).ToString(numberOfDecimalPlaces, true);
	}
}
