using System.Collections.Generic;
using System.Linq;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	/// <summary>
	/// Rule: Do not use Count()/LongCount() when Any() can be used
	/// </summary>
	class CA1827
	{
		public string M1(IEnumerable<string> list)
			=> list.Count() != 0 ? "Not empty" : "Empty";  // CA1827: Do not use Count()/LongCount() when Any() can be used

		public string M2(IEnumerable<string> list)
			=> list.LongCount() > 0 ? "Not empty" : "Empty";  // CA1827: Do not use Count()/LongCount() when Any() can be used
	}
}
