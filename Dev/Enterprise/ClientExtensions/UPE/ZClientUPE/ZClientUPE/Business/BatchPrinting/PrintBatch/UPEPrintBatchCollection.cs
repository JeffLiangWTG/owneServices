
using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class UPEPrintBatchCollection : BusinessObjectCollection<UPEPrintBatch>
	{
		public UPEPrintBatchCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
