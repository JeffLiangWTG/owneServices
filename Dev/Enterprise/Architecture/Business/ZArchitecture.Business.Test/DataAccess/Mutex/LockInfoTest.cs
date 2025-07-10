using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Data.Mutex.Testing
{
	sealed class LockInfoTest : TestCase
	{
		public void TestConstructor()
		{
			ZDateTime now = ZDateTime.Now;
			LockInfo info = new LockInfo(now, MutexIDs.NewDummyAttachedToShipment, "1234", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, "HOST", 54321);
			AssertEquals("LockStartTime", now, info.LockStartTime);
			AssertEquals("MutexID", MutexIDs.NewDummyAttachedToShipment, info.MutexID);
			AssertEquals("RecordID", "1234", info.RecordID);
			AssertEquals("Found correct user", StaticCurrentFetcher.Instance.CurrentUser.PK, info.UserWithLock.PK);
			AssertEquals("HostName", "HOST", info.HostName);
			AssertEquals("ProcessId", 54321, info.ProcessId);
		}
	}
}
