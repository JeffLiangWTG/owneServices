using System;
using CargoWise.EntityFramework;

namespace Enterprise.Integration.ZArchitecture
{
	public interface IFilterModuleStrategy
	{
		void RunOnModuleFiltersCreated(IModuleFilterCollection filters, Type bizObjType, BusinessObjectFactory factory);
	}
}
