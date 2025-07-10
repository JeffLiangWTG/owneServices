using System.Collections.Generic;

namespace CargoWise.Common
{
	public interface IMaskMatcher
	{
		string GetMatch(IEnumerable<string> masks, string inputString);
	}
}
