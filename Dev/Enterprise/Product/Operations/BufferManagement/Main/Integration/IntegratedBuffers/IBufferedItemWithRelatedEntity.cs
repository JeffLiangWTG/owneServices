using CargoWise.PAVE.Common.Interfaces;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBufferedItemWithRelatedEntity : IBufferedItem
	{
		string RelatedEntityName { get; }
	}
}
