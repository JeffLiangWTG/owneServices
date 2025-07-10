using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine
{
	public interface IContactDataRestrictionProvider
	{
		ZQuery GetFilter(ModuleIdentifier moduleIdentifier, ZGuid orgContactPK, BusinessObjectFactory factory);
	}
}
