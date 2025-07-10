using Enterprise.ArchiveManager.Business.Actions.ArchiveReport.ReportGenerators;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Actions.ArchiveReport
{
	public class ArchiveReportGenerator : BaseReportGenerator
	{
		public override string ReportTypeString
			=> (NoResString)"Archive";

		public override void PrepareReportDataSource(ICommonArchiveStageDescriptor commonArchiveStageDescriptor, IArchiveSystemDescriptor systemDescriptor, IArchiveSchedule schedule, IArchiveLogger logger, IArchiveConfiguration config)
		{
			base.PrepareReportDataSource(commonArchiveStageDescriptor, systemDescriptor, schedule, logger, config);

			if (commonArchiveStageDescriptor.MainArchivePKColumn.TableName.Equals(JobHeaderSchema.Constants.TableName))
			{
				var jobShipmentsPerHour = ReportGeneratorHelper.CalculateRecordsPerHour(commonArchiveStageDescriptor, JobShipmentSchema.Constants.TableName);
				TopLevelDataSource.JobShipmentsPerHour = jobShipmentsPerHour;

				logger.LogInfo(systemDescriptor.Code, string.Format((NoResString)"JobShipments Per Hour: {0}, JobHeaders Per Hour: {1}", jobShipmentsPerHour, TopLevelDataSource.MainArchiveTablePerHour));
			}
			else
			{
				logger.LogInfo(systemDescriptor.Code, $"{commonArchiveStageDescriptor.MainArchivePKColumn.TableName}s Per Hour: {TopLevelDataSource.MainArchiveTablePerHour}");
			}
		}
	}
}
