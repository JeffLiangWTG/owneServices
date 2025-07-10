using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalShipmentDataObjectProvider
	{
		IDataObject GetDataObject(BusinessObject bizObj, IDataWritingInformationCollector informationCollector);
	}
}