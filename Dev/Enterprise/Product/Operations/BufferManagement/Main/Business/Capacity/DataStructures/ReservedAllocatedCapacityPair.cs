using System.Collections.Generic;

namespace Enterprise.BufferManagement.Business
{
	internal struct ReservedAllocatedCapacityPair
	{
		internal ReservedAllocatedCapacityPair(Dictionary<int, decimal> allocatedCapacity, Dictionary<int, decimal> reservedCapacity)
		{
			AllocatedCapacity = allocatedCapacity;
			ReservedCapcity = reservedCapacity;
		}

		internal Dictionary<int, decimal> AllocatedCapacity { get; }
		internal Dictionary<int, decimal> ReservedCapcity { get; }
	}
}
