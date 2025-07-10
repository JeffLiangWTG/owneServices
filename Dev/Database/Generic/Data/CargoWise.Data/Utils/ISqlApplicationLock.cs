using System;

namespace CargoWise.Data.Utils
{
	/// <summary>
	/// Interface that defines an application lock
	/// </summary>
	public interface ISqlApplicationLock : IDisposable
	{
		/// <summary>
		/// Gets the unique identifier of the lock
		/// </summary>
		string Key { get; }

		/// <summary>
		/// Returns whether the lock is currently held
		/// </summary>
		/// <returns>True if held, otherwise false</returns>
		bool IsHoldingLock();

		/// <summary>
		/// String representation of the lock
		/// </summary>
		/// <returns>A string representation for diagnostics</returns>
		string ToString();
	}
}