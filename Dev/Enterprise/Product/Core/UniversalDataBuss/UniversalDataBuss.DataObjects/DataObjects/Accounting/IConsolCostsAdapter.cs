using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	public interface IConsolCostsAdapter
	{
		ConsolCosts Generate(IGenericJobCostPlugInBase consolCostParent, IDataObjectWriterStrategy writerStrategy);
		void ImportConsolCosts(BusinessObjectFactory factory, IXmlImportLogger logger, IConsolCostsData consolCostsData, ZGuid jobHeaderParentPK, ZString jobHeaderParentTablePrefix);
	}
}
