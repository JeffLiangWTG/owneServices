using System.Collections.Generic;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IRegexProviderWrapper
	{
		IEnumerable<string> GetOuterMostMacros(string text);
	}
}