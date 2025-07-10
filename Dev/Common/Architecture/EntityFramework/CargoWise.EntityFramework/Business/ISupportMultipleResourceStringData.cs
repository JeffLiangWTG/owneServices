using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public interface ISupportMultipleResourceStringData
	{
		IReadOnlyList<string> MultipleKeysToUse { get; }
	}
}
