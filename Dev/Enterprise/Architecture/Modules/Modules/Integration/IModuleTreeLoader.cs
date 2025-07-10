using Enterprise.Core.Environment;
using Enterprise.Core.Modules;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IModuleTreeLoader
	{
		void Initialise(ModuleTree treeToLoad, IZSecurity securityInstance);
		void Initialise(ModuleTree treeToLoad, IZSecurity securityInstance, ClientHook clientHook);
		void LoadModules();
	}
}
