using System;
using NUnit.Framework;

namespace Enterprise.Semaphores.Common.Testing
{
	class SemaphoreInfoTest : TestCase
	{
		public void TestSemaphoreInfo()
		{
			Guid userPk = Guid.NewGuid();
			IHeartbeatInfo heartbeatInfo = new HeartbeatInfo(Guid.NewGuid(), "somehost", userPk, "TestUser", "TST", "test@test.test", LogonType.Staff, -1, HeartbeatTypes.Enterprise, "XYZ");
			ISemaphoreType semaphore = new SemaphoreForTesting();
			ISemaphoreInfo semaphoreInfo = new SemaphoreInfo(heartbeatInfo, semaphore, DateTime.UtcNow);
			AssertEquals("HostName", heartbeatInfo.HostName, semaphoreInfo.OwnerSession.HostName);
			AssertEquals("UserPk", heartbeatInfo.UserPk, semaphoreInfo.OwnerSession.UserPk);
			AssertEquals("FullUserName", heartbeatInfo.FullUserName, semaphoreInfo.OwnerSession.FullUserName);
			AssertEquals("LogonIdentificationCode", heartbeatInfo.LogonIdentificationCode, semaphoreInfo.OwnerSession.LogonIdentificationCode);
			AssertEquals("EmailAddress", heartbeatInfo.EmailAddress, semaphoreInfo.OwnerSession.EmailAddress);
			AssertEquals("LogonType", heartbeatInfo.LogonType, semaphoreInfo.OwnerSession.LogonType);
			AssertEquals("ProcessId", heartbeatInfo.ProcessId, semaphoreInfo.OwnerSession.ProcessId);
			AssertEquals("HeartbeatType", heartbeatInfo.HeartbeatType, semaphoreInfo.OwnerSession.HeartbeatType);
			AssertEquals("SessionReference", heartbeatInfo.SessionReference, semaphoreInfo.OwnerSession.SessionReference);
			AssertEquals("LockInfo", semaphore.LockInfo, semaphoreInfo.Semaphore.LockInfo);
			AssertEquals("MaxConcurrentHandles", semaphore.MaxConcurrentHandles, semaphoreInfo.Semaphore.MaxConcurrentHandles);
			AssertEquals("ClientIdentifier", "XYZ", heartbeatInfo.ClientIdentifier);
			AssertNotNull("CreateTimeUtc", semaphoreInfo.CreateTimeUtc);
		}
	}
}
