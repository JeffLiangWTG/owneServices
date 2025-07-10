using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public class DepositBatchCollectionProvider : CollectionProvider, Integration.IDepositBatchCollectionProvider
	{
		public DepositBatchCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new DepositBatchModuleCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.DepositBatch;
	}
}