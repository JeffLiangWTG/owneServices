using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine
{
	class RegexProviderWrapper : IRegexProviderWrapper
	{
		public IEnumerable<string> GetOuterMostMacros(string text)
		{
			var matches = RegexProvider.OutermostMacroRegex.Matches(text);
			return matches.Cast<Match>().Select(m => m.Value);
		}
	}
}
