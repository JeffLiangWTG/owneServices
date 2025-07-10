using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class CapabilityScope : ICapabilityScope
	{
		public CapabilityScope(ZGuid pk, ZString scope)
		{
			PK = pk;
			Scope = scope;
		}

		public ZGuid PK { get; }
		public ZString Scope { get; }
	}
}
