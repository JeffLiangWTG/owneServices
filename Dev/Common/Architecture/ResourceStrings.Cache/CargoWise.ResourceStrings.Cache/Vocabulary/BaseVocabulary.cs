using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.ResourceStrings.Vocabulary
{
	[Immutable]
	abstract class BaseVocabulary
	{
		protected BaseVocabulary(ImmutableList<VocabularyRule> rules)
		{
			Argument.NotNull(rules, nameof(rules));
			this.rules = rules;
		}

		readonly ImmutableList<VocabularyRule> rules;

		protected static VocabularyRule CreatePluralRule(string rule, string replacement)
		{
			Argument.NotNull(rule, nameof(rule));
			Argument.NotNull(replacement, nameof(replacement));
			return new VocabularyRule(rule, replacement);
		}

		protected static VocabularyRule CreateIrregularRule(string singularNoun, string plural, bool matchWhole = false)
		{
			Argument.NotNullOrEmpty(singularNoun, nameof(singularNoun));
			Argument.NotNullOrEmpty(plural, nameof(plural));
			if (singularNoun.Length <= 1)
			{
				throw new ArgumentException("Invalid argument.", nameof(singularNoun));
			}

			if (plural.Length <= 1)
			{
				throw new ArgumentException("Invalid argument.", nameof(plural));
			}

			var startPosition = matchWhole ? "^" : null;

			var rule = FormattableString.Invariant($"{startPosition}({singularNoun[0]}){singularNoun.Substring(1)}$"); // keep the case of first character of singularNoun
			var replacement = FormattableString.Invariant($"$1{plural.Substring(1)}");
			return CreatePluralRule(rule, replacement);
		}

		public string ApplyRules(string singularNoun)
		{
			Argument.NotNull(singularNoun, nameof(singularNoun));
			if (rules.Count == 0)
			{
				return singularNoun;
			}

			var result = singularNoun;
			var index = rules.Count - 1;
			while (index >= 0 && (result = rules[index]?.Apply(singularNoun)) == null)
			{
				--index;
			}

			var uppercaseRegex = new Regex("^[A-Z]+$", RegexOptions.Compiled);
			return uppercaseRegex.IsMatch(Regex.Escape(singularNoun)) ? result?.ToUpper(CultureInfo.CurrentCulture) : result;
		}
	}

	[Immutable]
	class VocabularyRule
	{
		readonly Regex regex;
		readonly string replacement;

		public VocabularyRule(string pattern, string replacement)
		{
			Argument.NotNull(pattern, nameof(pattern));
			Argument.NotNull(replacement, nameof(replacement));
			regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			this.replacement = replacement;
		}

		public string Apply(string singularNoun)
		{
			Argument.NotNull(singularNoun, nameof(singularNoun));
			return !regex.IsMatch(singularNoun) ? null : regex.Replace(singularNoun, replacement);
		}
	}
}
