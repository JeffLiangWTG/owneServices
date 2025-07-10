using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.NumberFountain.Testing
{
	[UseSnapshotProtection]
	abstract class NumberFountainTestBase : TestCase
	{
		protected abstract INumberFountain GetFountain(string name, Guid ownerPk, bool rollover, int minValue, int maxValue);

		protected abstract string IntToStringForGetNext(int i);

		public void TestConstructor()
		{
			Constructor(GetName(), Guid.Empty);
		}

		public void TestConstructorOwned()
		{
			Constructor(GetName(), Guid.NewGuid());
		}

		protected abstract void Constructor(string name, Guid ownerPk);

		protected static void TestFountainConstructor(IFountain fountain, string name, Guid ownerPk, bool rollover, int minValue, int maxValue)
		{
			AssertEquals(name, fountain.Name);
			AssertEquals(ownerPk, fountain.OwnerPk);
			AssertEquals(rollover, fountain.RollOver);
			AssertEquals(minValue, fountain.MinValue);
			AssertEquals(maxValue, fountain.MaxValue);
		}

		public void TestGetNext()
		{
			GetNext(GetName(), Guid.Empty, TestConnection());
		}

		public void TestGetNextOwned()
		{
			GetNext(GetName(), Guid.NewGuid(), TestConnection());
		}

		void GetNext(string name, Guid ownerPk, DbConnection connection)
		{
			using (connection.BeginTransactionWithManager())
			{
				var fountain = GetFountain(name, ownerPk, false, 1, 9);
				var transaction = Transaction(connection);
				var dbConnection = Connection(connection);

				AssertExceptionThrown(
					typeof(ArgumentNullException),
					() => fountain.GetNextFormatted(null, null)
				);

				for (var i = 1; i <= 9; i++)
				{
					AssertEquals("Next value", IntToStringForGetNext(i), fountain.GetNextFormatted(dbConnection, transaction));
				}

				AssertExceptionThrown(
					typeof(NumberFountainMaximumValueReachedException),
					() => fountain.GetNextFormatted(dbConnection, transaction)
				);

				connection.CommitTransaction();
			}
		}

		public void TestGetNextInt()
		{
			GetNextInt(GetName(), Guid.Empty, TestConnection());
		}

		public void TestGetNextIntOwned()
		{
			GetNextInt(GetName(), Guid.NewGuid(), TestConnection());
		}

		void GetNextInt(string name, Guid ownerPk, DbConnection connection)
		{
			using (connection.BeginTransactionWithManager())
			{
				var fountain = GetFountain(name, ownerPk, false, 1, 9);
				var transaction = Transaction(connection);
				var dbConnection = Connection(connection);

				AssertExceptionThrown(
					typeof(ArgumentNullException),
					() => fountain.GetNext(null, null)
					);

				for (var i = 1; i <= 9; i++)
				{
					AssertEquals("Next value", i, fountain.GetNext(dbConnection, transaction));
				}

				AssertExceptionThrown(
					typeof(NumberFountainMaximumValueReachedException),
					() => fountain.GetNext(dbConnection, transaction)
					);

				connection.CommitTransaction();
			}
		}

		public void TestGetNexts()
		{
			GetNexts(GetName(), Guid.Empty, TestConnection());
		}

		public void TestGetNextsOwned()
		{
			GetNexts(GetName(), Guid.NewGuid(), TestConnection());
		}

		void GetNexts(string name, Guid ownerPk, DbConnection connection)
		{
			using (connection.BeginTransactionWithManager())
			{
				var fountain = GetFountain(name, ownerPk, false, 18, 1017);
				var transaction = Transaction(connection);
				var dbConnection = Connection(connection);

				fountain.SetValues(dbConnection, transaction, minValue: 18, nextValue: 18, maxValue: 1017);

				AssertExceptionThrown(
					typeof(ArgumentNullException),
					() => fountain.GetNexts(null, null, 10)
					);
				AssertExceptionThrown(
					typeof(ArgumentOutOfRangeException),
					() => fountain.GetNexts(dbConnection, transaction, -1)
					);
				AssertExceptionThrown(
					typeof(ArgumentOutOfRangeException),
					() => fountain.GetNexts(dbConnection, transaction, 0)
					);

				TestArrays("Next values", Enumerable.Range(18, 1000).Select(IntToStringForGetNext), fountain.GetNextsFormatted(dbConnection, transaction, 1000));

				AssertExceptionThrown(
					typeof(NumberFountainMaximumValueReachedException),
					() => fountain.GetNexts(dbConnection, transaction, 1)
					);

				connection.CommitTransaction();
			}
		}

		public void TestGetNextInts()
		{
			GetNextInts(GetName(), Guid.Empty, TestConnection());
		}

		public void TestGetNextIntsOwned()
		{
			GetNextInts(GetName(), Guid.NewGuid(), TestConnection());
		}

		void GetNextInts(string name, Guid ownerPk, DbConnection connection)
		{
			using (connection.BeginTransactionWithManager())
			{
				var fountain = GetFountain(name, ownerPk, false, 18, 1017);
				var transaction = Transaction(connection);
				var dbConnection = Connection(connection);

				AssertExceptionThrown(
					typeof(ArgumentNullException),
					() => fountain.GetNexts(null, null, 10)
					);
				AssertExceptionThrown(
					typeof(ArgumentOutOfRangeException),
					() => fountain.GetNexts(dbConnection, transaction, -1)
					);
				AssertExceptionThrown(
					typeof(ArgumentOutOfRangeException),
					() => fountain.GetNexts(dbConnection, transaction, 0)
					);

				TestArrays("Next values", Enumerable.Range(18, 1000).Select(x => (long)x), fountain.GetNexts(dbConnection, transaction, 1000));

				AssertExceptionThrown(
					typeof(NumberFountainMaximumValueReachedException),
					() => fountain.GetNexts(dbConnection, transaction, 1)
					);

				connection.CommitTransaction();
			}
		}

		protected abstract DbConnection TestConnection();

		protected static IDbTransaction Transaction(DbConnection connection)
		{
			return ((IDbConnectionInternals)connection).ADOTransaction;
		}

		protected static IDbConnection Connection(DbConnection connection)
		{
			return ((IDbConnectionInternals)connection).InternalDbConnection;
		}
		protected static string GetName() => DateTime.Now.ToString("HHmmssffff"); // For test reasons only

		public static void TestArrays<T>(string message, IEnumerable<T> expected, IReadOnlyList<T> result)
		{
			var enumerable = expected as T[] ?? expected.ToArray();
			AssertEquals(message, enumerable.Length, result.Count);
			for (var i = 0; i < enumerable.Length; i++)
			{
				AssertEquals(message, enumerable[i], result[i]);
			}
		}
	}
}
