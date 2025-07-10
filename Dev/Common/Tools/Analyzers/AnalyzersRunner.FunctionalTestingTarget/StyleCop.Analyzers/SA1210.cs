//SA1210:Using directives should be ordered alphabetically by the namespaces
using System.Text.RegularExpressions;
using System;

namespace AnalyzersRunner.FunctionalTestingTarget.StyleCop.Analyzers
{
	class SA1210
	{
		public void Method()
		{
			_ = new Regex("");
			_ = DayOfWeek.Monday;
		}
	}
}
