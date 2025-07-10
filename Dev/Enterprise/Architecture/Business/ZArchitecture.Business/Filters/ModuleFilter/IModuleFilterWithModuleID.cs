using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public interface IModuleFilterWithModuleID
	{
		ModuleIdentifier ModuleId { get; }
	}
}
