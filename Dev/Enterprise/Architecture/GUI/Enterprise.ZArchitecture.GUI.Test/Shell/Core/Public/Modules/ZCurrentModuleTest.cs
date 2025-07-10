using System;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	class ZCurrentModulesTest : TestCaseWithFactory
	{
		public void TestGetCurrentModule()
		{
			using (var module = new DummyFilterGridModule())
			{
				ZCurrentModules.Instance.SetCurrentModule(module);
				Assert("The same module instance should be returned from the cache", module == ZCurrentModules.Instance.GetCurrentModule(module.ID));
			}
		}

		public void TestGetCurrentModule_ReturnsNullWhenModuleDisposed()
		{
			using (var module = new DummyFilterGridModule())
			{
				ZCurrentModules.Instance.SetCurrentModule(module);
				Assert("The same module instance should be returned from the cache", module == ZCurrentModules.Instance.GetCurrentModule(module.ID));

				module.Dispose();
				AssertEquals("No current module should be returned because the module is disposed.", null, ZCurrentModules.Instance.GetCurrentModule(module.ID));
			}
		}

		public void TestGetCurrentModule_ModuleWeakReferenced()
		{
			var moduleRef = GetWeakReferenceToModule();
			GC.Collect();
			AssertEquals("The module should be able to be collected at any time to prevent memory leak", false, moduleRef.IsAlive);
		}

		WeakReference GetWeakReferenceToModule()
		{
			var module = new DummyFilterGridModule();
			DisposableLeakListener.Instance.UnRegisterDisposable(module);

			var moduleRef = new WeakReference(module);
			ZCurrentModules.Instance.SetCurrentModule(module);
			Assert("The same module instance should be returned from the cache", module == ZCurrentModules.Instance.GetCurrentModule(module.ID));
			module = null;

			return moduleRef;
		}
	}
}
