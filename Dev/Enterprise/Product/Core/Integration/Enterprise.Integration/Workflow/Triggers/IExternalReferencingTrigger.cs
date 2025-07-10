using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IExternalReferencingTrigger
	{
		ZGuid ReferencedID { get; }
	}
}
