namespace Enterprise.ZArchitecture.Core.Testing
{
	public class DummySysInfoWithLowPageFile : ZSystemInformation
	{
		protected override void FillMemoryStatusStruct(ref MemoryStatusStructEx memoryStatus)
		{
			memoryStatus.ullAvailPageFile = 3 * 1024 * 1024;
			memoryStatus.ullTotalPageFile = 5 * 1024 * 1024;
			memoryStatus.ullAvailVirtual = ulong.MaxValue;
			memoryStatus.ullTotalVirtual = ulong.MaxValue;
			memoryStatus.ullTotalPhys = ulong.MaxValue;
			memoryStatus.ullAvailPhys = ulong.MaxValue;
		}
	}
}
