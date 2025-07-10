using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMBoardCollection : IBusinessObjectCollection
	{
		new IBMBoard this[int index] { get; }
	}
}
