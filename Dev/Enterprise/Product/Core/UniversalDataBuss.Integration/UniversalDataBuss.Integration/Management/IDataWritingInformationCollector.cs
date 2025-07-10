using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDataWritingInformationCollector
	{
		void NotifyExported(IDataObject dataObject, BusinessObject businessObject);
	}
}