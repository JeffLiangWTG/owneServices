using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	class AlternateGLAccountFilterStrip : ZFilterStrip
	{
		protected override ZCodeFindBox CreateFindBox(IModuleFilterWithModuleID moduleFilter)
		{
			var alternateGLAccountFilter = moduleFilter as AlternateGLAccountModuleFilter;

			if (alternateGLAccountFilter != null)
			{
				return new AlternateGLAccountZGuidFindBox() { ModuleID = moduleFilter.ModuleId };
			}

			return base.CreateFindBox(moduleFilter);
		}
	}
}
