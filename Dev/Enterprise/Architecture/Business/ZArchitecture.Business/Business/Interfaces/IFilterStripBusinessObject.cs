using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.ZArchitecture.Business
{
	public delegate void ModuleFiltersCreatedHandler(IFilterStripBusinessObject filterStripBizO);

	public interface IFilterStripBusinessObject
	{
		BusinessObjectFactory Factory { get; }
		ZQuery Filter { get; }
		FilterStripLayoutsHelper LayoutsHelper { get; set; }
		IEnumerable<NonPersistentBusinessObject> ActiveModuleFiltersForValidation { get; }

		void AddAuditFiltersIfRequired();
		bool LoadLayout(StmModuleFilter layout, bool maySkipLayoutIfHasInitialCode = false, bool disableValidation = false);
		ZQuery GetFilterWhere(ZGuid filterPk, Func<IModuleFilter, bool> filtersToIncludeFunc, BusinessObjectFactory factory = null);
		ZQuery GetFilterWhere(StmModuleFilter filter, Func<IModuleFilter, bool> filtersToIncludeFunc);
		StmModuleFilter FindLayout(ZString layoutName, ZBool isPublished);
		int GetFilterStripsCount(StmModuleFilter layout);
		Type QueryObjectType { get; set; }

		Type ModuleType { get; set; }

		string LayoutContext { get; set; }

		IModuleFilterCollection ModuleFilterCollection { get; }

		event ModuleFiltersCreatedHandler ModuleFiltersCreated;

		bool GetShouldUseHelperFilter { get; }
	}
}
