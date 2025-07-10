using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMComponentResourceLinkCollection : IBusinessObjectCollection
	{
		new IBMComponentResourceLink this[int index] { get; }
	}
}
