using System.Collections.Generic;
using Enterprise.Core.Environment;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IModuleListingSubset
	{
		IEnumerable<ModuleIdentifier> ModuleIdentifiers { get; }
		IEnumerable<ModuleInfo> ModuleInfos { get; }
		IEnumerable<ControllerInfo> ControllerInfos { get; }

		void InitializeSecurityCheckpoints(IZSecurity security);
		void InitializeModuleTree(ModuleTreeCategories moduleTreeCategories, IZSecurity security);
	}
}
