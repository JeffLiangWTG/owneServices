using System;
using System.Collections;
using Enterprise.Core.Modules;

namespace Enterprise.ZArchitecture.Modules
{
	public class ZCurrentModules
	{
		protected ZCurrentModules()
		{
		}

		public static ZCurrentModules Instance => instance ?? (instance = new ZCurrentModules());
		[ThreadStatic]
		static ZCurrentModules instance;

		public ZModule GetCurrentModule(ModuleIdentifier moduleID)
		{
			var resultRef = (WeakReference)ModuleIDToModuleHash[moduleID];
			var result = (resultRef == null) ? null : resultRef.Target as ZModule;
			return (result == null || result.IsDisposed) ? null : result;
		}

		public void SetCurrentModule(IZModule module)
		{
			var moduleRef = (WeakReference)ModuleIDToModuleHash[module.ModuleID];
			if (moduleRef == null)
			{
				moduleRef = new WeakReference(null);
				ModuleIDToModuleHash[module.ModuleID] = moduleRef;
			}
			moduleRef.Target = module;
		}

		readonly Hashtable ModuleIDToModuleHash = new Hashtable();
	}
}
