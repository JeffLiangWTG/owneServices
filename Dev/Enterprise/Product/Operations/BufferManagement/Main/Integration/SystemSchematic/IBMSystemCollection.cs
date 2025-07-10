using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMSystemCollection : IBusinessObjectCollection
	{
		new IBMSystem this[int index] { get; }
	}
}
