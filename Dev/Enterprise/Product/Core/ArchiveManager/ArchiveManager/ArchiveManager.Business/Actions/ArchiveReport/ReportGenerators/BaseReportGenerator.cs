using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Business.Actions.ArchiveReport.ReportGenerators
{
	public abstract class BaseReportGenerator : IReportGenerator
	{
		public ReportDataSourceNonPersistentBusinessObject TopLevelDataSource { get; }

		public BaseReportGenerator()
			=> TopLevelDataSource = new ReportDataSourceNonPersistentBusinessObject(new BusinessObjectFactory());

		public abstract string ReportTypeString { get; }

		public virtual void PrepareReportDataSource(ICommonArchiveStageDescriptor commonArchiveStageDescriptor, IArchiveSystemDescriptor systemDescriptor, IArchiveSchedule schedule, IArchiveLogger logger, IArchiveConfiguration config)
		{
			TopLevelDataSource.ReportName = systemDescriptor.Name;

			TopLevelDataSource.StageName = commonArchiveStageDescriptor.Name;
			TopLevelDataSource.StageCount = systemDescriptor.GetArchiveStageDescriptors(config).ToList().Count;
			TopLevelDataSource.StageStartTime = commonArchiveStageDescriptor.ArchiveStageStopWatch.StartAt.ToString();
			TopLevelDataSource.StageEndTime = commonArchiveStageDescriptor.ArchiveStageStopWatch.EndAt.ToString();
			TopLevelDataSource.StageDuration = ReportGeneratorHelper.StageDescriptorRunDurationToString(commonArchiveStageDescriptor.ArchiveStageStopWatch);

			TopLevelDataSource.MainArchiveTableName = commonArchiveStageDescriptor.MainArchivePKColumn.TableName;
			TopLevelDataSource.MainArchiveTablePerHour = ReportGeneratorHelper.CalculateRecordsPerHour(commonArchiveStageDescriptor, commonArchiveStageDescriptor.MainArchivePKColumn.TableName);

			TopLevelDataSource.BatchSize = RegistryHelper.GetBatchSizeValue(systemDescriptor.Code);
			TopLevelDataSource.OnOrBeforeMinimum = RegistryHelper.GetOnOrBeforeMinimumValue(systemDescriptor.Code);

			TopLevelDataSource.DateParameter = ReportGeneratorHelper.GetDateParameter(systemDescriptor, config);
			TopLevelDataSource.OnOrBeforeDate = config.ArchiveJobsOnOrBeforeThisDate.ToString("dd/MM/yyyy");
			TopLevelDataSource.MaxRunDuration = config.MaxRunDurationInMinutes;

			TopLevelDataSource.PastTenseVerb = systemDescriptor.PastTenseVerb;
			TopLevelDataSource.StageStartDate = ReportGeneratorHelper.JobStartDateToString(commonArchiveStageDescriptor);
			TopLevelDataSource.StageEndDate = ReportGeneratorHelper.JobEndDateToString(schedule.GetWatermark(commonArchiveStageDescriptor.Name), commonArchiveStageDescriptor);
		}

		public void GenerateReport(ICommonArchiveStageDescriptor commonArchiveStageDescriptor, IArchiveSystemDescriptor systemDescriptor, IArchiveSchedule schedule, IArchiveLogger logger, IArchiveConfiguration config)
		{
			PrepareReportDataSource(commonArchiveStageDescriptor, systemDescriptor, schedule, logger, config);
			ReportGeneratorHelper.AddProcessedRecordsCountToReportDataSource(TopLevelDataSource, commonArchiveStageDescriptor.ProcessingInfoPerTable);
			ReportGeneratorHelper.SortReportDataSourceCollection(TopLevelDataSource.Collection);

			LogReportDataSourceInfo(logger, systemDescriptor);

			using (var tempFile = TempFile.NewWithExtension("xls"))
			{
				ReportGeneratorHelper.GenerateFile($"{ReportTypeString}ReportTemplate.xlsx", Res.GetString("43F62573-3837-4E69-93A3-2551E4E1405B", "{0} Summary Report", systemDescriptor.Name), tempFile.Filename, TopLevelDataSource);
				ReportGeneratorHelper.AttachGeneratedReportToArchiveSchedule(schedule, tempFile, systemDescriptor, commonArchiveStageDescriptor, config, logger);
			}

			logger.LogInfo(systemDescriptor.Code, $"Generated {ReportTypeString} Report and stored on eDocs tab of the {ReportTypeString} Schedule");
		}

		void LogReportDataSourceInfo(IArchiveLogger logger, IArchiveSystemDescriptor systemDescriptor)
		{
			if (TopLevelDataSource.Collection.Count > 0)
			{
				var builder = new StringBuilder();

				foreach (var element in TopLevelDataSource.Collection.Cast<ReportDataSourceNonPersistentBusinessObject>())
				{
					_ = builder.AppendLine($"{element.TableNameCounted}: {element.DeletedRecordCount}");
				}

				logger.LogInfo(systemDescriptor.Code, $"{TopLevelDataSource.PastTenseVerb} Data Dated Between {TopLevelDataSource.StageStartDate} and {TopLevelDataSource.StageEndDate}{System.Environment.NewLine}{builder}");
			}
		}
	}
}
