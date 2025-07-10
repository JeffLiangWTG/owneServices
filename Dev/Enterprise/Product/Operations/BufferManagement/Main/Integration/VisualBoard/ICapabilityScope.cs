using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface ICapabilityScope
	{
		ZGuid PK { get; }
		ZString Scope { get; }
	}
}
