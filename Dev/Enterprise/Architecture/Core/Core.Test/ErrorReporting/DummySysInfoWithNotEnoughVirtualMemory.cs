namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DummySysInfoWithNotEnoughVirtualMemory : ZSystemInformation
	{
		protected override void FillMemoryStatusStruct(ref MemoryStatusStructEx memoryStatus)
		{
			memoryStatus.ullAvailVirtual = (ulong)(ZSystemInformation.MinimumRequirements.AvailableMemoryInMB - 100L) * 1024L * 1024L;
			memoryStatus.ullTotalPhys = (ulong)ZSystemInformation.MinimumRequirements.TotalPhysicalMemoryInMB * 2L * 1024L * 1024L;
			memoryStatus.ullAvailPhys = (ulong)ZSystemInformation.MinimumRequirements.TotalPhysicalMemoryInMB * 2L * 1024L * 1024L;
		}
	}
}
