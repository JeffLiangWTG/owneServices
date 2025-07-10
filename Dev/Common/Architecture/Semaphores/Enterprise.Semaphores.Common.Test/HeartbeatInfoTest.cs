using System;
using NUnit.Framework;

namespace Enterprise.Semaphores.Common.Testing
{
	class HeartbeatInfoTest : TestCase
	{
		public void TestHeartbeatInfo()
		{
			var userPk = Guid.NewGuid();
			IHeartbeatInfo heartbeatInfo = new HeartbeatInfo(Guid.NewGuid(), "somehost", userPk, "Test", "TST", "test@test.test", LogonType.Staff, -1, HeartbeatTypes.Enterprise, "123");
			AssertEquals("somehost", heartbeatInfo.HostName);
			AssertEquals(userPk, heartbeatInfo.UserPk);
			AssertEquals("Test", heartbeatInfo.FullUserName);
			AssertEquals("TST", heartbeatInfo.LogonIdentificationCode);
			AssertEquals("test@test.test", heartbeatInfo.EmailAddress);
			AssertEquals(LogonType.Staff, heartbeatInfo.LogonType);
			AssertEquals(-1, heartbeatInfo.ProcessId);
			AssertEquals(HeartbeatTypes.Enterprise, heartbeatInfo.HeartbeatType);
			AssertEquals("123", heartbeatInfo.ClientIdentifier);
		}
	}
}
