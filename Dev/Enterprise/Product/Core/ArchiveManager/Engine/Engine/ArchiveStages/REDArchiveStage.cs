using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine.ArchiveStages
{
	public class REDArchiveStage : MAIOnlyArchiveStage
	{
		public REDArchiveStage(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor) : base(descriptor, systemDescriptor)
		{
		}
	}
}
