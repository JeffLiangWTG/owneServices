
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IDocSimpleContainer
	{
		ZString ContainerNumber { get; }
		ZString SealNumber { get; }
		ZInt TotalAllocatedJobPackages { get; }
		ZDecimal TotalAllocatedJobWeight { get; }
		ZDecimal TotalAllocatedJobVolume { get; }
		DocRefContainer Container { get; }
		ZString DeliveryMode { get; }
		ZString Type { get; }
		ZShort ContainerCount { get; }
		ZString ClientRef { get; }
	}
}

