using System;
using CargoWise.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class MultiTransactionManagerTest : TestCase
	{
		public void TestConstructorDisposedOnCriticalException()
		{
			CombineAssertions(() =>
			{
				TestConstructorDisposedOnCriticalException(new OutOfMemoryException());
				TestConstructorDisposedOnCriticalException(new DatabaseUpgradedException());
				TestConstructorDisposedOnCriticalException(new Mock<DatabaseUpgradeException>("NotImportantMessage").Object);
			});

			void TestConstructorDisposedOnCriticalException(Exception mockException)
			{
				// Arrange
				var innerTransaction = new Mock<ITransactionManager>();

				var starter1Mock = new Mock<ITransactionStarter>();
				starter1Mock
					.Setup(x => x.BeginTransactionWithManager())
					.Returns(() => innerTransaction.Object);

				var starter2Mock = new Mock<ITransactionStarter>();
				starter2Mock
					.Setup(x => x.BeginTransactionWithManager())
					.Throws(mockException);

				var transactions = new[] { starter1Mock.Object, starter2Mock.Object };

				// Act
				AssertExceptionThrown(mockException.GetType(), () => new MultiTransactionManager<MultiTransactionManagerTest>(this, transactions));

				// Assert
				starter1Mock.Verify(x => x.BeginTransactionWithManager(), Times.Once);
				starter1Mock.VerifyNoOtherCalls();
				starter2Mock.Verify(x => x.BeginTransactionWithManager(), Times.Once);
				starter2Mock.VerifyNoOtherCalls();
				innerTransaction.Verify(x => x.Dispose(), Times.Once);
				innerTransaction.VerifyNoOtherCalls();
			}
		}

		[ExpectNoExceptions]
		public void TestCommit()
		{
			var innerTransaction1Mock = new Mock<ITransactionManager>();
			innerTransaction1Mock.Setup(x => x.CommitTransaction());
			innerTransaction1Mock.Setup(x => x.Dispose());
			var innerTransaction2Mock = new Mock<ITransactionManager>();
			innerTransaction2Mock.Setup(x => x.CommitTransaction());
			innerTransaction2Mock.Setup(x => x.Dispose());

			var starter1Mock = new Mock<ITransactionStarter>();
			starter1Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction1Mock.Object);
			var starter2Mock = new Mock<ITransactionStarter>();
			starter2Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction2Mock.Object);

			var transactions = new ITransactionStarter[2] { starter1Mock.Object, starter2Mock.Object };

			using (var transactionManager = new MultiTransactionManager<MultiTransactionManagerTest>(this, transactions))
			{
				transactionManager.CommitTransaction();
			}

			innerTransaction1Mock.Verify();
			innerTransaction2Mock.Verify();
			starter1Mock.Verify();
			starter2Mock.Verify();
		}

		public void TestCommitRolledBackOnCriticalException()
		{
			CombineAssertions(() =>
			{
				TestCommitRolledBackOnCriticalException(new OutOfMemoryException());
				TestCommitRolledBackOnCriticalException(new DatabaseUpgradedException());
				TestCommitRolledBackOnCriticalException(new Mock<DatabaseUpgradeException>("NotImportantMessage").Object);
			});

			void TestCommitRolledBackOnCriticalException(Exception mockException)
			{
				// Arrange
				var innerTransaction1 = new Mock<ITransactionManager>();
				var innerTransaction2 = new Mock<ITransactionManager>();
				innerTransaction2
					.Setup(x => x.CommitTransaction())
					.Throws(mockException);

				var starter1Mock = new Mock<ITransactionStarter>();
				starter1Mock
					.Setup(x => x.BeginTransactionWithManager())
					.Returns(innerTransaction1.Object);

				var starter2Mock = new Mock<ITransactionStarter>();
				starter2Mock
					.Setup(x => x.BeginTransactionWithManager())
					.Returns(innerTransaction2.Object);

				var transactions = new[] { starter1Mock.Object, starter2Mock.Object };

				// Act
				using (var transactionManager = new MultiTransactionManager<MultiTransactionManagerTest>(this, transactions))
				{
					AssertExceptionThrown(mockException.GetType(), () => transactionManager.CommitTransaction());
				}

				// Assert
				innerTransaction1.Verify(x => x.CommitTransaction(), Times.Once);
				innerTransaction2.Verify(x => x.CommitTransaction(), Times.Once);
				innerTransaction1.Verify(x => x.RollbackTransaction(), Times.Once);
				innerTransaction2.Verify(x => x.RollbackTransaction(), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestRollback()
		{
			var innerTransaction1Mock = new Mock<ITransactionManager>();
			innerTransaction1Mock.Setup(x => x.RollbackTransaction());
			innerTransaction1Mock.Setup(x => x.Dispose());
			var innerTransaction2Mock = new Mock<ITransactionManager>();
			innerTransaction2Mock.Setup(x => x.RollbackTransaction());
			innerTransaction2Mock.Setup(x => x.Dispose());

			var starter1Mock = new Mock<ITransactionStarter>();
			starter1Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction1Mock.Object);
			var starter2Mock = new Mock<ITransactionStarter>();
			starter2Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction2Mock.Object);

			var transactions = new ITransactionStarter[2] { starter1Mock.Object, starter2Mock.Object };

			using (var transactionManager = new MultiTransactionManager<MultiTransactionManagerTest>(this, transactions))
			{
				transactionManager.RollbackTransaction();
			}

			innerTransaction1Mock.Verify();
			innerTransaction2Mock.Verify();
			starter1Mock.Verify();
			starter2Mock.Verify();
		}

		public void TestRollbackGathersExceptions()
		{
			var exception1 = new NotSupportedException();
			var innerTransaction1Mock = new Mock<ITransactionManager>();
			innerTransaction1Mock.Setup(x => x.RollbackTransaction()).Throws(exception1);
			innerTransaction1Mock.Setup(x => x.Dispose());
			var exception2 = new ArgumentNullException();
			var innerTransaction2Mock = new Mock<ITransactionManager>();
			innerTransaction2Mock.Setup(x => x.RollbackTransaction()).Throws(exception2);
			innerTransaction2Mock.Setup(x => x.Dispose());

			var starter1Mock = new Mock<ITransactionStarter>();
			starter1Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction1Mock.Object);
			var starter2Mock = new Mock<ITransactionStarter>();
			starter2Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction2Mock.Object);

			var transactions = new ITransactionStarter[2] { starter1Mock.Object, starter2Mock.Object };

			try
			{
				using (var transactionManager = new MultiTransactionManager<MultiTransactionManagerTest>(this, transactions))
				{
					transactionManager.RollbackTransaction();
				}
			}
			catch (AggregateException ex)
			{
				var allExceptions = ex.Flatten();
				Assert(allExceptions.InnerExceptions.Contains(exception1));
				Assert(allExceptions.InnerExceptions.Contains(exception2));
			}
			innerTransaction1Mock.Verify();
			innerTransaction2Mock.Verify();
		}

		[ExpectNoExceptions]
		public void TestDispose()
		{
			var innerTransaction1Mock = new Mock<ITransactionManager>();
			innerTransaction1Mock.Setup(x => x.Dispose());
			var innerTransaction2Mock = new Mock<ITransactionManager>();
			innerTransaction2Mock.Setup(x => x.Dispose());

			var starter1Mock = new Mock<ITransactionStarter>();
			starter1Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction1Mock.Object);
			var starter2Mock = new Mock<ITransactionStarter>();
			starter2Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction2Mock.Object);

			var transactions = new ITransactionStarter[2] { starter1Mock.Object, starter2Mock.Object };

			using (var transactionManager = new MultiTransactionManager<MultiTransactionManagerTest>(this, transactions))
			{ }

			innerTransaction1Mock.Verify();
			innerTransaction2Mock.Verify();
		}

		public void TestDisposeGathersExceptions()
		{
			var disposeException1 = new NotSupportedException();
			var innerTransaction1Mock = new Mock<ITransactionManager>();
			innerTransaction1Mock.Setup(x => x.Dispose()).Throws(disposeException1);
			var disposeException2 = new ArgumentNullException();
			var innerTransaction2Mock = new Mock<ITransactionManager>();
			innerTransaction2Mock.Setup(x => x.Dispose()).Throws(disposeException2);

			var starter1Mock = new Mock<ITransactionStarter>();
			starter1Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction1Mock.Object);
			var starter2Mock = new Mock<ITransactionStarter>();
			starter2Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction2Mock.Object);

			var transactions = new ITransactionStarter[2] { starter1Mock.Object, starter2Mock.Object };

			try
			{
				using (var transactionManager = new MultiTransactionManager<MultiTransactionManagerTest>(this, transactions))
				{ }
			}
			catch (AggregateException ex)
			{
				var allExceptions = ex.Flatten();
				Assert(allExceptions.InnerExceptions.Contains(disposeException1));
				Assert(allExceptions.InnerExceptions.Contains(disposeException2));
			}
			innerTransaction1Mock.Verify();
			innerTransaction2Mock.Verify();
		}

		[ExpectNoExceptions]
		public void TestThrowDuringBeginDisposesTransactionsThatBegun()
		{
			var innerTransaction1Mock = new Mock<ITransactionManager>();
			innerTransaction1Mock.Setup(x => x.Dispose());

			var starter1Mock = new Mock<ITransactionStarter>();
			starter1Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction1Mock.Object);
			var starter2Mock = new Mock<ITransactionStarter>();
			starter2Mock.Setup(x => x.BeginTransactionWithManager()).Throws(new InvalidOperationException());

			var transactions = new[] { starter1Mock.Object, starter2Mock.Object };

			try
			{
				new MultiTransactionManager<MultiTransactionManagerTest>(this, transactions);
				Fail("BeginTransactionWithManager should have thrown.");
			}
			catch (InvalidOperationException)
			{
			}

			innerTransaction1Mock.Verify();
			starter1Mock.Verify();
			starter2Mock.Verify();
		}

		public void TestThrowDuringBeginAndThrowInDisposeGathersExeceptions()
		{
			var disposeException = new NotSupportedException();
			var innerTransaction1Mock = new Mock<ITransactionManager>();
			innerTransaction1Mock.Setup(x => x.Dispose()).Throws(disposeException);

			var beginException = new InvalidOperationException();
			var starter1Mock = new Mock<ITransactionStarter>();
			starter1Mock.Setup(x => x.BeginTransactionWithManager()).Returns(innerTransaction1Mock.Object);
			var starter2Mock = new Mock<ITransactionStarter>();
			starter2Mock.Setup(x => x.BeginTransactionWithManager()).Throws(beginException);

			var transactions = new ITransactionStarter[2] { starter1Mock.Object, starter2Mock.Object };

			try
			{
				new MultiTransactionManager<MultiTransactionManagerTest>(this, transactions);
				Fail("BeginTransactionWithManager should have thrown.");
			}
			catch (AggregateException ex)
			{
				var allExceptions = ex.Flatten();
				Assert(allExceptions.InnerExceptions.Contains(disposeException));
				Assert(allExceptions.InnerExceptions.Contains(beginException));
			}

			innerTransaction1Mock.Verify();
			starter1Mock.Verify();
			starter2Mock.Verify();
		}
	}
}
