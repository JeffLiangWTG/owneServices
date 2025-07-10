using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public interface IBusinessObjectLoader
	{
		BusinessObject Load(BusinessObjectFactory factory, ZGuid pk);
	}
}
