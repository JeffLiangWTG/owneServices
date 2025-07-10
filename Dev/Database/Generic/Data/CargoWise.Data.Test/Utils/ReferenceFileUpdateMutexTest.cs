using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class ReferenceFileUpdateMutexTest : TransactionedTestCase
	{
		public void TestAcquireUpdateLockAndCanAquireLock()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				connection1.BeginTransaction();
				var usReferenceDbName = ((IPhysicalRefDbLocation)connection1).GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "US");
				var caReferenceDbName = ((IPhysicalRefDbLocation)connection1).GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "CA");
				AssertEquals("Should have locked", true, ReferenceFileUpdateMutex.AcquireUpdateLock(connection1, TimeSpan.Zero, usReferenceDbName, "HELLOWORLD"));
				using (var connection2 = Db.NewExtraConnectionToMainDb())
				{
					connection2.BeginTransaction();
					AssertEquals("Cannot Aquire Lock on different connection", false, ReferenceFileUpdateMutex.CanAquireLock(connection2, usReferenceDbName, "HELLOWORLD"));
					AssertEquals("Can Aquire Lock on different connection for different database", true, ReferenceFileUpdateMutex.CanAquireLock(connection2, caReferenceDbName, "HELLOWORLD"));
				}
				AssertEquals("Can Aquire Lock on same connection", true, ReferenceFileUpdateMutex.CanAquireLock(connection1, usReferenceDbName, "HELLOWORLD"));
			}
			using (var connection3 = Db.NewExtraConnectionToMainDb())
			{
				var usReferenceDbName = ((IPhysicalRefDbLocation)connection3).GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "US");
				connection3.BeginTransaction();
				AssertEquals("Can Aquire Lock on different connection when lock has been released", true, ReferenceFileUpdateMutex.CanAquireLock(connection3, usReferenceDbName, "HELLOWORLD"));
			}
		}

		public void TestAcquireUpdateLockAndCanAquireLockUsingRefDbTypeEnum()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				connection1.BeginTransaction();
				AssertEquals("Should have locked", true, ReferenceFileUpdateMutex.AcquireUpdateLock(connection1, TimeSpan.Zero, RefDbTypeEnum.Enterprise, "US", "HELLOWORLD"));
				using (var connection2 = Db.NewExtraConnectionToMainDb())
				{
					connection2.BeginTransaction();
					AssertEquals("Cannot Aquire Lock on different connection", false, ReferenceFileUpdateMutex.CanAquireLock(connection2, RefDbTypeEnum.Enterprise, "US", "HELLOWORLD"));
					AssertEquals("Can Aquire Lock on different connection for different database", true, ReferenceFileUpdateMutex.CanAquireLock(connection2, RefDbTypeEnum.Enterprise, "CA", "HELLOWORLD"));
				}
				AssertEquals("Can Aquire Lock on same connection", true, ReferenceFileUpdateMutex.CanAquireLock(connection1, RefDbTypeEnum.Enterprise, "US", "HELLOWORLD"));
			}
			using (var connection3 = Db.NewExtraConnectionToMainDb())
			{
				connection3.BeginTransaction();
				AssertEquals("Can Aquire Lock on different connection when lock has been released", true, ReferenceFileUpdateMutex.CanAquireLock(connection3, RefDbTypeEnum.Enterprise, "US", "HELLOWORLD"));
			}
		}
	}
}
