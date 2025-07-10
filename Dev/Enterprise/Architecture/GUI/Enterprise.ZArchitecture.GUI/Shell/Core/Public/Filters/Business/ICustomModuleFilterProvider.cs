using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public interface ICustomModuleFilterProvider
	{
		ModuleFilter GetModuleFilter(SchemaColumn columnSchema, string headerText);
	}
}