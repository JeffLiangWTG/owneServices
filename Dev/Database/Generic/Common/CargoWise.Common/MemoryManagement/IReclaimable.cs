using System;

namespace CargoWise.Common.MemoryManagement
{
	/// <summary>
	/// Represents a reclaimable object wrapper.
	/// Provides the ability to manage the instance.
	/// </summary>
	public interface IReclaimable
	{
		/// <summary>
		/// A plain english description of the reclaimable object
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Construction time of the object
		/// </summary>
		DateTime ConstructionTime { get; }

		/// <summary>
		/// The method that is called when the system is under memory pressure or cleanup is manually instigated
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		FlushResult CleanUp(FlushAction action);
	}
}
