using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ICYDUnitLineItem
	{
		ZGuid PK { get; }

		ZGuid YLI_RC_ContainerType { get; }
	}
}
