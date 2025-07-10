using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Integration;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CodePairValue : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CodePairValue({provider}, {field}, {index}[ ,TreatIndexAsString])>",
				ResString.GetMultilingualString("46cab816-71e4-40bf-ade6-dccd1782c4f3",
				@"Returns the field value(s) (code or description) from specified value pair provider's element with specified index. When {0} equals '{1}', returns comma-separated list of all values. {0} is 1-based or string Code. The extra parameter is used if {0} is always a string Code.",
"{index}", "All"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<CodePairValue(StorageClass, Code, 1)>", "20F"),
					((NoResString)"<CodePairValue(StorageClass, Description, 20F)>", (NoResString)"Twenty Foot Equivalent Unit"),
					((NoResString)"<CodePairValue(StorageClass, Description, 20F, TreatIndexAsString)>", (NoResString)"Twenty Foot Equivalent Unit"),
					((NoResString)"<CodePairValue(StorageClass, Description, \"<Storage Amount>\")>", (NoResString)"Twenty Foot Equivalent Unit") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var value = String.Empty;
			var regexMatch = Regex.Match(macro);

			var provider = CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider(regexMatch.Groups["CodeValuePairProviderName"].Value);
			if (provider == null)
			{
				ReportMacroError(report, Res.GetString("1fe4d171-23fe-42e8-a207-47ad07f993df", "Invalid provider name - {0}", regexMatch.Groups["CodeValuePairProviderName"].Value));
				return value;
			}

			try
			{
				var codeDescriptionPairList = provider.GetCodeDescriptionPairList();
				if (regexMatch.Groups["ElementNumber"].Value.Equals("ALL", StringComparison.OrdinalIgnoreCase))
				{
					foreach (ICodeDescription codeDescriptionPair in codeDescriptionPairList)
					{
						value = (String.IsNullOrEmpty(value) ? "" : value + ", ") + GetPropertyValueByPropertyName(codeDescriptionPair, regexMatch.Groups["FieldName"].Value);
					}
				}
				else
				{
					int position;
					var elementNumber = regexMatch.Groups["ElementNumber"].Value;
					var doubleQuoteMatch = regexForDoubleQuotes.Match(elementNumber);
					if (doubleQuoteMatch.Success)
					{
						elementNumber = doubleQuoteMatch.Groups["RealValue"].Value;
					}

					if (regexMatch.Groups["TreatIndexAsString"].Value.Equals("TREATINDEXASSTRING", StringComparison.OrdinalIgnoreCase) ||
							!Int32.TryParse(elementNumber, out position))
					{
						position = codeDescriptionPairList.IndexOfCode(elementNumber);
					}
					else
					{
						position--;
					}

					if (position >= codeDescriptionPairList.Count || position == -1)
					{
						value = String.Empty;
					}
					else
					{
						value = GetPropertyValueByPropertyName(codeDescriptionPairList[position], regexMatch.Groups["FieldName"].Value);
					}
				}
			}
			catch (ArgumentException ex)
			{
				ReportMacroError(report, ex.Message);
			}

			return value;
		}

		static readonly Regex regexForDoubleQuotes = new Regex(@"""(?<RealValue>.*)""", RegexOptions.Compiled);
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)CodePairValue(?:[\s]*)\((?:[\s]*)(?<CodeValuePairProviderName>[^,]+(?!\s))(?:[\s]*)\,(?:[\s]*)(?<FieldName>[^,]+(?!\s))(?:[\s]*)\,(?:[\s]*)(?<ElementNumber>[^,]*|"".*""(?!\s))(?:[\s]*)(?:(?:,)(?:\s*)(?<TreatIndexAsString>TreatIndexAsString)(?:\s*))?\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		public override Regex Regex
		{
			get { return regex; }
		}

		protected string GetPropertyValueByPropertyName(ICodeDescription codeDescriptionPair, string propertyName)
		{
			switch (propertyName.ToUpperInvariant())
			{
				case "CODE":
					return codeDescriptionPair.Code;
				case "DESCRIPTION":
					return codeDescriptionPair.Description;
				default:
					throw new ArgumentException(Res.GetString("a206a829-9ada-4662-8ee1-fa6b137c6599", "Invalid property name - {0}", propertyName));
			}
		}
	}
}
