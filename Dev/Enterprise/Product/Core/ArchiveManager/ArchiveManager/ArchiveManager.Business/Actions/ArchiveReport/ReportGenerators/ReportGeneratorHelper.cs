using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Actions.ArchiveReport
{
	public static class ReportGeneratorHelper
	{
		public static void TryGenerateReport(ICommonArchiveStageDescriptor commonArchiveStageDescriptor, IArchiveSystemDescriptor systemDescriptor, IArchiveLogger logger, IArchiveSchedule schedule, IArchiveConfiguration config)
		{
			if (systemDescriptor.ReportGenerator == null)
			{
				logger.LogError(systemDescriptor.Code, $"Attempted to call ReportGenerator during Finalise method but it was not set on '{systemDescriptor.Name}'.");
				return;
			}

			systemDescriptor.ReportGenerator.GenerateReport(commonArchiveStageDescriptor, systemDescriptor, schedule, logger, config);
		}

		public static void GenerateFile(string fileName, string reportName, string outputExcelReportPath, NonPersistentBusinessObject topLevelDataSource)
		{
			using var templateStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.ArchiveManager.Business.Actions.ArchiveReport.ReportTemplates." + fileName);
			var factory = new BusinessObjectFactory();
			var stmMenuItem = factory.New<DocumentCommand>();
			var excelTemplate = new ExcelTemplateWrappingStream(reportName, templateStream);

			using var pack = new DocumentPack(stmMenuItem);
			using var report = new Report(pack, excelTemplate, BODocDataProvider.Get(topLevelDataSource), excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
			using var outputStream = new FileStream(outputExcelReportPath, FileMode.Create);
			_ = report.Save(outputStream);
			outputStream.Flush();
		}

		public static void AttachGeneratedReportToArchiveSchedule(IArchiveSchedule schedule, TempFile tempFile, IArchiveSystemDescriptor systemDescriptor, IArchiveStageDescriptor stageDescriptor, IArchiveConfiguration config, IArchiveLogger logger)
		{
			var reportFileName = GetReportFileName(systemDescriptor, stageDescriptor, config);
			schedule.AttachEDoc(tempFile.Filename, reportFileName, Core.Constants.RefDocTypes.MiscellaneousDocument, GetReportFileDescription(systemDescriptor));
		}

		public static void AddProcessedRecordsCountToReportDataSource(ReportDataSourceNonPersistentBusinessObject topLevelDataSource, ConcurrentDictionary<string, ITableProcessingInfo> processedRecordsCountPerTable)
		{
			var tableRecordCount = TableRecordCountsHelper.GetTableRecordCounts();

			foreach (var table in processedRecordsCountPerTable)
			{
				if (table.Value.Purged)
				{
					var tableName = table.Key;

					var recordCount = topLevelDataSource.Collection.AddNew();
					recordCount.TableNameCounted = tableName;
					recordCount.DeletedRecordCount = processedRecordsCountPerTable[tableName].Count;

					if (tableName == StorageDocsSchema.PK.TableName)
					{
						var sdTableRecordCount = TableRecordCountsHelper.GetSDTableRecordCounts();
						recordCount.RecordCountAfter = sdTableRecordCount.RowCount;
						recordCount.SpaceUsedAfter = KBToMB(sdTableRecordCount.DataKB + sdTableRecordCount.IndexSizeKB);
					}
					else
					{
						var tableAfter = tableRecordCount[tableName];
						recordCount.RecordCountAfter = tableAfter.RowCount;
						recordCount.SpaceUsedAfter = KBToMB(tableAfter.DataKB + tableAfter.IndexSizeKB);
					}
				}
			}
		}

		public static long KBToMB(long kiloBytes)
			=> kiloBytes / 1024;

		public static int GetProcessedCountForTable(string tableName, ConcurrentDictionary<string, ITableProcessingInfo> processedRecordsCountPerTable)
			=> processedRecordsCountPerTable.TryGetValue(tableName, out var result) ? result.Count : 0;

		public static double StageDescriptorRunDurationInSeconds(ICommonArchiveStageDescriptor commonArchiveStageDescriptor)
			=> Math.Max(1, commonArchiveStageDescriptor.ArchiveStageStopWatch.Elapsed.TotalSeconds);

		public static string StageDescriptorRunDurationToString(IArchiveStageStopwatch stopwatch)
			=> string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds);

		public static string JobStartDateToString(ICommonArchiveStageDescriptor commonArchiveStageDescriptor)
			=> commonArchiveStageDescriptor.WatermarkAtBeginning == null
				? Res.GetString("6e652fe0-87fd-43b7-b37a-9e9f02d11e47", "Earliest Possible Date")
				: commonArchiveStageDescriptor.WatermarkAtBeginning.WatermarkDate.ToShortDateString();

		public static string JobEndDateToString(IArchiveWatermark watermark, ICommonArchiveStageDescriptor commonArchiveStageDescriptor)
			=> watermark == null
				? commonArchiveStageDescriptor.ArchiveToDateAtBeginning.ToShortDateString()
				: watermark.WatermarkDate.ToShortDateString();

		public static long CalculateRecordsPerHour(ICommonArchiveStageDescriptor commonArchiveStageDescriptor, string tableName)
		{
			long totalRecords = commonArchiveStageDescriptor.ProcessingInfoPerTable.ContainsKey(tableName) ? commonArchiveStageDescriptor.ProcessingInfoPerTable[tableName].Count : 0;
			var recordsPerHour = totalRecords / StageDescriptorRunDurationInSeconds(commonArchiveStageDescriptor) * 3600;

			return (long)Utilities.Round((decimal)recordsPerHour, 0);
		}

		public static string GetDateParameter(IArchiveSystemDescriptor archiveSystemDescriptor, IArchiveConfiguration config)
		{
			return archiveSystemDescriptor.AllowDateParameterSelection ? DateParameterStrings.GetName(config.IsFilteringByJobOpenDate) : "N/A";
		}

		public static string GetReportFileName(IArchiveSystemDescriptor systemDescriptor, IArchiveStageDescriptor stageDescriptor, IArchiveConfiguration config)
		{
			var systemDescriptorName = systemDescriptor.Name.GetUnresolvedString().Replace(" ", "");
			var currentDateTimeFormatted = ZDateTime.UtcNow.ToString("yyyyMMddHHmm");
			var systemHasMultipleStages = systemDescriptor.GetArchiveStageDescriptors(config).Count() > 1;

			return systemHasMultipleStages
				? $"{systemDescriptorName}Report_{stageDescriptor.MainArchivePKColumn.TableName}_{currentDateTimeFormatted}.xls"
				: $"{systemDescriptorName}Report_{currentDateTimeFormatted}.xls";
		}

		public static string GetReportFileDescription(IArchiveSystemDescriptor systemDescriptor)
			=> $"{systemDescriptor.Noun.GetUnresolvedString()} Report";

		public static void SortReportDataSourceCollection(ReportDataSourceNonPersistentBusinessObjectCollection topLevelDataSourceCollection)
		{
			topLevelDataSourceCollection.Sort<ReportDataSourceNonPersistentBusinessObject>(
			(lhs, rhs) =>
			{
				var diff = rhs.DeletedRecordCount.CompareTo(lhs.DeletedRecordCount);
				return diff == 0
					? rhs.RecordCountAfter.CompareTo(lhs.RecordCountAfter)
					: diff;
			});
		}
	}
}
