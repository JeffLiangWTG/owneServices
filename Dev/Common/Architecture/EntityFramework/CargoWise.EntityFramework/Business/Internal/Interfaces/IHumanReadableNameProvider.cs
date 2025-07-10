using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IHumanReadableNameProvider : IService
	{
		ZString GetHumanReadableName(ZPropertyInfo propertyInfo);
	}
}
