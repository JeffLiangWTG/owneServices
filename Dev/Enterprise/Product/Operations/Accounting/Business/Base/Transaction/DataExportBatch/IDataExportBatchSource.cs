using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.DataExportBatch
{
	public interface IDataExportBatchSource : IBusiness
	{
		ZGuid PK { get; }
		ZBool IsDataExportBatchSupported { get; }
		DataExportBatchDependentCollection DataExportBatchCollection { get; }
	}
}
