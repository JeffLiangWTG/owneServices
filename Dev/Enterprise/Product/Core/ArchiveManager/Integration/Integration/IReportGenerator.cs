namespace Enterprise.ArchiveManager.Integration
{
	public interface IReportGenerator
	{
		void PrepareReportDataSource(ICommonArchiveStageDescriptor commonArchiveStageDescriptor, IArchiveSystemDescriptor systemDescriptor, IArchiveSchedule schedule, IArchiveLogger logger, IArchiveConfiguration config);

		void GenerateReport(ICommonArchiveStageDescriptor commonArchiveStageDescriptor, IArchiveSystemDescriptor systemDescriptor, IArchiveSchedule schedule, IArchiveLogger logger, IArchiveConfiguration config);
	}
}
