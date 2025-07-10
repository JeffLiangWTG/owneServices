using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IOAuth2Scope
	{
		ZString ScopeName { get; set; }
		ZPropertyInfo ScopeNameInfo { get; }
	}
}
