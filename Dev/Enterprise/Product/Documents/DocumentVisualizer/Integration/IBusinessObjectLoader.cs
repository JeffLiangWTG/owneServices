using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IBusinessObjectLoader
	{
		IBusiness LoadBusinessObject(BusinessObjectFactory factoryToLoadInto, IBusiness bizObj);
	}
}
