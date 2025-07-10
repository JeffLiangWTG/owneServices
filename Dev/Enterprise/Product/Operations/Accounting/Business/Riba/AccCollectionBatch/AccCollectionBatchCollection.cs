using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Riba
{
	[ModuleID(ModuleId.AccCollectionBatch)]
	public class AccCollectionBatchCollection : ActiveBusinessObjectCollection<AccCollectionBatch>
	{
		public AccCollectionBatchCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public AccCollectionBatchCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

