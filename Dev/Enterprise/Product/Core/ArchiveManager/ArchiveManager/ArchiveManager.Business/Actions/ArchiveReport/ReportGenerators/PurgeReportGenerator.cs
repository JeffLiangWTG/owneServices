using Enterprise.ArchiveManager.Business.Actions.ArchiveReport.ReportGenerators;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Actions.ArchiveReport
{
	public class PurgeReportGenerator : BaseReportGenerator
	{
		public override string ReportTypeString
			=> (NoResString)"Purge";

		public override void PrepareReportDataSource(ICommonArchiveStageDescriptor commonArchiveStageDescriptor, IArchiveSystemDescriptor systemDescriptor, IArchiveSchedule schedule, IArchiveLogger logger, IArchiveConfiguration config)
		{
			base.PrepareReportDataSource(commonArchiveStageDescriptor, systemDescriptor, schedule, logger, config);

			TopLevelDataSource.TotaleDocs = commonArchiveStageDescriptor.ProcessingInfoPerTable.ContainsKey(StorageDocsSchema.Constants.TableName) ? commonArchiveStageDescriptor.ProcessingInfoPerTable[StorageDocsSchema.Constants.TableName].Count : 0;
			TopLevelDataSource.StorageDocsPerHour = ReportGeneratorHelper.CalculateRecordsPerHour(commonArchiveStageDescriptor, StorageDocsSchema.Constants.TableName);

			logger.LogInfo(systemDescriptor.Code, $"{commonArchiveStageDescriptor.MainArchivePKColumn.TableName}s Per Hour: {TopLevelDataSource.MainArchiveTablePerHour}, StorageDocs Per Hour: {TopLevelDataSource.StorageDocsPerHour}");
		}
	}
}
