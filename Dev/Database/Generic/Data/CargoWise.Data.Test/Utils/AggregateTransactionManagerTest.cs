using CargoWise.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class AggregateTransactionManagerTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCommit()
		{
			var innerTransactionMock = new Mock<ITransactionManager>();
			innerTransactionMock.Setup(x => x.CommitTransaction()).Verifiable();
			innerTransactionMock.Setup(x => x.Dispose()).Verifiable();

			using (var transactionManager = new AggregateTransactionManager<AggregateTransactionManagerTest>(this, innerTransactionMock.Object))
			{
				transactionManager.CommitTransaction();
			}

			innerTransactionMock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestRollback()
		{
			var innerTransactionMock = new Mock<ITransactionManager>();
			innerTransactionMock.Setup(x => x.RollbackTransaction()).Verifiable();
			innerTransactionMock.Setup(x => x.Dispose()).Verifiable();

			using (var transactionManager = new AggregateTransactionManager<AggregateTransactionManagerTest>(this, innerTransactionMock.Object))
			{
				transactionManager.RollbackTransaction();
			}

			innerTransactionMock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestDispose()
		{
			var innerTransactionMock = new Mock<ITransactionManager>();
			innerTransactionMock.Setup(x => x.Dispose()).Verifiable();

			using (var transactionManager = new AggregateTransactionManager<AggregateTransactionManagerTest>(this, innerTransactionMock.Object))
			{ }

			innerTransactionMock.VerifyAll();
		}
	}
}
