using System.Collections.Generic;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine
{
	/// <summary>
	/// Archive System provides implementation to each ArchiveSystemDescriptor provided by modules, that allows ArchiveManager to archive them.
	/// </summary>
	public sealed class ArchiveSystem : IArchiveSystem
	{
		public ArchiveSystem(IArchiveSystemDescriptor descriptor)
			=> Descriptor = descriptor;

		public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
			=> Descriptor.GetArchiveStages(config);

		public IArchiveSystemDescriptor Descriptor { get; }
	}
}
