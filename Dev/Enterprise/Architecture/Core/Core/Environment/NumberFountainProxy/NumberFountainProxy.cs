using System;
using CargoWise.Data;
using Enterprise.NumberFountain;

namespace Enterprise.ZArchitecture.Environment
{
	partial class NumberFountainProxy : INumberFountainProxy
	{
		public NumberFountainProxy(INumberFountain inner)
		{
			this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
		}

		public bool EnsureConsistentSequence => inner.EnsureConsistentSequence;

		public string Prefix => inner is IFormattedFountain formattedFountain ? formattedFountain.Prefix : string.Empty;

		public string Name => inner.Name;

		public long GetNext(IDbConnected dbConnected)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			return inner.GetNext(connection.InternalDbConnection, connection.InternalDbTransaction);
		}

		public long[] GetNexts(IDbConnected dbConnected, int amount)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			return inner.GetNexts(connection.InternalDbConnection, connection.InternalDbTransaction, amount);
		}

		public string GetNextFormatted(IDbConnected dbConnected)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			return inner.GetNextFormatted(connection.InternalDbConnection, connection.InternalDbTransaction);
		}

		public string[] GetNextsFormatted(IDbConnected dbConnected, int amount)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			return inner.GetNextsFormatted(connection.InternalDbConnection, connection.InternalDbTransaction, amount);
		}

		public void SetNext(IDbConnected dbConnected, long nextValue)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			inner.SetNext(connection.InternalDbConnection, connection.InternalDbTransaction, nextValue);
		}

		public void SetValues(IDbConnected dbConnected, long minValue, long nextValue, long maxValue)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			inner.SetValues(connection.InternalDbConnection, connection.InternalDbTransaction, minValue, nextValue, maxValue);
		}

		public void GetMinAndMaxValues(IDbConnected dbConnected, out long minValue, out long maxValue)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			inner.GetMinAndMaxValues(connection.InternalDbConnection, connection.InternalDbTransaction, out minValue, out maxValue);
		}

		public long PeekPreliminary(IDbConnected dbConnected)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			return inner.PeekPreliminary(connection.InternalDbConnection, connection.InternalDbTransaction);
		}

		public long PeekPreliminaryOrDefault(IDbConnected dbConnected, long defaultValue)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			return inner.PeekPreliminaryOrDefault(connection.InternalDbConnection, connection.InternalDbTransaction, defaultValue);
		}

		public string PeekPreliminaryFormatted(IDbConnected dbConnected)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			return inner.PeekPreliminaryFormatted(connection.InternalDbConnection, connection.InternalDbTransaction);
		}

		public void FixFountain(IDbConnected dbConnected, string uniqueIndexViolated)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			inner.FixFountain(connection.InternalDbConnection, uniqueIndexViolated);
		}

		public void FixFountain(IDbConnected dbConnected, DbCommand commandToFindMaxValueInDatabase)
		{
			IDbConnectionInternals connection = dbConnected.Connection;
			NumberFountainVisited_ForTest();
			inner.FixFountain(commandToFindMaxValueInDatabase);
		}

		#region IEquatable

		public override bool Equals(object other) => other is NumberFountainProxy oProxy && oProxy.Equals(this);
		public bool Equals(INumberFountainProxy other) => other is NumberFountainProxy oProxy && oProxy.Equals(this);
		public bool Equals(NumberFountainProxy other) => other.inner.Equals(inner);

		public override int GetHashCode() => inner.GetHashCode();

		#endregion // IEquatable

		#region Implementation

		readonly INumberFountain inner;

		partial void NumberFountainVisited_ForTest();

		#endregion // Implementation
	}
}

#region Test
#if DEBUG

#region Partial class

namespace Enterprise.ZArchitecture.Environment
{
	using CargoWise.Common;

	public partial class NumberFountainProxy
	{
		public static readonly Overridable<int> NumberOfFountainCommands_ForTest = new Overridable<int>(0);

		partial void NumberFountainVisited_ForTest()
		{
			if (Globals.IsTest)
			{
				NumberFountainProxy.NumberOfFountainCommands_ForTest.Value++;
			}
		}
	}
}

#endregion // Partial class

#endif
#endregion
