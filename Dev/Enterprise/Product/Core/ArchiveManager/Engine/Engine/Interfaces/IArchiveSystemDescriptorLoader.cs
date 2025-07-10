
using System.Collections.Generic;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine
{
	/// <summary>
	/// Loads ArchiveSystemDescriptor objects across the entire system.
	/// </summary>
	public interface IArchiveSystemDescriptorLoader
	{
		IEnumerable<IArchiveSystemDescriptor> Load();
	}
}
