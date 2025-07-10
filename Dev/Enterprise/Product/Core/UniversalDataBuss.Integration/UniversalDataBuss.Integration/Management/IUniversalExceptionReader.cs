using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalExceptionReader
	{
		public void PopulateExceptions(IDataObject dataObject, BusinessObject parent, IXmlImportLogger logger, IUniversalObjectFactory factory);
	}
}
