namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DummySysInfoWithLessThanMinimumRequirements : ZSystemInformation
	{
		protected override void FillMemoryStatusStruct(ref MemoryStatusStructEx memoryStatus)
		{
			memoryStatus.ullTotalPhys = ((ulong)ZSystemInformation.MinimumRequirements.TotalPhysicalMemoryInMB - 100L) * 1024L * 1024L;
		}
	}
}
