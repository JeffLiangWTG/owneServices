using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Integration;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SaveInTransactionActionTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestSaveInTransactionActionWithFactory_NullFactory()
		{
			var mock = new Mock<SaveInTransactionActionWithFactory>(new object[] { null });
			object result = mock.Object;
		}

		public void TestSaveInTransactionActionWithFactory()
		{
			using (DbConnection connection = Db.NewExtraConnectionToMainDb())
			{
				BusinessObjectFactory factory = new BusinessObjectFactory(connection);
				var mock = new Mock<SaveInTransactionActionWithFactory>(new object[] { factory });
				ITransactionParticipant action = mock.Object;
				AssertNotNull(action);

				using (var transactionManager = action.BeginTransactionWithManager())
				{
					Assert(connection.IsInTransactionOtherThanTransactionedTestCase);

					transactionManager.CommitTransaction();
					Assert(!connection.IsInTransactionOtherThanTransactionedTestCase);
				}

				using (var transactionManager = action.BeginTransactionWithManager())
				{
					Assert(connection.IsInTransactionOtherThanTransactionedTestCase);
				}
				Assert(!connection.IsInTransactionOtherThanTransactionedTestCase);
			}
		}

		public void TestSaveInTransactionActionWithMainConnection_NoErrorIfWeb()
		{
			ErrorReporter.Clear();
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.IsWeb = true;

				var mock = new Mock<SaveInTransactionActionWithMainConnection>();
				ITransactionParticipant action = mock.Object;
				AssertNotNull(action);

				Assert(ErrorReporter.LastMessageReported.IsNullOrEmpty());
				ErrorReporter.Clear();
			}
		}

		public void TestSaveInTransactionActionWithMainConnection()
		{
			var mock = new Mock<SaveInTransactionActionWithMainConnection>();
			ITransactionParticipant action = mock.Object;
			AssertNotNull(action);

			int transactionCount = Db.Connection.AppTransactionCount;
			using (var transactionManager = action.BeginTransactionWithManager())
			{
				AssertEquals(transactionCount + 1, Db.Connection.AppTransactionCount);
				Assert(action.IsInTransaction);

				transactionManager.CommitTransaction();
				AssertEquals(transactionCount, Db.Connection.AppTransactionCount);
				Assert(!action.IsInTransaction);
			}

			using (var transactionManager = action.BeginTransactionWithManager())
			{
				AssertEquals(transactionCount + 1, Db.Connection.AppTransactionCount);
				Assert(action.IsInTransaction);
			}
			AssertEquals(transactionCount, Db.Connection.AppTransactionCount);
			Assert(!action.IsInTransaction);
		}

		public void TestSaveInTransactionDelegateAction()
		{
			bool delegateInvoked = false;
			SaveInTransactionDelegateAction action = new SaveInTransactionDelegateAction(Factory, delegate
			{ delegateInvoked = true; return ChangedTableNames.Empty; });
			((ITransactionParticipant)action).SaveInTransaction();
			AssertEquals("Delegate should be invoked", true, delegateInvoked);
		}

		public void TestSaveInTransactionAction()
		{
			var mock = new Mock<SaveInTransactionAction>();
			ITransactionParticipant action = mock.Object;
			AssertNotNull(action);

			mock.Protected()
				.Setup<ITransactionManager>("BeginTransactionWithManager")
				.Returns(action.BeginTransactionWithManager());
			mock.VerifyAll();

			mock.Protected().Setup<bool>("IsInTransaction").Returns(true);
			Assert(action.IsInTransaction);
			mock.VerifyAll();

			mock.Protected().Setup<bool>("IsInTransaction").Returns(false);
			Assert(!action.IsInTransaction);
			mock.VerifyAll();

			mock.Protected().Setup("OnAllTransactionsBeginning");
			action.OnAllTransactionsBeginning();
			mock.VerifyAll();

			mock.Protected().Setup("OnAllTransactionsCommitted", ItExpr.IsAny<IChangedTableNames>());
			action.OnAllTransactionsCommitted(ChangedTableNames.Empty);
			mock.VerifyAll();

			mock.Protected().Setup("OnAllTransactionsRolledBack");
			action.OnAllTransactionsRolledBack();
			mock.VerifyAll();

			mock.Protected().Setup<IChangedTableNames>("SaveInTransaction").Returns<IChangedTableNames>(null);
			action.SaveInTransaction();
			mock.VerifyAll();
		}
	}
}
