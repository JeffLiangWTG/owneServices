using System;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Data.Utils
{
	/// <summary>
	/// Defines the interface for class that can provide application locks
	/// </summary>
	public interface ISqlApplicationLockProvider
	{
		/// <summary>
		/// Try to obtain an application lock (No wait)
		/// </summary>
		/// <param name="key">Unique lock identifier</param>
		/// <param name="appLock">The lock (if successful)</param>
		/// <param name="dbName">Optional database name for lock</param>
		/// <returns>True if lock was taken, otherwise false</returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter required by design")]
		bool TryGetLock(string key, out ISqlApplicationLock appLock, string dbName = null);

		/// <summary>
		/// Try to obtain an application lock
		/// </summary>
		/// <param name="key">Unique lock identifier</param>
		/// <param name="lockTimeout">How long to wait for the lock</param>
		/// <param name="appLock">The lock (if successful)</param>
		/// <param name="dbName">Optional database name for lock</param>
		/// <returns>True if lock was taken, otherwise false</returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter required by design")]
		bool TryGetLock(string key, TimeSpan lockTimeout, out ISqlApplicationLock appLock, string dbName = null);
	}
}
