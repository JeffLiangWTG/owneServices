using System.Linq;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine.ArchiveStages
{
	public class PDRPurgeStage : ArchiveStage
	{
		public PDRPurgeStage(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor) : base(descriptor, systemDescriptor)
		{
		}

		protected override void LogSummaryOfNumberDeletedAndTimeTaken()
		{
			var totalTimeTakenToDelete = allLoadedArchiveSets.Sum(set => set.TimeTakenToDeleteAllDocuments);
			var totalNumberOfDocumentsDeleted = allLoadedArchiveSets.Sum(set =>
			set.GetTotalDocumentsDeletedFromArchiveItems().ContainsKey(StorageDocsSchema.Constants.TableName) ? set.GetTotalDocumentsDeletedFromArchiveItems()[StorageDocsSchema.Constants.TableName].Count : 0);
			_logger.LogInfo(systemDescriptor.Code, $"Time taken to delete {totalNumberOfDocumentsDeleted} document(s) and {totalNumberOfArchiveSetsLoaded} {mainArchiveableType.PKColumn.TableName} record(s) with their related record(s): " +
				$"{totalTimeTakenToDelete}ms");
		}
	}
}
