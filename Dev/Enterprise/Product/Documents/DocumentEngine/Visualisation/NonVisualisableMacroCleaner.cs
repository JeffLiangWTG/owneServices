using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.Visualisation
{
	internal static class NonVisualisableMacroCleaner
	{
		public static string RemoveMacrosUsedForFormattingOnly(string macros)
		{
			macros = macros.TrimEnd('\n', '\r');
			foreach (var regex in RegexesToRemove)
			{
				macros = regex.Replace(macros, RemoveMacroMatchEvaluator);
			}
			return macros;
		}

		static string RemoveMacroMatchEvaluator(Match match)
		{
			return "";
		}

		public static readonly ImmutableList<Regex> RegexesToRemove = new ValueProviderCollector().ValueProviders.Providers.OfType<INonVisualisableValueProvider>()
			.Select(v => v.RegexToReplaceMacro)
			.GroupBy(regex => regex.Options)
			.Select(
				group =>
				{
					var combinedPattern = string.Join("|", group.Select(regex => $"({regex})"));
					return new Regex(combinedPattern, group.Key);
				}
			).ToImmutableList();
	}
}
