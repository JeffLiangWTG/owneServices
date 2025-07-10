using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMBufferTimespanCollection : IBusinessObjectCollection
	{
		new IBMBufferTimespan this[int index] { get; }
	}
}
