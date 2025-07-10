using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class TransactionDepartmentCollectionProvider : CollectionProvider
	{
		public TransactionDepartmentCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new GlbDepartmentCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(GlbDepartment)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new GlbDepartmentCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbDepartment;
	}
}
