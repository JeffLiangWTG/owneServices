using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	class ZFilterCollectionFindBoxForVariableModuleFilter : ZFilterCollectionFindBox
	{
		public ZFilterCollectionFindBoxForVariableModuleFilter(IModuleFilterWithSelectedFilters moduleFilter, bool isPopupButtonEnabledWhenReadOnly = false, Type parentType = null, ModuleIdentifier parentModuleID = null)
			: base(moduleFilter, isPopupButtonEnabledWhenReadOnly, parentType, parentModuleID)
		{
		}

		protected override void SetModuleId(ModuleIdentifier moduleId)
		{
			base.SetModuleId(moduleId);

			SetPopupButtonReadOnlyForNewlySelectedModule(ModuleFilter);
		}
	}
}
