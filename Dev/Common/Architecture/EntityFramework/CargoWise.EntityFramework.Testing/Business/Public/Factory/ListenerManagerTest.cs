using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data.Utils;
using CargoWise.Integration;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ListenerManagerTest : TestCaseWithFactory
	{
		public void TestNotifyFactorySaveBeginning_WorksWhileModifyingCollection()
		{
			var listenManager = new ListenerManager();

			var listener1 = new DummyListener(listenManager);
			var listener2 = new DummyListener(listenManager);

			listenManager.Register(listener1);
			listenManager.Register(listener2);

			var factories = new DummyFactory[] { new DummyFactory() };

			AssertNoExceptionThrown(() => { listenManager.NotifyFactorySaveBeginning(factories); });
		}

		public void TestNotifyFactorySaveCompleted_WorksWhileModifyingCollection()
		{
			var listenManager = new ListenerManager();

			var listener1 = new DummyListener(listenManager);
			var listener2 = new DummyListener(listenManager);

			listenManager.Register(listener1);
			listenManager.Register(listener2);

			var factories = new DummyFactory[] { new DummyFactory() };

			AssertNoExceptionThrown(() => { listenManager.NotifyFactorySaveCompleted(factories, false); });
		}

		public void TestThreadSafety()
		{
			var listenerManager = new ListenerManager();
			var threads = new List<Thread>();
			var exceptions = new ConcurrentBag<Exception>();

			for (int i = 0; i < 20; ++i)
			{
				var listener = new DummyListener(listenerManager);
				threads.Add(new Thread(() =>
				{
					try
					{
						var factories = new DummyFactory[] { new DummyFactory() };
						for (int j = 0; j < 100; ++j)
						{
							if (exceptions.Any())
							{
								break;
							}

							listenerManager.Register(listener);
							listenerManager.NotifyFactorySaveCompleted(factories, false);
							listenerManager.UnRegister(listener);
						}
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
					}
				}));
			}

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			AssertEquals(true, exceptions.IsEmpty);
		}

		#region Implimentation

		class DummyListener : ITransactionParticipantListener
		{
			public DummyListener(ListenerManager manager)
			{
				this.manager = manager;
			}

			DummyListener() { }

			ListenerManager manager { get; set; }

			public void FactorySaveBeginning(ITransactionParticipant[] factories)
			{
				var newListener = new DummyListener(manager);
				manager.Register(newListener);
			}

			public void FactorySaveCompleted(ITransactionParticipant[] factories, bool successful)
			{
				var newListener = new DummyListener(manager);
				manager.Register(newListener);
			}
		}

		class DummyFactory : ITransactionParticipant
		{
			public bool IsInTransaction => throw new NotImplementedException();

			public bool AllowTransactionWithOtherParticipant => throw new NotImplementedException();

			public ITransactionParticipant[] ChildParticipants => throw new NotImplementedException();

			public IEnumerable<ISqlApplicationLock> TransactionLocks => throw new NotImplementedException();

			public ITransactionManager BeginTransactionWithManager()
			{
				throw new NotImplementedException();
			}

			public void OnAllTransactionsBeginning()
			{
				throw new NotImplementedException();
			}

			public void OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
			{
				throw new NotImplementedException();
			}

			public void OnAllTransactionsRolledBack()
			{
				throw new NotImplementedException();
			}

			public IChangedTableNames SaveInTransaction()
			{
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}
