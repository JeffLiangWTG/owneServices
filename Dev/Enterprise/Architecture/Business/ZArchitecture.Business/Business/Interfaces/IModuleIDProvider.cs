using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public interface IModuleIDProvider
	{
		ModuleIdentifier ModuleID { get; }
	}
}
