using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Core.Environment.Registry;

public interface IRegistryChangeLogFilterParameters
{
	ZDBOnlySubQuery GenerateSubQueryFilterFromFilterParameters(ZDBOnlySubQuery subQuery);
}
