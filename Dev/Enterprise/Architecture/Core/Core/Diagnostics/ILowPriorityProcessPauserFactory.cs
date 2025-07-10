using CargoWise.Data.SqlServer;

namespace Enterprise.ZArchitecture.Core
{
	public interface ILowPriorityProcessPauserFactory
	{
		ILowPriorityProcessPauser Create();
		ILowPriorityProcessPauser Create(IBacklogInfoProvider[] providers);
	}
}
