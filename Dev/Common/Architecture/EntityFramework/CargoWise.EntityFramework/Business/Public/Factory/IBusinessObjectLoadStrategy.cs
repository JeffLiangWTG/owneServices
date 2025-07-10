using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectLoadStrategy
	{
		BusinessObject Load(BusinessObjectFactory factory, ZGuid pk);
	}
}
