using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine.ArchiveStages
{
	public class PALArchiveStage : MAIOnlyArchiveStage
	{
		public PALArchiveStage(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor) : base(descriptor, systemDescriptor)
		{
		}
	}
}
