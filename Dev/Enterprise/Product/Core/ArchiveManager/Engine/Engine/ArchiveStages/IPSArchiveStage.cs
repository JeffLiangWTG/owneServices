using System.Linq;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine.ArchiveStages
{
	public class IPSArchiveStage : ArchiveStage
	{
		public IPSArchiveStage(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor)
			: base(descriptor, systemDescriptor)
		{
			this.systemDescriptor = systemDescriptor;
		}

		protected override void LogSummaryOfDocumentsGeneratedAndTimeTaken()
		{
			var totalNumberOfDocumentsGenerated = allLoadedArchiveSets.Sum(set => set.TotalNumberOfDocumentsGeneratedInSet);
			var totalTimeTakenToLoadDocuments = allLoadedArchiveSets.Sum(set => set.TimeTakenToGenerateDocuments);
			_logger.LogInfo(systemDescriptor.Code, $"Time taken to generate {totalNumberOfDocumentsGenerated} missing document(s): {totalTimeTakenToLoadDocuments}ms");
		}

		readonly new IArchiveSystemDescriptor systemDescriptor;
	}
}
