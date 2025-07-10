using Enterprise.Integration.ServiceManager;

namespace Enterprise.Integration.DocumentEngine
{
	/// <summary>
	/// !!! If you add members to these types, create a new file for them. !!!
	/// </summary>
	public interface IStmPrintQueueCollection { }
	public interface IReportScheduleTask : IStmScheduleTask { }
	public interface IArchiveScheduleTask : IStmScheduleTask { }
	public interface IUniversalCopyScheduleTask : IStmScheduleTask { }
}
