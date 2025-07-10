using System;
using System.Collections.Generic;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine.Test.ArchiveStages
{
	class ArchiveStageThatThrowsExceptionForTest : ArchiveStage
	{
		public ArchiveStageThatThrowsExceptionForTest(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor)
			: base(descriptor, systemDescriptor)
		{
		}

		public int NumberOfTimesToThrow { get; set; }

		public override IEnumerable<IArchiveSet> GetNextArchiveSet(IArchiveWatermark watermark, IArchiveSchedule schedule, IArchiveStage stage)
		{
			if (NumberOfTimesToThrow > 0 )
			{
				NumberOfTimesToThrow--;
				throw new Exception("Test Exception");
			}

			return base.GetNextArchiveSet(watermark, schedule, stage);
		}
	}
}
