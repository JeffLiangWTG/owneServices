using System.Linq;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine.ArchiveStages
{
	public class OPSArchiveStage : ArchiveStage
	{
		public OPSArchiveStage(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor) : base(descriptor, systemDescriptor)
		{
		}

		protected override void LogSummaryOfDocumentsGeneratedAndTimeTaken()
		{
			var totalNumberOfDocumentsGenerated = allLoadedArchiveSets.Sum(set => set.TotalNumberOfDocumentsGeneratedInSet);
			var totalTimeTakenToLoadDocuments = allLoadedArchiveSets.Sum(set => set.TimeTakenToGenerateDocuments);
			_logger.LogInfo(systemDescriptor.Code, $"Time taken to generate {totalNumberOfDocumentsGenerated} missing document(s): {totalTimeTakenToLoadDocuments}ms");
		}
	}
}
