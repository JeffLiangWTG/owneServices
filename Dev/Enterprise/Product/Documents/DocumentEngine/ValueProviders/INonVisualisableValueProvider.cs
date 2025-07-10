using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine
{
	internal interface INonVisualisableValueProvider
	{
		Regex RegexToReplaceMacro { get; }
	}
}