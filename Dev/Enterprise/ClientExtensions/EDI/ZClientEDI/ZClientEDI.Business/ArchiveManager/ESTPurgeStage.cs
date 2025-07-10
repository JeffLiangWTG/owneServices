using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;

namespace ZClientEDI.Business.ArchiveManager
{
	public class ESTPurgeStage(IArchiveStageDescriptor descriptor, ISelfContainedArchiveSystemDescriptor systemDescriptor) : ArchiveStage(descriptor, systemDescriptor)
	{
		public override int BatchSize => systemDescriptor.BatchSizeControlValue;
	}
}
