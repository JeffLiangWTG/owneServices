using System.Linq;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine.ArchiveStages
{
	public class HARArchiveStage : ArchiveStage
	{
		public HARArchiveStage(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor)
			: base(descriptor, systemDescriptor)
		{ }

		public override IArchiveStepResult ArchiveToImages(IArchiveSet archiveSet)
		{
			archiveSet.MainArchiveItem.Purgeable = false;

			archiveSet.GetArchiveItems()
				.Where(item => item.TableCode.Equals(HVLVConsignmentHeaderSchema.Constants.Prefix))
				.ToList()
				.ForEach(item => item.Purgeable = false);

			return base.ArchiveToImages(archiveSet);
		}
	}
}
