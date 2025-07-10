using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ITopLevelDataObjectReaderForTemplateRecord
	{
		void ReadDirectlyIntoBusinessObject(BusinessObject targetBizo);
	}
}
