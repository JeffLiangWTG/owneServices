using System;
using System.Collections.Generic;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Semaphores.Common.Testing
{
	class SemaphoreHandleTest : TransactionedTestCase
	{
		const int AttemptCountToHandleDeadLocks = 3;

		public void TestLoadHandle()
		{
			IHeartbeatInfoFactory heartbeatFactory = new MockHeartbeatInfoFactoryForTesting();

			using (HeartbeatWithNoTimerForTesting testHeartbeat = new HeartbeatWithNoTimerForTesting(heartbeatFactory, TimeSpan.FromHours(1)))
			{
				testHeartbeat.Register_Exposed();
				IHeartbeat heartbeat = testHeartbeat;
				ISemaphoreType semaphore = new SemaphoreForTesting();

				AssertEquals("PRE-CONDITION: No created TestSemaphore handles", 0, GetSemaphoreHandlesCount(semaphore));
				Assert("Nothing to load yet", !(SemaphoreHandle.Load(heartbeat.HeartbeatId, semaphore, AttemptCountToHandleDeadLocks) as ISemaphoreHandle).Success);

				DateTime timeBeforeCreating = DateTime.UtcNow.AddMinutes(-1);
				using (ISemaphoreHandle createdSemaphoreHandle = SemaphoreHandle.New(heartbeat.HeartbeatId, semaphore, AttemptCountToHandleDeadLocks))
				{
					DateTime timeAfterCreating = DateTime.UtcNow.AddMinutes(1);
					AssertNotNull("TestSemaphore handle object should be instantiated ", createdSemaphoreHandle);
					AssertEquals("TestSemaphore handle was created in DB", 1, GetSemaphoreHandlesCount(semaphore));

					using (ISemaphoreHandle semaphoreHandle = SemaphoreHandle.Load(heartbeat.HeartbeatId, semaphore, AttemptCountToHandleDeadLocks))
					{
						//Assert properties of created semaphore handle
						Assert("Semaphore acquired with success?", semaphoreHandle.Success);
						AssertNull("No thrown exceptions", semaphoreHandle.CreateException);

						AssertEquals("LockInfo", semaphore.LockInfo, semaphoreHandle.Semaphore.LockInfo);
						AssertEquals("MaxConcurrentHandles", semaphore.MaxConcurrentHandles, semaphoreHandle.Semaphore.MaxConcurrentHandles);
						AssertEquals("Category", semaphore.Category, semaphoreHandle.Semaphore.Category);

						ISemaphoreProvider testProvider = new SemaphoreProviderForTesting();
						ISemaphoreInfo[] activeHandles = testProvider.GetActiveSemaphoreHandles(semaphore);
						AssertEquals("Number of active handles", 1, activeHandles.Length);
						AssertEquals("HostName", heartbeat.Info.HostName, activeHandles[0].OwnerSession.HostName);
						AssertEquals("UserPk", heartbeat.Info.UserPk, activeHandles[0].OwnerSession.UserPk);
						AssertEquals("FullUserName", MockHeartbeatInfoFactoryForTesting.MockFullUserName, activeHandles[0].OwnerSession.FullUserName);
						AssertEquals("ProcessId", heartbeat.Info.ProcessId, activeHandles[0].OwnerSession.ProcessId);
						AssertEquals("SessionReference", heartbeat.Info.SessionReference, activeHandles[0].OwnerSession.SessionReference);
						Assert("CreateTimeUtc", (activeHandles[0].CreateTimeUtc > timeBeforeCreating) && (activeHandles[0].CreateTimeUtc < timeAfterCreating));
					}
				}

				AssertEquals("TestSemaphore handle was deleted by unique id", 0, GetSemaphoreHandlesCount(semaphore));
			}
		}

		public void TestCreateNotActiveSemaphoreHandle()
		{
			MockHeartbeatInfoFactoryForTesting infoFactory = new MockHeartbeatInfoFactoryForTesting();
			IHeartbeat heartbeat = new HeartbeatWithNoTimerForTesting(infoFactory, TimeSpan.FromMinutes(0));
			ISemaphoreType semaphore = new SemaphoreForTesting();

			using (ISemaphoreHandle semaphoreHandle = SemaphoreHandle.New(heartbeat.HeartbeatId, semaphore, AttemptCountToHandleDeadLocks))
			{
				AssertNotNull("TestSemaphore handle object should be instantiated ", semaphoreHandle);
				AssertEquals("TestSemaphore handle should NOT be created in DB", 0, GetSemaphoreHandlesCount(semaphore));

				//Assert properties of created semaphore handle
				AssertEquals("Semaphore acquired with success?", false, semaphoreHandle.Success);
				AssertNotNull("An exception should be throw", semaphoreHandle.CreateException);

				AssertEquals("LockInfo", semaphore.LockInfo, semaphoreHandle.Semaphore.LockInfo);
				AssertEquals("MaxConcurrentHandles", semaphore.MaxConcurrentHandles, semaphoreHandle.Semaphore.MaxConcurrentHandles);
				AssertEquals("Category", semaphore.Category, semaphoreHandle.Semaphore.Category);
			}
		}

		public void TestCreateActiveSemaphoreHandle()
		{
			IHeartbeatInfoFactory heartbeatFactory = new MockHeartbeatInfoFactoryForTesting();

			using (HeartbeatWithNoTimerForTesting testHeartbeat = new HeartbeatWithNoTimerForTesting(heartbeatFactory, TimeSpan.FromHours(1)))
			{
				testHeartbeat.Register_Exposed();
				IHeartbeat heartbeat = testHeartbeat;
				ISemaphoreType semaphore = new SemaphoreForTesting();

				AssertEquals("PRE-CONDITION: No created TestSemaphore handles", 0, GetSemaphoreHandlesCount(semaphore));

				DateTime timeBeforeCreating = DateTime.UtcNow.AddMinutes(-1);
				using (ISemaphoreHandle semaphoreHandle = SemaphoreHandle.New(heartbeat.HeartbeatId, semaphore, AttemptCountToHandleDeadLocks))
				{
					DateTime timeAfterCreating = DateTime.UtcNow.AddMinutes(1);
					AssertNotNull("TestSemaphore handle object should be instantiated ", semaphoreHandle);
					AssertEquals("TestSemaphore handle was created in DB", 1, GetSemaphoreHandlesCount(semaphore));

					//Assert properties of created semaphore handle
					Assert("Semaphore acquired with success?", semaphoreHandle.Success);
					AssertNull("No thrown exceptions", semaphoreHandle.CreateException);

					AssertEquals("LockInfo", semaphore.LockInfo, semaphoreHandle.Semaphore.LockInfo);
					AssertEquals("MaxConcurrentHandles", semaphore.MaxConcurrentHandles, semaphoreHandle.Semaphore.MaxConcurrentHandles);
					AssertEquals("Category", semaphore.Category, semaphoreHandle.Semaphore.Category);

					ISemaphoreProvider testProvider = new SemaphoreProviderForTesting();
					ISemaphoreInfo[] activeHandles = testProvider.GetActiveSemaphoreHandles(semaphore);
					AssertEquals("Number of active handles", 1, activeHandles.Length);
					AssertEquals("HostName", heartbeat.Info.HostName, activeHandles[0].OwnerSession.HostName);
					AssertEquals("UserPk", heartbeat.Info.UserPk, activeHandles[0].OwnerSession.UserPk);
					AssertEquals("FullUserName", MockHeartbeatInfoFactoryForTesting.MockFullUserName, activeHandles[0].OwnerSession.FullUserName);
					AssertEquals("ProcessId", heartbeat.Info.ProcessId, activeHandles[0].OwnerSession.ProcessId);
					AssertEquals("SessionReference", heartbeat.Info.SessionReference, activeHandles[0].OwnerSession.SessionReference);
					Assert("CreateTimeUtc", (activeHandles[0].CreateTimeUtc > timeBeforeCreating) && (activeHandles[0].CreateTimeUtc < timeAfterCreating));
				}

				AssertEquals("TestSemaphore handle was deleted by unique id", 0, GetSemaphoreHandlesCount(semaphore));
			}
		}

		public void TestSemaphoreHandleException()
		{
			ISemaphoreType testSemaphore = new SemaphoreForTesting();
			List<ISemaphoreProvider> providers = new List<ISemaphoreProvider>();
			for (int i = 0; i < 4; i++)
			{
				providers.Add(new SemaphoreProviderForTesting());
			}

			using (ISemaphoreHandle handle1 = providers[0].CreateSemaphoreHandle(testSemaphore))
			using (ISemaphoreHandle handle2 = providers[1].CreateSemaphoreHandle(testSemaphore))
			using (ISemaphoreHandle handle3 = providers[2].CreateSemaphoreHandle(testSemaphore))
			using (ISemaphoreHandle handle4 = providers[3].CreateSemaphoreHandle(testSemaphore))
			{
				Assert("handle1", handle1.Success);
				Assert("handle2", handle2.Success);
				Assert("handle3", handle3.Success);
				Assert("handle4", !handle4.Success);
				Assert("SqlException should be thrown", handle4.CreateException.InnerException is SqlException);
				Assert(
					"Exception should be wrapped as a SemaphoreReachedMaxAllowedHandlesException but was " +
					handle4.CreateException.GetType().FullName,
					handle4.CreateException is SemaphoreReachedMaxAllowedHandlesException);
			}
		}

		public void TestGetFormattedSemaphoreInvalidSessionIdException()
		{
			MockHeartbeatInfoFactoryForTesting infoFactory = new MockHeartbeatInfoFactoryForTesting();
			IHeartbeat heartbeat = new HeartbeatWithNoTimerForTesting(infoFactory, TimeSpan.FromMinutes(0));
			ISemaphoreType semaphore = new SemaphoreForTesting();
			Guid otherSessionId = Guid.NewGuid();

			using (ISemaphoreHandle semaphoreHandle = SemaphoreHandle.New(otherSessionId, semaphore, AttemptCountToHandleDeadLocks))
			{
				AssertNotNull("TestSemaphore handle object should be instantiated ", semaphoreHandle);
				AssertEquals("TestSemaphore handle should NOT be created in DB", 0, GetSemaphoreHandlesCount(semaphore));

				//Assert properties of created semaphore handle
				AssertEquals("Semaphore acquired with success?", false, semaphoreHandle.Success);
				AssertNotNull("An exception should be throw", semaphoreHandle.CreateException);

				string expectedMessage =
					"Cannot create semaphore handle with an invalid session ID.\r\n" +
					"Expected Session ID: [" + otherSessionId.ToString() + "]";
				AssertEquals("Exception message", expectedMessage, semaphoreHandle.CreateException.Message);
			}
		}

		int GetSemaphoreHandlesCount(ISemaphoreType semaphore)
		{
			string sqlText = string.Format(@"
				SELECT count(*)
				FROM dbo.StmServiceSemaphore WITH(READCOMMITTED)
				WHERE SS_LockInfo = '{0}'",
				semaphore.LockInfo);
			int count = (int)TestConnection.ExecuteScalar(sqlText);
			return count;
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			InsertUser();
		}

		void InsertUser()
		{
			string sqlText = string.Format("INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_EmailAddress, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{0}', '{1}', '{2}', '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');",
				MockHeartbeatInfoFactoryForTesting.MockUserPk,
				MockHeartbeatInfoFactoryForTesting.MockUserId,
				MockHeartbeatInfoFactoryForTesting.MockFullUserName,
				MockHeartbeatInfoFactoryForTesting.MockUserEmail
				);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		#endregion

		protected override DbConnection TestConnection
		{
			get { return Db.Connection; }
		}
	}

	public class SemaphoreForTesting : ISemaphoreType
	{
		public SemaphoreForTesting()
			: this("TestSemaphore")
		{ }

		public SemaphoreForTesting(string lockInfo)
		{
			this.LockInfo = lockInfo;
		}

		#region ISemaphoreType Members

		public int MaxConcurrentHandles
		{
			get { return 3; }
		}

		public string Category
		{
			get { return "TST"; }
		}

		public string LockInfo { get; private set; }

		#endregion
	}
}
