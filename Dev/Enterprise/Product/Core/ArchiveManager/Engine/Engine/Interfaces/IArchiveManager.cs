using System.Collections.Generic;
using System.Threading;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine
{
	/// <summary>
	/// Loads and manages all ArchiveSystem objects provided throughout the entire system
	/// </summary>
	public interface IArchiveManager
	{
		List<IArchiveSystemDescriptor> ArchiveSystemDescriptors { get; }
		void Run(string systemCode, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSchedule schedule, CancellationToken token);
	}
}
