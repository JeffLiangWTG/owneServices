using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class BaseTransactionManagerTest : TestCase
	{
		public void TestRollbackOnDisposeIfNotCommitted()
		{
			bool rolledBack = false;
			using (new TransactionManagerWithExposedOwner<BaseTransactionManagerTest>(this, () =>
			{
				rolledBack = true;
			}))
			{ }

			AssertEquals(true, rolledBack);
		}

		public void TestNoRollbackOnDisposeIfCommitted()
		{
			bool rolledBack = false;
			using (var transactionManager = new TransactionManagerWithExposedOwner<BaseTransactionManagerTest>(this, () =>
			{
				rolledBack = true;
			}))
			{
				transactionManager.CommitTransaction();
			}

			AssertEquals(false, rolledBack);
		}

		public void TestNoRollbackOnDisposeIfRolledBack()
		{
			int rollBackCount = 0;
			using (var transactionManager = new TransactionManagerWithExposedOwner<BaseTransactionManagerTest>(this, () =>
			{
				++rollBackCount;
			}))
			{
				transactionManager.RollbackTransaction();
			}

			AssertEquals(1, rollBackCount);
		}

		public void TestRollbackActionAndRollbackThrow()
		{
			var rollbackException = new NotSupportedException();
			var rollbackActionException = new ArgumentNullException();
			try
			{
				using (var transactionManager = new TransactionManagerWithExposedOwner<BaseTransactionManagerTest>(this, () => throw rollbackActionException))
				{
					transactionManager.RollbackException = rollbackException;
					transactionManager.RollbackTransaction();
				}
				Fail("Rollback did not throw");
			}
			catch (AggregateException ex)
			{
				var allExceptions = ex.Flatten();
				Assert(allExceptions.InnerExceptions.Contains(rollbackException));
				Assert(allExceptions.InnerExceptions.Contains(rollbackActionException));
			}
		}

		[ExpectNoExceptions]
		public void TestRollbackOnDisposeIfNotCommittedNullRollbackAction()
		{
			using (new TransactionManagerWithExposedOwner<BaseTransactionManagerTest>(this, null))
			{ }
		}

		[ExpectNoExceptions]
		public void TestRollbackTwice()
		{
			using (var transactionManager = new TransactionManagerWithExposedOwner<BaseTransactionManagerTest>(this, null))
			{
				transactionManager.RollbackTransaction();
				transactionManager.RollbackException = new InvalidOperationException();
				transactionManager.RollbackTransaction();
			}
		}

		[ExpectNoExceptions]
		public void TestRollbackAfterCommit()
		{
			using (var transactionManager = new TransactionManagerWithExposedOwner<BaseTransactionManagerTest>(this, null))
			{
				transactionManager.CommitTransaction();
				transactionManager.RollbackException = new InvalidOperationException();
				transactionManager.RollbackTransaction();
			}
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestCommitTwice()
		{
			using (var transactionManager = new TransactionManagerWithExposedOwner<BaseTransactionManagerTest>(this, null))
			{
				transactionManager.CommitTransaction();
				transactionManager.CommitTransaction();
			}
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestCommitAfterRollback()
		{
			using (var transactionManager = new TransactionManagerWithExposedOwner<BaseTransactionManagerTest>(this, null))
			{
				transactionManager.RollbackTransaction();
				transactionManager.CommitTransaction();
			}
		}

		public void TestOwner()
		{
			using (var transactionManager = new TransactionManagerWithExposedOwner<BaseTransactionManagerTest>(this))
			{
				AssertEquals(this, transactionManager.Owner);
			}
		}

		class TransactionManagerWithExposedOwner<T> : BaseTransactionManager<T>
		{
			public Exception RollbackException { get; set; }

			public T Owner { get { return owner; } }

			public TransactionManagerWithExposedOwner(T owner, Action rollbackAction = null) : base(owner, rollbackAction)
			{
			}

			protected override void Commit() { }

			protected override void Rollback()
			{
				if (RollbackException != null)
				{
					throw RollbackException;
				}
			}
		}
	}
}
