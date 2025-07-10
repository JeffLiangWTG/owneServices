using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Modules
{
	public class ZModuleResults
	{
		#region Instance 

		public static ZModuleResults Instance => instance ?? (instance = new ZModuleResults());

		[ThreadStatic]
		static ZModuleResults instance;

		#endregion

		protected ZModuleResults()
		{
		}

		public ZPKCollection GetPKCollectionForModule(ModuleIdentifier id)
		{
			ZPKCollection result = null;

			var weakRef = (WeakReference)Hash[id];

			if (weakRef != null)
			{
				result = (ZPKCollection)weakRef.Target;
			}

			if (result == null)
			{
				result = new ZPKCollection(new List<ZGuid>());
				Hash[id] = new WeakReference(result);
			}

			return result;
		}

		protected Hashtable Hash => hash ?? (hash = new Hashtable());
		Hashtable hash;
	}
}
