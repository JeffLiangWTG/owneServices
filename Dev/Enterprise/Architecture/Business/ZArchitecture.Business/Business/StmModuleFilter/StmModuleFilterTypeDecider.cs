using System;
using System.Collections;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class StmModuleFilterTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(StmModuleFilter);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var module = (string)row[StmModuleFilter.Schema.S9_ModuleID];
			var types = ObjectFactory.Get<Hashtable>("StmModuleFilterTypes");
			foreach (string key in types.Keys)
			{
				if (module.EndsWith(key))
				{
					return (Type)types[key];
				}
			}
			return typeof(StmModuleFilter);
		}

		public override Type GetTypeForNew()
		{
			return typeof(StmModuleFilter);
		}
	}
}
