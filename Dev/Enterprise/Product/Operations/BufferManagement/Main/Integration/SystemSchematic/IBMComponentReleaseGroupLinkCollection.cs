using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMComponentReleaseGroupLinkCollection : IBusinessObjectCollection
	{
		new IBMComponentReleaseGroupLink this[int index] { get; }
	}
}
