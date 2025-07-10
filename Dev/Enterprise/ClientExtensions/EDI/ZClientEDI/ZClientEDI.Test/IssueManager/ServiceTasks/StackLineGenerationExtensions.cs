using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Client.EDI.ServiceTask.Testing
{
	static class StackLineGenerationExtensions
	{
		internal static IEnumerable<StackLineCount> GetStackLineCount(this IEnumerable<StackLineCount> calls) => calls.GroupBy(call => call).Select(g =>
		{
			var call = g.Key;
			call.Count = g.Count();
			return call;
		});
	}
}
