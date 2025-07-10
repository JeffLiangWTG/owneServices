using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	sealed class ZMutexedPlugInTest : TestCaseWithDummy
	{
		public void TestMutexLock()
		{
			using (var plugIn1 = new TestPlugIn(Dummy))
			{
				plugIn1.OnGUIShown();
				AssertEquals("Mutex is locked by this instance", true, plugIn1.Mutex.HasLock);

				using (var plugIn2 = new TestPlugIn(Dummy))
				{
					plugIn2.OnGUIShown();
					AssertEquals("Mutex is locked", true, plugIn2.Mutex.IsLocked);
					AssertEquals("Mutex is locked by other instance", false, plugIn2.Mutex.HasLock);
				}
			}
		}

		public void TestMutexIsUnlockedWhenSaved()
		{
			using (var plugIn1 = new TestPlugIn(Dummy))
			{
				plugIn1.OnGUIShown();
				AssertEquals("PreCondition: Mutex is locked", true, plugIn1.Mutex.HasLock);

				Factory.Save();
				AssertEquals("Mutex is unlocked", false, plugIn1.Mutex.IsLocked);
			}
		}

		public void TestMutexIsUnlockedWhenDisposed()
		{
			TestPlugIn plugIn1;
			using (plugIn1 = new TestPlugIn(Dummy))
			{
				plugIn1.OnGUIShown();
				AssertEquals("PreCondition: Mutex is locked", true, plugIn1.Mutex.HasLock);
			}
			AssertEquals("Mutex is unlocked", false, plugIn1.Mutex.IsLocked);
		}

		internal class TestPlugIn : ZMutexedPlugIn
		{
			public TestPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
			{
			}

			protected override LicenceCheckpoint LicenceCheckPoint
			{
				get { return (LicenceCheckpoint)EnvProxy.Instance.Licence.AlwaysAllow; }
			}

			public override string Name
			{
				get { return "TestMutexPlugIn"; }
			}

			protected ZGlobalMutex fMutex;
			public override ZGlobalMutex Mutex
			{
				get
				{
					if (fMutex == null)
					{
						fMutex = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment, ((BusinessObject)HostBusinessEntity).PK.ToString());
					}
					return fMutex;
				}
			}

			public override void OnGUIShown()
			{
				base.OnGUIShown();
				if (!Mutex.IsLocked)
				{
					Mutex.Lock();
				}
			}

			protected internal override ZBool HasUserControl
			{
				get { return false; }
			}
		}
	}
}
