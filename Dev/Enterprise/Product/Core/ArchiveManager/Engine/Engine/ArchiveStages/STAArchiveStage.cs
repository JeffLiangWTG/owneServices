using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.ArchiveManager.Engine.ArchiveStages
{
	public class STAArchiveStage : ArchiveStage
	{
		public STAArchiveStage(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor)
			: base(descriptor, systemDescriptor)
		{
		}

		public override int BatchSize
			=> SystemDataRegistry.Instance.StandaloneRecordsBatchSizeControl.Value;
	}
}
