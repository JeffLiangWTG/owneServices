using System.Collections.Concurrent;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.StageDescriptors
{
	public static class OnArchiveSetProcessedHelpers
	{
		public static void AddOrUpdateDocumentsDeletedCount(IArchiveSet set, ConcurrentDictionary<string, ITableProcessingInfo> processingInfoPerTable)
		{
			foreach (var item in set.GetTotalDocumentsDeletedFromArchiveItems())
			{
				_ = processingInfoPerTable.AddOrUpdate(
					item.Key,
					item.Value,
					(id, processingInfo) => { processingInfo.Count += item.Value.Count; return processingInfo; });
			}
		}

		public static void AddOrUpdateRecordsProcessedCount(IArchiveSet set, ConcurrentDictionary<string, ITableProcessingInfo> processingInfoPerTable, bool recordsPurged)
		{
			var schemaResolver = new EnterpriseSchemaResolver();

			foreach (var item in set.GetArchiveItems())
			{
				_ = processingInfoPerTable.AddOrUpdate(
					schemaResolver.GetTableSchemaFromColumnNamePrefix(item.TableCode).TableName,
					new TableProcessingInfo(1, recordsPurged),
					(id, processingInfo) => { processingInfo.Count += 1; return processingInfo; });
			}
		}

		public static void AddOrUpdateRecordsProcessedCount(IArchiveSet set, ConcurrentDictionary<string, ITableProcessingInfo> processingInfoPerTable)
		{
			var schemaResolver = new EnterpriseSchemaResolver();

			foreach (var item in set.GetArchiveItems())
			{
				_ = processingInfoPerTable.AddOrUpdate(
					schemaResolver.GetTableSchemaFromColumnNamePrefix(item.TableCode).TableName,
					new TableProcessingInfo(1, item.Purgeable),
					(id, processingInfo) => { processingInfo.Count += 1; return processingInfo; });
			}
		}
	}
}
