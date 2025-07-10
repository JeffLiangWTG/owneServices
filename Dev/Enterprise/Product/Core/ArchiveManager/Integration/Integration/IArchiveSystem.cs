using System.Collections.Generic;

namespace Enterprise.ArchiveManager.Integration
{
	/// <summary>
	/// Archive System provides implementation to each ArchiveSystemDescriptor provided by modules, that allows ArchiveManager to archive them.
	/// </summary>
	public interface IArchiveSystem
	{
		IArchiveSystemDescriptor Descriptor { get; }

		IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config);
	}
}
