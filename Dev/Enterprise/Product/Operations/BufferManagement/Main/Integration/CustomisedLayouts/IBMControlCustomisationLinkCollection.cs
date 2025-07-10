using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMControlCustomisationLinkCollection : IBusinessObjectCollection
	{
		void DeleteAll();
		new IBMControlCustomisationLink this[int index] { get; }
	}
}
