using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Substitute : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(@"<Substitute(""{Text}"", ""{OldValue}"", ""{NewValue}"")>",
ResString.GetMultilingualString("81a6cd58-d180-4583-a995-2a98a0d04507", @"Replaces all occurrences of a specified value in the text, with another specified value."),
				new List<(string example, object expectedResult)> { ("<Substitute(\"<Z0_VarCharMax>\", \":\", \"\")>", (NoResString)"Hello") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = macro;
			var match = regex.Match(macro);

			if (match.Success)
			{
				var textGroup = match.Groups["Text"];
				var oldValueGroup = match.Groups["OldValue"];
				var newValueGroup = match.Groups["NewValue"];

				if (string.IsNullOrEmpty(oldValueGroup.Value))
				{
					ReportMacroError(report, Res.GetString("ca1e59d9-e54a-4ab0-a6e7-93164100bb84", "The character/string to substitute cannot be empty. The original text without substitution will be returned."));
					result = textGroup.Value;
				}
				else if (textGroup.Success && oldValueGroup.Success && newValueGroup.Success)
				{
					result = textGroup.Value.Replace(oldValueGroup.Value, newValueGroup.Value);
				}
			}

			return result;
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(@"^<[\s]*Substitute[\s]*\([\s]*""(?<Text>.*?)""[\s]*,[\s]*""(?<OldValue>.*?)""[\s]*,[\s]*""(?<NewValue>.*?)""[\s]*\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled);
	}
}
