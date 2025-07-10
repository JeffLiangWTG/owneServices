using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IOriginalValueProvider
	{
		IZType GetOriginalValue(ZPropertyInfo info);
	}
}
