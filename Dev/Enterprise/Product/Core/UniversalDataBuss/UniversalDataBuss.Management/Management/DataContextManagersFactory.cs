using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalDataBuss.Management
{
	static class DataContextManagersFactory
	{
		internal static IEnumerable<IDataContextManager> All
		{
			get
			{
				var types = (Hashtable)ObjectFactory.Get("UniversalDataContextManagers");
				foreach (string key in types.Keys)
				{
					if (key != "DummyBusinessObject" || IncludeDummyObjectForTesting)
					{
						var objectHandle = (ObjectHandle)types[key];
						yield return (IDataContextManager)objectHandle.GetObject();
					}
				}
			}
		}

		static bool IncludeDummyObjectForTesting
		{
#if DEBUG
			get { return Globals.IsTest; }
#else
			get { return false; }
#endif
		}
	}
}
