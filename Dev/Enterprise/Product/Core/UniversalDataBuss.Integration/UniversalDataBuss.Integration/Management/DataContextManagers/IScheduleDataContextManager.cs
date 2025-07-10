namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IScheduleDataContextManager : IDataContextManager
	{
		bool ManagesSchedules { get; }
		ITopLevelDataObjectWriter GetScheduleDataObjectWriter(IDataWritingManager writeManager);
	}
}