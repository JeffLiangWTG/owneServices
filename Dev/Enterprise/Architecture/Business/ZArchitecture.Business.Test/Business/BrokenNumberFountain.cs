using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class BrokenNumberFountain : INumberFountainProxy
	{
		public bool EnsureConsistentSequence => true;
		public string Prefix => string.Empty;
		public string Name => "Trevi Fountain";

		public void FixFountain(IDbConnected dbConnected, DbCommand commandToFindMaxValueInDatabase)
		{
			throw new Exception("Failed to fix fountain conflict.");
		}

		public void FixFountain(IDbConnected dbConnected, string uniqueIndexViolated)
		{
			throw new Exception("Failed to fix fountain conflict.");
		}

		public string GetNextFormatted(IDbConnected dbConnected)
		{
			return "1001";
		}

		public long GetNext(IDbConnected dbConnected)
		{
			return 1001;
		}

		public string[] GetNextsFormatted(IDbConnected dbConnected, int amount)
		{
			return new[] { "1001" };
		}

		public long[] GetNexts(IDbConnected dbConnected, int amount)
		{
			return new[] { (long)1001 };
		}

		public void SetNextValue(IDbConnected dbConnected, long nextValue)
		{
		}

		public void GetMinAndMaxValues(IDbConnected dbConnected, out long minValue, out long maxValue)
		{
			minValue = 1;
			maxValue = 2000;
		}

		public string PeekPreliminaryFormatted(IDbConnected dbConnected)
		{
			return "1000";
		}

		public long PeekPreliminary(IDbConnected dbConnected)
		{
			return 1000;
		}

		public long PeekPreliminaryOrDefault(IDbConnected connection, long defaultValue)
		{
			return defaultValue;
		}

		public void SetNext(IDbConnected dbConnected, long nextValue)
		{
		}

		public void SetValues(IDbConnected dbConnected, long minValue, long nextValue, long maxValue)
		{
		}

		public bool Equals(INumberFountainProxy other) => other == this;

		public long MaxValue => 2000;
	}
}
