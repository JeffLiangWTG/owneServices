using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessHeaderLinkCollection : IBusinessObjectCollection
	{
		new IProcessHeaderLink this[int index] { get; }
	}
}
