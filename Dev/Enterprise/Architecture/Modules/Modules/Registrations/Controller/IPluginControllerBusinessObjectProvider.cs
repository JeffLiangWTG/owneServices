using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IPluginControllerBusinessObjectProvider
	{
		IBusiness LoadBusinessEntityForPlugIn(BusinessObjectFactory factory, ZGuid sourceEntityPK);
	}
}
