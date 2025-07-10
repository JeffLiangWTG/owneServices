using System.Collections.Generic;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Integration;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummySimpleArchiveSystemWithNoArchiveStages))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummySimpleArchiveSystemWithNoArchiveStages : DummySimpleArchiveSystemDescriptor
	{
		public override string Code
			=> TestArchiveManagerConstants.Codes.DNS;

		public override IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield break;
		}
	}
}
