using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration.Management
{
	public interface IUniversalExceptionWriter
	{
		void PopulateExceptions(BusinessObject source, IDataObject destination);
	}
}
