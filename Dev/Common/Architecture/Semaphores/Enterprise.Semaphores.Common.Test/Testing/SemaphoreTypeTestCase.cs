using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Semaphores.Common.Testing
{
	[TestsSubclassesOf(typeof(ISemaphoreType))]
	public abstract class SemaphoreTypeTestCase : TransactionedTestCase
	{
		public void TestLockInfoMaxLength()
		{
			Assert("Length of semaphore lock info should not be greater than max", TestSemaphore.LockInfo.Length <= StmServiceSemaphoreSchema.SS_LockInfo.MaxLength);
		}

		public void TestCategoryMaxLength()
		{
			Assert("Length of semaphore category should not be greater than max", TestSemaphore.Category.Length <= StmServiceSemaphoreSchema.SS_ServiceClass.MaxLength);
		}

		public void TestValidMaxConcurrentHandles()
		{
			Assert("Max concurrent handles should be >= 0", TestSemaphore.MaxConcurrentHandles >= 0);
		}

		protected override DbConnection TestConnection
		{
			get { return Db.Connection; }
		}

		protected virtual ISemaphoreType TestSemaphore
		{
			get
			{
				if (testSemaphore == null)
				{
					testSemaphore = (ISemaphoreType)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()));
				}
				return testSemaphore;
			}
		}

		ISemaphoreType testSemaphore;
	}
}
