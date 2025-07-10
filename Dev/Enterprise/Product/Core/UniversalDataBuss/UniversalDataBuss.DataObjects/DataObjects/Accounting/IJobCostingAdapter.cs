using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	public interface IJobCostingAdapter
	{
		JobCosting Generate(IJobHeaderParentCore jobHeaderParent, IDataObjectWriterStrategy writerStrategy);
		void ImportCharges(BusinessObjectFactory factory, IXmlImportLogger logger, IJobCostingData jobCostingData, ZGuid jobHeaderParentPK, ZString jobHeaderParentTablePrefix);
	}
}
