using System;
using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZClientWebCargoWiseEDI.Base;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(MyAccountHeartBeatManager))]
	[HttpContextEnabledTest]
	public class MyAccountHeartBeatManagerTest : TestCaseWithFactory
	{
		Guid GetCurrentHeartbeatID(MyAccountHeartBeatManager manager) => (Guid)typeof(MyAccountHeartBeatManager).GetProperty("CurrentHeartbeatID", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic).GetValue(manager);

		readonly SemaphoreDbManager dbManager = new SemaphoreDbManager();

		public void TestRegisterOrUpdateUserContext_New()
		{
			var userPK = Guid.NewGuid();
			var manager = new MyAccountHeartBeatManager(HttpContext.Current.Session);
			AssertEquals(Guid.Empty, GetCurrentHeartbeatID(manager));

			manager.RegisterOrUpdateUserContext(userPK);

			var handles = dbManager.GetActiveSemaphoreHandles(GetCurrentHeartbeatID(manager));
			AssertEquals(1, handles.Length);
			AssertEquals("LGN", handles[0].Semaphore.Category);
			AssertEquals("MYAUserLogin", handles[0].Semaphore.LockInfo);
			AssertEquals("MYA", handles[0].OwnerSession.HeartbeatType);
			AssertEquals(userPK, handles[0].OwnerSession.UserPk);
			AssertEquals(HttpContext.Current.Session.SessionID, handles[0].OwnerSession.HostName);
		}

		public void TestRegisterOrUpdateUserContext_Update()
		{
			var userPK = Guid.NewGuid();
			var manager = new MyAccountHeartBeatManager(HttpContext.Current.Session);
			AssertEquals(Guid.Empty, GetCurrentHeartbeatID(manager));

			manager.RegisterOrUpdateUserContext(userPK);
			var heartBeatID = GetCurrentHeartbeatID(manager);
			var handles = dbManager.GetActiveSemaphoreHandles(heartBeatID);
			AssertEquals(1, handles.Length);
			AssertEquals(userPK, handles[0].OwnerSession.UserPk);

			var newUserPK = Guid.NewGuid();
			manager.RegisterOrUpdateUserContext(newUserPK);
			AssertEquals("Heartbeat ID should not be updated", heartBeatID, GetCurrentHeartbeatID(manager));

			handles = dbManager.GetActiveSemaphoreHandles(GetCurrentHeartbeatID(manager));
			AssertEquals(1, handles.Length);
			AssertEquals("LGN", handles[0].Semaphore.Category);
			AssertEquals("MYAUserLogin", handles[0].Semaphore.LockInfo);
			AssertEquals("MYA", handles[0].OwnerSession.HeartbeatType);
			AssertEquals(newUserPK, handles[0].OwnerSession.UserPk);
			AssertEquals(HttpContext.Current.Session.SessionID, handles[0].OwnerSession.HostName);
		}

		public void TestIsCurrentContextValid()
		{
			var manager = new MyAccountHeartBeatManager(HttpContext.Current.Session);
			AssertEquals(Guid.Empty, GetCurrentHeartbeatID(manager));
			Assert(!manager.IsCurrentContextValid);

			manager.RegisterOrUpdateUserContext(Guid.NewGuid());
			Assert(manager.IsCurrentContextValid);

			var currentHeartbeatID = GetCurrentHeartbeatID(manager);
			manager.DisposeCurrentLoginContext();
			Assert(!manager.IsCurrentContextValid);

			var handles = dbManager.GetActiveSemaphoreHandles(currentHeartbeatID);
			AssertEquals(0, handles.Length);
		}
	}
}
