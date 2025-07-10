using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class TransactionCoordinatorTest : TransactionCoordinatorTestCase
	{
		public void TestConstructor_ThrowIfParticipantAlreadyInTransactionAndMoreThan1Participant()
		{
			var mockParticipant1 = NewMockTransactionParticipant();
			var mockParticipant2 = NewMockTransactionParticipant();

			mockParticipant1.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(false);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant1.Object, mockParticipant2.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			Verify(mockParticipant1, mockParticipant2);

			mockParticipant1.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(true);
			mockParticipant1.Setup(m => m.AllowTransactionWithOtherParticipant).Returns(false);
			mockParticipant2.Setup(m => m.AllowTransactionWithOtherParticipant).Returns(false);
			mockParticipant1.Setup(m => m.ChildParticipants);
			mockParticipant2.Setup(m => m.ChildParticipants);

			try
			{
				coordinator = new TransactionCoordinator(participants);
				Fail("Did not throw InvalidOperationException");
			}
			catch (InvalidOperationException)
			{
			}

			Verify(mockParticipant1, mockParticipant2);
		}

		public void TestConstructor_ThrowIfParticipantAlreadyInTransactionAndMoreThan1Participant_TestExceptionMessageFormat()
		{
			var mockParticipant1 = NewMockTransactionParticipant();
			var mockParticipant2 = NewMockTransactionParticipant();
			var mockParticipant3 = NewMockTransactionParticipant();

			mockParticipant1.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(true);
			mockParticipant3.Setup(m => m.IsInTransaction).Returns(true);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant1.Object, mockParticipant2.Object, mockParticipant3.Object };

			try
			{
				var coordinator = new TransactionCoordinator(participants);
				Fail("Did not throw InvalidOperationException");
			}
			catch (InvalidOperationException e)
			{
				Assert("Count of TransactionParticipants that are already in transaction should be correctly reported.",
					e.Message.Contains("There were a total of 3 transaction participants. 2 of them already participating in another transaction"));
				Assert("Types of participants that are already in transaction should be correctly reported.",
					e.Message.Contains("types: Castle.Proxies.ITransactionParticipantProxy, Castle.Proxies.ITransactionParticipantProxy"));
			}

			Verify(mockParticipant1, mockParticipant2, mockParticipant3);
		}

		public void TestConstructor_AllowMultipleParticipants()
		{
			var mockParticipant1 = new Mock<ITransactionParticipant>();
			mockParticipant1.Setup(x => x.IsInTransaction).Returns(false);
			mockParticipant1.Setup(x => x.AllowTransactionWithOtherParticipant).Returns(false);

			var mockParticipant2 = new Mock<ITransactionParticipant>();
			mockParticipant2.Setup(x => x.IsInTransaction).Returns(false);
			mockParticipant2.Setup(x => x.AllowTransactionWithOtherParticipant).Returns(true);

			var mockParticipant3 = new Mock<ITransactionParticipant>();
			mockParticipant3.Setup(x => x.ChildParticipants);
			mockParticipant3.Setup(x => x.AllowTransactionWithOtherParticipant).Returns(false);

			var participants = new[] { mockParticipant1.Object, mockParticipant2.Object, mockParticipant3.Object };

			AssertNoExceptionThrown(() => new TransactionCoordinator(participants));
		}

		[ExpectNoExceptions]
		public void TestConstructor_DontThrowIfOnly1ParticipantAndAlreadyInTransaction()
		{
			var mockParticipant = NewMockTransactionParticipant();
			mockParticipant.Setup(m => m.IsInTransaction).Returns(true);
			mockParticipant.Setup(m => m.AllowTransactionWithOtherParticipant).Returns(false);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor_NullParticipantArray()
		{
			new TransactionCoordinator(null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConstructor_NullParticipantInArray()
		{
			var mockParticipant = NewMockTransactionParticipant();
			mockParticipant.Setup(m => m.IsInTransaction).Returns(false);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant.Object, null };
			new TransactionCoordinator(participants);
		}

		public void TestBeginCommit()
		{
			var mockParticipant1 = NewMockTransactionParticipant();
			var mockParticipant2 = NewMockTransactionParticipant();

			mockParticipant1.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(false);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant1.Object, mockParticipant2.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			var mockManagers = new List<Mock<ITransactionManager>>(2);

			mockParticipant1.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock);
			mockParticipant1.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock.Object);

			mockParticipant2.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock2 = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock2);
			mockParticipant2.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock2.Object);

			using (var transactionManager = coordinator.BeginTransactionWithManager())
			{
				Verify(mockParticipant1, mockParticipant2);

				foreach (var mock in mockManagers)
				{
					mock.Setup(m => m.CommitTransaction());
				}

				mockParticipant1.Setup(m => m.OnAllTransactionsCommitted(It.IsAny<IChangedTableNames>()));
				mockParticipant2.Setup(m => m.OnAllTransactionsCommitted(It.IsAny<IChangedTableNames>()));

				transactionManager.CommitTransaction(SaveInTransactionResult.Empty);
				Verify(mockParticipant1, mockParticipant2);
				Verify(mockManagers.ToArray());
			}
		}

		public void TestBeginRollback()
		{
			var mockParticipant1 = NewMockTransactionParticipant();
			var mockParticipant2 = NewMockTransactionParticipant();

			mockParticipant1.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(false);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant1.Object, mockParticipant2.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			var mockManagers = new List<Mock<ITransactionManager>>(2);

			mockParticipant1.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock);
			mockParticipant1.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock.Object);

			mockParticipant2.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock2 = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock2);
			mockParticipant2.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock2.Object);

			using (var transactionManager = coordinator.BeginTransactionWithManager())
			{
				Verify(mockParticipant1, mockParticipant2);

				AddOnRollbackExpectations(mockParticipant1, mockParticipant2);
				AddDisposeExpectations(mockManagers.ToArray());
				transactionManager.RollbackTransaction();
			}

			Verify(mockParticipant1, mockParticipant2);
			Verify(mockManagers.ToArray());
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestCommitTwice()
		{
			var mockParticipant = NewMockTransactionParticipant();
			var mockParticipant2 = NewMockTransactionParticipant();
			mockParticipant.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(false);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant.Object, mockParticipant2.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			var mockManagers = new List<Mock<ITransactionManager>>(2);

			mockParticipant.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock);
			mockParticipant.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock.Object);

			mockParticipant2.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock2 = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock2);
			mockParticipant2.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock2.Object);

			using (var transactionManager = coordinator.BeginTransactionWithManager())
			{
				Verify(mockParticipant);

				foreach (var mock in mockManagers)
				{
					mock.Setup(m => m.CommitTransaction());
				}

				mockParticipant.Setup(m => m.OnAllTransactionsCommitted(It.IsAny<IChangedTableNames>()));
				mockParticipant2.Setup(m => m.OnAllTransactionsCommitted(It.IsAny<IChangedTableNames>()));

				transactionManager.CommitTransaction(SaveInTransactionResult.Empty);
				Verify(mockParticipant);
				Verify(mockManagers.ToArray());

				transactionManager.CommitTransaction(SaveInTransactionResult.Empty);
			}
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestCommitAfterRollback()
		{
			var mockParticipant = NewMockTransactionParticipant();
			var mockParticipant2 = NewMockTransactionParticipant();
			mockParticipant.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(false);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant.Object, mockParticipant2.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			var mockManagers = new List<Mock<ITransactionManager>>(2);

			mockParticipant.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock);
			mockParticipant.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock.Object);

			mockParticipant2.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock2 = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock2);
			mockParticipant2.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock2.Object);

			using (var transactionManager = coordinator.BeginTransactionWithManager())
			{
				Verify(mockParticipant);

				AddOnRollbackExpectations(mockParticipant, mockParticipant2);
				AddDisposeExpectations(mockManagers.ToArray());
				transactionManager.RollbackTransaction();

				transactionManager.CommitTransaction(SaveInTransactionResult.Empty);
			}

			Verify(mockParticipant);
			Verify(mockManagers.ToArray());
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestBeginTwice()
		{
			var mockParticipant = NewMockTransactionParticipant();
			mockParticipant.Setup(m => m.IsInTransaction).Returns(false);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant.Object, NewMockTransactionParticipantObjectBegunButNeverInTransaction() };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			mockParticipant.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock = new Mock<ITransactionManager>();
			mockParticipant.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock.Object);
			AddOnRollbackExpectations(mockParticipant);
			try
			{
				using (var transactionManager = coordinator.BeginTransactionWithManager())
				{
					using (var transactionManager2 = coordinator.BeginTransactionWithManager())
					{ }
				}
			}
			finally
			{
				Verify(mockParticipant);
			}
		}

		[ExpectNoExceptions]
		public void TestRollbackTwice()
		{
			var mockParticipant = NewMockTransactionParticipant();
			mockParticipant.Setup(m => m.IsInTransaction).Returns(false);
			var mockParticipant2 = NewMockTransactionParticipant();
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(false);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant.Object, mockParticipant2.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			var mockManagers = new List<Mock<ITransactionManager>>(2);

			mockParticipant.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock);
			mockParticipant.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock.Object);

			mockParticipant2.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock2 = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock2);
			mockParticipant2.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock2.Object);

			using (var transactionManager = coordinator.BeginTransactionWithManager())
			{
				Verify(mockParticipant);

				AddOnRollbackExpectations(mockParticipant, mockParticipant2);
				AddDisposeExpectations(mockManagers.ToArray());
				transactionManager.RollbackTransaction();

				transactionManager.RollbackTransaction();
			}

			Verify(mockParticipant);
			Verify(mockManagers.ToArray());
		}

		[ExpectNoExceptions]
		public void TestRollbackAfterCommit()
		{
			var mockParticipant = NewMockTransactionParticipant();
			mockParticipant.Setup(m => m.IsInTransaction).Returns(false);
			var mockParticipant2 = NewMockTransactionParticipant();
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(false);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant.Object, mockParticipant2.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			var mockManagers = new List<Mock<ITransactionManager>>(2);

			mockParticipant.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock);
			mockParticipant.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock.Object);

			mockParticipant2.Setup(m => m.OnAllTransactionsBeginning());
			var managerMock2 = new Mock<ITransactionManager>();
			mockManagers.Add(managerMock2);
			mockParticipant2.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock2.Object);

			using (var transactionManager = coordinator.BeginTransactionWithManager())
			{
				Verify(mockParticipant);

				foreach (var mock in mockManagers)
				{
					mock.Setup(m => m.CommitTransaction());
				}

				mockParticipant.Setup(m => m.OnAllTransactionsCommitted(It.IsAny<IChangedTableNames>()));
				mockParticipant2.Setup(m => m.OnAllTransactionsCommitted(It.IsAny<IChangedTableNames>()));

				transactionManager.CommitTransaction(SaveInTransactionResult.Empty);
				Verify(mockParticipant);
				Verify(mockManagers.ToArray());

				transactionManager.RollbackTransaction();
			}
		}

		public void TestSaveInTransactions()
		{
			var mockParticipant1 = NewMockTransactionParticipant();
			var mockParticipant2 = NewMockTransactionParticipant();
			var mockChildParticipant = NewMockTransactionParticipant();

			mockParticipant1.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(false);
			mockChildParticipant.Setup(m => m.IsInTransaction).Returns(false);
			SetChildParticipants(mockParticipant1, new Mock<ITransactionParticipant>[] { mockChildParticipant });

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant1.Object, mockParticipant2.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			var mockManagers = new List<Mock<ITransactionParticipant>>()
			{
				mockParticipant1,
				mockParticipant2,
				mockChildParticipant
			};

			foreach (var mockParticipant in mockManagers)
			{
				mockParticipant.Setup(m => m.OnAllTransactionsBeginning());
				var managerMock = new Mock<ITransactionManager>();
				mockParticipant.Setup(m => m.BeginTransactionWithManager()).Returns(managerMock.Object);
			}

			AddOnRollbackExpectations(mockParticipant1, mockParticipant2, mockChildParticipant);
			using (var transactionManager = coordinator.BeginTransactionWithManager())
			{
				mockParticipant1.Setup(m => m.SaveInTransaction()).Returns(ChangedTableNames.Empty);
				mockParticipant2.Setup(m => m.SaveInTransaction()).Returns(ChangedTableNames.Empty);
				mockChildParticipant.Setup(m => m.SaveInTransaction()).Returns(ChangedTableNames.Empty);

				coordinator.SaveInTransactions();

				ITransactionParticipant[] savedParticipants = coordinator.SavedParticipants;
				AssertEquals(3, savedParticipants.Length);
				AssertEquals(mockParticipant1.Object, savedParticipants[0]);
				AssertEquals(mockChildParticipant.Object, savedParticipants[1]);
				AssertEquals(mockParticipant2.Object, savedParticipants[2]);
			}

			Verify(mockParticipant1, mockParticipant2, mockChildParticipant);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestSaveWithNoBegin()
		{
			var mockParticipant = NewMockTransactionParticipant();
			mockParticipant.Setup(m => m.IsInTransaction).Returns(false);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant.Object, NewMockTransactionParticipantObjectBegunButNeverInTransaction() };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			coordinator.SaveInTransactions();
			mockParticipant.VerifyAll();
		}

		public void TestBeginTransactions_ParticipantBeginThrowsException()
		{
			var mockParticipant1 = NewMockTransactionParticipant();
			mockParticipant1.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant1.Setup(m => m.OnAllTransactionsBeginning());

			var mockManager = new Mock<ITransactionManager>();
			mockManager.Setup(m => m.Dispose());
			mockParticipant1.Setup(m => m.BeginTransactionWithManager()).Returns(mockManager.Object);

			var mockParticipant2 = NewMockTransactionParticipant();
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant2.Setup(m => m.OnAllTransactionsBeginning());
			mockParticipant2.Setup(m => m.BeginTransactionWithManager()).Throws(new CustomTestException());

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant1.Object, mockParticipant2.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			try
			{
				using (var transactionManager = coordinator.BeginTransactionWithManager())
				{ }
				Fail("Should have rethrown exception after rollbacks.");
			}
			catch (CustomTestException)
			{
			}

			Verify(mockParticipant1, mockParticipant2, mockManager);
		}

		public void TestCommitTransactions_ParticipantCommitThrowsException()
		{
			var mockParticipant1 = NewMockTransactionParticipant();
			mockParticipant1.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant1.Setup(m => m.OnAllTransactionsBeginning());

			var mockManager1 = new Mock<ITransactionManager>();
			mockManager1.Setup(m => m.CommitTransaction());
			mockManager1.Setup(m => m.Dispose());
			mockParticipant1.Setup(m => m.BeginTransactionWithManager()).Returns(mockManager1.Object);

			var mockParticipant2 = NewMockTransactionParticipant();
			mockParticipant2.Setup(m => m.IsInTransaction).Returns(false);
			mockParticipant2.Setup(m => m.OnAllTransactionsBeginning());

			var mockManager2 = new Mock<ITransactionManager>();
			mockManager2.Setup(m => m.CommitTransaction()).Throws(new CustomTestException());
			mockManager2.Setup(m => m.Dispose());
			mockParticipant2.Setup(m => m.BeginTransactionWithManager()).Returns(mockManager2.Object);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant1.Object, mockParticipant2.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			using (var transactionManager = coordinator.BeginTransactionWithManager())
			{
				try
				{
					transactionManager.CommitTransaction(SaveInTransactionResult.Empty);
					Fail("Should have rethrown exception after rollbacks.");
				}
				catch (CustomTestException)
				{
				}

				try
				{
					transactionManager.CommitTransaction(SaveInTransactionResult.Empty);
					Fail("Should throw exception if you try to commit again.");
				}
				catch (InvalidOperationException)
				{
				}
				catch (Exception ex)
				{
					Fail("Wrong exception thrown." + ex.ToString());
				}
			}

			Verify(mockParticipant1, mockParticipant2, mockManager1, mockManager2);
		}

		#region TestAllowMultipleParticipantsInTransaction

		public void TestAllowMultipleParticipantsInTransaction()
		{
			Assert("Transaction coordinator should not allow multiple participants in pre-existing transaction.",
				!new TransactionCoordinatorForTest(new[] { new BusinessObjectFactory() }).AllowMultipleParticipantsInTransactionExposed);
		}

		class TransactionCoordinatorForTest : TransactionCoordinator
		{
			public TransactionCoordinatorForTest(ITransactionParticipant[] participants) : base(participants) { }

			public bool AllowMultipleParticipantsInTransactionExposed
			{
				get { return AllowMultipleParticipantsInTransaction; }
			}
		}

		#endregion

		#region Implementation
		ITransactionParticipant NewMockTransactionParticipantObjectBegunButNeverInTransaction()
		{
			var mock = new Mock<ITransactionParticipant>();
			mock.Setup(m => m.IsInTransaction).Returns(false);
			mock.Setup(m => m.AllowTransactionWithOtherParticipant).Returns(false);
			mock.Setup(m => m.BeginTransactionWithManager()).Returns(new StubTransactionManager());
			mock.Setup(m => m.AllowTransactionWithOtherParticipant).Returns(false);
			mock.Object.BeginTransactionWithManager();

			return mock.Object;
		}

		void AddOnRollbackExpectations(params Mock<ITransactionParticipant>[] mocks)
		{
			foreach (var mock in mocks)
			{
				mock.Setup(m => m.OnAllTransactionsRolledBack());
			}
		}

		void AddDisposeExpectations(params Mock<ITransactionManager>[] mocks)
		{
			foreach (var mock in mocks)
			{
				mock.Setup(m => m.Dispose());
			}
		}

		[Serializable]
		class CustomTestException : Exception
		{
			public CustomTestException() : base() { }

#if NETFRAMEWORK
			protected CustomTestException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		#endregion
	}

	#region class TransactionCoordinatorTestCase

	public abstract class TransactionCoordinatorTestCase : TransactionedTestCase
	{
		protected Mock<ITransactionParticipant> NewMockTransactionParticipant()
		{
			var result = new Mock<ITransactionParticipant>();
			result.Setup(m => m.ChildParticipants);
			result.Setup(m => m.AllowTransactionWithOtherParticipant).Returns(false);
			return result;
		}

		protected Mock<ITransactionParticipant> NewMockTransactionParticipantNeverInTransaction()
		{
			var result = new Mock<ITransactionParticipant>();
			result.Setup(m => m.IsInTransaction).Returns(false);
			result.Setup(m => m.AllowTransactionWithOtherParticipant).Returns(false);
			return result;
		}

		protected void SetChildParticipants(Mock<ITransactionParticipant> mockParticipant, Mock<ITransactionParticipant>[] mockChildParticipants)
		{
			ArrayList childParticipants = new ArrayList();

			foreach (var mockChildParticipant in mockChildParticipants)
			{
				childParticipants.Add(mockChildParticipant.Object);
			}

			mockParticipant.Reset();
			mockParticipant.Setup(m => m.ChildParticipants).Returns((ITransactionParticipant[])childParticipants.ToArray(typeof(ITransactionParticipant)));
		}

		protected void Verify(params Mock[] mocks)
		{
			foreach (var mock in mocks)
			{
				AssertNoExceptionThrown(() => mock.VerifyAll());
			}
		}
	}

	#endregion

	#region PrivateAccessTest

	sealed class TransactionCoordinatorPrivateAccessTest : TransactionCoordinatorTestCase
	{
		public void TestParticipantsWithChildParticipants()
		{
			var parentParticipantA = NewMockTransactionParticipantNeverInTransaction();
			var childParticipantA1 = NewMockTransactionParticipantNeverInTransaction();
			var childParticipantA2 = NewMockTransactionParticipantNeverInTransaction();
			var grandChildParticipantA1 = NewMockTransactionParticipantNeverInTransaction();
			var grandChildParticipantA2 = NewMockTransactionParticipantNeverInTransaction();
			var grandChildParticipantA3 = NewMockTransactionParticipantNeverInTransaction();
			var grandChildParticipantA4 = NewMockTransactionParticipantNeverInTransaction();

			var parentParticipantB = NewMockTransactionParticipantNeverInTransaction();
			var childParticipantB1 = NewMockTransactionParticipantNeverInTransaction();
			var childParticipantB2 = NewMockTransactionParticipantNeverInTransaction();
			var grandChildParticipantB1 = NewMockTransactionParticipantNeverInTransaction();
			var grandChildParticipantB2 = NewMockTransactionParticipantNeverInTransaction();
			var grandChildParticipantB3 = NewMockTransactionParticipantNeverInTransaction();
			var grandChildParticipantB4 = NewMockTransactionParticipantNeverInTransaction();
			var greatGrandChildParticipant = NewMockTransactionParticipantNeverInTransaction();

			SetChildParticipants(parentParticipantA, new Mock<ITransactionParticipant>[] { childParticipantA1, childParticipantA2 });
			SetChildParticipants(childParticipantA1, new Mock<ITransactionParticipant>[] { grandChildParticipantA1, grandChildParticipantA2 });
			SetChildParticipants(childParticipantA2, new Mock<ITransactionParticipant>[] { grandChildParticipantA3, grandChildParticipantA4 });

			SetChildParticipants(parentParticipantB, new Mock<ITransactionParticipant>[] { childParticipantB1, childParticipantB2 });
			SetChildParticipants(childParticipantB1, new Mock<ITransactionParticipant>[] { grandChildParticipantB1, grandChildParticipantB2 });
			SetChildParticipants(childParticipantB2, new Mock<ITransactionParticipant>[] { grandChildParticipantB3, grandChildParticipantB4 });
			SetChildParticipants(grandChildParticipantB4, new Mock<ITransactionParticipant>[] { greatGrandChildParticipant });
			greatGrandChildParticipant.Setup(m => m.ChildParticipants).Returns((ITransactionParticipant[])null);

			ITransactionParticipant[] participants = new ITransactionParticipant[] { parentParticipantA.Object, parentParticipantB.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			Verify(parentParticipantA, childParticipantA1, childParticipantA2, grandChildParticipantA1, grandChildParticipantA2, grandChildParticipantA3, grandChildParticipantA4,
				parentParticipantB, childParticipantB1, childParticipantB2, grandChildParticipantB1, grandChildParticipantB2, grandChildParticipantB3, grandChildParticipantB4, greatGrandChildParticipant);

			AssertEquals("participants.Length", 15, coordinator.participants.Length);

			AssertEquals("participants[0]", parentParticipantA.Object, coordinator.participants[0]);
			AssertEquals("participants[1]", childParticipantA1.Object, coordinator.participants[1]);
			AssertEquals("participants[2]", grandChildParticipantA1.Object, coordinator.participants[2]);
			AssertEquals("participants[3]", grandChildParticipantA2.Object, coordinator.participants[3]);
			AssertEquals("participants[4]", childParticipantA2.Object, coordinator.participants[4]);
			AssertEquals("participants[5]", grandChildParticipantA3.Object, coordinator.participants[5]);
			AssertEquals("participants[6]", grandChildParticipantA4.Object, coordinator.participants[6]);

			AssertEquals("participants[7]", parentParticipantB.Object, coordinator.participants[7]);
			AssertEquals("participants[8]", childParticipantB1.Object, coordinator.participants[8]);
			AssertEquals("participants[9]", grandChildParticipantB1.Object, coordinator.participants[9]);
			AssertEquals("participants[10]", grandChildParticipantB2.Object, coordinator.participants[10]);
			AssertEquals("participants[11]", childParticipantB2.Object, coordinator.participants[11]);
			AssertEquals("participants[12]", grandChildParticipantB3.Object, coordinator.participants[12]);
			AssertEquals("participants[13]", grandChildParticipantB4.Object, coordinator.participants[13]);
			AssertEquals("participants[14]", greatGrandChildParticipant.Object, coordinator.participants[14]);
		}

		public void TestParticipantsWithEmptyOrNullChildParticipants()
		{
			var mockParticipant1 = NewMockTransactionParticipantNeverInTransaction();
			var mockParticipant2 = NewMockTransactionParticipantNeverInTransaction();

			mockParticipant1.Setup(m => m.ChildParticipants).Returns((ITransactionParticipant[])null);
			mockParticipant2.Setup(m => m.ChildParticipants).Returns(Array.Empty<ITransactionParticipant>());

			ITransactionParticipant[] participants = new ITransactionParticipant[] { mockParticipant1.Object, mockParticipant2.Object };
			TransactionCoordinator coordinator = new TransactionCoordinator(participants);

			Verify(mockParticipant1, mockParticipant2);

			AssertEquals("participants.Length", 2, coordinator.participants.Length);
			AssertEquals("participants[0]", mockParticipant1.Object, coordinator.participants[0]);
			AssertEquals("participants[1]", mockParticipant2.Object, coordinator.participants[1]);
		}
	}

	#endregion
}
