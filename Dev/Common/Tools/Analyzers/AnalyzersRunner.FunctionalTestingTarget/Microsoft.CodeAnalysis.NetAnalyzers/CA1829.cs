using System.Collections.Generic;
using System.Linq;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	/// <summary>
	/// Rule: Use Length/Count property instead of Enumerable.Count method
	/// </summary>
	class CA1829
	{
		public int GetCount(int[] array)
			=> array.Count();  // CA1829: Use Length/Count property instead of Enumerable.Count method

		public int GetCount(ICollection<int> collection)
			=> collection.Count();  // CA1829: Use Length/Count property instead of Enumerable.Count method
	}
}
