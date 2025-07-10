using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Semaphores.Common.Testing
{
	public class SemaphoreProviderTest : TransactionedTestCase
	{
		public void TestSemaphoreProvider()
		{
			ISemaphoreType testSemaphore = new SemaphoreForTesting();

			ISemaphoreProvider provider1 = new SemaphoreProviderForTesting();
			ISemaphoreProvider provider2 = new SemaphoreProviderForTesting();
			ISemaphoreProvider provider3 = new SemaphoreProviderForTesting();
			ISemaphoreProvider provider4 = new SemaphoreProviderForTesting();
			ISemaphoreProvider providerWithInactiveHeartbeat = new SemaphoreProviderForTestingBase(TimeSpan.FromMinutes(-1));

			AssertEquals("PRE-CONDITION: No created heartbeats", 0, GetHeartbeatCount(MockHeartbeatInfoFactoryForTesting.MockUserPk));
			AssertEquals("Number of Handles", 0, provider1.GetActiveSemaphoreHandles(testSemaphore).Length);

			using (ISemaphoreHandle handle1 = provider1.CreateSemaphoreHandle(testSemaphore))
			{
				Assert("handle1", handle1.Success);

				using (ISemaphoreHandle handle2 = provider2.CreateSemaphoreHandle(testSemaphore))
				{
					Assert("handle2", handle2.Success);
					AssertEquals("Number of Handles", 2, provider2.GetActiveSemaphoreHandles(testSemaphore).Length);

					using (ISemaphoreHandle handle3 = providerWithInactiveHeartbeat.CreateSemaphoreHandle(testSemaphore))
					{
						Assert("handle3", handle3.Success);
						AssertEquals("Number of Handles - Not active handle was created", 2, providerWithInactiveHeartbeat.GetActiveSemaphoreHandles(testSemaphore).Length);
					}

					using (ISemaphoreHandle handle4 = provider3.CreateSemaphoreHandle(testSemaphore))
					{
						Assert("handle4", handle4.Success);

						AssertEquals("Provider1 - active handles", 3, provider1.GetActiveSemaphoreHandles(testSemaphore).Length);
						AssertEquals("Provider2 - active handles", 3, provider2.GetActiveSemaphoreHandles(testSemaphore).Length);
						AssertEquals("Provider3 - active handles", 3, provider3.GetActiveSemaphoreHandles(testSemaphore).Length);
						AssertEquals("Provider4 - active handles", 3, provider4.GetActiveSemaphoreHandles(testSemaphore).Length);

						using (ISemaphoreHandle handle5 = provider4.CreateSemaphoreHandle(testSemaphore))
						{
							Assert("handle5", !handle5.Success);
							Assert("Exception does not match:\r\n" + handle5.CreateException.Message, handle5.CreateException is SemaphoreReachedMaxAllowedHandlesException);
							AssertEquals("Number of Handles", 3, provider4.GetActiveSemaphoreHandles(testSemaphore).Length);

							AssertEquals("Heartbeats were created in DB", 4, GetHeartbeatCount(MockHeartbeatInfoFactoryForTesting.MockUserPk));
						}
					}
				}
			}

			AssertEquals("Heartbeats were disposed by semaphore handle providers", 0, GetHeartbeatCount(MockHeartbeatInfoFactoryForTesting.MockUserPk));
		}

		public void TestGetActiveSemaphoreHandles()
		{
			ISemaphoreType testSemaphore1a = new SemaphoreForTesting("TestSemaphore1");
			ISemaphoreType testSemaphore1b = new SemaphoreForTesting("TestSemaphore1");
			ISemaphoreType testSemaphore2 = new SemaphoreForTesting("TestSemaphore2");

			ISemaphoreProvider provider1 = new SemaphoreProviderForTesting();
			ISemaphoreProvider provider2 = new SemaphoreProviderForTesting();
			ISemaphoreProvider provider3 = new SemaphoreProviderForTesting();

			using (ISemaphoreHandle handle1 = provider1.CreateSemaphoreHandle(testSemaphore1a))
			using (ISemaphoreHandle handle2 = provider2.CreateSemaphoreHandle(testSemaphore1a))
			using (ISemaphoreHandle handle3 = provider1.CreateSemaphoreHandle(testSemaphore1b))
			using (ISemaphoreHandle handle4 = provider2.CreateSemaphoreHandle(testSemaphore1b))
			using (ISemaphoreHandle handle5 = provider3.CreateSemaphoreHandle(testSemaphore2))
			{
				Assert("handle1.Success", handle1.Success);
				Assert("handle2.Success", handle2.Success);
				Assert("handle3.Success", handle3.Success);
				Assert("!handle4.Success", !handle4.Success);
				AssertEquals("Handle not created, semaphore has reached max quantity of concurrent handles.", handle4.CreateException.Message);
				Assert("handle5.Success", handle5.Success);
				AssertEquals("Active handles with: LockInfo = TestSemaphore1", 2, provider1.GetActiveSemaphoreHandles(testSemaphore1a).Length);
				AssertEquals("Active handles with: LockInfo = TestSemaphore1", 2, provider1.GetActiveSemaphoreHandles(testSemaphore1b).Length);
				AssertEquals("Active handles with: LockInfo = TestSemaphore2", 1, provider1.GetActiveSemaphoreHandles(testSemaphore2).Length);
			}
		}

		public void TestGetActiveSemaphoreHandlesWithSameProvider()
		{
			ISemaphoreType testSemaphore1 = new SemaphoreForTesting("TestSemaphore1");
			ISemaphoreType testSemaphore2 = new SemaphoreForTesting("TestSemaphore2");

			ISemaphoreProvider provider = new SemaphoreProviderForTesting();

			using (ISemaphoreHandle handle1 = provider.CreateSemaphoreHandle(testSemaphore1))
			{
				Assert("handle1.Success", handle1.Success);
				AssertEquals("Active handles with: LockInfo = TestSemaphore1", 1, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
				AssertEquals("Active handles with: LockInfo = TestSemaphore2", 0, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);

				using (ISemaphoreHandle handle2 = provider.CreateSemaphoreHandle(testSemaphore1))
				{
					Assert("handle2.Success", handle2.Success);
					AssertEquals("Active handles with: LockInfo = TestSemaphore1", 1, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
					AssertEquals("Active handles with: LockInfo = TestSemaphore2", 0, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);

					using (ISemaphoreHandle handle3 = provider.CreateSemaphoreHandle(testSemaphore1))
					{
						Assert("handle3.Success", handle3.Success);
						AssertEquals("Active handles with: LockInfo = TestSemaphore1", 1, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
						AssertEquals("Active handles with: LockInfo = TestSemaphore2", 0, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);

						using (ISemaphoreHandle handle4 = provider.CreateSemaphoreHandle(testSemaphore2))
						{
							Assert("handle4.Success", handle4.Success);
							AssertEquals("Active handles with: LockInfo = TestSemaphore1", 1, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
							AssertEquals("Active handles with: LockInfo = TestSemaphore2", 1, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);
						}

						AssertEquals("Active handles with: LockInfo = TestSemaphore1", 1, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
						AssertEquals("Active handles with: LockInfo = TestSemaphore2", 0, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);
					}

					AssertEquals("Active handles with: LockInfo = TestSemaphore1", 1, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
					AssertEquals("Active handles with: LockInfo = TestSemaphore2", 0, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);
				}

				AssertEquals("Active handles with: LockInfo = TestSemaphore1", 1, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
				AssertEquals("Active handles with: LockInfo = TestSemaphore2", 0, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);

				using (ISemaphoreHandle handle5 = provider.CreateSemaphoreHandle(testSemaphore1))
				{
					Assert("handle5.Success", handle5.Success);
					AssertEquals("Active handles with: LockInfo = TestSemaphore1", 1, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
					AssertEquals("Active handles with: LockInfo = TestSemaphore2", 0, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);
				}

				AssertEquals("Active handles with: LockInfo = TestSemaphore1", 1, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
				AssertEquals("Active handles with: LockInfo = TestSemaphore2", 0, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);
			}

			AssertEquals("Active handles with: LockInfo = TestSemaphore1", 0, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
			AssertEquals("Active handles with: LockInfo = TestSemaphore2", 0, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);
		}

		public void TestRemoveSemaphore()
		{
			using var testProvider = new SemaphoreProviderForTesting();
			ISemaphoreProvider provider = testProvider;
			ISemaphoreType testSemaphore1 = new SemaphoreForTesting("TestSemaphore1");
			ISemaphoreType testSemaphore2 = new SemaphoreForTesting("TestSemaphore2");

			using var handle1 = provider.CreateSemaphoreHandle(testSemaphore1);
			using var handle2 = provider.CreateSemaphoreHandle(testSemaphore2);

			AssertEquals("Pre-requisite: Active handles with: LockInfo = TestSemaphore1", 1, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
			AssertEquals("Pre-requisite: Active handles with: LockInfo = TestSemaphore2", 1, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);

			provider.RemoveSemaphore(testSemaphore1);

			AssertEquals("After remove TestSemaphore1: Active handles with: LockInfo = TestSemaphore1", 0, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
			AssertEquals("After remove TestSemaphore1: Pre-requisite: Active handles with: LockInfo = TestSemaphore2", 1, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);

			provider.RemoveSemaphore(testSemaphore2);

			AssertEquals("After remove TestSemaphore2: Active handles with: LockInfo = TestSemaphore1", 0, provider.GetActiveSemaphoreHandles(testSemaphore1).Length);
			AssertEquals("After remove TestSemaphore2: Pre-requisite: Active handles with: LockInfo = TestSemaphore2", 0, provider.GetActiveSemaphoreHandles(testSemaphore2).Length);

			AssertNoExceptionThrown(() => provider.RemoveSemaphore(testSemaphore1));
		}

		public void TestDisposeHeartbeatSkippedIfNotYetCreated()
		{
			SemaphoreProviderForTesting provider = new SemaphoreProviderForTesting();

			int refDbHitCount = TestConnection.ExecutedCommandCountForAllConnections;
			provider.DisposeInternalHeartbeat_Exposed();
			int dbHitCountAfterDisposeCalled = TestConnection.ExecutedCommandCountForAllConnections;
			AssertEquals("DB Hit Count when nothing to dispose", 0, dbHitCountAfterDisposeCalled - refDbHitCount);
			AssertEquals("Created Heartbeat count", 0, GetHeartbeatCount(MockHeartbeatInfoFactoryForTesting.MockUserPk));

			// Call CreateSemaphoreHandle just to force heartbeat lazy instantiation
			using (((ISemaphoreProvider)provider).CreateSemaphoreHandle(new SemaphoreForTesting()))
			{
				AssertEquals("Heartbeat count after instantiation", 1, GetHeartbeatCount(MockHeartbeatInfoFactoryForTesting.MockUserPk));
				refDbHitCount = TestConnection.ExecutedCommandCountForAllConnections;
			}

			// 2 DB hits (1 disposing semaphore + 1 disposing heartbeat)
			dbHitCountAfterDisposeCalled = TestConnection.ExecutedCommandCountForAllConnections;
			AssertEquals("DB Hit Count when heartbeat is there to be disposed", 2, dbHitCountAfterDisposeCalled - refDbHitCount);
			AssertEquals("Heartbeat count after disposal", 0, GetHeartbeatCount(MockHeartbeatInfoFactoryForTesting.MockUserPk));
		}

		int GetHeartbeatCount(Guid userPk)
		{
			string sqlText = string.Format(@"
				SELECT count(*) 
				FROM dbo.StmServiceHeartBeat WITH (READCOMMITTED)
				WHERE SV_ParentId = '{0}'",
				userPk);
			int count = (int)TestConnection.ExecuteScalar(sqlText);
			return count;
		}

		protected override DbConnection TestConnection
		{
			get { return Db.Connection; }
		}

		#region Statics

		public void TestCreateSemaphoreHandle()
		{
			var semaphoreProvider = new SemaphoreProviderForTesting();
			var iSemaphoreProvider = (ISemaphoreProvider)semaphoreProvider;
			var semaphoreType = new SemaphoreForTesting();

			AssertEquals("Precondition - no created heartbeats.", 0, GetHeartbeatCount(MockHeartbeatInfoFactoryForTesting.MockUserPk));
			AssertEquals("Precondition - no semaphore handles.", 0, iSemaphoreProvider.GetActiveSemaphoreHandles(semaphoreType).Length);
			using (var handle = SemaphoreProviderForTesting.CreateSemaphoreHandle(semaphoreProvider, semaphoreType))
			{
				var activeSemaphoreHandles = iSemaphoreProvider.GetActiveSemaphoreHandles(semaphoreType);
				AssertEquals(true, handle.Success);
				AssertEquals("Only 1 heartbeat should be created.", 1, GetHeartbeatCount(MockHeartbeatInfoFactoryForTesting.MockUserPk));
				AssertEquals("Only 1 semaphore handle should be created.", 1, activeSemaphoreHandles.Length);
				AssertEquals("Should create a heartbeat for specified semaphore provider.", semaphoreProvider.InternalHeartbeat_Exposed.HeartbeatId, activeSemaphoreHandles[0].OwnerSession.HeartbeatId);
				AssertEquals("Should create a semaphore handles for specified semaphore type.", semaphoreType.GetType(), activeSemaphoreHandles[0].Semaphore.GetType());
			}
		}

		public void TestRemoteLogoff()
		{
			var semaphoreProvider = new SemaphoreProviderForTesting();
			var semaphoreType = new SemaphoreForTesting();
			using (var handle1 = SemaphoreProviderForTesting.CreateSemaphoreHandle(semaphoreProvider, semaphoreType))
			{
				var activeSemaphoreHandles1 = ((ISemaphoreProvider)semaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
				AssertEquals("Precondition - should have 1 semaphore handles.", 1, activeSemaphoreHandles1.Length);

				// Remote log out current user and current computer. Should do nothing.
				SemaphoreProviderForTesting.RemoteLogoff(MockHeartbeatInfoFactoryForTesting.MockUserPk, MockHeartbeatInfoFactoryForTesting.MockHostName, MockHeartbeatInfoFactoryForTesting.MockHeartbeatType, string.Empty);
				var activeSemaphoreHandles2 = ((ISemaphoreProvider)semaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
				AssertEquals(1, activeSemaphoreHandles2.Length);

				// Remote log out another user and current computer. Should do nothing.
				SemaphoreProviderForTesting.RemoteLogoff(Guid.NewGuid(), MockHeartbeatInfoFactoryForTesting.MockHostName, MockHeartbeatInfoFactoryForTesting.MockHeartbeatType, string.Empty);
				var activeSemaphoreHandles3 = ((ISemaphoreProvider)semaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
				AssertEquals(1, activeSemaphoreHandles3.Length);

				// Remote log out current user, another computer and another heartbeat type. Shoud do nothing.
				SemaphoreProviderForTesting.RemoteLogoff(MockHeartbeatInfoFactoryForTesting.MockUserPk, "ANOTHER_PC", "TST", string.Empty);
				var activeSemaphoreHandles4 = ((ISemaphoreProvider)semaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
				AssertEquals(1, activeSemaphoreHandles4.Length);

				// Remote log out current user, another computer and current heartbeat type. Shoud logoff same user from PC that do not match entered PC and uses same heatbeat type.
				SemaphoreProviderForTesting.RemoteLogoff(MockHeartbeatInfoFactoryForTesting.MockUserPk, "ANOTHER_PC", MockHeartbeatInfoFactoryForTesting.MockHeartbeatType, string.Empty);
				var activeSemaphoreHandles5 = ((ISemaphoreProvider)semaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
				AssertEquals("Remote Logoff should expire all semaphores for the specific user that doesn't match entered PC name.", 0, activeSemaphoreHandles5.Length);
			}
		}

		public void TestReleaseLocks()
		{
			var semaphoreProvider = new SemaphoreProviderForTesting();
			var semaphoreType = new SemaphoreForTesting();
			using (var handle1 = SemaphoreProvider.CreateSemaphoreHandle(semaphoreProvider, semaphoreType))
			{
				var activeSemaphoreHandles1 = ((ISemaphoreProvider)semaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
				AssertEquals("Precondition - should have 1 semaphore handles.", 1, activeSemaphoreHandles1.Length);

				SemaphoreProvider.ReleaseLocks("TestSemaphore", MockHeartbeatInfoFactoryForTesting.MockHeartbeatType, MockHeartbeatInfoFactoryForTesting.MockUserPk);
				var activeSemaphoreHandles2 = ((ISemaphoreProvider)semaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
				AssertEquals(0, activeSemaphoreHandles2.Length);
			}
		}

		public void TestCancelOtherSessionDoesNotStartExpiryCounterForSessionsOnAnotherWorkstation()
		{
			// expired row
			Db.Connection.ExecuteNonQuery(@"insert into dbo.StmServiceHeartBeat(SV_PK, SV_ExpiresAtUtc, SV_WorkstationName, SV_ProcessID, SV_HeartbeatType, SV_ParentID, SV_PArentTableCode, SV_CreateTimeUTC)
values ('9FA73133-9830-43EE-B579-DE80D21A4069', '2016-12-20', 'Hello', 123, 'ENT', '217369CF-A3B8-42D1-BC6F-AA4A6437D939', 'GS', DateAdd(DAY, -1, GetUTCDate()))");
			SemaphoreProviderForTesting.RemoteLogoff(new Guid("217369CF-A3B8-42D1-BC6F-AA4A6437D939"), "ANOTHERBOX", "ENT", string.Empty);
			AssertEquals(1, Db.Connection.ExecuteScalar<int>("select count(*) from dbo.StmServiceHeartbeat where SV_ExpiresAtUtc = '2016-12-20'"));
		}

		#endregion
	}

	#region Testing Classes

	public class SemaphoreProviderForTestingBase : SemaphoreProvider
	{
		internal SemaphoreProviderForTestingBase(TimeSpan heartbeatDuration, int? attemptsToHandleDeadlocks = null)
			: base(attemptsToHandleDeadlocks)
		{
			if (!NUnit.Framework.TestingState.IsRunningTests)
			{
				throw new InvalidOperationException("This class can be only used in tests.");
			}

			this.heartbeatDuration = heartbeatDuration;
		}

		internal SemaphoreProviderForTestingBase()
			: this(TimeSpan.FromMinutes(30))
		{
		}

		public sealed override IHeartbeat InternalHeartbeat
		{
			get
			{
				if (internalHeartbeat == null)
				{
					HeartbeatWithNoTimerForTesting testHeartbeat = new HeartbeatWithNoTimerForTesting(HeartbeatSessionInfoFactory, HeartbeatDuration);
					testHeartbeat.Register_Exposed();
					internalHeartbeat = testHeartbeat;
				}

				return internalHeartbeat;
			}
		}

		protected sealed override TimeSpan HeartbeatDuration
		{
			get { return heartbeatDuration; }
		}

		readonly TimeSpan heartbeatDuration;

		protected override IHeartbeatInfoFactory HeartbeatSessionInfoFactory
		{
			get { return mockHeartbeatInfoFactory; }
		}

		readonly IHeartbeatInfoFactory mockHeartbeatInfoFactory = new MockHeartbeatInfoFactoryForTesting();

		protected override IHeartBeatRemoteLogoff LogoffHandler
		{
			get { throw new NotImplementedException(); }
		}
	}

	public class SemaphoreProviderForTesting : SemaphoreProviderForTestingBase, IDisposable
	{
		public SemaphoreProviderForTesting(int? attemptsToHandleDeadlocks = null)
			: base(TimeSpan.FromMinutes(30), attemptsToHandleDeadlocks)
		{
		}

		internal void DisposeInternalHeartbeat_Exposed()
		{
			DisposeInternalHeartbeat();
		}

		internal IHeartbeat InternalHeartbeat_Exposed
		{
			get { return InternalHeartbeat; }
		}

		public void DisposeAllSemaphores()
		{
			foreach (WeakReference semaphoreReference in activeSemaphores)
			{
				IDisposable disposableSemaphore = semaphoreReference.Target as IDisposable;

				if (disposableSemaphore != null)
				{
					UnregisterDisposableSemaphore(disposableSemaphore);
				}
			}
		}

		void IDisposable.Dispose()
		{
			DisposeAllSemaphores();
		}
	}

	#endregion
}
