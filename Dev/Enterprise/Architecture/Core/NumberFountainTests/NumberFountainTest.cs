using Enterprise.NumberFountain.Internal;

namespace Enterprise.NumberFountain.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using CargoWise.Data;
	using CargoWise.Data.Testing;
	using NUnit.Framework;

	[UseSnapshotProtection]
	public sealed class NumberFountainNoTransactionTest : TestCase
	{
		[ExpectExceptionMessage(typeof(Exception), "The value returned from GetMaxFountainColumnValue is not valid. Value: 0")]
		public void TestFixFountainWhenInvalidMaxValueIsReturned()
		{
			string sqlText = @"
				select * into #StorageMain from dbo.StorageMain
				DELETE dbo.StorageMain
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), -87, newid(), 1)
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), -534, newid(), 1)
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), 0, newid(), 1)
				CREATE UNIQUE INDEX NR_UX__SM_CD1_SM_CD2 ON StorageMain(SM_CD1, SM_CD2)";
			Db.Connection.ExecuteNonQuery(sqlText);

			CacheableFountainForTest testFountain = new CacheableFountainForTest();
			AssertEquals("Peek 1st Value", 1, testFountain.PeekPreliminary(Connection, Transaction));
			Db.Connection.BeginTransaction();
			AssertEquals("Get 1st Value", 1, testFountain.GetNext(Connection, Transaction));
			Db.Connection.CommitTransaction();
			AssertEquals("Peek 2nd Value", 2, testFountain.PeekPreliminary(Connection, Transaction));

			var cmd = Connection.CreateCommand();
			cmd.CommandText = "SELECT MAX(SM_CD1) FROM dbo.StorageMain";
			cmd.Transaction = Transaction;
			testFountain.FixFountain(cmd);
			Fail("Should have had error");
		}

		/// <summary>
		/// Picks a table simple to insert values that has an integer field and populates it.
		/// Then a unique index is added on the integer column, in order to test the Number Fountain fix.
		/// </summary>
		public void TestFixFountainWithAnIntegerColumn()
		{
			string sqlText = @"
				select * into #StorageMain from dbo.StorageMain
				DELETE dbo.StorageMain
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), 87, newid(), 1)
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), 534, newid(), 1)
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), 9, newid(), 1)";
			Db.Connection.ExecuteNonQuery(sqlText);

			sqlText = "CREATE UNIQUE INDEX NR_UX__SM_CD1 ON StorageMain(SM_CD1)";
			Db.Connection.ExecuteNonQuery(sqlText);

			CacheableFountainForTest testFountain = new CacheableFountainForTest();
			AssertEquals("Peek 1st Value", 1, testFountain.PeekPreliminary(Connection, Transaction));
			Db.Connection.BeginTransaction();
			AssertEquals("Get 1st Value", 1, testFountain.GetNext(Connection, Transaction));
			AssertEquals("Peek 2nd Value", 2, testFountain.PeekPreliminary(Connection, Transaction));
			Db.Connection.CommitTransaction();

			testFountain.FixFountain(Connection, "NR_UX__SM_CD1");

			Db.Connection.BeginTransaction();
			try
			{
				AssertEquals("Peek After Fix", 535, testFountain.PeekPreliminary(Connection, Transaction));
				AssertEquals("Get After Fix", 535, testFountain.GetNext(Connection, Transaction));
				AssertEquals("Peek After Fix+Get", 536, testFountain.PeekPreliminary(Connection, Transaction));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public void TestFixFountainWithCompositeIndexAndDBCommandIsNotSupplied()
		{
			try
			{
				string sqlText = @"
				select * into #StorageMain from dbo.StorageMain
				DELETE dbo.StorageMain
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), 87, newid(), 1)
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), 534, newid(), 1)
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), 9, newid(), 1)";
				Db.Connection.ExecuteNonQuery(sqlText);

				sqlText = "CREATE UNIQUE INDEX NR_UX__SM_CD1_SM_CD2 ON StorageMain(SM_CD1, SM_CD2)";
				Db.Connection.ExecuteNonQuery(sqlText);

				CacheableFountainForTest testFountain = new CacheableFountainForTest();
				AssertEquals("Peek 1st Value", 1, testFountain.PeekPreliminary(Connection, Transaction));
				Db.Connection.BeginTransaction();
				AssertEquals("Get 1st Value", 1, testFountain.GetNext(Connection, Transaction));
				AssertEquals("Peek 2nd Value", 2, testFountain.PeekPreliminary(Connection, Transaction));
				Db.Connection.RollbackTransaction();

				testFountain.FixFountain(Connection, "NR_UX__SM_CD1_SM_CD2");
			}
			catch (Exception e)
			{
				string expected = "[NR_UX__SM_CD1_SM_CD2] is a composite index and therefore invalid for a Number Fountain Fix";
				AssertEquals("Wrong exception caught:\r\n" + e, expected, e.Message);
			}
		}

		public void TestFixFountainWithCompositeIndexAndDBCommandIsSupplied()
		{
			string sqlText = @"
				select * into #StorageMain from dbo.StorageMain;
				DELETE dbo.StorageMain
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), 87, newid(), 1)
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), 534, newid(), 1)
				INSERT dbo.StorageMain (SM_PK, SM_CD1, SM_ParentFK, SM_DB) VALUES (NEWID(), 9, newid(), 1)";
			Db.Connection.ExecuteNonQuery(sqlText);

			sqlText = "CREATE UNIQUE INDEX NR_UX__SM_CD1_SM_CD2 ON StorageMain(SM_CD1, SM_CD2)";
			Db.Connection.ExecuteNonQuery(sqlText);

			CacheableFountainForTest testFountain = new CacheableFountainForTest();
			AssertEquals("Peek 1st Value", 1, testFountain.PeekPreliminary(Connection, Transaction));
			Db.Connection.BeginTransaction();
			AssertEquals("Get 1st Value", 1, testFountain.GetNext(Connection, Transaction));
			AssertEquals("Peek 2nd Value", 2, testFountain.PeekPreliminary(Connection, Transaction));

			var command = Connection.CreateCommand();
			command.CommandText = "SELECT MAX(SM_CD1) FROM dbo.StorageMain";
			Db.Connection.CommitTransaction();
			testFountain.FixFountain(command);
			Db.Connection.BeginTransaction();
			AssertEquals("Peek After Fix", 535, testFountain.PeekPreliminary(Connection, Transaction));
			AssertEquals("Get After Fix", 535, testFountain.GetNext(Connection, Transaction));
			AssertEquals("Peek After Fix+Get", 536, testFountain.PeekPreliminary(Connection, Transaction));
			Db.Connection.RollbackTransaction();
		}

		public void TestFixFountainThrowsExceptionWhenNoNumericValueIsReturnedFromTable()
		{
			string sqlText = @"
				select * into #RefEquipment from dbo.RefEquipment;
				DELETE dbo.RefEquipment
				INSERT dbo.RefEquipment (RQ_PK, RQ_Registration, RQ_ShortCode) VALUES(NEWID(), 'ZZ9', 'A')
				INSERT dbo.RefEquipment (RQ_PK, RQ_Registration, RQ_ShortCode) VALUES(NEWID(), 'X7' , 'B')";
			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.ExecuteNonQuery();

			CacheableFountainForTest testFountain = new CacheableFountainForTest();

			try
			{
				testFountain.FixFountain(Connection, "NR_UX__RQ_Registration");
				Fail("Should have thrown exception");
			}
			catch (Exception e)
			{
				string expected = "Cannot extract numeric value from column [RefEquipment.RQ_Registration] - Number Fountain Fix";
				AssertEquals("Wrong exception caught:\r\n" + e, expected, e.Message);
			}
		}

		public void TestFixFountainThrowsExceptionWhenAlreadyInTransaction()
		{
			AssertEquals("[PRE-CONDITION] Initial Transaction Count", 0, Db.Connection.AppTransactionCount);

			try
			{
				// Begins a 'secret' transaction so as to pass normal connection tests client side
				Db.Connection.ExecuteNonQuery("begin transaction");

				CacheableFountainForTest testFountain = new CacheableFountainForTest();

				try
				{
					testFountain.FixFountain(Connection, "NR_UX__RQ_Registration");
					Fail("Should have thrown exception");
				}
				catch (System.Data.Common.DbException e)
				{
					AssertEquals("Wrong exception caught:\r\n" + e, "SQL Connection must not be in a transaction whilst fixing fountain", e.Message);
				}
			}
			finally
			{
				Db.Connection.ExecuteNonQuery("rollback");
			}
		}

		public void TestFixFountainThrowsExceptionWhenCompositeIndex()
		{
			CacheableFountainForTest testFountain = new CacheableFountainForTest();

			try
			{
				testFountain.FixFountain(Connection, "NR_UC__SN_Name_SN_Owner_SN_Sequence");
				Fail("Should have thrown exception");
			}
			catch (Exception e)
			{
				string expected = "[NR_UC__SN_Name_SN_Owner_SN_Sequence] is a composite index and therefore invalid for a Number Fountain Fix";
				AssertEquals("Wrong exception caught:\r\n" + e, expected, e.Message);
			}
		}

		public void TestFixFountainThrowsExceptionWhenConnectionClosed()
		{
			CacheableFountainForTest testFountain = new CacheableFountainForTest();

			var connection = Connection;
			connection.Close();

			try
			{
				testFountain.FixFountain(connection, "NR_UX__RQ_Registration");
				Fail("Should have thrown exception");
			}
			catch (Exception e)
			{
				string expected = "ExecuteReader requires an open and available Connection. The connection's current state is closed.";
				AssertEquals("Wrong exception caught:\r\n" + e, expected, e.Message);
			}
		}

		public void TestFixFountain()
		{
			Db.Connection.BeginTransaction();
			string sqlText = @"
				select * into #RefEquipment from dbo.RefEquipment
				DELETE dbo.RefEquipment
				INSERT dbo.RefEquipment (RQ_PK, RQ_Registration, RQ_ShortCode) VALUES(NEWID(), '0000000197', 'A')
				INSERT dbo.RefEquipment (RQ_PK, RQ_Registration, RQ_ShortCode) VALUES(NEWID(), 'W000000759', 'B')
				INSERT dbo.RefEquipment (RQ_PK, RQ_Registration, RQ_ShortCode) VALUES(NEWID(), '0000000345', 'C')
				INSERT dbo.RefEquipment (RQ_PK, RQ_Registration, RQ_ShortCode) VALUES(NEWID(), '0000000002', 'D')
				INSERT dbo.RefEquipment (RQ_PK, RQ_Registration, RQ_ShortCode) VALUES(NEWID(), '2200000002', 'E')";
			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.ExecuteNonQuery();

			CacheableFountainForTest testFountain = new CacheableFountainForTest();
			AssertEquals("Peek 1st Value", 1, testFountain.PeekPreliminary(Connection, Transaction));
			AssertEquals("Get 1st Value", 1, testFountain.GetNext(Connection, Transaction));
			AssertEquals("Peek 2nd Value", 2, testFountain.PeekPreliminary(Connection, Transaction));

			Db.Connection.CommitTransaction();

			testFountain.FixFountain(Connection, "NR_UX__RQ_Registration");

			AssertEquals("Peek After Fix", 2200000003, testFountain.PeekPreliminary(Connection, null));
			Db.Connection.BeginTransaction();
			try
			{
				AssertEquals("Get After Fix", 2200000003, testFountain.GetNext(Connection, Transaction));
				AssertEquals("Peek After Fix+Get", 2200000004, testFountain.PeekPreliminary(Connection, Transaction));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public void TestFixFountainThrowsExceptionWhenInvalidIndex()
		{
			CacheableFountainForTest testFountain = new CacheableFountainForTest();

			try
			{
				testFountain.FixFountain(Connection, "Some_Invalid_Index_Name");
				Fail("Should have thrown exception");
			}
			catch (Exception e)
			{
				string expected = "Index name could not be found in current database : Some_Invalid_Index_Name";
				AssertEquals("Wrong exception caught:\r\n" + e, expected, e.Message);
			}
		}

		public void TestGetMinAndMaxValues()
		{
			var testFountain = new CacheableFountainForTest();

			testFountain.GetMinAndMaxValues(Connection, Transaction, out var minValue, out var maxValue);
			AssertEquals("Min Value", FountainUtils.MinNumber, minValue);
			AssertEquals("Max Value", FountainUtils.MaxNumber, maxValue);

			using (var transaction = Connection.BeginTransaction())
			{
				testFountain.SetValues(Connection, transaction, minValue: 5, nextValue: 0, maxValue: 7);
				transaction.Commit();
			}

			testFountain.GetMinAndMaxValues(Connection, null, out minValue, out maxValue);
			AssertEquals("Min Value", 5, minValue);
			AssertEquals("Max Value", 7, maxValue);
		}

		IDbTransaction Transaction
		{
			get
			{
				IDbConnectionInternals internals = Db.Connection;
				return internals.ADOTransaction;
			}
		}

		IDbConnection Connection
		{
			get
			{
				IDbConnectionInternals internals = Db.Connection;
				var connection = internals.ADOConnection;
				if (connection.State == ConnectionState.Closed)
				{
					connection.Open();
				}
				return connection;
			}
		}
	}

	/// <summary>
	/// Transactions are controlled in setup/teardown
	/// </summary>
	[UseSnapshotProtection]
	public sealed class NumberFountainWithTransactionTest : TestCase
	{
		public void TestCacheableFountainGetNext()
		{
			CacheableFountainForTest testFountain = new CacheableFountainForTest();
			AssertEquals("New Key in Fountain", 1, testFountain.GetNext(Connection, Transaction));
			AssertEquals("Get increments value", 2, testFountain.GetNext(Connection, Transaction));
		}

		public void TestCacheableFountainSetNext()
		{
			CacheableFountainForTest testFountain = new CacheableFountainForTest();
			testFountain.SetNext(Connection, Transaction, 250);
			AssertEquals("Same as Set", 250, testFountain.PeekPreliminary(Connection, Transaction));
			AssertEquals("Take the 1st number", 250, testFountain.GetNext(Connection, Transaction));
			AssertEquals("Incremented", 251, testFountain.GetNext(Connection, Transaction));
		}

		public void TestCacheableFountainRollOver()
		{
			// create new numberfountain key "Test1" and set initial value
			CacheableFountainForTest testFountain = new CacheableFountainForTest(rollOver: true);
			long maxSetValue = FountainUtils.MaxNumber - FountainUtils.CacheSize - 1;
			testFountain.SetNext(Connection, Transaction, maxSetValue);

			// Ensure that Value is correct (doesn't increment)
			AssertEquals("Peek", maxSetValue, testFountain.PeekPreliminary(Connection, Transaction));

			for (long i = maxSetValue; i < FountainUtils.MaxNumber; i++)
			{
				// Get next value from the Fountain
				testFountain.GetNextFormatted(Connection, Transaction);
			}

			// Should get the max value
			AssertEquals("GetNext Max", FountainUtils.MaxNumber, testFountain.GetNext(Connection, Transaction));

			// Number should rollover from MaxLong to MinLong
			AssertEquals("Peek()", FountainUtils.MinNumber, testFountain.PeekPreliminary(Connection, Transaction));
		}

		public void TestCacheableFountainSettingOwnerPk()
		{
			Guid ownerPk1 = Guid.NewGuid();
			CacheableFountainForTest testFountain1 = new CacheableFountainForTest(ownerPk1, "name");
			AssertEquals("New Key in Fountain 1", FountainUtils.MinNumber, testFountain1.GetNext(Connection, Transaction));

			// Creates TestFountain2 as a different fountain than TestFountain1
			Guid ownerPk2 = Guid.NewGuid();
			CacheableFountainForTest testFountain2 = new CacheableFountainForTest(ownerPk2, "name");
			AssertEquals("Value in Fountain 2", FountainUtils.MinNumber, testFountain2.PeekPreliminary(Connection, Transaction));
			AssertEquals("Value in Fountain 2", -1, testFountain2.PeekPreliminaryOrDefault(Connection, Transaction, -1));

			// Creates TestFountain3 as the same fountain as TestFountain1
			CacheableFountainForTest testFountain3 = new CacheableFountainForTest(ownerPk1, "name");
			AssertEquals("Value in Fountain 1", 2, testFountain1.PeekPreliminary(Connection, Transaction));
			AssertEquals("Value in Fountain 3", 2, testFountain3.PeekPreliminary(Connection, Transaction));
			AssertEquals("Value in Fountain 1", 2, testFountain1.PeekPreliminaryOrDefault(Connection, Transaction, -1));
			AssertEquals("Value in Fountain 3", 2, testFountain3.PeekPreliminaryOrDefault(Connection, Transaction, -1));
		}

		public void TestGetSetMinAndMaxValues()
		{
			// THIS FILE WILL BE DELETED AFTER NEW FOUNTAIN IMPLEMENTATION!!!!!!
			{
				var testFountain = new CacheableFountainForTest();

				testFountain.GetMinAndMaxValues(Connection, Transaction, out var minValue, out var maxValue);
				AssertEquals("Min Value", FountainUtils.MinNumber, minValue);
				AssertEquals("Max Value", FountainUtils.MaxNumber, maxValue);

				testFountain.SetValues(Connection, Transaction, minValue: 5, nextValue: 0, maxValue: 7);
				testFountain.GetMinAndMaxValues(Connection, Transaction, out minValue, out maxValue);

				AssertEquals("Min Value", 5, minValue);
				AssertEquals("Max Value", 7, maxValue);
			}
		}

		IDbTransaction Transaction
		{
			get
			{
				IDbConnectionInternals internals = Db.Connection;
				return internals.ADOTransaction;
			}
		}

		IDbConnection Connection
		{
			get
			{
				IDbConnectionInternals internals = Db.Connection;
				return internals.ADOConnection;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Db.Connection.BeginTransaction();
		}

		protected override void TearDown()
		{
			Db.Connection.RollbackTransaction();
			base.TearDown();
		}
	}

	[UseSnapshotProtection]
	public class NumberFountainNonTransactionalTest : TestCase
	{
		public void TestCacheableFountainPeekPreliminary()
		{
			CacheableFountainForTest testFountain = new CacheableFountainForTest();
			Assert("TestFountain should NOT exist on DB (1)", !testFountain.NumberFountainExistsOnDb());
			AssertEquals("New Key in Fountain", FountainUtils.MinNumber, testFountain.PeekPreliminary(Connection, null));
			AssertEquals("New Key in Fountain", -1, testFountain.PeekPreliminaryOrDefault(Connection, null, -1));
			AssertEquals("Peek doesn't increment value", FountainUtils.MinNumber, testFountain.PeekPreliminary(Connection, null));
			AssertEquals("Peek doesn't increment value", -1, testFountain.PeekPreliminaryOrDefault(Connection, null, -1));
			Assert("TestFountain should NOT exist on DB (2)", !testFountain.NumberFountainExistsOnDb());
		}

		public void TestPrefix()
		{
			CacheableFountainForTest testFountain1 = new CacheableFountainForTest();
			AssertEquals("Prefix - Fountain1", "", testFountain1.Prefix_Exposed);
		}

		#region Transaction Check Tests

		IDbConnection Connection
		{
			get { return ((IDbConnectionInternals)Db.Connection).ADOConnection; }
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGetNextThrowsExceptionWhenNotInTransaction()
		{
			CacheableFountainForTest testFountain = new CacheableFountainForTest();
			testFountain.GetNextFormatted(null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestSetNextThrowsExceptionWhenNotInTransaction()
		{
			CacheableFountainForTest testFountain = new CacheableFountainForTest();
			testFountain.SetNext(null, 100);
		}

		[ExpectNoExceptions]
		public void TestPeekPreliminaryDoesNotThrowExceptionWhenNotInTransaction()
		{
			CacheableFountainForTest testFountain = new CacheableFountainForTest();
			testFountain.PeekPreliminaryFormatted(((IDbConnectionInternals)Db.Connection).ADOConnection, null);
		}

		#endregion
	}

	#region Mock Classes

	public abstract class NumberFountainForTest : INumberFountain, IFountain
	{
		protected NumberFountainForTest()
			: this(Guid.Empty)
		{
		}

		protected NumberFountainForTest(Guid ownerPk, string name = null)
			: this(ownerPk, FountainUtils.MinNumber, name)
		{
		}

		protected NumberFountainForTest(Guid ownerPk, long minNumber, string name = null)
		{
			_fountain = (NonFormattedNumberFountain)NonFormattedNumberFountainFactory.New(name ?? GetTestKey(), ownerPk, FountainUtils.NoRollOver, minNumber);
		}

		protected NumberFountainForTest(bool rollOver, string name = null)
		{
			_fountain = (NonFormattedNumberFountain)NonFormattedNumberFountainFactory.New(name ?? GetTestKey(), Guid.Empty, rollOver);
		}

		public bool EnsureConsistentSequence => _fountain.EnsureConsistentSequence;

		internal static string GetTestKey()
		{
			return DateTime.Now.ToString("HHmmssffff"); // It's a test! No need to get time from DB
		}

		public bool NumberFountainExistsOnDb()
		{
			var sqlText =
				"SELECT count(*) FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner";
			var count = Convert.ToInt64(Db.Connection.ExecuteScalar(
				sqlText,
			cmd =>
			{
					cmd.AddParameter("@name", SqlDbType.VarChar, 256, Name);
					cmd.AddParameter("@owner", SqlDbType.UniqueIdentifier, OwnerPk);
				}));
			return (count == 1);
		}

		public string Prefix_Exposed => "";

		readonly NonFormattedNumberFountain _fountain;

		#region INumberFountain

		public long GetNext(IFountainContext fountainContext, bool callInSameTransaction = true)
		{
			return _fountain.GetNext(fountainContext, callInSameTransaction);
		}

		public long[] GetNexts(IFountainContext fountainContext, int amount, bool callInSameTransaction = true)
		{
			return _fountain.GetNexts(fountainContext, amount, callInSameTransaction);
		}

		public string GetNextFormatted(IFountainContext fountainContext, bool callInSameTransaction = true)
		{
			return _fountain.GetNextFormatted(fountainContext, callInSameTransaction);
		}

		public string[] GetNextsFormatted(IFountainContext fountainContext, int amount, bool callInSameTransaction = true)
		{
			return _fountain.GetNextsFormatted(fountainContext, amount, callInSameTransaction);
		}

		public void SetNext(IFountainContext fountainContext, long nextValue, bool callInSameTransaction = true)
		{
			_fountain.SetNext(fountainContext, nextValue, callInSameTransaction);
		}

		public void SetValues(IFountainContext fountainContext, long minValue, long nextValue, long maxValue, bool callInSameTransaction = true)
		{
			_fountain.SetValues(fountainContext, minValue, nextValue, maxValue, callInSameTransaction);
		}

		public void GetMinAndMaxValues(IDbConnection connection, IDbTransaction transaction, out long minValue, out long maxValue)
		{
			_fountain.GetMinAndMaxValues(connection, transaction, out minValue, out maxValue);
		}

		public long PeekPreliminary(IDbConnection connection, IDbTransaction transaction)
		{
			return _fountain.PeekPreliminary(connection, transaction);
		}

		public long PeekPreliminaryOrDefault(IDbConnection connection, IDbTransaction transaction, long defaultValue)
		{
			return _fountain.PeekPreliminaryOrDefault(connection, transaction, defaultValue);
		}

		public string PeekPreliminaryFormatted(IDbConnection connection, IDbTransaction transaction)
		{
			return _fountain.PeekPreliminaryFormatted(connection, transaction);
		}

		public void FixFountain(IDbConnection connection, string uniqueIndexViolated)
		{
			_fountain.FixFountain(connection, uniqueIndexViolated);
		}

		public void FixFountain(IDbCommand commandToFindMaxValueInDatabase)
		{
			_fountain.FixFountain(commandToFindMaxValueInDatabase);
		}

		public bool Equals(INumberFountain other) => other.Equals(_fountain);

		#endregion

		#region IFountain

		public string Name => _fountain.Name;
		public Guid OwnerPk => _fountain.OwnerPk;
		public long MinValue => _fountain.MinValue;
		public long MaxValue => _fountain.MaxValue;
		public bool RollOver => _fountain.RollOver;

		#endregion
	}

	class CacheableFountainForTest : NumberFountainForTest
	{
		public CacheableFountainForTest()
			: this(Guid.Empty)
		{
		}

		public CacheableFountainForTest(Guid ownerPk, string name = null)
			: base(ownerPk, name)
		{
		}

		public CacheableFountainForTest(bool rollOver)
			: base(rollOver)
		{
		}

		public long GetMinValue(DbConnection connection)
		{
			var sqlText = $"SELECT SN_MinimumValue FROM dbo.StmNums WHERE SN_Name = '{Name}' AND SN_Owner = '{OwnerPk}'";
			return Convert.ToInt64(connection.ExecuteScalar(sqlText));
		}

		public long GetMaxValue(DbConnection connection)
		{
			var sqlText = $"SELECT SN_MaximumValue FROM dbo.StmNums WHERE SN_Name = '{Name}' AND SN_Owner = '{OwnerPk}'";
			return Convert.ToInt64(connection.ExecuteScalar(sqlText));
		}
	}

	static class EnumerableExtensions
	{
		public static IEnumerable<long> Range(long start, long count)
		{
			for (long i = 0; i < count; i++)
			{
				yield return start + i;
			}
		}
	}

	#endregion
}
