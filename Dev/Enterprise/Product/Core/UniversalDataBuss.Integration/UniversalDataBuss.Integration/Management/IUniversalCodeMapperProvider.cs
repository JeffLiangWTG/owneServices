using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalCodeMapperProvider
	{
		IUniversalCodeMapper Create(ITopLevelDataObject dataObject, BusinessObjectFactory factory);
	}
}
