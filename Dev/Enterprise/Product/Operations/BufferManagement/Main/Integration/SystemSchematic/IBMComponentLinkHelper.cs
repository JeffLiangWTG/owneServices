using System.Collections.Immutable;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMComponentLinkHelper
	{
		ImmutableHashSet<string> GetFiltersAllowedToSkip();
	}
}
