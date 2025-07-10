using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine
{
	internal interface IValueProviderThatShouldBeCopiedIfNewRowAdded
	{
		Regex RegexToFindMacro { get; }
	}
}
