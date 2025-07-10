using System;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Environment
{
	public interface INumberFountainProxy : IEquatable<INumberFountainProxy>
	{
		/// <summary>
		/// When this is false, we are using the LossyNumberFountain
		/// </summary>
		bool EnsureConsistentSequence { get; }
		string Prefix { get; }
		string Name { get; }

		long GetNext(IDbConnected dbConnected);
		long[] GetNexts(IDbConnected dbConnected, int amount);

		string GetNextFormatted(IDbConnected dbConnected);
		string[] GetNextsFormatted(IDbConnected dbConnected, int amount);

		void SetNext(IDbConnected dbConnected, long nextValue);

		/// <summary>
		/// Resets fountain values.
		/// </summary>
		/// <param name="dbConnected">Data Persistence Context.</param>
		/// <param name="minValue">Min Value to set. 0 if parameter should be omitted</param>
		/// <param name="nextValue">Next Value to set. 0 if parameter should be omitted</param>
		/// <param name="maxValue">Max Value to set. 0 if parameter should be omitted</param>
		void SetValues(IDbConnected dbConnected, long minValue, long nextValue, long maxValue);

		/// <summary>
		/// Returns the minimum and maximum values for the fountain.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		void GetMinAndMaxValues(IDbConnected dbConnected, out long minValue, out long maxValue);

		/// <summary>
		/// Returns a preliminary value for the fountain. 
		/// Doesn't lock the row for update, neither cares if row is being updated by another transaction.
		/// </summary>
		long PeekPreliminary(IDbConnected dbConnected);

		/// <summary>
		/// Returns the default value, if the fountain is not set
		/// </summary>
		long PeekPreliminaryOrDefault(IDbConnected dbConnected, long defaultValue);

		/// <summary>
		/// Returns a preliminary value for the fountain. 
		/// Doesn't lock the row for update, neither cares if row is being updated by another transaction.
		/// </summary>
		string PeekPreliminaryFormatted(IDbConnected dbConnected);

		void FixFountain(IDbConnected dbConnected, string uniqueIndexViolated);
		void FixFountain(IDbConnected dbConnected, DbCommand commandToFindMaxValueInDatabase);
	}
}
