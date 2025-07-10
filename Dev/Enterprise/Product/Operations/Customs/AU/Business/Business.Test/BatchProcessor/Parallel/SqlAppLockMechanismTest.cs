using System;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SqlAppLockMechanismTest : TestCaseWithFactory
	{
		string ConstructMessage(string message, string paramName)
		{
#if NETFRAMEWORK
			return $"{message}{System.Environment.NewLine}Parameter name: {paramName}";
#else
			return $"{message} (Parameter '{paramName}')";
#endif
		}

		public void TestPrefixConstraint()
		{
			_ = new SqlAppLockMechanism(new string('X', LockMechanismConstants.MaxPrefixLength));

			AssertEquals
			(
				ConstructMessage("Lock key prefix cannot be longer than 32 characters.", "prefix"),
				AssertExceptionThrown<ArgumentException>(() => new SqlAppLockMechanism(new string('X', LockMechanismConstants.MaxPrefixLength + 1))).Message
			);

			AssertEquals
			(
				ConstructMessage("Lock key prefix cannot be null, empty or white-space only.", "prefix"),
				AssertExceptionThrown<ArgumentException>(() => new SqlAppLockMechanism(null)).Message
			);

			AssertEquals
			(
				ConstructMessage("Lock key prefix cannot be null, empty or white-space only.", "prefix"),
				AssertExceptionThrown<ArgumentException>(() => new SqlAppLockMechanism("")).Message
			);

			AssertEquals
			(
				ConstructMessage("Lock key prefix cannot be null, empty or white-space only.", "prefix"),
				AssertExceptionThrown<ArgumentException>(() => new SqlAppLockMechanism(" \t\r\n ")).Message
			);
		}

		public void TestSupportConstraints()
		{
			var longPrefix = new string('P', LockMechanismConstants.MaxPrefixLength);
			var shortPrefix = new string('P', LockMechanismConstants.MaxPrefixLength - 1);
			var longKey = new string('K', LockMechanismConstants.MaxKeyLength);
			var shortKey = new string('K', LockMechanismConstants.MaxKeyLength - 1);

			var lm = new SqlAppLockMechanism(longPrefix);

			IDisposable l1 = null;
			SqlApplicationLock l2 = null;
			SqlApplicationLock l3 = null;
			SqlApplicationLock l4 = null;
			try
			{
				AssertEquals(true, lm.TryGetLock(longKey, out l1));
				AssertNotNull(l1);

				using (var conn = Db.NewExtraConnectionToMainDb())
				{
					AssertEquals("long key should be locked", false, conn.TryGetLock(longPrefix + longKey, out l2));
					AssertEquals("long key should not be truncated and confused with short key", true, conn.TryGetLock(longPrefix + shortKey, out l3));
					AssertEquals("long key should not be truncated and confused with short key", true, conn.TryGetLock(shortPrefix + longKey, out l4));
				}
			}
			finally
			{
				l1?.Dispose();
				l2?.Dispose();
				l3?.Dispose();
				l4?.Dispose();
			}
		}

		public void TestCorrectKeysAreLocked()
		{
			var lockMechanism = new SqlAppLockMechanism("TEST_KEY_");

			IDisposable key1Lock = null;
			IDisposable key2Lock = null;
			IDisposable key3Lock = null;
			SqlApplicationLock appLock1 = null;
			SqlApplicationLock appLock2 = null;

			try
			{
				AssertEquals(true, lockMechanism.TryGetLock("001", out key1Lock));
				AssertNotNull(key1Lock);
				AssertEquals(true, lockMechanism.TryGetLock("002", out key2Lock));
				AssertNotNull(key2Lock);

				key1Lock.Dispose();

				using (var conn = Db.NewExtraConnectionToMainDb())
				{
					AssertEquals(true, conn.TryGetLock("TEST_KEY_001", out appLock1));
					AssertEquals(false, conn.TryGetLock("TEST_KEY_002", out appLock2));

					key2Lock.Dispose();

					AssertEquals(true, conn.TryGetLock("TEST_KEY_002", out appLock2));

					AssertEquals(false, lockMechanism.TryGetLock("001", out key1Lock));
					AssertNull(key1Lock);
					AssertEquals(false, lockMechanism.TryGetLock("002", out key2Lock));
					AssertNull(key2Lock);
					AssertEquals(true, lockMechanism.TryGetLock("003", out key3Lock));
					AssertNotNull(key3Lock);

					appLock2.Dispose();

					AssertEquals(false, lockMechanism.TryGetLock("001", out key1Lock));
					AssertNull(key1Lock);
					AssertEquals(true, lockMechanism.TryGetLock("002", out key2Lock));
					AssertNotNull(key2Lock);
				}

				AssertEquals(true, lockMechanism.TryGetLock("001", out key1Lock));
				AssertNotNull(key1Lock);
			}
			finally
			{
				key1Lock?.Dispose();
				key2Lock?.Dispose();
				key3Lock?.Dispose();
				appLock1?.Dispose();
				appLock2?.Dispose();
			}
		}

		public void TestLockTwice()
		{
			var lm = new SqlAppLockMechanism("TEST_KEY_");

			IDisposable l1 = null;
			IDisposable l2 = null;
			SqlApplicationLock l3 = null;
			SqlApplicationLock l4 = null;
			SqlApplicationLock l5 = null;

			try
			{
				AssertEquals(true, lm.TryGetLock("ABC", out l1));
				AssertNotNull(l1);
				AssertEquals(true, lm.TryGetLock("ABC", out l2));
				AssertNotNull(l2);

				using (var c2 = Db.NewExtraConnectionToMainDb())
				{
					AssertEquals(false, c2.TryGetLock("TEST_KEY_ABC", out l3));

					l1.Dispose();
					AssertEquals(false, c2.TryGetLock("TEST_KEY_ABC", out l4));

					l2.Dispose();
					AssertEquals(true, c2.TryGetLock("TEST_KEY_ABC", out l5));
				}
			}
			finally
			{
				l1?.Dispose();
				l2?.Dispose();
				l3?.Dispose();
				l4?.Dispose();
				l5?.Dispose();
			}
		}
	}
}
